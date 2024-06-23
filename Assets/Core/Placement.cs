using System;
using ThirdPerson;
using UnityEngine;

namespace Discone {

/// an object placement
[Serializable]
public record Placement {
    // -- props --
    /// the position
    public Vector3 Position;

    /// the facing direction
    public Vector3 Forward;

    // -- lifetime --
    [Obsolete]
    public Placement() {
    }

    /// create a pending checkpoint
    public Placement(Vector3 position, Vector3 forward) {
        Position = position;
        Forward = forward;
    }

    // -- factories --
    /// create checkpoint from the current state frame
    public static Placement FromState(CharacterState.Frame frame) {
        return new Placement(
            frame.Position,
            frame.Forward
        );
    }

    /// create checkpoint from a transform
    public static Placement FromTransform(Transform transform) {
        return new Placement(
            transform.position,
            transform.forward
        );
    }
}

}