using ThirdPerson;

namespace Discone {

public record CheckpointState {
    // -- constants --
    const float k_Inactive = -1.0f;

    // -- props --
    /// whether the checkpoint is saving
    public bool IsSaving;

    /// whether the checkpoint is loading
    public bool IsLoading {
        get => Load_Elapsed >= 0;
    }

    // -- save --
    /// the checkpoint being saved
    public Checkpoint Save_PendingCheckpoint;

    // -- load --
    /// the elapsed time
    public float Load_Elapsed = k_Inactive;

    /// the total cast time
    public float Load_Duration;

    /// the state when the load starts
    public CharacterState.Frame Load_SrcState;

    /// the final state when the load completes
    public CharacterState.Frame Load_DstState;

    /// the current state while loading
    public CharacterState.Frame Load_CurState;

    // -- load/commands
    /// reset the elapsed load time
    public void Load_Reset() {
        Load_Elapsed = k_Inactive;
    }
}

}