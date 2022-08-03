using Mirror;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityEngine.Serialization;
using System;

/// the online "manager"
public class Online: NetworkManager {
    // -- types --
    /// the current
    public enum State {
        Host,
        Connecting,
        Client,
        Disconnected
    }

    // -- fields --
    [Header("config")]
    [Tooltip("should the host restart on client disconnect")]
    [SerializeField] bool m_RestartHostOnDisconnect;

    // -- state --
    [Header("state")]
    [Tooltip("the host address to connect to")]
    [SerializeField] StringReference m_HostAddress;

    [Tooltip("if the player is the host")]
    [SerializeField] BoolVariable m_IsHost;

    // -- inputs --
    [Header("inputs")]
    [Tooltip("an event when the client starts")]
    [SerializeField] VoidEvent m_StartClientEvent;

    [Tooltip("an event when the client disconnects")]
    [SerializeField] VoidEvent m_DisconnectEvent;

    // -- outputs --
    [Header("outputs")]
    [Tooltip("an event for logging errors")]
    [SerializeField] StringEvent m_ErrorEvent;

    // -- deps --
    [Header("deps")]
    [Tooltip("a reference to the player character")]
    [FormerlySerializedAs("m_PlayerCharacter")]
    [SerializeField] GameObjectVariable m_Player;

    // -- props --
    /// the set of event subscriptions
    Subscriptions subscriptions = new Subscriptions();

    /// the current state
    public State m_State = State.Host;

    // -- lifecycle --
    public override void Awake() {
        base.Awake();

        // bind atom events
        subscriptions
            .Add(m_StartClientEvent, OnTryStartClient)
            .Add(m_DisconnectEvent, OnTryDisconnect);
    }

    public override void Start() {
        base.Start();

        // set the initial address
        var addr = m_HostAddress?.Value;
        if (addr != null && addr != "") {
            networkAddress = addr;
        }

        // start a host for every player, immediately
        // TODO: is this a good idea? for now at least
        try {
            SwitchToHost();
        } catch (System.Net.Sockets.SocketException err) {
            var code = err.ErrorCode;
            if (code == 10048 && (addr == "localhost" || addr == "127.0.0.1")) {
                SwitchToClient();
            }
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();

        // unbind events
        subscriptions.Dispose();
    }

    // -- commands --
    /// start game as host
    void SwitchToHost() {
        m_State = State.Host;
        m_IsHost.Value = true;

        StartHost();
    }

    /// start game as client
    void SwitchToClient() {
        m_State = State.Connecting;
        m_IsHost.Value = false;

        StartClient();
    }

    // -- l/mirror
    public override void OnClientError(Exception exception) {
        base.OnClientError(exception);

        m_ErrorEvent?.Raise($"[online] client exception: {exception.Message}");

        Debug.Log($"[online] client error {exception}");
    }

    public override void OnClientConnect() {
        base.OnClientConnect();

        if (m_State != State.Host) {
            m_State = State.Client;
            Debug.Log($"[online] client connected!");
        }
    }

    public override void OnClientNotReady() {
        base.OnClientNotReady();

        Debug.Log($"[online] client not ready...");
    }

    public override void OnClientDisconnect() {
        base.OnClientDisconnect();

        Debug.Log($"[online] client disconnected...");
        if (m_State == State.Host) {
            return;
        }

        // raise a timeout error if we fail to connect
        if (m_State == State.Connecting) {
            m_ErrorEvent?.Raise($"[online] failed to connect to server {networkAddress}");
        }

        // and then restart the host
        if (m_RestartHostOnDisconnect) {
            this.DoNextFrame(() => SwitchToHost());
        }
    }

    // -- l/mirror/server
    public override void OnServerDisconnect(NetworkConnection c) {
        base.OnServerDisconnect(c);
    }

    public override void OnServerConnect(NetworkConnection c) {
        base.OnServerConnect(c);
    }

    public override void OnServerError(NetworkConnection c, Exception e) {
        base.OnServerError(c, e);
    }

    public override void OnStartHost() {
        base.OnStartHost();
    }

    // -- queries --
    /// if we're acting as the host
    bool IsHost {
        get => NetworkServer.active;
    }

    // -- events --
    /// when the host starts
    void OnTryStartClient() {
        // ignore repeat presses
        // TODO: what if the player changed the ip
        if (m_State == State.Connecting) {
            return;
        }

        // stop the host, if active
        if (IsHost) {
            StopHost();
        }

        this.DoNextFrame(() => {
            // start the client
            networkAddress = m_HostAddress.Value;
            SwitchToClient();
            Debug.Log($"[online] connecting client: {networkAddress}");
        });
    }

    /// when the host/client disconnect
    void OnTryDisconnect() {
        // flag the network as disconnected
        m_State = State.Disconnected;

        // stop host or client, which should start up a new host
        if (IsHost) {
            StopHost();
        } else {
            StopClient();
        }
    }
}