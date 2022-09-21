using UnityEngine;

/// mathf "static" methods
static class Mathx {
    /// remap a value from one range to another
    public static float Remap(
        float min0, float max0,
        float min1, float max1,
        float value
    ) {
        return Mathf.Lerp(min1, max1, Mathf.InverseLerp(min0, max0, value));
    }
}