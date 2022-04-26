using UnityEngine;
using UnityEngine.UI;
using UnityAtoms.BaseAtoms;

[RequireComponent(typeof(Slider))]
class FloatVariableSlider: MonoBehaviour {
    [SerializeField] private FloatVariable m_Variable;

    // -- props --
    /// the slider
    private Slider m_Slider;

    // -- lifecycle --
    void Start() {
        // set props
        m_Slider = GetComponent<Slider>();

        // configure the slider's initial state
        m_Slider.SetValueWithoutNotify(m_Variable.Value);

        // bind events
        m_Slider.onValueChanged.AddListener(s => m_Variable.SetValue(s));
        m_Variable.Changed.Register(v => m_Slider.SetValueWithoutNotify(v));
    }
}