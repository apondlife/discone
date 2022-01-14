using UnityEngine;

class MovementSystem: CharacterSystem {
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
        m_State.PlanarVelocity = Vector3.zero;
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
        m_State.PlanarVelocity = m_Input.DesiredPlanarDirection * m_Tunables.PlanarSpeed;

        if(m_Input.DesiredPlanarDirection.magnitude == 0) {
            ChangeTo(NotMoving);
        }
    }
}