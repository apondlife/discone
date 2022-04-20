using UnityEngine;

namespace ThirdPerson {

/// how the character is affected by gravity
sealed class IdleSystem: CharacterSystem {
    // -- lifetime --
    public IdleSystem(CharacterData character)
        : base(character) {
    }

    protected override CharacterPhase InitInitialPhase() {
        return Idle;
    }

    // -- Idle --
    CharacterPhase NotIdle => new CharacterPhase(
        name: "NotIdle",
        update: NotIdle_Update,
        enter: NotIdle_Enter
    );

    void NotIdle_Enter() {
        m_State.IdleTime = 0;
    }

    void NotIdle_Update() {
        Debug.Log($"not idle, {m_State.Velocity}, {m_State.VerticalSpeed}, {m_State.PlanarVelocity}");
        if(m_State.Velocity.sqrMagnitude <= 0.1f) {
           ChangeTo(Idle);
        }
    }

    // -- Airborne --
    CharacterPhase Idle => new CharacterPhase(
        name: "Idle",
        update: Idle_Update
    );

    void Idle_Update() {
        m_State.IdleTime += Time.deltaTime;
        //Debug.Log("idle");
        if(m_State.Velocity.sqrMagnitude > 0.1f) {
           ChangeTo(NotIdle);
        }
    }
}

}