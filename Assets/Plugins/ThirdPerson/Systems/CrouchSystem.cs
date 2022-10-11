using System;
using UnityEngine;

namespace ThirdPerson {

/// how crouch affects friction
[Serializable]
sealed class CrouchSystem: CharacterSystem {
    // -- lifetime --
    protected override Phase InitInitialPhase() {
        return NotCrouching;
    }

    // -- props --
    /// the crouch's initial direction
    Vector3 m_DirCrouch;

    // -- NotCrouching --
    Phase NotCrouching => new Phase(
        name: "NotCrouching",
        enter: NotCrouching_Enter,
        update: NotCrouching_Update
    );

    void NotCrouching_Enter() {
        m_State.IsCrouching = false;

        // reset friction
        m_State.Horizontal_KineticFriction = m_Tunables.Horizontal_KineticFriction;
        m_State.Horizontal_StaticFriction = m_Tunables.Horizontal_StaticFriction;
    }

    void NotCrouching_Update(float delta) {
        if (m_State.IsGrounded && m_Input.IsCrouchPressed) {
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
        m_State.IsCrouching = true;

        // increase static friction on crouch
        m_State.Horizontal_StaticFriction = m_Tunables.Crouch_StaticFriction;

        // and store the crouch direction, the character won't reface for the
        // duration of the crouch (this is implemented in (coupled to) the
        // movement system)
        if (IsStopped) {
            m_DirCrouch = m_State.Forward;
        } else {
            m_DirCrouch = m_State.Curr.GroundVelocity.normalized;
        }
    }

    void Crouching_Update(float delta) {
        // if airborne or if crouch is released, end crouch
        if (!m_State.IsGrounded || !m_Input.IsCrouchPressed) {
            ChangeTo(NotCrouching);
            return;
        }

        // get input and crouch direction
        var dirInput = m_Input.Move;
        var dirCrouch = m_DirCrouch;

        // check alignment between them
        var inputDotCrouch = Vector3.Dot(dirInput, dirCrouch);

        // if the input is not in the direction of the crouch, we're braking;
        // otherwise, slide.
        var curve = inputDotCrouch <= 0.0f
            ? m_Tunables.Crouch_Brake_KineticFriction
            : m_Tunables.Crouch_Slide_KineticFriction;

        m_State.Horizontal_KineticFriction = curve.Evaluate(Mathf.Abs(inputDotCrouch));
    }

    // -- queries --
    /// if the ground speed is below the movement threshold
    bool IsStopped {
        get => m_State.Curr.GroundVelocity.magnitude < m_Tunables.Horizontal_MinSpeed;
    }
}

}