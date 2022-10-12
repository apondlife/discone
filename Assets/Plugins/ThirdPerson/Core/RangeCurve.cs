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
    public float Evaluate(float k) {
        return Mathf.Lerp(m_Min, m_Max, m_Curve.Evaluate(k));
    }
}

}