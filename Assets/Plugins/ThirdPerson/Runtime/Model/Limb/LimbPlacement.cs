using UnityEngine;

namespace ThirdPerson {

/// a position and rotation placement for the goal
public readonly struct LimbPlacement {
    /// .
    public readonly Vector3 Pos;

    /// .
    public readonly Vector3 Normal;

    /// the distance to the hit surface, if any
    public readonly float Distance;

    /// the kind of placement cast
    public readonly CastResult Result;

    /// .
    public LimbPlacement(
        Vector3 pos,
        Vector3 normal,
        float distance,
        CastResult result
    ) {
        Pos = pos;
        Normal = normal;
        Distance = distance;
        Result = result;
    }

    public static LimbPlacement Hit(
        RaycastHit hit,
        float offset,
        CastResult result
    ) {
        return new LimbPlacement(
            hit.point,
            hit.normal,
            Mathf.Max(hit.distance - offset, 0f),
            result
        );
    }

    public static LimbPlacement Miss {
        get => new(
            Vector3.zero,
            Vector3.zero,
            0f,
            CastResult.Miss
        );
    }

    // -- types --
    /// the result of the placement cast
    public enum CastResult {
        Hit,
        OutOfRange,
        Miss
    }
}

}