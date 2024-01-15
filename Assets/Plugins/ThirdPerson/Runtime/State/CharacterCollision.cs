using System;
using UnityEngine;

namespace ThirdPerson {

/// the source(s) of a collision
[Flags]
public enum CollisionSource {
    Move    = 1 << 0,
    Overlap = 1 << 1,
}

/// the collision info
[Serializable]
public struct CharacterCollision: IEquatable<CharacterCollision> {
    // -- constants --
    /// an empty collision
    public static readonly CharacterCollision None = new(
        normal: Vector3.zero,
        point: Vector3.zero,
        source: 0,
        angle: 0f
    );

    // -- props --
    /// the normal at the on the collision surface
    public Vector3 Normal;

    /// the collision point
    public Vector3 Point;

    /// the collision sources(s)
    public CollisionSource Source;

    /// the surface angle relative to up
    public float Angle;

    // -- lifetime --
    /// create a new collision
    public CharacterCollision(
        Vector3 normal,
        Vector3 point,
        CollisionSource source
    ) {
        Normal = normal;
        Point = point;
        Source = source;
        Angle = Vector3.Angle(normal, Vector3.up);
    }

    /// create a new collision
    public CharacterCollision(
        Vector3 normal,
        Vector3 point,
        CollisionSource source,
        float angle
    ) {
        Normal = normal;
        Point = point;
        Angle = angle;
        Source = source;
    }

    // -- commands --
    /// .
    public void SetNormal(Vector3 normal) {
        Normal = normal;
        Angle = Vector3.Angle(normal, Vector3.up);
    }

    /// add another collision source(s) & overwrite updated state
    public void AddSource(CollisionSource source, Vector3 point) {
        Source |= source;
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