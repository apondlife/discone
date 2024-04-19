using UnityEngine;

namespace ThirdPerson {

interface LimbContainer {
    /// the ik goal
    AvatarIKGoal Goal { get; }

    /// the initial length of the limb
    float InitialLen { get; }

    /// the initial direction of the limb's root
    Vector3 InitialDir { get; }

    /// the character container
    CharacterContainer Character { get; }

    // TODO: unclear if we really want to init as our own anchor
    /// the bone the stride is anchored by
    CharacterLimbAnchor InitialAnchor { get; }
}

}