using System;

namespace ThirdPerson {

// -- events --
[Flags]
public enum CharacterEvent {
    Jump = 1 << 0,
    Land = 1 << 1,
    Idle = 1 << 2,
}

// -- impl --
[Serializable]
public struct CharacterEventSet: IEquatable<CharacterEventSet> {
    // -- props --
    /// the events bitmask
    CharacterEvent m_Events;

    // -- commands --
    /// add an event to the set
    public void Add(CharacterEvent evt) {
        m_Events |= evt;
    }

    public void Clear() {
        m_Events = 0;
    }

    // -- queries --
    /// if the event is on
    public bool Contains(CharacterEvent evt) {
        return (m_Events & evt) != 0;
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
        return HashCode.Combine(m_Events);
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
}

}