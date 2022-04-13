using UnityEngine;
using UnityEngine.Rendering;

/// the fuzz post-processing effect
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Fuzz: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the post-processing material (shader)")]
    [SerializeField] Material m_Material;

    // -- lifecycle --
    void Awake() {
        // make sure we have the depth & normals texture
        var cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.DepthNormals;

        // draw fuzz geometry on top of skybox
        cam.RemoveAllCommandBuffers();

        // get texture refs
        var fuzz = Shader.PropertyToID("_FuzzTex");
        var main = Shader.PropertyToID("_MainTex");
        var temp = Shader.PropertyToID("_TempTex");

        // copy opaque geometry to temp texture
        var copy = new CommandBuffer();
        copy.name = "CopyOpaque";
        copy.GetTemporaryRT(fuzz, -1, -1);
        copy.SetGlobalTexture(
            main,
            BuiltinRenderTextureType.CurrentActive
        );

        copy.Blit(
            main,
            fuzz,
            m_Material
        );

        cam.AddCommandBuffer(CameraEvent.BeforeSkybox, copy);

        // copy depth buffer
        // var copyDepth = new CommandBuffer();
        // copyDepth.name = "CopyDepth";
        // copyDepth.GetTemporaryRT(temp, -1, -1);
        // copyDepth.SetGlobalTexture(
        //     main,
        //     BuiltinRenderTextureType.Depth
        // );

        // copyDepth.Blit(
        //     main,
        //     temp
        // );

        // cam.AddCommandBuffer(CameraEvent.BeforeSkybox, copyDepth);

        // clear main tex & dpeth buffer so skybox draws over the whole screen
        // TODO: this fucks up transparent objects
        var clear = new CommandBuffer();
        clear.name = "Clear";
        clear.SetRenderTarget(BuiltinRenderTextureType.CurrentActive);
        clear.ClearRenderTarget(RTClearFlags.All, Color.clear, 1.0f, 0);

        cam.AddCommandBuffer(CameraEvent.BeforeSkybox, clear);

        // overlay geometry onto skybox
        var overlay = new CommandBuffer();
        overlay.name = "DrawOpaque";
        overlay.GetTemporaryRT(fuzz, -1, -1);
        overlay.SetGlobalTexture(
            main,
            fuzz
        );

        overlay.Blit(
            fuzz,
            BuiltinRenderTextureType.CameraTarget,
            new Material(Shader.Find("Image/Overlay"))
        );

        // and clear the fuzz texture
        overlay.SetRenderTarget(fuzz);
        overlay.ClearRenderTarget(RTClearFlags.Color, Color.clear, 1.0f, 0);
        overlay.ReleaseTemporaryRT(fuzz);

        cam.AddCommandBuffer(CameraEvent.AfterSkybox, overlay);
    }
}
