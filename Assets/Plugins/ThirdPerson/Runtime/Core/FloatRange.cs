using System;
using UnityEngine;

namespace ThirdPerson {

/// a float value range
[Serializable]
public struct FloatRange {
    // -- fields --
    [Tooltip("the min value")]
    public float Min;

    [Tooltip("the max value")]
    public float Max;

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