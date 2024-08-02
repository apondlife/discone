using ThirdPerson;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
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
    Texture2D m_ColorTexture;

    /// a buffer for gpu texture data
    NativeArray<float> m_Buffer;

    /// the current request to read gpu texture data
    AsyncGPUReadbackRequest m_ReadReq;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>(true);

        // instance a new texture
        var renderTexture = new RenderTexture(m_RenderTexture);

        // allocate a buffer for reading off the gpu
        var bufferSize = (
            renderTexture.width *
            renderTexture.height *
            (int)GraphicsFormatUtility.GetBlockSize(renderTexture.graphicsFormat) /
            sizeof(float)
        );

        var buffer = new NativeArray<float>(bufferSize, Allocator.Persistent);

        // create a texture that we can sample colors from
        var colorTexture = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            renderTexture.graphicsFormat,
            TextureCreationFlags.None
        );

        colorTexture.filterMode = FilterMode.Point;

        // set texture props
        m_Buffer = buffer;
        m_ColorTexture = colorTexture;
        m_Camera.targetTexture = renderTexture;
        c.Effects.ColorTexture = colorTexture;

        // set replacement shader
        var shader = Shader.Find("Discone/SurfaceColor");
        m_Camera.SetReplacementShader(shader, "Surface");
    }

    void Update() {
        ReadTexture();
    }

    void FixedUpdate() {
        var next = c.State.Next;
        var surface = next.MainSurface;

        // look towards current surface, otherwise predict surface
        var direction = surface.IsSome
            ? -surface.Normal
            : next.Velocity;

        m_Camera.transform.forward = direction;
    }

    // -- commands --
    /// reads the render texture into the color texture asynchronously
    void ReadTexture() {
        var req = m_ReadReq;
        if (!req.done) {
            return;
        }

        if (!req.hasError) {
            m_ColorTexture.SetPixelData(m_Buffer, 0);
        }

        m_ReadReq = AsyncGPUReadback.RequestIntoNativeArray(ref m_Buffer, m_Camera.targetTexture);
    }

    /// reads the render texture into the color texture synchronously
    void ReadTextureSync() {
        var activeTexture = RenderTexture.active;
        var renderTexture = m_Camera.targetTexture;

        RenderTexture.active = renderTexture;
        m_ColorTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        m_ColorTexture.Apply();
        RenderTexture.active = activeTexture;
    }
}

}