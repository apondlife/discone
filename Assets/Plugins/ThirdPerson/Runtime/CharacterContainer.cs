
using UnityEngine;

namespace ThirdPerson {

/// a dependency container for character components
public interface CharacterContainer {
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
    CharacterInputQuery Inputs { get; }

    /// the events
    CharacterEvents Events { get; }

    /// the controller
    CharacterController Controller { get; }

    /// the rig
    CharacterRig Rig { get; }

    /// the model
    CharacterModel Model { get; }

    /// the character's animator
    Animator Animator { get; }
}

}