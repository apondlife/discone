using ThirdPerson;
namespace Discone {

/// the discone input frame
public readonly struct InputFrame: CharacterInputFrame {
    public CharacterInputMain Main { get; }

    public InputFrame(CharacterInputMain main) {
        Main = main;
    }
}

}