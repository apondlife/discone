using System;
using UnityEngine;

namespace ThirdPerson {

/// system state extensions
partial class CharacterState {
    partial class Frame {
        /// .
        public SystemState IdleState;
    }
}

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
        get => c.State.Next.IdleState;
        set => c.State.Next.IdleState = value;
    }

    // -- NotIdle --
    Phase NotIdle => new(
        name: "NotIdle",
        enter: NotIdle_Enter,
        update: NotIdle_Update
    );

    void NotIdle_Enter() {
        c.State.Next.IdleTime = 0.0f;
        c.Events.Schedule(CharacterEvent.Move);
    }

    void NotIdle_Update(float _) {
        if (c.State.Curr.Velocity.sqrMagnitude <= k_IdleSpeedThreshold) {
           ChangeTo(Idle);
        }
    }

    // -- Idle --
    Phase Idle => new(
        name: "Idle",
        enter: Idle_Enter,
        update: Idle_Update
    );

    void Idle_Enter() {
        // TODO: make into PhaseElapsed
        c.State.Next.IdleTime = Time.deltaTime;
        c.Events.Schedule(CharacterEvent.Idle);
    }

    void Idle_Update(float delta) {
        c.State.Next.IdleTime += delta;

        if (c.State.Curr.Velocity.sqrMagnitude > k_IdleSpeedThreshold) {
           ChangeTo(NotIdle);
        }
    }
}

}