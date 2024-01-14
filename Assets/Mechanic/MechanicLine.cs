using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Serialization;

namespace Discone.Ui {

/// a line label for the mechanic dialogue view
sealed class MechanicLine: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the fade-in animation")]
    [SerializeField] ThirdPerson.EaseTimer m_Fade;

    [Tooltip("the offset animation")]
    [SerializeField] ThirdPerson.EaseTimer m_Offset;

    [FormerlySerializedAs("m_OffsetRange")]
    [Tooltip("the character offset range")]
    [SerializeField] ThirdPerson.MapOutCurve m_OffsetDist;

    // -- refs --
    [Header("refs")]
    [Tooltip("the ui component")]
    [SerializeField] Transitionable m_Component;

    [Tooltip("the text label")]
    [SerializeField] TMP_Text m_Text;

    // -- props --
    /// the current character offsets
    ThirdPerson.Buffer<Vector2> m_Offsets = new(256);

    // -- lifecycle --
    protected override void Start() {
        base.Start();

        // set the initial component values
        m_Component.OnBeforeEnter();

        // start the label at 0 alpha
        m_Component.Show(0f, enter: true);
    }

    protected override void OnEnable() {
        base.OnEnable();
        m_Text.OnPreRenderText += OnPreRenderText;
    }

    void Update() {
        // transition the label in / out
        if (m_Fade.IsActive) {
            m_Fade.Tick();
            m_Component.Show(m_Fade.Pct);
        }

        if (m_Offset.IsActive) {
            m_Offset.Tick();
            m_Text.ForceMeshUpdate();
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

        // rebuild the initial text offsets
        m_Offsets.Clear();
        for (var i = 0; i < text.Length; i++) {
            var len = m_OffsetDist.Evaluate(Random.value);
            var dir = Random.insideUnitCircle.normalized;
            m_Offsets.Add(len * dir);
        }

        // start the animations
        m_Fade.Start();
        m_Offset.Start();
    }

    /// hide the dialogue line
    public void Hide() {
        // ignore if already hidden
        if (!m_Fade.IsActive && m_Fade.Raw == 0) {
            return;
        }

        m_Fade.Start(isReversed: true);
    }

    // -- events --
    void OnPreRenderText(TMP_TextInfo info) {
        var n = info.characterCount;

        for (var i = 0; i < n; i++) {
            var charInfo = info.characterInfo[i];

            var meshIdx = charInfo.materialReferenceIndex;
            var meshInfo = info.meshInfo[meshIdx];

            var vertIdx = charInfo.vertexIndex;
            var vertices = meshInfo.vertices;

            var offset = (Vector3)m_Offsets[i] * (1f - m_Offset.Pct);
            vertices[vertIdx + 0] += offset;
            vertices[vertIdx + 1] += offset;
            vertices[vertIdx + 2] += offset;
            vertices[vertIdx + 3] += offset;

            // var color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
            // info.meshInfo[meshIdx].colors32[vertIdx + 0] = color;
            // info.meshInfo[meshIdx].colors32[vertIdx + 1] = color;
            // info.meshInfo[meshIdx].colors32[vertIdx + 2] = color;
            // info.meshInfo[meshIdx].colors32[vertIdx + 3] = color;
        }
    }
}

}