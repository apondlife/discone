using UnityEngine;

namespace ThirdPerson {

/// the current position of a bone
public interface LimbAnchor {
    /// the root position
    Vector3 RootPos { get; }

    /// the goal position
    Vector3 GoalPos { get; }
}

}