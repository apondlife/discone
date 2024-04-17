using System;
using Soil;
using UnityEngine;

namespace ThirdPerson {

using Container = CharacterContainer;
using Phase = Phase<CharacterContainer>;

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
        return NotIdle;
    }

    protected override SystemState State {
        get => m_Container.State.Next.IdleState;
        set => m_Container.State.Next.IdleState = value;
    }

    // -- NotIdle --
    Phase NotIdle => new(
        name: "NotIdle",
        enter: NotIdle_Enter,
        update: NotIdle_Update
    );

    void NotIdle_Enter(Container c) {
        c.State.Next.IdleTime = 0f;
    }

    void NotIdle_Update(float _, Container c) {
        if (c.State.Curr.Velocity.sqrMagnitude <= k_IdleSpeedThreshold) {
           ChangeTo(Idle);
        }
    }

    // -- Idle --
    Phase Idle => new(
        name: "Idle",
        enter: Idle_Enter,
        update: Idle_Update,
        exit: Idle_Exit
    );

    void Idle_Enter(Container c) {
        // TODO: make into PhaseElapsed
        c.State.Next.IdleTime = Time.deltaTime;
        c.Events.Schedule(CharacterEvent.Idle);
    }

    void Idle_Update(float delta, Container c) {
        c.State.Next.IdleTime += delta;

        if (c.State.Curr.Velocity.sqrMagnitude > k_IdleSpeedThreshold) {
           ChangeTo(NotIdle);
        }
    }

    void Idle_Exit(Container c) {
        c.Events.Schedule(CharacterEvent.Move);
    }
}

}