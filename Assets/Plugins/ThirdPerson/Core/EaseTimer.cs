using System;
using UnityEngine;

namespace ThirdPerson {

[Serializable]
public record EaseTimer {
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
    float m_StartTime;

    /// the uncurved percent through the timer
    float m_RawPct;

    // -- lifetime --
    public EaseTimer(): this(0.0f) {}
    public EaseTimer(
        float duration,
        AnimationCurve curve = null
    ) {
        m_StartTime = k_Inactive;
        m_Duration = duration;
        m_Curve = curve;
    }

    // -- commands --
    /// start the timer (optionally, at a particular raw percent)
    public void Start(float pct = 0.0f) {
        m_StartTime = Time.time + pct * m_Duration;
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
        if (m_Curve == null || m_Curve.length == 0) {
             return value;
        }

        return m_Curve.Evaluate(value);
    }

    /// the uncurved progress
    public float Raw {
        get => m_RawPct;
    }
}

}