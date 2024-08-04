using System;
using UnityEngine;

namespace ThirdPerson {

/// the forces on the character, categorized by type
[Serializable]
public struct CharacterForce: IEquatable<CharacterForce> {
    // -- props --
    /// the accumulated, uncategorized force
    public Vector3 Other;

    /// the force applied by friction
    public Vector3 Friction;

    // -- commands --
    /// clear all the forces on the character
    public void Clear() {
        Other = Vector3.zero;
        Friction = Vector3.zero;
    }

    // -- queries --
    /// the accumulated force
    public Vector3 All {
        get => Other + Friction;
    }

    /// interpolate between the lhs & rhs forces into this value
    public void Interpolate(
        CharacterForce src,
        CharacterForce dst,
        float k
    ) {
        Other = Vector3.Lerp(src.Other, dst.Other, k);
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
            Other == o.Other &&
            Friction == o.Friction
        );
    }

    public override int GetHashCode() {
        return HashCode.Combine(Other, Friction);
    }

    // -- operators --
    public static bool operator ==(
        CharacterForce lhs,
        CharacterForce rhs
    ) {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(
        CharacterForce lhs,
        CharacterForce rhs
    ) {
        return !(lhs == rhs);
    }

    public static CharacterForce operator +(
        CharacterForce lhs,
        Vector3 rhs
    ) {
        var result = lhs;
        result.Other += rhs;
        return result;
    }

    public static CharacterForce operator -(
        CharacterForce lhs,
        Vector3 rhs
    ) {
        var result = lhs;
        result.Other -= rhs;
        return result;
    }
}

}