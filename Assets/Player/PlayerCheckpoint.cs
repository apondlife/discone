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
    [Tooltip("the load checkpoint input")]
    [SerializeField] InputActionReference m_LoadCheckpointAction;

    // -- props --
    /// if the checkpoint was saving previously
    bool m_PrevIsSaving = false;

    /// if the saving value changed
    bool m_IsSavingChanged = false;

    // -- lifecycle --
    void Update() {
        // coordinate input & current character's checkpoint
        var checkpoint = m_Character?.Value?.Checkpoint;
        if (checkpoint == null) {
            return;
        }

        // load/cancel checkpoint on press/release
        var load = m_LoadCheckpointAction.action;
        if (load.WasPressedThisFrame()) {
            checkpoint.StartLoad();
        } else if (load.WasReleasedThisFrame()) {
            checkpoint.StopLoad();
        }

        var isSaving = IsSaving;
        m_IsSavingChanged = m_PrevIsSaving != isSaving;
        m_PrevIsSaving = isSaving;
    }

    // -- queries --
    /// if currently saving a checkpoint
    public bool IsSaving {
        get => m_Character?.Value?.Checkpoint.IsSaving ?? false;
    }

    /// if the saving value changed
    public bool IsSavingChanged {
        get => m_IsSavingChanged;
    }
}