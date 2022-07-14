using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine.InputSystem;

/// the discone checkpoint controller
[RequireComponent(typeof(ThirdPerson.Player))]
sealed class PlayerCheckpoint: MonoBehaviour {
    // -- atoms --
    [Header("atoms")]
    [Tooltip("the progress of the checkpoint save")]
    [SerializeField] DisconeCharacterVariable m_Character;

    [Tooltip("the progress of the checkpoint save")]
    [SerializeField] FloatVariable m_SaveProgress;

    [Tooltip("the progress of the checkpoint load")]
    [SerializeField] FloatVariable m_LoadProgress;

    // -- refs --
    [Header("refs")]
    [Tooltip("the save checkpoint input")]
    [SerializeField] InputActionReference m_SaveCheckpointAction;

    [Tooltip("the load checkpoint input")]
    [SerializeField] InputActionReference m_LoadCheckpointAction;

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
}