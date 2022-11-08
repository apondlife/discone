using Mirror;
using UnityEngine;
using UnityAtoms.BaseAtoms;
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

    // -- state --
    [Header("state")]
    [Tooltip("the host address to connect to")]
    [SerializeField] StringReference m_HostAddress;

    [Tooltip("if the player is the host")]
    [SerializeField] BoolVariable m_IsHost;

    // -- fields --
    [Header("config")]
    [Tooltip("should the host restart on client disconnect")]
    [SerializeField] bool m_RestartHostOnDisconnect;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("an event when the client starts")]
    [SerializeField] VoidEvent m_StartClientEvent;

    [Tooltip("an event when the client disconnects")]
    [SerializeField] VoidEvent m_DisconnectEvent;

    // -- published --
    [Header("published")]
    [Tooltip("an event for logging errors")]
    [SerializeField] StringEvent m_ErrorEvent;

    // -- refs --
    [Header("refs")]
    [Tooltip("if this is a standalone server (no host client)")]
    [SerializeField] BoolReference m_IsStandalone;

    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

    // -- props --
    /// the set of event subscriptions
    Subscriptions subscriptions = new Subscriptions();

    /// the current state
    State m_State = State.Host;

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

        if (IsStandalone) {
            StartAsServer();
        } else {
            StartAsHost();
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();

        // unbind events
        subscriptions.Dispose();
    }

    // -- l/client
    [Client]
    public override void OnClientError(Exception exception) {
        base.OnClientError(exception);

        Debug.Log($"[online] client error: {exception}");
        m_ErrorEvent?.Raise($"[online] client error: {exception.Message}");
    }

    [Client]
    public override void OnClientConnect() {
        base.OnClientConnect();

        // finish connection flow
        if (m_State == State.Connecting) {
            m_State = State.Client;
            Debug.Log($"[online] connected as client");
        }
        else if (m_State == State.Host) {
            Debug.Log($"[online] connected as host client");
        }

        // create player
        var message = new CreatePlayerMessage();
        NetworkClient.Send(message);
    }

    [Client]
    public override void OnClientNotReady() {
        base.OnClientNotReady();

        Debug.Log($"[online] client not ready...");
    }

    [Client]
    public override void OnClientDisconnect() {
        base.OnClientDisconnect();

        Debug.Log($"[online] client disconnected...");

        // if we are a host, we don't do anything
        if (IsHost) {
            Debug.Log($"[online] host client disconnected");
            return;
        }

        // if we're still attempting to connect, we timed out
        if (m_State == State.Connecting) {
            m_ErrorEvent?.Raise($"[online] failed to connect to server {networkAddress}");
        }
        // if we are non host client disconnect from a server, sync our player record
        else {
            m_Store.SyncPlayer();
        }

        // and then restart as a host
        if (m_RestartHostOnDisconnect) {
            this.DoNextFrame(() => SwitchToHost());
        }
    }

    // -- l/server
    [Server]
    public override void OnStartClient() {
        base.OnStartClient();
        Debug.Log("[online] started client");
    }

    [Server]
    public override void OnStartServer() {
        base.OnStartServer();

        Debug.Log("[online] started server");

        NetworkServer.RegisterHandler<CreatePlayerMessage>(Server_OnCreatePlayer);
    }

    [Server]
    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);

        Debug.Log($"[online] connect [id={conn.connectionId} addr={conn.address}]");
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection conn) {
        Debug.Log($"[online] disconnect [id={conn.connectionId} addr={conn.address}]");

        // give player a chance to clean up before being destroyed
        var player = conn.identity.gameObject.GetComponent<OnlinePlayer>();
        if (player == null) {
            Debug.LogError($"[online] diconnected player is not an OnlinePlayer!");
        } else {
            player.Server_OnDisconnect();
        }

        // destroy the player
        base.OnServerDisconnect(conn);
    }

    // -- commands --
    /// start the game as a standalone server
    void StartAsServer() {
        SwitchToHost();
    }

    /// start the game as a host (server & client)
    void StartAsHost() {
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

    /// start game as host
    void SwitchToHost() {
        m_State = State.Host;
        m_IsHost.Value = true;

        if (IsStandalone) {
            Debug.Log("[online] starting standalone server");
            StartServer();
        } else {
            Debug.Log("[online] starting host");
            StartHost();
        }
    }

    /// start game as client
    void SwitchToClient() {
        Debug.Log("[online] switching to client");

        m_State = State.Connecting;
        m_IsHost.Value = false;

        StartClient();
    }

    // -- queries --
    /// if we're acting as the host
    bool IsHost {
        get => NetworkServer.active;
    }

    /// if the game is running as a standalone server
    bool IsStandalone {
        get {
            #if UNITY_SERVER
            return true;
            #elif !UNITY_EDITOR
            return false;
            #else
            return m_IsStandalone;
            #endif
        }
    }

    // -- events --
    [Server]
    void Server_OnCreatePlayer(
        NetworkConnection conn,
        CreatePlayerMessage _
    ) {
        var t = GetStartPosition();
        var player = Instantiate(playerPrefab, t.position, t.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    /// when the player presses "connect"
    void OnTryStartClient() {
        // ignore repeat presses
        // TODO: what if the player changed the ip
        if (m_State == State.Connecting) {
            return;
        }

        // if host, sync & save world to disk
        if (IsHost) {
            var _ = m_Store.Save();
        }
        // if client, just sync player
        else {
            m_Store.SyncPlayer();
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
        // can't stop hosting...
        if (IsHost) {
            return;
        }

        // flag the network as disconnected
        m_State = State.Disconnected;

        // stop client, which should start up a new host
        StopClient();
    }
}

public struct CreatePlayerMessage: NetworkMessage {
}