using System;
using UnityEngine;

namespace Soil {

/// a normalized curve with source and destination ranges
[Serializable]
public struct MapCurve: FloatTransform {
    // -- fields --
    [Tooltip("the curve")]
    public AnimationCurve Curve;

    [Tooltip("the source range")]
    public FloatRange Src;

    [Tooltip("the destination range")]
    public FloatRange Dst;

    // -- FloatTransform --
    public float Evaluate(float input) {
        return Evaluate(Src, Curve, Dst, input);
    }

    // -- queries --
    /// evaluate the curve in the src range
    public static float Evaluate(
        FloatRange src,
        AnimationCurve curve,
        float input
    ) {
        return Mathx.Evaluate(curve, src.InverseLerp(input));
    }

    /// evaluate the curve in the dst range
    public static float Evaluate(
        AnimationCurve curve,
        FloatRange dst,
        float input
    ) {
        return dst.Lerp(Mathx.Evaluate(curve, input));
    }

    /// evaluate the curve in the src & dst range
    public static float Evaluate(
        FloatRange src,
        AnimationCurve curve,
        FloatRange dst,
        float input
    ) {
        return dst.Lerp(Evaluate(src, curve, input));
    }

    // -- debug --
    public override string ToString() {
        return $"<MapCurve src={Src} dst={Dst}>";
    }
}

}