using Soil;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Serialization;

namespace Discone.Ui {

/// a line label for the mechanic dialogue view
sealed class MechanicLine: UIBehaviour {
    // -- tuning --
    [Header("tuning")]
    [FormerlySerializedAs("m_Rotation")]
    [Tooltip("the initial angle range")]
    [SerializeField] MapOutCurve m_Angle;

    [Tooltip("the fade-in/out animation")]
    [SerializeField] EaseTimer m_Fade;

    [Tooltip("the move animation")]
    [SerializeField] EaseTimer m_Move;

    [Tooltip("the angle range for moving lines")]
    [SerializeField] MapOutCurve m_Move_Angle;

    [FormerlySerializedAs("m_Offset")]
    [Tooltip("the scatter animation")]
    [SerializeField] EaseTimer m_Scatter;

    [FormerlySerializedAs("m_ScatterDist")]
    [FormerlySerializedAs("m_OffsetDist")]
    [FormerlySerializedAs("m_OffsetRange")]
    [Tooltip("the character offset range")]
    [SerializeField] MapOutCurve m_Scatter_Dist;

    // -- refs --
    [Header("refs")]
    [Tooltip("the text label")]
    [SerializeField] TMP_Text m_Text;

    [Tooltip("the canvas group")]
    [SerializeField] CanvasGroup m_Group;

    // -- props --
    /// the rect transform
    RectTransform m_Rect;

    /// the fade alpha range
    FloatRange m_Fade_Alpha;

    /// the move pos range
    Vector2Range m_Move_Pos;

    /// the move rotation range
    FloatRange m_Move_Rotation;

    /// the per-character scatter offsets
    readonly Buffer<Vector2> m_Scatter_Offsets = new(256);

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // set props
        m_Rect = (RectTransform)transform;
    }

    protected override void OnEnable() {
        base.OnEnable();
        m_Text.OnPreRenderText += OnPreRenderText;
    }

    void Update() {
        // transition the label in / out
        if (m_Fade.TryTick()) {
            m_Group.alpha = m_Fade_Alpha.Lerp(m_Fade.Pct);
        }

        // scatter the text in
        if (m_Scatter.TryTick()) {
            m_Text.ForceMeshUpdate();
        }

        // offset the label into a new position
        if (m_Move.TryTick()) {
            m_Rect.anchoredPosition = m_Move_Pos.Lerp(m_Move.Pct);
            m_Text.transform.rotation = Quaternion.AngleAxis(
                m_Move_Rotation.Lerp(m_Move.Pct),
                Vector3.forward
            );
        }
    }

    protected override void OnDisable() {
        m_Text.OnPreRenderText -= OnPreRenderText;
        base.OnDisable();
    }

    // -- commands --
    /// show a line of dialogue
    public void Show(string text) {
        // update the text
        m_Text.text = text;

        // reset visual state
        m_Group.alpha = 0f;
        m_Rect.anchoredPosition = Vector3.zero;

        // set initial rotation
        m_Text.transform.rotation = Quaternion.AngleAxis(
            m_Angle.Evaluate(Random.value),
            Vector3.forward
        );

        // start animations
        Fade(alpha: 1f);
        Scatter();
    }

    /// hide the line
    public void Hide() {
        if (!IsHidden) {
            Fade(0f);
        }
    }

    /// fade to an alpha
    public void Fade(float alpha) {
        m_Fade_Alpha.Src = m_Group.alpha;
        m_Fade_Alpha.Dst = alpha;
        m_Fade.Start();
    }

    /// move to an offset from center
    public void Move(Vector2 offset) {
        var rect = (RectTransform)transform;
        m_Move_Pos.Src = rect.anchoredPosition;
        m_Move_Pos.Dst = offset;

        var angle = m_Text.transform.eulerAngles.z;
        m_Move_Rotation.Src = angle <= 180f ? angle : angle - 360f;
        m_Move_Rotation.Dst = m_Move_Angle.Evaluate(Random.value);

        m_Move.Start();
    }

    /// scatter the text in
    void Scatter() {
        var n = m_Text.text.Length;

        // rebuild the initial text offsets
        m_Scatter_Offsets.Clear();
        for (var i = 0; i < n; i++) {
            var len = m_Scatter_Dist.Evaluate(Random.value);
            var dir = Random.insideUnitCircle.normalized;
            m_Scatter_Offsets.Add(len * dir);
        }

        // start animations
        m_Scatter.Start();
    }

    // -- queries --
    /// the height of the text
    public float Height {
        get => m_Text.preferredHeight;
    }

    /// the time in seconds to appear
    public float EnterDuration {
        get => m_Scatter.Duration;
    }

    /// if the line is currently hidden
    public bool IsHidden {
        get => m_Group.alpha == 0f;
    }

    // -- events --
    /// when the text is about to be draw
    void OnPreRenderText(TMP_TextInfo info) {
        var n = info.characterCount;

        for (var i = 0; i < n; i++) {
            var charInfo = info.characterInfo[i];

            var meshIdx = charInfo.materialReferenceIndex;
            var meshInfo = info.meshInfo[meshIdx];

            var vertIdx = charInfo.vertexIndex;
            var vertices = meshInfo.vertices;

            var offset = (Vector3)m_Scatter_Offsets[i] * (1f - m_Scatter.Pct);
            vertices[vertIdx + 0] += offset;
            vertices[vertIdx + 1] += offset;
            vertices[vertIdx + 2] += offset;
            vertices[vertIdx + 3] += offset;
        }
    }
}

}