using UnityEngine;

namespace Discone {

/// ids for various shader props
static class ShaderProps {
    // -- constants --
    /// the main texture
    public static int Main = Shader.PropertyToID("_MainTex");

    /// the emission texture
    public static int Emission = Shader.PropertyToID("_EmissionMap");

    /// the foreground color
    public static int Foreground = Shader.PropertyToID("_Foreground");

    /// the foreground color exposure
    public static int ForegroundExposure = Shader.PropertyToID("_ExposureForeground");

    /// the background color
    public static int Background = Shader.PropertyToID("_Background");

    /// the background color exposure
    public static int BackgroundExposure = Shader.PropertyToID("_ExposureBackground");

    /// the region dissolve pct
    public static int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

    /// the region letterbox pct
    public static int LetterboxAmount = Shader.PropertyToID("_LetterboxAmount");
}

}