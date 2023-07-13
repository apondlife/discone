using System;
using UnityEngine;

namespace ThirdPerson {

/// a normalized curve with a min & max value
[Serializable]
public struct AdsrCurve {
    // -- fields --
    [Tooltip("the sustained value after attack & decay")]
    [SerializeField] float m_Sustain;

    [Tooltip("the attack curve")]
    [SerializeField] DurationCurve m_Attack;

    [Tooltip("the decay curve after attack curve towards sustain")]
    [SerializeField] DurationCurve m_Decay;

    [Tooltip("the decay curve towards sustain value")]
    [SerializeField] DurationCurve m_Release;

    // -- queries --
    /// evaluate the curve in the range
    public float Evaluate(
        float startTime,
        float amplitudeScale = 1f,
        float releaseStartTime = float.MaxValue
    ) {
        // elapsed in attack/decay/sustain
        var elapsed = Math.Min(releaseStartTime, Time.time) - startTime;

        // by default, sustain
        var scale = 1.0f;

        // if in attack, use the attack scale
        if (elapsed < m_Attack.Duration) {
            scale = m_Attack.Evaluate(elapsed);
        }
        // if
        else if (elapsed < m_Attack.Duration + m_Decay.Duration) {
            scale = Mathf.Lerp(
                m_Attack.Max,
                1.0f,
                m_Decay.Evaluate(elapsed - m_Attack.Duration)
            );
        }

        // calculate release scale from final pre-release scale
        var releaseElapsed = Mathf.Max(Time.time - releaseStartTime, 0.0f);
        scale = Mathf.Lerp(
            scale,
            0.0f,
            m_Release.Evaluate(releaseElapsed)
        );

        // scale the amplitude of the curve (around 1) by a parameter
        scale = 1 + (scale - 1) * amplitudeScale;

        return m_Sustain * scale;
    }
}

}