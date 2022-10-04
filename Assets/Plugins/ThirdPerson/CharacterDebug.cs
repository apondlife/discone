using UnityEngine;
using System.Linq;

namespace ThirdPerson {

#if UNITY_EDITOR
/// debug extensions for the character
public partial class Character: MonoBehaviour {
    // -- constants --
    /// no frame
    const int k_Debug_FrameNone = -1;

    /// the debug pause key
    const KeyCode k_Pause = KeyCode.Alpha5;

    /// the debug frame rewind key
    const KeyCode k_Rewind = KeyCode.Alpha6;

    /// the debug frame advance key
    const KeyCode k_Advance = KeyCode.Alpha7;

    // -- props --
    /// if we're debugging
    bool m_Debug_IsEnabled = false;

    /// the debug frame offset
    int m_Debug_FrameOffset = k_Debug_FrameNone;

    /// the debug state frame
    CharacterState.Frame m_Debug_StateFrame = null;

    // -- lifecycle --
    void Update() {
        if (Input.GetKeyDown(k_Pause)) {
            Debug_OnPause();
        } else if (Input.GetKeyDown(k_Rewind)) {
            Debug_OnFrameRewind();
        } else if (Input.GetKeyDown(k_Advance)) {
            Debug_OnFrameAdvance();
        }
    }

    // -- commands --
    /// when the frame is steps
    void Debug_StepFrames(int frames) {
        if (!m_Debug_IsEnabled) {
           return;
        }

        // don't move outside the tape
        var next = m_Debug_FrameOffset + frames;
        if (next < 0 || next >= Mathf.Min(m_State.BufferSize, m_Input.BufferSize)) {
            return;
        }

        m_Debug_FrameOffset = next;

        // move the head of the state and input tapes
        m_State.Debug_MoveHead(frames);
        m_Input.Debug_MoveHead(frames);

        // copy the current state
        m_Debug_StateFrame = m_State.Curr.Copy();

        // step from a clean copy of the previous state
        m_State.Force(m_State.Prev);

        // run the systems for the debug state/input
        Step();

        // ignore any mutations from the step
        m_State.Force(m_Debug_StateFrame);
        m_Debug_StateFrame = null;
    }

    // -- events --
    /// when debug pause is pressed
    void Debug_OnPause() {
        // toggle debug state
        m_Debug_IsEnabled = !m_Debug_IsEnabled;
        m_Debug_FrameOffset = m_Debug_IsEnabled ? 0 : k_Debug_FrameNone;

        // pause character when debugging
        m_IsPaused = m_Debug_IsEnabled;
    }

    /// when rewind is pressed
    void Debug_OnFrameRewind() {
        if (!m_Debug_IsEnabled) {
            Debug_OnPause();
        }

        Debug_StepFrames(+1);
    }

    /// when advance is pressed
    void Debug_OnFrameAdvance() {
        if (!m_Debug_IsEnabled) {
            Debug_OnPause();
        }

        Debug_StepFrames(-1);
    }
}
#endif

}