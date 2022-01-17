using UnityEngine;

/// static extensions for Vector3
public static class Vec3 {
    /// create a Vector3 with components (0, 0, y)
    public static Vector3 XYN(this Vector3 v) {
        return new Vector3(v.x, v.y, 0.0f);
    }

    /// create a Vector3 with components (0, 0, y)
    public static Vector3 XNZ(this Vector3 v) {
        return new Vector3(v.x, 0.0f, v.z);
    }

    /// create a Vector3 with components (0, 0, y)
    public static Vector3 NYN(this Vector3 v) {
        return new Vector3(0.0f, 0.0f, v.z);
    }

    /// create a Vector3 with components (0, 0, y)
    public static Vector3 NNZ(this Vector3 v) {
        return new Vector3(0.0f, 0.0f, v.z);
    }
}

/// static extensions for Vector2
public static class Vec2 {
    /// normalize the vector
    public static Vector2 Normalize(Vector2 v) {
        v.Normalize();
        return v;
    }

    /// create a Vector3 with components (x, 0, y)
    public static Vector3 XZ(this Vector2 v) {
        return v.XNY();
    }

    /// create a Vector3 with components (0, 0, y)
    public static Vector3 XNY(this Vector2 v) {
        return new Vector3(v.x, 0.0f, v.y);
    }

    /// create a Vector3 with components (x, 0, 0)
    public static Vector3 XNN(this Vector2 v) {
        return new Vector3(v.x, 0.0f, v.y);
    }

    /// create a Vector3 with components (0, 0, y)
    public static Vector3 NNY(this Vector2 v) {
        return new Vector3(0, 0.0f, v.y);
    }
}