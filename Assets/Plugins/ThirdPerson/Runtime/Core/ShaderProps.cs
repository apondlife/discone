using UnityEngine;

namespace ThirdPerson {

/// ids for various shader props
static class ShaderProps {
    // -- constants --
    /// the main texture
    public static readonly int Main = Shader.PropertyToID("_MainTex");

    /// the plane the character distorts around
    public static readonly int Distortion_Plane = Shader.PropertyToID("_Distortion_Plane");

    /// the amount the character distorts towards the plane normal
    public static readonly int Distortion_PositiveScale = Shader.PropertyToID("_Distortion_PositiveScale");

    /// the amount the character distorts against the plane normal
    public static readonly int Distortion_NegativeScale = Shader.PropertyToID("_Distortion_NegativeScale");

    /// the amount the character distorts
    public static readonly int Distortion_Intensity = Shader.PropertyToID("_Distortion_Intensity");
}

}