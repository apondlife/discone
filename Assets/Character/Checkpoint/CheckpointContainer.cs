namespace Discone {

/// a dependency container for checkpoint systems
interface CheckpointContainer {
    /// the checkpoint tuning
    CheckpointTuning Tuning { get; }

    /// the character
    Character Character { get; }

    /// the checkpoint
    Checkpoint Checkpoint { get; }

    // TODO: checkpoint and systems are too entangled; checkpoint should read system state (events)
    /// grab the nearby checkpoint
    public void GrabCheckpoint();

    /// create the checkpoint
    public void CreateCheckpoint(Checkpoint checkpoint);
}

}