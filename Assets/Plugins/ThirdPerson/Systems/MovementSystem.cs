using UnityEngine;

namespace ThirdPerson {

/// how the character moves on the ground & air
sealed class MovementSystem: CharacterSystem {
    // -- lifetime --
    public MovementSystem(CharacterData character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return NotMoving;
    }

    // -- NotMoving --
    CharacterPhase NotMoving => new CharacterPhase(
        name: "NotMoving",
        update: NotMoving_Update
    );

    void NotMoving_Update() {
        if (m_Input.MoveAxis.magnitude > 0) {
            ChangeTo(Moving);
            return;
        }

        // start floating if no longer grounded
        if (!m_State.IsGrounded) {
            ChangeTo(Floating);
            return;
        }

        // calculate next velocity, integrating drag
        var v0 = m_State.PlanarVelocity;
        if (v0.sqrMagnitude != 0.0f) {
            var d0 = v0 * m_Tunables.Deceleration;
            var vt = v0 - d0 * Time.deltaTime;

            // update planar velocity
            m_State.Velocity = vt + m_State.Velocity.y * Vector3.up;
        }
    }

    // -- Moving --
    CharacterPhase Moving => new CharacterPhase(
        name: "Moving",
        update: Moving_Update
    );

    void Moving_Update() {
        // start floating if no longer grounded
        if (!m_State.IsGrounded) {
            ChangeTo(Floating);
            return;
        }

        // get current forward & input direction
        var dirForward = m_State.Forward;
        var dirInput = m_Input.MoveAxis;

        // pivot if direction change was significant
        if (Vector3.Dot(dirForward, dirInput) < m_Tunables.PivotStartThreshold) {
            ChangeTo(Pivot);
            return;
        }

        // rotate towards input direction
        var hasInput = dirInput.sqrMagnitude != 0.0f;
        if (hasInput) {
            dirForward = Vector3.RotateTowards(
                m_State.Forward,
                dirInput,
                m_Tunables.TurnSpeed * Mathf.Deg2Rad * Time.deltaTime,
                Mathf.Infinity
            );

            m_State.SetProjectedForward(dirForward);
        }

        // calculate next velocity, integrating input & drag
        // vt = v0 + (input acceleration - drag - turning friction) * t
        var v0 = m_State.PlanarVelocity;
        var ai = m_Tunables.Acceleration * dirInput.magnitude * m_State.Forward;
        var cf = hasInput ? m_Tunables.TurningFriction * (1.0f - Mathf.Abs(Vector3.Dot(v0.normalized, ai.normalized))) : 0.0f;
        var d0 = v0 * (m_Tunables.Deceleration + cf);
        var vt = v0 + (ai - d0) * Time.deltaTime;

        // update planar velocity
        m_State.Velocity = vt + m_State.Velocity.y * Vector3.up;

        // once speed is zero, stop moving
        if(!hasInput && v0.magnitude < m_Tunables.MinPlanarSpeed) {
            ChangeTo(NotMoving);
        }
    }

    // -- Pivot --
    CharacterPhase Pivot => new CharacterPhase(
        "Pivot",
        enter: Pivot_Enter,
        update: Pivot_Update,
        exit: Pivot_Exit
    );

    void Pivot_Enter() {
        m_State.PivotDirection = m_Input.MoveAxis;
        m_State.PivotFrame = 0;
    }

    void Pivot_Update() {
        if (!m_State.IsGrounded) {
            ChangeTo(Floating);
            return;
        }

        m_State.PivotFrame += 1;

        // rotate towards pivot direction
        var dirFacing = m_State.Forward;
        dirFacing = Vector3.RotateTowards(
            m_State.Forward,
            m_State.PivotDirection,
            m_Tunables.PivotSpeed * Mathf.Deg2Rad * Time.deltaTime,
            Mathf.Infinity
        );

        m_State.SetProjectedForward(dirFacing);

        // calculate next velocity, decelerating towards zero to finish pivot
        var v0 = m_State.PlanarVelocity;
        var vt = v0 - Mathf.Min(v0.magnitude, m_Tunables.PivotDeceleration * Time.deltaTime) * v0.normalized;

        // update planar velociy
        m_State.Velocity = vt + m_State.Velocity.y * Vector3.up;

        // if the character has stopped, switch to not moving
        if(vt.sqrMagnitude == 0.0f) {
            ChangeTo(NotMoving);
            return;
        }
    }

    void Pivot_Exit() {
        m_State.PivotFrame = -1;
    }

    // -- Floating --
    CharacterPhase Floating => new CharacterPhase(
        name: "Floating",
        update: Floating_Update
    );

    void Floating_Update() {
        if (m_State.IsGrounded) {
            ChangeTo(Moving);
            return;
        }

        var v0 = m_State.PlanarVelocity;
        var vt = v0 + m_Input.MoveAxis * m_Tunables.AerialDriftAcceleration * Time.deltaTime;
        m_State.Velocity = vt + m_State.Velocity.y * Vector3.up;
    }
}

}