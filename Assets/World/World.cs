using Mirror;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// the world of discone
[RequireComponent(typeof(WorldChunks))]
public sealed class World: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the world singleton")]
    [SerializeField] WorldVariable m_Single;

    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

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

        // bind events
        m_Subscriptions
            .Add(m_Store.LoadFinished, OnStoreLoadFinished);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// spawn all flowers from the store
    [Server]
    void SpawnFlowers() {
        // find flowers, if any
        var flowers = m_Store.World?.Flowers;
        if (flowers == null) {
            return;
        }

        // spawn all flowers
        foreach (var rec in flowers) {
            CharacterFlower.Spawn(rec);
        }
    }

    // -- events --
    /// when the store finishes loading
    private void OnStoreLoadFinished() {
        SpawnFlowers();
    }

    // -- queries --
    /// the chunks
    public WorldChunks Chunks {
        get => m_Chunks;
    }
}
