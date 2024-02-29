using System;

namespace ThirdPerson {

// -- events --
[Flags]
public enum CharacterEvent {
    Jump = 1 << 0,
    Land = 1 << 1,
    Idle = 1 << 2,
    Move = 1 << 3,
}

// -- impl --
[Serializable]
public struct CharacterEventSet: IEquatable<CharacterEventSet> {
    // -- props --
    /// the events bitmask
    public CharacterEvent Mask;

    // -- commands --
    /// add an event to the set
    public void Add(CharacterEvent evt) {
        Mask |= evt;
    }

    /// clear all events
    public void Clear() {
        Mask = 0;
    }

    // -- queries --
    /// if the event is on
    public bool Contains(CharacterEvent evt) {
        return (Mask & evt) != 0;
    }

    /// if the set is empty
    public bool IsEmpty {
        get => Mask == 0;
    }

    // -- IEquatable --
    public override bool Equals(object o) {
        if (o is CharacterEventSet c) {
            return Equals(c);
        } else {
            return false;
        }
    }

    public bool Equals(CharacterEventSet o) {
        return true;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Mask);
    }

    public static bool operator ==(
        CharacterEventSet a,
        CharacterEventSet b
    ) {
        return a.Equals(b);
    }

    public static bool operator !=(
        CharacterEventSet a,
        CharacterEventSet b
    ) {
        return !(a == b);
    }

    // -- Debug --
    public override string ToString() {
        return Mask.ToString();
    }
}

}