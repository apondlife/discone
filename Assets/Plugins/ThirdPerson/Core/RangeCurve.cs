using System;
using UnityEngine;

namespace ThirdPerson {

/// a normalized curve with a min & max value
[Serializable]
public struct RangeCurve {
    // -- fields --
    [Tooltip("the curve")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("the src (t=0) value")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Min")]
    [SerializeField] float m_Src;

    [Tooltip("the dst (t=1) value")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Max")]
    [SerializeField] float m_Dst;

    // -- queries --
    /// evaluate the curve in the range
    public float Evaluate(float k) {
        return Mathf.Lerp(m_Curve.Evaluate(k), m_Src, m_Dst);
    }
}

}