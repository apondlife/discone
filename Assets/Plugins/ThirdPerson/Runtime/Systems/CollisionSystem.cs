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
        var curr = c.State.Curr;
        var next = c.State.Next;

        // move character using controller if not idle
        var frame = c.Controller.Move(
            next.Position,
            next.Velocity,
            next.Up,
            delta
        );

        // find the ground collision if it exists
        next.Ground = frame.Ground;
        next.Wall = frame.Wall;

        // find the last most relevant touched surface
        var surface = curr.LastSurface;

        // if we're in the air, there's no surface
        if (next.Ground.IsNone && next.Wall.IsNone) {
            surface = CharacterCollision.None;
        }
        // if we weren't touching a wall, and now we are, it's the wall
        else if (curr.Wall.IsNone && next.Wall.IsSome) {
            surface = next.Wall;
        }
        // otherwise if we weren't touching a ground, and now we are, it's the ground
        else if (curr.Ground.IsNone && next.Ground.IsSome) {
            surface = next.Ground;
        }
        // otherwise, if the last surface was a ground, use any new ground
        else if (curr.LastSurface.Normal == curr.Ground.Normal && next.Ground.IsSome) {
            surface = next.Ground;
        }
        // otherwise, if the last surface was a wall, use any new wall
        else if (curr.LastSurface.Normal == curr.Wall.Normal && next.Wall.IsSome) {
            surface = next.Wall;
        }

        // initialize prev last surface if unset
        if (curr.PrevLastSurface.IsNone) {
            next.LastSurface = surface;
            next.PrevLastSurface = surface;
        }
        // if we were in there air, then replace the last surface
        else if (curr.LastSurface.IsNone && surface.IsSome) {
            next.LastSurface = surface;
        }
        // if we changed surfaces, push the new surface onto the queue
        else if (curr.LastSurface.Normal != surface.Normal) {
            next.PrevLastSurface = curr.LastSurface;
            next.LastSurface = surface;
        }

        // sync controller state back to character state
        next.Velocity = frame.Velocity;
        next.Acceleration = (next.Velocity - curr.Velocity) / delta;
        next.Position = frame.Position;
    }
}

}