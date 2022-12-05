using Mirror;
using UnityAtoms;
using UnityEngine;

/// the world of discone
[RequireComponent(typeof(WorldChunks))]
public sealed class World: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the world singleton")]
    [SerializeField] WorldVariable m_Single;

    // -- props --
    /// the chunks
    WorldChunks m_Chunks;

    // -- lifeycle --
    void Awake() {
        // set props
        m_Chunks = GetComponent<WorldChunks>();

        // set singleton
        m_Single.Value = this;
    }

    // -- queries --
    /// the chunks
    public WorldChunks Chunks {
        get => m_Chunks;
    }
}
