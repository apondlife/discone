using System;
using Soil;
using UnityEngine;

namespace Discone {

using Container = CheckpointContainer;
using Phase = Phase<CheckpointContainer>;

/// a character's ability to save new checkpoints
/// not => smelling => (grab) planting => (plant) done
[Serializable]
sealed class SaveCheckpointSystem: SimpleSystem<Container> {
    // -- types --
    /// the tuning for the checkpoint system
    [Serializable]
    public sealed class Tuning {
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
    [Tooltip("the tuning")]
    [SerializeField] public Tuning m_Tuning;

    // -- props --
    /// whether the system is saving
    bool m_IsSaving;

    /// the checkpoint being saved
    Checkpoint m_PendingCheckpoint;

    // -- ThirdPerson.System --
    protected override Phase InitInitialPhase() {
        return NotSaving;
    }

    // -- NotSaving --
    Phase NotSaving => new(
        name: "NotSaving",
        enter: NotSaving_Enter,
        update: NotSaving_Update,
        exit: NotSaving_Exit
    );

    void NotSaving_Enter(Container c) {
        m_IsSaving = false;
    }

    void NotSaving_Update(float delta, Container c) {
        if (CanSave(c)) {
            ChangeTo(Delaying);
        }
    }

    void NotSaving_Exit(Container c) {
        m_PendingCheckpoint = Checkpoint.FromState(c.Character.State.Next);
    }

    // -- Delaying --
    Phase Delaying => new(
        name: "Delaying",
        enter: Delaying_Enter,
        update: Delaying_Update
    );

    void Delaying_Enter(Container c) {
    }

    void Delaying_Update(float delta, Container c) {
        // continue delaying
        if (!CanSave(c)) {
            ChangeTo(NotSaving);
            return;
        }

        // start smelling once delay elapses
        if (PhaseElapsed > m_Tuning.Delay) {
            ChangeTo(Smelling);
        }
    }

    // -- Smelling --
    Phase Smelling => new(
        name: "Smelling",
        enter: Smelling_Enter,
        update: Smelling_Update
    );

    void Smelling_Enter(Container c) {
        m_IsSaving = true;
    }

    void Smelling_Update(float delta, Container c) {
        // continue smelling
        if (!CanSave(c)) {
            ChangeTo(NotSaving);
            return;
        }

        // start planting once you finish smelling around for a flower
        if (PhaseElapsed > m_Tuning.SmellDuration) {
            ChangeTo(Planting);
        }
    }

    // -- Planting --
    Phase Planting => new(
        name: "Planting",
        enter: Planting_Enter,
        update: Planting_Update
    );

    void Planting_Enter(Container c) {
        c.GrabCheckpoint();
    }

    void Planting_Update(float delta, Container c) {
        if (!CanSave(c)) {
            ChangeTo(NotSaving);
            return;
        }

        // switch to simply existing after planting
        if (PhaseElapsed > m_Tuning.PlantDuration) {
            ChangeTo(Being);
        }
    }

    // -- Being --
    Phase Being => new(
        name: "Being",
        enter: Being_Enter,
        update: Being_Update
    );

    void Being_Enter(Container c) {
        c.CreateCheckpoint(m_PendingCheckpoint);
    }

    void Being_Update(float delta, Container c) {
        if (!CanSave(c)) {
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
    bool CanSave(Container c) {
        return c.Character.State.Curr.IsCrouching && c.Character.State.Curr.IsIdle;
    }
}

}