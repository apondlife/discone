using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// the checkpoint's eyelid animation
public class Eyelid: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the image for the bottom eyelid")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_EyelidCloseCurve")]
    [SerializeField] private AnimationCurve m_Curve;

    [Tooltip("the percent at which the eyelid is closed during checkpoint save")]
    [SerializeField] float m_CheckpointEyelidCloseDuration;

    // -- refs --
    [Header("refs")]
    [Tooltip("the image for the top eyelid")]
    [SerializeField] private Image m_TopEyelid;

    [Tooltip("the image for the bottom eyelid")]
    [SerializeField] private Image m_BottomEyelid;

    [Tooltip("the save checkpoint input")]
    [SerializeField] InputActionReference m_CloseEyesAction;

    // -- props --
    /// the elapsed time in the close animation
    private float m_ClosingElapsed;

    // -- lifecycle --
    void Update() {
        // update elapsed time
        var close = m_CloseEyesAction.action;
        if (close.IsPressed()) {
            UpdateElapsed(Time.deltaTime);
        } else if (m_ClosingElapsed > 0.0f) {
            UpdateElapsed(-Time.deltaTime);
        }

        // update eyelid visibility
        var pct = m_Curve.Evaluate(Mathf.InverseLerp(
            0.0f,
            m_CheckpointEyelidCloseDuration,
            m_ClosingElapsed
        ));

        m_TopEyelid.fillAmount = pct;
        m_BottomEyelid.fillAmount = pct;
    }

    // -- helpers --
    /// add the delta to the elapsed time, clamped to its range
    void UpdateElapsed(float delta) {
        m_ClosingElapsed = Mathf.Clamp(
            m_ClosingElapsed + delta,
            0.0f,
            m_CheckpointEyelidCloseDuration
        );
    }
}
