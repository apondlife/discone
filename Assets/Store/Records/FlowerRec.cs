using UnityEngine;

/// the serialized flower state
[System.Serializable]
public record FlowerRec {
    // -- props --
    /// the flower's character
    public CharacterKey Key;

    /// the flower's world position
    public Vector3 Pos;

    // -- lifetime --
    /// instantiate a rec from a flower
    public static FlowerRec From(CharacterFlower flower) {
        if (flower == null) {
            return null;
        }

        return new FlowerRec() {
            Key = flower.Key,
            Pos = flower.transform.position,
        };
    }
}