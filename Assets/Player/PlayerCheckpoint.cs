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
    }

    // -- queries --
    /// if currently saving a checkpoint
    public bool IsSaving {
        get => m_Character?.Value?.Checkpoint.IsSaving ?? false;
    }
}