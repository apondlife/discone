namespace ThirdPerson {

/// a dependency container for player components
interface PlayerContainer {
    // -- queries --
    /// the player's current character's camera
    Camera Camera { get; }
}

}