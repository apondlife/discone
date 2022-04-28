using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(FuzzProcessingRenderer), PostProcessEvent.AfterStack, "Custom/FuzzProcessing")]
public sealed class FuzzProcessing : PostProcessEffectSettings
{
        // [Range(0f, 1f), Tooltip("FuzzProcessing effect intensity.")]
        public TextureParameter texture = new TextureParameter();
        public FloatParameter TextureScale = new FloatParameter { value = 1.0f };
        public FloatParameter MaxOrthogonality = new FloatParameter { value = 1.0f };
        public FloatParameter DepthScale = new FloatParameter { value = 0.0f };
        public FloatParameter HueShift = new FloatParameter { value = 0.0f };
        public FloatParameter SaturationShift = new FloatParameter { value = 0.0f };
        public FloatParameter ValueShift = new FloatParameter { value = 0.0f };
        public FloatParameter DissolveDepth = new FloatParameter { value = 0.1f };
        public FloatParameter DissolveBand = new FloatParameter { value = 0.1f };
        public FloatParameter FuzzOffset = new FloatParameter { value = 0.1f };
        public FloatParameter ConvolutionOffsetX = new FloatParameter { value = 0.0f };
        public FloatParameter ConvolutionOffsetY = new FloatParameter { value = 0.0f };
        public FloatParameter ConvolutionDelta = new FloatParameter { value = 0.1f };
        public FloatParameter HueScale = new FloatParameter { value = 0.1f };
        public FloatParameter SaturationScale = new FloatParameter { value = 0.1f };
        public FloatParameter ValueScale = new FloatParameter { value = 0.1f };
}

public sealed class FuzzProcessingRenderer : PostProcessEffectRenderer<FuzzProcessing>
{
    public override void Render(PostProcessRenderContext context)
    {
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
        sheet.properties.SetFloat("_ConvolutionOffsetX", settings.ConvolutionOffsetX);
        sheet.properties.SetFloat("_ConvolutionOffsetY", settings.ConvolutionOffsetY);
        sheet.properties.SetFloat("_ConvolutionDelta", settings.ConvolutionDelta);
        sheet.properties.SetFloat("_HueScale", settings.HueScale);
        sheet.properties.SetFloat("_SaturationScale", settings.SaturationScale);
        sheet.properties.SetFloat("_ValueScale", settings.ValueScale);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0, clear: true, preserveDepth: true);
    }
}
