using System;
using UnityEngine;
using Yarn.Unity;

namespace Discone.Ui {

/// the mechanic's eyelid dialogue view
sealed class MechanicDialogueView: DialogueViewBase {
    // -- refs --
    [Header("refs")]
    [Tooltip("the mechanic's visible lines")]
    [SerializeField] MechanicLine[] m_Lines;

    // -- props --
    /// the index of the current line label
    int m_LineIndex;

    // -- commands --
    /// hide any visible lines
    public void Hide() {
        var currLine = m_Lines[m_LineIndex];
        currLine.Hide();
    }

    // -- DialogueViewBase --
    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        Debug.Log(Tag.Mechanic.F($"run line {dialogueLine.RawText}"));

        var curr = m_LineIndex;
        var next = (m_LineIndex + 1) % m_Lines.Length;

        var currLine = m_Lines[curr];
        var nextLine = m_Lines[next];

        currLine.Hide();
        nextLine.Show(dialogueLine.Text.Text);

        m_LineIndex = next;

        onDialogueLineFinished();
    }
}

}