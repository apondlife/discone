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

        // update collisions
        // TODO: store a list of N collisions this frame
        next.Ground = frame.Ground;
        next.Wall = frame.Wall;

        // find the newest collision surface
        var newSurface = next.WallSurface;
        if (curr.Wall.IsNone && next.Wall.IsSome) {
            newSurface = next.Wall;
        } else if (curr.Ground.IsNone && next.Ground.IsSome) {
            newSurface = next.Ground;
        }

        // find the last most relevant touched surface
        var surface = curr.CurrSurface;

        // if we're in the air, there's no surface
        if (next.Ground.IsNone && next.Wall.IsNone) {
            surface = CharacterCollision.None;
        }
        // otherwise, if the newest surface is different, use that
        else if (surface.Normal != newSurface.Normal) {
            surface = newSurface;
        }

        // TODO: maybe initialize PerceivedSurface on first contact
        next.CurrSurface = surface;
 
        // move the perceived surface towards the current surface
        next.PerceivedSurface.SetNormal(Vector3.RotateTowards(
            curr.PerceivedSurface.Normal,
            next.CurrSurface.Normal,
            c.Tuning.Surface_PerceptionAngularSpeed * Mathf.Deg2Rad * delta,
            c.Tuning.Surface_PerceptionLengthSpeed * delta
        ));
 
        // point for perceived surface is invalid
        next.PerceivedSurface.Point = Vector3.negativeInfinity;

        // sync controller state back to character state
        next.Velocity = frame.Velocity;
        next.Acceleration = (next.Velocity - curr.Velocity) / delta;
        next.Position = frame.Position;
    }
}

}