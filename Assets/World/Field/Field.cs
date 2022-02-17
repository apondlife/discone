using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityAtoms.BaseAtoms;

/// an infinite field
[ExecuteAlways]
sealed class Field: MonoBehaviour {
    // -- constants --
    /// the maximum number of active chunks
    private const int k_MaxChunks = 16;

    /// the duration between purges
    private const float k_PurgeChunksInterval = 2.0f;

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
    Vector2Int m_TargetCoord = new Vector2Int(69, 420);

    /// the map of visible chunks
    Dictionary<Vector2Int, Terrain> m_Chunks = new Dictionary<Vector2Int, Terrain>();

    /// a pool of free terrain instances
    Queue<Terrain> m_ChunkPool = new Queue<Terrain>();

    // -- p/cache
    /// the size of a chunk
    float m_ChunkSize;

    // -- lifecycle --
    void Start() {
        // in editor, draw entire field
        if (!Application.IsPlaying(gameObject)) {
            CreateEditorChunks();
            return;
        }

        // ensure terrain is square
        var td = m_Terrain.GetComponent<Terrain>().terrainData;
        Debug.Assert(td.size.x == td.size.z, "field's terrain chunk was not square");

        // capture chunk size
        m_ChunkSize = td.size.x;

        // start purge routine
        StartCoroutine(Coroutines.Interval(k_PurgeChunksInterval, PurgeChunks));
    }

    void Update() {
        // in editor, do nothing
        if (!Application.IsPlaying(gameObject)) {
            return;
        }

        var tt = m_TargetObject.Value.transform;

        // if the target changed chunks, create neighbors
        var coord = IntoCoordinate(tt.position);
        if (m_TargetCoord != coord) {
            m_TargetCoord = coord;
            CreateChunks();
        }
    }

    // -- commands --
    // -- c/create
    /// create new chunks as the player moves
    void CreateChunks(int size = 3) {
        // the number of chunks to create
        var n = size * size;

        // instantiate a square of 9 chunks around the target
        for (var i = 0; i < n; i++) {
            var c = m_TargetCoord;
            c.x += i % size - 1;
            c.y += i / size - 1;

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

        // reassign neighbors
        chunk.SetNeighbors(
            m_Chunks.Get(coord + Vector2Int.left),
            m_Chunks.Get(coord + Vector2Int.up),
            m_Chunks.Get(coord + Vector2Int.right),
            m_Chunks.Get(coord + Vector2Int.down)
        );
    }

    // render the heightmap for a particular terrain chunk & offset
    void RenderChunk(Vector2Int coord, Terrain terrain) {
        var td = terrain.terrainData;

        // set display name
        var x = coord.x;
        var y = coord.y;
        terrain.name = $"Chunk ({IntoString(x)}, {IntoString(y)})";

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

    // -- c/purge
    /// purge any unused chunks
    void PurgeChunks() {
        if (m_Chunks.Count <= k_MaxChunks) {
            return;
        }

        // remove the chunks that are further away and no longer needed
        var t = m_TargetCoord;
        var sortedDistances = m_Chunks.Keys.ToList();
        sortedDistances.Sort((a, b) => Vec2.Manhattan(t, a) - Vec2.Manhattan(t, b));

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

    // -- c/editor
    /// create chunks for the editor field
    void CreateEditorChunks() {
        // set target coordinate
        // var tc = Camera.current.transform;
        m_TargetCoord = Vector2Int.zero;

        // create chunks
        CreateChunks(3);
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
