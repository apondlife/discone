using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a player's default input source for controlling characters
public sealed class PlayerInputSource: PlayerInputSource<CharacterInputFrame.Default> {
    public override CharacterInputFrame.Default Read() {
        return new CharacterInputFrame.Default(
            main: ReadMain()
        );
    }
}

/// a player's input source for controlling characters
public abstract class PlayerInputSource<F>: MonoBehaviour, CharacterInputSource<F> where F: CharacterInputFrame {
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

    // -- queries --
    public CharacterInputMain ReadMain() {
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
        return new CharacterInputMain(
            forward * input.y + right * input.x,
            m_Jump.action.IsPressed(),
            m_Crouch.action.IsPressed()
        );
    }

    // -- CharacterInputSource --
    public virtual bool IsEnabled {
        get => enabled;
        set => enabled = value;
    }

    public abstract F Read();
}

}