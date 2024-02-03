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
    public bool UseHeightColor;

    [ShowIf("UseHeightColor")]
    [Tooltip("the height fog color")]
    [SerializeField] Color m_HeightColor;

    [Tooltip("the height fog exponential density factor")]
    public float HeightDensity;

    // -- lifetime --
    public RegionFog(bool useHeightColor) {
        UseHeightColor = useHeightColor;
    }

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

        cur.m_HeightColor = Color.Lerp(
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
    /// the active height color
    public Color HeightColor {
        get => UseHeightColor ? m_HeightColor : Color;
    }

    /// create a copy of the region fog
    public RegionFog Copy() {
        return this with {};
    }
}

}