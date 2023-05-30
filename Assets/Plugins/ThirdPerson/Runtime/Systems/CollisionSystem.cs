using System;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState CollisionState;
    }
}

/// how the character is affected by gravity
[Serializable]
sealed class CollisionSystem: CharacterSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return Active;
    }

    protected override SystemState State {
        get => c.State.Next.CollisionState;
        set => c.State.Next.CollisionState = value;
    }

    // -- NotIdle --
    Phase Active => new Phase(
        name: "Active",
        update: Active_Update
    );

    void Active_Update(float delta) {
        var v = c.State.Next.Velocity;

        // move character using controller if not idle
        var frame = c.Controller.Move(
            c.State.Next.Position,
            c.State.Next.Velocity,
            c.State.Next.Up,
            delta
        );

        // find the ground collision if it exists
        c.State.Next.Ground = frame.Ground;
        c.State.Next.Wall = frame.Wall;

        // sync controller state back to character state
        c.State.Next.Velocity = frame.Velocity;
        c.State.Next.Acceleration = (c.State.Next.Velocity - c.State.Curr.Velocity) / delta;
        c.State.Next.Position = frame.Position;
    }
}

}