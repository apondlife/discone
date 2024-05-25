using UnityEngine;

namespace ThirdPerson {

/// the current state of the limb
public record LimbState {
    // -- stride --
    /// if the limb is not striding
    public bool IsNotStriding;

    /// if the limb is currently free
    public bool IsFree;

    /// if the limb is currently held
    public bool IsHeld;

    /// the current ik position of the limb
    public Vector3 GoalPos;

    /// the current surface placement
    public LimbPlacement Placement;

    /// the distance to the held surface
    public float HeldDistance;

    /// the bone the stride is anchored by
    public LimbAnchor Anchor;

    /// an offset that translates the held position
    public Vector3 SlideOffset;

    /// the current stride length input scale
    public float InputScale;
}

}