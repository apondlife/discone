using UnityEngine;

public class FieldChunk: MonoBehaviour {
    // -- fields --
    [Header("config")]
    [Tooltip("the custom terrain data, if any")]
    [SerializeField] TerrainData m_CustomData;

    [Tooltip("the material for the height shader")]
    [SerializeField] Material m_TerrainHeight;

    [Header("references")]
    [Tooltip("the terrain")]
    [SerializeField] Terrain m_Terrain;

    [Tooltip("the terrain collider")]
    [SerializeField] TerrainCollider m_TerrainCollider;

    // -- props --
    /// the current coordinate
    Vector2Int m_Coord;

    /// the instantiated terrain data, if any
    /// TODO: this could be pooled
    TerrainData m_GeneratedData;

    // -- lifecycle --
    void Awake() {
        gameObject.hideFlags = HideFlags.DontSave;
    }

    // -- commands --
    /// load the heightmap for the coordinate
    public void Load(Vector2Int coord) {
        var x = coord.x;
        var y = coord.y;

        // store the coordinate
        m_Coord = coord;

        // get the name of the chunk / asset
        name = $"Chunk({x.ToSignedString()},{y.ToSignedString()})";

        // load the custom chunk data, if it exists
        // TODO: cache this? (https://forum.unity.com/threads/does-unity-cache-results-of-resources-load.270861/)
        m_CustomData = Resources.Load<TerrainData>(name);

        // if missing, use the generated data
        var td = m_CustomData;
        if (td == null) {
            if (m_GeneratedData == null) {
                m_GeneratedData = Instantiate(m_Terrain.terrainData);
                m_GeneratedData.name = "Chunk-generated";
            }

            td = m_GeneratedData;

            // render the heightmap for this coordinate
            Render(coord);
        }

        // update data used by terrain
        m_Terrain.terrainData = td;
        m_TerrainCollider.terrainData = td;
    }

    /// render the generated heightmap for the coordinate
    void Render(Vector2Int coord) {
        var x = coord.x;
        var y = coord.y;

        // render into the generated data
        var td = m_GeneratedData;

        // in order for the edges of each chunk to overlap, we need to scale the coordinate offset so
        // that the last row of vertices of the neighbor chunk and the first row in this chunk are
        // the same. the offset is in uv-space.
        // see: https://answers.unity.com/questions/581760/why-are-heightmap-resolutions-power-of-2-plus-one.html
        var offsetScale = (float)(td.heightmapResolution - 1) / td.heightmapResolution;

        // render height material into chunk heightmap
        m_TerrainHeight.SetVector(
            "_Offset",
            new Vector3(x, y) * offsetScale
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

    /// set the chunk's neighbors
    public void SetNeighbors(
        FieldChunk l,
        FieldChunk t,
        FieldChunk r,
        FieldChunk b
    ) {
        m_Terrain.SetNeighbors(
            l?.m_Terrain,
            t?.m_Terrain,
            r?.m_Terrain,
            b?.m_Terrain
        );
    }

    /// refresh the chunk's current coordinate
    public void Reload() {
        Load(m_Coord);
    }

    // -- queries --
    /// the terrain's terrain data
    public TerrainData TerrainData {
        get => m_Terrain.terrainData;
    }
}
