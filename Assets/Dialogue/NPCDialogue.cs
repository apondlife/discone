using UnityEngine;
using UnityEngine.InputSystem;
using UnityAtoms.BaseAtoms;
using ThirdPerson;

[RequireComponent(typeof(Collider))]
public class NPCDialogue: MonoBehaviour {
    // -- constants --
    private const string _dialogueTargetTag = "PlayerDialogueTarget";

    // -- config --
    [Header("config")]
    [Tooltip("IMPORTANT: the title of the yarn node for this npc's dialogue")]
    [UnityEngine.Serialization.FormerlySerializedAs("dialogueMessage")]
    [SerializeField] private string m_NodeTitle;

    [Tooltip("the character's talk indicator")]
    [UnityEngine.Serialization.FormerlySerializedAs("talkable")]
    [SerializeField] private GameObject m_TalkIndicator;

    // -- refs --
    [Header("refs")]
    [Tooltip("the character")]
    [SerializeField] private GameObject m_Character;

    [Tooltip("the input reference")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Talk")]
    [SerializeField] InputActionReference m_TalkInput;

    // -- debug --
    [Header("debug")]
    [Tooltip("if this is listening for nearby players")]
    [SerializeField] bool m_IsListening;

    [Tooltip("if this is in range to talk")]
    [SerializeField] bool m_IsInRange = false;

    // -- events --
    [Header("events")]
    [Tooltip("when the player starts driving a character")]
    [SerializeField] CharacterEvent m_DriveStart;

    [Tooltip("when the player stops driving a character")]
    [SerializeField] CharacterEvent m_DriveStop;

    [Tooltip("start the dialogue for this character")]
    [SerializeField] private GameObjectEvent m_StartDialogue;

    // -- lifecycle --
    void Start() {
        // TODO: do in prefab
        if (m_TalkIndicator) {
            m_TalkIndicator.SetActive(false);
        }
    }

    void OnEnable() {
        m_TalkInput.action.performed += OnTalkPressed;
    }

    void OnDisable() {
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
        get => m_Character;
    }

    // -- events --
    // when the player starts driving a character
    void StartListening() {
        m_IsListening = true;
    }

    // when the player stops driving a character
    void StopListening() {
        m_IsListening = false;
        m_IsInRange = false;
    }

    /// when the player presses talk
    void OnTalkPressed(InputAction.CallbackContext _) {
        // talk to the character if they're in range
        if (m_IsListening && m_IsInRange) {
            StartTalking();
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!m_IsListening) {
            return;
        }

        if (other.CompareTag(_dialogueTargetTag)) {
            Debug.Log($"[dialogue] character in range <{m_NodeTitle}>");
            m_IsInRange = true;

            if (m_TalkIndicator) {
                m_TalkIndicator.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (!m_IsListening) {
            return;
        }

        if (other.CompareTag(_dialogueTargetTag)) {
            m_IsInRange = false;

            if (m_TalkIndicator) {
                m_TalkIndicator.SetActive(false);
            }
            // TODO: end dialogue on exit, new atom?
        }
    }
}
