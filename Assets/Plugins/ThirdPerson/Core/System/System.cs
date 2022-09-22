using System;
using UnityEngine;

namespace ThirdPerson {

/// a character system; may be a state machine
[Serializable]
public abstract class System {
    // -- state --
    [Header("state")]
    [Tooltip("the current phase")]
    [SerializeField] protected Phase m_Phase;

    // -- s/debug
    #if UNITY_EDITOR
    [Tooltip("if this system is logging")]
    [SerializeField] bool m_IsLogging;
    #endif

    // -- props --
    /// a name for this system
    protected string m_Name;

    // -- lifetime --
    /// create a new system
    public System() {
        // set props
        m_Name = this.GetType().Name;
    }

    public void Init() {
        // set more props
        m_Phase = InitInitialPhase();
    }

    /// construct the initial phase
    abstract protected Phase InitInitialPhase();

    // -- commands --
    /// update the system's current phase
    public virtual void Update(float delta) {
        m_Phase.Update(delta);
    }

    /// switch the system to a new phase and run the phase change lifecycle
    protected void ChangeTo(Phase next) {
        // if this is the same phase, don't do anything
        if (m_Phase.Equals(next)) {
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