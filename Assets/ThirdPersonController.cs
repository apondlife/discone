using UnityEngine;

// CharacterState    - the state: pos, speed, heading, etc.
// CharacterMovement - reads input, runs state machine(s), updates state
// CharacterCamera   - it's the camera
// CharacterInput(?) - the input, reads raw input, translates into something

using UnityEngine;
using UnityEngine.InputSystem;

/// the main third person controller
public class ThirdPersonController : MonoBehaviour
{
    // this is separate
    [SerializeField] private Camera cameraReference;

    // from this
    [SerializeField] private CharacterController character;
    // is this character params?

    // [SerializeField] private CharacterMoveTunables tunables;
    public float PlanarSpeed = 1;
    public PlayerInput PlayerInput;

    void FixedUpdate()
    {
        // camera to left/forward movement
        var forward = Vector3.ProjectOnPlane(
            cameraReference.transform.forward,
            Vector3.up).normalized;
        var right = cameraReference.transform.right;

        // this would also be separate
        var pInput = PlayerInput.currentActionMap["Move"].ReadValue<Vector2>();
        var input = forward * pInput.y + right * pInput.x;

        // this would set the
        character.Move(input * PlanarSpeed * Time.deltaTime);
    }
}
