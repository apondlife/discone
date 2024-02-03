using System;
using NaughtyAttributes;
using UnityEngine;

namespace Discone {

/// the fog settings for a particular region
[Serializable]
public record RegionFog {
    // -- config --
    [Tooltip("the fog color")]
    public Color Color;

    [Tooltip("the min distance for the distance fog")]
    public float StartDistance;

    [Tooltip("the max distance for the distance fog")]
    public float EndDistance;

    [Tooltip("the height fog color")]
    public Color HeightColor;

    [Tooltip("the height fog minimum distance")]
    public float HeightMin;

    [Tooltip("the height fog exponential density factor")]
    public float HeightDensity;

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

        cur.HeightColor = Color.Lerp(
            src.HeightColor,
            dst.HeightColor,
            t
        );

        cur.HeightDensity = Mathf.Lerp(
            src.HeightDensity,
            dst.HeightDensity,
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