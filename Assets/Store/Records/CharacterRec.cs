using UnityEngine;
using System;

/// the serialized character state
[Serializable]
public record CharacterRec {
    // -- props --
    /// the character
    public CharacterKey Key;

    /// the world position
    public Vector3 Pos;

    /// the world rotation
    public Quaternion Rot;

    /// the last flower
    public FlowerRec Flower;
}