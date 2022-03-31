using Mirror;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityEngine.Serialization;
using System;

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

    private Subscriptions subscriptions = new Subscriptions();

    // -- lifecycle --
    public override void Awake() {
        base.Awake();

        // bind atom events
        subscriptions
            .Add(m_StartClientEvent, DidStartClient)
            .Add(m_DisconnectEvent, DidDisconnect);
    }

    public override void Start() {
        base.Start();

        // we are starting host for every player immediately
        // TODO: is this a good idea? yes, for now.
        StartHost();
    }

    public override void OnDestroy() {
        base.OnDestroy();

        // unbind events
        subscriptions.Dispose();
    }

    // -- l/mirror
    public override void OnClientError(Exception exception) {
        base.OnClientError(exception);

        if (!NetworkServer.active) {
            Debug.Log($"[online] client error {exception}");
        }
    }

    public override void OnClientConnect() {
        base.OnClientConnect();

        if (!NetworkServer.active) {
            Debug.Log($"[online] client connected!");
        }
    }

    public override void OnClientDisconnect() {
        base.OnClientDisconnect();

        Debug.Log($"[online] client disconnected");
        StartHost();
    }

    // -- queries --
    /// if we're acting as the host
    bool IsHost {
        get => NetworkServer.active;
    }

    // -- events --
    /// when the host starts
    void DidStartClient() {
        // stop the host
        if (IsHost) {
            StopHost();
        }

        // start the client
        networkAddress = m_HostAddress.Value;
        StartClient();

        Debug.Log($"[online] starting client: {networkAddress}");
    }

    /// when the host/client disconnect
    void DidDisconnect() {
        if (IsHost) {
            StopHost();
        } else {
            StopClient();
        }
    }
}