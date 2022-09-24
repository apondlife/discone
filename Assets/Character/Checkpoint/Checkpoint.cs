using UnityEngine;

/// a checkpoint position
public record Checkpoint {
    // -- props --
    /// the position
    public readonly Vector3 Position;

    /// the character's facing direction
    public readonly Vector3 Forward;

    // -- lifetime --
    /// create a pending checkpoint
    public Checkpoint(Vector3 position, Vector3 forward) {
        Position = position;
        Forward = forward;
    }

    // -- factories --
    /// create checkpoint from the current state frame
    public static Checkpoint FromState(CharacterState.Frame frame) {
        return new Checkpoint(
            frame.Position,
            frame.Forward
        );
    }
}