using System;
using UnityEngine;

/// the skybox color for a region
[Serializable]
public sealed class SkyColor {
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
    public SkyColor(
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
}