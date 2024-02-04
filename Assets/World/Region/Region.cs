using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Discone {

[Serializable]
public record Region {
    // -- config --
    [Header("config")]
    [Tooltip("the title that displays on the region sign")]
    public string DisplayName;

    [Tooltip("the event string sent to fmod to change ambient music")]
    public string MusicString;

    [FormerlySerializedAs("SkyColor")]
    [Tooltip("the sky properties")]
    public RegionSky Sky;

    [Tooltip("the fog properties")]
    public RegionFog Fog;

    // -- lifetime --
    public Region() {
        Sky = new RegionSky(
            Color.black,
            0.0f,
            Color.black,
            0.0f
        );

        Fog = new RegionFog();
    }

    // -- commands --
    /// update to the region sky/fog
    public void Set(Region dst) {
        var storage = this;
        Lerp(ref storage, dst, dst, 1f);
    }

    /// lerp the sky/fog between two regions
    public static void Lerp(
        ref Region cur,
        Region src,
        Region dst,
        float t
    ) {
        RegionSky.Lerp(
            ref cur.Sky,
            src.Sky,
            dst.Sky,
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
            Sky = Sky.Copy(),
            Fog = Fog.Copy()
        };
    }
}

}