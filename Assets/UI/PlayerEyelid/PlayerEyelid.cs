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

    [Tooltip("the curve for the animation")]
    [SerializeField] AnimationCurve m_Curve;

    [Tooltip("if the eyelids start closed")]
    [SerializeField] bool m_IsClosedOnStart;

    // -- refs --
    [Header("refs")]
    [Tooltip("the image for the top eyelid")]
    [SerializeField] Image m_TopEyelid;

    [Tooltip("the image for the bottom eyelid")]
    [SerializeField] Image m_BottomEyelid;

    [Tooltip("the butterfly emitter")]
    [SerializeField] ParticleSystem m_Butterflies;

    [Tooltip("the player's current number of butterflies")]
    [SerializeField] IntReference m_ButterflyCount;

    // -- props --
    /// the elapsed time in the close animation
    float m_ClosingElapsed;

    /// the subscriptions
    readonly DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        m_Subscriptions
            .Add(m_IsClosed.Changed, OnIsClosedChanged);
    }

    protected override void Start() {
        base.Start();

        if (m_IsClosedOnStart) {
            UpdateElapsed(m_Duration);
            UpdateVisibility();
        }
    }

    void Update() {
        var delta = Time.deltaTime;

        // open/close the eyes
        if (m_IsClosing.Value) {
            UpdateElapsed(delta);
        } else if (m_ClosingElapsed > 0.0f) {
            UpdateElapsed(-delta);
        }

        UpdateVisibility();
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// add the delta to the elapsed time, clamped to its range
    void UpdateElapsed(float delta) {
        // get next elapsed duration
        var curr = Mathf.Clamp(
            m_ClosingElapsed + delta,
            0.0f,
            m_Duration
        );

        m_ClosingElapsed = curr;

        // if the eyes just closed or just opened, fire the event
        m_IsClosed.Value = curr == m_Duration;
    }

    /// update eyelid visibility
    void UpdateVisibility() {
        var pct = m_Curve.Evaluate(Mathf.InverseLerp(
            0.0f,
            m_Duration,
            m_ClosingElapsed
        ));

        m_TopEyelid.fillAmount = pct;
        m_BottomEyelid.fillAmount = pct;
    }

    // -- events --
    /// when the closed state changes
    void OnIsClosedChanged(bool isClosed) {
        if (!isClosed) {
            m_Butterflies.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return;
        }

        // show the number of collected butterflies
        var main = m_Butterflies.main;
        main.maxParticles = m_ButterflyCount;

        // start the system
        m_Butterflies.Play();
    }
}

}