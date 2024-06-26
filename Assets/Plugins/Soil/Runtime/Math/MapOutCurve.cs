using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Soil {

/// a normalized curve with a min & max value
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "MapOutCurve")]
[Serializable]
public struct MapOutCurve: FloatTransform {
    // -- fields --
    [FormerlySerializedAs("m_Curve")]
    [Tooltip("the curve")]
    public AnimationCurve Curve;

    [FormerlySerializedAs("m_Dst")]
    [Tooltip("the destination range")]
    public FloatRange Dst;

    // -- FloatTransform --
    public float Evaluate(float input) {
        return Evaluate(Curve, Dst, input);
    }

    // -- queries --
    /// evaluate the curve in the range
    public static float Evaluate(
        AnimationCurve curve,
        FloatRange range,
        float input
    ) {
        var k = input;
        if (curve != null && curve.length != 0) {
            k = curve.Evaluate(input);
        }

        return range.Lerp(k);
    }

    // -- debug --
    public override string ToString() {
        return $"<MapOutCurve dst={Dst}>";
    }
}

}