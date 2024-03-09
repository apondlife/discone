using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState FrictionState;
    }
}

/// how the character moves on the ground & air
[Serializable]
sealed class FrictionSystem: CharacterSystem {
    // -- System --
    protected override Phase InitInitialPhase() {
        return NotOnSurface;
    }

    protected override SystemState State {
        get => c.State.Next.FrictionState;
        set => c.State.Next.FrictionState = value;
    }

    // -- NotOnSurface --
    Phase NotOnSurface => new(
        name: "NotOnSurface",
        update: NotOnSurface_Update
    );

    void NotOnSurface_Update(float delta) {
        if (c.State.Curr.IsColliding) {
            ChangeTo(OnSurface);
            return;
        }

        AddDragAndFriction(c.Tuning.Friction_AerialDrag, 0f, delta);
    }

    // -- OnSurface --
    Phase OnSurface => new(
        name: "OnSurface",
        update: OnSurface_Update
    );

    void OnSurface_Update(float delta) {
        // return to the ground if grounded
        if (!c.State.Curr.IsColliding || c.State.Next.Events.Contains(CharacterEvent.Jump)) {
            ChangeToImmediate(NotOnSurface, delta);
            return;
        }

        var drag = 0f;
        if (!c.State.IsStopped) {
            drag = c.State.Next.Surface_Drag;
        }

        var friction = c.State.Next.Surface_StaticFriction;
        if (!c.State.IsStopped || c.InputQuery.Move.sqrMagnitude > 0f) {
            friction = c.State.Next.Surface_KineticFriction;
        }

        // scale by surface
        var currSurface = c.State.Curr.MainSurface;
        drag *= c.Tuning.Friction_SurfaceDragScale.Evaluate(currSurface.Angle);
        friction *= c.Tuning.Friction_SurfaceFrictionScale.Evaluate(currSurface.Angle);;

        AddDragAndFriction(drag, friction, delta);
    }

    // -- integrate --
    // TODO: consider if this should be integrated somewhere more fundamental (e.g. on state v & a)
    /// integrate velocity delta from all Friction forces
    void AddDragAndFriction(
        float drag,
        float friction,
        float delta
    ) {
        // integrate accelerated velocity
        var v0 = c.State.Next.SurfaceVelocity;
        var a0 = c.State.Next.SurfaceForce;
        var va = v0 + a0 * delta;

        // integrated deceleration opposing va
        var deceleration = va.normalized * (friction + drag * va.sqrMagnitude);

        // if deceleration would overcome our accelerated velocity, cancel
        // current velocity instead
        var dv = deceleration * delta;
        if (dv.sqrMagnitude >= va.sqrMagnitude) {
            c.State.Next.Force -= a0 + v0 / delta;
        }
        // otherwise, apply the Friction acceleration
        else {
            c.State.Next.Force -= deceleration;
        }

        // debug drawings
        if (dv.sqrMagnitude >= va.sqrMagnitude) {
            DebugDraw.Push(
                "Friction-stop",
                c.State.Curr.Position,
                a0 + v0 / delta,
                new DebugDraw.Config(Color.black, DebugDraw.Tag.Friction, width: 3f)
            );
        }

        DebugDraw.Push(
            "Friction-va",
            c.State.Curr.Position,
            va,
            new DebugDraw.Config(Color.green, DebugDraw.Tag.Friction)
        );

        DebugDraw.Push(
            "Friction-dv",
            c.State.Curr.Position,
            -dv,
            new DebugDraw.Config(Color.red, DebugDraw.Tag.Friction)
        );
    }
}

}