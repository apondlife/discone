using UnityEngine;

namespace Discone {

/// ids for various shader props
static class ShaderProps {
    // -- constants --
    /// the main texture
    public static int Main = Shader.PropertyToID("_MainTex");

    /// the emission texture
    public static int Emission = Shader.PropertyToID("_EmissionMap");
}

}