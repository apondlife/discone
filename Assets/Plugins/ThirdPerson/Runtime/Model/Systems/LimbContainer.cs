using UnityEngine;

namespace ThirdPerson {

interface LimbContainer {
    /// the ik goal
    AvatarIKGoal Goal { get; }

    /// the bone the stride is anchored by
    CharacterBone Anchor { get; }

    /// the length of the limb
    float Length { get; }

    /// the offset of the bone used for placement
    Vector3 EndOffset { get; }

    /// the character container
    CharacterContainer Character { get; }
}

}