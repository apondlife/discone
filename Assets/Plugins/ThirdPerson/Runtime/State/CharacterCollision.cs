using System;
using UnityEngine;

namespace ThirdPerson {

/// the collision info
[Serializable]
public struct CharacterCollision: IEquatable<CharacterCollision> {
    // -- constants --
    /// an empty collision
    public static readonly CharacterCollision None = new(
        normal: Vector3.zero,
        point: Vector3.zero,
        angle: 0f
    );

    // -- props --
    /// the normal at the on the collision surface
    public Vector3 Normal;

    /// the collision point
    public Vector3 Point;

    /// the surface angle relative to up
    public float Angle;

    // -- lifetime --
    /// create a new collision
    public CharacterCollision(
        Vector3 normal,
        Vector3 point,
        float angle
    ) {
        Normal = normal;
        Point = point;
        Angle = angle;
    }

    // -- commands --
    /// .
    public void SetNormal(Vector3 normal) {
        Normal = normal;
        Angle = Vector3.Angle(normal, Vector3.up);
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