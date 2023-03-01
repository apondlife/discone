using UnityEngine;

namespace Discone {

/// ids for various shader props
static class ShaderProps {
    // -- constants --
    /// the main texture
    public static readonly int Main = Shader.PropertyToID("_MainTex");

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

    /// the region dissolve pct
    public static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

    /// the region letterbox pct
    public static readonly int LetterboxAmount = Shader.PropertyToID("_LetterboxAmount");

    /// the current character position
    public static readonly int CharacterPos = Shader.PropertyToID("_CharacterPos");

    /// the inverse view matrix
    public static readonly int InvView = Shader.PropertyToID("_InvView");

    /// the inverse view projection matrix
    public static readonly int InvProjection = Shader.PropertyToID("_InvProjection");

    /// the angle that is considered to be a wall
    public static readonly int WallAngle = Shader.PropertyToID("_WallAngle");

    /// a small value shared by all shaders
    public static readonly int Epsilon = Shader.PropertyToID("_Epsilon");

    /// the ambient light intensity relative to the directional light
    public static readonly int AmbientLightIntensity = Shader.PropertyToID("_AmbientLightIntensity");

    /// the reflected light intensity relative to the directional light
    public static readonly int ReflectedLightIntensity = Shader.PropertyToID("_ReflectedLightIntensity");
}

}