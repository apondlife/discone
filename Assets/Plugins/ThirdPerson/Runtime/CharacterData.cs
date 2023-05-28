namespace ThirdPerson {

/// a container for shuffling around character data
readonly struct CharacterData {
    /// -- props --
    /// the character's name
    public readonly string Name;

    /// the character's input
    public readonly CharacterInput Input;

    /// the character's current state
    public readonly CharacterState State;

    /// the character's tuning/constants
    public readonly CharacterTuning Tuning;

    /// the raw unity character controller
    public readonly CharacterController Controller;

    /// the character events
    public readonly CharacterEvents Events;

    // -- lifetime --
    /// create a new container
    public CharacterData(
        string name,
        CharacterInput input,
        CharacterState state,
        CharacterTuning tuning,
        CharacterController controller,
        CharacterEvents events
    ) {
        Name = name;
        Input = input;
        State = state;
        Tuning = tuning;
        Controller = controller;
        Events = events;
    }
}

}