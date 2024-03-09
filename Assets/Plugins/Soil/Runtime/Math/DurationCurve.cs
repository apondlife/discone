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
        return m_Curve.Evaluate(elapsed / m_Duration);
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