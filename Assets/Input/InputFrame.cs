using ThirdPerson;

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

    // -- CharacterInputFrame --
    public CharacterInputMain Main { get; }
}

}