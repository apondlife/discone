using System;
using UnityEngine;

namespace ThirdPerson {

/// the forces on the character, categorized by type
[Serializable]
public struct CharacterForce: IEquatable<CharacterForce> {
    // -- props --
    /// the input force
    public Vector3 Input;

    /// the force applied by gravity
    public Vector3 Gravity;

    /// the force applied by friction
    public Vector3 Friction;

    // -- commands --
    /// clear all the forces on the character
    public void Clear() {
        Input = Vector3.zero;
        Gravity = Vector3.zero;
        Friction = Vector3.zero;
    }

    // -- queries --
    /// the accumulated force
    public Vector3 All {
        get => Input + Gravity + Friction;
    }

    /// the accumulated force without gravity
    public Vector3 WithoutGravity {
        get => Input + Friction;
    }

    /// interpolate between the lhs & rhs forces into this value
    public void Interpolate(
        CharacterForce src,
        CharacterForce dst,
        float k
    ) {
        Input = Vector3.Lerp(src.Input, dst.Input, k);
        Gravity = Vector3.Lerp(src.Gravity, dst.Gravity, k);
        Friction = Vector3.Lerp(src.Friction, dst.Friction, k);
    }

    // -- IEquatable --
    public override bool Equals(object o) {
        if (o is CharacterForce c) {
            return Equals(c);
        } else {
            return false;
        }
    }

    public bool Equals(CharacterForce o) {
        return (
            Input == o.Input &&
            Gravity == o.Gravity &&
            Friction == o.Friction
        );
    }

    public override int GetHashCode() {
        return HashCode.Combine(Input, Gravity, Friction);
    }

    // -- operators --
    public static bool operator ==(
        CharacterForce a,
        CharacterForce b
    ) {
        return a.Equals(b);
    }

    public static bool operator !=(
        CharacterForce a,
        CharacterForce b
    ) {
        return !(a == b);
    }
}

}