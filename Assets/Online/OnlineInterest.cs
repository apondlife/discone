using Mirror;
using System.Collections.Generic;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

/// calculates the players interested in an object
public class OnlineInterest: InterestManagement {
    // -- config --
    [Header("config")]
    [Tooltip("The maximum range that objects will be visible at.")]
    [SerializeField] float m_VisRange = 10.0f;

    [Tooltip("the maximum distance in chunks an object is visible")]
    [SerializeField] int m_ChunkDist;

    [Tooltip("the chunk size")]
    [SerializeField] FloatReference m_ChunkSize;

    [Tooltip("the interval (s) to rebuild an identity's observers")]
    [UnityEngine.Serialization.FormerlySerializedAs("rebuildInterval")]
    [SerializeField] float m_RebuildInterval = 1.0f;

    // -- refs --
    [Header("refs")]
    [Tooltip("the entity repos")]
    [SerializeField] EntitiesVariable m_Entities;

    // -- props --
    /// a cache of interesting objects by id
    Dictionary<uint, Interest> m_Interests = new Dictionary<uint, Interest>();

    /// if the set of simulated characters changed
    bool m_SimulatedChanged;

    /// the set net ids for characters the host simulates
    HashSet<uint> m_SimulatedCharacters = new HashSet<uint>();

    /// the time of the last rebuild
    double m_LastRebuildTime = -1.0;

    /// if the interest logged a warning about initialization w/ no netId
    bool m_HasLoggedNoIdWarning;

    // -- lifecycle --
    [ServerCallback]
    void Update() {
        if (m_Entities.Value == null) {
            return;
        }

        // rebuild interest on an interval
        if (NetworkTime.time >= m_LastRebuildTime + m_RebuildInterval) {
            // see if we have any players
            var hasPlayer = false;

            // sync all the player coords
            foreach (var conn in NetworkServer.connections.Values) {
                if (conn == null || !conn.isAuthenticated || conn.identity == null) {
                    continue;
                }

                hasPlayer = true;
                SyncCoord(FindOrCreateInterestById(conn.identity));
            }

            // rebuild the set of simulated charaters
            m_SimulatedChanged = true;
            m_SimulatedCharacters.Clear();

            // if we have at least one player, rebuild interest set
            if (hasPlayer) {
                RebuildAll();
                m_LastRebuildTime = NetworkTime.time;
            }
        }

        // if the set of simulated characters may have changed, update the
        // server's simulation of every character. we only need to simulate
        // characters that some player is interested in.
        if (m_SimulatedChanged) {
            foreach (var character in m_Entities.Value.Characters.All) {
                var online = character.Online;
                online.SyncSimulation(m_SimulatedCharacters.Contains(online.netId));
            }

            m_SimulatedChanged = false;
        }
    }

    // -- InterestManagment --
    /// [Server]
    public override void Reset() {
        base.Reset();

        Log.Interest.I($"reset state");

        m_Interests.Clear();
        m_SimulatedCharacters.Clear();
        m_LastRebuildTime = -1.0;
        m_SimulatedChanged = false;
    }


    /// [Server]
    public override bool OnCheckObserver(
        NetworkIdentity identity,
        NetworkConnectionToClient newObserver
    ) {
        var player = FindOrCreateInterestById(newObserver.identity) as PlayerInterest;
        if (player == null) {
            return false;
        }

        return IsInteresting(identity, player);
    }

    /// [Server]
    public override void OnRebuildObservers(
        NetworkIdentity identity,
        HashSet<NetworkConnectionToClient> newObservers
    ) {
        // check which players can see the identity
        var players = m_Entities.Value.Players.All;
        foreach (var conn in NetworkServer.connections.Values) {
            if (conn == null || !conn.isAuthenticated || conn.identity == null) {
                continue;
            }

            var player = FindOrCreateInterestById(conn.identity) as PlayerInterest;
            if (player == null) {
                continue;
            }

            if (IsInteresting(identity, player)) {
                newObservers.Add(conn);
            }
        }
    }

    /// [Server]
    public override void SetHostVisibility(NetworkIdentity identity, bool visible) {
        // we want to ignore the default behaviour here of hiding the object
        #if UNITY_EDITOR
        var name = identity.gameObject.name;
        if (!visible) {
            identity.gameObject.name = $"*{name}";
        } else if (name[0] == '*') {
            identity.gameObject.name = name.Substring(1);
        }
        #endif

        var interest = FindOrCreateInterestById(identity);
        switch (interest) {
        // if the object is a flower we want to try planting it again
        case FlowerInterest f:
            f.Object.Host_SetVisibility(visible); break;
        }
    }

    // -- queries --
    /// if the identity is interesting to the player
    [Server]
    bool IsInteresting(
        NetworkIdentity identity,
        PlayerInterest player
    ) {
        // find or create cached interest
        var interest = FindOrCreateInterestById(identity);
        if (interest == null) {
            return true;
        }

        // update the coordinate of any dynamic object
        if (!interest.IsStatic) {
            SyncCoord(interest);
        }

        // dispatch correct interest method
        return interest switch {
            PlayerInterest p => IsInteresting(p, player),
            CharacterInterest c => IsInteresting(c, player),
            FlowerInterest f => IsInteresting(f, player),
            _ => true,
        };
    }

