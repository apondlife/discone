using System;
using UnityEngine;

namespace Soil {

/// a normalized curve with a min & max value
[UnityEngine.Scripting.APIUpdating.MovedFrom(true, "ThirdPerson", "ThirdPerson", "AdsrCurve")]
[Serializable]
public struct AdsrCurve {
    /// a value when the curve is not released
    public const float NotReleased = float.MaxValue;

    // -- fields --
    [Tooltip("the initial value")]
    [SerializeField] float m_InitialValue;

    [Tooltip("the time it takes for the curve to start")]
    [SerializeField] float m_Delay;

    [Tooltip("the scale for the attack & decay")]
    [SerializeField] float m_AttackValue;

    [Tooltip("the attack curve")]
    [SerializeField] DurationCurve m_Attack;

    [Tooltip("the hold duration between attack & decay")]
    [SerializeField] float m_HoldDuration;

    [Tooltip("the decay curve after attack curve towards sustain")]
    [SerializeField] DurationCurve m_Decay;

    [Tooltip("the sustained value after decay")]
    [SerializeField] float m_SustainValue;

    [Tooltip("the release curve towards zero")]
    [SerializeField] DurationCurve m_Release;

    // -- queries --
    /// evaluate the curve in the range
    public float Evaluate(
        float elapsed,
        float releaseAt = NotReleased
    ) {
        var value = m_SustainValue;

        // the amount of time since release
        var releaseElapsed = Mathf.Max(elapsed - releaseAt, 0f);

        // calculate progress through the adsr pre-release
        elapsed -= releaseElapsed;

        // delay
        if (elapsed > 0f && elapsed <= m_Delay) {
            value = m_InitialValue;
        }

        // move past delay
        elapsed -= m_Delay;

        // if we're in attack, attack towards the attack value
        if (elapsed > 0f && elapsed <= m_Attack.Duration) {
            value = Mathf.Lerp(
                m_InitialValue,
                m_AttackValue,
                m_Attack.Evaluate(elapsed)
            );
        }

        // move past attack
        elapsed -= m_Attack.Duration;

         // if we're in hold, hold the attack value
        if (elapsed > 0f && elapsed <= m_HoldDuration) {
            value = m_AttackValue;
        }

        // move past hold
        elapsed -= m_HoldDuration;

        // if we're in decay, decay towards the sustain value
        if (elapsed > 0f && elapsed <= m_Decay.Duration) {
            value = Mathf.Lerp(
                m_AttackValue,
                m_SustainValue,
                m_Decay.Evaluate(elapsed)
            );
        }

        // release from the pre-release value towards 0
        if (releaseElapsed > 0f) {
            value = Mathf.Lerp(
                value,
                0.0f,
                m_Release.Evaluate(releaseElapsed)
            );
        }

        return value;
    }
}

}