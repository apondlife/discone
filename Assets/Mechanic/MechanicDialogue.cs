using System;
using UnityEngine;
using Yarn.Unity;

namespace Discone.Ui {

/// the mechanic's eyelid dialogue view
sealed class MechanicDialogue: DialogueViewBase {
    // -- refs --
    [Header("refs")]
    [Tooltip("the mechanic's visible lines")]
    [SerializeField] MechanicLine[] m_Lines;

    // -- props --
    /// the index of the current line label
    int m_LineIndex;

    // -- DialogueViewBase --
    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        base.RunLine(dialogueLine, onDialogueLineFinished);

        var curr = m_LineIndex;
        var next = (m_LineIndex + 1) % m_Lines.Length;

        var currLine = m_Lines[curr];
        var nextLine = m_Lines[next];

        currLine.Hide();
        nextLine.Show(dialogueLine.Text.Text);

        m_LineIndex = next;
    }

    public override void DialogueComplete() {
        base.DialogueComplete();

        var currLine = m_Lines[m_LineIndex];
        currLine.Hide();
    }
}

}