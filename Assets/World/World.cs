using UnityEngine;

/// the world of discone
[RequireComponent(typeof(WorldEntities))]
[RequireComponent(typeof(WorldChunks))]
sealed class World: MonoBehaviour {
    // -- props --
    /// the entities
    WorldEntities m_Entities;

    /// the chunks
    WorldChunks m_Chunks;

    // -- lifeycle --
    void Awake() {
        /// set props
        m_Entities = GetComponent<WorldEntities>();
        m_Chunks = GetComponent<WorldChunks>();
    }

    // -- queries --
    /// the entities
    public WorldEntities Entities {
        get => m_Entities;
    }

    /// the chunks
    public WorldChunks Chunks {
        get => m_Chunks;
    }
}
