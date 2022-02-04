using UnityEngine;

/// an infinite field
sealed class Field: MonoBehaviour {
    // -- nodes --
    [Header("config")]
    [Tooltip("the target to follow")]
    [SerializeField] Transform m_Target;

    [Header("references")]
    [Tooltip("the prefab for creating terrain chunks")]
    [SerializeField] Terrain m_TerrainChunkPrefab;

    [Tooltip("the material for the height shader")]
    [SerializeField] Material m_TerrainHeight;

    // -- lifecycle --
    void Awake() {
        // update height map
        var chunks = new Terrain[9];
        for (var i = 0; i < chunks.Length; i++) {
            Instantiate(m_TerrainChunkPrefab);
        }

        // render the heightmap around the target position
        m_TerrainHeight.SetVector(
            "_TargetPosition",
            Vector3.zero
        );

        Graphics.Blit(
            null,
            td.heightmapTexture,
            m_TerrainHeight
        );

        // mark the entire heightmap as dirty
        var tr = new RectInt(
            0,
            0,
            td.heightmapResolution,
            td.heightmapResolution
        );

        td.DirtyHeightmapRegion(
            tr,
            TerrainHeightmapSyncControl.HeightOnly
        );

        // sync it
        td.SyncHeightmap();
    }
}
