using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a player's input source for controlling characters
public sealed class PlayerInputSource: MonoBehaviour, CharacterInputSource {
    // -- fields --
    [Header("references")]
    [Tooltip("the transform for the player's look viewpoint")]
    [SerializeField] private Transform m_Look;

    [Tooltip("the move input")]
    [SerializeField] private InputActionReference m_Move;

    [Tooltip("the jump input")]
    [SerializeField] private InputActionReference m_Jump;

    // -- CharacterInputSource --
    /// read the next frame of input
    public CharacterInput.Frame Read() {
        var forward = Vector3.Normalize(Vector3.ProjectOnPlane(
            m_Look.transform.forward,
            Vector3.up
        ));

        var right = m_Look.transform.right;
        var pInput = m_Move.action.ReadValue<Vector2>();
        var move = forward * pInput.y + right * pInput.x;

        // produce a new frame
        return new CharacterInput.Frame(
            m_Jump.action.IsPressed(),
            move
        );
    }
}

}