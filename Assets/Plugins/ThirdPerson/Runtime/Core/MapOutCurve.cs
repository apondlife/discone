using System;
using UnityEngine;

namespace ThirdPerson {

/// a normalized curve with a min & max value
[Serializable]
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "RangeCurve")]
public struct MapOutCurve {
    // -- fields --
    [Tooltip("the curve")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("the destination range")]
    [SerializeField] FloatRange m_Dst;

    // -- queries --
    /// evaluate the curve in the range
    public float Evaluate(float input) {
        return Evaluate(m_Curve, m_Dst, input);
    }

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
        return $"<MapOutCurve dst={m_Dst}>";
    }
}

}