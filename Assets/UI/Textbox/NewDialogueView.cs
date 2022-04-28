using System;
using System.Collections;
using System.Linq;
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
    internal TextMeshProUGUI characterNameText = null;

    LocalizedLine currentLine = null;
    TextGlow textGlower;

    // -- events --
    [Header("events")]
    [Tooltip("when the next line runs")]
    [SerializeField] VoidEvent m_RunNextLine;

    [SerializeField] TextboxPlacement[] placements;

    // -- lifecycle --
    void Start() {
        canvasGroup.alpha = 0;
        textGlower = GetComponent<TextGlow>();
    }

    public void Reset() {
        canvasGroup = GetComponentInParent<CanvasGroup>();
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished) {
        currentLine = dialogueLine;

        // choose a random placement
        // there are more than one possible "placements" for how 
        // the dialogue UI will be laid out        
        var placement = placements[UnityEngine.Random.Range(0, placements.Length - 1)];
        lineText = placement.lineText;
        characterNameText = placement.characterNameText;

        canvasGroup.gameObject.SetActive(true);
        // set only the chosen placement active
        placement.gameObject.SetActive(true);
        for (int i = 0; i < placements.Length; i++) {
            if (placements[i] != placement) {
                placements[i].gameObject.SetActive(false);
                Debug.Log("Setting " + placements[i].gameObject.name + " inactive");
            }
        }
        


        characterNameText.SetText(dialogueLine.CharacterName);
        lineText.SetText(dialogueLine.TextWithoutCharacterName.Text);
        
        //StartCoroutine(PopInCharactersRandomly());
        // textGlower.StartGlowText();

        // shake!
        foreach (MarkupAttribute attr in dialogueLine.TextWithoutCharacterName.Attributes) {
            if (attr.Name == "em") {
                Color32 color = characterNameText.color;
                textGlower.StartGlowText(lineText, color, attr.Position, attr.Length);
            }
        }



        // Immediately appear
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;

        onDialogueLineFinished();
    }

    private void ShowCharacter(int i) {
        TMP_TextInfo textInfo = lineText.textInfo;

        Color32[] newVertexColors;
        Color32 c0 = lineText.color;

        int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

        // Get the vertex colors of the mesh used by this text element (character or sprite).
        newVertexColors = textInfo.meshInfo[materialIndex].colors32;

        // Get the index of the first vertex used by this text element.
        int vertexIndex = textInfo.characterInfo[i].vertexIndex;

        newVertexColors[vertexIndex + 0] = c0;
        newVertexColors[vertexIndex + 1] = c0;
        newVertexColors[vertexIndex + 2] = c0;
        newVertexColors[vertexIndex + 3] = c0;

        // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
        lineText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

    }

    private void HideCharacters() {
        lineText.ForceMeshUpdate();
        TMP_TextInfo textInfo = lineText.textInfo;

        Color32[] newVertexColors;
        Color32 c0 = lineText.color;
        int characterCount = textInfo.characterCount;

        for (int i = 0; i < characterCount; i++) {
            // Get the index of the material used by the current character.
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

            // Get the vertex colors of the mesh used by this text element (character or sprite).
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;

            // Get the index of the first vertex used by this text element.
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            c0 = lineText.color;
            c0.a = 0;

            newVertexColors[vertexIndex + 0] = c0;
            newVertexColors[vertexIndex + 1] = c0;
            newVertexColors[vertexIndex + 2] = c0;
            newVertexColors[vertexIndex + 3] = c0;

            // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
            lineText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }

    IEnumerator PopInCharactersRandomly() {
        HideCharacters();

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
