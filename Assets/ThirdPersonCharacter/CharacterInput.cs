using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class CharacterInput { // abstract class / interface
    [SerializeField] private Camera cameraReference;
    [SerializeField] private PlayerInput UnityPlayerInput;

    public bool DesireToJump ;//{ get; private set; }
    public Vector3 DesiredPlanarDirection ;//{ get; private set; }

    public void Update() {
        var forward = Vector3.ProjectOnPlane(
            cameraReference.transform.forward,
            Vector3.up).normalized;
        var right = cameraReference.transform.right;

        // this would also be separate
        var pInput = UnityPlayerInput.currentActionMap["Move"].ReadValue<Vector2>();
        var input = forward * pInput.y + right * pInput.x;

        DesiredPlanarDirection = forward * pInput.y + right * pInput.x;
    }
}