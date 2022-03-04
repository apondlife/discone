using Mirror;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityEngine.Serialization;

public class Online: NetworkManager {
    // -- types --
    /// the message sent to the server when creating a character
    struct CreateCharacter: NetworkMessage {
        public Vector3 Position;
    }

    // -- fields --
    [Header("state")]
    [Tooltip("the host address to connect to")]
    [SerializeField] StringReference m_HostAddress;

    [Header("events")]
    [Tooltip("an event when the starts")]
    [SerializeField] VoidEvent m_StartHostEvent;

    [Tooltip("an event when the starts")]
    [SerializeField] VoidEvent m_StartClientEvent;

    [Header("deps")]
    [Tooltip("a reference to the player character")]
    [FormerlySerializedAs("m_PlayerCharacter")]
    [SerializeField] GameObjectVariable m_Player;

    // -- lifecycle --
    public override void Awake() {
        base.Awake();

        // bind events
        m_StartHostEvent.Register(DidStartHost);
        m_StartClientEvent.Register(DidStartClient);
    }

    // -- NetworkManager --
    public override void OnStartServer() {
        base.OnStartServer();

        NetworkServer.RegisterHandler<CreateCharacter>(DidCreateCharacter);
    }

    public override void OnClientConnect() {
        base.OnClientConnect();

        // you can send the message here, or wherever else you want
        NetworkClient.Send(new CreateCharacter() {
            Position = m_Player.Value.transform.position
        });
    }

    // -- events --
    /// when a new character spawns on the server
    void DidCreateCharacter(NetworkConnection conn, CreateCharacter msg) {
        // add the online player to the game
        var obj = OnlinePlayer.Spawn(playerPrefab, conn.connectionId, msg.Position);
        NetworkServer.AddPlayerForConnection(conn, obj);
    }

    /// when the host starts
    void DidStartHost() {
        StartHost();
    }

    /// when the host starts
    void DidStartClient() {
        networkAddress = m_HostAddress.Value;
        StartClient();
    }
}