using System;
using UnityEngine;

namespace ThirdPerson {

// /// system state extensions
// partial class CharacterState {
//     partial class Frame {
//         /// .
//         public SystemState IdleState;
//     }
// }

/// how the character is affected by gravity
[Serializable]
sealed class IdleSystem: CharacterSystem {
    // -- constants --
    const float k_IdleSpeedThreshold = 0.1f;

    // -- System --
    protected override Phase InitInitialPhase() {
        return Idle;
    }

    protected override SystemState State {
        get => m_State.Next.IdleState;
    }

    // -- NotIdle --
    Phase NotIdle => new Phase(
        name: "NotIdle",
        enter: NotIdle_Enter,
        update: NotIdle_Update
    );

    void NotIdle_Enter() {
        m_State.Next.IdleTime = 0.0f;
    }

    void NotIdle_Update(float _) {
        if (m_State.Curr.Velocity.sqrMagnitude <= k_IdleSpeedThreshold) {
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
        m_State.Next.IdleTime = Time.deltaTime;
        m_Events.Schedule(CharacterEvent.Idle);
    }

    void Idle_Update(float delta) {
        m_State.Next.IdleTime += delta;

        if (m_State.Curr.Velocity.sqrMagnitude > k_IdleSpeedThreshold) {
           ChangeTo(NotIdle);
        }
    }
}

}