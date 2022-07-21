using System;
using UnityEngine;

namespace ThirdPerson {

/// the type of collision surface
public enum CollisionSurface {
    Ground,
    Wall,
}

/// the collision info
public readonly struct CharacterCollision: IEquatable<CharacterCollision> {
    // -- props --
    /// the normal at the on the collision surface
    public readonly Vector3 Normal;

    /// the collision point
    public readonly Vector3 Point;

    /// the surface for the collision, if any
    public readonly CollisionSurface Surface;

    // -- lifetime --
    /// create a new collision
    public CharacterCollision(Vector3 normal, Vector3 point, CollisionSurface surface) {
        Normal = normal;
        Point = point;
        Surface = surface;
    }

    // -- queries --
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