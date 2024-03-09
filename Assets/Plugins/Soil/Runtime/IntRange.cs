using System;
using UnityEngine;

namespace Soil {

/// a float value range
[Serializable]
public struct IntRange {
    // -- fields --
    [Tooltip("the min value")]
    public int Min;

    [Tooltip("the max value")]
    public int Max;

    // -- lifetime --
    public IntRange(int min, int max) {
        Min = min;
        Max = max;
    }

    // -- queries --
    /// interpolate between the min & max
    public float Lerp(float k) {
        return Mathf.LerpUnclamped(Min, Max, k);
    }

    /// normalize the value between min & max
    public float InverseLerp(float val) {
        return Mathf.InverseLerp(Min, Max, val);
    }

    // -- aliases --
    /// the source value
    public int Src {
        get => Min;
        set => Min = value;
    }

    /// the destination value
    public int Dst {
        get => Max;
        set => Max = value;
    }

    // -- debug --
    public override string ToString() {
        return $"[{Min}...{Max}]";
    }
}

}