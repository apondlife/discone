using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityAtoms.BaseAtoms;

/// an infinite field
sealed class Field: MonoBehaviour {
    // -- nodes --
    [Header("config")]
    [Tooltip("the target to follow")]
    [SerializeField] GameObjectReference m_TargetObject;

    [Header("references")]
    [Tooltip("the prefab for creating terrain")]
    [SerializeField] GameObject m_Terrain;

    [Tooltip("the material for the height shader")]
    [SerializeField] Material m_TerrainHeight;

    // -- props --
    /// the target's current coordinate. the current center chunk index
    Vector2Int m_TargetCoord;

    /// the size of a chunk
    float m_ChunkSize;

    /// the map of visible chunks
    Dictionary<Vector2Int, Terrain> m_Chunks = new Dictionary<Vector2Int, Terrain>();

    /// a pool of free terrain instances
    Queue<Terrain> m_ChunkPool = new Queue<Terrain>();

    Transform m_Target => m_TargetObject.Value.transform;

    // -- lifecycle --
    void Start() {
        // ensure terrain is square
        var td = m_Terrain.GetComponent<Terrain>().terrainData;
        Debug.Assert(td.size.x == td.size.z, "field's terrain chunk was not square");

        // capture chunk size
        m_ChunkSize = td.size.x;

        // get initial target coordinate
        m_TargetCoord = IntoCoordinate(m_Target.position);

        // create the initial chunks
        CreateChunks();

        StartCoroutine(PurgeChunksAsync());
    }

    void Update() {
        // if we didn't change chunks, do nothing
        var coord = IntoCoordinate(m_Target.position);
        if (m_TargetCoord != coord) {
            m_TargetCoord = coord;
            CreateChunks();
        }

        if(debounceTimer > 0 && Time.time > debounceTimer) {
            debounceTimer = -1;
            PurgeChunks(m_TargetCoord);
        }
    }

    // -- commands --
    /// create new chunks as the player moves
    void CreateChunks() {
        // instantiate a square of 9 chunks around the target
        for (var i = 0; i < 9; i++) {
            var c = m_TargetCoord;
            c.x += i % 3 - 1;
            c.y += i / 3 - 1;

            CreateChunk(c);
        }
    }

    /// create a new chunk at a coordinate if necessary
    void CreateChunk(Vector2Int coord) {
        // if the chunk already exists, don't create one
        if (m_Chunks.ContainsKey(coord)) {
            return;
        }

        // add the chunk to the map of active chunks
        var chunk = DequeueChunk();
        m_Chunks.Add(coord, chunk);

        // render the chunks heightmap
        RenderChunk(coord, chunk);

        //
        chunk.SetNeighbors(
            m_Chunks.Get(coord + Vector2Int.left),
            m_Chunks.Get(coord + Vector2Int.up),
            m_Chunks.Get(coord + Vector2Int.right),
            m_Chunks.Get(coord + Vector2Int.down)
        );
    }

    private const int k_MaxChunks = 16;
    private const float k_ChunkPurgePeriod = 2.0f;

    int ManhDist(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    IEnumerator PurgeChunksAsync() {
        while (true) {
            yield return new WaitForSecondsRealtime(k_ChunkPurgePeriod);
            PurgeChunks(m_TargetCoord);
        }
    }

    float debounceTimer = 0;

    void PurgeChunks(Vector2Int coord) {
        if (m_Chunks.Count <= k_MaxChunks) {
            return;
        }

        // remove the chunks that are further away and no longer needed
        var sortedDistances = m_Chunks.Keys.ToList();
        sortedDistances.Sort((a, b) => ManhDist(coord, a) - ManhDist(coord, b));

        var i = sortedDistances.Count - 1;
        while(m_Chunks.Count > k_MaxChunks) {
            var farthest = sortedDistances[i];
            var toRemove = m_Chunks[farthest];

            // do remove stuff here
            toRemove.gameObject.SetActive(false);
            // TODO: maybe the pool should also be size limited
            m_ChunkPool.Enqueue(toRemove);
            m_Chunks.Remove(farthest);

            i--;
        }
    }

    // render the heightmap for a particular terrain chunk & offset
    void RenderChunk(Vector2Int coord, Terrain terrain) {
        var td = terrain.terrainData;

        // set display name
        var x = coord.x;
        var y = coord.y;
        terrain.name = $"Chunk ({IntoString(x)}, {IntoString(y)})";

        // the heightmap res is a power of 2 + 1, so we scale the offset magically
        // TODO: understand magic
        var scale = ((float)td.heightmapResolution - 1.0f) / td.heightmapResolution;

        // render height material into chunk heightmap
        m_TerrainHeight.SetVector(
            "_Offset",
            new Vector3(coord.x, coord.y) * scale
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

        // move the terrain into position
        var tt = terrain.transform;
        tt.position = IntoPosition(coord);
    }

    /// dequeue a terrain chunk from the pool
    Terrain DequeueChunk() {
        // reuse an existing terrain if available
        if (m_ChunkPool.Count != 0) {
            var chunk = m_ChunkPool.Dequeue();
            chunk.gameObject.SetActive(true);
            return chunk;
        }

        // otherwise, create a new terrain
        var obj = Instantiate(m_Terrain, transform);
        var tt = obj.GetComponent<Terrain>();
        var tc = obj.GetComponent<TerrainCollider>();

        // and terrain data
        var td = Instantiate(tt.terrainData);
        td.name = "ChunkData";
        tt.terrainData = td;
        tc.terrainData = td;

        return tt;
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

    // format the coordinate component
    string IntoString(int component) {
        if (component < 0) {
            return component.ToString();
        }

        return $"+{component}";
    }
}
