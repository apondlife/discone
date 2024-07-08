using System;
using UnityEngine;

namespace Soil {

/// a timer that curves its progress
[Serializable]
public record EaseTimer {
    // -- constants --
    // the sentinel time for an inactive timer
    const float k_Inactive = -1.0f;

    // -- cfg --
    [Tooltip("the ease config")]
    [SerializeField] Config m_Config;

    #pragma warning disable CS0414
    [Tooltip("the config source")]
    [SerializeField] ConfigSource m_ConfigSource;
    #pragma warning restore CS0414

    // -- props --
    /// the time elapsed through the timer
    float m_Elapsed;

    /// the uncurved percent through the timer
    float m_RawPct;

    /// if the timer is running in reverse
    bool m_IsReversed;

    // -- lifetime --
    /// create a timer with an empty config
    public EaseTimer() : this(new Config()) {}

    /// create a timer from a config
    public EaseTimer(Config config) {
        m_Config = config;
        m_Elapsed = k_Inactive;
    }

    // -- commands --
    /// start the timer (optionally, at a particular raw percent)
    public void Start(float pct = 0.0f, bool isReversed = false) {
        m_RawPct = pct;
        m_Elapsed = pct * m_Config.Duration;
        m_IsReversed = isReversed;
    }

    /// cancel the timer
    public void Cancel() {
        m_Elapsed = k_Inactive;
    }

    /// advance the timer based on current time
    public void Tick() {
        // if not active, abort
        if (!IsActive) {
            return;
        }

        var delta = Time.deltaTime;

        // check progress
        // TODO: do unscaled time?
        // TODO: do negative time?
        m_Elapsed += delta;
        var k = m_Elapsed / m_Config.Duration;

        // if complete, clamp and stop the timer
        if (k >= 1.0f) {
            k = 1.0f;
            m_Elapsed = k_Inactive;
        }

        // save current progress
        m_RawPct = m_IsReversed ? 1f - k : k;
    }

    /// try to tick this timer forward if it's active
    public bool TryTick() {
        if (!IsActive) {
            return false;
        }

        Tick();
        return true;
    }

    /// try to complete this timer if it's active
    public bool TryComplete() {
        if (!IsActive) {
            return false;
        }

        Tick();
        return IsComplete;
    }

    /// reconfigure the timer
    public void Configure(Config config) {
        Configure(config.Duration, config.Curve);
    }

    /// reconfigure the timer
    public void Configure(float duration, AnimationCurve curve) {
        m_Config.Duration = duration;
        m_Config.Curve = curve;
    }

    // -- queries --
    /// the curved progress
    public float Pct {
        get => PctFrom(m_RawPct);
    }

    /// curve an arbitrary progress pct
    public float PctFrom(float value) {
        return Mathx.Evaluate(m_Config.Curve, value);
    }

    /// the uncurved progress
    public float Raw {
        get => m_RawPct;
    }

    /// the elapsed time
    public float Elapsed {
        get => m_Elapsed;
    }

    /// the total duration
    public float Duration {
        get => m_Config.Duration;
        set => m_Config.Duration = value;
    }

    /// if the timer is zero-duration
    public bool IsZero {
        get => m_Config.Duration == 0f;
    }

    /// if the timer is active
    public bool IsActive {
        get => m_Elapsed != k_Inactive;
    }

    /// if the timer is inactive
    public bool IsInactive {
        get => !IsActive;
    }

    /// if the timer is running in reverse
    public bool IsReversed {
        get => m_IsReversed;
    }

    /// if the timer is complete
    public bool IsComplete {
        get => m_RawPct == (m_IsReversed ? 0f : 1f);
    }

    // -- config --
    /// the source of the ease timer config
    public enum ConfigSource {
        Local,
        External
    }

    /// the config for an ease timer
    [Serializable]
    public struct Config {
        // -- cfg --
        [Units("s")]
        [Tooltip("the timer duration")]
        public float Duration;

        [Tooltip("the timer curve")]
        public AnimationCurve Curve;

        // -- lifetime --
        public Config(float duration = 0f, AnimationCurve curve = null) {
            Duration = duration;
            Curve = curve;
        }
    }
}

}