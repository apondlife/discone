using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityAtoms.BaseAtoms;
using ThirdPerson;

/// an online player
[RequireComponent(typeof(Player))]
sealed class OnlinePlayer: NetworkBehaviour {
    // -- references --
    [Header("references")]
    [Tooltip("a reference to the player character")]
    [SerializeField] GameObjectVariable m_Player;

    // -- lifecycle --
    void Awake() {
        if (isLocalPlayer) {
            
        }
    }

    // -- NetworkBehaviour --
    public override void OnStartClient() {
        base.OnStartClient();

        // disable camera for other players
        if (!isLocalPlayer) {
        }
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();

        // transfer control from offline to online player
        Transfer();
    }

    // -- commands --
    /// spawn a new player
    public static GameObject Spawn(GameObject prefab, int id, Vector3 pos) {
        var obj = GameObject.Instantiate(prefab);
        obj.name = $"Player-{id}";
        obj.transform.position = pos;
        return obj;
    }

    /// transfer control to online player and destroy offline version
    public void Transfer() {
        // give the local player a special name
        gameObject.name = "Player (local)";

        // replace the offline character with the online one (see: Online.OnCreateCharacter)
        var online = GetComponent<Player>();
        var offline = m_Player.GetComponent<Player>();

        var character = offline.CurrentCharacter;
        online.Drive(character);
        Destroy(offline.gameObject);

        m_Player.Value = gameObject;
    }
}
