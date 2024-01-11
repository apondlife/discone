using System;
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
        var a0 = next.Force * delta;

        // reapply any accumulated inertia
        var i0 = next.Inertia * -curr.MainSurface.Normal;

        // add inertia & acceleration to velocity
        var v0 = next.Velocity + i0 + a0;

        // move character using controller if not idle
        var frame = c.Controller.Move(
            next.Position,
            v0,
            next.Up,
            delta
        );

        // update collisions
        next.Surfaces = frame.Surfaces.ToArrayOrNull();

        // move the perceived surface towards the current surface
        var perceivedNormal = curr.PerceivedSurface.Normal;
        if (curr.PerceivedSurface.IsNone) {
            perceivedNormal = next.MainSurface.Normal;
        }

        // TODO: maybe update the time since last touching the curr surface
        next.PerceivedSurface.SetNormal(Vector3.RotateTowards(
            perceivedNormal,
            next.MainSurface.Normal,
            c.Tuning.Surface_PerceptionAngularSpeed * Mathf.Deg2Rad * delta,
            0f
        ));

        // TODO: can we do anything about this?
        next.PerceivedSurface.Point = Vector3.negativeInfinity;

        // sync controller state back to character state
        next.Velocity = frame.Velocity;
        next.Acceleration = (frame.Velocity - curr.Velocity) / delta;
        next.Position = frame.Position;

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

        // the change in velocity, the resultant surface from the collision
        var v1 = frame.Velocity;
        var dv = v0 - v1;

        // build a virtual main surface
        var nextMain = CharacterCollision.None;
        if (next.IsColliding) {
            var nextNormal = Vector3.zero;

            // by default, weight all the surfaces
            var n = frame.Surfaces.Count;
            foreach (var surface in frame.Surfaces) {
                nextMain.Point += surface.Point / n;
                nextNormal += surface.Normal;
            }

            // if dv is nonzero, use that as the surface normal
            if (dv != Vector3.zero) {
                nextNormal = -dv;
            }

            // update the normal & angle
            nextMain.SetNormal(nextNormal.normalized);

            DebugDraw.Push(
                "velocity-main",
                nextMain.Point,
                Vector3.Project(v0 - a0 - i0, nextMain.Normal)
            );

            DebugDraw.Push(
                "acceleration-main",
                nextMain.Point,
                Vector3.Project(a0, nextMain.Normal)
            );

            DebugDraw.Push(
                "inertia-main",
                nextMain.Point,
                Vector3.Project(i0, nextMain.Normal)
            );
        }

        next.MainSurface = nextMain;

        // inertia, the momentum lost after collision, not including acceleration
        var aNormal = Mathf.Max(Vector3.Dot(a0, -nextMain.Normal), 0f) * -nextMain.Normal;
        var inertia = (v0 - aNormal).magnitude - v1.magnitude;
        next.Inertia = Math.Max(inertia, 0f);

        DebugDraw.Push(
            "collision-v0",
            next.Position,
            v0 + Mathf.Max(Vector3.Dot(a0, -nextMain.Normal), 0f) * nextMain.Normal
        );

        DebugDraw.Push(
            "collision-v1",
            next.Position,
            v1
        );

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