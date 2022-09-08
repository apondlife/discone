using System;

// TODO: make all these classes or records
/// the serialized world state
[Serializable]
public record WorldRec {
    // -- props --
    /// all the flowers in the world
    public FlowerRec[] Flowers;
}