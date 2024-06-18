using System;
using ThirdPerson;
using UnityEngine;

/// a checkpoint position
[Serializable]
public record Checkpoint {
    // -- props --
    /// the position
    public Vector3 Position;

    /// the character's facing direction
    public Vector3 Forward;

    // -- lifetime --
    [Obsolete]
    public Checkpoint() {
    }

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

    /// create checkpoint from a transform
    public static Checkpoint FromTransform(Transform transform) {
        return new Checkpoint(
            transform.position,
            transform.forward
        );
    }
}