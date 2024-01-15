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
    Phase NotMoving => new(
        name: "NotMoving",
        update: NotMoving_Update
    );

    void NotMoving_Update(float delta) {
        // start floating if no longer grounded
        if (!c.State.Next.IsOnGround) {
            ChangeTo(Floating);
            return;
        }

        // integrate forces
        AddDragAndFriction(delta);

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
    Phase Moving => new(
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
        var v = c.State.Next.SurfaceVelocity;

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

        // integrate forces
        // 22.10.26: removed static friction when in moving
        c.State.Next.Force += c.Tuning.Horizontal_Acceleration * Vector3.Project(inputDir, c.State.Next.Forward);
        AddDragAndFriction(delta);
    }

    // -- Sliding --
    Phase Sliding => new(
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
        var moveVel = c.State.Next.SurfaceVelocity;
        var moveDir = moveVel.normalized;
        var moveMag = moveVel.magnitude;
        var scaleLateral = moveMag * (1.0f - Mathf.Abs(Vector3.Angle(moveDir, slideDir) / 90.0f));

        // calculate lateral thrust
        var thrustLateral = scaleLateral * c.Tuning.Crouch_LateralMaxSpeed * inputSlideLateral;

        // integrate forces
        c.State.Next.Force += c.Tuning.Horizontal_Acceleration * thrustLateral;
        AddDragAndFriction(delta);

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
    Phase Pivot => new(
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
        var v0 = c.State.Curr.SurfaceVelocity;
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
    Phase Floating => new(
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
    void AddDragAndFriction(float delta) {
        // TODO: aerial drag
        var drag = 0f;
        if (!c.State.IsStopped) {
            drag = c.Tuning.Horizontal_Drag;
        }

        var friction = c.State.Horizontal_StaticFriction;
        if (!c.State.IsStopped || c.Input.Move.sqrMagnitude > 0f) {
            friction = c.State.Horizontal_KineticFriction;
        }

        // get curr surface
        var surface = c.State.Curr.MainSurface;

        // integrate accelerated velocity
        var v0 = c.State.Next.SurfaceVelocity;
        var a0 = Vector3.ProjectOnPlane(c.State.Next.Force, surface.Normal);
        var va = v0 + a0 * delta;

        // scale friction by surface
        var frictionScale = c.Tuning.Surface_FrictionScale.Evaluate(surface.Angle);
        friction *= frictionScale;

        // integrated deceleration opposing va
        var deceleration = va.normalized * (friction + drag * va.sqrMagnitude);

        // if deceleration would overcome our accelerated velocity, cancel
        // current velocity instead
        var dv = deceleration * delta;
        if (dv.sqrMagnitude >= va.sqrMagnitude) {
            c.State.Next.Force -= a0 + v0 / delta;
        }
        // otherwise, apply the movement acceleration
        else {
            c.State.Next.Force -= deceleration;
        }

        // debug drawings
        if (dv.sqrMagnitude >= va.sqrMagnitude) {
            DebugDraw.Push(
                "movement-stop",
                c.State.Curr.Position,
                a0 + v0 / delta,
                new DebugDraw.Config(Color.black, DebugDraw.Tag.Movement, width: 3f)
            );
        }

        DebugDraw.Push(
            "movement-va",
            c.State.Curr.Position,
            va,
            new DebugDraw.Config(Color.green, DebugDraw.Tag.Movement)
        );

        DebugDraw.Push(
            "movement-dv",
            c.State.Curr.Position,
            -dv,
            new DebugDraw.Config(Color.red, DebugDraw.Tag.Movement)
        );
    }
}

}