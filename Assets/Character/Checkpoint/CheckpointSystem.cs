using ThirdPerson;

/// a checkpoint system
abstract class CheckpointSystem: ThirdPerson.System {
    // -- deps --
    /// the character
    protected CharacterState m_State;

    /// the checkpoint
    protected CharacterCheckpoint m_Checkpoint;

    // -- ThirdPerson.System --
    protected override SystemState State { get; set; } = new SystemState();

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