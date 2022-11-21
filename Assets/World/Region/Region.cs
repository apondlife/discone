using System;
using UnityEngine;

[Serializable]
public record Region {
    // -- config --
    [Header("config")]
    [Tooltip("the text that displays on the screen for this region")]
    public string DisplayName;

    [Tooltip("the event string sent to fmod to change audio for this region")]
    public string MusicString;

    [Tooltip("the sky color for this region")]
    public Discone.RegionSkyColor SkyColor;

    [Tooltip("the fog for this region")]
    public Discone.RegionFog Fog;

    // -- lifetime --
    public Region() {
        SkyColor = new Discone.RegionSkyColor(Color.black, 0.0f, Color.black, 0.0f);
        Fog = new Discone.RegionFog();
    }

    // -- commands --
    /// lerp between two regions
    public static void Lerp(
        ref Region cur,
        Region src,
        Region dst,
        float t
    ) {
        Discone.RegionSkyColor.Lerp(
            ref cur.SkyColor,
            src.SkyColor,
            dst.SkyColor,
            t
        );

        Discone.RegionFog.Lerp(
            ref cur.Fog,
            src.Fog,
            dst.Fog,
            t
        );
    }

    // -- queries --
    /// create a copy of the region
    public Region Copy() {
        return this with {
            SkyColor = SkyColor.Copy(),
            Fog = Fog.Copy()
        };
    }
}