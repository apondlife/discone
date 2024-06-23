namespace Discone {

/// a dependency container for checkpoint systems
interface CheckpointContainer {
    /// the tuning
    CheckpointTuning Tuning { get; }

    /// the state
    CheckpointState State { get; }

    /// the character
    Character Character { get; }

    /// the checkpoint
    Placement Checkpoint { get; }

    // TODO: checkpoint and systems are too entangled; checkpoint should read system state (events)
    /// grab the nearby checkpoint
    public void Grab();

    /// create the checkpoint
    public void Create(Placement checkpoint);
}

}