using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityAtoms.BaseAtoms;

/// an infinite field
[ExecuteAlways]
public sealed class Field: MonoBehaviour {
    // -- constants --
    /// the maximum number of active chunks
    private const int k_MaxChunks = 16;

    /// the duration between purges
    private const float k_PurgeChunksInterval = 2.0f;

    // -- fields --
    [Header("config")]
    [Tooltip("the target to follow")]
    [SerializeField] GameObjectReference m_TargetObject;

    [Header("references")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Terrain")]
    [Tooltip("the prefab for creating chunks")]
    [SerializeField] FieldChunk m_Chunk;

    [Tooltip("the field height material")]
    [SerializeField] Material m_FieldHeight;

    [Tooltip("the scale for the floor noise")]
    [SerializeField] float m_FloorScale = 0.3f;

    [Tooltip("the minimum height of the floor")]
    [SerializeField] float m_MinFloor = 100.0f;

    [Tooltip("the maximum height of the floor")]
    [SerializeField] float m_MaxFloor = 600.0f;

    [Tooltip("the scale for the elevation noise")]
    [SerializeField] float m_ElevationScale = 0.97f;

    [Tooltip("the minimum elevation amount")]
    [SerializeField] float m_MinElevation = 0.0f;

    [Tooltip("the maximum elevation amount")]
    [SerializeField] float m_MaxElevation = 20.0f;

    // -- props --
    /// the target's current coordinate. the current center chunk index
    Vector2Int m_TargetCoord = new Vector2Int(69, 420);

    /// the map of visible chunks
    Dictionary<Vector2Int, FieldChunk> m_Chunks = new Dictionary<Vector2Int, FieldChunk>();

    /// a pool of free chunk instances
    Queue<FieldChunk> m_ChunkPool = new Queue<FieldChunk>();

    // -- p/cache
    /// the size of a chunk
    float m_ChunkSize;

    // -- lifecycle --
    void Awake() {
        // dont persist changes to the editor
        // m_FieldHeight = m_FieldHeight.Unsaved();
    }

    void Start() {
        // capture chunk size
        Debug.Assert(m_Chunk.Size.x == m_Chunk.Size.z, "field's terrain chunk was not square");
        m_ChunkSize = m_Chunk.Size.x;

        #if UNITY_EDITOR
        // destroy any editor terrain
        ClearEditorChunks();

        // if editor, don't do anything else
        if (!Application.IsPlaying(gameObject)) {
            return;
        }
        #endif

        // start purge routine
        StartCoroutine(Coroutines.Interval(k_PurgeChunksInterval, PurgeChunks));
    }

    void Update() {
        #if UNITY_EDITOR
        // if editor, create editor chunks
        if (!Application.IsPlaying(gameObject)) {
            CreateEditorChunks();
            return;
        }
        #endif

        // if the target is active
        if (!m_TargetObject.Value) {
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

    void OnValidate () {
        m_FieldHeight.SetFloat("_FloorScale", m_FloorScale);
        m_FieldHeight.SetFloat("_MinFloor", m_MinFloor);
        m_FieldHeight.SetFloat("_MaxFloor", m_MaxFloor);
        m_FieldHeight.SetFloat("_ElevationScale", m_ElevationScale);
        m_FieldHeight.SetFloat("_MinElevation", m_MinElevation);
        m_FieldHeight.SetFloat("_MaxElevation", m_MaxElevation);

        ReloadEditorChunks();
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

        // load the chunk for this coordinate
        chunk.Load(coord);

        // reassign neighbors
        chunk.SetNeighbors(
            m_Chunks.Get(coord + Vector2Int.left),
            m_Chunks.Get(coord + Vector2Int.up),
            m_Chunks.Get(coord + Vector2Int.right),
            m_Chunks.Get(coord + Vector2Int.down)
        );

        // move the chunk into position
        var tc = chunk.transform;
        tc.localPosition = IntoPosition(coord);
    }

    /// dequeue a terrain chunk from the pool
    FieldChunk DequeueChunk() {
        var chunk = null as FieldChunk;

        // reuse an existing chunk if available
        if (m_ChunkPool.Count != 0) {
            chunk = m_ChunkPool.Dequeue();
            chunk.gameObject.SetActive(true);
        }
        // otherwise, create a new chunk
        else {
            chunk = Instantiate(m_Chunk, transform);
            chunk.gameObject.layer = gameObject.layer;
        }

        return chunk;
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
    void ReloadEditorChunks() {
        foreach (var (_, chunk) in m_Chunks) {
            chunk.Reload();
        }
    }

    /// create chunks for the editor field
    void CreateEditorChunks() {
        #if UNITY_EDITOR
        // get the editor camera
        var scene = UnityEditor.SceneView.lastActiveSceneView;
        if (scene == null) {
            return;
        }

        var camera = scene.camera;
        if (camera == null) {
            return;
        }

        // don't create chunks in prefab mode
        var preview = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (preview != null) {
            return;
        }

        // get the look position and direction
        var ct = camera.transform;
        var lp = ct.position;
        var ld = ct.forward;

        // the ground plane normal (position is always zero) (pretty unclear to me why
        // the normal is reversed)
        var pp = Vector3.zero;
        var pn = Vector3.up;

        // the magnitude of the the look pos to the plane & the look's scale (angle) in the plane
        var a = Vector3.Dot(pp - lp, pn);
        var b = Vector3.Dot(ld, pn);

        // get the intersection
        var intersection = Vector3.zero;
        if (Mathf.Abs(b) < 0.00001f) {
            if (Mathf.Abs(a) >= 0.00001f) {
                return;
            }

            intersection = lp;
        } else {
            intersection = lp + a / b * ld;
        }

        // set target coordinate
        var coord = IntoCoordinate(intersection);
        if (m_TargetCoord != coord) {
            m_TargetCoord = coord;
            CreateChunks(3);
        }
        #endif
    }

    /// clear all editor chunks
    public void ClearEditorChunks() {
        #if UNITY_EDITOR
        m_Chunks.Clear();

        // destroy any editor terrain
        var t = transform;
        while (t.childCount > 0) {
            DestroyImmediate(t.GetChild(0).gameObject);
        }

        // reset target coord to sentinel
        m_TargetCoord = new Vector2Int(69, 420);
        #endif
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
