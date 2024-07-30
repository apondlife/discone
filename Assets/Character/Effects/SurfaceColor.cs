using ThirdPerson;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Camera = UnityEngine.Camera;

namespace Discone {

/// samples the surface color near the character
sealed class SurfaceColor: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the camera looking at the surface")]
    [SerializeField] Camera m_Camera;

    [Tooltip("the render texture to instance")]
    [SerializeField] RenderTexture m_RenderTexture;

    // -- props --
    /// the containing character
    CharacterContainer c;

    /// the texture 2d mapping the render texture
    Texture2D m_SampleTexture;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>(true);

        // instance a new texture
        var renderTexture = new RenderTexture(m_RenderTexture);
        var sampleTexture = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            renderTexture.graphicsFormat,
            TextureCreationFlags.None
        );
        sampleTexture.filterMode = FilterMode.Point;

        m_SampleTexture = sampleTexture;
        m_Camera.targetTexture = renderTexture;
        c.Effects.ColorTexture = sampleTexture;

        // set replacement shader
        var shader = Shader.Find("Discone/SurfaceColor");
        m_Camera.SetReplacementShader(shader, "Surface");
    }

    void FixedUpdate() {
        var _renderTexture = m_Camera.targetTexture;
        var prev = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        m_SampleTexture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        m_SampleTexture.Apply();
        RenderTexture.active = prev;

        var next = c.State.Next;
        var surface = next.MainSurface;

        // look towards current surface, otherwise predict surface
        var direction = surface.IsSome
            ? -surface.Normal
            : next.Velocity;

        m_Camera.transform.forward = direction;
    }
}

}