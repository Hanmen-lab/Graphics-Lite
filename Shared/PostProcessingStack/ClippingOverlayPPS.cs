using KKAPI.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Graphics.XPostProcessing
{
    [Serializable]
    [PostProcess(typeof(ColorClippingRenderer), PostProcessEvent.AfterStack, "Custom/Color Clipping")]
    public sealed class ColorClipping : PostProcessEffectSettings
    {
        [Range(0.001f, 1f), Tooltip("Threshold for shadow clipping detection")]
        public FloatParameter shadowThreshold = new FloatParameter { value = 0.001f };

        [Range(0f, 1f), Tooltip("Threshold for highlight clipping detection")]
        public FloatParameter highlightThreshold = new FloatParameter { value = 0.95f };


        [Tooltip("Show shadow clipping")]
        public BoolParameter showShadows = new BoolParameter { value = true };

        [Tooltip("Show highlight clipping")]
        public BoolParameter showHighlights = new BoolParameter { value = true };

    }

    public sealed class ColorClippingRenderer : PostProcessEffectRenderer<ColorClipping>
    {
        private Shader shader;
        AssetBundle assetBundle;
        private Color shadowColor = Color.blue;
        private Color highlightColor = Color.red;

        public override void Init()
        {
            if (shader != null) return;

            assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("clipping.unity3d"));
            if (assetBundle == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load asset bunle 'clipping.unity3d'");

            shader = assetBundle.LoadAsset<Shader>("Assets/ClippingOverlay/Shaders/ClippingOverlayPPS.shader");
            if (shader == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load shader 'ClippingOverlayPPS.shader'");

            assetBundle.Unload(false);
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_ShadowThreshold", settings.shadowThreshold);
            sheet.properties.SetFloat("_HighlightThreshold", settings.highlightThreshold);
            sheet.properties.SetColor("_ShadowColor", shadowColor);
            sheet.properties.SetColor("_HighlightColor", highlightColor);
            sheet.properties.SetFloat("_ShowShadows", settings.showShadows ? 1f : 0f);
            sheet.properties.SetFloat("_ShowHighlights", settings.showHighlights ? 1f : 0f);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
