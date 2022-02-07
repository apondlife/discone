using System.Collections.Generic;
using UnityEngine;

/// an infinite field
sealed class Field: MonoBehaviour {
    // -- nodes --
    [Header("config")]
    [Tooltip("the target to follow")]
    [SerializeField] Transform m_Target;

    [Header("references")]
    [Tooltip("the prefab for creating terrain")]
    [SerializeField] GameObject m_Terrain;

    [Tooltip("the material for the height shader")]
    [SerializeField] Material m_TerrainHeight;

    [Tooltip("the material for the height shader")]
    [SerializeField] Material m_Whatever;

    // -- props --
    /// the target's current coordinate
    Vector2Int m_TargetCoord;

    /// the size of a chunk
    float m_ChunkSize;

    /// the map of visible chunks
    Dictionary<Vector2Int, Terrain> m_Chunks;

    /// a pool of free terrain instances
    Queue<Terrain> m_TerrainPool = new Queue<Terrain>();

    // -- lifecycle --
    void Awake() {
        // ensure terrain is square
        var td = m_Terrain.GetComponent<Terrain>().terrainData;
        Debug.Assert(td.size.x == td.size.z, "field's terrain chunk was not square");

        // capture chunk size
        m_ChunkSize = td.size.x;

        // get initial target coordinate
        m_TargetCoord = IntoCoordinate(m_Target.position);

        // instantiate a square of 9 chunks around the target
        var chunks = new Dictionary<Vector2Int, Terrain>();
        for (var i = 0; i < 9; i++) {
            // get terrain
            var terrain = DequeueTerrain();

            // get coord
            var coord = m_TargetCoord;
            coord.x += i % 3 - 1;
            coord.y += i / 3 - 1;

            // store chunk
            chunks.Add(coord, terrain);
        }

        m_Chunks = chunks;

        // render each chunk
        foreach (var (coord, terrain) in chunks) {
            RenderChunk(coord, terrain);
        }

        // reassign the chunk neighbors
        foreach (var (coord, terrain) in chunks) {
            terrain.SetNeighbors(
                m_Chunks.Get(coord + Vector2Int.left),
                m_Chunks.Get(coord + Vector2Int.up),
                m_Chunks.Get(coord + Vector2Int.right),
                m_Chunks.Get(coord + Vector2Int.down)
            );
        }
    }

    // -- commands --
    /// render the heightmap for a particular terrain chunk & offset
    void RenderChunk(Vector2Int coord, Terrain terrain) {
        var td = terrain.terrainData;

        // set display name
        terrain.name = $"Chunk ({coord.x}, {coord.y})";

        // get chunk position
        var pos = IntoPosition(coord);
        Debug.Log($"[Field] render chunk {coord}");

        // render height material into chunk heightmap
        m_TerrainHeight.SetVector(
            "_TargetPosition",
            IntoPosition(coord)
        );

        Graphics.Blit(
            null,
            td.heightmapTexture,
            coord.x > 0 ? m_Whatever : m_TerrainHeight
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

        // move the terrain into position
        var tt = terrain.transform;
        tt.position = pos;
    }

    /// dequeue a terrain from the terrain pool
    Terrain DequeueTerrain() {
        // reuse an existing terrain if available
        if (m_TerrainPool.Count != 0) {
            return m_TerrainPool.Dequeue();
        }

        // otherwise, create a new terrain and terrain data
        var obj = Instantiate(m_Terrain, transform);
        var terrain = obj.GetComponent<Terrain>();
        terrain.terrainData = Instantiate(terrain.terrainData);

        return terrain;
    }

    // -- queries --
    /// finds the chunk coordinate at the position
    Vector2Int IntoCoordinate(Vector3 pos) {
        var cs = m_ChunkSize;
        var ch = cs * 0.5f;

        // get x and y coord for this position
        var x = Mathf.FloorToInt((pos.x + ch) / cs);
        var y = Mathf.FloorToInt((pos.z + ch) / cs);

        return new Vector2Int(x, y);
    }

    /// finds the position of this coordinate
    Vector3 IntoPosition(Vector2Int coord) {
        var cs = m_ChunkSize;
        var ch = cs * 0.5f;

        // get x and z position for this coordinate
        var x = coord.x * cs - ch;
        var z = coord.y * cs - ch;

        return new Vector3(x, 0.0f, z);
    }
}
