using System;
using UnityEngine;

namespace ThirdPerson {

/// how the character is affected by gravity
[Serializable]
sealed class IdleSystem: CharacterSystem {
    // -- lifetime --
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
        m_State.Curr.IdleTime = 0.0f;
    }

    void NotIdle_Update() {
        if (m_State.Prev.Velocity.sqrMagnitude <= 0.1f) {
           ChangeTo(Idle);
        }
    }

    // -- Idle --
    CharacterPhase Idle => new CharacterPhase(
        name: "Idle",
        update: Idle_Update
    );

    void Idle_Update() {
        m_State.Curr.IdleTime += Time.deltaTime;

        if (m_State.Prev.Velocity.sqrMagnitude > 0.1f) {
           ChangeTo(NotIdle);
        }
    }
}

}