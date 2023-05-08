using UnityEngine;
using UnityEngine.Rendering;

namespace Discone {

/// a command buffer based rendering test
sealed class Render_Test: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the fuzz filter")]
    [SerializeField] Material m_Fuzz;

    [Tooltip("the final game image")]
    [SerializeField] RenderTexture m_DstImage;

    // -- lifecycle --
    void Start() {
        var buf = new CommandBuffer();
        buf.name = "Render_Test";

        int src = ShaderProps.Src;
        buf.GetTemporaryRT(src, -1, -1, 0, FilterMode.Point);
        buf.Blit(BuiltinRenderTextureType.CurrentActive, src);

        int tmp = ShaderProps.Tmp1;
        buf.GetTemporaryRT(tmp, -1, -1, 16, FilterMode.Point);
        buf.Blit(src, tmp, m_Fuzz);

        // buf.Blit(src, m_DstImage);
        buf.Blit(tmp, m_DstImage);

        buf.ReleaseTemporaryRT(src);
        buf.ReleaseTemporaryRT(tmp);

        var cam = Camera.main;
        cam.AddCommandBuffer(CameraEvent.BeforeSkybox, buf);
    }
}

}