using Mirror;
using System.Collections.Generic;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

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

    /// the time of the last rebuild
    double m_LastRebuildTime = -1.0;

    // -- lifecycle --
    [ServerCallback]
    void Update() {
        // rebuild all spawned NetworkIdentity's observers every interval
        if (NetworkTime.time >= m_LastRebuildTime + m_RebuildInterval) {
            RebuildAll();
            m_LastRebuildTime = NetworkTime.time;
        }
    }

    // -- InterestManagment --
    [Server]
    public override bool OnCheckObserver(
        NetworkIdentity identity,
        NetworkConnection newObserver
    ) {
        var player = newObserver.identity.GetComponent<OnlinePlayer>();
        if (player == null) {
            return false;
        }

        return IsInteresting(identity, player);
    }

    [Server]
    public override void OnRebuildObservers(
        NetworkIdentity identity,
        HashSet<NetworkConnection> newObservers,
        bool initialize
    ) {
        // check which players can see the identity
        var players = m_Entities.Value.Players.All;
        foreach (var player in players) {
            if (IsInteresting(identity, player)) {
                newObservers.Add(player.connectionToClient);
            }
        }
    }

    [Server]
    public override void SetHostVisibility(NetworkIdentity identity, bool visible) {
        // we want to ignore the default behaviour here of hiding the object
        #if UNITY_EDITOR
        var name = identity.gameObject.name;
        if (!visible) {
            identity.gameObject.name = $"~{name}";
        } else if (name[0] == '~') {
            identity.gameObject.name = name.Substring(1);
        }
        #endif

        // if the object is a character, we can set its sync stuff
        var character = identity.GetComponent<DisconeCharacter>();
        if (character != null) {
            character.SyncSimulation(visible);
        }
    }

    // -- queries --
    /// if the identity is interesting to the player
    [Server]
    bool IsInteresting(
        NetworkIdentity identity,
        OnlinePlayer player
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
        OnlinePlayer player
    ) {
        var character = interest.Object;

        // if a player is driving the character, we're interested in it.
        // this includes the current player's character
        if (m_Entities.Value.Characters.IsDriven(character)) {
            return true;
        }

        // if the player has no character, it might be looking for an initial one
        if (player.Character == null) {
            return character.IsInitial;
        }

        return IsVisible(interest, player);
    }

    /// if the other player is interesting to the player
    [Server]
    bool IsInteresting(
        PlayerInterest interest,
        OnlinePlayer player
    ) {
        // all players are interesting
        // the stars follow them
        return true;
    }

    /// if the flower is interesting to the player
    [Server]
    bool IsInteresting(
        FlowerInterest interest,
        OnlinePlayer player
    ) {
        return IsVisible(interest, player);
    }

    /// if the interest is visible to the player
    bool IsVisible(
        Interest interest,
        OnlinePlayer player
    ) {
        // if the object is not a visible chunk, we're not interested
        var icoord = interest.Coord;
        var pcoord = player.Coord.Value;
        var isVisible = !(
            Mathf.Abs(icoord.x - pcoord.x) > m_ChunkDist ||
            Mathf.Abs(icoord.y - pcoord.y) > m_ChunkDist
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
            Debug.LogError("[interest] identity has not been initialized yet.");
            return null;
        }

        // get the cached interest by id
        if (!m_Interests.TryGetValue(id, out var interest)) {
            if (identity.GetComponent<OnlinePlayer>() is OnlinePlayer p) {
                interest = new PlayerInterest(p);
            } else if (identity.GetComponent<DisconeCharacter>() is DisconeCharacter c) {
                interest = new CharacterInterest(c);
            } else if (identity.GetComponent<CharacterFlower>() is CharacterFlower f) {
                interest = new FlowerInterest(f);
            } else {
                #if UNITY_EDITOR
                FoundUninterestingType(identity.GetComponent<NetworkBehaviour>());
                #endif
            }

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
    sealed class CharacterInterest: Interest<DisconeCharacter> {
        // -- Interest --
        public override bool IsStatic => false;
        public CharacterInterest(DisconeCharacter c) : base(c) {}
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
            Debug.LogWarning($"[interest] interest in object of unknown type: {c.name}");
        }
    }
    #endif
}
