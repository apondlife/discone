using System.Collections.Generic;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

/// an infinite field
[ExecuteAlways]
public sealed class Field: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the size of a world chunk")]
    [SerializeField] FloatVariable m_ChunkSize;

    // -- config --
    [Header("config")]
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

    // -- events --
    [Header("events")]
    [Tooltip("when a chunk loads")]
    [SerializeField] Vector2IntEvent m_LoadedChunk;

    [Tooltip("when a chunk unloads")]
    [SerializeField] Vector2IntEvent m_UnloadedChunk;

    // -- props --
    /// the map of visible chunks
    Dictionary<Vector2Int, FieldChunk> m_Chunks = new Dictionary<Vector2Int, FieldChunk>();

    /// a pool of free chunk instances
    Queue<FieldChunk> m_ChunkPool = new Queue<FieldChunk>();

    /// the set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Init() {
        #if UNITY_EDITOR
        // destroy any editor terrain
        ClearEditorChunks();
        #endif

        // capture chunk size
        Debug.Assert(m_Chunk.Size.x == m_Chunk.Size.z, "field's terrain chunk was not square");
        m_ChunkSize.Value = m_Chunk.Size.x;

        // dont persist changes to the editor
        // m_FieldHeight = m_FieldHeight.Unsaved();

        // bind events
        m_Subscriptions
            .Add(m_LoadedChunk, CreateChunk)
            .Add(m_UnloadedChunk, DestroyChunk);
    }

    void Awake() {
        Init();
    }

    #if UNITY_EDITOR
    void Start() {
        // if editor, don't do anything else
        if (!Application.IsPlaying(gameObject)) {
            Init();
        }
    }
    #endif

    void OnValidate () {
#if UNITY_EDITOR
        m_FieldHeight.SetFloat("_FloorScale", m_FloorScale);
        m_FieldHeight.SetFloat("_MinFloor", m_MinFloor);
        m_FieldHeight.SetFloat("_MaxFloor", m_MaxFloor);
        m_FieldHeight.SetFloat("_ElevationScale", m_ElevationScale);
        m_FieldHeight.SetFloat("_MinElevation", m_MinElevation);
        m_FieldHeight.SetFloat("_MaxElevation", m_MaxElevation);

        ReloadEditorChunks();
#endif
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// create a new chunk at a coordinate if necessary
    void CreateChunk(Vector2Int coord) {
        // if the chunk already exists, don't create one
        if (m_Chunks.ContainsKey(coord)) {
            return;
        }

        // add the chunk to the map of active chunks
        var chunk = DequeueChunk();
        m_Chunks[coord] = chunk;

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
        tc.localPosition = WorldCoord.IntoPosition(coord, m_ChunkSize.Value);
    }

    /// destroy the chunk at this coordinate if necessary
    void DestroyChunk(Vector2Int coord) {
        // if it doesn't exist, do nothing
        if (!m_Chunks.ContainsKey(coord)) {
            return;
        }

        // turn off the chunk
        var chunk = m_Chunks[coord];
        chunk.gameObject.SetActive(false);
        m_Chunks.Remove(coord);

        // and add it back to the pool
        // TODO: maybe the pool should also be size limited
        m_ChunkPool.Enqueue(chunk);
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

    // -- c/editor
    #if UNITY_EDITOR
    /// reload chunks with new params
    void ReloadEditorChunks() {
        foreach (var (_, chunk) in m_Chunks) {
            chunk.Reload();
        }
    }

    /// clear all editor chunks
    public void ClearEditorChunks() {
        m_Chunks.Clear();
        m_ChunkPool.Clear();

        // destroy any editor terrain
        var t = transform;
        while (t.childCount > 0) {
            DestroyImmediate(t.GetChild(0).gameObject);
        }
    }
    #endif
}
