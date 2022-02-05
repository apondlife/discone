using UnityEngine;

namespace ThirdPerson {

/// the collision info
struct CharacterCollision {
    // -- props --
    /// the normal of the collision surface
    public readonly Vector3 Normal;

    // -- lifetime --
    /// create a new collision
    public CharacterCollision(Vector3 normal) {
        Normal = normal;
    }
}

}