    /// if the character is interesting to the player
    [Server]
    bool IsInteresting(
        CharacterInterest interest,
        PlayerInterest player
    ) {
        var character = interest.Object.Character;

        // track is interesting so we can run add the character to a set as a
        // side effect
        var isInteresting = false;

        // if a player is driving the character, we're interested in it.
        // this includes the current player's character
        if (m_Entities.Value.Characters.IsDriven(character)) {
            isInteresting = true;
        }
        // unless the player just spawned a character on the server
        // in which case it's already set in the player, but not driven
        else if (player.Object.Character == character) {
            isInteresting = true;
        }
        // if the player has no character, it might be looking for an initial one
        else if (player.Object.Character == null) {
            // AAA: stale?
            isInteresting = character.Online.IsInitial;
        }
        // if the character is visible
        else {
            isInteresting = IsVisible(interest, player);
        }

        // cache all the interesting characters
        // so their simulation state can be updated accordingly
        var id = interest.Object.netId;
        if (isInteresting && id != 0) {
            m_SimulatedChanged = true;
            m_SimulatedCharacters.Add(id);
        }

        return isInteresting;
    }

    /// if the other player is interesting to the player
    [Server]
    bool IsInteresting(
        PlayerInterest interest,
        PlayerInterest player
    ) {
        // all players are interesting
        // the stars follow them
        return true;
    }

    /// if the flower is interesting to the player
    [Server]
    bool IsInteresting(
        FlowerInterest interest,
        PlayerInterest player
    ) {
        return IsVisible(interest, player);
    }

    /// if the interest is visible to the player
    bool IsVisible(
        Interest interest,
        PlayerInterest player
    ) {
        // if the object is not a visible chunk, we're not interested
        var icoord = interest.Coord;
        var pcoord = player.Coord;
        var isVisible = (
            Mathf.Abs(icoord.x - pcoord.x) <= m_ChunkDist &&
            Mathf.Abs(icoord.y - pcoord.y) <= m_ChunkDist
        );

        if (!isVisible) {
            return false;
        }

        // if a object is within the vision radius, we're not interested
        var dist = Vector3.Distance(
            player.Position,
            interest.Position
        );

        isVisible = dist < m_VisRange;
        if (!isVisible) {
            return false;
        }

        return true;
    }

    /// update the coordinate of the interest
    void SyncCoord(Interest interest) {
        interest.Coord = WorldCoord.FromPosition(interest.Position, m_ChunkSize);
    }

    // -- queries --
    /// find or create an interest for the id
    Interest FindOrCreateInterestById(NetworkIdentity identity) {
        // get the id
        var id = identity.netId;
        if (id == 0) {
            if (!m_HasLoggedNoIdWarning) {
                Log.Interest.W($"identity has not been initialized yet.");
                m_HasLoggedNoIdWarning = true;
            }

            return null;
        }

        // get the cached interest by id
        if (!m_Interests.TryGetValue(id, out var interest)) {
            // create the interest
            if (identity.GetComponent<OnlinePlayer>() is OnlinePlayer p) {
                interest = new PlayerInterest(p);
            } else if (identity.GetComponent<Character_Online>() is Character_Online c) {
                interest = new CharacterInterest(c);
            } else if (identity.GetComponent<CharacterFlower>() is CharacterFlower f) {
                interest = new FlowerInterest(f);
            } else {
                #if UNITY_EDITOR
                FoundUninterestingType(identity.GetComponent<NetworkBehaviour>());
                #endif
            }

            // cache it!
            m_Interests.Add(id, interest);

            // and set the coord of any static interests immediately (this should be a done in
            // the constructor, but passing the chunk size down is a pain)
            if (interest != null && interest.IsStatic) {
                SyncCoord(interest);
            }
        }

        return interest;
    }

    // -- Interests --
    /// an object of interest
    abstract class Interest {
        // -- props --
        /// the hashed coordinate of this object
        public Vector2Int Coord;

        // -- queries --
        /// if this object is static
        public abstract bool IsStatic { get; }

        /// the object's current position
        public abstract Vector3 Position { get; }
    }

    /// an object of interest
    abstract class Interest<T>: Interest where T: NetworkBehaviour {
        // -- props --
        /// the interesting object
        public readonly T Object;

        // -- lifetime --
        /// create a new interest
        public Interest(T obj) {
            Object = obj;
        }

        // -- queries --
        /// the object's current position
        public override Vector3 Position {
            get => Object.transform.position;
        }
    }

    /// a player of interest
    sealed class PlayerInterest: Interest<OnlinePlayer> {
        // -- Interest --
        public override bool IsStatic => false;
        public PlayerInterest(OnlinePlayer p): base(p) {}
    }

    /// a character of interest
    sealed class CharacterInterest: Interest<Character_Online> {
        // -- Interest --
        public override bool IsStatic => false;
        public CharacterInterest(Character_Online c) : base(c) {}
    }

    /// a flower of interest
    sealed class FlowerInterest: Interest<CharacterFlower> {
        // -- Interest --
        public override bool IsStatic => true;
        public FlowerInterest(CharacterFlower f): base(f) {}
    }

    // -- debugging --
    #if DEBUG
    /// the set of types without managed interest
    HashSet<System.Type> m_UninterestingTypes = new HashSet<System.Type>();

    /// warn on the first instance of an uninteresting type
    void FoundUninterestingType(NetworkBehaviour c) {
        var type = c.GetType();
        if (!m_UninterestingTypes.Contains(type)) {
            m_UninterestingTypes.Add(type);
            Log.Interest.W($"interest in object of unknown type: {c.name}");
        }
    }
#endif
}

}