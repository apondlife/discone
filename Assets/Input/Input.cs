using ThirdPerson;

namespace Discone {

static class Input {
    /// if the loading input is active
    public static bool IsLoading(this CharacterInput<InputFrame> input) {
        return input.Curr.IsLoadPressed;
    }
}

}