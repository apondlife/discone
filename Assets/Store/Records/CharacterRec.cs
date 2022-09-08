using UnityEngine;
using System;

/// the serialized character state
[Serializable]
public struct CharacterRec {
    // -- props --
    /// the character
    public CharacterKey Key;

    /// the world position
    public Vector3 Pos;

    /// the last flower
    public FlowerRec Flower;

    // -- factories --
    /// instantiate a rec from a character
    public static CharacterRec From(DisconeCharacter character) {
        return new CharacterRec() {
            Key = character.Key,
            Pos = character.transform.position,
            Flower = FlowerRec.From(character.Checkpoint.Flower),
        };
    }
}