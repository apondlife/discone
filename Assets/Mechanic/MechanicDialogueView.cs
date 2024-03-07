using System;
using NaughtyAttributes;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;
using Random = UnityEngine.Random;

namespace Discone.Ui {

/// the mechanic's eyelid dialogue view
sealed class MechanicDialogueView: DialogueViewBase {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the spacing between lines")]
    [SerializeField] float m_Spacing;

    // -- props --
    // the mechanic's line fields
    ThirdPerson.Queue<MechanicLine> m_Lines;

    // -- lifecycle --
    void Awake() {
        m_Lines = new ThirdPerson.Queue<MechanicLine>(GetComponentsInChildren<MechanicLine>());
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

    [Button("Test")]
    void Test() {
        var line = new LocalizedLine();
        var text = new MarkupParseResult();
        text.Text = "";

        var n = Random.Range(2, 10);
        for (var i = 0; i < n; i++) {
            text.Text += "asdf ";
        }

        line.Text = text;
        RunLine(line, null);
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
        var offset = Vector2.zero;
        for (var i = 0; i < max + 1; i++) {
            var line = m_Lines[i];
            line.Move(offset);
            offset.y += line.Height + m_Spacing;
        }

        // hide the old line
        var prevLine = m_Lines[max];
        prevLine.Hide();

        onDialogueLineFinished?.Invoke();
    }
}

}