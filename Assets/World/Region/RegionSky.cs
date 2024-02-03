using System;
using UnityEngine;

namespace Discone {

/// the skybox color for a region
[Serializable]
public record RegionSky {
    // -- props --
    [Tooltip("the foreground color")]
    public Color Foreground;

    [Tooltip("the foreground color exposure")]
    [Range(0.0f, 8.0f)]
    public float ForegroundExposure;

    [Tooltip("the background color")]
    public Color Background;

    [Tooltip("the background color exposure")]
    [Range(0.0f, 8.0f)]
    public float BackgroundExposure;

    // -- lifetime --
    /// create a new region sky color
    public RegionSky(
        Color foreground,
        float foregroundExposure,
        Color background,
        float backgroundExposure
    ) {
        Foreground = foreground;
        ForegroundExposure = foregroundExposure;
        Background = background;
        BackgroundExposure = backgroundExposure;
    }

    // -- commands --
    /// lerp between two sky colors
    public static void Lerp(
        ref RegionSky cur,
        RegionSky src,
        RegionSky dst,
        float t
    ) {
        cur.Foreground = Color.Lerp(
            src.Foreground,
            dst.Foreground,
            t
        );

        cur.ForegroundExposure = Mathf.Lerp(
            src.ForegroundExposure,
            dst.ForegroundExposure,
            t
        );

        cur.Background = Color.Lerp(
            src.Background,
            dst.Background,
            t
        );

        cur.BackgroundExposure = Mathf.Lerp(
            src.BackgroundExposure,
            dst.BackgroundExposure,
            t
        );
    }

    // -- queries --
    /// create a copy of this sky color
    public RegionSky Copy() {
        return this with {};
    }
}

}