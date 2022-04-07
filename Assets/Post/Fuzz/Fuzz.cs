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
        var camera = GetComponent<Camera>();
        camera.depthTextureMode = DepthTextureMode.DepthNormals;

        var overlaySha = Shader.Find("Image/Overlay");
        var overlayMat = new Material(overlaySha);

        camera.RemoveAllCommandBuffers();
        var beforeSkyboxBuffer = new CommandBuffer();
        int rt = Shader.PropertyToID("_Temp");

        beforeSkyboxBuffer.GetTemporaryRT(rt, -1, -1);
        beforeSkyboxBuffer.SetGlobalTexture(
            Shader.PropertyToID("_MainTex"),
            BuiltinRenderTextureType.CurrentActive);


        beforeSkyboxBuffer.Blit(
            rt,
            rt,
            m_Material);


        camera.AddCommandBuffer(CameraEvent.BeforeSkybox, beforeSkyboxBuffer);

        var beforeSkyboxBuffer2 = new CommandBuffer();
        beforeSkyboxBuffer2.SetRenderTarget(BuiltinRenderTextureType.CurrentActive);
        // TODO: this fucks up transparent objects
        beforeSkyboxBuffer2.ClearRenderTarget(RTClearFlags.All, new Color(0, 0.0f, 0, 0), 1.0f, 0);

        camera.AddCommandBuffer(CameraEvent.BeforeSkybox, beforeSkyboxBuffer2);

        var afterSkyboxBuffer = new CommandBuffer();
        afterSkyboxBuffer.GetTemporaryRT(rt, -1, -1);
        beforeSkyboxBuffer.SetGlobalTexture(
            Shader.PropertyToID("_MainTex"),
            rt);

        afterSkyboxBuffer.Blit(
            rt,
            BuiltinRenderTextureType.CameraTarget,
            overlayMat
            );

        afterSkyboxBuffer.SetRenderTarget(rt);
        afterSkyboxBuffer.ClearRenderTarget(RTClearFlags.Color, new Color(0, 0.0f, 0, 0), 1.0f, 0);
        afterSkyboxBuffer.ReleaseTemporaryRT(rt);

        camera.AddCommandBuffer(CameraEvent.AfterSkybox, afterSkyboxBuffer);
    }

    // void OnRenderImage(RenderTexture src, RenderTexture dst) {
        // render the effect
        // Graphics.Blit(src, dst, m_Material);
    // }
}
