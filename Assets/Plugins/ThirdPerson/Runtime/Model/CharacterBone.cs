using UnityEngine;

namespace ThirdPerson {

/// the current position of a bone
public interface CharacterBone {
    /// the root position
    public Vector3 RootPos { get; }

    /// the goal position
    public Vector3 GoalPos { get; }
}

}