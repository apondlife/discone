using System;

/// the serialized player state
[Serializable]
public record PlayerRec {
    // -- props --
    /// the player's current character
    public CharacterRec Character;
}