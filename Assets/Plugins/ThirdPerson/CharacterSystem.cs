using System;
using UnityEngine;

namespace ThirdPerson {

/// a character system; may be a state machine
[Serializable]
abstract class CharacterSystem {
    // -- state --
    [Header("state")]
    [Tooltip("the current phase")]
    [SerializeField] protected CharacterPhase m_Phase;

    // -- props --
    /// a name for this system
    protected string m_Name;

    // TODO: (refactor) remove m_Data and this can just be a reference to the character
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

    private CharacterData m_Data;

    // -- p/debug
    #if UNITY_EDITOR
    // if this system logs
    bool m_IsLogging;
    #endif

    // -- lifetime --
    /// create a new system
    public CharacterSystem() {
        // set props
        m_Name = this.GetType().Name;
    }

    public void Init(CharacterData d) {
        // set more props
        m_Data = d;
        m_Phase = InitInitialPhase();
    }

    /// construct the initial phase
    abstract protected CharacterPhase InitInitialPhase();

    // -- commands --
    /// update the system's current phase
    public virtual void Update() {
        m_Phase.Update();
    }

    /// switch the system to a new phase and run the phase change lifecycle
    protected void ChangeTo(CharacterPhase next) {
        // if this is the same phase, don't do anything
        if (m_Phase.Equals(next)) {
            Debug.Log($"phase {m_Phase.Name} == {next.Name}");
            return;
        }

        var prev = m_Phase;

        // otherwise, run phase change lifecycle
        m_Phase.Exit();
        m_Phase = next;
        m_Phase.Enter();

        // debug
        #if UNITY_EDITOR
        if (m_IsLogging) {
            UnityEngine.Debug.Log($"{m_Name}: did change {prev.Name} -> {next.Name}");
        }
        #endif
    }

    // -- debug --
    #if UNITY_EDITOR
    /// log debug information for this system
    protected void UseLogging(bool isLogging = true) {
        m_IsLogging = isLogging;
    }
    #endif
}

}