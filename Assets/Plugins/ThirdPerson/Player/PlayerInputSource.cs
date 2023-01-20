using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a player's input source for controlling characters
public sealed class PlayerInputSource: MonoBehaviour, CharacterInputSource {
    // -- refs --
    [Header("refs")]
    [Tooltip("the transform for the player's look viewpoint")]
    [SerializeField] Transform m_Look;

    [Tooltip("the move input")]
    [SerializeField] InputActionReference m_Move;

    [Tooltip("the jump input")]
    [SerializeField] InputActionReference m_Jump;

    [Tooltip("the crouch input")]
    [SerializeField] InputActionReference m_Crouch;

    // -- CharacterInputSource --
    public bool IsEnabled {
        get => enabled;
    }

    public CharacterInput.Frame Read() {
        var forward = Vector3.Normalize(Vector3.ProjectOnPlane(
            m_Look.forward,
            Vector3.up
        ));

        var right = Vector3.Normalize(Vector3.ProjectOnPlane(
            m_Look.right,
            Vector3.up
        ));

        var input = m_Move.action.ReadValue<Vector2>();

        // produce a new frame
        return new CharacterInput.DefaultFrame(
            forward * input.y + right * input.x,
            m_Jump.action.IsPressed(),
            m_Crouch.action.IsPressed()
        );
    }
}

}