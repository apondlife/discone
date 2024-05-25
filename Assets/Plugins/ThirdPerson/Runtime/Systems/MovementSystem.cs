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

    // -- NotMoving --
    static readonly Phase<CharacterContainer> NotMoving = new("NotMoving",
        update: (delta, s, c) => {
            // start floating if no longer grounded
            if (!c.State.Next.IsOnGround) {
                s.ChangeTo(Floating);
                return;
            }

            // we're moving if the character is not stopped or if there's input
            var shouldStartMoving = !c.State.IsStopped || HasMoveInput(c);

            // change to sliding if moving & crouching
            if (shouldStartMoving && c.State.IsCrouching) {
                s.ChangeTo(Sliding);
                return;
            }

            // change to moving once moving
            if (shouldStartMoving) {
                s.ChangeTo(Moving);
                return;
            }
        }
    );

    // -- Moving --
    static readonly Phase<CharacterContainer> Moving = new("Moving",
        update: (delta, s, c) => {
            // start floating if no longer grounded
            if (!c.State.Next.IsOnGround) {
                s.ChangeToImmediate(Floating, delta);
                return;
            }

            // we're moving if the character is not stopped or if there's input
            var shouldStopMoving = c.State.IsStopped && !HasMoveInput(c);

            // once speed is zero, stop moving
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
            var fwd = c.State.Curr.Forward;
            var inputDir = c.Inputs.Move;
            var inputDotFwd = Vector3.Dot(inputDir, fwd);

            // pivot if direction change was significant
            var shouldPivot = (
                inputDotFwd < c.Tuning.PivotStartThreshold &&
                v.sqrMagnitude > c.Tuning.PivotSqrSpeedThreshold
            );

            if (shouldPivot) {
                s.ChangeToImmediate(Pivot, delta);
                return;
            }

            // turn towards input direction
            TurnTowards(
                c.Inputs.Move,
                c.Tuning.TurnSpeed,
                delta,
                c
            );

            // add movement force
            var direction = Vector3.Project(inputDir, c.State.Next.Forward);
            var acceleration = c.Tuning.Surface_Acceleration.Evaluate(c.State.Curr.MainSurface.Angle);
            c.State.Next.Force += acceleration * direction;
        }
    );

    // -- Sliding --
    static readonly Phase<CharacterContainer> Sliding = new("Sliding",
        update: (delta, s, c) => {
            // start floating if no longer grounded
            if (!c.State.Next.IsOnGround) {
                s.ChangeToImmediate(Floating, delta);
                return;
            }

            // get current forward & input direction
            var slideDir = c.State.Next.CrouchDirection;
            var inputDir = c.Inputs.Move;

            // split the input axis
            var inputDotSlide = Vector3.Dot(inputDir, slideDir);
            var inputSlide = slideDir * inputDotSlide;
            var inputSlideLateral = inputDir - inputSlide;

            // make lateral thrust proportional to speed in slide direction
            var moveVel = c.State.Next.SurfaceVelocity;
            var moveDir = moveVel.normalized;
            var moveMag = moveVel.magnitude;
            var scaleLateral = moveMag * (1.0f - Mathf.Abs(Vector3.Angle(moveDir, slideDir) / 90.0f));

            // add lateral movement
            var moveLateral = scaleLateral * c.Tuning.Crouch_LateralMaxSpeed * inputSlideLateral;
            c.State.Next.Force += c.Tuning.Surface_Acceleration.Evaluate(c.State.Curr.MainSurface.Angle) * moveLateral;

            // turn towards input direction
            TurnTowards(
                c.Inputs.Move,
                c.Tuning.Crouch_TurnSpeed,
                delta,
                c
            );

            // we're moving if the character is not stopped or if there's input
            var shouldStopMoving = c.State.IsStopped && !HasMoveInput(c);

            // once not crouching change to move/not move state
            if (!c.State.IsCrouching) {
                s.ChangeTo(shouldStopMoving ? NotMoving : Moving);
                return;
            }

            // once speed is zero, stop moving
            if (shouldStopMoving) {
                s.ChangeTo(NotMoving);
                return;
            }
        }
    );

    // -- Pivot --
    static readonly Phase<CharacterContainer> Pivot = new("Pivot",
        enter: (_, c) => {
            c.State.Next.PivotDirection = c.Inputs.Move;
            c.State.Next.PivotFrame = 0;
        },
        update: (delta, s, c) => {
            if (!c.State.Next.IsOnGround) {
                s.ChangeToImmediate(Floating, delta);
                return;
            }

            c.State.Next.PivotFrame += 1;

            // rotate towards pivot direction
            TurnTowards(
                c.State.Curr.PivotDirection,
                c.Tuning.PivotSpeed,
                delta,
                c
            );

            // calculate next velocity, decelerating towards zero to finish pivot
            var v0 = c.State.Curr.SurfaceVelocity;
            var a = Mathf.Min(v0.magnitude / delta, c.Tuning.PivotDeceleration) * v0.normalized;

            // update velocity
            c.State.Next.Force -= a;

            // once speed is zero, transition to next state
            if (c.State.IsStopped) {
                s.ChangeTo(HasMoveInput(c) ? Moving : NotMoving);
                return;
            }
        },
        exit: (s, c) => {
            c.State.Next.PivotFrame = -1;
        }
    );

    // -- Floating --
    static readonly Phase<CharacterContainer> Floating = new("Floating",
        update: (delta, s, c) => {
            // return to the ground if grounded
            if (c.State.Next.IsOnGround) {
                s.ChangeToImmediate(c.State.IsCrouching ? Sliding : Moving, delta);
                return;
            }

            // rotate towards input direction
            // TODO: this should be a discone feature, not a third person one
            // the ability to modify tuning at run time
            if (c.Inputs.IsCrouchPressed) {
                TurnTowards(
                    c.Inputs.Move,
                    c.Tuning.Air_TurnSpeed,
                    delta,
                    c
                );
            }

            // add aerial drift
            var a = c.Inputs.Move * c.Tuning.AerialDriftAcceleration;
            c.State.Next.Force += a;
        }
    );

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
}

}