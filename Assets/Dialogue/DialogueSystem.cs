using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
using UnityAtoms.BaseAtoms;
using UnityEngine.Serialization;

namespace Discone {

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
    [FormerlySerializedAs("m_Start")]
    [SerializeField] GameObjectEvent m_StartDialogue;

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

    [Tooltip("the dialogue canvas")]
    [SerializeField] GameObject m_Canvas;

    [Header("textboxes")]
    [Tooltip("ivan textbox")]
    [SerializeField] DialogueViewBase ivanTextbox;

    [Tooltip("default textbox")]
    [SerializeField] NeueArtfulDialogueView defaultTextbox;


    // -- props --
    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Awake() {
        // bind events
        m_Subscriptions
            .Add(m_StartDialogue, OnStartDialogue)
            .Add(m_Complete, OnDialogueComplete);

        m_Canvas.SetActive(true);
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// start dialogue with a particular character
    void StartDialogue(CharacterDialogue dialogue) {
        if (!dialogue) {
            Log.Dialog.E($"tried to start dialogue w/ a character w/ no CharacterDialogue");
            return;
        }

        if (m_ActiveDialogue != null) {
            return;
        }

        Log.Dialog.I($"start dialogue <{dialogue.NodeTitle}>");

        // prepare the dialogue view
        var textbox = ChooseTextbox(dialogue.NodeTitle);
        textbox.gameObject.SetActive(true);
        yarnDialogueRunner.SetDialogueViews(new[] { textbox });

        // show the dialogue for this character
        m_IsActive.Value = true;
        m_ActiveDialogue = dialogue;
        yarnDialogueRunner.StartDialogue(dialogue.NodeTitle);

        // register the continue talking event
        m_Talk.action.performed += OnTalkPressed;
    }

    /// advance dialgoue to the next line
    void RunNextLine() {
        Log.Dialog.I($"advance line <{m_ActiveDialogue.NodeTitle}>");
        yarnDialogueRunner.OnViewRequestedInterrupt();
    }

    /// complete dialgoue with the current character
    void CompleteDialogue() {
        if (m_ActiveDialogue == null) {
            Log.Dialog.E($"tried to complete dialogue w/ no active CharacterDialogue");
            return;
        }

        Log.Dialog.I($"complete dialogue <{m_ActiveDialogue.NodeTitle}>");

        // complete the active dialgoue
        // TODO: make this a CharacterEvent
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
        if (!obj) {
            Log.Dialog.E($"attempting to start dialogue with a destroyed object");
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

}