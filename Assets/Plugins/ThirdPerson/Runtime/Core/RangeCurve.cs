using System;
using UnityEngine;

namespace ThirdPerson {

/// a normalized curve with a min & max value
[Serializable]
public struct RangeCurve {
    // -- fields --
    [Tooltip("the curve")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("the min value")]
    [SerializeField] float m_Min;

    [Tooltip("the max value")]
    [SerializeField] float m_Max;

    // -- queries --
    /// evaluate the curve in the range
    public float Evaluate(float input) {
        var k = input;
        if (m_Curve != null && m_Curve.length != 0) {
            k = m_Curve.Evaluate(input);
        }

        return Mathf.Lerp(m_Min, m_Max, k);
    }

    // -- debug --
    public override string ToString() {
        return $"<RangeCurve min={m_Min} max={m_Max}>";
    }
}

}