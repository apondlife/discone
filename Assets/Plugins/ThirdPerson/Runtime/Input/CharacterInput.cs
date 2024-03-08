using Soil;
using UnityEngine;

namespace ThirdPerson {

/// the character's input facade
public sealed class CharacterInput {
    // -- constants --
    #if UNITY_EDITOR
    const float k_BufferDuration = 5f;
    #else
    const float k_BufferDuration = 1f;
    #endif

    // -- props --
    /// the source of input frames
    CharacterInputSource m_Source = null;

    /// a queue of the most recent input frames
    readonly Ring<Frame> m_Frames = new((uint)(k_BufferDuration / Time.fixedDeltaTime));

    /// the last time we read input
    float m_Time;

    // the time the last jump down input happened
    float m_JumpDownTime;

    // -- commands --
    /// drive the input with a source
    public void Drive(CharacterInputSource source) {
        m_Source = source;

        // // TODO: maybe fill the entire queue with frames?
        // Read();
    }

    /// read the next frame of input
    public void Read() {
        if (m_Source == null || !m_Source.IsEnabled) {
            return;
        }

        // add a new input frame
        var curr = m_Frames[0];
        var next = m_Source.Read();
        m_Frames.Add(next);

        // track input times
        m_Time = Time.time;

        if (next.IsJumpPressed && !curr.IsJumpPressed) {
            m_JumpDownTime = m_Time;
        }
    }

    // -- queries --
    /// the move axis this frame
    public Vector3 Move {
        get => m_Frames[0]?.Move ?? Vector3.zero;
    }

    /// the magnitude of the move input this frame
    public float MoveMagnitude {
        get => Move.magnitude;
    }

    /// if jump is pressed this frame
    public bool IsJumpPressed {
        get => m_Frames[0]?.IsJumpPressed ?? false;
    }

    /// if crouch is pressed this frame
    public bool IsCrouchPressed {
        get => m_Frames[0]?.IsCrouchPressed ?? false;
    }

    /// if jump was pressed within the buffer window
    public bool IsJumpDown(float buffer) {
        return IsJumpPressed && (m_Time - m_JumpDownTime) <= buffer;
    }

    /// if move was pressed in the past n frames
    public bool IsMoveIdle(int past = 1) {
        for (var i = 0; i < past; i++) {
            if (m_Frames[i]?.Move != Vector3.zero) {
                return false;
            }
        }

        return true;
    }

    /// the buffer size
    public int BufferSize {
        get => m_Frames.Length;
    }

    // -- types --
    /// the minimal frame of input for third person to work
    public interface Frame {
        /// the projected position of the move analog stick
        Vector3 Move { get; }

        /// if jump is down
        bool IsJumpPressed { get; }

        /// if crouch is down
        bool IsCrouchPressed { get; }
    }

    /// a default frame structure
    public readonly struct DefaultFrame: Frame {
        // -- props --
        /// the projected position of the move analog stick
        public readonly Vector3 m_Move;

        /// if jump is pressed
        public readonly bool m_IsJumpDown;

        /// if crouch is pressed
        public readonly bool m_IsCrouchDown;

        // -- lifetime --
        /// create a new frame
        public DefaultFrame(
            Vector3 moveAxis,
            bool isJumpDown,
            bool isCrouchDown
        ) {
            m_Move = moveAxis;
            m_IsJumpDown = isJumpDown;
            m_IsCrouchDown = isCrouchDown;
        }

        /// -- Frame --
        public Vector3 Move {
            get => m_Move;
        }

        public bool IsJumpPressed {
            get => m_IsJumpDown;
        }

        public bool IsCrouchPressed {
            get => m_IsCrouchDown;
        }
    }

    // -- debug --
    #if UNITY_EDITOR
    /// set the current frame offset
    public void Debug_MoveHead(int offset) {
        m_Frames.Move(offset);
    }
    #endif
}

}