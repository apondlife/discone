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
    const float k_Inactive = -1.0f;

    // -- types --
    /// tuning for the load checkpoint system
    [Serializable]
    public sealed class Tuning {
        /// the max load duration
        public float LoadCastMaxTime;

        /// the load duration at the point distance
        public float LoadCastPointTime;

        /// the distance at the point duration
        public float LoadCastPointDistance;

        /// the time multiplier when unloading
        public float LoadCancelMultiplier;
    }

    // -- deps --
    [Tooltip("the tuning")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Tunables")]
    [SerializeField] public Tuning m_Tuning;

    // -- props --
    /// the elapsed time
    float m_Elapsed = k_Inactive;

    /// the total cast time
    float m_Duration;

    /// the state when the load starts
    CharacterState.Frame m_SrcState;

    /// the final state when the load completes
    CharacterState.Frame m_DstState;

    /// the current state while loading
    CharacterState.Frame m_CurState;

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
        m_Elapsed = k_Inactive;
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
        var f = m_Tuning.LoadCastPointTime / m_Tuning.LoadCastMaxTime;
        var d = m_Tuning.LoadCastPointDistance;
        var k = f / (d * (1 - f));
        m_Duration = m_Tuning.LoadCastMaxTime * (1 - 1 / (k * distance + 1));

        // pause the character
        c.Character.Pause();

        // and start load
        m_Elapsed = 0.0f;
        m_SrcState = c.Character.State.Next;
        m_DstState = c.Checkpoint.IntoState();
        m_CurState = m_DstState.Copy();
    }

    void Loading_Update(float delta, Container c) {
        // if loading, aggregate time
        if (c.Character.Input.IsLoading()) {
            m_Elapsed += delta;
        } else if (m_Elapsed >= 0.0f) {
            m_Elapsed -= Mathf.Max(0, delta * m_Tuning.LoadCancelMultiplier);
        }

        // if we reach 0, cancel the load
        if (m_Elapsed < 0) {
            c.Character.ForceState(m_SrcState);
            ChangeTo(Loaded);
            return;
        }
        // finish the load once elapsed
        else if (m_Elapsed >= m_Duration) {
            c.Character.ForceState(m_DstState);
            ChangeTo(Loaded);
            return;
        }
        // otherwise, interpolate the load
        else {
            // we are interpolating position quadratically
            var pct = Mathf.Clamp01(m_Elapsed / m_Duration);
            var k = pct * pct;

            // update to the interpolated state
            CharacterState.Frame.Interpolate(
                m_SrcState,
                m_DstState,
                ref m_CurState,
                k
            );

            c.Character.ForceState(m_CurState);
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
        m_Elapsed = k_Inactive;
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
        get => m_Elapsed >= 0;
    }
}

}