using System;

/// an attribute for specifying a file's current version
[AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class StoreVersion: Attribute {
    // -- props --
    /// the current version
    public readonly int Value;

    // -- lifetime --
    /// create an attribute
    public StoreVersion(int value) {
        Value = value;
    }
}