/// the base class for every phase
public class Character {
    /// -- props --
    /// the character's input
    private CharacterInput m_Input;

    /// the character's current state
    private CharacterState m_State;

    /// the character's tunables/constants
    private CharacterTunables m_Tunables;

    // -- lifetime --
    /// create a new state
    public Character(CharacterInput input, CharacterState state, CharacterTunables tunables) {
        m_Input = input;
        m_State = state;
        m_Tunables = tunables;
    }

    // -- queries --
    public CharacterInput Input {
        get => m_Input;
    }
}
