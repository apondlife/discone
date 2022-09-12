using UnityEngine;

/// the serialized flower state
[System.Serializable]
public record FlowerRec {
    // -- props --
    /// the flower's character
    public CharacterKey Key;

    /// the flower's world position
    public Vector3 Pos;
}