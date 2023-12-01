using System;
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
    protected override Phase InitInitialPhase() {
        return NotMoving;
    }

    protected override SystemState State {
        get => c.State.Next.MovementState;
        set => c.State.Next.MovementState = value;
    }

    // -- NotMoving --
    Phase NotMoving => new Phase(
        name: "NotMoving",
        update: NotMoving_Update
    );

    void NotMoving_Update(float delta) {
        // start floating if no longer grounded
        if (!c.State.Next.IsOnGround) {
            ChangeTo(Floating);
            return;
        }

        // intergrate forces
        IntegrateForces(
            c.State.Next.GroundVelocity,
            Vector3.zero,
            c.State.WasStopped ? 0.0f : c.State.Next.Horizontal_Drag,
            c.State.WasStopped ? c.State.Next.Horizontal_StaticFriction : c.State.Next.Horizontal_KineticFriction,
            delta
        );


        // we're moving if the character is not stopped or if there's input
        var shouldStartMoving = !c.State.IsStopped || HasMoveInput;

        // change to sliding if moving & crouching
        if (shouldStartMoving && c.State.IsCrouching) {
            ChangeTo(Sliding);
            return;
        }

        // change to moving once moving
        if (shouldStartMoving) {
            ChangeTo(Moving);
            return;
        }
    }

    // -- Moving --
    Phase Moving => new Phase(
        name: "Moving",
        update: Moving_Update
    );

    void Moving_Update(float delta) {
        // start floating if no longer grounded
        if (!c.State.Next.IsOnGround) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        // we're moving if the character is not stopped or if there's input
        var shouldStopMoving = c.State.IsStopped && !HasMoveInput;

        // once speed is zero, stop moving
        if (shouldStopMoving) {
            ChangeToImmediate(NotMoving, delta);
            return;
        }

        // if crouching, change to sliding
        if (c.State.Next.IsCrouching) {
            ChangeToImmediate(Sliding, delta);
            return;
        }

        // the current ground velocity
        var v = c.State.Next.GroundVelocity;

        // the current forward & input direction
        var fwd = c.State.Curr.Forward;
        var inputDir = c.Input.Move;
        var inputDotFwd = Vector3.Dot(inputDir, fwd);

        // pivot if direction change was significant
        var shouldPivot = (
            inputDotFwd < c.Tuning.PivotStartThreshold &&
            v.sqrMagnitude > c.Tuning.PivotSqrSpeedThreshold
        );

        if (shouldPivot) {
            ChangeToImmediate(Pivot, delta);
            return;
        }

        // turn towards input direction
        TurnTowards(
            c.Input.Move,
            c.Tuning.TurnSpeed,
            delta
        );

        // intergrate forces
        // 22.10.26: removed static friction when in moving
        IntegrateForces(
            v,
            c.Tuning.Horizontal_Acceleration * Vector3.Project(inputDir, c.State.Next.Forward),
            c.State.WasStopped ? 0.0f : c.State.Horizontal_Drag,
            c.State.Horizontal_KineticFriction,
            delta
        );
    }

    // -- Sliding --
    Phase Sliding => new Phase(
        name: "Sliding",
        update: Sliding_Update
    );

    void Sliding_Update(float delta) {
        // start floating if no longer grounded
        if (!c.State.Next.IsOnGround) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        // get current forward & input direction
        var slideDir = c.State.Next.CrouchDirection;
        var inputDir = c.Input.Move;

        // split the input axis
        var inputDotSlide = Vector3.Dot(inputDir, slideDir);
        var inputSlide = slideDir * inputDotSlide;
        var inputSlideLateral = inputDir - inputSlide;

        // make lateral thrust proportional to speed in slide direction
        var moveVel = c.State.Next.GroundVelocity;
        var moveDir = moveVel.normalized;
        var moveMag = moveVel.magnitude;
        var scaleLateral = moveMag * (1.0f - Mathf.Abs(Vector3.Angle(moveDir, slideDir) / 90.0f));

        // calculate lateral thrust
        var thrustLateral = scaleLateral * c.Tuning.Crouch_LateralMaxSpeed * inputSlideLateral;

        // integrate forces
        IntegrateForces(
            c.State.Next.GroundVelocity,
            c.Tuning.Horizontal_Acceleration * thrustLateral,
            c.State.WasStopped ? 0.0f : c.State.Horizontal_Drag,
            c.State.WasStopped ? c.State.Horizontal_StaticFriction : c.State.Horizontal_KineticFriction,
            delta
        );

        // turn towards input direction
        TurnTowards(
            c.Input.Move,
            c.Tuning.Crouch_TurnSpeed,
            delta
        );

        // we're moving if the character is not stopped or if there's input
        var shouldStopMoving = c.State.IsStopped && !HasMoveInput;

        // once not crouching change to move/not move state
        if (!c.State.IsCrouching) {
            ChangeTo(shouldStopMoving ? NotMoving : Moving);
            return;
        }

        // once speed is zero, stop moving
        if (shouldStopMoving) {
            ChangeTo(NotMoving);
            return;
        }
    }

    // -- Pivot --
    Phase Pivot => new Phase(
        "Pivot",
        enter: Pivot_Enter,
        update: Pivot_Update,
        exit: Pivot_Exit
    );

    void Pivot_Enter() {
        c.State.Next.PivotDirection = c.Input.Move;
        c.State.Next.PivotFrame = 0;
    }

    void Pivot_Update(float delta) {
        if (!c.State.Next.IsOnGround) {
            ChangeToImmediate(Floating, delta);
            return;
        }

        c.State.Next.PivotFrame += 1;

        // rotate towards pivot direction
        TurnTowards(
            c.State.Curr.PivotDirection,
            c.Tuning.PivotSpeed,
            delta
        );

        // calculate next velocity, decelerating towards zero to finish pivot
        var v0 = c.State.Curr.GroundVelocity;
        var a = Mathf.Min(v0.magnitude / delta, c.Tuning.PivotDeceleration) * v0.normalized;

        // update velocity
        c.State.Next.Force -= a;

        // once speed is zero, transition to next state
        if (c.State.IsStopped) {
            ChangeTo(HasMoveInput ? Moving : NotMoving);
            return;
        }
    }

    void Pivot_Exit() {
        c.State.Next.PivotFrame = -1;
    }

    // -- Floating --
    Phase Floating => new Phase(
        name: "Floating",
        update: Floating_Update
    );

    void Floating_Update(float delta) {
        // return to the ground if grounded
        if (c.State.Next.IsOnGround) {
            ChangeToImmediate(c.State.IsCrouching ? Sliding : Moving, delta);
            return;
        }

        // rotate towards input direction
        // TODO: this should be a discone feature, not a third person one
        // the ability to modify tuning at run time
        if (c.Input.IsCrouchPressed) {
            TurnTowards(
                c.Input.Move,
                c.Tuning.Air_TurnSpeed,
                delta
            );
        }

        // add aerial drift
        var v0 = c.State.Curr.PlanarVelocity;
        var a = c.Input.Move * c.Tuning.AerialDriftAcceleration;
        c.State.Next.Force += a;
    }

    // -- commands --
    /// turn the character towards the direction by turn speed; impure command
    void TurnTowards(Vector3 direction, float turnSpeed, float delta) {
        // if no direction, do nothing
        if (direction.sqrMagnitude <= 0.0f) {
            return;
        }

        // rotate towards input
        var fwd = Vector3.RotateTowards(
            c.State.Curr.Forward,
            c.Input.Move,
            turnSpeed * Mathf.Deg2Rad * delta,
            Mathf.Infinity
        );

        // project current direction
        c.State.Next.SetProjectedForward(fwd);
    }

    // -- queries --
    /// if there is any user input
    bool HasMoveInput {
        get => c.Input.Move.sqrMagnitude > 0.0f;
    }

    // -- integrate --
    // TODO: consider if this should be integrated somewhere more fundamental (e.g. on state v & a)
    /// integrate velocity delta from all movement forces
    void IntegrateForces(
        Vector3 v0,
        Vector3 thrust,
        float drag,
        float friction,
        float delta
    ) {
        // get curr velocity dir & mag
        var v0Dir = v0.normalized;
        var v0SqrMag = v0.sqrMagnitude;

        // scale friction by surface
        var frictionScale = c.Tuning.Surface_FrictionScale.Evaluate(c.State.Curr.GroundSurface.Angle);
        friction *= frictionScale;

        // get separate acceleration and deceleration
        var acceleration = thrust;
        var deceleration = v0Dir * (friction + drag * v0SqrMag);

        // get velocity delta in each direction
        var dva = acceleration * delta;
        var dvd = deceleration * delta;

        // if deceleration would overcome our accelerated velocity, cancel
        // current velocity instead
        var va = v0 + dva;
        if (dvd.sqrMagnitude >= va.sqrMagnitude) {
            c.State.Next.Force -= v0 / delta;
        }
        // otherwise, apply the movement acceleration
        else {
            c.State.Next.Force += acceleration - deceleration;
        }
    }
}

}