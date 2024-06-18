using System;
using Soil;
using ThirdPerson;
using UnityEngine;

namespace Discone {

/// a character's ability to load to their saved checkpoint
[Serializable]
sealed class LoadCheckpointSystem: SimpleSystem<CheckpointContainer> {
    // -- ThirdPerson.System --
    protected override Phase<CheckpointContainer> InitInitialPhase() {
        return NotLoading;
    }

    // -- NotLoading --
    static readonly Phase<CheckpointContainer> NotLoading = new("NotLoading",
        enter: (s, c) => {
            c.State.Load_Reset();
        },
        update: (delta, s, c) => {
            if (CanLoad(c)) {
                s.ChangeTo(Loading);
            }
        }
    );

    // -- Loading --
    static readonly Phase<CheckpointContainer> Loading = new("Loading",
        enter: (s, c) => {
            // get distance to current checkpoint
            var distance = Vector3.Distance(
                c.Character.State.Next.Position,
                c.Checkpoint.Position
            );

            // calculate cast time
            var f = c.Tuning.Load_CastPointTime / c.Tuning.Load_CastMaxTime;
            var d = c.Tuning.Load_CastPointDistance;
            var k = f / (d * (1 - f));
            c.State.Load_Duration = c.Tuning.Load_CastMaxTime * (1 - 1 / (k * distance + 1));

            // pause the character
            c.Character.Pause();

            // and start load
            c.State.Load_Elapsed = 0.0f;
            c.State.Load_SrcState = c.Character.State.Next;
            c.State.Load_DstState = c.Character.State.Create(c.Checkpoint.Position, c.Checkpoint.Forward);
            c.State.Load_CurState = c.State.Load_DstState.Copy();
        },
        update: (delta, s, c) => {
            // if loading, aggregate time
            if (c.Character.Input.IsLoading()) {
                c.State.Load_Elapsed += delta;
            } else if (c.State.Load_Elapsed >= 0.0f) {
                c.State.Load_Elapsed -= Mathf.Max(0, delta * c.Tuning.Load_CancelMultiplier);
            }

            // if we reach 0, cancel the load
            if (c.State.Load_Elapsed < 0) {
                c.Character.ForceState(c.State.Load_SrcState);
                s.ChangeTo(Loaded);
                return;
            }
            // finish the load once elapsed
            else if (c.State.Load_Elapsed >= c.State.Load_Duration) {
                c.Character.ForceState(c.State.Load_DstState);
                s.ChangeTo(Loaded);
                return;
            }
            // otherwise, interpolate the load
            else {
                // we are interpolating position quadratically
                var pct = Mathf.Clamp01(c.State.Load_Elapsed / c.State.Load_Duration);
                var k = pct * pct;

                // update to the interpolated state
                CharacterState.Frame.Interpolate(
                    c.State.Load_SrcState,
                    c.State.Load_DstState,
                    ref c.State.Load_CurState,
                    k
                );

                c.Character.ForceState(c.State.Load_CurState);
            }

        },
        exit: (_, c) => {
           c.Character.Unpause();
        }
    );

    // -- Loaded --
    static readonly Phase<CheckpointContainer> Loaded = new("Loaded",
        enter: (s, c) => {
            c.State.Load_Reset();
        },
        update: (_, s, c) => {
            if (!c.Character.Input.IsLoading()) {
                s.ChangeTo(NotLoading);
            }
        }
    );

    // -- queries --
    /// if the player can load to their flower
    static bool CanLoad(CheckpointContainer c) {
        return c.Character.Input.IsLoading() && c.Checkpoint != null;
    }
}

}