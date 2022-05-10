using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine.UI;

public class NeueArtfulDialogueView : DialogueViewBase
{

    [SerializeField]
    internal CanvasGroup canvasGroup;

    // [SerializeField]
    // internal TextMeshProUGUI lineText = null;

    [SerializeField]
    internal TextMeshProUGUI characterNameText = null;

    [SerializeField]
    internal Image nameBackground = null;

    LocalizedLine currentLine = null;

    LocalizedLine lastLine = null;

    GameObject continueSignal = null;

    NeueArtfulBox currentBox = null;


    TextColor textColorer;
    Color32 color;

    // -- events --
    [Header("events")]
    [Tooltip("when the next line runs")]
    [SerializeField] VoidEvent m_RunNextLine;

    [SerializeField] NeueArtfulBox[] boxes;

    // -- lifecycle --
    void Start() {
        canvasGroup.alpha = 0;
        textColorer = GetComponent<TextColor>();
        color = nameBackground.color;

        foreach (var box in boxes) {
            box.continueSignal.gameObject.SetActive(false);
        }
    }

    public void Reset() {
        canvasGroup = GetComponentInParent<CanvasGroup>();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {

        // if we're a new character, or have changed characters,
        // hide all the boxes to start
        if (lastLine == null || lastLine.CharacterName != dialogueLine.CharacterName) {
            lastLine = dialogueLine;
            canvasGroup.gameObject.SetActive(true);
            characterNameText.SetText(dialogueLine.CharacterName);

            for (int i = 0; i < boxes.Length; i++) {
                boxes[i].gameObject.SetActive(false);
                boxes[i].currentlyUsed = false;
            }
        }

        bool foundFit = FindBoxFit(dialogueLine, true);
        // currentBox = boxes[0];
        // currentBox.gameObject.SetActive(true);
        // currentBox.lineText.text = dialogueLine.TextWithoutCharacterName.Text;
        //currentBox.lineText.renderMode = TextRenderFlags.DontRender;

        // this just gets rid of the render that's queue'd by setting the text
        currentBox.lineText.ForceMeshUpdate();

        

        foreach (MarkupAttribute attr in dialogueLine.TextWithoutCharacterName.Attributes) {
            if (attr.Name == "em") {
                textColorer.ColorText(currentBox.lineText, color, attr.Position, attr.Length);
            }
        }

        HideCharacters(currentBox.lineText);
        StartCoroutine(PopInCharactersRandomly(currentBox.lineText));


        // Immediately appear
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;

        onDialogueLineFinished();
    }

    private bool FindBoxFit(LocalizedLine dialogueLine, bool overwriteOkay) {
        for (int i = 0; i < boxes.Length; i++) {

            NeueArtfulBox tryBox = boxes[i];
            if (!overwriteOkay && tryBox.currentlyUsed) {
                continue;
            }

            string cachedText = tryBox.lineText.text;

            // this actually sets the text of the lint text too
            TMP_TextInfo textInfo = tryBox.lineText.GetTextInfo(dialogueLine.TextWithoutCharacterName.Text);
            tryBox.lineText.SetText(cachedText);

            if (textInfo.characterCount == dialogueLine.TextWithoutCharacterName.Text.Length) {
                tryBox.gameObject.SetActive(true);
                
                tryBox.lineText.SetText(dialogueLine.TextWithoutCharacterName.Text);
                tryBox.currentlyUsed = true;

                // turn off previous continue thing - turn on new one
                continueSignal?.SetActive(false);
                continueSignal = tryBox.continueSignal;
                continueSignal.SetActive(true);

                currentBox = tryBox;

                return true;
                // break;
            } else {
                tryBox.lineText.SetText(cachedText);
            }

        }

        return false;
    }

    private async void ShowCharacter(TextMeshProUGUI lineText, int i) {
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

    private void HideCharacters(TextMeshProUGUI lineText) {
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

            c0 = newVertexColors[vertexIndex];
            c0.a = 0;

            newVertexColors[vertexIndex + 0] = c0;
            newVertexColors[vertexIndex + 1] = c0;
            newVertexColors[vertexIndex + 2] = c0;
            newVertexColors[vertexIndex + 3] = c0;

            // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
            //lineText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            
        }
        lineText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    IEnumerator PopInCharactersRandomly(TextMeshProUGUI lineText) {
        //HideCharacters();

        TMP_TextInfo textInfo = lineText.textInfo;
        int characterCount = textInfo.characterCount;

        // make ordered list of ints (i.e. [0,1,2,3...])
        // https://stackoverflow.com/questions/10681882/create-c-sharp-int-with-value-as-0-1-2-3-length
        int[] orderedArr = Enumerable.Range(0, characterCount).ToArray();

        System.Random rnd = new System.Random();
        int[] randArr = orderedArr.OrderBy(x => rnd.Next()).ToArray();

        for (int i = 0; i < characterCount; i++) {
            ShowCharacter(lineText, randArr[i]);

            yield return new WaitForSeconds(0.02f);
        }

        yield return new WaitForSeconds(0.05f);

    }

    Color32 ProbeColorDebug(TextMeshProUGUI text, int characterIdx) {
        TMP_TextInfo textInfo = text.textInfo;

        int materialIndex = textInfo.characterInfo[characterIdx].materialReferenceIndex;

        int vertexIndex = textInfo.characterInfo[characterIdx].vertexIndex;

        Color32 c0 = new Color32();

        if (textInfo != null) {
            c0 = textInfo.meshInfo[materialIndex].colors32[vertexIndex];

        }
        return c0;
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
