using System;
using TMPro;
using UnityEngine;
using Yarn.Unity;

namespace Discone {

/// the mechanic's eyelid dialogue
sealed class MechanicDialogueView: DialogueViewBase {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the delay before running a line")]
    [SerializeField] ThirdPerson.EaseTimer m_Fade;

    // -- refs --
    [Header("refs")]
    [Tooltip("the dialogue cavnas group")]
    [SerializeField] CanvasGroup m_Group;

    [Tooltip("the mechanic's voice, materialized")]
    [SerializeField] TMP_Text[] m_Voice;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Voice = GetComponentsInChildren<TMP_Text>();
    }

    void Start() {
        m_Group.alpha = 0f;
    }

    void FixedUpdate() {
        if (m_Fade.IsActive) {
            m_Fade.Tick();
            m_Group.alpha = m_Fade.Pct;
        }
    }

    // -- DialogueViewBase --
    public override void DialogueStarted() {
        base.DialogueStarted();

        m_Fade.Start();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        base.RunLine(dialogueLine, onDialogueLineFinished);

        m_Voice.text = dialogueLine.Text.Text;
    }

    public override void DialogueComplete() {
        base.DialogueComplete();

        m_Fade.Start(isReversed: true);
    }
}

}