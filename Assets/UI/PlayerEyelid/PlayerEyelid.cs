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
    [Tooltip("an event when the player is closing their eyes")]
    [SerializeField] BoolVariable m_IsClosing;

    [Tooltip("an event when the eyelid just closes or starts to open")]
    [SerializeField] BoolVariable m_IsClosed;

    // -- config --
    [Header("config")]
    [Tooltip("the duration of the animation")]
    [SerializeField] float m_Duration;

    [Tooltip("the small pixel overlap to fully close")]
    [SerializeField] float m_Overlap;

    [Tooltip("the debounce to before stopping the particle system")]
    [SerializeField] EaseTimer m_HideDelay;

    [Tooltip("the curve for the animation")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("if the eyelids start closed")]
    [SerializeField] bool m_IsClosedOnStart;

    // -- refs --
    [Header("refs")]
    [Tooltip("the image for the top eyelid")]
    [SerializeField] RectMask2D m_Top;

    [Tooltip("the image for the bottom eyelid")]
    [SerializeField] RectMask2D m_Bottom;

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

        // if the eyes just closed or just opened, fire the event
        var nextClosed = elapsed == m_Duration;
        m_IsClosed.Value = nextClosed;

        // if our eyes are at all closed
        if (elapsed > 0f) {
            // keep debouncing the hide
            m_HideDelay.Start();

            // and show the butterflies, if necessary
            if (!m_Butterflies.isPlaying) {
                ShowButterflies();
            }
        }

        // update the eyelid visibility
        var pct = m_Curve.Evaluate(Mathf.InverseLerp(
            0.0f,
            m_Duration,
            m_Elapsed
        ));

        var height = m_Top.rectTransform.rect.height;
        var offset = Mathf.Lerp(height, height * 0.5f - m_Overlap, pct);
        m_Top.padding = new Vector4(0f, offset, 0f, 0f);
        m_Bottom.padding = new Vector4(0f, 0f, 0f, offset);
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