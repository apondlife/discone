using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace Discone.Ui {

/// the player's eyelid image filter
public class PlayerEyelid_Filter: UIBehaviour {
    // -- config --
    [Tooltip("a blur filter for the eyelid image")]
    [SerializeField] Material m_Blur;

    [Tooltip("a color filter for the eyelid image")]
    [SerializeField] Material m_Color;

    // -- refs --
    [Header("refs")]
    [Tooltip("the source image")]
    [SerializeField] Texture m_SrcImage;

    [Tooltip("the image as sensed through the eyelids")]
    [SerializeField] RenderTexture m_DstImage;

    // -- props --
    /// the eyelid command buffer
    CommandBuffer m_Buffer;

    // -- lifecycle --
    protected override void Start() {
        base.Start();

        // get src dimensions
        // TODO: recalculate if screen size changes
        var w1 = m_SrcImage.width;
        var h1 = m_SrcImage.height;
        var w2 = w1 / 2;
        var h2 = h1 / 2;

        var f2w = 2f / w1;
        var f2h = 2f / h1;
        var f4w = 4f / w1;
        var f4h = 4f / h1;

        // init buffer
        var buf = new CommandBuffer();
        buf.name = typeof(PlayerEyelid_Filter).Name;

        // copy screen into temporary render texture
        int src = ShaderProps.Src;
        buf.GetTemporaryRT(src, w1, w2, 0, FilterMode.Bilinear);
        buf.Blit(m_SrcImage, src);

        // get two smaller rts
        int blur1 = ShaderProps.Tmp1;
        int blur2 = ShaderProps.Tmp2;
        buf.GetTemporaryRT(blur1, w2, h2, 0, FilterMode.Bilinear);
        buf.GetTemporaryRT(blur2, w2, h2, 0, FilterMode.Bilinear);

        // downscale screen copy into smaller rt, release screen rt
        buf.Blit(src, blur1);
        buf.ReleaseTemporaryRT(src);

        // add blur
        int blurOffsets = ShaderProps.Offsets;

        // add horizontal blur
        buf.SetGlobalVector(blurOffsets, new Vector4(f2w, 0f, 0f, 0f));
        buf.Blit(blur1, blur2, m_Blur);

        // add vertical blur
        buf.SetGlobalVector(blurOffsets, new Vector4(0f, f2h, 0f, 0f));
        buf.Blit(blur2, blur1, m_Blur);

        // add horizontal blur
        buf.SetGlobalVector(blurOffsets, new Vector4(f4w, 0f, 0f, 0f));
        buf.Blit(blur1, blur2, m_Blur);

        // add vertical blur
        buf.SetGlobalVector(blurOffsets, new Vector4(0f, f4h, 0f, 0f));
        buf.Blit(blur2, blur1, m_Blur);

        // apply the color filter
        buf.Blit(blur1, blur2, m_Color);

        // upscale the image
        buf.Blit(blur2, m_DstImage);

        // store the buffer
        m_Buffer = buf;
    }

    void Update() {
        Graphics.ExecuteCommandBuffer(m_Buffer);
    }
}

}