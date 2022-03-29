using UnityEngine;

namespace ThirdPerson {

public sealed class CharacterInput {
    // -- props --
    /// the source of input frames
    CharacterInputSource m_Source = null;

    /// the most recent input frames
    Queue<Frame> m_Frames = new Queue<Frame>(30);

    // -- commands --
    /// drive the input with a source
    public void Drive(CharacterInputSource source) {
        m_Source = source;
    }

    /// read the next frame of input
    public void Read() {
        if (m_Source == null || !m_Source.IsEnabled) {
            return;
        }

        m_Frames.Add(m_Source.Read());
    }

    // -- queries --
    /// the move axis this frame
    public Vector3 MoveAxis {
        get => m_Frames[0].MoveAxis;
    }

    /// if jump is pressed this frame
    public bool IsJumpPressed {
        get => m_Frames[0].IsJumpPressed;
    }

    public bool IsHoldingWall {
        get => m_Frames[0].IsJumpPressed;
    }

    /// if jump was pressed in the past n frames
    public bool IsJumpDown(uint past = 1) {
        for (var i = 0u; i < past; i++) {
            if (m_Frames[i].IsJumpPressed && !m_Frames[i + 1].IsJumpPressed) {
                return true;
            }
        }

        return false;
    }

    // -- types --
    /// a single frame of input
    public readonly struct Frame {
        /// if jump is pressed
        public readonly bool IsJumpPressed;

        /// the projected position of the move analog stick
        public readonly Vector3 MoveAxis;

        /// create a new frame
        public Frame(bool isJumpPressed, Vector3 move) {
            IsJumpPressed = isJumpPressed;
            MoveAxis = move;
        }
    }
}

}