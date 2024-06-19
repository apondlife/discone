using System;

namespace ThirdPerson {

/// an id to a particular jump in the jump sequence
[Serializable]
public struct JumpId: IEquatable<JumpId> {
    /// the index of the tuning in the jump list
    public int Index;

    /// the number of times this jump has been used
    public int Count;

    // -- IEquatable --
    public bool Equals(JumpId other) {
        return Index == other.Index && Count == other.Count;
    }

    public override bool Equals(object obj) {
        return obj is JumpId other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Index, Count);
    }

    // -- operators --
    public static bool operator ==(
        JumpId a,
        JumpId b
    ) {
        return a.Equals(b);
    }

    public static bool operator !=(
        JumpId a,
        JumpId b
    ) {
        return !(a == b);
    }
}

}