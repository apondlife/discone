using ThirdPerson;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Discone.Ui {

/// the player's eyelid animation
public class PlayerEyelid: UIBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the percent through the eye close animation")]
    [SerializeField] FloatVariable m_ClosePct;

    [Tooltip("an event when the eyelid just closes or starts to open")]
    [SerializeField] BoolVariable m_IsClosed;

    // -- config --
    [Header("config")]
    // TODO: this is an ease timer?
    [Tooltip("the duration of the animation")]
    [SerializeField] float m_Duration;

    [Tooltip("the curve for the animation")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("the small pixel overlap to fully close")]
    [SerializeField] float m_Overlap;

    [Tooltip("the debounce to before stopping the particle system")]
    [SerializeField] EaseTimer m_HideDelay;

    [Tooltip("if the eyelids start closed")]
    [SerializeField] bool m_IsClosedOnStart;

    // -- refs --
    [Header("refs")]
    [Tooltip("if the player is closing their eyes")]
    [SerializeField] BoolVariable m_IsClosing;

    [Tooltip("the butterfly emitter")]
    [SerializeField] ParticleSystem m_Butterflies;

    [Tooltip("the player's current number of butterflies")]
    [SerializeField] IntReference m_ButterflyCount;

    // -- props --
    /// the elapsed time in the close animation
    float m_Elapsed;

    // -- lifecycle --
    protected override void Start() {
        base.Start();

        if (m_IsClosedOnStart) {
            UpdateElapsed(m_Duration);
        }
    }

    void Update() {
        var delta = Time.deltaTime;

        // open/close the eyes
        if (m_IsClosing.Value) {
            UpdateElapsed(delta);
        } else if (m_Elapsed > 0.0f) {
            UpdateElapsed(-delta);
        }

        // hide the butterflies after a delay
        if (m_HideDelay.IsActive) {
            m_HideDelay.Tick();

            if (m_HideDelay.IsComplete) {
                HideButterflies();
            }
        }
    }

    // -- commands --
    /// add the delta to the elapsed time, clamped to its range
    void UpdateElapsed(float delta) {
        // get next elapsed duration
        var elapsed = Mathf.Clamp(
            m_Elapsed + delta,
            0.0f,
            m_Duration
        );

        m_Elapsed = elapsed;

        // synchronize external state
        var pct = Mathf.InverseLerp(
            0.0f,
            m_Duration,
            m_Elapsed
        );

        m_ClosePct.Value = pct;
        m_IsClosed.Value = pct == 1f;

        // if our eyes are at all closed
        if (pct > 0f) {
            // keep debouncing the hide
            m_HideDelay.Start();

            // and show the butterflies, if necessary
            if (!m_Butterflies.isPlaying) {
                ShowButterflies();
            }
        }
    }

    /// show the currently collected butterflies
    void ShowButterflies() {
        // show the number of collected butterflies
        var main = m_Butterflies.main;
        main.maxParticles = m_ButterflyCount;

        // start the system
        m_Butterflies.Play();
        m_Butterflies.Emit(m_ButterflyCount);
    }

    // hid the currently visible butterflies
    void HideButterflies() {
        m_Butterflies.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}

}