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
}