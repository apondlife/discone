using UnityAtoms.BaseAtoms;
using UnityEngine;

/// a stateful coordinate in the world; updated externally
public sealed class WorldCoord: MonoBehaviour {
    // -- constants --
    /// an undefined coord
    public static readonly Vector2Int None = new Vector2Int(69, 420);

    // -- state --
    [Header("state")]
    [Tooltip("the current world coordinate")]
    [SerializeField] Vector2Int m_Value = None;

    // -- refs --
    [Header("refs")]
    [Tooltip("the world chunk size")]
    [SerializeField] FloatReference m_ChunkSize;

    // -- props/hot --
    /// the current world coordinate
    public Vector2Int Value {
        get => m_Value;
        set => m_Value = value;
    }

    // -- queries --
    /// finds the coordinate for this position
    public Vector2Int FromPosition(Vector3 pos) {
        return FromPosition(pos, m_ChunkSize);
    }

    /// finds the coordinate for this position given a chunk size
    public static Vector2Int FromPosition(Vector3 pos, float chunkSize) {
        var cs = chunkSize;
        var ch = cs * 0.5f;

        // get x and y coord for this position
        var x = Mathf.FloorToInt((pos.x + ch) / cs);
        var y = Mathf.FloorToInt((pos.z + ch) / cs);

        return new Vector2Int(x, y);
    }

    /// finds the position for this coordinate given a chunk size
    public static Vector3 IntoPosition(Vector2Int coord, float chunkSize) {
        var cs = chunkSize;
        var ch = cs * 0.5f;

        // get x and z position for this coordinate
        var x = coord.x * cs - ch;
        var z = coord.y * cs - ch;

        return new Vector3(x, 0.0f, z);
    }
}