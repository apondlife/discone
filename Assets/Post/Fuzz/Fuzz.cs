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
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
       Graphics.Blit(src, dest, m_Material);
    }
}
