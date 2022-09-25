using ThirdPerson;

/// a checkpoint system
abstract class CheckpointSystem: ThirdPerson.System {
    // -- deps --
    /// the character
    protected CharacterState m_State;

    /// the checkpoint
    protected CharacterCheckpoint m_Checkpoint;

    // -- lifetime --
    /// initialize the system
    public void Init(
        CharacterState state,
        CharacterCheckpoint checkpoint
    ) {
        base.Init();

        m_State = state;
        m_Checkpoint = checkpoint;
    }
}