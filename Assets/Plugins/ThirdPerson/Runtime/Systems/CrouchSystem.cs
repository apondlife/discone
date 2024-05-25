using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState CrouchState;
    }
}

/// how crouch affects friction
[Serializable]
sealed class CrouchSystem: CharacterSystem {
    // -- System --
    protected override Phase<CharacterContainer> InitInitialPhase() {
        return NotCrouching;
    }

    protected override SystemState State {
        get => c.State.Next.CrouchState;
        set => c.State.Next.CrouchState = value;
    }

    // -- NotCrouching --
    static readonly Phase<CharacterContainer> NotCrouching = new("NotCrouching",
        enter: (_, c) => {
            // stop crouching
            c.State.IsCrouching = false;

            // reset friction
            c.State.Next.Surface_Drag = c.Tuning.Friction_SurfaceDrag;
            c.State.Next.Surface_KineticFriction = c.Tuning.Friction_Kinetic;
            c.State.Next.Surface_StaticFriction = c.Tuning.Friction_Static;

        },
        update: (_, s, c) => {
            // reset friction every frame in debug
            // TODO: doing this every frame in the build right now bc we don't have
            // a good way to initialize frames from tuning and/or split up network
            // state from client state
            c.State.Next.Surface_Drag = c.Tuning.Friction_SurfaceDrag;
            c.State.Next.Surface_KineticFriction = c.Tuning.Friction_Kinetic;
            c.State.Next.Surface_StaticFriction = c.Tuning.Friction_Static;

            // switch to crouching on input
            if (c.State.Next.IsColliding && c.Inputs.IsCrouchPressed) {
                s.ChangeTo(Crouching);
                return;
            }
        }
    );

    // -- Crouching --
    static readonly Phase<CharacterContainer> Crouching = new("Crouching",
        enter: (_, c) => {
            // start crouching
            c.State.IsCrouching = true;

            // increase static friction on crouch
            c.State.Next.Surface_StaticFriction = c.Tuning.Crouch_StaticFriction;

            // and store the crouch direction, the character won't reface for the
            // duration of the crouch (this is implemented in (coupled to) the
            // movement system)
            c.State.Next.CrouchDirection = c.State.IsStopped
                ? c.State.Curr.Forward
                : c.State.Curr.PlanarVelocity.normalized;
        },
        update: (_, s, c) => {
            // if airborne or if crouch is released, end crouch
            if (!c.State.Next.IsColliding || !c.Inputs.IsCrouchPressed) {
                s.ChangeTo(NotCrouching);
                return;
            }

            // update crouch direction if it changes significantly (> 90°)
            var moveDir = c.State.Curr.SurfaceVelocity.normalized;
            var moveDotCrouch = Vector3.Dot(moveDir, c.State.Curr.CrouchDirection);
            if (moveDotCrouch < 0.0f) {
                c.State.Next.CrouchDirection = moveDir;
            }

            // check alignment between input and crouch
            var crouchDir = c.State.Next.CrouchDirection;
            var inputDir = c.Inputs.Move;
            var inputDotCrouch = Vector3.Dot(inputDir, crouchDir);

            // if we're stopped and change direction, change crouch direction
            if (c.State.IsStopped && inputDotCrouch < 0.0f) {
                c.State.Next.CrouchDirection = inputDir;
            }

            // if the input is not in the direction of the crouch, we're braking,
            // otherwise, slide.
            var drag = inputDotCrouch <= 0.0f
                ? c.Tuning.Crouch_NegativeDrag
                : c.Tuning.Crouch_PositiveDrag;

            c.State.Next.Surface_Drag = drag.Evaluate(Mathf.Abs(inputDotCrouch));

            var kineticFriction = inputDotCrouch <= 0.0f
                ? c.Tuning.Crouch_NegativeKineticFriction
                : c.Tuning.Crouch_PositiveKineticFriction;

            c.State.Next.Surface_KineticFriction = kineticFriction.Evaluate(Mathf.Abs(inputDotCrouch));

            // apply crouch gravity
            c.State.Next.Force += c.Tuning.Crouch_Acceleration * Vector3.up;
        }
    );
}

}