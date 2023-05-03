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
    protected override void Start() {
        base.Start();

        // set the initial component values
        m_Component.OnBeforeEnter();

        // start the label at 0 alpha
        m_Component.Show(0f, enter: true);
    }

    void Update() {
        // transition the label in / out
        if (m_Fade.IsActive) {
            m_Fade.Tick();
            m_Component.Show(m_Fade.Pct);
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