using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class CharacterInput { // abstract class / interface
    // -- props --
    [Header("references")]
    [Tooltip("the player camera")]
    [SerializeField] private Camera cameraReference;

    [Tooltip("the player camera")]
    [SerializeField] private PlayerInput UnityPlayerInput;

    public bool IsJumpPressed;
    public Vector3 DesiredPlanarDirection;

    /// the move input
    InputAction m_Move;

    /// the jump input
    InputAction m_Jump;

    // -- lifecycle --
    public void Awake() {
        m_Move = UnityPlayerInput.currentActionMap["Move"];
        m_Jump = UnityPlayerInput.currentActionMap["Jump"];
    }

    public void Update() {
        var forward = Vector3.Normalize(Vector3.ProjectOnPlane(
            cameraReference.transform.forward,
            Vector3.up
        ));

        var right = cameraReference.transform.right;

        // this would also be separate
        var pInput = m_Move.ReadValue<Vector2>();
        var input = forward * pInput.y + right * pInput.x;

        DesiredPlanarDirection = forward * pInput.y + right * pInput.x;
        IsJumpPressed = m_Jump.IsPressed();
    }
}