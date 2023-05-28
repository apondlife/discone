using System;
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
    protected override Phase InitInitialPhase() {
        return NotCrouching;
    }

    protected override SystemState State {
        get => m_State.Next.CrouchState;
        set => m_State.Next.CrouchState = value;
    }

    // -- NotCrouching --
    Phase NotCrouching => new Phase(
        name: "NotCrouching",
        enter: NotCrouching_Enter,
        update: NotCrouching_Update
    );

    void NotCrouching_Enter() {
        // stop crouching
        m_State.IsCrouching = false;

        // reset friction
        m_State.Horizontal_Drag = m_Tuning.Horizontal_Drag;
        m_State.Horizontal_KineticFriction = m_Tuning.Horizontal_KineticFriction;
        m_State.Horizontal_StaticFriction = m_Tuning.Horizontal_StaticFriction;
    }

    void NotCrouching_Update(float delta) {
        // reset friction every frame in debug
        // TODO: doing this every frame in the build right now bc we don't have
        // a good way to initialize frames from tuning and/or split up network
        // state from client state
        m_State.Horizontal_Drag = m_Tuning.Horizontal_Drag;
        m_State.Horizontal_KineticFriction = m_Tuning.Horizontal_KineticFriction;
        m_State.Horizontal_StaticFriction = m_Tuning.Horizontal_StaticFriction;

        // switch to crouching on input
        if (m_State.Next.IsOnGround && m_Input.IsCrouchPressed) {
            ChangeTo(Crouching);
            return;
        }
    }

    // -- Crouching --
    Phase Crouching => new Phase(
        name: "Crouching",
        enter: Crouching_Enter,
        update: Crouching_Update
    );

    void Crouching_Enter() {
        // start crouching
        m_State.IsCrouching = true;

        // increase static friction on crouch
        m_State.Horizontal_StaticFriction = m_Tuning.Crouch_StaticFriction;

        // and store the crouch direction, the character won't reface for the
        // duration of the crouch (this is implemented in (coupled to) the
        // movement system)
        m_State.Next.CrouchDirection = m_State.WasStopped
            ? m_State.Curr.Forward
            : m_State.Curr.PlanarVelocity.normalized;
    }

    void Crouching_Update(float delta) {
        // if airborne or if crouch is released, end crouch
        if (!m_State.Next.IsOnGround || !m_Input.IsCrouchPressed) {
            ChangeTo(NotCrouching);
            return;
        }

        // update crouch direction if it changes significantly (> 90°)
        var moveDir = m_State.Curr.GroundVelocity.normalized;
        var moveDotCrouch = Vector3.Dot(moveDir, m_State.Curr.CrouchDirection);
        if (moveDotCrouch < 0.0f) {
            m_State.Next.CrouchDirection = moveDir;
        }

        // check alignment between input and crouch
        var crouchDir = m_State.Next.CrouchDirection;
        var inputDir = m_Input.Move;
        var inputDotCrouch = Vector3.Dot(inputDir, crouchDir);

        // if we're stopped and change direction, change crouch direction
        if (m_State.IsStopped && inputDotCrouch < 0.0f) {
            m_State.Next.CrouchDirection = inputDir;
        }

        // if the input is not in the direction of the crouch, we're braking,
        // otherwise, slide.
        var drag = inputDotCrouch <= 0.0f
            ? m_Tuning.Crouch_NegativeDrag
            : m_Tuning.Crouch_PositiveDrag;

        m_State.Next.Horizontal_Drag = drag.Evaluate(Mathf.Abs(inputDotCrouch));

        var kineticFriction = inputDotCrouch <= 0.0f
            ? m_Tuning.Crouch_NegativeKineticFriction
            : m_Tuning.Crouch_PositiveKineticFriction;

        m_State.Next.Horizontal_KineticFriction = kineticFriction.Evaluate(Mathf.Abs(inputDotCrouch));

        // apply crouch gravity
        m_State.Next.Velocity += m_Tuning.Crouch_Acceleration * delta * Vector3.up;
    }
}

}