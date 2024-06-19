using Soil;
using UnityEngine;

namespace ThirdPerson {

/// the character's input facade
public class CharacterInput<F>: CharacterInputQuery where F: CharacterInputFrame {
    // -- constants --
    #if UNITY_EDITOR
    const float k_BufferDuration = 5f;
    #else
    const float k_BufferDuration = 1f;
    #endif

    // -- props --
    /// the source of input frames
    CharacterInputSource<F> m_Source = null;

    // TODO: read input in update
    /// a queue of the most recent input frames
    readonly Ring<F> m_Frames = new((uint)(k_BufferDuration / Time.fixedDeltaTime));

    /// the time since the last jump press
    float m_TimeSinceJumpPress;

    /// the time the last jump press was released
    float m_JumpPressReleasedAt = CharacterInputQuery.NotReleased;

    // -- commands --
    /// drive the input with a source
    public void Drive(CharacterInputSource<F> source) {
        m_Source = source;
    }

    /// read the next frame of input
    public void Read(float delta) {
        if (m_Source == null || !m_Source.IsEnabled) {
            return;
        }

        // add a new input frame
        var curr = m_Frames[0];
        var next = m_Source.Read();
        m_Frames.Add(next);

        // if the jump was just pressed
        if (!curr.IsJumpPressed && next.IsJumpPressed) {
            m_TimeSinceJumpPress = 0f;
            m_JumpPressReleasedAt = CharacterInputQuery.NotReleased;
        }
        // otherwise, count time since press
        else {
            m_TimeSinceJumpPress += delta;
        }

        // if the jump was just released
        if (curr.IsJumpPressed && !next.IsJumpPressed) {
            m_JumpPressReleasedAt = m_TimeSinceJumpPress;
        }
    }

    // -- queries --
    /// the current input frame
    public F Curr {
        get => m_Frames[0];
    }

    // -- CharacterInputQuery --
    public int BufferSize {
        get => m_Frames.Length;
    }

    public Vector3 Move {
        get => m_Frames[0]?.Move ?? Vector3.zero;
    }

    public float MoveMagnitude {
        get => Move.magnitude;
    }

    public bool IsJumpPressed {
        get => m_Frames[0]?.IsJumpPressed ?? false;
    }

    public float TimeSinceJumpPress {
        get => m_TimeSinceJumpPress;
    }

    public float JumpPressReleasedAt {
        get => m_JumpPressReleasedAt;
    }

    public bool IsMoveIdle(int past = 1) {
        for (var i = 0; i < past; i++) {
            if (m_Frames[i]?.Move != Vector3.zero) {
                return false;
            }
        }

        return true;
    }

    // -- debug --
    #if UNITY_EDITOR
    /// set the current frame offset
    public void Debug_MoveHead(int offset) {
        m_Frames.Offset(offset);
    }
    #endif
}

}