using System;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

/// the world chunks
[ExecuteAlways]
public sealed class WorldChunks: MonoBehaviour {
    // -- constants --
    /// the size of the square of chunks to spawn around a coord
    const int k_SpawnSize = 3;

    // -- config --
    [Header("config")]
    [Tooltip("the time in s before a chunk is unloaded")]
    [SerializeField] float m_UnloadDelay;

    // -- events --
    [Header("events")]
    [Tooltip("when any player enters a new chunk")]
    [SerializeField] Vector2IntEvent m_EnteredChunk;

    [Tooltip("when every player exits a chunk")]
    [SerializeField] Vector2IntEvent m_ExitedChunk;

    [Tooltip("when a chunk's deferred unload finishes")]
    [SerializeField] Vector2IntEvent m_UnloadedChunk;

    // -- refs --
    [Header("refs")]
    [Tooltip("the entity singleton")]
    [SerializeField] EntitiesVariable m_Entities;

    [Tooltip("the size of a world chunk")]
    [SerializeField] FloatReference m_ChunkSize;

    // -- props --
    /// the number of players in each chunk
    Dictionary<Vector2Int, int> m_Chunks = new Dictionary<Vector2Int, int>();

    /// actions for any chunks with an in-progress unload
    Dictionary<Vector2Int, Action> m_Unloading = new Dictionary<Vector2Int, Action>();

    // -- lifecycle --
    #if UNITY_EDITOR
    void Start() {
        // destroy any editor terrain
        ClearEditorChunks();

        // if editor, don't do anything else
        if (!Application.IsPlaying(gameObject)) {
            return;
        }
    }
    #endif

    #if UNITY_EDITOR
    void Update() {
        // if editor, create editor chunks
        if (!Application.IsPlaying(gameObject)) {
            CreateEditorChunks();
            return;
        }
    }
    #endif

    void FixedUpdate() {
        var players = m_Entities.Value.Players
            .FindCullers();

        foreach (var player in players) {
            TrackChunksForPlayer(player);
        }
    }

    // -- commands --
    /// create new chunks as a player moves
    void TrackChunksForPlayer(OnlinePlayer player) {
        // if the target is active
        var pt = player.transform;

        // get their coord
        var coord = player.Coord;

        // if the target changed chunks, create neighbors
        var next = coord.FromPosition(pt.position);
        var curr = coord.Value;

        if (curr != next) {
            // update coord
            coord.Value = next;

            // first enter new chunks, spawning any new ones
            TrackChunks(next, enter: true);

            // then leave old chunks, despawning any empty ones
            TrackChunks(curr, enter: false);
        }
    }

    /// create new chunks as a player moves
    void TrackChunks(Vector2Int center, bool enter) {
        // ignore the none coord
        if (center == WorldCoord.None) {
            return;
        }

        // the number of chunks to create
        var n = k_SpawnSize * k_SpawnSize;

        // track a square of n chunks around the target
        for (var i = 0; i < n; i++) {
            var c = center;
            c.x += i % k_SpawnSize - 1;
            c.y += i / k_SpawnSize - 1;

            // enter of leave the chunk
            if (enter) {
                EnterChunk(c);
            } else {
                LeaveChunk(c);
            }
        }
    }

    /// enter this chunk
    void EnterChunk(Vector2Int coord) {
        // get the current count, if any
        var count = 0;
        m_Chunks.TryGetValue(coord, out count);

        // and increment it
        count += 1;
        m_Chunks[coord] = count;

        // if this is a new chunk, fire the event
        if (count == 1) {
            Debug.Log($"[chunks] entered {coord}");
            m_EnteredChunk.Raise(coord);
        }
    }

    /// leave the chunk
    void LeaveChunk(Vector2Int coord) {
        // get the current count, if any
        var count = 1;
        m_Chunks.TryGetValue(coord, out count);

        // and decrement it
        count -= 1;
        m_Chunks[coord] = count;

        // if this is an empty chunk, fire the event
        if (count == 0) {
            Debug.Log($"[chunks] exited {coord}");
            m_ExitedChunk.Raise(coord);

            // grab the debounced unload fn or create one
            Action unload;
            if (!m_Unloading.TryGetValue(coord, out unload)) {
                unload = Actions.Debounce(m_UnloadDelay, () => {
                    // if its still empty after the delay, unload it
                    if (m_Chunks[coord] == 0) {
                        Debug.Log($"[chunks] unloaded {coord}");
                        m_UnloadedChunk.Raise(coord);
                    }

                    // and destroy this fn
                    m_Unloading.Remove(coord);
                });
            }

            // and invoke it
            unload.Invoke();
        }
    }

    // -- queries --
    /// the chunk size
    public float ChunkSize {
        get => m_ChunkSize;
    }

    /// if the chunk is active
    public bool IsChunkActive(Vector2Int coord) {
        var count = 0;

        if (!m_Chunks.TryGetValue(coord, out count)) {
            return false;
        }

        return count > 0;
    }

    // -- editor --
    #if UNITY_EDITOR
    /// the editor's world coordinate
    Vector2Int m_EditorCoord = WorldCoord.None;

    /// create chunks for the editor field
    void CreateEditorChunks() {
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
        var coord = WorldCoord.FromPosition(intersection, m_ChunkSize);
        if (m_EditorCoord != coord) {
            m_EditorCoord = coord;
            TrackChunks(coord, enter: true);
        }
    }

    /// clear all editor chunks
    public void ClearEditorChunks() {
        // reset state
        m_Chunks.Clear();
        m_Unloading.Clear();

        // reset target coord to sentinel
        m_EditorCoord = WorldCoord.None;
    }
    #endif
}
