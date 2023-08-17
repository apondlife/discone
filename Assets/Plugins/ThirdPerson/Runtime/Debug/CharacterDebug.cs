#if UNITY_EDITOR
using UnityEngine;

namespace ThirdPerson {

/// debug extensions for the character
public partial class Character: MonoBehaviour {
    // -- constants --
    /// no frame
    const int k_Debug_FrameNone = -1;

    /// the debug pause key
    const KeyCode k_Debug_Pause = KeyCode.Alpha5;

    /// the debug frame rewind key
    const KeyCode k_Debug_Rewind = KeyCode.Alpha6;

    /// the debug frame advance key
    const KeyCode k_Debug_Advance = KeyCode.Alpha7;

    /// the number of seconds before key repeat
    const float k_Debug_RepeatTimeout = 0.5f;

    // -- props --
    /// if we're debugging
    bool m_Debug_IsEnabled = false;

    /// the debug frame offset
    int m_Debug_FrameOffset = k_Debug_FrameNone;

    /// the debug state frame
    CharacterState.Frame m_Debug_StateFrame = null;

    /// the current pressed key
    KeyCode m_CurrentKey = KeyCode.None;

    /// the time of the current key press
    float m_CurrentKeyTime = -1.0f;

    // -- lifecycle --
    void Update() {
        // capture key press
        if (UnityEngine.Input.GetKeyDown(k_Debug_Pause)) {
            Debug_OnKeyDown(k_Debug_Pause);
        } else if (UnityEngine.Input.GetKeyDown(k_Debug_Rewind)) {
            Debug_OnKeyDown(k_Debug_Rewind);
        } else if (UnityEngine.Input.GetKeyDown(k_Debug_Advance)) {
            Debug_OnKeyDown(k_Debug_Advance);
        }

        // run key repeat for rewind and advance
        var isRepeat = (
            UnityEngine.Input.GetKey(m_CurrentKey) &&
            (m_CurrentKey == k_Debug_Rewind || m_CurrentKey == k_Debug_Advance) &&
            Time.time - m_CurrentKeyTime > k_Debug_RepeatTimeout
        );

        if (isRepeat) {
            Debug_OnKey();
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
        if (next >= Mathf.Min(m_State.BufferSize, m_Input.BufferSize)) {
            return;
        }

        if (next < 0) {
            m_State.Advance();
            m_Input.Read();
            m_Debug_FrameOffset = 0;
        } else  {
            m_Debug_FrameOffset = next;

            // move the head of the state and input tapes
            m_State.Debug_MoveHead(frames);
            m_Input.Debug_MoveHead(frames);

            // restore the system to the correct phase
            foreach (var system in m_Systems) {
                system.RestorePhase();
            }

            // copy the current state
            m_Debug_StateFrame = m_State.Next.Copy();

            // step from a clean copy of the previous state
            m_State.Force(m_State.Curr);
        }

        // run the systems for the debug state/input
        Step();

        // ignore any mutations from the step
        if (m_Debug_StateFrame != null) {
            m_State.Force(m_Debug_StateFrame);
        }

        m_Debug_StateFrame = null;
    }

    // -- events --
    /// when a key is pressed
    void Debug_OnKeyDown(KeyCode key) {
        m_CurrentKey = key;
        m_CurrentKeyTime = Time.time;
        Debug_OnKey();
    }

    /// when a key's action fires
    void Debug_OnKey() {
        switch (m_CurrentKey) {
        case k_Debug_Pause:
            Debug_OnPause(); break;
        case k_Debug_Rewind:
            Debug_OnFrameRewind(); break;
        case k_Debug_Advance:
            Debug_OnFrameAdvance(); break;
        }
    }

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

}
#endif