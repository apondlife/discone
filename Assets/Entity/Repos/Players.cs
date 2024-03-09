using Mirror;
using System.Collections.Generic;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

/// a repository of players
public sealed class Players: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the map of players keyed by net id")]
    [SerializeField] List<OnlinePlayer> m_All = new List<OnlinePlayer>();

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when a player connects")]
    [SerializeField] OnlinePlayerEvent m_PlayerConnected;

    [Tooltip("when a player disconnets")]
    [SerializeField] OnlinePlayerEvent m_PlayerDisconnected;

    [Tooltip("when the local player starts")]
    [SerializeField] OnlinePlayerEvent m_CurrentPlayerStarted;

    // -- refs --
    [Header("refs")]
    [Tooltip("if the player is the host")]
    [SerializeField] BoolReference m_IsHost;

    // -- props --
    /// the current (local) player
    OnlinePlayer[] m_Current = new OnlinePlayer[0];

    /// the subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_PlayerConnected, OnPlayerConnected)
            .Add(m_PlayerDisconnected, OnPlayerDisconnected)
            .Add(m_CurrentPlayerStarted, OnCurrentPlayerStarted);
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
        get => m_Current.Length != 0 ? m_Current[0] : null;
    }

    /// the list of all players
    public IEnumerable<OnlinePlayer> All {
        get => m_All;
    }

    /// the list of players used to cull other entities
    public IEnumerable<OnlinePlayer> FindCullers() {
        return m_IsHost ? m_All : m_Current;
    }

    // -- events --
    /// when a player connects
    void OnPlayerConnected(OnlinePlayer player) {
        m_All.Add(player);
    }

    /// when a player disconnects
    void OnPlayerDisconnected(OnlinePlayer player) {
        m_All.Remove(player);
    }

    /// when the currrent player starts
    void OnCurrentPlayerStarted(OnlinePlayer player) {
        m_Current = new OnlinePlayer[1] { player };
    }

    // -- helpers --
    /// get an id for a net id
    uint Id(NetworkIdentity identity) {
        return identity.netId;
    }

    /// get an id for a player
    uint Id(OnlinePlayer player) {
        return player.netIdentity.netId;
    }
}

}