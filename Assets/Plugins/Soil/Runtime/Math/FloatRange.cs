using System;
using UnityEngine;

namespace Soil {

/// a float value range
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "FloatRange")]
[Serializable]
public struct FloatRange {
    // -- fields --
    [Tooltip("the min value")]
    public float Min;

    [Tooltip("the max value")]
    public float Max;

    // -- queries --
    /// interpolate between the min & max
    public float Evaluate(float k) {
        return Lerp(k);
    }

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
    public float Src {
        get => Min;
        set => Min = value;
    }

    /// the destination value
    public float Dst {
        get => Max;
        set => Max = value;
    }

    // -- debug --
    public override string ToString() {
        return $"[{Min}...{Max}]";
    }
}

}