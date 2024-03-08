using System;
using Soil;
using UnityEngine;
using Yarn.Unity;

namespace Discone.Ui {

/// the mechanic's eyelid dialogue view
sealed partial class MechanicDialogueView: DialogueViewBase {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the spacing between lines")]
    [SerializeField] float m_Spacing;

    // -- props --
    // the mechanic's line fields
    Ring<MechanicLine> m_Lines;

    // -- lifecycle --
    void Awake() {
        m_Lines = new Ring<MechanicLine>(GetComponentsInChildren<MechanicLine>());
    }

    // -- commands --
    /// hide any visible lines
    public void Hide() {
        for (var i = 0; i < MaxVisibleLines; i++) {
            var line = m_Lines[-i];
            line.Hide();
        }
    }

    // -- queries --
    int MaxVisibleLines {
        get => m_Lines.Length - 1;
    }

    // -- DialogueViewBase --
    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        var max = m_Lines.Length - 2;

        // advance to the next line
        m_Lines.Move(-1);

        // show the new line
        var nextLine = m_Lines[0];
        nextLine.Show(dialogueLine.Text.Text);

        // offset all the lines
        if (max > 1) {
            var offset = Vector2.zero;
            for (var i = 0; i < max + 1; i++) {
                var line = m_Lines[i];
                line.Move(offset);
                offset.y += line.Height + m_Spacing;
            }
        }

        // hide the old line
        var prevLine = m_Lines[max];
        prevLine.Hide();

        onDialogueLineFinished?.Invoke();
    }
}

}