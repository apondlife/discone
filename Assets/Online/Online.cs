using kcp2k;
using Mirror;
using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace Discone {

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
    [SerializeField] StringReference m_ServerAddress;

    [Tooltip("if this is running as server")]
    [SerializeField] BoolVariable m_IsServer;

    // -- config --
    [Header("config")]
    [Tooltip("if the client should try connecting to the server on start")]
    [SerializeField] bool m_ConnectToServerOnStart;

    [Tooltip("if the client should restart as host disconnect")]
    [SerializeField] bool m_RestartHostOnDisconnect;

    [Tooltip("the maximum number of times to retry on port conflicts")]
    [SerializeField] int m_MaxPortConflicts;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("a message to start the client")]
    [SerializeField] VoidEvent m_StartClient;

    [Tooltip("a message to disconnect the client")]
    [SerializeField] VoidEvent m_DisconnectClient;

    [Tooltip("creates a new online player")]
    [SerializeField] VoidEvent m_Online_CreatePlayer;

    // -- published --
    [Header("published")]
    [Tooltip("a message to show an error")]
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

    /// the number of port conflicts we've hit attempting to start a host
    int m_PortConflicts = 0;

    /// .
    DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    public override void Awake() {
        base.Awake();

        // bind atom events
        m_Subscriptions
            .Add(m_StartClient, OnTryStartClient)
            .Add(m_DisconnectClient, OnTryDisconnect)
            .Add(m_Online_CreatePlayer, Client_OnCreatePlayer);

        // feedback when you do things you don't want to
        #if UNITY_EDITOR && UNITY_SERVER
        Log.Online.E($"you probably don't want to run server mode in the editor!");
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
        m_Subscriptions.Dispose();
    }

    // -- l/client
    /// [Client]
    public override void OnClientError(TransportError error, string reason) {
        base.OnClientError(error, reason);

        Log.Online.I($"client error: {error}@{reason}");
        m_ShowError?.Raise($"client error: {error}@{reason}");
    }

    /// [Client]
    public override void OnClientConnect() {
        base.OnClientConnect();

        // finish connection flow
        if (m_State == State.Connecting) {
            m_State = State.Client;
            Log.Online.I($"connected as client");
        }
        else if (m_State == State.Server) {
            Log.Online.I($"connected as host client");
        }

        CreatePlayer();
    }

    /// [Client]
    public override void OnClientNotReady() {
        base.OnClientNotReady();

        Log.Online.I($"client not ready...");
    }

    /// [Client]
    public override void OnClientDisconnect() {
        base.OnClientDisconnect();

        Log.Online.I($"client disconnected...");

        // if we are a host, we don't do anything
        if (IsServerActive) {
            Log.Online.I($"host client disconnected");
            return;
        }

        // if we're still attempting to connect, we timed out
        if (m_State == State.Connecting) {
            m_ShowError?.Raise($"failed to connect to server {networkAddress}");
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
        Log.Online.I($"started as client");

        // broadcast event
        m_ClientStarted.Raise();
    }

    /// [Server]
    public override void OnStartServer() {
        base.OnStartServer();
        Log.Online.I($"started as server");

        // bind message handlers
        NetworkServer.RegisterHandler<CreatePlayerMessage>(Server_OnCreatePlayer);

        // broadcast event
        m_ServerStarted.Raise();
    }

    /// [Server]
    public override void OnServerConnect(NetworkConnectionToClient conn) {
        base.OnServerConnect(conn);
        Log.Online.I($"connect <id={conn.connectionId} addr={conn.address}>");
    }

    /// [Server]
    public override void OnServerDisconnect(NetworkConnectionToClient conn) {
        Log.Online.I($"disconnect <id={conn.connectionId} addr={conn.address}>");

        // give player a chance to clean up before being destroyed
        var identity = conn.identity;
        if (identity == null) {
            Log.Online.E($"disconnected player did not have an identity");
        }

        var player = identity?.gameObject.GetComponent<OnlinePlayer>();
        if (player == null) {
            Log.Online.E($"disconnected player is not an OnlinePlayer!");
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
        // start a host for every player, immediately
        // TODO: is this a good idea? for now at least
        try {
            SwitchToServer();
        } catch (System.Net.Sockets.SocketException err) {
            // if we have a port conflict, try to start on a new port
            if (err.ErrorCode == 10048 && transport is KcpTransport kcp) {
                // abandon retry if we reach our maximum attempts
                m_PortConflicts += 1;
                if (m_PortConflicts > m_MaxPortConflicts) {
                    Log.Online.E($"tried to start host {m_PortConflicts} times, but all were occupied");
                    throw;
                }

                // otherwise, try the next port
                Log.Online.W($"tried to start host but port {kcp.port} is occupied");
                kcp.port += 1;
                StartAsHost();
                return;
            }

            throw;
        }
    }

    /// start game as server/host
    void SwitchToServer() {
        m_State = State.Server;
        m_IsServer.Value = true;

        if (IsStandalone) {
            Log.Online.I($"switch to server (standalone) @ port {Port}");
            StartServer();
        } else {
            Log.Online.I($"switch to server (host) @ port {Port}");
            StartHost();
        }
    }

    /// start game as client
    void SwitchToClient() {
        // set the initial address
        var addr = m_ServerAddress?.Value;
        if (addr != null && addr != "") {
            networkAddress = addr;
        }

        // try to start the client
        Log.Online.I($"switch to client @ ip {networkAddress}:{Port}");

        m_State = State.Connecting;
        m_IsServer.Value = false;

        StartClient();
    }

    /// restart the game (which will reload the scene)
    public void Restart() {
        if (NetworkClient.isConnected) {
            StopClient();
        }

        if (NetworkServer.active) {
            StopServer();
        }

        ResetStatics();
    }

    // create player
    void CreatePlayer() {
        var message = new CreatePlayerMessage();
        NetworkClient.Send(message);
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

    /// the current port, if available
    int Port {
        get {
            if (transport is KcpTransport kcp) {
                return kcp.port;
            }

            return -1;
        }
    }

    // -- events --
    [Server]
    void Server_OnCreatePlayer(
        NetworkConnectionToClient conn,
        CreatePlayerMessage _,
        int channelId
    ) {
        var t = GetStartPosition();
        Log.Online.I($"on create player @ {t.name} <id={conn.connectionId} addr={conn.address} ch={channelId}>");

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

        this.DoNextFrame(SwitchToClient);
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

    /// creates a new player from the client
    [Client]
    void Client_OnCreatePlayer() {
        CreatePlayer();
    }
}

public struct CreatePlayerMessage: NetworkMessage {
}

}