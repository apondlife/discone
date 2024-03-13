using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

public class Demo: MonoBehaviour {
    enum State {
        Inactive,
        Recording,
        Playing,
    }

    // -- refs --
    [Header("refs")]
    [Tooltip("the recording toggle input")]
    [SerializeField] InputActionReference m_Record;

    [Tooltip("the demo start input")]
    [SerializeField] InputActionReference m_Start;

    [Tooltip("the player")]
    [SerializeField] DisconePlayerVariable m_Player;

    [Tooltip("if the demo is recording")]
    [SerializeField] BoolReference m_IsRecording;

    [Tooltip("if the demo is running")]
    [SerializeField] BoolReference m_IsRunning;

    // -- published --
    [Header("published")]
    [Tooltip("if the demo is running")]
    [SerializeField] VoidEvent m_Reset;

    // -- props --
    /// the list of subscriptions
    DisposeBag m_Subscriptions = new();

    /// the current state
    [NaughtyAttributes.ReadOnly]
    State m_State;

    /// the input recording
    InputRecording m_Recording = new(isLooping: true);

    /// if the player is idle after start
    bool m_IsIdle;

    // -- lifecycle --
    void Start() {
        m_IsRunning.Value = false;

        m_Subscriptions
            .Add(m_Record.action, OnRecord)
            .Add(m_Start.action, OnStart);
    }

    void FixedUpdate() {
        switch (m_State) {
            case State.Recording: {
                var frame = m_Player.Value.Character.Input.Curr;
                m_Recording.Record(frame);
                break;
            }
            case State.Playing: {
                break;
            }
        }

        if (m_IsRunning.Value) {
            var inputSource = m_Player.Value.InputSource;
            var hasAnyInput = inputSource.Read().Any;

            // once the input is released
            if (!hasAnyInput) {
                m_IsIdle = true;
            }

            // on any input, restart game
            if (m_IsIdle && hasAnyInput) {
                m_Reset.Raise();
            }
        }
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- events --
    void OnStart(InputAction.CallbackContext _) {
        m_IsIdle = false;
        m_IsRunning.Value = true;
        SwitchTo(State.Playing);
    }

    void OnRecord(InputAction.CallbackContext _) {
        // cycle state
        var nextState = m_State + 1;
        if (nextState > State.Playing) {
            nextState = State.Inactive;
        }

        SwitchTo(nextState);
    }

    void SwitchTo(State nextState) {
        Log.Debug.I($"demo {m_State} -> {nextState}");

        var isRecording = nextState == State.Recording;
        if (isRecording) {
            m_Recording.Clear();
            // place flower
        }

        var character = m_Player.Value.Character;
        if (nextState == State.Playing) {
            // reset to flower
            m_Recording.Play();
            character.Drive(m_Recording);
        }

        if (nextState == State.Inactive) {
            m_Recording.Pause();
            character.Drive(m_Player.Value.InputSource);
        }

        m_State = nextState;
        m_IsRecording.Value = isRecording;
    }
}

}