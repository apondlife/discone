using UnityEngine;
using UnityEngine.Rendering;

/// blit a post processing effect
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Blit: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the post-processing material (shader)")]
    [SerializeField] Material m_Material;

    [SerializeField] float dissolveAmmount;

    // -- lifecycle --
    void Awake() {
        // make sure we have the depth & normals texture
        var camera = GetComponent<Camera>();

        // // create new command buffer
        // var buffer = new CommandBuffer();
        // buffer.name = "frans buffer";
        // // m_Cameras[cam] = m_GlowBuffer;

        // int tempID = Shader.PropertyToID("_fransTempRT");
        // buffer.GetTemporaryRT(tempID, -1, -1);
        // buffer.SetRenderTarget(tempID);
        // buffer.ClearRenderTarget(true, true, Color.red); // clear before drawing to it each frame!!


        // camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, buffer);
    }

    void Update() {
        m_Material.SetFloat("_DissolveAmmount", dissolveAmmount);

    }

    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        // render the effect
        Graphics.Blit(src, dst, m_Material);
    }
}
