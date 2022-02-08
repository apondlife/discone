using UnityEngine;

/// an infinite field
sealed class Field: MonoBehaviour {
    // -- nodes --
    [Header("config")]
    [Tooltip("the target to follow")]
    [SerializeField] Transform m_Target;

    [Header("references")]
    [Tooltip("the terrain")]
    [SerializeField] Terrain m_Terrain;

    [Tooltip("a material for the height shader")]
    [SerializeField] Material m_TerrainHeight;

    // -- lifecycle --
    void Start() {
        // update height map
        var td = m_Terrain.terrainData;

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
