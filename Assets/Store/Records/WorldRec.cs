using System;

/// the serialized world state
[Serializable]
public struct WorldRec {
    // -- props --
    /// all the flowers in the world
    public FlowerRec[] Flowers;
}