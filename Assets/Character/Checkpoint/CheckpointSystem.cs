using ThirdPerson;
using Soil;

namespace Discone {

/// a checkpoint system
abstract class CheckpointSystem: Soil.System {
    // -- deps --
    /// the character
    protected CharacterState m_State;

    /// the character input
    protected CharacterInput<InputFrame> m_Input;

    /// the checkpoint
    protected CharacterCheckpoint m_Checkpoint;

    // -- Soil.System --
    protected override SystemState State { get; set; } = new();

    // -- lifetime --
    /// initialize the system
    public void Init(
        CharacterState state,
        CharacterInput<InputFrame> input,
        CharacterCheckpoint checkpoint
    ) {
        base.Init();

        m_State = state;
        m_Input = input;
        m_Checkpoint = checkpoint;
    }
}

}