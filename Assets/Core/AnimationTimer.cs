using System;
using UnityEngine;

namespace Discone {

[Serializable]
record AnimationTimer {
    // -- constants --
    // the sentinel time for an inacitve timer
    const float k_Inactive = -1.0f;

    // -- cfg --
    [Tooltip("the timer duration")]
    [SerializeField] float m_Duration;

    [Tooltip("the timer curve")]
    [SerializeField] AnimationCurve m_Curve;

    // -- props --
    /// when the timer started
    float m_StartTime = k_Inactive;

    /// the uncurved percent through the timer
    float m_RawPct;

    // -- commands --
    /// start the timer
    public void Start() {
        m_StartTime = Time.time;
    }

    /// advance the timer based on current time
    public void Tick() {
        // if not active, abort
        if (m_StartTime == k_Inactive) {
            return;
        }

        // check progress
        var k = (Time.time - m_StartTime) / m_Duration;

        // if complete, clamp and stop the timer
        if (k >= 1.0f) {
            k = 1.0f;
            m_StartTime = k_Inactive;
        }

        // save current progress
        m_RawPct = k;
    }

    // -- queries --
    /// if the timer is active
    public bool IsActive {
        get => m_StartTime != k_Inactive;
    }

    /// the curved progress
    public float Pct {
        get => PctFrom(m_RawPct);
    }

    /// curve an arbitrary progress pct
    public float PctFrom(float value) {
        return m_Curve.Evaluate(value);
    }

    /// the uncurved progress
    public float Raw {
        get => m_RawPct;
    }
}

}