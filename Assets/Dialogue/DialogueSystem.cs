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
    [SerializeField] NPCDialogue m_ActiveDialogue;

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

    // -- lifecycle --
    void Awake() {
        // bind events
        m_Start.Register(OnStartDialogue);
        m_Complete.Register(OnDialogueComplete);
    }

    // -- commands --
    /// start dialogue with a particular character
    void StartDialogue(NPCDialogue dialogue) {
        if (dialogue == null) {
            Debug.LogError($"[dialogue] tried to start dialogue w/ a character w/ no NPCDialogue");
            return;
        }

        if (m_ActiveDialogue != null) {
            return;
        }

        Debug.Log($"[dialogue] start dialgoue <{dialogue.NodeTitle}>");

        // show the dialogue for this character
        m_IsActive.Value = true;
        m_ActiveDialogue = dialogue;
        yarnDialogueRunner.StartDialogue(dialogue.NodeTitle);

        // register the continue talking event
        m_Talk.action.performed += OnTalkPressed;
    }

    /// advance dialgoue to the next line
    void RunNextLine() {
        Debug.Log($"[dialogue] advance line <{m_ActiveDialogue.NodeTitle}>");
        yarnDialogueRunner.OnViewUserIntentNextLine();
    }

    /// complete dialgoue with the current character
    void CompleteDialogue() {
        if (m_ActiveDialogue == null) {
            Debug.LogError($"[dialogue] tried to complete dialogue w/ no active NPCDialogue");
            return;
        }

        Debug.Log($"[dialogue] complete dialogue <{m_ActiveDialogue.NodeTitle}>");

        // complete the active dialgoue
        m_SwitchCharacter.Raise(m_ActiveDialogue.Character);
        m_IsActive.Value = false;
        m_ActiveDialogue = null;

        // stop listening for the continue talking event
        m_Talk.action.performed -= OnTalkPressed;
    }

    // -- events --
    /// when a dialogue node is started
    void OnStartDialogue(GameObject obj) {
        var dialogue = obj.GetComponent<NPCDialogue>();
        StartDialogue(dialogue);
    }

    /// when the talk button is pressed
    void OnTalkPressed(InputAction.CallbackContext _) {
        // if there's an active dialgoue, continue. see NPCDialogue#StartTalking to see
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
