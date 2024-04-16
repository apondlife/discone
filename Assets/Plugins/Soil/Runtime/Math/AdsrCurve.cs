using System;
using UnityEngine;

namespace Soil {

/// a normalized curve with a min & max value
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "AdsrCurve")]
[Serializable]
public struct AdsrCurve {
    // -- fields --
    [Tooltip("the sustained value after decay")]
    [SerializeField] float m_Sustain;

    [Tooltip("the hold duration between attack & decay")]
    [SerializeField] float m_HoldDuration;

    [Tooltip("the scale for the attack & decay")]
    [SerializeField] float m_MaxScale;

    [Tooltip("the attack curve")]
    [SerializeField] DurationCurve m_Attack;

    [Tooltip("the decay curve after attack curve towards sustain")]
    [SerializeField] DurationCurve m_Decay;

    [Tooltip("the decay curve towards sustain value")]
    [SerializeField] DurationCurve m_Release;

    // -- queries --
    // TODO: this needs to accept time deltas to avoid becoming unreliable
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
        var maxScale = m_MaxScale * amplitudeScale;

        // if in attack, use the attack scale
        if (elapsed < m_Attack.Duration) {
            scale = m_Attack.Evaluate(elapsed) * maxScale;
        }
        // if in hold, use max until reaching decay
        else if (elapsed < m_Attack.Duration + m_HoldDuration) {
            scale = maxScale;
        }
        // if in decay, decay down to sustain
        else if (elapsed < m_Attack.Duration + m_HoldDuration + m_Decay.Duration) {
            scale = Mathf.Lerp(
                maxScale,
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

        return m_Sustain * scale;
    }
}

}