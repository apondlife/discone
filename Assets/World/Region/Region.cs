using System;
using UnityEngine;

namespace Discone {

[Serializable]
public record Region {
    // -- config --
    [Header("config")]
    [Tooltip("the text that displays on the screen for this region")]
    public string DisplayName;

    [Tooltip("the event string sent to fmod to change audio for this region")]
    public string MusicString;

    [Tooltip("the sky color for this region")]
    public RegionSkyColor SkyColor;

    [Tooltip("the fog for this region")]
    public RegionFog Fog;

    // -- lifetime --
    public Region() {
        SkyColor = new RegionSkyColor(Color.black, 0.0f, Color.black, 0.0f);
        Fog = new RegionFog();
    }

    // -- commands --
    /// lerp between two regions
    public static void Lerp(
        ref Region cur,
        Region src,
        Region dst,
        float t
    ) {
        RegionSkyColor.Lerp(
            ref cur.SkyColor,
            src.SkyColor,
            dst.SkyColor,
            t
        );

        RegionFog.Lerp(
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

}