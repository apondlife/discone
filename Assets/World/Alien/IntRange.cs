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

    // -- debug --
    public override string ToString() {
        return $"[${Min}...{Max}]";
    }
}

}