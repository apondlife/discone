using System;
using UnityEngine;

namespace Soil {

/// a normalized curve with a duration
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "DurationCurve")]
[Serializable]
public struct DurationCurve {
    // -- fields --
    [Tooltip("the curve")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("the duration value")]
    [SerializeField] float m_Duration;

    // -- queries --
    /// evaluate the curve in the range
    public float Evaluate(float elapsed) {
        var k = elapsed / m_Duration;

        if (m_Curve != null && m_Curve.length != 0) {
            k = m_Curve.Evaluate(k);
        }

        return k;
    }

    /// the curve duration
    public float Duration {
        get => m_Duration;
    }

    /// the max value
    public float Max {
        get => m_Curve.Evaluate(1.0f);
    }
}

}