using ThirdPerson;
using UnityEngine;

namespace Discone {

/// the discone input frame
public readonly struct InputFrame: CharacterInputFrame {
    // -- props --
    /// if the loading input is active
    public readonly bool IsLoadPressed;

    // -- lifetime --
    public InputFrame(
        CharacterInputMain main,
        bool isLoadPressed
    ) {
        Main = main;
        IsLoadPressed = isLoadPressed;
    }

    // -- queries --
    /// if there is any input
    public bool Any {
        get => IsLoadPressed || Main.Any;
    }

    /// if there is any move input
    public bool AnyMove {
        get => Main.Move != Vector3.zero;
    }

    // -- CharacterInputFrame --
    public CharacterInputMain Main { get; }
}

}