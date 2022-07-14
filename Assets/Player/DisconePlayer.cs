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

    // -- atoms --
    [Header("atoms")]
    [Tooltip("the progress of the checkpoint save")]
    [SerializeField] FloatVariable m_SaveProgress;

    [Tooltip("the progress of the checkpoint load")]
    [SerializeField] FloatVariable m_LoadProgress;

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

        // configure cameras
        var cameras = GetComponentsInChildren<UnityEngine.Camera>();
        foreach (var camera in cameras) {
            camera.farClipPlane = m_FarClipPlane.Value;
        }
    }

    void Update() {
        if (m_Character == null) {
            return;
        }

        // coordinate input & current character's checkpoint
        var checkpoint = m_Character.Checkpoint;

        // save/cancel checkpoint on press/release
        var save = m_SaveCheckpointAction.action;
        if (save.WasPressedThisFrame()) {
            checkpoint.StartSave();
        } else if (save.WasReleasedThisFrame()) {
            checkpoint.CancelSave();
        }

        // load/cancel checkpoint on press/release
        var load = m_LoadCheckpointAction.action;
        if (load.WasPressedThisFrame()) {
            checkpoint.StartLoad();
        } else if (load.WasReleasedThisFrame()) {
            checkpoint.CancelLoad();
        }

        // update external atoms
        m_SaveProgress?.SetValue(checkpoint.SaveElapsed);
        m_LoadProgress?.SetValue(checkpoint.LoadPercent);
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
}