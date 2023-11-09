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

        // integrate acceleration (forces)
        var a = next.Acceleration * delta;
        var v = next.Velocity + next.Inertia + a;

        // move character using controller if not idle
        var frame = c.Controller.Move(
            next.Position,
            v,
            next.Up,
            delta
        );

        // update collisions
        // TODO: store a list of n collisions this frame
        next.Ground = frame.Ground;
        next.Wall = frame.Wall;

        // find the last relevant touched surface
        var surface = curr.CurrSurface;

        // if the newest surface is different, use that
        var newSurface = next.GroundSurface;
        if (newSurface.IsSome && surface.Normal != newSurface.Normal) {
            surface = newSurface;
        }

        next.CurrSurface = surface;

        // move the perceived surface towards the current surface
        var perceivedNormal = curr.PerceivedSurface.Normal;
        if (curr.PerceivedSurface.IsNone) {
            perceivedNormal = next.CurrSurface.Normal;
        }

        // TODO: maybe update the time since last touching the curr surface
        next.PerceivedSurface.SetNormal(Vector3.RotateTowards(
            perceivedNormal,
            next.CurrSurface.Normal,
            c.Tuning.Surface_PerceptionAngularSpeed * Mathf.Deg2Rad * delta,
            0f
        ));

        // point for perceived surface is invalid
        next.PerceivedSurface.Point = Vector3.negativeInfinity;

        // sync controller state back to character state
        next.Velocity = frame.Velocity;
        next.Acceleration = (frame.Velocity - curr.Velocity) / delta;
        next.Position = frame.Position;

        // calculate inertia, momentum lost after collision; the frame velocity is the
        // velocity projected into each collision surface (if hitting a wall, it's 0)
        var inertia = v - frame.Velocity;
        var inertiaDir = inertia.normalized;
        var inertiaMag = inertia.magnitude;

        // remove acceleration into surface (unrealized) from inertia & prevent
        // inversion of direction
        inertia -= inertiaDir * Mathf.Clamp(Vector3.Dot(a, inertiaDir), 0f, inertiaMag);

        next.Inertia = inertia;
    }
}

}