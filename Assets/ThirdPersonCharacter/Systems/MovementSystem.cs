using UnityEngine;

class MovementSystem : CharacterSystem {
    // -- lifetime --
    public MovementSystem(CharacterInput input, CharacterState state, CharacterTunables tunables)
        : base(input, state, tunables) {
    }

    protected override CharacterPhase InitInitialPhase() {
      return NotMoving;
    }

    // -- NotMoving --
    CharacterPhase NotMoving => new CharacterPhase(
        name: "Movement_NotMoving",
        update: NotMoving_Update
    );

    void NotMoving_Update() {
        if(m_Input.DesiredPlanarDirection.magnitude > 0) {
            ChangeTo(Moving);
        }
    }

    // -- Moving --
    CharacterPhase Moving => new CharacterPhase(
        name: "Movement_Moving",
        update: Moving_Update
    );

    void Moving_Update() {
        m_State.Velocity = m_Input.DesiredPlanarDirection * m_Tunables.PlanarSpeed;

        if(m_Input.DesiredPlanarDirection.magnitude == 0) {
            ChangeTo(NotMoving);
        }
    }
}