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

    // -- props --
    /// the input actions
    PlayerInputActions m_Actions;

    // -- commands --
    public void Bind(PlayerInputActions actions) {
        m_Actions = actions;
    }

    // -- PlayerInputSource --
    protected override PlayerInputActions Actions {
        get => m_Actions;
    }

    protected override Transform Look {
        get => m_Look;
    }

    protected override void ReadNext(ref CharacterInputFrame.Default frame) {
        ReadMain(ref frame);
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

    // -- queries --
    /// the input actions
    protected abstract PlayerInputActions Actions { get; }

    // the transform for the player's look viewpoint
    protected abstract Transform Look { get; }

    /// reads the next frame of input
    protected abstract void ReadNext(ref F frame);

    /// reads the next frame of third person input
    protected void ReadMain(ref F frame) {
        if (!Look) {
            Log.Player.E($"input source has no look transform");
            return;
        }

        var forward = Vector3.Normalize(Vector3.ProjectOnPlane(
            Look.forward,
            Vector3.up
        ));

        var right = Vector3.Normalize(Vector3.ProjectOnPlane(
            Look.right,
            Vector3.up
        ));

        var input = Vector3.ClampMagnitude(Actions.Move, 1f);

        // produce a new frame
        frame.Main = new(
            move: Vector3.ClampMagnitude(input.y * forward + input.x * right, 1f),
            isJumpPressed: Actions.IsJumpPressed
        );
    }

    // -- CharacterInputSource --
    public void Read(ref F frame) {
         if (Actions != null) {
             ReadNext(ref frame);
         }
    }

    public virtual bool IsEnabled {
        get => m_IsEnabled;
        set => m_IsEnabled = value;
    }
}

}