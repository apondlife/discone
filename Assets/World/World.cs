using Mirror;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// the world of discone
[RequireComponent(typeof(WorldChunks))]
public sealed class World: NetworkBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the world singleton")]
    [SerializeField] WorldVariable m_Single;

    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

    // -- refs --
    [Header("refs")]
    [Tooltip("if this is a server instance")]
    [SerializeField] BoolReference m_IsHost;

    // -- props --
    /// the chunks
    WorldChunks m_Chunks;

    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifeycle --
    void Awake() {
        // set props
        m_Chunks = GetComponent<WorldChunks>();

        // set singleton
        m_Single.Value = this;
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- l/mirror
    public override void OnStartServer() {
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

    // -- events --
    /// when the store finishes loading
    [Server]
    void Server_OnStoreLoadFinished() {
        Server_SpawnFlowers();
    }

    // -- queries --
    /// the chunks
    public WorldChunks Chunks {
        get => m_Chunks;
    }
}
