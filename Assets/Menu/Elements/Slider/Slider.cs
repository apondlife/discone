using UnityEngine;
using TMPro;

namespace Discone.Ui {

class Slider: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the value label")]
    [SerializeField] TMP_Text m_ValueLabel;

    // -- events --
    /// when the value changes
    public void OnValueChanged(float value) {
        m_ValueLabel.text = ((int)value).ToString();
    }
}

}