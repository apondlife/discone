using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using TMPro;

public class IvanDialogueView : DialogueViewBase
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

    void Start()
    {
        textAnimator = GetComponent<TextShakeChars>();
        canvasGroup.alpha = 0;
    }

    public void Reset()
    {
        canvasGroup = GetComponentInParent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
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
}
