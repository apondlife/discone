using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

[System.Serializable]
class PlayerInputProvider : InputProvider {
    [Tooltip("the unity player input")]
    [SerializeField] private PlayerInput m_PlayerInput;

    // -- props --
    /// the move input
    InputAction m_Move;

    /// the jump input
    InputAction m_Jump;

    // -- lifecycle --
    /// initialize the input wrapper
    public override void Init() {
        m_Move = m_PlayerInput.currentActionMap["Move"];
        m_Jump = m_PlayerInput.currentActionMap["Jump"];
    }

    public override Vector2 Move {
        get => m_Move.ReadValue<Vector2>();
    }

    public override bool IsJumpPressed {
        get => m_Jump.IsPressed();
    }
}
}
