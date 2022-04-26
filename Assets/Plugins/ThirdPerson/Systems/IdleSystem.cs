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

    // -- NotIdle --
    CharacterPhase NotIdle => new CharacterPhase(
        name: "NotIdle",
        update: NotIdle_Update,
        enter: NotIdle_Enter
    );

    void NotIdle_Enter() {
        m_State.IdleTime = 0.0f;
    }

    void NotIdle_Update() {
        if (m_State.Velocity.sqrMagnitude <= 0.1f) {
           ChangeTo(Idle);
        }
    }

    // -- Idle --
    CharacterPhase Idle => new CharacterPhase(
        name: "Idle",
        update: Idle_Update
    );

    void Idle_Update() {
        m_State.IdleTime += Time.deltaTime;

        if (m_State.Velocity.sqrMagnitude > 0.1f) {
           ChangeTo(NotIdle);
        }
    }
}

}