using Mirror;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using System;

/// the online "manager"
public class Online: NetworkManager {
    // -- types --
    /// the connection state
    public enum State {
        Server,
        Connecting,
        Client,
        Disconnected
    }

    // -- state --
    [Header("state")]
    [Tooltip("the server address to connect to")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_HostAddress")]
    [SerializeField] StringReference m_ServerAddres;

    [Tooltip("if this is running as server")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_IsHost")]
    [SerializeField] BoolVariable m_IsServer;

    // -- fields --
    [Header("config")]
    [Tooltip("if the client should try connecting to the server on start")]
    [SerializeField] bool m_ConnectToServerOnStart;

    [Tooltip("if the client should restart as host disconnect")]
    [SerializeField] bool m_RestartHostOnDisconnect;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("a message to start the client")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_StartClient")]
    [SerializeField] VoidEvent m_StartClient;

    [Tooltip("a message to disconnect the client")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_DisconnectEvent")]
    [SerializeField] VoidEvent m_DisconnectClient;

    // -- published --
    [Header("published")]
    [Tooltip("a message to show an error")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_ErrorEvent")]
    [SerializeField] StringEvent m_ShowError;

    [Tooltip("an event after the server starts")]
    [SerializeField] VoidEvent m_ServerStarted;

    [Tooltip("an event after the client starts")]
    [SerializeField] VoidEvent m_ClientStarted;

    // -- refs --
    [Header("refs")]
    [Tooltip("if this is a standalone server (no host client)")]
    [SerializeField] BoolReference m_IsStandalone;

    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

    // -- props --
    /// the current server/client state
    State m_State = State.Server;

    /// .
    DisposeBag subscriptions = new DisposeBag();

    // -- lifecycle --
    public override void Awake() {
        base.Awake();

        // bind atom events
        subscriptions
            .Add(m_StartClient, OnTryStartClient)
            .Add(m_DisconnectClient, OnTryDisconnect);

        // feedback when you do things you don't want to
        #if UNITY_EDITOR && UNITY_SERVER
        Debug.LogError("[online] you probably don't want to run server mode in the editor!");
        #endif
    }

    public override void Start() {
        base.Start();

        if (IsStandalone) {
            StartAsServer();
        } else if (!m_ConnectToServerOnStart) {
            StartAsHost();
        } else {
            OnTryStartClient();
        }
    }

    public override void OnDestroy() {
        base.OnDestroy();

        // unbind events
        subscriptions.Dispose();
    }

    // -- l/client
    /// [Client]
    public override void OnClientError(Exception exception) {
        base.OnClientError(exception);

        Debug.Log($"[online] client error: {exception}");
        m_ShowError?.Raise($"[online] client error: {exception.Message}");
    }

    /// [Client]
    public override void OnClientConnect() {
        base.OnClientConnect();

        // finish connection flow
        if (m_State == State.Connecting) {
            m_State = State.Client;
            Debug.Log($"[online] connected as client");
        }
        else if (m_State == State.Server) {
            Debug.Log($"[online] connected as host client");
        }

        // create player
        var message = new CreatePlayerMessage();
        NetworkClient.Send(message);
    }

    /// [Client]
    public override void OnClientNotReady() {
        base.OnClientNotReady();

        Debug.Log($"[online] client not ready...");
    }

    /// [Client]
    public override void OnClientDisconnect() {
        base.OnClientDisconnect();

        Debug.Log($"[online] client disconnected...");

        // if we are a host, we don't do anything
        if (IsServerActive) {
            Debug.Log($"[online] host client disconnected");
            return;
        }

        // if we're still attempting to connect, we timed out
        if (m_State == State.Connecting) {
            m_ShowError?.Raise($"[online] failed to connect to server {networkAddress}");
        }
        // if we are non host client disconnect from a server, sync our player record
        else {
            m_Store.SyncPlayer();
        }

        // and then restart as a host
        if (m_RestartHostOnDisconnect) {
            this.DoNextFrame(() => SwitchToServer());
        }
    }

    // -- l/server
    /// [Server]
    public override void OnStartClient() {
        base.OnStartClient();

        // broadcast event
        Debug.Log("[online] started as client");
        m_ClientStarted.Raise();
    }

    /// [Server]
    public override void OnStartServer() {
        base.OnStartServer();

        // bind message handlers
        NetworkServer.RegisterHandler<CreatePlayerMessage>(Server_OnCreatePlayer);

        // broadcast event
        Debug.Log("[online] started as server");
        m_ServerStarted.Raise();
    }

    /// [Server]
    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);

        Debug.Log($"[online] connect [id={conn.connectionId} addr={conn.address}]");
    }

    /// [Server]
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
        SwitchToServer();
    }

    /// start the game as a host (server & client)
    void StartAsHost() {
        // set the initial address
        var addr = m_ServerAddres?.Value;
        if (addr != null && addr != "") {
            networkAddress = addr;
        }

        // start a host for every player, immediately
        // TODO: is this a good idea? for now at least
        try {
            SwitchToServer();
        } catch (System.Net.Sockets.SocketException err) {
            var code = err.ErrorCode;
            if (code == 10048 && (addr == "localhost" || addr == "127.0.0.1")) {
                SwitchToClient();
            }
        }
    }

    /// start game as server/host
    void SwitchToServer() {
        m_State = State.Server;
        m_IsServer.Value = true;

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
        m_IsServer.Value = false;

        StartClient();
    }

    // -- queries --
    /// .
    bool IsServerActive {
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
        if (IsServerActive) {
            var _ = m_Store.Save();
        }
        // if client, just sync player
        else {
            m_Store.SyncPlayer();
        }

        // stop the host, if active
        if (IsServerActive) {
            StopHost();
        }

        this.DoNextFrame(() => {
            // start the client
            networkAddress = m_ServerAddres.Value;
            SwitchToClient();
            Debug.Log($"[online] connecting client: {networkAddress}");
        });
    }

    /// when the host/client disconnect
    void OnTryDisconnect() {
        // can't stop hosting...
        if (IsServerActive) {
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