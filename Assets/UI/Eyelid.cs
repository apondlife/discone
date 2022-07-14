using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

/// the checkpoint's eyelid animation
public class Eyelid : MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the image for the top eyelid")]
    [SerializeField] private Image m_TopEyelid;

    [Tooltip("the image for the bottom eyelid")]
    [SerializeField] private Image m_BottomEyelid;

    [Tooltip("the image for the bottom eyelid")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_EyelidCloseCurve")]
    [SerializeField] private AnimationCurve m_Curve;

    [Tooltip("the percent at which the eyelid is closed during checkpoint save")]
    [SerializeField] float m_CheckpointEyelidCloseDuration;

    // -- atoms --
    [Header("atoms")]
    [Tooltip("the progress of the current checkpoint save")]
    [SerializeField] private FloatReference m_CheckpointSaveElapsed;

    // -- props --
    private float m_ClosingElapsed;

    // -
    private void Update() {
        // the save is loading
        if (m_CheckpointSaveElapsed > 0.0f) {
            m_ClosingElapsed = Mathf.Min(m_CheckpointSaveElapsed, m_CheckpointEyelidCloseDuration);
        }
        // if the checkpoint ended or was cancelled
        else {
            m_ClosingElapsed -= Time.deltaTime;
        }

        // normalize and curve the elapsed time
        var pct = m_Curve.Evaluate(Mathf.InverseLerp(
            0.0f,
            m_CheckpointEyelidCloseDuration,
            m_ClosingElapsed
        ));

        m_TopEyelid.fillAmount = pct;
        m_BottomEyelid.fillAmount = pct;
    }


}
