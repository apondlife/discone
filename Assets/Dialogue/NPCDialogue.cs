using UnityEngine;
using UnityEngine.InputSystem;
using UnityAtoms.BaseAtoms;

[RequireComponent(typeof(Collider))]
public class NPCDialogue: MonoBehaviour {
    // -- constants --
    private const string _dialogueTargetTag = "PlayerDialogueTarget";

    // -- config --
    [Header("config")]
    [Tooltip("IMPORTANT: the title of the yarn node for this npc's dialogue")]
    /// TODO: should this be called the "dialogue node title"?
    [SerializeField] private string dialogueMessage;

    [Tooltip("the yarn node for the dialogue for this npc")]
    [SerializeField] private GameObject talkable;

    // -- references --
    [Header("references")]
    [Tooltip("the character")]
    [SerializeField] private GameObject m_Character;

    [Tooltip("the input reference")]
    [SerializeField] InputActionReference m_Talk;

    [Tooltip("if the character is in range to talk")]
    [SerializeField] private bool _playerInTalkRange = false;

    // -- events --
    [Header("events")]
    [Tooltip("start the dialogue for this character")]
    [SerializeField] private GameObjectEvent m_StartDialogue;

    // -- lifecycle --
    void Start() {
        if (talkable) {
            talkable.SetActive(false);
        }
    }

    void OnEnable() {
        m_Talk.action.performed += OnTalkPressed;
    }

    void OnDisable() {
        m_Talk.action.performed -= OnTalkPressed;
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
        get => dialogueMessage;
    }

    /// the character for this dialogue
    public GameObject Character {
        get => m_Character;
    }

    // -- events --
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag(_dialogueTargetTag)) {
            Debug.Log($"[dialogue] character in range <{dialogueMessage}>");
            _playerInTalkRange = true;
            if (talkable) talkable.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag(_dialogueTargetTag)) {
            _playerInTalkRange = false;
            if (talkable) talkable.SetActive(false);
            // TODO: end dialogue on exit, new atom?
        }
    }

    /// when the player presses talk
    void OnTalkPressed(InputAction.CallbackContext _) {
        // talk to the character if they're in range
        if (_playerInTalkRange) {
            StartTalking();
        }
    }
}
