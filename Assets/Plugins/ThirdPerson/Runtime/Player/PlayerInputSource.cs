using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a player's default input source for controlling characters
[Serializable]
public sealed class PlayerInputSource: PlayerInputSource<CharacterInputFrame.Default> {
    public override CharacterInputFrame.Default Read() {
        return new CharacterInputFrame.Default(
            main: ReadMain()
        );
    }
}

/// a player's input source for controlling characters
[Serializable]
public abstract class PlayerInputSource<F>: CharacterInputSource<F> where F: CharacterInputFrame {
    // -- state --
    [Header("state")]
    [Tooltip("if this input source is enabled")]
    [SerializeField] bool m_IsEnabled;

    // -- refs --
    [Header("refs")]
    [Tooltip("the transform for the player's look viewpoint")]
    [SerializeField] Transform m_Look;

    [Tooltip("the move input")]
    [SerializeField] InputActionReference m_Move;

    [Tooltip("the jump input")]
    [SerializeField] InputActionReference m_Jump;

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

        var input = Vector3.ClampMagnitude(m_Move.action.ReadValue<Vector2>(), 1f);

        // produce a new frame
        return new CharacterInputMain(
            move: Vector3.ClampMagnitude(input.y * forward + input.x * right, 1f),
            isJumpPressed: m_Jump.action.IsPressed()
        );
    }

    // -- CharacterInputSource --
    public virtual bool IsEnabled {
        get => m_IsEnabled;
        set => m_IsEnabled = value;
    }

    public abstract F Read();
}

}