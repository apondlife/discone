using UnityEngine;

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
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        // render the effect
        Graphics.Blit(src, dst, m_Material);
    }
}
