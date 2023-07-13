using System;
using UnityEngine;

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

        // find the last most relevant touched surface
        // if we weren't touching a wall, and now we are, it's the wall
        if (c.State.Curr.Wall.IsNone && c.State.Next.Wall.IsSome) {
            c.State.Next.LastSurface = c.State.Next.Wall;
        }
        // otherwise if we weren't touching a ground, and now we are, it's the ground
        else if (c.State.Curr.Ground.IsNone && c.State.Next.Ground.IsSome) {
            c.State.Next.LastSurface = c.State.Next.Ground;
        }
        // otherwise, if the last surface was a ground, use any new ground
        else if (c.State.Curr.LastSurface.Normal == c.State.Curr.Ground.Normal && c.State.Next.Ground.IsSome) {
            c.State.Next.LastSurface = c.State.Next.Ground;
        }
        // otherwise, if the last surface was a wall, use any new wall
        else if (c.State.Curr.LastSurface.Normal == c.State.Curr.Wall.Normal && c.State.Next.Wall.IsSome) {
            c.State.Next.LastSurface = c.State.Next.Wall;
        }
        // otherwise, the last surface stays the same
        else {
            c.State.Next.LastSurface = c.State.Curr.LastSurface;
        }

        // sync controller state back to character state
        c.State.Next.Velocity = frame.Velocity;
        c.State.Next.Acceleration = (c.State.Next.Velocity - c.State.Curr.Velocity) / delta;
        c.State.Next.Position = frame.Position;
    }
}

}