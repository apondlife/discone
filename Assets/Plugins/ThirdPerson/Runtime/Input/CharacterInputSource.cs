namespace ThirdPerson {

/// a source for character input
public interface CharacterInputSource<F> where F: CharacterInputFrame {
    /// if the input is enabled
    bool IsEnabled { get; }

    /// read a frame of input
    void Read(ref F frame);
}

}