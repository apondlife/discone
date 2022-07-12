using ThirdPerson;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using UnityEngine.InputSystem;

/// the discone player
[RequireComponent(typeof(ThirdPerson.Player))]
sealed class DisconePlayer: MonoBehaviour {
    // -- events --
    [Header("events")]
    [Tooltip("when the player starts driving a character")]
    [SerializeField] CharacterEvent m_DriveStart;

    [Tooltip("when the player stops driving a character")]
    [SerializeField] CharacterEvent m_DriveStop;

    [Tooltip("if the dialogue is active")]
    [SerializeField] BoolEvent m_IsDialogueActiveChanged;

    // -- refs --
    [Header("refs")]
    [Tooltip("the input source")]
    [SerializeField] PlayerInputSource m_InputSource;

    [Tooltip("the save checkpoint input")]
    [SerializeField] InputActionReference m_SaveCheckpointAction;

    [Tooltip("the load checkpoint input")]
    [SerializeField] InputActionReference m_LoadCheckpointAction;

    [Tooltip("the distance to the far clip plane")]
    [SerializeField] FloatReference m_FarClipPlane;

    // -- props --
    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        // bind events
        m_Subscriptions
            .Add(m_DriveStart, OnDriveStart)
            .Add(m_DriveStop, OnDriveStop)
            .Add(m_IsDialogueActiveChanged, OnIsDialogueActiveChanged);

        // bind events
        m_SaveCheckpointAction.action.performed += OnSaveCheckpointPressed;
        m_LoadCheckpointAction.action.performed += OnLoadCheckpointPressed;

        // configure cameras
        var cameras = GetComponentsInChildren<UnityEngine.Camera>();
        foreach (var camera in cameras) {
            camera.farClipPlane = m_FarClipPlane.Value;
        }
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- queries --
    /// get the dialogue attached to a character
    NPCDialogue FindDialogue(Character character) {
        var dialogue = character.GetComponentInChildren<NPCDialogue>();
        Debug.Assert(dialogue != null, $"character {character.name} has no dialogue attached.");
        return dialogue;
    }

    CharacterCheckpoint m_CharacterCheckpoint;

    // -- events --
    // when the player starts driving a character
    void OnDriveStart(Character character) {
        FindDialogue(character)?.StopListening();
        m_CharacterCheckpoint = character.GetComponent<CharacterCheckpoint>();
    }

    // when the player stops driving a character
    void OnDriveStop(Character character) {
        FindDialogue(character)?.StartListening();
        m_CharacterCheckpoint = null;
    }

    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        m_InputSource.enabled = !isDialogueActive;
    }

    /// when the player presses the save checkpoint action
    void OnSaveCheckpointPressed(InputAction.CallbackContext _) {
        Debug.Log($"pressed save checkpoint");
        m_CharacterCheckpoint?.Save();
    }

    /// when the player presses the load checkpoint action
    void OnLoadCheckpointPressed(InputAction.CallbackContext _) {
        Debug.Log($"pressed load checkpoint");
        m_CharacterCheckpoint?.Load();
    }
}