using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState MovementState;
    }
}

/// how the character moves on the ground & air
[Serializable]
sealed class MovementSystem: CharacterSystem {
    // -- System --
    protected override Phase<CharacterContainer> InitInitialPhase() {
        return NotMoving;
    }

    protected override SystemState State {
        get => c.State.Next.MovementState;
        set => c.State.Next.MovementState = value;
    }

    public override void Init(CharacterContainer c) {
        // init tuning into state
        c.State.Next.Surface_Drag = c.Tuning.Friction_SurfaceDrag;
        c.State.Next.Surface_KineticFriction = c.Tuning.Friction_Kinetic;
        c.State.Next.Surface_StaticFriction = c.Tuning.Friction_Static;

        base.Init(c);
    }

    // -- NotMoving --
    static readonly Phase<CharacterContainer> NotMoving = new("NotMoving",
        update: NotMoving_Update
    );

    static void NotMoving_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // start floating if no longer grounded
        if (!c.State.Next.IsOnGround) {
            s.ChangeTo(Floating);
            return;
        }

        // change to sliding if moving & crouching
        var shouldStartMoving = ShouldStartMoving(c);
        if (shouldStartMoving && c.State.Next.IsCrouching) {
            s.ChangeTo(Sliding);
            return;
        }

        // change to moving once moving
        if (shouldStartMoving) {
            s.ChangeTo(Moving);
            return;
        }
    }

    // -- Moving --
    static readonly Phase<CharacterContainer> Moving = new("Moving",
        update: Moving_Update
    );

    static void Moving_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // start floating if no longer grounded
        if (!c.State.Next.IsOnGround) {
            s.ChangeToImmediate(Floating, delta);
            return;
        }

        // once speed is zero, stop moving
        var shouldStopMoving = !ShouldStartMoving(c);
        if (shouldStopMoving) {
            s.ChangeToImmediate(NotMoving, delta);
            return;
        }

        // if crouching, change to sliding
        if (c.State.Next.IsCrouching) {
            s.ChangeToImmediate(Sliding, delta);
            return;
        }

        // the current ground velocity
        var v = c.State.Next.SurfaceVelocity;

        // the current forward & input direction
        var inputDir = c.Inputs.Move;
        var inputAngle = Vector3.Angle(inputDir, c.State.Curr.Forward);

        // pivot if direction change was significant
        var shouldPivot = (
            inputAngle > c.Tuning.PivotStartThreshold &&
            v.sqrMagnitude > c.Tuning.PivotSqrSpeedThreshold
        );

        if (shouldPivot) {
            s.ChangeToImmediate(Pivot, delta);
            return;
        }

        // turn towards input direction
        TurnTowards(c.Inputs.Move, c.Tuning.TurnSpeed, delta, c);

        // add movement force
        var direction = Vector3.Project(inputDir, c.State.Next.Forward);
        var acceleration = c.Tuning.Surface_Acceleration.Evaluate(c.State.Curr.MainSurface.Angle);
        c.State.Next.Force += acceleration * direction;
    }

    // -- Sliding --
    static readonly Phase<CharacterContainer> Sliding = new("Sliding",
        enter: Sliding_Enter,
        update: Sliding_Update,
        exit: Sliding_Exit
    );

    static void Sliding_Enter(System<CharacterContainer> s, CharacterContainer c) {
        // increase static friction on crouch
        c.State.Next.Surface_StaticFriction = c.Tuning.Crouch_StaticFriction;
    }

    static void Sliding_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // start floating if no longer grounded
        if (!c.State.Next.IsOnGround) {
            s.ChangeToImmediate(Floating, delta);
            return;
        }

        // once speed is zero, stop moving
        if (!ShouldStartMoving(c)) {
            s.ChangeToImmediate(NotMoving, delta);
            return;
        }

        // once not crouching change to move/not move state
        if (!c.State.Next.IsCrouching) {
            s.ChangeToImmediate(Moving, delta);
            return;
        }

        // get directions
        var moveDir = c.State.Curr.PlanarDirection;
        var inputDir = c.Inputs.Move;

        // get power
        var power = c.Tuning.Crouch_Power.Evaluate(s.PhaseElapsed);

        // check alignment between input and crouch
        var inputAngle = Vector3.Angle(inputDir, moveDir);
        var inputDotCrouch = Mathf.Cos(inputAngle * Mathf.Deg2Rad);
        var inputMag = Mathf.Abs(inputDotCrouch);

        // if the input is not in the direction of the crouch, we're braking,
        // otherwise, slide.
        var drag = inputDotCrouch <= 0.0f
            ? c.Tuning.Crouch_NegativeDrag
            : c.Tuning.Crouch_PositiveDrag;

        var nextDrag = Mathf.LerpUnclamped(
            c.Tuning.Friction_SurfaceDrag,
            drag.Evaluate(inputMag),
            power
        );

        var kineticFriction = inputDotCrouch <= 0.0f
            ? c.Tuning.Crouch_NegativeKineticFriction
            : c.Tuning.Crouch_PositiveKineticFriction;

        var nextKineticFriction = Mathf.LerpUnclamped(
            c.Tuning.Friction_Kinetic,
            kineticFriction.Evaluate(inputMag),
            power
        );

        // split the input axis (can't do acos of dot product since it can be greater
        // than 1)
        var inputDirInline = inputDotCrouch * moveDir;
        var inputDirCross = inputDir - inputDirInline;

        // scale input inline & cross as a fn of input x move angle
        var inputInline = c.Tuning.Crouch_InlineScale.Evaluate(inputAngle) * inputDirInline;
        var inputCross = c.Tuning.Crouch_CrossScale.Evaluate(inputAngle) * inputDirCross;

        // NOTE: should this be lerped?
        var inputSlide = inputInline + inputCross;

        // add movement force
        var acceleration = c.Tuning.Surface_Acceleration.Evaluate(c.State.Curr.MainSurface.Angle);
        var force = acceleration * inputSlide;

        // update state
        c.State.Next.Force += force;
        c.State.Next.Surface_Drag = nextDrag;
        c.State.Next.Surface_KineticFriction = nextKineticFriction;

        // turn towards input direction
        TurnTowards(c.Inputs.Move, c.Tuning.Crouch_TurnSpeed, delta, c);
    }

    static void Sliding_Exit(System<CharacterContainer> s, CharacterContainer c) {
        // reset friction
        c.State.Next.Surface_Drag = c.Tuning.Friction_SurfaceDrag;
        c.State.Next.Surface_KineticFriction = c.Tuning.Friction_Kinetic;
        c.State.Next.Surface_StaticFriction = c.Tuning.Friction_Static;
    }

    // -- Pivot --
    static readonly Phase<CharacterContainer> Pivot = new("Pivot",
        enter: Pivot_Enter,
        update: Pivot_Update
    );

    static void Pivot_Enter(System<CharacterContainer> _, CharacterContainer c) {
        c.State.Next.PivotDirection = c.Inputs.Move;
    }

    static void Pivot_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        if (!c.State.Next.IsOnGround) {
            s.ChangeToImmediate(Floating, delta);
            return;
        }

        // cancel pivot on crouch/jump
        if (c.State.Curr.IsCrouching) {
            s.ChangeTo(Sliding);
            return;
        }

        // rotate towards pivot direction
        TurnTowards(c.State.Curr.PivotDirection, c.Tuning.PivotSpeed, delta, c);

        // calculate next velocity, decelerating towards zero to finish pivot
        var v0 = c.State.Curr.SurfaceVelocity;
        var a = Mathf.Min(v0.magnitude / delta, c.Tuning.PivotDeceleration) * v0.normalized;

        // update velocity
        c.State.Next.Force -= a;

        // once speed is zero or the forward and pivot are aligned, transition to next state
        var angle = Vector3.Angle(c.State.Next.Forward, c.State.Curr.PivotDirection);
        if (c.State.IsStopped || angle < c.Tuning.PivotEndThreshold) {
            s.ChangeTo(HasMoveInput(c) ? Moving : NotMoving);
            return;
        }
    }

    // -- Floating --
    static readonly Phase<CharacterContainer> Floating = new("Floating",
        update: Floating_Update
    );

    static void Floating_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        // return to the ground if grounded
        if (c.State.Next.IsOnGround) {
            s.ChangeToImmediate(c.State.Next.IsCrouching ? Sliding : Moving, delta);
            return;
        }

        // rotate towards input direction
        // TODO: this should be a discone feature, not a third person one
        // the ability to modify tuning at run time
        // TODO: reevaluate
        if (c.Inputs.IsJumpPressed) {
            TurnTowards(c.Inputs.Move, c.Tuning.Air_TurnSpeed, delta, c);
        }

        // add aerial drift
        var a = c.Inputs.Move * c.Tuning.AerialDriftAcceleration;
        c.State.Next.Force += a;
    }

    // -- commands --
    /// turn the character towards the direction by turn speed; impure command
    static void TurnTowards(Vector3 direction, float turnSpeed, float delta, CharacterContainer c) {
        // if no direction, do nothing
        if (direction.sqrMagnitude <= 0.0f) {
            return;
        }

        // rotate towards input
        var fwd = Vector3.RotateTowards(
            c.State.Curr.Forward,
            c.Inputs.Move,
            turnSpeed * Mathf.Deg2Rad * delta,
            Mathf.Infinity
        );

        // project current direction
        c.State.Next.SetProjectedForward(fwd);
    }

    // -- queries --
    /// if there is any user input
    static bool HasMoveInput(CharacterContainer c) {
        return c.Inputs.Move.sqrMagnitude > 0.0f;
    }

    // we're supposed to start moving if the character is not stopped or if there's input
    static bool ShouldStartMoving(CharacterContainer c) {
        return !c.State.IsStopped || HasMoveInput(c);
    }
}

}