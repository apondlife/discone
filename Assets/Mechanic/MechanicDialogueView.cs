using System;
using System.Collections.Generic;
using Soil;
using ThirdPerson;
using UnityEngine;
using Yarn.Unity;
using Random = UnityEngine.Random;

namespace Discone.Ui {

/// the mechanic's eyelid dialogue view
sealed class MechanicDialogueView: DialogueViewBase {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the spacing between fading lines")]
    [SerializeField] float m_Spacing;

    [Tooltip("the exponential alpha base")]
    [SerializeField] float m_Alpha_Base;

    [Tooltip("the horizontal offset for fading lines")]
    [SerializeField] MapOutCurve m_Offset_Horizontal;

    // -- props --
    // the mechanic's line fields
    Ring<MechanicLine> m_Lines;

    // -- lifecycle --
    void Awake() {
        m_Lines = new Ring<MechanicLine>(GetComponentsInChildren<MechanicLine>());
    }

    // -- commands --
    /// hide any visible lines
    public void Hide(bool animated = true) {
        foreach (var line in m_Lines) {
            line.Hide(animated);
        }
    }

    /// push lines back in context
    public void Push() {
        PushLines();
    }

    /// push lines back in context from a start index
    void PushLines(int start = 0, float nextHeight = -1f) {
        var max = m_Lines.Length - 2;
        if (max == 1) {
            return;
        }

        // if we don't have the height of the next line, use the current line instead
        if (nextHeight < 0f) {
            nextHeight = m_Lines[start].Height;
        }

        // offset all the lines
        var offset = new Vector2(0f, nextHeight * 0.5f + m_Spacing);
        var alpha = 1f;

        for (var i = 0; i < max; i++) {
            var line = m_Lines[start + i];
            if (line.IsHidden) {
                break;
            }

            var lineHeight = line.Height * 0.5f;

            // for older lines, update offset so that our text clears the line below
            offset.y += lineHeight;

            // and offset horizontally
            offset.x = m_Offset_Horizontal.Evaluate(Random.value);

            // exponentiate the alpha
            alpha *= m_Alpha_Base;

            // move to this position
            line.Move(offset);
            line.Fade(alpha);

            // and update offset to the top edge of this line
            offset.y += lineHeight + m_Spacing;
        }

        // hide the oldest line
        var prevLine = m_Lines[start + max - 1];
        prevLine.Hide();
    }

    // -- queries --
    /// the time in seconds for a line to appear
    public float LineEnterDuration {
        get => !m_Lines.IsEmpty ? m_Lines[0].EnterDuration : 0f;
    }

    // -- DialogueViewBase --
    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        // advance to the next line
        m_Lines.Offset();

        // show the new line
        var nextLine = m_Lines.Head;
        nextLine.Show(dialogueLine.Text.Text);

        // offset all the lines
        PushLines(start: 1, nextHeight: nextLine.Height);

        // complete the line immediately
        onDialogueLineFinished?.Invoke();
    }
}

}