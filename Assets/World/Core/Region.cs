using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Region {
    // -- config --
    [Header("config")]
    [Tooltip("the text that displays on the screen for this region")]
    public string DisplayName;

    [Tooltip("the event string sent to fmod to change audio for this region")]
    public string MusicString;

    [Tooltip("the color of the fog for this region")]
    [FormerlySerializedAs("SkyboxFog")]
    public Color FogColor;

    [Tooltip("the sky color for this region")]
    public SkyColor SkyColor;
}