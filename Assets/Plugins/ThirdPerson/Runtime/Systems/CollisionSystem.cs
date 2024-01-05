using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
        var a = next.Force * delta;
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
        next.Surfaces = frame.Surfaces.ToArrayOrNull();

        // given next surface
        var nextGround = next.GroundSurface;

        // find the last relevant touched surface; if the newest surface is different, use that
        var currSurface = curr.CurrSurface;
        if (nextGround.IsSome && currSurface.Normal != nextGround.Normal) {
            currSurface = nextGround;
        }

        next.CurrSurface = currSurface;

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
        var inertia = frame.Inertia;
        var inertiaDir = inertia.normalized;
        var inertiaMag = inertia.magnitude;

        // find the surface we touched before the new surface, if any
        // var prevSurface = curr.PrevSurface;
        // var prevDotMain = Vector3.Dot(
        //     curr.MainSurface.Normal,
        //     next.MainSurface.Normal
        // );
        //
        // if (prevDotMain - 1f < -0.00001f) {
        //     var normalDelta = Vector3.Dot(prevSurface.Normal, curr.MainSurface.Normal);
        //
        //     if (Mathf.Abs(normalDelta - 1f) > 0.001f) {
        //         prevSurface = curr.MainSurface;
        //     }
        // }
        //
        // next.PrevSurface = prevSurface;

        // remove acceleration into surface (unrealized) from inertia & prevent
        // inversion of direction
        inertia -= inertiaDir * Mathf.Clamp(Vector3.Dot(a, inertiaDir), 0f, inertiaMag);

        next.Inertia = inertia;

        // build a virtual main surface
        var nextMainSurface = CharacterCollision.None;
        if (next.IsColliding) {
            // by default, weight all the surfaces
            var n = frame.Surfaces.Count;
            foreach (var surface in frame.Surfaces) {
                nextMainSurface.Point += surface.Point / n;
                nextMainSurface.Normal += surface.Normal;
            }

            // if inertia is nonzero, use that as the surface normal
            if (inertia != Vector3.zero) {
                nextMainSurface.Normal = -inertia;
            }

            // use inertia w/ acceleration for normal force
            nextMainSurface.Normal = nextMainSurface.Normal.normalized;
            nextMainSurface.NormalMag = frame.Inertia.magnitude;
        }

        next.MainSurface = nextMainSurface;

        // debug curr surfaces (the ones relevant to the surface system)
        DebugDraw.Push(
            "surface-main",
            next.MainSurface.IsSome ? next.MainSurface.Point : next.Position,
            next.MainSurface.Normal,
            new DebugDraw.Config(Color.blue)
        );
    }
}

}