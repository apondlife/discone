using System;
using UnityEngine;

namespace ThirdPerson {

/// how the character is affected by gravity
[Serializable]
sealed class IdleSystem: CharacterSystem {
    private const float k_IdleSpeedThreshold = 0.1f;


    // -- lifetime --
    protected override CharacterPhase InitInitialPhase() {
        return Idle;
    }

    // -- NotIdle --
    CharacterPhase NotIdle => new CharacterPhase(
        name: "NotIdle",
        enter: NotIdle_Enter,
        update: NotIdle_Update
    );

    void NotIdle_Enter() {
        m_State.Curr.IdleTime = 0.0f;
    }

    void NotIdle_Update() {
        if (m_State.Prev.Velocity.sqrMagnitude <= k_IdleSpeedThreshold) {
           ChangeTo(Idle);
        }
    }

    // -- Idle --
    CharacterPhase Idle => new CharacterPhase(
        name: "Idle",
        enter: Idle_Enter,
        update: Idle_Update
    );

    void Idle_Enter() {
        m_State.Curr.IdleTime = Time.deltaTime;
        m_Events.Schedule(CharacterEvent.Idle);
    }

    void Idle_Update() {
        m_State.Curr.IdleTime += Time.deltaTime;

        if (m_State.Prev.Velocity.sqrMagnitude > k_IdleSpeedThreshold) {
           ChangeTo(NotIdle);
        }
    }
}

}