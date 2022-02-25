namespace ThirdPerson {

/// a source for character input
public interface CharacterInputSource {
    /// read a frame of input
    CharacterInput.Frame Read();
}

}