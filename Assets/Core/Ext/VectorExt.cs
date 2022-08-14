using UnityEngine;

/// extensions to vector types
public static class Vec2 {
    /// calculate the manhattan distance between two int vectors
    public static int Manhattan(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// create a Vector3 with components (x, 0, y)
    public static Vector3 XNZ(this Vector3 v) {
        return new Vector3(v.x, 0.0f, v.z);
    }
}