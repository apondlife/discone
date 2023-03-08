namespace ThirdPerson {

/// a source for character input
public interface CharacterInputSource {
    /// if the input is enabled
    bool IsEnabled { get; }

    /// read a frame of input
    CharacterInput.Frame Read();
}

}