using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityAtoms.BaseAtoms;

[RequireComponent(typeof(Slider))]
public class FloatVariableSlider : MonoBehaviour
{
    [SerializeField] private FloatVariable m_Variable;

    private Slider m_Slider;
    // Start is called before the first frame update
    void Start()
    {
        m_Slider = GetComponent<Slider>();
        m_Slider.onValueChanged.AddListener(s => m_Variable.SetValue(s));
        m_Variable.Changed.Register(v => m_Slider.SetValueWithoutNotify(v));
    }
}