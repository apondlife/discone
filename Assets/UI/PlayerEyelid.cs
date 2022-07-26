using UnityAtoms;
using UnityEngine;
using UnityEngine.UI;

/// the player's eyelid animation
public class PlayerEyelid: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the duration of the animation")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_CheckpointEyelidCloseDuration")]
    [SerializeField] float m_Duration;

    [Tooltip("the curve for the animation")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_EyelidCloseCurve")]
    [SerializeField] private AnimationCurve m_Curve;

    // -- refs --
    [Header("refs")]
    [Tooltip("the image for the top eyelid")]
    [SerializeField] private Image m_TopEyelid;

    [Tooltip("the image for the bottom eyelid")]
    [SerializeField] private Image m_BottomEyelid;

    [Tooltip("the current character")]
    [SerializeField] DisconePlayerVariable m_Player;

    // -- props --
    /// the elapsed time in the close animation
    private float m_ClosingElapsed;

    // -- lifecycle --
    void Update() {
        // update elapsed time
        if (m_Player.Value.Checkpoint.IsSaving) {
            UpdateElapsed(Time.deltaTime);
        } else if (m_ClosingElapsed > 0.0f) {
            UpdateElapsed(-Time.deltaTime);
        }

        // update eyelid visibility
        var pct = m_Curve.Evaluate(Mathf.InverseLerp(
            0.0f,
            m_Duration,
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
            m_Duration
        );
    }
}
