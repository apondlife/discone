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
    public float m_VisRange = 10;

    [Tooltip("the maximum distance in chunks an object is visible")]
    [SerializeField] int m_ChunkDist;

    [Tooltip("Rebuild all every 'rebuildInterval' seconds.")]
    public float rebuildInterval = 1;

    double lastRebuildTime=-1;

    // -- refs --
    [Header("refs")]
    [Tooltip("the entity repos")]
    [SerializeField] EntitiesVariable m_Entities;

    [Tooltip("the world chunk size")]
    [SerializeField] FloatReference m_ChunkSize;

    // -- props --
    /// the players repo
    Players m_Players => m_Entities.Value.Players;

    /// a map of identity to coordinate
    Dictionary<NetworkIdentity, Vector2Int> m_Coords
        = new Dictionary<NetworkIdentity, Vector2Int>();

    // -- lifecycle --
    public override bool OnCheckObserver(
        NetworkIdentity identity,
        NetworkConnection newObserver
    ) {
        var player = newObserver.identity.GetComponent<OnlinePlayer>();
        if (player == null) {
            return false;
        }

        return OnCheckPlayerObserver(player, identity);
    }

    public override void OnRebuildObservers(
        NetworkIdentity identity,
        HashSet<NetworkConnection> newObservers,
        bool initialize
    ) {
        // TODO: maybe cache components/data in some other objects keyed by identity
        // TODO: are these coordinates updated in sensible places on the server???

        // for each connection
        foreach (var player in m_Players.All) {
            // if we are initializizing
            if(initialize) {
                var character = identity.GetComponent<DisconeCharacter>();
                // if we are spawning a character, it exists and its an initial character, it should be visible, so that the client can drive one of them
                if(character?.IsInitial == true) {
                    newObservers.Add(player.connectionToClient);
                    continue;
                }
            }

            if (OnCheckPlayerObserver(player, identity)) {
                newObservers.Add(player.connectionToClient);
            }
        }
    }

    Vector2Int GetCoord(NetworkIdentity identity) {
        Vector2Int icoord;

        // if we have a world coord, use that
        var coord = identity.GetComponent<WorldCoord>();
        if (coord != null) {
            icoord = coord.Value;
        }
        // otherwise, this object is static so cache the value
        else {
            if (!m_Coords.TryGetValue(identity, out icoord)) {
                icoord = WorldCoord.FromPosition(identity.transform.position, m_ChunkSize);
                m_Coords.Add(identity, icoord);
            }
        }

        return icoord;
    }

    bool OnCheckPlayerObserver(
        OnlinePlayer player,
        NetworkIdentity identity
    ) {
        if (player.Character == null) {
            // if the player has no character, it might be looking for an initial one
            var character = identity.GetComponent<DisconeCharacter>();
            if(character?.IsInitial == true) {
                return true;
            }
            return false;
        }

        if (player.Character.gameObject == identity.gameObject) {
            return true;
        }



        var icoord = GetCoord(identity);

        // check if the object is in a visible chunk
        var pcoord = player.Coord.Value;
        var isNearChunk = !(
            Mathf.Abs(icoord.x - pcoord.x) > m_ChunkDist ||
            Mathf.Abs(icoord.y - pcoord.y) > m_ChunkDist
        );

        if (!isNearChunk) {
            return false;
        }

        // check if the object is in sight range
        Vector3 position = identity.transform.position;
        var isNearPosition = Vector3.Distance(player.Position, position) < m_VisRange;
        if(!isNearPosition) {
            return false;
        }

        return true;
    }

    public override void SetHostVisibility(NetworkIdentity identity, bool visible) {
        // we want to ignore the default behaviour here of hiding the object


        #if UNITY_EDITOR
        var name = identity.gameObject.name;
        if(!visible) {
            identity.gameObject.name = $"~{name}";
        } else if(name[0] == '~') {
            identity.gameObject.name = name.Substring(1);
        }
        #endif

        // if the object is a character, we can set its sync stuff
        var character = identity.GetComponent<DisconeCharacter>();
        if(character != null) {
            character.SyncSimulation(visible);
        }

    }

    [ServerCallback]
    void Update()
    {
        // rebuild all spawned NetworkIdentity's observers every interval
        if (NetworkTime.time >= lastRebuildTime + rebuildInterval)
        {
            RebuildAll();
            lastRebuildTime = NetworkTime.time;
        }
    }
}
