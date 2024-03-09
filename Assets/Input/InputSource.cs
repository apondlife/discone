using ThirdPerson;

namespace Discone {

public sealed class InputSource: PlayerInputSource<InputFrame> {
    public override InputFrame Read() {
        return new InputFrame(
            main: ReadMain()
        );
    }
}

}