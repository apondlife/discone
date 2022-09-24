using System;
using ThirdPerson;
using UnityEngine;

/// a character's ability to load to their saved checkpointj
sealed class LoadCheckpointSystem: ThirdPerson.System {
    [Serializable]
    public sealed class Tunables {
        public float LoadCastMaxTime;
        public float LoadCastPointTime;
        public float LoadCastPointDistance;
        public float LoadCancelMultiplier;
    }

    [System.Obsolete]
    public sealed class LoadInput {
        public bool IsLoading;
    }

    // -- deps --
    /// the tunables
    Tunables m_Tunables;

    /// the character
    CharacterState m_State;

    /// the checkpoint
    CharacterCheckpoint m_Checkpoint;

    // -- props --
    /// the input state
    LoadInput m_Input = new LoadInput();

    /// the elapsed time
    float m_Elapsed;

    /// the total cast time
    float m_Duration;

    /// the state when the load starts
    CharacterState.Frame m_SrcState;

    /// the final state when if the load finishes completes
    CharacterState.Frame m_DstState;

    // -- lifetime --
    public LoadCheckpointSystem(
        Tunables tunables,
        CharacterState character,
        CharacterCheckpoint checkpoint
    ): base() {
        m_Tunables = tunables;
        m_State = character;
        m_Checkpoint = checkpoint;
    }

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
        update: NotLoading_Update,
        exit: NotLoading_Exit
    );

    void NotLoading_Enter() {
        m_Elapsed = 0.0f;
        m_Checkpoint.Character.Pause();
    }

    void NotLoading_Update(float delta) {
        if (m_Input.IsLoading) {
            ChangeTo(Loading);
        }
    }

    void NotLoading_Exit() {
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
        m_DstState = m_Checkpoint.Flower.IntoState();
    }

    void Loading_Update(float delta) {
        // if loading, aggregate time
        if (m_Input.IsLoading) {
            m_Elapsed += delta;
        } else if (m_Elapsed >= 0.0f) {
            m_Elapsed -= Mathf.Max(0, delta * m_Tunables.LoadCancelMultiplier);
        }

        // if we reach 0, cancel the load
        if (m_Elapsed <= 0) {
            m_Checkpoint.Character.ForceState(m_SrcState);
            ChangeTo(NotLoading);
        }
        // finish the load once elapsed
        else if (m_Elapsed > m_Duration) {
            m_Checkpoint.Character.ForceState(m_DstState);
            ChangeTo(NotLoading);
        }
        // otherwise, interpolate the load
        else {
            // we are interpolating position quadratically
            var pct = Mathf.Clamp01(m_Elapsed / m_Duration);
            var k = pct * pct;

            // update to the interpolated state
            var state = CharacterState.Frame.Interpolate(
                m_SrcState,
                m_DstState,
                k
            );

            m_Checkpoint.Character.ForceState(state);
        }
    }

    void Loading_Exit() {

    }
}