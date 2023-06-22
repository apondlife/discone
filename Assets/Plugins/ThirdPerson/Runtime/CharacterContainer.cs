
namespace ThirdPerson {

/// a dependency container for character components
interface CharacterContainer {
    // -- queries --
    /// the name of the character
    public string Name { get; }

    /// the tuning
    public CharacterTuning Tuning { get; }

    /// the input
    public CharacterInput Input { get; }

    /// the state
    public CharacterState State { get; }

    /// the events
    public CharacterEvents Events { get; }

    /// the controller
    public CharacterController Controller { get; }

    /// the model
    public CharacterModel Model { get; }
}

}
