using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// a chunk of field terrain
public class FieldChunk: MonoBehaviour {
    // -- fields --
    [Header("tuning")]
    [Tooltip("the custom chunk data, if any")]
    [SerializeField] FieldChunkData m_CustomData;

    [Header("config")]
    [Tooltip("the material for the height shader")]
    [SerializeField] Material m_TerrainHeight;

    [Tooltip("the prefab terrain data")]
    [SerializeField] TerrainData m_TerrainDataPrefab;

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

    /// the instantiated material, if any
    /// TODO: this could be pooled
    Material m_GeneratedMaterial;

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
        m_CustomData = Resources.Load<FieldChunkData>(name);

        // if missing, use the generated data
        var td = m_CustomData?.TerrainData;
        if (td == null) {
            if (m_GeneratedData == null) {
                // TODO: we instantiate so that we can change the material props based on
                // neighbors, but is this the right approach?
                m_GeneratedData = Instantiate(m_TerrainDataPrefab);
                m_GeneratedData.name = "Chunk-generated";
            }

            td = m_GeneratedData;

            // render the heightmap for this coordinate
            Render(coord);
        }

        // update data used by terrain
        m_Terrain.terrainData = td;
        m_TerrainCollider.terrainData = td;

        // use a custom material, if any
        var mat = m_CustomData?.Material;
        if (mat == null) {
            if (m_GeneratedMaterial == null) {
                m_GeneratedMaterial = Instantiate(m_Terrain.materialTemplate);
                m_GeneratedMaterial.name = "Chunk-generated";
            }

            mat = m_GeneratedMaterial;
        }

        m_Terrain.materialTemplate = mat;
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

        m_TerrainHeight.SetFloat(
            "_TerrainHeight",
            m_Terrain.terrainData.heightmapScale.y
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

        if (m_CustomData?.Material != null) {
            return;
        }

        var neighbors = new List<FieldChunk>() { l, t, r, b }
            .Where(f => f != null)
            .ToList();

        if (neighbors.Count == 0) {
            return;
        }

        void SetMaterialPropertyFromNeighbors(string name) {
            var total = 0.0f;

            foreach (var neighbor in neighbors) {
                var material = neighbor.m_Terrain.materialTemplate;
                total += material.GetFloat(name);
            }

            m_Terrain.materialTemplate.SetFloat(name, total / neighbors.Count);
        }

        new[] {"_HueMin", "_HueMax", "_SatMin", "_SatMax", "_ValMin", "_ValMax"}
            .ToList()
            .ForEach(SetMaterialPropertyFromNeighbors);
    }

    /// refresh the chunk's current coordinate
    public void Reload() {
        Load(m_Coord);
    }

    // -- queries --
    /// the size of the chunk
    public Vector3 Size {
        get => m_TerrainDataPrefab.size;
    }

    /// the active terrain data
    public TerrainData TerrainData {
        get => m_Terrain.terrainData;
    }

    /// the active terrain material
    public Material Material {
        get => m_Terrain.materialTemplate;
    }

    /// the custom terrain data, if any
    public FieldChunkData CustomData {
        get => m_CustomData;
    }
}
