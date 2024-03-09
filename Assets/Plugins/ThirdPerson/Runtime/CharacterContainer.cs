
namespace ThirdPerson {

/// a dependency container for character components
interface CharacterContainer {
    // -- queries --
    /// the name of the character
    string Name { get; }

    /// if the character is paused
    bool IsPaused { get; }

    /// the tuning
    CharacterTuning Tuning { get; }

    /// the state
    CharacterState State { get; }

    /// the input state
    CharacterInputQuery InputQuery { get; }

    /// the events
    CharacterEvents Events { get; }

    /// the controller
    CharacterController Controller { get; }

    /// the model
    CharacterModel Model { get; }
}

}