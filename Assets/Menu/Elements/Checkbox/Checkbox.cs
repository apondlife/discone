using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Discone.Ui {

[ExecuteAlways]
public class Checkbox: MonoBehaviour {
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
    void Awake() {
        // set props
        m_Toggle = GetComponent<Toggle>();

        RenderText();
    }

    void Start() {
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