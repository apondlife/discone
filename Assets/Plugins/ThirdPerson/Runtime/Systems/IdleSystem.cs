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
        enter: (_, c) => {
            c.State.Next.IdleTime = 0f;
        },
        update: (_, s, c) => {
            if (c.Inputs.IsMoveIdle() && c.State.Curr.Velocity.sqrMagnitude <= c.Tuning.Idle_SqrSpeedThreshold) {
               s.ChangeTo(Idle);
            }
        }
    );

    // -- Idle --
    static readonly Phase<CharacterContainer> Idle = new("Idle",
        enter: (_, c) => {
            // TODO: make into PhaseElapsed
            c.State.Next.IdleTime = Time.deltaTime;
            c.Events.Schedule(CharacterEvent.Idle);
        },
        update: (delta, s, c) => {
            c.State.Next.IdleTime += delta;

            if (!c.Inputs.IsMoveIdle() || c.State.Curr.Velocity.sqrMagnitude > c.Tuning.Idle_SqrSpeedThreshold) {
               s.ChangeTo(NotIdle);
            }
        },
        exit: (_, c) => {
            c.Events.Schedule(CharacterEvent.Move);
        }
    );
}

}