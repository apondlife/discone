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
        var k = input;
        if (m_Curve != null && m_Curve.length != 0) {
            k = m_Curve.Evaluate(input);
        }

        return m_Dst.Lerp(k);
    }

    // -- debug --
    public override string ToString() {
        return $"<MapOutCurve dst={m_Dst}>";
    }
}

}