using System;
using Soil;
using ThirdPerson;
using UnityEngine;

namespace Discone {

using Container = CheckpointContainer;
using Phase = Phase<CheckpointContainer>;

/// a character's ability to load to their saved checkpoint
[Serializable]
sealed class LoadCheckpointSystem: SimpleSystem<Container> {
    // -- ThirdPerson.System --
    protected override Phase InitInitialPhase() {
        return NotLoading;
    }

    // -- NotLoading --
    Phase NotLoading => new(
        name: "NotLoading",
        enter: NotLoading_Enter,
        update: NotLoading_Update
    );

    void NotLoading_Enter(Container c) {
        c.State.Load_Reset();
    }

    void NotLoading_Update(float delta, Container c) {
        if (CanLoad(c)) {
            ChangeTo(Loading);
        }
    }

    // -- Loading --
    Phase Loading => new(
        name: "Loading",
        enter: Loading_Enter,
        update: Loading_Update,
        exit: Loading_Exit
    );

    void Loading_Enter(Container c) {
        // get distance to current checkpoint
        var distance = Vector3.Distance(
            c.Character.State.Position,
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
        c.State.Load_DstState = c.Checkpoint.IntoState();
        c.State.Load_CurState = c.State.Load_DstState.Copy();
    }

    void Loading_Update(float delta, Container c) {
        // if loading, aggregate time
        if (c.Character.Input.IsLoading()) {
            c.State.Load_Elapsed += delta;
        } else if (c.State.Load_Elapsed >= 0.0f) {
            c.State.Load_Elapsed -= Mathf.Max(0, delta * c.Tuning.Load_CancelMultiplier);
        }

        // if we reach 0, cancel the load
        if (c.State.Load_Elapsed < 0) {
            c.Character.ForceState(c.State.Load_SrcState);
            ChangeTo(Loaded);
            return;
        }
        // finish the load once elapsed
        else if (c.State.Load_Elapsed >= c.State.Load_Duration) {
            c.Character.ForceState(c.State.Load_DstState);
            ChangeTo(Loaded);
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

    }

    void Loading_Exit(Container c) {
       c.Character.Unpause();
    }

    // -- Loaded --
    Phase Loaded => new(
        name: "Loaded",
        enter: Loaded_Enter,
        update: Loaded_Update
    );

    void Loaded_Enter(Container c) {
        c.State.Load_Reset();
    }

    void Loaded_Update(float _, Container c) {
        if (!c.Character.Input.IsLoading()) {
            ChangeTo(NotLoading);
        }
    }

    // -- queries --
    /// if the player can load to their flower
    bool CanLoad(Container c) {
        return c.Character.Input.IsLoading() && c.Checkpoint != null;
    }

    public bool IsLoading {
        get => c.State.Load_Elapsed >= 0;
    }
}

}