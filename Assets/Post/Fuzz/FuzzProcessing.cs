using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(FuzzProcessingRenderer), PostProcessEvent.AfterStack, "Custom/FuzzProcessing")]
public sealed class FuzzProcessing : PostProcessEffectSettings {
    public TextureParameter texture = new TextureParameter();
    public FloatParameter TextureScale = new FloatParameter { value = 1.0f };
    public FloatParameter MaxOrthogonality = new FloatParameter { value = 1.0f };
    public FloatParameter DepthScale = new FloatParameter { value = 0.0f };
    public FloatParameter HueShift = new FloatParameter { value = 0.0f };
    public FloatParameter SaturationShift = new FloatParameter { value = 0.0f };
    public FloatParameter ValueShift = new FloatParameter { value = 0.0f };

    [Range(0, 1)]
    public FloatParameter DissolveDepth = new FloatParameter { value = 0.1f };

    [Range(0, 1)]
    public FloatParameter DissolveBand = new FloatParameter { value = 0.1f };
    public FloatParameter FuzzOffset = new FloatParameter { value = 0.1f };
    public FloatParameter NoiseTimeScale= new FloatParameter { value = 0.1f };
    public FloatParameter NoiseScale= new FloatParameter { value = 1000f };

    public FloatParameter ConvolutionDelta = new FloatParameter { value = 0.1f };
    public FloatParameter HueScale = new FloatParameter { value = 0.1f };
    public FloatParameter SaturationScale = new FloatParameter { value = 0.1f };
    public FloatParameter ValueScale = new FloatParameter { value = 0.1f };
}

public sealed class FuzzProcessingRenderer : PostProcessEffectRenderer<FuzzProcessing> {
    public override void Render(PostProcessRenderContext context) {
        var sheet = context.propertySheets.Get(Shader.Find("Image/Fuzz"));
        sheet.properties.SetTexture("_Texture", settings.texture);
        sheet.properties.SetFloat("_TextureScale", settings.TextureScale);
        sheet.properties.SetFloat("_MaxOrthogonality", settings.MaxOrthogonality);
        sheet.properties.SetFloat("_DepthScale", settings.DepthScale);
        sheet.properties.SetFloat("_HueShift", settings.HueShift);
        sheet.properties.SetFloat("_SaturationShift", settings.SaturationShift);
        sheet.properties.SetFloat("_ValueShift", settings.ValueShift);
        sheet.properties.SetFloat("_DissolveDepth", settings.DissolveDepth);
        sheet.properties.SetFloat("_DissolveBand", settings.DissolveBand);
        sheet.properties.SetFloat("_FuzzOffset", settings.FuzzOffset);
        sheet.properties.SetFloat("_ConvolutionDelta", settings.ConvolutionDelta);
        sheet.properties.SetFloat("_HueScale", settings.HueScale);
        sheet.properties.SetFloat("_SaturationScale", settings.SaturationScale);
        sheet.properties.SetFloat("_ValueScale", settings.ValueScale);
        sheet.properties.SetFloat("_NoiseTimeScale", settings.NoiseTimeScale);
        sheet.properties.SetFloat("_NoiseScale", settings.NoiseScale);


        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0, clear: true, preserveDepth: true);
    }
}
