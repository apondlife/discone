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

        // // TODO: maybe fill the entire queue with frames?
        // Read();
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
    public Vector3 Move {
        get => m_Frames[0]?.Move ?? Vector3.zero;
    }

    /// if jump is down this frame
    public bool IsJumpPressed {
        get => m_Frames[0]?.IsJumpDown ?? false;
    }

    public bool IsHoldingWall {
        get => m_Frames[0]?.IsJumpDown ?? false;
    }

    /// if jump was pressed in the past n frames
    public bool IsJumpDown(uint past = 1) {
        for (var i = 0u; i < past; i++) {
            if (m_Frames[i]?.IsJumpDown == true && !m_Frames[i + 1]?.IsJumpDown == true) {
                return true;
            }
        }

        return false;
    }

    // -- types --
    /// the minimial frame of input for third person to work
    public interface Frame {
        /// the projected position of the move analog stick
        Vector3 Move { get; }

        /// if jump is down
        bool IsJumpDown { get; }
    }

    /// a default frame structure
    public readonly struct DefaultFrame: Frame {
        // -- props --
        /// the projected position of the move analog stick
        public readonly Vector3 m_Move;

        /// if jump is pressed
        public readonly bool m_IsJumpDown;

        // -- lifetime --
        /// create a new frame
        public DefaultFrame(
            Vector3 moveAxis,
            bool isJumpDown
        ) {
            m_Move = moveAxis;
            m_IsJumpDown = isJumpDown;
        }

        /// -- Frame --
        public Vector3 Move {
            get => m_Move;
        }

        public bool IsJumpDown {
            get => m_IsJumpDown;
        }
    }
}

}