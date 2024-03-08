using System;
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

    [Tooltip("the horizontal offset for fading lines")]
    [SerializeField] MapOutCurve m_HorizontalOffset;

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
        foreach (var line in m_Lines) {
            line.Hide();
        }
    }

    // -- queries --
    /// the time in seconds for a line to appear
    public float LineEnterDuration {
        get => !m_Lines.IsEmpty ? m_Lines[0].EnterDuration : 0f;
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
                var lineHeight = line.Height * 0.5f;

                // for older lines, update offset so that our text clears the line below
                if (i != 0) {
                    offset.y += lineHeight;

                    // and offset horizontally
                    offset.x = m_HorizontalOffset.Evaluate(Random.value);
                }

                // move to this position
                line.Move(offset);

                // and update offset to the top edge of this line
                offset.y += lineHeight + m_Spacing;
            }
        }

        // hide the oldest line
        var prevLine = m_Lines[max];
        prevLine.Hide();

        onDialogueLineFinished?.Invoke();
    }
}

}