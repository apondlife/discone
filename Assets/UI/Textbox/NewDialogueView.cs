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

    //TextShakeChars textAnimator;
    VertexAttributeModifier textAnimator;

    // -- events --
    [Header("events")]
    [Tooltip("when the next line runs")]
    [SerializeField] VoidEvent m_RunNextLine;

    [SerializeField] TextboxPlacement[] placements;

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

        Debug.Log("running line");

        // choose a random placement
        // there are more than one possible "placements" for how 
        // the dialogue UI will be laid out        
        var placement = placements[UnityEngine.Random.Range(0, placements.Length - 1)];
        //Debug.Log(placement.gameObject.name);
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
        

        //Debug.Log("RUNNING LINE!");

        characterNameText.SetText(dialogueLine.CharacterName);
        //lineText.text = dialogueLine.TextWithoutCharacterName.Text;
        lineText.SetText(dialogueLine.TextWithoutCharacterName.Text);

        // textAnimator.StartShakeText(lineText, 2, 10);
        
        StartCoroutine(PopInCharactersRandomly());



        // Immediately appear
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;

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

        // This last process could be done to only update the vertex data that has changed as opposed to all of the vertex data but it would require extra steps and knowing what type of renderer is used.
        // These extra steps would be a performance optimization but it is unlikely that such optimization will be necessary.
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
