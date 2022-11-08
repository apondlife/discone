using System;

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

    // -- StoreFile --
    /// the file version
    public int Version => V;

    /// if this record has any data
    public bool HasData {
        get => Flowers != null;
    }
}