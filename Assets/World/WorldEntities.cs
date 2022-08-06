using System.Collections.Generic;
using System.Linq;
using UnityAtoms;
using UnityEngine;

/// a repository of entities in the world; pretty anemic rn
sealed class WorldEntities: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the tag for characters")]
    [SerializeField] string m_CharacterTag;

    // -- events --
    [Header("events")]
    [Tooltip("when a player connects")]
    [SerializeField] OnlinePlayerEvent m_PlayerConnected;

    [Tooltip("when a player disconnets")]
    [SerializeField] OnlinePlayerEvent m_PlayerDisconnected;

    // -- props --
    /// the current (local) player
    OnlinePlayer m_CurrentPlayer;

    /// the list of players
    List<OnlinePlayer> m_Players = new List<OnlinePlayer>();

    /// the list of characters
    List<DisconeCharacter> m_Characters;

    /// the subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        // find all characters
        m_Characters = GameObject
            .FindGameObjectsWithTag(m_CharacterTag)
            .Select((o) => o.GetComponent<DisconeCharacter>())
            .ToList();

        // bind events
        m_Subscriptions
            .Add(m_PlayerConnected, OnPlayerConnected)
            .Add(m_PlayerDisconnected, OnPlayerDisconnected);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- queries -
    /// the current (local) player
    public OnlinePlayer CurrentPlayer {
        get => m_CurrentPlayer;
    }

    /// the list of all players
    public List<OnlinePlayer> Players {
        get => m_Players;
    }

    /// the list of all characters
    public List<OnlinePlayer> Characters {
        get => m_Players;
    }

    // -- events --
    /// when a player connects
    void OnPlayerConnected(OnlinePlayer player) {
        m_Players.Add(player);

        if (player.isLocalPlayer) {
            m_CurrentPlayer = player;
        }
    }

    /// when a player disconnects
    void OnPlayerDisconnected(OnlinePlayer player) {
        m_Players.Remove(player);

        if (player.isLocalPlayer) {
            m_CurrentPlayer = null;
        }
    }
}
