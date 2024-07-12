using ThirdPerson;
using UnityEngine;

namespace Discone {

/// the discone input frame
public struct InputFrame: CharacterInputFrame {
    // -- props --
    /// if the loading input is active
    public bool IsLoadPressed;

    // -- queries --
    /// if there is any input
    public bool Any {
        get => IsLoadPressed || Main.Any;
    }

    /// if there is any move input
    public bool AnyMove {
        get => Main.AnyMove;
    }

    // -- CharacterInputFrame --
    public CharacterInputMain Main { get; set; }
}

}