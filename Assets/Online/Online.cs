using Mirror;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityEngine.Serialization;

/// the online "manager"
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

    [Header("input events")]
    [Tooltip("an event when the starts")]
    [SerializeField] VoidEvent m_StartClientEvent;

    [Tooltip("an event when the starts")]
    [SerializeField] VoidEvent m_DisconnectEvent;

    [Header("deps")]
    [Tooltip("a reference to the player character")]
    [FormerlySerializedAs("m_PlayerCharacter")]
    [SerializeField] GameObjectVariable m_Player;

    // -- lifecycle --
    public override void Awake() {
        base.Awake();

        // bind events
        m_StartClientEvent.Register(DidStartClient);
        m_DisconnectEvent.Register(DidDisconnect);
    }

    public override void Start() {
        base.Start();

        // we are starting host for every player immediately
        // TODO: is this a good idea? yes, for now.
        StartHost();
    }

    // -- events --
    /// when the host starts
    void DidStartClient() {
        if (NetworkServer.active) {
            StopHost();
        }

        networkAddress = m_HostAddress.Value;
        StartClient();
        Debug.Log($"[online] started client: {networkAddress}");
    }

    /// when the host/client disconnect
    void DidDisconnect() {
        if (NetworkServer.active) {
            StopHost();
        } else {
            StopClient();
        }
    }
}