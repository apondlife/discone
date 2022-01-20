using UnityEngine;

/// how the character moves on the ground & air
sealed class MovementSystem: CharacterSystem {
    // -- lifetime --
    public MovementSystem(Character character)
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


        // TODO: slowdown
        m_State.PlanarVelocity = Vector3.zero;

        if(m_Input.DesiredPlanarDirection.magnitude > 0) {
            ChangeTo(Moving);
            return;
        }

        // start floating if no longer grounded
        if (!m_State.IsGrounded) {
            ChangeTo(Floating);
            return;
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

        // get current facing & input direction
        var dirFacing = m_State.FacingDirection;
        var dirInput = m_Input.DesiredPlanarDirection;

        // pivot if direction change was significant
        if (Vector3.Dot(dirFacing, dirInput) < m_Tunables.PivotStartThreshold) {
            ChangeTo(Pivot);
            return;
        }

        // rotate towards input direction
        var hasInput = dirInput.sqrMagnitude != 0.0f;
        if (hasInput) {
            dirFacing = Vector3.RotateTowards(
                m_State.FacingDirection,
                dirInput,
                m_Tunables.TurnSpeed * Mathf.Deg2Rad * Time.deltaTime,
                Mathf.Infinity
            );

            m_State.SetProjectedFacingDirection(dirFacing);
        }

        // calculate next velocity, integrating input & drag
        // vt = v0 + (input acceleration - drag - turning friction) * t
        var v0 = m_State.PlanarVelocity;
        var ai = m_Tunables.Acceleration * dirInput.magnitude * m_State.FacingDirection;
        var cf = hasInput ? m_Tunables.TurningFriction * (1.0f - Mathf.Abs(Vector3.Dot(v0.normalized, dirInput.normalized))) : 0.0f;
        // var f0 = cf * v0.normalized;
        var d0 = v0 * (m_Tunables.Deceleration + cf);
        var vt = v0 + (ai - d0) * Time.deltaTime;

        // update planar velocity
        m_State.SetProjectedPlanarVelocity(vt);

        // once speed is zero, stop moving
        if(m_State.PlanarVelocity.sqrMagnitude == 0.0f) {
            ChangeTo(NotMoving);
        }
    }

    // -- Pivot --
    CharacterPhase Pivot => new CharacterPhase(
        "Pivot",
        enter: Pivot_Enter,
        update: Pivot_Update
    );

    Vector3 m_DirPivot;

    void Pivot_Enter() {
        m_DirPivot = m_Input.DesiredPlanarDirection;
    }

    void Pivot_Update() {
        if (!m_State.IsGrounded) {
            ChangeTo(Floating);
            return;
        }

        // rotate towards pivot direction
        var dirFacing = m_State.FacingDirection;
        dirFacing = Vector3.RotateTowards(
            m_State.FacingDirection,
            m_DirPivot,
            m_Tunables.PivotSpeed * Mathf.Deg2Rad * Time.deltaTime,
            Mathf.Infinity
        );

        m_State.SetProjectedFacingDirection(dirFacing);

        // calculate next velocity, decelerating towards zero to finish pivot
        var v0 = m_State.PlanarVelocity;
        var vt = v0 - Mathf.Min(v0.magnitude, m_Tunables.PivotDeceleration * Time.deltaTime) * v0.normalized;

        // update planar velociy
        m_State.SetProjectedPlanarVelocity(vt);

        // if the character has stopped, switch to not moving
        if(vt.sqrMagnitude == 0.0f) {
            ChangeTo(NotMoving);
            return;
        }
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
        var vt = v0 + m_Input.DesiredPlanarDirection * m_Tunables.FloatAcceleration * Time.deltaTime;
        m_State.SetProjectedPlanarVelocity(vt);
    }
}