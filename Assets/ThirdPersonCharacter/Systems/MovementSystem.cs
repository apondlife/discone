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

        // if no direction input, switch to not moving
        var dirMove = m_Input.DesiredPlanarDirection;
        if(dirMove.magnitude == 0) {
            ChangeTo(NotMoving);
            return;
        }


        // turn character towards movement direction
        m_State.SetProjectedFacingDirection(Vector3.RotateTowards(
            m_State.FacingDirection,
            dirMove,
            m_Tunables.TurnSpeed * Mathf.Deg2Rad * Time.deltaTime,
            Mathf.Infinity
        ));

        // move character in input direction
        m_State.PlanarSpeed = m_Tunables.PlanarSpeed;
    }
}