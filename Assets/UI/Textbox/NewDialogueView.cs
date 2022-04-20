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

        Debug.Log("RUNNING LINE!");



        

        //int startPos = 0;

        

        characterNameText.SetText(dialogueLine.CharacterName);
        //lineText.text = dialogueLine.TextWithoutCharacterName.Text;
        lineText.SetText(dialogueLine.TextWithoutCharacterName.Text);

        lineText.ForceMeshUpdate();

        TMP_TextInfo textInfo = lineText.textInfo;
        Debug.Log("char count: " + textInfo.characterCount);
        

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            Debug.Log("character!  " + charInfo.character);
            
            if (i == 2) {
                Debug.Log("setting character  " + charInfo.character + " to invisible");
                charInfo.isVisible = false;
            }
            Debug.Log(charInfo.isVisible);
            // lineText.ForceMeshUpdate();

            // Color32 c1;
            // c1 = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 127);                         

            // int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            // newVertexColors = textInfo.meshInfo.vertexColors;
            // newVertexColors[vertexIndex + 0] = c1;
            // newVertexColors[vertexIndex + 1] = c1;           
            // newVertexColors[vertexIndex + 2] = c1;
            // newVertexColors[vertexIndex + 3] = c1;

            // lineText.mesh.vertices = textInfo.meshInfo.vertices;
            // lineText.mesh.uv = textInfo.meshInfo.uv0;
            // lineText.mesh.uv2 = textInfo.meshInfo.uv2;
            // lineText.mesh.colors32 = newVertexColors;
            

        }




        // if (inlineCharacterName) {
        //     characterNameText.text = "";
        //     lineText.text = dialogueLine.Text.Text;

        //     // This is where i would do animations... if i had any!!!
        //     foreach (MarkupAttribute attr in dialogueLine.Text.Attributes) {
        //         if (attr.Name == "shake") {
        //             textAnimator.StartShakeText(lineText, attr.Position, attr.Length);
        //         }
        //     }
        // }
        // else {
        //     characterNameText.text = dialogueLine.CharacterName;
        //     lineText.text = dialogueLine.TextWithoutCharacterName.Text;

        //     foreach (MarkupAttribute attr in dialogueLine.TextWithoutCharacterName.Attributes) {
        //         if (attr.Name == "shake") {
        //             textAnimator.StartShakeText(lineText, attr.Position, attr.Length);
        //         }
        //     }
        // }

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
