using Soil;
using UnityEngine;

namespace ThirdPerson {

/// ids for various shader props
static class ShaderProps {
    // -- constants --
    /// the main texture
    public static readonly ShaderProp MainTex = new(nameof(MainTex));

    /// the world position of the character
    public static readonly ShaderProp Character_Pos = new(nameof(Character_Pos));

    /// the intensity of the character distortion
    public static readonly ShaderProp Distortion_Intensity = new(nameof(Distortion_Intensity));

    /// the plane the character distorts above
    public static readonly ShaderProp Distortion_BotPlane = new(nameof(Distortion_BotPlane));

    /// the plane the character distorts below
    public static readonly ShaderProp Distortion_TopPlane = new(nameof(Distortion_TopPlane));

    /// a scale on intensity along the plane's axis
    public static readonly ShaderProp Distortion_AxialScale = new(nameof(Distortion_AxialScale));

    /// a scale on intensity around the plane's axis (inversely proportional to axial)
    public static readonly ShaderProp Distortion_RadialScale = new(nameof(Distortion_RadialScale));

    // TODO: move these (and a lot of camera stuff) to discone
    /// the camera's current clip pos
    public static readonly ShaderProp CameraClipPos = new(nameof(CameraClipPos));

    /// the camera's current clip plane
    public static readonly ShaderProp CameraClipPlane = new(nameof(CameraClipPlane));

    /// the character's ground plane
    public static readonly ShaderProp CharacterSurfacePlane = new(nameof(CharacterSurfacePlane));
}

}