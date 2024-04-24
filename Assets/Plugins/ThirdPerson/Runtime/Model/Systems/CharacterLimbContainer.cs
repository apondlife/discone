using UnityEngine;

namespace ThirdPerson {

interface CharacterLimbContainer {
    /// the ik goal
    AvatarIKGoal Goal { get; }

    /// the position of the root
    Vector3 RootPos { get; }

    /// the tuning for the limb
    CharacterLimbTuning Tuning { get; }

    /// the character container
    CharacterContainer Character { get; }

    /// the initial length of the limb
    float InitialLen { get; }

    /// the initial direction of the limb's root
    Vector3 InitialDir { get; }

    // TODO: unclear if we really want to init as our own anchor
    /// the bone the stride is anchored by
    CharacterLimbAnchor InitialAnchor { get; }
}

}