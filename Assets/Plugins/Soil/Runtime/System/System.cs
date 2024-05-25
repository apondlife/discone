using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace Soil {

/// a character system; may be a state machine
[Serializable]
public abstract class System<Container> {
    // -- state --
    [Header("state")]
    [Tooltip("the current phase")]
    [SerializeField] protected Phase<Container> m_Phase;

    // -- s/debug
    // TODO: investigate why we can't #if UNITY_EDITOR these properties
    [Tooltip("if this system is disabled")]
    [SerializeField] bool m_IsDisabled;

    [Tooltip("if this system is logging")]
    [SerializeField] bool m_IsLogging;

    // -- deps --
    // the system container
    protected Container c;

    // -- props --
    /// a name for this system
    protected string m_Name;

    // -- p/debug
    #if UNITY_EDITOR
    /// the set of phases we've changed to this frame
    List<string> m_Debug_Phases = new();
    #endif

    // -- lifetime --
    /// create a new system
    public System() {
        // set props
        m_Name = GetType().Name;
    }

    /// initialize & configure the system
    public virtual void Init(Container c) {
        this.c = c;

        var phase = InitInitialPhase();
        SetPhase(phase);
        phase.Enter(this, c);
    }

    /// construct the initial phase
    protected abstract Phase<Container> InitInitialPhase();

    // -- commands --
    /// update the system's current phase
    public virtual void Update(float delta) {
        #if UNITY_EDITOR
        // clear debug phases
        m_Debug_Phases.Clear();

        // if the system is disabled, turn it off
        if (m_IsDisabled) {
            return;
        }

        // ensure a phase!
        if (m_Phase.Update == null) {
            Log.System.E($"must call init! {this}!");
            return;
        }
        #endif

        var state = State;
        state.PhaseElapsed += delta;
        State = state;

        m_Phase.Update(delta, this, c);
    }

    /// switch to a new phase and run the phase change lifecycle
    public void ChangeTo(Phase<Container> next) {
        // if this is the same phase, don't do anything
        if (m_Phase.Equals(next)) {
            return;
        }

        #if UNITY_EDITOR
        // add the debug phase
        m_Debug_Phases.Add(next.Name);

        // track the prev phase for logging
        var prev = m_Phase;
        #endif

        // otherwise, run phase change lifecycle
        m_Phase.Exit(this, c);
        SetPhase(next);
        m_Phase.Enter(this, c);

        // debug
        #if UNITY_EDITOR
        if (m_IsLogging) {
            Log.System.I($"{m_Name}: did change {prev.Name} -> {next.Name}");
        }
        #endif
    }

    /// switch to a new phase, run the phase change lifecycle, and run an immediate update
    public void ChangeToImmediate(Phase<Container> next, float delta) {
        // if we hit a phase loop, we need to terminate immediately
        #if UNITY_EDITOR
        if (m_Debug_Phases.Contains(next.Name)) {
            m_Debug_Phases.Add(next.Name);
            Log.System.E($"phase change recursion:\n{string.Join("->", m_Debug_Phases)}");
            return;
        }
        #endif

        // change phase
        ChangeTo(next);

        // and run the update immediately
        m_Phase.Update(delta, this, c);
    }

    /// set the current phase & initialize its state w/o calling events
    protected void SetPhase(Phase<Container> phase) {
        m_Phase = phase;

        var state = State;
        state.PhaseName = m_Phase.Name;
        state.PhaseStart = Time.time;
        state.PhaseElapsed = 0f;
        State = state;
    }

    // -- queries --
    /// the system's current state
    protected abstract SystemState State { get; set; }

    /// .
    public float PhaseStart {
        get => State.PhaseStart;
    }

    /// .
    public float PhaseElapsed {
        get => State.PhaseElapsed;
    }

    // -- debug --
    /// the current phase name
    public string Debug_PhaseName {
        get => State.PhaseName;
    }

    #if UNITY_EDITOR
    /// log debug information for this system
    protected void UseLogging(bool isLogging = true) {
        m_IsLogging = isLogging;
    }
    #endif
}

}