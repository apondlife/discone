using UnityEngine;
using System;

/// the serialized character state
[Serializable]
public record CharacterRec {
    // -- props --
    /// the character
    public CharacterKey K;

    /// the world position
    public Vector3 P;

    /// the world rotation
    public Quaternion R;

    /// the last flower
    public FlowerRec F;

    // -- lifetime --
    [Obsolete("use the paramterized constructor")]
    public CharacterRec() {
    }

    /// create a new record
    public CharacterRec(
        CharacterKey key,
        Vector3 pos,
        Quaternion rot,
        FlowerRec flower
    ) {
        K = key;
        P = pos;
        R = rot;
        F = flower;
    }

    // -- queries --
    /// the character
    public CharacterKey Key => K;

    /// the world position
    public Vector3 Pos => P;

    /// the world rotation
    public Quaternion Rot => R;

    /// the last flower
    public FlowerRec Flower => F;
}