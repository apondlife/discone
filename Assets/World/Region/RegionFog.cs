using System;
using UnityEngine;

namespace Discone {

/// the fog settings for a particular region
[Serializable]
public record RegionFog {
    // -- config --
    [Tooltip("the fog color")]
    public Color Color;

    [Tooltip("the fog start distance")]
    public float StartDistance;

    [Tooltip("the fog end distance")]
    public float EndDistance;

    // -- commands --
    /// lerp between two fogs
    public static void Lerp(
        ref RegionFog cur,
        RegionFog src,
        RegionFog dst,
        float t
    ) {
        cur.Color = Color.Lerp(
            src.Color,
            dst.Color,
            t
        );

        cur.EndDistance = Mathf.Lerp(
            src.EndDistance,
            dst.EndDistance,
            t
        );

        cur.StartDistance = Mathf.Lerp(
            src.StartDistance,
            dst.StartDistance,
            t
        );
    }

    // -- queries --
    /// create a copy of the region fog
    public RegionFog Copy() {
        return this with {};
    }
}

}