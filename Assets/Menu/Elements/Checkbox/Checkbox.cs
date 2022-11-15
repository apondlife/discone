using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Discone.Ui {

[ExecuteAlways]
public class Checkbox: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the value text when checked")]
    [SerializeField] string m_OnText;

    [Tooltip("the value text when not checked")]
    [SerializeField] string m_OffText;

    // -- refs --
    [Header("refs")]
    [Tooltip("the text label")]
    [SerializeField] TMP_Text m_Label;

    // -- props --
    /// the underlying toggle
    Toggle m_Toggle;

    // -- lifecycle --
    protected override void Awake() {
        base.Awake();

        // set props
        m_Toggle = GetComponent<Toggle>();

        // set initial state
        RenderText();
    }

    protected override void Start() {
        base.Start();

        // bind events
        m_Toggle.onValueChanged.AddListener(OnValueChanged);
    }

    // -- commands --
    /// render the text
    void RenderText() {
        m_Label.text = m_Toggle.isOn ? m_OnText : m_OffText;
    }

    // -- events --
    /// when the value changes
    void OnValueChanged(bool isOn) {
        RenderText();
    }
}

}