using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Discone.Ui {

sealed class Slider: UIBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the current value")]
    [SerializeField] FloatVariable m_Value;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the value range")]
    [SerializeField] ThirdPerson.RangeCurve m_Range;

    // -- refs --
    [Header("refs")]
    [Tooltip("the inner slider")]
    [SerializeField] UnityEngine.UI.Slider m_Control;

    [Tooltip("the value label")]
    [SerializeField] TMP_Text m_Label;

    // -- props --
    /// a dispose bag
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    protected override void Start() {
        base.Start();

        // set initial state
        m_Control.value = m_Value.Value;

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
    /// render the value
    void Render() {
        var value = m_Value.Value;

        // update control
        m_Control.value = value;

        // update label
        var val = m_Range.Evaluate(value);
        var str = ((int)val).ToString();
        m_Label.text = str;
    }

    // -- events --
    /// when the value changes
    void OnValueChanged(float _) {
        Render();
    }

    /// when the value changes
    void OnControlChanged(float value) {
        m_Value.Value = value;
    }
}

}