using UnityEngine;

namespace ThirdPerson {

/// the collision info
public readonly struct CharacterCollision {
    // -- props --
    /// the normal at the on the collision surface
    public readonly Vector3 Normal;

    /// the collision point
    public readonly Vector3 Point;

    // -- lifetime --
    /// create a new collision
    public CharacterCollision(Vector3 normal, Vector3 point) {
        Normal = normal;
        Point = point;
    }
}

}