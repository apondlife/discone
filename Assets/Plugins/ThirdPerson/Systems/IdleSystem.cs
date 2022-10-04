using System;
using UnityEngine;

namespace ThirdPerson {

/// how the character is affected by gravity
[Serializable]
sealed class IdleSystem: CharacterSystem {
    // -- constants --
    const float k_IdleSpeedThreshold = 0.1f;

    // -- lifetime --
    protected override Phase InitInitialPhase() {
        return Idle;
    }

    // -- NotIdle --
    Phase NotIdle => new Phase(
        name: "NotIdle",
        enter: NotIdle_Enter,
        update: NotIdle_Update
    );

    void NotIdle_Enter() {
        m_State.Curr.IdleTime = 0.0f;
    }

    void NotIdle_Update(float _) {
        if (m_State.Prev.Velocity.sqrMagnitude <= k_IdleSpeedThreshold) {
           ChangeTo(Idle);
        }
    }

    // -- Idle --
    Phase Idle => new Phase(
        name: "Idle",
        enter: Idle_Enter,
        update: Idle_Update
    );

    void Idle_Enter() {
        m_State.Curr.IdleTime = Time.deltaTime;
        m_Events.Schedule(CharacterEvent.Idle);
    }

    void Idle_Update(float delta) {
        m_State.Curr.IdleTime += delta;

        if (m_State.Prev.Velocity.sqrMagnitude > k_IdleSpeedThreshold) {
           ChangeTo(NotIdle);
        }
    }
}

}