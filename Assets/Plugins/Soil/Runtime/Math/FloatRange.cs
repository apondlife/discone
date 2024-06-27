using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Soil {

/// a float value range
[MovedFrom(true, "ThirdPerson", "ThirdPerson", "FloatRange")]
[Serializable]
public struct FloatRange {
    // -- fields --
    [Tooltip("the min value")]
    public float Min;

    [Tooltip("the max value")]
    public float Max;

    // -- lifetime --
    public FloatRange(float min, float max) {
        Min = min;
        Max = max;
    }

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

    /// clamp the value between min & max
    public float Clamp(float val) {
        return Mathf.Clamp(val, Min, Max);
    }

    /// clamp the magnitude of the value between min & max, preserving sign
    public float ClampMagnitude(float val) {
        return Mathf.Sign(val) * Clamp(Mathf.Abs(val));
    }

    /// the length of the range
    public float Length {
        get => Mathf.Abs(Max - Min);
    }

    /// checks if a value is within the range
    public bool Contains(float value) {
        return (value >= Min && value < Max)
            || (value >= Max && value < Min);
    }

    // -- operators --
    public static FloatRange operator *(FloatRange range, float scale) {
        return new FloatRange(range.Min * scale, range.Max * scale);
    }

    public static FloatRange operator *(float scale, FloatRange range) {
        return range * scale;
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

    /// the lower bound
    public float Lower {
        get => Min;
        set => Min = value;
    }

    /// the upper bound
    public float Upper {
        get => Max;
        set => Max = value;
    }

    // -- debug --
    public override string ToString() {
        return $"[{Min}...{Max}]";
    }
}

}