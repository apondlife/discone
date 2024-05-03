using UnityEngine;

namespace ThirdPerson {

interface LimbContainer {
    /// the ik goal
    AvatarIKGoal Goal { get; }

    /// the position of the root
    Vector3 RootPos { get; }

    /// the tuning for the limb
    LimbTuning Tuning { get; }

    /// the character container
    CharacterContainer Character { get; }

    /// the initial length of the limb
    float InitialLen { get; }

    /// the search direction of the limb
    Vector3 SearchDir { get; }

    // TODO: unclear if we really want to init as our own anchor
    /// the bone the stride is anchored by
    LimbAnchor InitialAnchor { get; }
}

}