using System.Collections.Generic;
using System.Linq;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// processes collisions (perception) for all players and characters
sealed class GameCollisions: MonoBehaviour {
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

    // -- refs --
    [Header("refs")]
    [Tooltip("if the player is the host")]
    [SerializeField] BoolReference m_IsHost;

    [Tooltip("the world chunk size")]
    [SerializeField] FloatReference m_ChunkSize;

    // -- props --
    /// the current list of players
    List<PlayerState> m_Players = new List<PlayerState>();

    /// the list of characters
    DisconeCharacter[] m_Characters;

    /// the subscriptions
    Subscriptions m_Subscriptions;

    // -- lifecycle --
    // void Awake() {
    //     // find all characters
    //     m_Characters = GameObject
    //         .FindGameObjectsWithTag(m_CharacterTag)
    //         .Select((o) => o.GetComponent<DisconeCharacter>())
    //         .ToArray();

    //     // bind events
    //     m_Subscriptions
    //         .Add(m_PlayerConnected, OnPlayerConnected)
    //         .Add(m_PlayerDisconnected, OnPlayerDisconnected);
    // }

    // void FixedUpdate() {
    //     foreach (var p in m_Players) {
    //         var pt = p.Player.transform;

    //         // if the target changed chunks, create neighbors
    //         var coord = IntoCoordinate(pt.position);
    //         if (coord != p.ChunkCoord) {
    //             p.ChunkCoord = coord;
    //             CreateChunks();
    //         }
    //     }
    // }

    // -- events --
    /// when a player connects
    void OnPlayerConnected(OnlinePlayer player) {
        if (m_IsHost.Value || player.isLocalPlayer) {
            m_Players.Add(new PlayerState(player));
        }
    }

    /// when a player disconnects
    void OnPlayerDisconnected(OnlinePlayer player) {
        if (m_IsHost.Value || player.isLocalPlayer) {
            var i = m_Players.FindIndex((s) => s.Player == player);
            if (i > 0) {
                m_Players.RemoveAt(i);
            }
        }
    }

    // -- queries --
    /// finds the chunk coordinate at the position
    Vector2Int IntoCoordinate(Vector3 pos) {
        var cs = m_ChunkSize.Value;
        var ch = cs * 0.5f;

        // get x and y coord for this position
        var x = Mathf.FloorToInt((pos.x + ch) / cs);
        var y = Mathf.FloorToInt((pos.z + ch) / cs);

        return new Vector2Int(x, y);
    }

    /// finds the position of this coordinate
    Vector3 IntoPosition(Vector2Int coord) {
        var cs = m_ChunkSize.Value;
        var ch = cs * 0.5f;

        // get x and z position for this coordinate
        var x = coord.x * cs - ch;
        var z = coord.y * cs - ch;

        return new Vector3(x, 0.0f, z);
    }

    // -- types --
    /// a player's current collision state
    private sealed class PlayerState {
        // -- constants --
        /// when then player is not in a chunk
        static readonly Vector2Int k_ChunkNone = new Vector2Int(69, 420);

        /// when the player is not talking to anyone
        static readonly int k_TalkingTargetNone = 420;

        // -- props --
        /// a reference to the player
        public readonly OnlinePlayer Player;

        /// the coordinate of the current chunk
        public Vector2Int ChunkCoord;

        /// the id of the chracter this player is talking to (a sentin)
        public int TalkingTargetId;

        // -- lifetime --
        /// create a state for this player
        public PlayerState(OnlinePlayer player) {
            Player = player;
            ChunkCoord = k_ChunkNone;
            TalkingTargetId = k_TalkingTargetNone;
        }
    }
}
