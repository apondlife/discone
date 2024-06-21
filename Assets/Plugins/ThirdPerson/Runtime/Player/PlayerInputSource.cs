using System;
using UnityEngine;

namespace ThirdPerson {

/// a player's default input source for controlling characters
[Serializable]
public sealed class PlayerInputSource: PlayerInputSource<CharacterInputFrame.Default> {
    // -- fields --
    [Header("fields")]
    [Tooltip("the transform for the player's look viewpoint")]
    [SerializeField] Transform m_Look;

    // -- PlayerInputSource --
    protected override Transform Look {
        get => m_Look;
    }

    public override CharacterInputFrame.Default Read() {
        return new CharacterInputFrame.Default(
            main: ReadMain()
        );
    }
}

/// a source of the current input state
public interface PlayerInputActions {
    /// the move vector in input-space
    Vector2 Move { get; }

    /// if the jump input is pressed
    bool IsJumpPressed { get; }
}

/// a player's input source for controlling characters
[Serializable]
public abstract class PlayerInputSource<F>: CharacterInputSource<F> where F: CharacterInputFrame {
    // -- state --
    [Header("state")]
    [Tooltip("if this input source is enabled")]
    [SerializeField] bool m_IsEnabled;

    // -- props --
    /// the input actions
    PlayerInputActions m_Actions;

    // -- commands --
    public void Bind(PlayerInputActions actions) {
        m_Actions = actions;
    }

    // -- queries --
    public CharacterInputMain ReadMain() {
        if (!Look) {
            return new CharacterInputMain();
        }

        var forward = Vector3.Normalize(Vector3.ProjectOnPlane(
            Look.forward,
            Vector3.up
        ));

        var right = Vector3.Normalize(Vector3.ProjectOnPlane(
            Look.right,
            Vector3.up
        ));

        var input = Vector3.ClampMagnitude(m_Actions.Move, 1f);

        // produce a new frame
        return new CharacterInputMain(
            move: Vector3.ClampMagnitude(input.y * forward + input.x * right, 1f),
            isJumpPressed: m_Actions.IsJumpPressed
        );
    }

    // the transform for the player's look viewpoint
    protected abstract Transform Look { get; }

    // -- CharacterInputSource --
    public virtual bool IsEnabled {
        get => m_IsEnabled;
        set => m_IsEnabled = value;
    }

    public abstract F Read();
}

}