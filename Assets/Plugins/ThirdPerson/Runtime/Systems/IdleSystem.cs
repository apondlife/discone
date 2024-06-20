using System;
using Soil;
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
    // -- System --
    protected override Phase<CharacterContainer> InitInitialPhase() {
        return NotIdle;
    }

    protected override SystemState State {
        get => c.State.Next.IdleState;
        set => c.State.Next.IdleState = value;
    }

    // -- NotIdle --
    static readonly Phase<CharacterContainer> NotIdle = new("NotIdle",
        enter: NotIdle_Enter,
        update: NotIdle_Update
    );

    static void NotIdle_Enter(System<CharacterContainer> _, CharacterContainer c) {
        c.State.Next.IdleTime = 0f;
    }

    static void NotIdle_Update(float _, System<CharacterContainer> s, CharacterContainer c) {
        if (c.Inputs.IsMoveIdle() && c.State.Curr.Velocity.sqrMagnitude <= c.Tuning.Idle_SqrSpeedThreshold) {
            s.ChangeTo(Idle);
        }
    }

    // -- Idle --
    static readonly Phase<CharacterContainer> Idle = new("Idle",
        enter: Idle_Enter,
        update: Idle_Update,
        exit: Idle_Exit
    );

    static void Idle_Enter(System<CharacterContainer> _, CharacterContainer c) {
        // TODO: make into PhaseElapsed
        c.State.Next.IdleTime = Time.deltaTime;
        c.Events.Schedule(CharacterEvent.Idle);
    }

    static void Idle_Update(float delta, System<CharacterContainer> s, CharacterContainer c) {
        c.State.Next.IdleTime += delta;

        if (!c.Inputs.IsMoveIdle() || c.State.Curr.Velocity.sqrMagnitude > c.Tuning.Idle_SqrSpeedThreshold) {
            s.ChangeTo(NotIdle);
        }
    }

    static void Idle_Exit(System<CharacterContainer> _, CharacterContainer c) {
        c.Events.Schedule(CharacterEvent.Move);
    }
}

}