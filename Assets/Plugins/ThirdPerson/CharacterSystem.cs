namespace ThirdPerson {

/// a character system; may be a state machine
abstract class CharacterSystem {
    // -- props --
    /// a name for this system
    protected string m_Name;

    /// the character name
    protected string m_CharacterName;

    /// the current phase
    protected CharacterPhase m_Phase;

    /// a ref to the character's input
    protected CharacterInput m_Input;

    /// a ref to the character's state
    protected CharacterState m_State;

    /// a ref to the character's events
    protected CharacterEvents m_Events;

    /// a ref to the character's tunables
    protected CharacterTunablesBase m_Tunables;

    /// a ref to the character's controller
    protected CharacterController m_Controller;

    // -- p/debug
    #if UNITY_EDITOR
    // if this system logs
    bool m_IsLogging;
    #endif

    // -- lifetime --
    /// create a new system
    public CharacterSystem(CharacterData d) {
        // set dependencies
        m_CharacterName = d.Name;
        m_Input = d.Input;
        m_State = d.State;
        m_Tunables = d.Tunables;
        m_Controller = d.Controller;
        m_Events = d.Events;

        // set props
        m_Name = this.GetType().Name;
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
            UnityEngine.Debug.Log($"{m_Name}: did change  {prev.Name} -> {next.Name}");
        }
        #endif
    }

    // -- c/debug
    #if UNITY_EDITOR
    /// log debug information for this system
    protected void UseLogging(bool isLogging = true) {
        m_IsLogging = isLogging;
    }
    #endif
}

}