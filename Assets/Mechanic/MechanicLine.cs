using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Discone.Ui {

/// a line label for the mechanic dialogue view
sealed class MechanicLine: UIBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the delay before running a line")]
    [SerializeField] ThirdPerson.EaseTimer m_Fade;

    // -- refs --
    [Header("refs")]
    [Tooltip("the ui component")]
    [SerializeField] Component m_Component;

    [Tooltip("the text label")]
    [SerializeField] TMP_Text m_Text;

    // -- lifecycle --
    void FixedUpdate() {
        if (m_Fade.IsActive) {
            m_Fade.Tick();
            m_Component.Show(m_Fade.Pct, enter: !m_Fade.isReversed);
        }
    }

    // -- commands --
    /// show a line of dialogue
    public void Show(string text) {
        m_Text.text = text;
        m_Fade.Start();
    }

    /// hide the dialogue line
    public void Hide() {
        m_Fade.Start(isReversed: true);
    }
}

}