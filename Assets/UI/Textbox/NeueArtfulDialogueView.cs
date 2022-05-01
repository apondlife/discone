using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using TMPro;
using UnityAtoms.BaseAtoms;

public class NeueArtfulDialogueView : DialogueViewBase
{

    [SerializeField]
    internal CanvasGroup canvasGroup;

    [SerializeField]
    internal TextMeshProUGUI lineText = null;

    [SerializeField]
    internal TextMeshProUGUI characterNameText = null;

    LocalizedLine currentLine = null;

    // NeueArtfulBox box = null;

    
    TextColor textColorer;

    // -- events --
    [Header("events")]
    [Tooltip("when the next line runs")]
    [SerializeField] VoidEvent m_RunNextLine;

    [SerializeField] NeueArtfulBox[] boxes;

    // -- lifecycle --
    void Start() {
        canvasGroup.alpha = 0;
        textColorer = GetComponent<TextColor>();
    }

    public void Reset() {
        canvasGroup = GetComponentInParent<CanvasGroup>();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        

        // if we're a new character, or have changed characters, try to fit
        // the line in the smallest possible placement
        // the placements are sorted from smallest to largest
        if (currentLine == null || currentLine.CharacterName != dialogueLine.CharacterName) {
            currentLine = dialogueLine;
            Debug.Log("artful!");
            Debug.Log(dialogueLine.CharacterName);

            

            
            for (int i = 0; i < boxes.Length; i++) {

                NeueArtfulBox tryBox = boxes[i];
                if (tryBox.currentlyUsed) {
                    continue;
                }

                lineText = tryBox.lineText;

                // try to fit line in trybox
                lineText.SetText(dialogueLine.TextWithoutCharacterName.Text);
                lineText.ForceMeshUpdate();
                TMP_TextInfo textInfo = lineText.textInfo;

                Debug.Log(dialogueLine.TextWithoutCharacterName.Text);
                Debug.Log(textInfo.characterCount);
                Debug.Log(dialogueLine.TextWithoutCharacterName.Text.Length);
                
                

            

            //         lineText = placement.lineText;
            // characterNameText = placement.characterNameText;

            // // set only the chosen placement active
            // placement.gameObject.SetActive(true);

            //     // if (placements[i] != placement) {
                //     placements[i].gameObject.SetActive(false);
                // }
            }          
        }    
        
        
        canvasGroup.gameObject.SetActive(true);
        // characterNameText.SetText(dialogueLine.CharacterName);
        // lineText.SetText(dialogueLine.TextWithoutCharacterName.Text);

        // lineText.renderMode = TextRenderFlags.DontRender;
        
        

        //HideCharacters();

        // color the text
        // foreach (MarkupAttribute attr in dialogueLine.TextWithoutCharacterName.Attributes) {
        //     if (attr.Name == "em") {
        //         Color32 color = placement.color;
        //         textColorer.ColorText(lineText, color, attr.Position, attr.Length);
        //     }
        //     // // TEST
        //     // Debug.Log("TEST");
        //     // TMP_TextInfo textInfo = lineText.textInfo;
        //     // var charInfo = textInfo.characterInfo[attr.Position];
        //     // Debug.Log(charInfo.character);
        //     // int materialIndex = charInfo.materialReferenceIndex;
        //     // Debug.Log(textInfo.meshInfo[materialIndex].colors32[charInfo.vertexIndex]);
        // }


        // HideCharacters();
        // StartCoroutine(PopInCharactersRandomly());



        // Immediately appear
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;

        onDialogueLineFinished();
    }

    private void ShowCharacter(int i) {
        TMP_TextInfo textInfo = lineText.textInfo;

        Color32[] newVertexColors;

        int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

        // Get the vertex colors of the mesh used by this text element (character or sprite).
        newVertexColors = textInfo.meshInfo[materialIndex].colors32;

        // Get the index of the first vertex used by this text element.
        int vertexIndex = textInfo.characterInfo[i].vertexIndex;

        Color32 c0 = newVertexColors[vertexIndex];
        c0.a = 255;

        newVertexColors[vertexIndex + 0] = c0;
        newVertexColors[vertexIndex + 1] = c0;
        newVertexColors[vertexIndex + 2] = c0;
        newVertexColors[vertexIndex + 3] = c0;

        // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
        lineText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

    }

    private void HideCharacters() {
        //lineText.ForceMeshUpdate();
        TMP_TextInfo textInfo = lineText.textInfo;

        Color32[] newVertexColors;
        Color32 c0;
        int characterCount = textInfo.characterCount;

        for (int i = 0; i < characterCount; i++) {
            // Get the index of the material used by the current character.
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            // the color might be different because of the specific coloring we do for the attribute
            //Color32 currentColor = textInfo.meshInfo[materialIndex].colors32;

            // Get the vertex colors of the mesh used by this text element (character or sprite).
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;

            // Get the index of the first vertex used by this text element.
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            //c0 = lineText.color;
            c0 = newVertexColors[vertexIndex];
            c0.a = 0;

            Debug.Log(textInfo.characterInfo[i].character);
            Debug.Log(c0);

            newVertexColors[vertexIndex + 0] = c0;
            newVertexColors[vertexIndex + 1] = c0;
            newVertexColors[vertexIndex + 2] = c0;
            newVertexColors[vertexIndex + 3] = c0;

            // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
            lineText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }

    IEnumerator PopInCharactersRandomly() {
        //HideCharacters();

        //lineText.ForceMeshUpdate();
        TMP_TextInfo textInfo = lineText.textInfo;
        int characterCount = textInfo.characterCount;

        // make ordered list of ints (i.e. [0,1,2,3...])
        // https://stackoverflow.com/questions/10681882/create-c-sharp-int-with-value-as-0-1-2-3-length
        int[] orderedArr = Enumerable.Range(0, characterCount).ToArray();

        System.Random rnd = new System.Random();
        int[] randArr = orderedArr.OrderBy(x => rnd.Next()).ToArray();

        for (int i = 0; i < characterCount; i++) {
            ShowCharacter(randArr[i]);
            yield return new WaitForSeconds(0.02f);
        }

        yield return new WaitForSeconds(0.05f);

    }




    public override void DismissLine(Action onDismissalComplete) {
        currentLine = null;

        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
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
