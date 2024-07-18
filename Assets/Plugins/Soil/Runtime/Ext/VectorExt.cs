using UnityEngine;

namespace Soil {

/// static extensions for vector types
public static class VectorX {
    // -- queries --
    /// the dot product between two vectors in a plane (not normalized)
    public static float DotOnPlane(Vector3 lhs, Vector3 rhs, Vector3 up) {
        return Vector3.Dot(
            Vector3.ProjectOnPlane(lhs, up),
            Vector3.ProjectOnPlane(rhs, up)
        );
    }

    /// normalize the vector
    public static Vector2 Normalize(Vector2 v) {
        v.Normalize();
        return v;
    }

    /// the manhattan distance between two int vectors
    public static int Manhattan(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// create a Vector3 with components (x, 0, 0)
    public static Vector3 XNN(this Vector2 v) {
        return new Vector3(v.x, 0f, v.y);
    }

    /// create a Vector3 with components (0, 0, y)
    public static Vector3 XNY(this Vector2 v) {
        return new Vector3(v.x, 0f, v.y);
    }

    /// create a Vector3 with components (x, 0, y)
    public static Vector3 XNZ(this Vector3 v) {
        return new Vector3(v.x, 0f, v.z);
    }

    /// create a Vector3 with components (0, 0, y)
    public static Vector3 NNY(this Vector2 v) {
        return new Vector3(0f, 0f, v.y);
    }

    /// create a Vector3 with components (0, 0, y)
    public static Vector3 NNZ(this Vector3 v) {
        return new Vector3(0f, 0f, v.z);
    }

    /// create a Vector3 with components (0, y, 0)
    public static Vector3 NYN(this Vector3 v) {
        return new Vector3(0f, v.y, 0f);
    }

    /// create a Vector3 with components (x, y, 0)
    public static Vector3 XYN(this Vector3 v) {
        return new Vector3(v.x, v.y, 0f);
    }

    /// create a Vector3 with components (y, x, 0)
    public static Vector3 YXN(this Vector2 v) {
        return new Vector3(v.y, v.x, 0f);
    }
}

}