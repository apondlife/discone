using System;
using UnityEngine;

namespace ThirdPerson {

/// a character system; may be a state machine
[Serializable]
abstract class CharacterSystem: System {
    // -- props --
    /// the character name
    protected string m_CharacterName => m_Data.Name;

    /// a ref to the character's input
    protected CharacterInput m_Input => m_Data.Input;

    /// a ref to the character's state
    protected CharacterState m_State => m_Data.State;

    /// a ref to the character's events
    protected CharacterEvents m_Events => m_Data.Events;

    /// a ref to the character's tunables
    protected CharacterTunablesBase m_Tunables => m_Data.Tunables;

    /// a ref to the character's controller
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
        // set props
        m_Data = d;

        // run base init
        Init();
    }
}

}