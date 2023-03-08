using System;
using UnityEngine;

namespace ThirdPerson {

/// the collision info
public struct CharacterCollision: IEquatable<CharacterCollision> {
    // -- props --
    /// the normal at the on the collision surface
    public Vector3 Normal;

    /// the collision point
    public Vector3 Point;

    // -- lifetime --
    /// create a new collision
    public CharacterCollision(Vector3 normal, Vector3 point) {
        Normal = normal;
        Point = point;
    }

    // -- queries --
    /// if this is "some collision"
    public bool IsSome {
        get => !IsNone;
    }

    /// if this is "no collision"
    public bool IsNone {
        get => Normal == Vector3.zero;
    }

    // -- IEquatable --
    public override bool Equals(object o) {
        if (o is CharacterCollision c) {
            return Equals(c);
        } else {
            return false;
        }
    }

    public bool Equals(CharacterCollision o) {
        return (
            Normal == o.Normal &&
            Point  == o.Point
        );
    }

    public override int GetHashCode() {
        return HashCode.Combine(Normal, Point);
    }

    public static bool operator ==(
        CharacterCollision a,
        CharacterCollision b
    ) {
        return a.Equals(b);
    }

    public static bool operator !=(
        CharacterCollision a,
        CharacterCollision b
    ) {
        return !(a == b);
    }
}

}