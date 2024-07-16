using System;
using UnityEngine;

namespace ThirdPerson {

// -- events --
[Flags]
public enum CharacterEvent {
    Jump = 1 << 0,
    Land = 1 << 1,
    Idle = 1 << 2,
    Move = 1 << 3,
    Step_LeftFoot = 1 << 4,
    Step_RightFoot = 1 << 5,
    Step_LeftHand = 1 << 6,
    Step_RightHand = 1 << 7,

    // -- aggregates --
    Step = Step_LeftFoot | Step_RightFoot | Step_LeftHand | Step_RightHand
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

    // -- debug --
    public override string ToString() {
        return Mask.ToString();
    }
}

// -- extensions --
static class CharacterEventExt {
    /// map the goal into the matching `Step_` character event
    public static CharacterEvent AsStepEvent(this AvatarIKGoal goal) {
        return goal switch {
            AvatarIKGoal.LeftFoot => CharacterEvent.Step_LeftFoot,
            AvatarIKGoal.RightFoot => CharacterEvent.Step_RightFoot,
            AvatarIKGoal.LeftHand => CharacterEvent.Step_LeftHand,
            _ => CharacterEvent.Step_RightHand,
        };
    }
}

}