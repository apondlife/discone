using System;
using ThirdPerson;

/// a character's ability to save new checkpoints
/// not => smelling => (grab) planting => (plant) done
sealed class SaveCheckpointSystem: ThirdPerson.System {
    // -- types --
    /// the tunables for the checkpoint system
    [Serializable]
    public sealed class Tunables {
        /// the time (s) to smell a flower
        public float SmellDuration;

        /// the time (s) to plant a flower
        public float PlantDuration;
    }

    [Obsolete]
    public sealed class SaveInput {
        public bool IsSaving;
    }

    // -- deps --
    /// the tunables
    Tunables m_Tunables;

    /// the character's state
    CharacterState m_State;

    /// the checkpoint
    CharacterCheckpoint m_Checkpoint;

    // -- props --
    /// the input state
    SaveInput m_Input = new SaveInput();

    /// the save elapsed time
    float m_SaveElapsed;

    /// the save elapsed time
    CharacterCheckpoint.Checkpoint m_PendingCheckpoint;

    // -- lifetime --
    public SaveCheckpointSystem(
        Tunables tunables,
        CharacterState state,
        CharacterCheckpoint checkpoint
    ): base() {
        m_Tunables = tunables;
        m_State = state;
        m_Checkpoint = checkpoint;
    }

    // -- queries --
    /// the input state
    public SaveInput Input {
        get => m_Input;
    }

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
        m_PendingCheckpoint = null;
        m_SaveElapsed = 0.0f;
        m_Checkpoint.IsSaving = false;
    }

    void NotSaving_Update(float delta) {
        if (m_Input.IsSaving) {
            ChangeTo(Smelling);
        }
    }

    void NotSaving_Exit() {
        m_PendingCheckpoint = CharacterCheckpoint.Checkpoint.FromState(m_State.Curr);
        m_Checkpoint.IsSaving = true;
    }

    // -- Smelling --
    Phase Smelling => new Phase(
        name: "Smelling",
        update: Smelling_Update
    );

    void Smelling_Update(float delta) {
        Active_Update(delta);

        // start planting once you finish smelling around for a flower
        if (m_SaveElapsed > m_Tunables.SmellDuration) {
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
        m_Checkpoint.GrabCheckpoint();
    }

    void Planting_Update(float delta) {
        Active_Update(delta);

        // switch to simply existing after planting
        if (m_SaveElapsed > m_Tunables.PlantDuration) {
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
        m_Checkpoint.CreateCheckpoint(m_PendingCheckpoint);
        m_PendingCheckpoint = null;
    }

    void Being_Update(float delta) {
        Active_Update(delta);
    }

    // -- shared --
    // the base update when attempting to save
    void Active_Update(float delta) {
        m_SaveElapsed += delta;

        if (!m_Input.IsSaving || !m_State.IsGrounded || !m_State.IsIdle) {
            ChangeTo(NotSaving);
        }
    }

    // -- queries --
    /// TODO: this should be written to some external state structure
    public bool IsSaving {
        get => m_SaveElapsed > 0.0f;
    }
}