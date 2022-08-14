using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Yarn.Unity;
using UnityAtoms.BaseAtoms;

/// the dialogue system
public class DialogueSystem: MonoBehaviour {
    // -- name --
    [Header("state")]
    [Tooltip("if the dialogue is active")]
    [SerializeField] BoolVariable m_IsActive;

    [Tooltip("the dialogue for the character we're talking to")]
    [SerializeField] CharacterDialogue m_ActiveDialogue;

    // -- events --
    [Header("events")]
    [Tooltip("when to start dialogue with a character")]
    [SerializeField] GameObjectEvent m_Start;

    [Tooltip("when the dialogue completes")]
    [SerializeField] VoidEvent m_Complete;

    [Tooltip("when to switch character")]
    [SerializeField] GameObjectEvent m_SwitchCharacter;

    // -- references --
    [Header("references")]
    [Tooltip("the input reference")]
    [SerializeField] InputActionReference m_Talk;

    [Tooltip("the dialogue runner")]
    [SerializeField] DialogueRunner yarnDialogueRunner;

    [Header("textboxes")]
    [Tooltip("ivan textbox")]
    [SerializeField] DialogueViewBase ivanTextbox;

    [Tooltip("default textbox")]
    [SerializeField] NeueArtfulDialogueView defaultTextbox;


    // -- props --
    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        // bind events
        m_Subscriptions
            .Add(m_Start, OnStartDialogue)
            .Add(m_Complete, OnDialogueComplete);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// start dialogue with a particular character
    void StartDialogue(CharacterDialogue dialogue) {
        if (dialogue == null) {
            Debug.LogError($"[dialog] tried to start dialogue w/ a character w/ no CharacterDialogue");
            return;
        }

        if (m_ActiveDialogue != null) {
            return;
        }

        Debug.Log($"[dialog] start dialgoue <{dialogue.NodeTitle}>");

        DialogueViewBase textbox = ChooseTextbox(dialogue.NodeTitle);
        DialogueViewBase[] textboxArray = { textbox };
        yarnDialogueRunner.SetDialogueViews(textboxArray);

        // show the dialogue for this character
        m_IsActive.Value = true;
        m_ActiveDialogue = dialogue;
        yarnDialogueRunner.StartDialogue(dialogue.NodeTitle);

        // register the continue talking event
        m_Talk.action.performed += OnTalkPressed;
    }

    /// advance dialgoue to the next line
    void RunNextLine() {
        Debug.Log($"[dialog] advance line <{m_ActiveDialogue.NodeTitle}>");
        yarnDialogueRunner.OnViewUserIntentNextLine();
    }

    /// complete dialgoue with the current character
    void CompleteDialogue() {
        if (m_ActiveDialogue == null) {
            Debug.LogError($"[dialog] tried to complete dialogue w/ no active CharacterDialogue");
            return;
        }

        Debug.Log($"[dialog] complete dialogue <{m_ActiveDialogue.NodeTitle}>");

        // complete the active dialgoue
        m_SwitchCharacter.Raise(m_ActiveDialogue.Character);
        m_IsActive.Value = false;
        m_ActiveDialogue = null;

        // stop listening for the continue talking event
        m_Talk.action.performed -= OnTalkPressed;
    }

    /// choose textbox/dialogue view
    DialogueViewBase ChooseTextbox(string nodeName) {
        // choose dialogue view (depending on character)
        // TODO: probably a lot more elegant way to do this
        if (nodeName == "Ivan") {
            return ivanTextbox;
        } else {
            // HACK
            defaultTextbox.lastLine = null;
            return defaultTextbox;
        }
    }

    // -- events --
    /// when a dialogue node is started
    void OnStartDialogue(GameObject obj) {
        if (obj == null) {
            Debug.LogError($"attempting to start dialogue with a destroyed object");
            return;
        }

        var dialogue = obj.GetComponent<CharacterDialogue>();
        StartDialogue(dialogue);
    }

    /// when the talk button is pressed
    void OnTalkPressed(InputAction.CallbackContext _) {
        // if there's an active dialgoue, continue. see CharacterDialogue#StartTalking to see
        // how dialogue starts
        if (m_ActiveDialogue != null) {
            RunNextLine();
        }
    }

    /// when dialogue completes
    void OnDialogueComplete() {
        CompleteDialogue();
    }
}
