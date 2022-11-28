using System;
using ThirdPerson;
using UnityEngine;

/// a character's ability to load to their saved checkpoint
[Serializable]
sealed class LoadCheckpointSystem: CheckpointSystem {
    const float k_Inactive = -1.0f;
    // -- types --
    /// tunables for the load checkpoint system
    [Serializable]
    public sealed class Tunables {
        /// the max load duration
        public float LoadCastMaxTime;

        /// the load duration at the point distance
        public float LoadCastPointTime;

        /// the distance at the point duration
        public float LoadCastPointDistance;

        /// the time multiplier when unloading
        public float LoadCancelMultiplier;
    }

    [System.Obsolete]
    public sealed class LoadInput {
        public bool IsLoading;
    }

    // -- deps --
    [Tooltip("the tunables")]
    [SerializeField] public Tunables m_Tunables;

    // -- props --
    /// the input state
    LoadInput m_Input = new LoadInput();

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

    // -- queries --
    /// the input state
    public LoadInput Input {
        get => m_Input;
    }

    // -- ThirdPerson.System --
    protected override Phase InitInitialPhase() {
        return NotLoading;
    }

    // -- NotLoading --
    Phase NotLoading => new Phase(
        name: "NotLoading",
        enter: NotLoading_Enter,
        update: NotLoading_Update
    );

    void NotLoading_Enter() {
        m_Elapsed = k_Inactive;
    }

    void NotLoading_Update(float delta) {
        if (CanLoad) {
            ChangeTo(Loading);
        }
    }

    // -- Loading --
    Phase Loading => new Phase(
        name: "Loading",
        enter: Loading_Enter,
        update: Loading_Update,
        exit: Loading_Exit
    );

    void Loading_Enter() {
        // get distance to current checkpoint
        var distance = Vector3.Distance(
            m_State.Position,
            m_Checkpoint.Checkpoint.Position
        );

        // calculate cast time
        var f = m_Tunables.LoadCastPointTime / m_Tunables.LoadCastMaxTime;
        var d = m_Tunables.LoadCastPointDistance;
        var k = f / (d * (1 - f));
        m_Duration = m_Tunables.LoadCastMaxTime * (1 - 1 / (k * distance + 1));

        // pause the character
        m_Checkpoint.Character.Pause();

        // and start load
        m_Elapsed = 0.0f;
        m_SrcState = m_State.Curr;
        m_DstState = m_Checkpoint.Checkpoint.IntoState();
        m_CurState = m_DstState.Copy();
    }

    void Loading_Update(float delta) {
        // if loading, aggregate time
        if (m_Input.IsLoading) {
            m_Elapsed += delta;
        } else if (m_Elapsed >= 0.0f) {
            m_Elapsed -= Mathf.Max(0, delta * m_Tunables.LoadCancelMultiplier);
        }

        // if we reach 0, cancel the load
        if (m_Elapsed < 0) {
            m_Checkpoint.Character.ForceState(m_SrcState);
            ChangeTo(Loaded);
            return;
        }
        // finish the load once elapsed
        else if (m_Elapsed >= m_Duration) {
            m_Checkpoint.Character.ForceState(m_DstState);
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

            m_Checkpoint.Character.ForceState(m_CurState);
        }

    }

    void Loading_Exit() {
       m_Checkpoint.Character.Unpause();
    }

    // -- Loaded --
    Phase Loaded => new Phase(
        name: "Loaded",
        enter: Loaded_Enter,
        update: Loaded_Update
    );

    void Loaded_Enter() {
        m_Elapsed = k_Inactive;
    }

    void Loaded_Update(float _) {
        if (!m_Input.IsLoading) {
            ChangeTo(NotLoading);
        }
    }

    // -- queries --
    /// if the player can load to their flower
    bool CanLoad {
        get => m_Input.IsLoading && m_Checkpoint.Flower != null;
    }

    public bool IsLoading {
        get => m_Elapsed >= 0;
    }
}