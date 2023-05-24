using System;
using UnityEngine;

namespace ThirdPerson {

/// a float value range
[Serializable]
struct FloatRange {
    // -- fields --
    [Tooltip("the min value")]
    [SerializeField] float Min;

    [Tooltip("the max value")]
    [SerializeField] float Max;

    // -- queries --
    /// interpolate between the min & max
    public float Lerp(float k) {
        return Mathf.Lerp(Min, Max, k);
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