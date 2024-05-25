using UnityEngine;

namespace ThirdPerson {

interface LimbContainer {
    /// the ik goal
    AvatarIKGoal Goal { get; }

    /// the position of the root
    Vector3 RootPos { get; }

    /// the tuning
    LimbTuning Tuning { get; }

    /// the state
    LimbState State { get; }

    /// the search direction
    Vector3 SearchDir { get; }

    /// the initial length
    float InitialLen { get; }

    /// the character container
    CharacterContainer Character { get; }
}

}