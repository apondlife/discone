using Soil;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

namespace Discone {

[ExecuteAlways]
sealed class DemoRecordingView: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("blinking period")]
    [SerializeField] float m_Period;

    [Tooltip("minimum blinking alpha")]
    [SerializeField] float m_MinAlpha;

    // -- refs --
    [Header("refs")]
    [Tooltip("if we are currently recording")]
    [SerializeField] BoolReference m_IsRecording;

    [Tooltip("the recording light")]
    [SerializeField] Image m_Light;

    void Update() {
        var color = m_Light.color;
        color.a = 0f;
        if(m_IsRecording) {
            color.a = Mathf.Lerp(
                m_MinAlpha,
                1f,
                Mathf.Sin(Time.time * m_Period * Mathx.TAU)
            );
        }

        m_Light.color = color;

    }
}

}