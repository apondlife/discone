using System.Collections.Generic;
using UnityAtoms;
using UnityEngine;

/// a repository of players
public sealed class Players: MonoBehaviour {
    // -- events --
    [Header("events")]
    [Tooltip("when a player connects")]
    [SerializeField] OnlinePlayerEvent m_PlayerConnected;

    [Tooltip("when a player disconnets")]
    [SerializeField] OnlinePlayerEvent m_PlayerDisconnected;

    // -- props --
    /// the current (local) player
    OnlinePlayer m_Current;

    /// the list of players
    List<OnlinePlayer> m_All = new List<OnlinePlayer>();

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
    /// the current (local) player
    public OnlinePlayer Current {
        get => m_Current;
    }

    /// the list of all players
    public List<OnlinePlayer> All {
        get => m_All;
    }

    // -- events --
    /// when a player connects
    void OnPlayerConnected(OnlinePlayer player) {
        m_All.Add(player);

        if (player.isLocalPlayer) {
            m_Current = player;
        }
    }

    /// when a player disconnects
    void OnPlayerDisconnected(OnlinePlayer player) {
        m_All.Remove(player);

        if (player.isLocalPlayer) {
            m_Current = null;
        }
    }
}
