using System;
using UnityEngine;

namespace Soil {

/// a float value range
[Serializable]
public struct Vector2Range {
    // -- fields --
    [Tooltip("the min value")]
    public Vector2 Min;

    [Tooltip("the max value")]
    public Vector2 Max;

    // -- queries --
    /// interpolate between the min & max
    public Vector2 Lerp(float k) {
        return Vector2.LerpUnclamped(Min, Max, k);
    }

    // -- aliases --
    /// the source value
    public Vector2 Src {
        get => Min;
        set => Min = value;
    }

    /// the destination value
    public Vector2 Dst {
        get => Max;
        set => Max = value;
    }

    // -- debug --
    public override string ToString() {
        return $"[{Min}...{Max}]";
    }
}

}