using System;

namespace Discone {

/// the serialized player state
[Serializable]
[StoreVersion(2)]
public record PlayerRec: StoreFile {
    // -- props --
    /// the local version
    public int V;

    /// the player's current character
    public CharacterRec Character;

    // -- lifetime --
    /// create a record
    public PlayerRec() {
        V = this.CurrentVersion();
    }

    // -- StoreFile --
    /// the file version
    public int Version => V;

    /// if this file has any data
    public bool HasData {
        get => Character != null;
    }
}

}