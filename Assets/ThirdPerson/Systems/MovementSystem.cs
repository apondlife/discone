using UnityEngine;

/// the state machine for the character's movement
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

        // move the character; if there's input, accelerate towards max speed
        var vt = m_State.PlanarVelocity;
        if (hasInput) {
            vt += m_Tunables.Acceleration * dirInput.magnitude * Time.deltaTime * m_State.FacingDirection;
        }
        // otherwise, decelerate to zero
        else {
            vt -= Mathf.Min(vt.magnitude, m_Tunables.Deceleration * Time.deltaTime) * vt.normalized;
        }

        m_State.SetProjectedPlanarVelocity(Vector3.ClampMagnitude(vt, m_Tunables.MaxPlanarSpeed));

        // once speed is zero, stop moving
        if(vt.sqrMagnitude == 0.0f) {
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

        // decelerate towards zero to finish pivot
        var vt = m_State.PlanarVelocity;
        vt -= Mathf.Min(vt.magnitude, m_Tunables.PivotDeceleration * Time.deltaTime) * vt.normalized;
        m_State.SetProjectedPlanarVelocity(Vector3.ClampMagnitude(vt, m_Tunables.MaxPlanarSpeed));

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

        //TODO: moving
        //TODO: turning

        var vt = m_State.PlanarVelocity;
        vt += m_Input.DesiredPlanarDirection * m_Tunables.FloatAcceleration * Time.deltaTime;
        m_State.SetProjectedPlanarVelocity(vt);
    }
}