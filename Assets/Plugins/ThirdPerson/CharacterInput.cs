using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

[System.Serializable]
public sealed class CharacterInput {
    // -- fields --
    [Header("references")]
    [Tooltip("the transform for the player's look viewpoint")]
    [SerializeField] private Transform m_Look;

    [Tooltip("the unity player input")]
    [SerializeField] private PlayerInput m_PlayerInput;

    // -- props --
    Queue<Frame> m_Frames = new Queue<Frame>(30);

    /// the move input
    InputAction m_Move;

    /// the jump input
    InputAction m_Jump;

    // -- lifecycle --
    /// initialize the input wrapper
    public void Init() {
        m_Move = m_PlayerInput.currentActionMap["Move"];
        m_Jump = m_PlayerInput.currentActionMap["Jump"];
    }

    /// read the next frame of input
    public void Read() {
        var forward = Vector3.Normalize(Vector3.ProjectOnPlane(
            m_Look.transform.forward,
            Vector3.up
        ));

        var right = m_Look.transform.right;

        // this would also be separate
        var pInput = m_Move.ReadValue<Vector2>();
        var move = forward * pInput.y + right * pInput.x;

        m_Frames.Add(new Frame(
            m_Jump.IsPressed(),
            move
        ));
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