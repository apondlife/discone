using System;

namespace ThirdPerson {

/// a character system; may be a state machine
[Serializable]
abstract class CharacterSystem: System {
    // -- props --
    /// the character name
    protected string m_CharacterName => m_Data.Name;

    /// a ref to the character's input
    /// TODO: CharacterContainer, CharacterContainerConvertible
    protected CharacterInput m_Input => m_Data.Input;

    /// a ref to the character's state
    /// TODO: CharacterContainer, CharacterContainerConvertible
    protected CharacterState m_State => m_Data.State;

    /// a ref to the character's events
    /// TODO: CharacterContainer, CharacterContainerConvertible
    protected CharacterEvents m_Events => m_Data.Events;

    /// a ref to the character's tunables
    /// TODO: CharacterContainer, CharacterContainerConvertible
    protected CharacterTunablesBase m_Tunables => m_Data.Tunables;

    /// a ref to the character's controller
    /// TODO: CharacterContainer, CharacterContainerConvertible
    protected CharacterController m_Controller => m_Data.Controller;

    /// TODO: (refactor) remove m_Data and this can just be a reference to the character
    private CharacterData m_Data;

    // -- lifetime --
    /// create a new system
    public CharacterSystem() {
        // set props
        m_Name = this.GetType().Name;
    }

    /// initialize this system with character data
    public void Init(CharacterData d) {
        base.Init();

        // set props
        m_Data = d;
    }
}

}