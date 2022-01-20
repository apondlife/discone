using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

[System.Serializable]
sealed class CharacterInput {
    // -- fields --
    [Header("references")]
    [Tooltip("the transform for the player's look viewpoint")]
    [SerializeField] private Transform m_Look;

    [Tooltip("the unity player input")]
    [SerializeField] private PlayerInput m_PlayerInput;

    public bool IsJumpPressed;
    public Vector3 DesiredPlanarDirection;

    /// the move input
    InputAction m_Move;

    /// the jump input
    InputAction m_Jump;

    // -- lifecycle --
    public void Init() {
        m_Move = m_PlayerInput.currentActionMap["Move"];
        m_Jump = m_PlayerInput.currentActionMap["Jump"];
    }

    public void Read() {
        var forward = Vector3.Normalize(Vector3.ProjectOnPlane(
            m_Look.transform.forward,
            Vector3.up
        ));

        var right = m_Look.transform.right;

        // this would also be separate
        var pInput = m_Move.ReadValue<Vector2>();
        var input = forward * pInput.y + right * pInput.x;

        DesiredPlanarDirection = forward * pInput.y + right * pInput.x;
        IsJumpPressed = m_Jump.IsPressed();
    }
}

}