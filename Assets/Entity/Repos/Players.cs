using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// a repository of players
public sealed class Players: MonoBehaviour {
    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when a player connects")]
    [SerializeField] OnlinePlayerEvent m_PlayerConnected;

    [Tooltip("when a player disconnets")]
    [SerializeField] OnlinePlayerEvent m_PlayerDisconnected;

    // -- refs --
    [Header("refs")]
    [Tooltip("if the player is the host")]
    [SerializeField] BoolReference m_IsHost;

    // -- props --
    /// the map of players keyed by net id
    List<OnlinePlayer> m_All = new List<OnlinePlayer>();

    /// the current (local) player
    OnlinePlayer[] m_Current = new OnlinePlayer[0];

    /// the subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_PlayerConnected, OnPlayerConnected)
            .Add(m_PlayerDisconnected, OnPlayerDisconnected);
    }

    void OnDestroy() {
        // clear events
        m_Subscriptions.Dispose();
    }

    // -- queries -
    /// if there are any players
    public bool Any {
        get => m_All.Count != 0;
    }

    /// the current (local) player
    public OnlinePlayer Current {
        get {
            return GetCurrent()?[0];
        }
    }

    /// the list of all players
    public IEnumerable<OnlinePlayer> All {
        get => m_All;
    }

    /// the list of players used to cull other entities
    public IEnumerable<OnlinePlayer> FindCullers() {
        return m_IsHost ? m_All : GetCurrent();
    }

    // -- events --
    /// when a player connects
    void OnPlayerConnected(OnlinePlayer player) {
        m_All.Add(player);
    }

    /// when a player disconnects
    void OnPlayerDisconnected(OnlinePlayer player) {
        m_All.Remove(player);

        if (player.isLocalPlayer) {
            m_Current = new OnlinePlayer[0];
        }
    }

    // -- helpers --
    OnlinePlayer[] GetCurrent() {
        if(m_Current.Length == 0) {
            var current = m_All.FirstOrDefault(p => p.isLocalPlayer);
                if(current != null) {
                    m_Current = new[] { current };
                } else {
                    return null;
                }
        }

        return m_Current;
    }

    /// get an id for a net id
    uint Id(NetworkIdentity identity) {
        return identity.netId;
    }

    /// get an id for a player
    uint Id(OnlinePlayer player) {
        return player.netIdentity.netId;
    }
}
