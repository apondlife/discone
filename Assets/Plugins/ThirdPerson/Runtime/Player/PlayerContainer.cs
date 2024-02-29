namespace ThirdPerson {

/// a dependency container for player components
public interface PlayerContainer {
    // -- queries --
    /// the player's current character
    public Character Character { get; }
}

}