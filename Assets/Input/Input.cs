using ThirdPerson;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

/// the input for a player
public sealed class Input: MonoBehaviour, PlayerInputActions {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the name of the jump input action")]
    [SerializeField] string m_JumpName;

    [Tooltip("the name of the move input action")]
    [SerializeField] string m_MoveName;

    [Tooltip("the name of the load input action")]
    [SerializeField] string m_LoadName;

    // -- refs --
    [Header("refs")]
    [Tooltip("the input system player input")]
    [SerializeField] PlayerInput m_PlayerInput;

    // -- props --
    /// .
    InputAction m_JumpAction;

    /// .
    InputAction m_MoveAction;

    /// .
    InputAction m_LoadAction;

    // -- commands --
    /// bind the input to an input system action asset
    void Awake() {
        var asset = m_PlayerInput.actions;
        m_JumpAction = asset.FindAction(m_JumpName);
        m_MoveAction = asset.FindAction(m_MoveName);
        m_LoadAction = asset.FindAction(m_LoadName);
    }

    // -- queries --
    /// if the load button is pressed
    public bool IsLoadPressed {
        get => m_LoadAction.IsPressed();
    }

    // -- PlayerInputActions --
    public Vector2 Move {
        get => m_MoveAction.ReadValue<Vector2>();
    }

    public bool IsJumpPressed {
        get => m_JumpAction.IsPressed();
    }
}

}