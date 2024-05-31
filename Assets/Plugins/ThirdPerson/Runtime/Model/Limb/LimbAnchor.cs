using UnityEngine;

namespace ThirdPerson {

/// the current position of a bone
public readonly struct LimbAnchor {
    /// the root position
    public readonly Vector3 RootPos;

    /// the goal position
    public readonly Vector3 GoalPos;

    /// -- lifetime --
    public LimbAnchor(
        Vector3 rootPos,
        Vector3 goalPos
    ) {
        RootPos = rootPos;
        GoalPos = goalPos;
    }
}

}