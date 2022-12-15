using System;

#if UNITY_EDITOR
using System.Reflection;
using UnityEngine;
#endif

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
        m_Name = GetType().Name;
    }

    // -- lifecycle --
    /// initialize this system with character data
    public void Init(CharacterData d) {
        base.Init();

        // set props
        m_Data = d;
    }

    #if UNITY_EDITOR
    public string Name {
        get => m_Name;
    }

    public override void Update(float delta) {
        base.Update(delta);

        m_State.SystemPhases[m_Name] = m_Phase.Name;
    }

    public void RestorePhase(string name) {
        foreach (var prop in GetType().GetTypeInfo().DeclaredProperties) {
            if (prop.Name == name) {
                if (prop.PropertyType != typeof(Phase)) {
                    Debug.LogError("[system] tried to restore a phase that was not a phase: {name}");
                    return;
                }

                m_Phase = (Phase)prop.GetValue(this);;
            }
        }
    }
    #endif
}

}