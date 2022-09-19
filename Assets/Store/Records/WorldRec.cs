using System;

// TODO: make all these classes or records
/// the serialized world state
[Serializable]
[StoreVersion(2)]
public record WorldRec: StoreFile {
    // -- props --
    /// the local version
    public int V;

    /// all the flowers in the world
    public FlowerRec[] Flowers;

    // -- lifetime --
    /// create a record
    public WorldRec() {
        V = this.CurrentVersion();
    }

    // -- queries --
    public int Version => V;
}