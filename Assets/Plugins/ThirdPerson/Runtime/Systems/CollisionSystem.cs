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
    Phase Active => new(
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

        // sync controller state back to character state
        next.Velocity = frame.Velocity;
        next.Acceleration = (frame.Velocity - curr.Velocity) / delta;
        next.Position = frame.Position;

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
        }

        next.MainSurface = nextMain;

        // inertia, the momentum lost after collision, not including acceleration
        var a0Nrm = Mathf.Max(Vector3.Dot(a0, -nextMain.Normal), 0f) * -nextMain.Normal;
        var v0Mag = (v0 - a0Nrm).magnitude;
        var v1Mag = (v1).magnitude;
        var energy = Mathf.Max(v0Mag * v0Mag - v1Mag * v1Mag, 0f);
        next.Inertia = Mathf.Sqrt(energy);

        // debug drawings
        DebugDraw.Push(
            "surface-main",
            next.MainSurface.IsSome ? next.MainSurface.Point : next.Position,
            next.MainSurface.Normal,
            new DebugDraw.Config(Color.blue, tags: DebugDraw.Tag.Collision)
        );

        DebugDraw.Push(
            "collision-v0",
            curr.Position,
            v0,
            new DebugDraw.Config(tags: DebugDraw.Tag.Collision)
        );

        DebugDraw.Push(
            "collision-i0",
            curr.Position,
            i0,
            new DebugDraw.Config(tags: DebugDraw.Tag.Collision)
        );
    }
}

}