using UnityEngine;

namespace ThirdPerson {

interface LimbContainer {
    /// the ik goal
    AvatarIKGoal Goal { get; }

    /// the position of the root
    Vector3 RootPos { get; }

    /// the tuning for the limb
    LimbTuning Tuning { get; }

    /// the state of the limb
    LimbState State { get;  }

    /// the search direction of the limb
    Vector3 SearchDir { get; }

    /// the initial length of the limb
    float InitialLen { get; }

    /// the character container
    CharacterContainer Character { get; }
}

}