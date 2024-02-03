using Soil;
using UnityEngine;

namespace Discone {

/// ids for various shader props
static class ShaderProps {
    // -- buffers --
    /// the main texture
    public static readonly int Main = Shader.PropertyToID("_MainTex");

    /// the source texture for a command sequence
    public static readonly int Src = Shader.PropertyToID("_SrcTex");

    /// a temp texture for a command sequence
    public static readonly int Tmp1 = Shader.PropertyToID("_TmpTex1");

    /// a temp texture for a command sequence
    public static readonly int Tmp2 = Shader.PropertyToID("_TmpTex2");

    // -- discone --
    /// the emission texture
    public static readonly int Emission = Shader.PropertyToID("_EmissionMap");

    /// the foreground color
    public static readonly int Foreground = Shader.PropertyToID("_Foreground");

    /// the foreground color exposure
    public static readonly int ForegroundExposure = Shader.PropertyToID("_ExposureForeground");

    /// the background color
    public static readonly int Background = Shader.PropertyToID("_Background");

    /// the background color exposure
    public static readonly int BackgroundExposure = Shader.PropertyToID("_ExposureBackground");

    /// the height fog color
    public static readonly ShaderProp HeightFog_Color = ShaderProp.Named("_HeightFog_Color");

    /// the height fog density
    public static readonly ShaderProp HeightFog_Density = ShaderProp.Named("_HeightFog_Density");

    /// the height fog minimum height
    public static readonly ShaderProp HeightFog_MinHeight = ShaderProp.Named("_HeightFog_MinDistance");

    /// the region dissolve pct
    public static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

    /// the region letterbox pct
    public static readonly int LetterboxAmount = Shader.PropertyToID("_LetterboxAmount");

    /// the current character position
    public static readonly int CharacterPos = Shader.PropertyToID("_CharacterPos");

    /// the angle that is considered to be a wall
    public static readonly int WallAngle = Shader.PropertyToID("_WallAngle");

    /// a small value shared by all shaders
    public static readonly int Epsilon = Shader.PropertyToID("_Epsilon");

    /// the ambient light intensity relative to the directional light
    public static readonly int AmbientLightIntensity = Shader.PropertyToID("_AmbientLightIntensity");

    /// the reflected light intensity relative to the directional light
    public static readonly int ReflectedLightIntensity = Shader.PropertyToID("_ReflectedLightIntensity");

    /// the eyelid blur offsets
    public static readonly int Offsets = Shader.PropertyToID("_Offsets");

    /// the spritesheet current sprite
    public static readonly int CurrentSprite = Shader.PropertyToID("_CurrentSprite");
}

}