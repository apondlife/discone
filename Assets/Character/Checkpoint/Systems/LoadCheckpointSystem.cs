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

    public override void Init(CheckpointContainer c) {
        base.Init(c);
        c.Character.State.InitFrame(c.State.Load_DstState);
    }

    // -- NotLoading --
    static readonly Phase<CheckpointContainer> NotLoading = new("NotLoading",
        enter: NotLoading_Enter,
        update: NotLoading_Enter
    );

    static void NotLoading_Enter(System<CheckpointContainer> s, CheckpointContainer c) {
        c.State.Load_Reset();
    }

    static void NotLoading_Enter(float delta, System<CheckpointContainer> s, CheckpointContainer c) {
        if (CanLoad(c)) {
            s.ChangeTo(Loading);
        }
    }

    // -- Loading --
    static readonly Phase<CheckpointContainer> Loading = new("Loading",
        enter: Loading_Enter,
        update: Loading_Update,
        exit: Loading_Exit
    );

    static void Loading_Enter(System<CheckpointContainer> s, CheckpointContainer c) {
        // get distance to current checkpoint
        var distance = Vector3.Distance(c.Character.State.Next.Position, c.Checkpoint.Position);

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
        c.State.Load_DstState.Assign(c.Checkpoint.Position, c.Checkpoint.Forward);
    }

    static void Loading_Update(float delta, System<CheckpointContainer> s, CheckpointContainer c) {
        // if loading, aggregate time
        if (c.Character.Input.IsLoading()) {
            c.State.Load_Elapsed += delta;
        }
        else if (c.State.Load_Elapsed >= 0.0f) {
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
            c.State.Load_CurState.Interpolate(
                c.State.Load_SrcState,
                c.State.Load_DstState,
                k
            );

            c.Character.ForceState(c.State.Load_CurState);
        }
    }

    static void Loading_Exit(System<CheckpointContainer> _, CheckpointContainer c) {
        c.Character.Unpause();
    }

    // -- Loaded --
    static readonly Phase<CheckpointContainer> Loaded = new("Loaded",
        enter: Loaded_Enter,
        update: Loaded_Update
    );

    static void Loaded_Enter(System<CheckpointContainer> s, CheckpointContainer c) {
        c.State.Load_Reset();
    }

    static void Loaded_Update(float _, System<CheckpointContainer> s, CheckpointContainer c) {
        if (!c.Character.Input.IsLoading()) {
            s.ChangeTo(NotLoading);
        }
    }

    // -- queries --
    /// if the player can load to their flower
    static bool CanLoad(CheckpointContainer c) {
        return c.Character.Input.IsLoading() && c.Checkpoint != null;
    }
}

}