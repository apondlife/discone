using System.Linq;
using System.Collections.Generic;
using UnityAtoms;
using Mirror;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// a repository of flowers
public sealed class Flowers: NetworkBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the chunk size for flower position hashing")]
    [SerializeField] FloatReference m_ChunkSize;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when a flower is planted")]
    [SerializeField] CharacterFlowerEvent m_FlowerPlanted;

    // -- refs --
    [Header("refs")]
    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

    // -- props --
    /// a map of all flowers
    Dictionary<Vector3, CharacterFlower> m_All = new Dictionary<Vector3, CharacterFlower>();

    /// a bag of subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_FlowerPlanted, OnFlowerPlanted);
    }

    void OnDestroy() {
        // release events
        m_Subscriptions.Dispose();
    }

    // -- l/mirror
    public override void OnStartServer() {
        base.OnStartServer();

        // bind server events
        m_Subscriptions
            .Add(m_Store.LoadFinished, Server_OnStoreLoadFinished);
    }

    // -- commands --
    /// spawn all flowers from the store
    [Server]
    void Server_SpawnFlowers() {
        // find flowers, if any
        var flowers = m_Store.World?.Flowers;
        if (flowers == null) {
            return;
        }

        // spawn all flowers
        Debug.Log($"[world] loaded {flowers.Length} flowers, spawning...");
        foreach (var rec in flowers) {
            CharacterFlower.Server_Spawn(rec);
        }
    }

    // -- queries --
    /// the list of all flowers
    public IEnumerable<CharacterFlower> All {
        get => m_All.Values;
    }

    /// find the closest flower
    public CharacterFlower FindClosest(Vector3 pos) {
        var distCache = new Dictionary<CharacterFlower, float>();

        float Distance(CharacterFlower f) {
            if(!distCache.TryGetValue(f, out var dist)) {
                dist = Vector3.Distance(f.transform.position, pos);
                distCache.Add(f, dist);
            } else {
                Debug.Log($"cache miss");
            }

            return dist;
        }

        var closest = m_All.Values
            .OrderBy(Distance)
            .FirstOrDefault();

        return closest;
    }

    /// find a flower that overlaps this one
    public CharacterFlower FindOverlap(Vector3 pos) {
        if (!m_All.TryGetValue(pos, out var flower)) {
            return null;
        }

        return flower;
    }

    // -- events --
    /// when a flower is planted
    void OnFlowerPlanted(CharacterFlower flower) {
        m_All.Add(flower.Checkpoint.Position, flower);
    }

    /// when the store finishes loading
    [Server]
    void Server_OnStoreLoadFinished() {
        Server_SpawnFlowers();
    }
}
