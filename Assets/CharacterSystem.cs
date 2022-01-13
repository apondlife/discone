using UnityEngine;

/// a character system, a
abstract class CharacterSystem {
    // -- props --
    /// a name for this system
    private string m_Name;

    /// the current phase
    protected CharacterPhase m_Phase;

    /// a shorthand reference to the character's input
    protected CharacterInput m_Input;

    /// a shorthand reference to the character's state
    protected CharacterState m_State;

    /// a shorthand reference to the character's tunables
    protected CharacterTunables m_Tunables;

    // -- lifetime --
    /// create a new system
    public CharacterSystem(CharacterInput input, CharacterState state, CharacterTunables tunables) {
        // set dependencies
        m_Input = input;
        m_State = state;
        m_Tunables = tunables;

        // set props
        m_Name = this.GetType().Name;
        m_Phase = InitInitialPhase();
    }

    /// construct the initial phase
    abstract protected CharacterPhase InitInitialPhase();

    // -- commands --
    /// update the system's current phase
    public void Update() {
        m_Phase.Update();
    }

    /// switch the system to a new phase and run the phase change lifecycle
    protected void ChangeTo(CharacterPhase next) {
        // if this is the same phase, don't do anything
        if (m_Phase.Equals(next)) {
            return;
        }

        var prev = m_Phase;
        Debug.Log($"{m_Name}: will change {prev.Name} -> {next.Name}");

        // otherwise, run phase change lifecycle
        m_Phase.Exit();
        m_Phase = next;
        m_Phase.Enter();

        Debug.Log($"{m_Name}: did change  {prev.Name} -> {next.Name}");
    }
}