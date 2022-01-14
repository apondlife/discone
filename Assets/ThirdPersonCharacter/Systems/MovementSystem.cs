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
        // drag
        m_State.PlanarSpeed = 0;

        if(m_Input.DesiredPlanarDirection.magnitude > 0) {
            ChangeTo(Moving);
        }
    }

    // -- Moving --
    CharacterPhase Moving => new CharacterPhase(
        name: "Moving",
        update: Moving_Update
    );

    void Moving_Update() {
        if (!m_State.IsGrounded) {
            return;
        }

        // check input direction
        var dirInput = m_Input.DesiredPlanarDirection;
        var hasInput = dirInput.sqrMagnitude != 0.0f;

        // turn character towards input direction
        if (hasInput) {
            var dirMove = Vector3.RotateTowards(
                m_State.FacingDirection,
                dirInput,
                m_Tunables.TurnSpeed * Mathf.Deg2Rad * Time.deltaTime,
                Mathf.Infinity
            );

            m_State.SetProjectedFacingDirection(dirMove);
        }

        var vt = m_State.PlanarSpeed;

        // if there's input, accelerate towards max speed
        if (hasInput) {
            vt += m_Tunables.Acceleration * dirInput.magnitude * Time.deltaTime;
        }
        // otherwise, decelerate to zero
        else {
            vt -= m_Tunables.Deceleration * Time.deltaTime;
        }

        m_State.PlanarSpeed = Mathf.Clamp(vt, 0.0f, m_Tunables.MaxPlanarSpeed);

        // once speed is zero, stop moving
        if(m_State.PlanarSpeed == 0.0f) {
            ChangeTo(NotMoving);
        }
    }
}