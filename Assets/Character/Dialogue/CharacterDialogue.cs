using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class CharacterDialogue: MonoBehaviour {
    // -- constants --
    private const string _dialogueTargetTag = "PlayerDialogueTarget";

    // -- config --
    [Header("config")]
    [Tooltip("IMPORTANT: the title of the yarn node for this characters's dialogue")]
    [UnityEngine.Serialization.FormerlySerializedAs("dialogueMessage")]
    [SerializeField] private string m_NodeTitle;

    [Tooltip("the character's talk indicator")]
    [UnityEngine.Serialization.FormerlySerializedAs("talkable")]
    [SerializeField] private CharacterDialogueIndicator m_TalkIndicator;

    // -- refs --
    [Header("refs")]
    [Tooltip("the input action")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Talk")]
    [SerializeField] InputActionReference m_TalkInput;

    [Tooltip("a reference to the current character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    // -- debug --
    [Header("debug")]
    [Tooltip("if this is in range to talk")]
    [SerializeField] bool m_IsInRange = false;

    // -- events --
    [Header("events")]
    [Tooltip("start the dialogue for this character")]
    [SerializeField] private GameObjectEvent m_StartDialogue;

    // -- props --
    // the parent character
    DisconeCharacter m_Character;

    // -- lifecycle --
    void Start() {
        // get parent
        m_Character = GetComponentInParent<DisconeCharacter>();

        // TODO: do in prefab
        if (m_TalkIndicator) {
            m_TalkIndicator.Hide();
        }
    }

    void Update() {
        // update the indicator
        if (m_TalkIndicator) {
            if (m_IsInRange && IsListening) {
                m_TalkIndicator.Show();
            } else {
                m_TalkIndicator.Hide();
            }
        }
    }

    void OnEnable() {
        m_TalkInput.action.performed += OnTalkPressed;
    }

    void OnDisable() {
        m_TalkInput.action.performed -= OnTalkPressed;
    }

    void OnDestroy() {
        m_TalkInput.action.performed -= OnTalkPressed;
    }

    // -- commands --
    /// start talking to the character
    /// NOTE: this should never run if we're already talking, should check for input before this
    void StartTalking() {
        m_StartDialogue.Raise(gameObject);
    }

    // -- queries --
    /// the title of the dialogue node to start
    public string NodeTitle {
        get => m_NodeTitle;
    }

    /// the character for this dialogue
    public GameObject Character {
        get => m_Character.gameObject;
    }

    // -- events --
    // when the player stops driving a character
    public void StopListening() {
        // by doing this, the player has to come in and out of range again to redo a dialogue with a character
        m_IsInRange = false;
    }

    /// when the player presses talk
    void OnTalkPressed(InputAction.CallbackContext _) {
        // talk to the character if they're in range
        if (IsListening && m_IsInRange) {
            StartTalking();
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!IsListening) {
            return;
        }

        if (other.CompareTag(_dialogueTargetTag)) {
            Debug.Log($"[dialog] character in range <{m_NodeTitle}>");
            m_IsInRange = true;
        }
    }

    void OnTriggerExit(Collider other) {
        if (!IsListening) {
            return;
        }

        if (other.CompareTag(_dialogueTargetTag)) {
            m_IsInRange = false;
            // TODO: end dialogue on exit, new atom?
        }
    }

    // -- queries --
    /// if this is listening for nearby players (ie: not being controlled by the local player)
    bool IsListening {
        get => m_CurrentCharacter.Value != m_Character;
    }
}
