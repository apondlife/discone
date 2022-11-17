using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Discone.Ui {

[ExecuteAlways]
sealed class Checkbox: UIBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the current value")]
    [SerializeField] BoolVariable m_Value;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the value text when checked")]
    [SerializeField] string m_OnText;

    [Tooltip("the value text when not checked")]
    [SerializeField] string m_OffText;

    // -- refs --
    [Header("refs")]
    [Tooltip("the inner toggle")]
    [SerializeField] Toggle m_Control;

    [Tooltip("the text label")]
    [SerializeField] TMP_Text m_Label;

    // -- props --
    /// a dispose bag
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    protected override void Start() {
        base.Start();

        // set initial state
        m_Control.isOn = m_Value.Value;

        // render view
        Render();

        // bind events
        m_Subscriptions
            .Add(m_Value.Changed, OnValueChanged)
            .Add(m_Control.onValueChanged, OnControlChanged);
    }

    protected override void OnDestroy() {
        base.OnDestroy();

        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// render the text
    void Render() {
        var isOn = m_Value.Value;

        // update control
        m_Control.isOn = isOn;

        // update label
        m_Label.text = isOn ? m_OnText : m_OffText;
    }

    // -- events --
    /// when the value changes
    void OnValueChanged(bool _) {
        Render();
    }

    /// when the control value changes
    void OnControlChanged(bool isOn) {
        m_Value.Value = isOn;
    }
}

}