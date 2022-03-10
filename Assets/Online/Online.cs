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
    [SerializeField] VoidEvent m_StartHostEvent;

    [Tooltip("an event when the starts")]
    [SerializeField] VoidEvent m_StartClientEvent;

    [Tooltip("an event when the starts")]
    [SerializeField] VoidEvent m_DisconnectEvent;

    [Header("output events")]
    [Tooltip("an event when the client successfully connects")]
    [SerializeField] VoidEvent m_OnStartClient;

    [Tooltip("an event when the client failts to connect")]
    [SerializeField] VoidEvent m_OnStopClient;

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
        m_DisconnectEvent.Register(DidDisconnect);
    }

    public override void Start() {
        base.Start();

        // we are starting host for every player immediately
        // TODO: is this a good idea? yes, for now.
        StartHost();
    }

    // -- NetworkManager --
    public override void OnStartServer() {
        base.OnStartServer();

        // NetworkServer.RegisterHandler<CreateCharacter>(DidCreateCharacter);
    }

    public override void OnClientConnect() {
        base.OnClientConnect();

        // // you can send the message here, or wherever else you want
        // NetworkClient.Send(new CreateCharacter() {
        //     Position = m_Player.Value.transform.position
        // });
    }

    // -- events --
    /// when a new character spawns on the server
    void DidCreateCharacter(NetworkConnection conn, CreateCharacter msg) {
        // // add the online player to the game
        // var obj = OnlinePlayer.Spawn(playerPrefab, conn.connectionId, msg.Position);
        // NetworkServer.AddPlayerForConnection(conn, obj);
    }

    /// when the host starts
    void DidStartHost() {
        StartHost();
        Debug.Log($"[online] started host");
    }

    /// when the host/client disconnect
    void DidDisconnect() {
        if (NetworkServer.active) {
            StopHost();
        } else {
            StopClient();
        }
    }

    /// when the host starts
    void DidStartClient() {
        if (NetworkServer.active) {
            StopHost();
        }

        networkAddress = m_HostAddress.Value;
        StartClient();
        Debug.Log($"[online] started client: {networkAddress}");
    }

    public override void OnStopClient() {
        m_OnStopClient?.Raise();
    }

    public override void OnStartClient() {
        m_OnStartClient?.Raise();
    }
}