using UnityEngine;

/// the world of discone
[RequireComponent(typeof(WorldChunks))]
public sealed class World: MonoBehaviour {
    // -- props --
    /// the chunks
    WorldChunks m_Chunks;

    // -- lifeycle --
    void Awake() {
        /// set props
        m_Chunks = GetComponent<WorldChunks>();
    }

    // -- queries --
    /// the chunks
    public WorldChunks Chunks {
        get => m_Chunks;
    }
}
