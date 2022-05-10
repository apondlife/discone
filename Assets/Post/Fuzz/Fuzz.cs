using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

/// the fuzz effect settings
[Serializable]
[PostProcess(typeof(FuzzRenderer), PostProcessEvent.AfterStack, "Custom/Fuzz")]
public sealed class Fuzz: PostProcessEffectSettings {
    public TextureParameter texture = new TextureParameter();
    public FloatParameter TextureScale = new FloatParameter { value = 1.0f };
    public FloatParameter MaxOrthogonality = new FloatParameter { value = 1.0f };
    public FloatParameter DepthScale = new FloatParameter { value = 0.0f };
    public FloatParameter HueShift = new FloatParameter { value = 0.0f };
    public FloatParameter SaturationShift = new FloatParameter { value = 0.0f };
    public FloatParameter ValueShift = new FloatParameter { value = 0.0f };

    [Header("Depth Dissolve")]
    [Range(0, 1)]
    public FloatParameter DepthPower = new FloatParameter { value = 0.1f };
    [Range(0, 1)]
    public FloatParameter DissolveDepthMin = new FloatParameter { value = 0.7f };
    [Range(0, 1)]
    public FloatParameter DissolveDepthMax = new FloatParameter { value = 0.99f };

    public FloatParameter NoiseTimeScale= new FloatParameter { value = 0.1f };
    public FloatParameter NoiseScale= new FloatParameter { value = 1000f };

    [Header("Fuzz")]
    public FloatParameter FuzzOffset = new FloatParameter { value = 0.1f };
    public FloatParameter ConvolutionDelta = new FloatParameter { value = 0.1f };
    public FloatParameter HueScale = new FloatParameter { value = 0.1f };
    public FloatParameter SaturationScale = new FloatParameter { value = 0.1f };
    public FloatParameter ValueScale = new FloatParameter { value = 0.1f };
}

/// the fuzz effect renderer
public sealed class FuzzRenderer: PostProcessEffectRenderer<Fuzz> {
    // -- constants --
    const string k_ShaderName = "Image/Fuzz";

    // -- props --
    /// a reference to the fuzz shader
    Shader m_Shader;

    // -- lifecycle --
    public override void Init() {
        base.Init();

        // set props
        m_Shader = Shader.Find(k_ShaderName);
    }

    public override void Render(PostProcessRenderContext ctx) {
        // grab shader props and settings
        var sheet = ctx.propertySheets.Get(m_Shader);
        var p = sheet.properties;
        var s = settings;

        // pass settings to shader
        p.SetTexture("_Texture", s.texture);
        p.SetFloat("_TextureScale", s.TextureScale);
        p.SetFloat("_MaxOrthogonality", s.MaxOrthogonality);
        p.SetFloat("_DepthScale", s.DepthScale);
        p.SetFloat("_HueShift", s.HueShift);
        p.SetFloat("_SaturationShift", s.SaturationShift);
        p.SetFloat("_ValueShift", s.ValueShift);
        p.SetFloat("_DissolveDepthMin", s.DissolveDepthMin);
        p.SetFloat("_DissolveDepthMax", s.DissolveDepthMax);
        p.SetFloat("_FuzzOffset", s.FuzzOffset);
        p.SetFloat("_ConvolutionDelta", s.ConvolutionDelta);
        p.SetFloat("_HueScale", s.HueScale);
        p.SetFloat("_SaturationScale", s.SaturationScale);
        p.SetFloat("_ValueScale", s.ValueScale);
        p.SetFloat("_NoiseTimeScale", s.NoiseTimeScale);
        p.SetFloat("_NoiseScale", s.NoiseScale);
        p.SetFloat("_DepthPower", s.DepthPower);

        // add effect
        ctx.command.BlitFullscreenTriangle(
            ctx.source,
            ctx.destination,
            sheet, 0,
            clear: true,
            preserveDepth: true
        );
    }
}
