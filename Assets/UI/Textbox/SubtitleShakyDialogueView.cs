using System;
using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using TMPro;

public class SubtitleShakyDialogueView : DialogueViewBase {
    [SerializeField]
    internal CanvasGroup canvasGroup;

    [SerializeField]
    internal TextMeshProUGUI lineText;

    LocalizedLine currentLine;

    [SerializeField]
    TextShakeChars textAnimator;

    // -- lifecycle --
    void Awake() {
        canvasGroup.alpha = 0;
    }

    public void Reset() {
        canvasGroup = GetComponentInParent<CanvasGroup>();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        currentLine = dialogueLine;
        lineText.gameObject.SetActive(true);
        canvasGroup.gameObject.SetActive(true);

        lineText.text = dialogueLine.Text.Text;

        // shake!
        foreach (MarkupAttribute attr in dialogueLine.Text.Attributes) {
            if (attr.Name == "em") {
                textAnimator.StartShakeText(lineText, attr.Position, attr.Length);
            }
        }

        // Immediately appear
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public override void DismissLine(Action onDismissalComplete) {
        currentLine = null;

        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        onDismissalComplete();
    }
}