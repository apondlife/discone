using UnityEngine;
using UnityAtoms;
using UnityEngine.InputSystem;

/// the discone checkpoint controller
[RequireComponent(typeof(ThirdPerson.Player))]
public sealed class PlayerCheckpoint: MonoBehaviour {
    // -- atoms --
    [Header("atoms")]
    [Tooltip("the progress of the checkpoint save")]
    [SerializeField] DisconeCharacterVariable m_Character;

    // -- refs --
    [Header("refs")]
    [Tooltip("the save checkpoint input")]
    [SerializeField] InputActionReference m_SaveCheckpointAction;

    [Tooltip("the load checkpoint input")]
    [SerializeField] InputActionReference m_LoadCheckpointAction;

    // -- props --
    /// if currently saving a checkpoint
    bool m_IsSaving = false;

    // -- lifecycle --
    void Update() {
        // coordinate input & current character's checkpoint
        var checkpoint = m_Character?.Value?.Checkpoint;
        if (checkpoint == null) {
            return;
        }

        // save/cancel checkpoint on press/release
        var save = m_SaveCheckpointAction.action;
        if (save.WasPressedThisFrame()) {
            checkpoint.StartSave();
        } else if (save.WasReleasedThisFrame()) {
            checkpoint.StopSave();
        }

        // start saving once character begins save
        if (save.IsPressed() && checkpoint.IsSaving) {
            m_IsSaving = true;
        } else if (save.WasReleasedThisFrame()) {
            m_IsSaving = false;
        }

        // load/cancel checkpoint on press/release
        var load = m_LoadCheckpointAction.action;
        if (load.WasPressedThisFrame()) {
            checkpoint.StartLoad();
        } else if (load.WasReleasedThisFrame()) {
            checkpoint.StopLoad();
        }
    }

    // -- queries --
    /// if currently saving a checkpoint
    public bool IsSaving {
        get => m_IsSaving;
    }
}