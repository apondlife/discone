using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;

public class Online: NetworkManager {
    // -- types --
    /// the message sent to the server when creating a character
    struct CreateCharacter: NetworkMessage {
        public Vector3 Position;
    }

    // -- fields --
    [Header("dependencies")]
    [Tooltip("a reference to the player character")]
    [SerializeField] GameObjectVariable m_PlayerCharacter;

    // -- NetworkManager --
    public override void OnStartServer() {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreateCharacter>(OnCreateCharacter);
    }

    public override void OnClientConnect() {
        base.OnClientConnect();

        // you can send the message here, or wherever else you want
        NetworkClient.Send(new CreateCharacter() {
            Position = m_PlayerCharacter.Value.transform.position
        });
    }

    // -- events --
    /// when a new character spawns on the server
    void OnCreateCharacter(NetworkConnection conn, CreateCharacter msg) {
        // instantiate the online player character
        // NOTE:the offline character is destroyed in OnlineCharacter.OnStartLocalPlayer
        GameObject obj = Instantiate(playerPrefab);
        obj.name = $"OnlinePlayer-{conn.connectionId}";
        obj.transform.position = msg.Position;

        // add them to the game
        NetworkServer.AddPlayerForConnection(conn, obj);
    }
}
