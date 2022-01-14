using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class CharacterInput { // abstract class / interface
    // -- props --
    [SerializeField] private Camera cameraReference;
    [SerializeField] private PlayerInput UnityPlayerInput;

    public bool DesiresToJump ;//{ get; private set; }
    public Vector3 DesiredPlanarDirection ;//{ get; private set; }

    InputAction m_Move;
    InputAction m_Jump;

    // -- lifecycle --
    public void Awake() {
        m_Move = UnityPlayerInput.currentActionMap["Move"];
        m_Jump = UnityPlayerInput.currentActionMap["Jump"];
    }

    public void Update() {
        var forward = Vector3.ProjectOnPlane(
            cameraReference.transform.forward,
            Vector3.up).normalized;
        var right = cameraReference.transform.right;

        // this would also be separate
        var pInput = m_Move.ReadValue<Vector2>();
        var input = forward * pInput.y + right * pInput.x;

        DesiredPlanarDirection = forward * pInput.y + right * pInput.x;
        DesiresToJump = m_Jump.IsPressed();
    }
}