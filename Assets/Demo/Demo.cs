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

    [Tooltip("the recording start input")]
    [SerializeField] InputActionReference m_Start;

    [Tooltip("the player")]
    [SerializeField] DisconePlayerVariable m_Player;

    [Tooltip("if the demo is recording")]
    [SerializeField] BoolReference m_IsRecording;

    // -- props --
    /// the list of subscriptions
    DisposeBag m_Subscriptions = new();

    /// the current state
    [NaughtyAttributes.ReadOnly]
    State m_State;

    /// the current state
    bool m_IsActive;

    /// the input recording
    InputRecording m_Recording = new();

    // -- lifecycle --
    void Start() {
        m_Subscriptions
            .Add(m_Record.action, OnRecord)
            .Add(m_Start.action, OnStart);
    }

    void FixedUpdate() {
        switch (m_State) {
            case State.Recording:
                // record input
                var frame = m_Player.Value.Character.Input.Curr;
                m_Recording.Record(frame);
                break;
            case State.Playing:
                break;
            default:
                break;
        }

        if (m_IsActive) {
            // if player input, restart game
        }
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- events --
    void OnStart(InputAction.CallbackContext _) {
        m_IsActive = true;
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