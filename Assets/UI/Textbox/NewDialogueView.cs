using System;
using System.Collections;
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

    //TextShakeChars textAnimator;
    VertexAttributeModifier textAnimator;

    // -- events --
    [Header("events")]
    [Tooltip("when the next line runs")]
    [SerializeField] VoidEvent m_RunNextLine;

    // -- lifecycle --
    void Start() {
        //textAnimator = GetComponent<TextShakeChars>();
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


        //todo: put this in Start()?
        

        

        //int startPos = 0;

        

        characterNameText.SetText(dialogueLine.CharacterName);
        //lineText.text = dialogueLine.TextWithoutCharacterName.Text;
        lineText.SetText(dialogueLine.TextWithoutCharacterName.Text);

        // textAnimator.StartShakeText(lineText, 2, 10);
        
        //StartCoroutine(AnimateColors());

        // Immediately appear
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;

        onDialogueLineFinished();
    }

    IEnumerator AnimateColors() {
        
        lineText.renderMode = TextRenderFlags.DontRender;

        lineText.ForceMeshUpdate();

        TMP_TextInfo textInfo = lineText.textInfo;

        while (true) {
            
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (i != 2) {
                    yield return new WaitForSeconds(0.1f);
                }
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                Debug.Log("character!  " + charInfo.character);
                
                // if (i == 2) {
                //     Debug.Log("setting character  " + charInfo.character + " to invisible");
                //     charInfo.isVisible = false;
                // }
                // Debug.Log(charInfo.isVisible);
                

                Color32 c1;
                c1 = new Color32((byte)UnityEngine.Random.Range(0, 255), 
                                (byte)UnityEngine.Random.Range(0, 255),
                                (byte)UnityEngine.Random.Range(0, 255), 127);                         

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                Color32[] newVertexColors = textInfo.meshInfo[0].colors32;
                newVertexColors[vertexIndex + 0] = c1;
                newVertexColors[vertexIndex + 1] = c1;           
                newVertexColors[vertexIndex + 2] = c1;
                newVertexColors[vertexIndex + 3] = c1;

                lineText.mesh.vertices = textInfo.meshInfo[0].vertices;
                lineText.mesh.uv = textInfo.meshInfo[0].uvs0;
                lineText.mesh.uv2 = textInfo.meshInfo[0].uvs2;
                lineText.mesh.colors32 = newVertexColors;

                lineText.ForceMeshUpdate();

                yield return new WaitForSeconds(0.1f);


            }

        }


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
