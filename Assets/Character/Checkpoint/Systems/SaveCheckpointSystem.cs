using System;
using ThirdPerson;
using UnityEngine;

/// a character's ability to save new checkpoints
/// not => smelling => (grab) planting => (plant) done
[Serializable]
sealed class SaveCheckpointSystem: CheckpointSystem {
    // -- types --
    /// the tunables for the checkpoint system
    [Serializable]
    public sealed class Tunables {
        [Tooltip("the time (s) start smelling after crouch")]
        [SerializeField] float m_Delay;

        [Tooltip("the time (s) to smell a flower")]
        [SerializeField] float m_SmellDuration;

        [Tooltip("the time (s) to plant a flower")]
        [SerializeField] float m_PlantDuration;

        // -- queries --
        /// the time (s) start smelling after crouch
        public float Delay {
            get => m_Delay;
        }

        /// the time (s) to smell a flower after delay
        public float SmellDuration {
            get => m_SmellDuration - Delay;
        }

        /// the time (s) to plant a flower after smell
        public float PlantDuration {
            get => m_PlantDuration - SmellDuration;
        }
    }

    // -- deps --
    [Tooltip("the tunables")]
    [SerializeField] public Tunables m_Tunables;

    // -- props --
    /// whether the system is saving
    bool m_IsSaving;

    /// the phase's elapsed time
    float m_PhaseElapsed;

    /// the checkpoint being saved
    Checkpoint m_PendingCheckpoint;

    // -- ThirdPerson.System --
    protected override Phase InitInitialPhase() {
        return NotSaving;
    }

    // -- NotSaving --
    Phase NotSaving => new Phase(
        name: "NotSaving",
        enter: NotSaving_Enter,
        update: NotSaving_Update,
        exit: NotSaving_Exit
    );

    void NotSaving_Enter() {
        m_IsSaving = false;
        m_PhaseElapsed = 0.0f;
    }

    void NotSaving_Update(float delta) {
        if (CanSave) {
            ChangeTo(Delaying);
        }
    }

    void NotSaving_Exit() {
        m_PendingCheckpoint = Checkpoint.FromState(m_State.Curr);
    }

    // -- Delaying --
    Phase Delaying => new Phase(
        name: "Delaying",
        enter: Delaying_Enter,
        update: Delaying_Update
    );

    void Delaying_Enter() {
        m_PhaseElapsed = 0.0f;
    }

    void Delaying_Update(float delta) {
        // continue delaying
        m_PhaseElapsed += delta;

        if (!CanSave) {
            ChangeTo(NotSaving);
            return;
        }

        // start smelling once delay elapses
        if (m_PhaseElapsed > m_Tunables.Delay) {
            ChangeTo(Smelling);
        }
    }

    // -- Smelling --
    Phase Smelling => new Phase(
        name: "Smelling",
        enter: Smelling_Enter,
        update: Smelling_Update
    );

    void Smelling_Enter() {
        m_IsSaving = true;
        m_PhaseElapsed = 0.0f;
    }

    void Smelling_Update(float delta) {
        // continue smelling
        m_PhaseElapsed += delta;
        if (!CanSave) {
            ChangeTo(NotSaving);
            return;
        }

        // start planting once you finish smelling around for a flower
        if (m_PhaseElapsed > m_Tunables.SmellDuration) {
            ChangeTo(Planting);
        }
    }

    // -- Planting --
    Phase Planting => new Phase(
        name: "Planting",
        enter: Planting_Enter,
        update: Planting_Update
    );

    void Planting_Enter() {
        m_PhaseElapsed = 0.0f;
        m_Checkpoint.GrabCheckpoint();
    }

    void Planting_Update(float delta) {
        // continue planting
        m_PhaseElapsed += delta;

        if (!CanSave) {
            ChangeTo(NotSaving);
            return;
        }

        // switch to simply existing after planting
        if (m_PhaseElapsed > m_Tunables.PlantDuration) {
            ChangeTo(Being);
        }
    }

    // -- Being --
    Phase Being => new Phase(
        name: "Being",
        enter: Being_Enter,
        update: Being_Update
    );

    void Being_Enter() {
        m_PhaseElapsed = 0.0f;
        m_Checkpoint.CreateCheckpoint(m_PendingCheckpoint);
    }

    void Being_Update(float delta) {
        // continue being
        m_PhaseElapsed += delta;

        if (!CanSave) {
            ChangeTo(NotSaving);
            return;
        }
    }

    // -- queries --
    /// TODO: this should be written to some external state structure
    public bool IsSaving {
        get => m_IsSaving;
    }

    /// if the character can currently save
    bool CanSave {
        get => m_State.IsCrouching && m_State.IsIdle;
    }
}