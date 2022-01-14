using UnityEngine;

/// a container for shuffling around character data
public readonly struct Character {
    /// -- props --
    /// the character's input
    public readonly CharacterInput Input;

    /// the character's current state
    public readonly CharacterState State;

    /// the character's tunables/constants
    public readonly CharacterTunables Tunables;

    /// the raw unity character controller
    public readonly CharacterController Controller;

    // -- lifetime --
    /// create a new container
    public Character(
        CharacterInput input,
        CharacterState state,
        CharacterTunables tunables,
        CharacterController controller
    ) {
        Input = input;
        State = state;
        Tunables = tunables;
        Controller = controller;
    }
}
