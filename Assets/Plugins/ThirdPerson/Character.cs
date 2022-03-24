namespace ThirdPerson {

/// a container for shuffling around character data
readonly struct Character {
    /// -- props --
    /// the character's input
    public readonly CharacterInput Input;

    /// the character's current state
    public readonly CharacterState State;

    /// the character's tunables/constants
    public readonly CharacterTunablesBase Tunables;

    /// the raw unity character controller
    public readonly CharacterController Controller;

    // -- lifetime --
    /// create a new container
    public Character(
        CharacterInput input,
        CharacterState state,
        CharacterTunablesBase tunables,
        CharacterController controller
    ) {
        Input = input;
        State = state;
        Tunables = tunables;
        Controller = controller;
    }
}

}