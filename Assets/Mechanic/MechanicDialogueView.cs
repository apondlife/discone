using System;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using Yarn.Unity;

namespace Discone {

/// the mechanic's eyelid dialogue
class MechanicDialogueView: DialogueViewBase {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the delay before running a line")]
    [SerializeField] ThirdPerson.EaseTimer m_Fade;

    // -- refs --
    [Header("refs")]
    [Tooltip("the dialogue cavnas group")]
    [SerializeField] CanvasGroup m_Group;

    [Tooltip("the mechanic's voice, materialized")]
    [SerializeField] TMP_Text m_Voice;

    // -- lifecycle --
    void Update() {
        if (m_Fade.IsActive) {
            m_Fade.Tick();
            m_Group.alpha = m_Fade.Pct;
        }
    }

    // -- DialogueViewBase --
    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        base.RunLine(dialogueLine, onDialogueLineFinished);

        m_Fade.Start();

        // show the line
        var line = dialogueLine.Text.Text;
        Debug.Log($"[mechnic] show `{line}`");
        m_Voice.text = line;
    }
}

}