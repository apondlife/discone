using System;
using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using TMPro;
using UnityAtoms.BaseAtoms;

public class NewDialogueView : DialogueViewBase
{

    [SerializeField]
    internal CanvasGroup canvasGroup;

    [SerializeField]
    internal TextMeshProUGUI lineText = null;

    [SerializeField]
    internal bool inlineCharacterName = true;


    [SerializeField]
    internal TextMeshProUGUI characterNameText = null;

    LocalizedLine currentLine = null;

    TextShakeChars textAnimator;

    // -- events --
    [Header("events")]
    [Tooltip("when the next line runs")]
    [SerializeField] VoidEvent m_RunNextLine;

    // -- lifecycle --
    void Start() {
        textAnimator = GetComponent<TextShakeChars>();
        canvasGroup.alpha = 0;
    }

    public void Reset() {
        canvasGroup = GetComponentInParent<CanvasGroup>();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        currentLine = dialogueLine;
        lineText.gameObject.SetActive(true);
        canvasGroup.gameObject.SetActive(true);

        if (inlineCharacterName) {
            characterNameText.text = "";
            lineText.text = dialogueLine.Text.Text;

            // This is where i would do animations... if i had any!!!
            foreach (MarkupAttribute attr in dialogueLine.Text.Attributes) {
                if (attr.Name == "shake") {
                    textAnimator.StartShakeText(lineText, attr.Position, attr.Length);
                }
            }
        }
        else {
            characterNameText.text = dialogueLine.CharacterName;
            lineText.text = dialogueLine.TextWithoutCharacterName.Text;

            foreach (MarkupAttribute attr in dialogueLine.TextWithoutCharacterName.Attributes) {
                if (attr.Name == "shake") {
                    textAnimator.StartShakeText(lineText, attr.Position, attr.Length);
                }
            }
        }

        // Immediately appear
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;

        onDialogueLineFinished();
    }

    public override void DismissLine(Action onDismissalComplete) {
        currentLine = null;

        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        onDismissalComplete();
    }

    // -- events --
    /// when the next line runs
    void OnRunNextLine() {
        // we're not actually displaying a line. no-op.
        if (currentLine == null) {
            return;
        }

        ReadyForNextLine();
    }
}
