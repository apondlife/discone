using System;
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
    Texture2D m_SampleTexture;

    /// the texture data as 2-byte chunks
    NativeArray<float> m_TextureBuffer;

    /// the previous request
    AsyncGPUReadbackRequest m_ReadRequest;

    bool m_IsRead = false;

    // -- lifecycle --
    void Awake() {
        // set deps
        c = GetComponentInParent<CharacterContainer>(true);

        // instance a new texture
        var renderTexture = new RenderTexture(m_RenderTexture);
        var w = renderTexture.width;
        var h = renderTexture.height;
        var graphicsFormat = renderTexture.graphicsFormat;

        var gsize = (int)GraphicsFormatUtility.GetBlockSize(graphicsFormat);
        var fsize = sizeof(float);

        Log.Temp.I($"size -> {w} * {h} * ({fsize} / {gsize}) = {w * h * (fsize / gsize)}");

        var textureBuffer = new NativeArray<float>(w * h * (fsize / gsize), Allocator.Persistent);
        var sampleTexture = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            renderTexture.graphicsFormat,
            TextureCreationFlags.None
        );

        sampleTexture.filterMode = FilterMode.Point;

        m_TextureBuffer = textureBuffer;
        m_SampleTexture = sampleTexture;
        m_Camera.targetTexture = renderTexture;
        c.Effects.ColorTexture = sampleTexture;

        // set replacement shader
        var shader = Shader.Find("Discone/SurfaceColor");
        m_Camera.SetReplacementShader(shader, "Surface");
    }

    void Update() {
        var req = m_ReadRequest;
        if (req.done) {
            if (!req.hasError) {
                Log.Temp.I($"done in update: {m_TextureBuffer[30]} {m_TextureBuffer.IsCreated}");
            } else {
                Log.Temp.I($"errr in update");
            }

            req = AsyncGPUReadback.RequestIntoNativeArray(ref m_TextureBuffer, m_RenderTexture);
        }

        m_ReadRequest = req;
    }

    void FixedUpdate() {
        // var req = m_ReadRequest;
        // if (req.done) {
        //     Log.Temp.I($"done in fixed update; err: {req.hasError}");
        //     req = AsyncGPUReadback.RequestIntoNativeArray(ref m_TextureBuffer, m_RenderTexture);
        //     m_IsRead = false;
        // }
        //
        // m_ReadRequest = req;

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

    void OnDestroy() {
        m_TextureBuffer.Dispose();
    }
}

}