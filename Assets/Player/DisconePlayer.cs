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
    /// the current game character
    DisconeCharacter m_Character;

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

    void Update() {
        // save/cancel checkpoint on press/release
        var save = m_SaveCheckpointAction.action;
        if (save.WasPressedThisFrame()) {
            m_Character.StartSaveCheckpoint();
        } else if (save.WasReleasedThisFrame()) {
            m_Character.CancelSaveCheckpoint();
        }

        // load/cancel checkpoint on press/release
        var load = m_LoadCheckpointAction.action;
        if (load.WasPressedThisFrame()) {
            m_Character.StartLoadCheckpoint();
        } else if (load.WasReleasedThisFrame()) {
            m_Character.CancelLoadCheckpoint();
        }
    }

    void OnDestroy() {
        // unbind events
        m_Subscriptions.Dispose();
    }

    // -- events --
    // when the player starts driving a character
    void OnDriveStart(Character character) {
        m_Character = character.GetComponent<DisconeCharacter>();
        m_Character.Drive();
    }

    // when the player stops driving a character
    void OnDriveStop(Character character) {
        m_Character.Release();
        m_Character = null;
    }

    /// when the dialog becomes in/active
    void OnIsDialogueActiveChanged(bool isDialogueActive) {
        m_InputSource.enabled = !isDialogueActive;
    }

    /// when the player presses the save checkpoint action
    void OnSaveCheckpointPressed(InputAction.CallbackContext _) {
        m_Character.StartSaveCheckpoint();
    }

    /// when the player presses the load checkpoint action
    void OnLoadCheckpointPressed(InputAction.CallbackContext _) {
        m_Character.StartLoadCheckpoint();
    }
}