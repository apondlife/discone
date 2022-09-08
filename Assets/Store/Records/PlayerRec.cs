using System;

/// the serialized player state
[Serializable]
public struct PlayerRec {
    // -- props --
    /// the player's current character
    public CharacterRec Character;
}