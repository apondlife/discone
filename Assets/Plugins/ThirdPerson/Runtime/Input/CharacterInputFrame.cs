using UnityEngine;

namespace ThirdPerson {

/// the minimal frame of input for third person to work
public interface CharacterInputFrame {
    /// the input frame's main module
    CharacterInputMain Main { get; }

    // -- queries --
    /// the projected position of the move analog stick
    public Vector3 Move {
        get => Main.Move;
    }

    /// if jump is pressed
    public bool IsJumpPressed {
        get => Main.IsJumpPressed;
    }

    // -- default --
    /// the default input frame
    public readonly struct Default: CharacterInputFrame {
        public CharacterInputMain Main { get; }

        public Default(CharacterInputMain main) {
            Main = main;
        }
    }
}

/// the input frame's main module
public readonly struct CharacterInputMain {
    // -- props --
    /// the projected position of the move analog stick
    public readonly Vector3 Move;

    /// if jump is pressed
    public readonly bool IsJumpPressed;

    // -- lifetime --
    /// create a new frame
    public CharacterInputMain(
        Vector3 move,
        bool isJumpPressed
    ) {
        Move = move;
        IsJumpPressed = isJumpPressed;
    }

    // -- queries --
    /// if there is any input
    public bool Any {
        get => IsJumpPressed || Move != Vector3.zero;
    }
}

}