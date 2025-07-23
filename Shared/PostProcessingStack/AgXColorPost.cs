using KKAPI.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using static RootMotion.FinalIK.HitReactionVRIK;


namespace Graphics
{
    [Serializable]
    [PostProcess(typeof(AgXColorPostRenderer), PostProcessEvent.AfterStack, "Custom/ColorPost")]
    public sealed class AgXColorPost : PostProcessEffectSettings
    {
        [DisplayName("Saturation"), Range(-100f, 100f), Tooltip("Pushes the intensity of all colors.")]
        public FloatParameter saturation = new FloatParameter { value = 1f };
        [DisplayName("Brightness"), Range(-100f, 100f), Tooltip("Makes the image brighter or darker.")]
        public FloatParameter brightness = new FloatParameter { value = 0f };
        [DisplayName("Temperature"), Range(-100f, 100f), Tooltip("Sets the white balance to a custom color temperature.")]
        public FloatParameter temperature = new FloatParameter { value = 0f };
        [DisplayName("Tint"), Range(-100f, 100f), Tooltip("Sets the white balance to compensate for a green or magenta tint.")]
        public FloatParameter tint = new FloatParameter { value = 0f };
        [DisplayName("Color Filter"), ColorUsage(false, true), Tooltip("Tint the render by multiplying a color.")]
        public ColorParameter colorFilter = new ColorParameter { value = Color.white };
        [DisplayName("Hue Shift"), Range(-180f, 180f), Tooltip("Shift the hue of all colors.")]
        public FloatParameter hueShift = new FloatParameter { value = 0f };
        [DisplayName("Contrast"), Range(-100f, 100f), Tooltip("Expands or shrinks the overall range of tonal values.")]
        public FloatParameter contrast = new FloatParameter { value = 0f };

        [DisplayName("Lift"), Tooltip("Controls the darkest portions of the render."), Trackball(TrackballAttribute.Mode.Lift)]
        public Vector4Parameter lift = new Vector4Parameter { value = new Vector4(1f, 1f, 1f, 0f) };

        [DisplayName("Gamma"), Tooltip("Power function that controls midrange tones."), Trackball(TrackballAttribute.Mode.Gamma)]
        public Vector4Parameter gamma = new Vector4Parameter { value = new Vector4(1f, 1f, 1f, 0f) };

        [DisplayName("Gain"), Tooltip("Controls the lightest portions of the render."), Trackball(TrackballAttribute.Mode.Gain)]
        public Vector4Parameter gain = new Vector4Parameter { value = new Vector4(1f, 1f, 1f, 0f) };

        public SplineParameter hueVsHueCurve = new SplineParameter { value = new Spline(new AnimationCurve(), 0.5f, true, new Vector2(0f, 1f)) };
        public SplineParameter hueVsSatCurve = new SplineParameter { value = new Spline(new AnimationCurve(), 0.5f, true, new Vector2(0f, 1f)) };
        public SplineParameter satVsSatCurve = new SplineParameter { value = new Spline(new AnimationCurve(), 0.5f, false, new Vector2(0f, 1f)) };
        public SplineParameter lumVsSatCurve = new SplineParameter { value = new Spline(new AnimationCurve(), 0.5f, false, new Vector2(0f, 1f)) };
        public SplineParameter masterCurve = new SplineParameter { value = new Spline(new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)), 0f, false, new Vector2(0f, 1f)) };
        public SplineParameter redCurve = new SplineParameter { value = new Spline(new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)), 0f, false, new Vector2(0f, 1f)) };
        public SplineParameter greenCurve = new SplineParameter { value = new Spline(new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)), 0f, false, new Vector2(0f, 1f)) };
        public SplineParameter blueCurve = new SplineParameter { value = new Spline(new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f)), 0f, false, new Vector2(0f, 1f)) };
        //[DisplayName("LUT"), Tooltip("LDR Lookup Texture, 1024x32")]
        //public TextureParameter lut = new TextureParameter { value = null, defaultState = TextureParameterDefault.Lut2D };
        [Range(0f, 1f), Tooltip("LUT blend")]
        public FloatParameter blend = new FloatParameter { value = 1.0f };
        [DisplayName("Use Background LUT"), Tooltip("Activate background LUT")]
        public BoolParameter useBackgroundLut = new BoolParameter { value = false };
        [DisplayName("Background LUT"), Tooltip("Background Lookup Texture, 1024x32")]
        public TextureParameter backgroundLut = new TextureParameter { value = null, defaultState = TextureParameterDefault.Lut2D };
        [Tooltip("Distance at which blend to background starts")]
        public FloatParameter backgroundBlendStart = new FloatParameter { value = 50.0f };
        [Tooltip("Range of blending default LUT to background LUT (from start of blend)")]
        public FloatParameter backgroundBlendRange = new FloatParameter { value = 10.0f };
        [DisplayName("External"), Tooltip("Switch Modes")]
        public BoolParameter external = new BoolParameter { value = false };

        //public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        //{
        //    if (lut.value == null || blend.value == 0)
        //    {
        //        return false;
        //    }
        //    return enabled.value;
        //}
    }

    public sealed class AgXColorPostRenderer : PostProcessEffectRenderer<AgXColorPost>
    {
        private Shader lutbaker, shader;
        AssetBundle assetbundle;
        RenderTexture m_InternalLdrLut;
        Texture2D m_GradingCurves;
        const int k_Lut2DSize = 32;
        readonly Color[] m_Pixels = new Color[Spline.k_Precision * 2];

        static class ShaderIDs
        {
            internal static readonly int ColorBalance = Shader.PropertyToID("_ColorBalance");
            internal static readonly int ColorFilter = Shader.PropertyToID("_ColorFilter");
            internal static readonly int HueSatCon = Shader.PropertyToID("_HueSatCon");
            internal static readonly int Brightness = Shader.PropertyToID("_Brightness");

            internal static readonly int Lut2D = Shader.PropertyToID("_Lut2D");
            internal static readonly int Lut2D_Params = Shader.PropertyToID("_Lut2D_Params");
            internal static readonly int _UserLut = Shader.PropertyToID("_UserLut");
            internal static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");
            internal static readonly int _BGLut = Shader.PropertyToID("_BGLut");
            internal static readonly int _BGLut_Params = Shader.PropertyToID("_BGLut_Params");
            internal static readonly int _BGLut_Blend = Shader.PropertyToID("_BGLut_Blend");
            internal static readonly int Lift = Shader.PropertyToID("_Lift");
            internal static readonly int InvGamma = Shader.PropertyToID("_InvGamma");
            internal static readonly int Gain = Shader.PropertyToID("_Gain");
        }

        public override DepthTextureMode GetCameraFlags()
        {
            if (settings.useBackgroundLut.value)
                return DepthTextureMode.Depth;
            return DepthTextureMode.None;
        }

        public override void Init()
        {
            if (lutbaker != null) return;

            assetbundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("userlut.unity3d"));
            if (assetbundle == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load asset bunle 'userlut.unity3d'");

            lutbaker = assetbundle.LoadAsset<Shader>("Assets/X-PostProcessing/Effects/AgXColor/Shaders/PostCorrection.shader");
            if (lutbaker == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load shader 'HanmenColorGrading.shader'");

            shader = assetbundle.LoadAsset<Shader>("Assets/X-PostProcessing/Effects/AgXColor/Shaders/UserLUT.shader");
            if (shader == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load shader 'HanmenColorGrading.shader'");

            assetbundle.Unload(false);
        }

        public override void Render(PostProcessRenderContext context)
        {
            //if (settings.external)
            //{
            //    RenderExternal(context);
            //}
            //else
            //{
            //    RenderInternal(context);
            //}
            RenderExternal(context);
        }

        void RenderExternal(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.ClearKeywords();
            {
                CheckInternalStripLut();

                // Lut setup
                var lutSheet = context.propertySheets.Get(lutbaker);
                lutSheet.ClearKeywords();

                lutSheet.properties.SetVector(ShaderIDs.Lut2D_Params, new Vector4(k_Lut2DSize, 0.5f / (k_Lut2DSize * k_Lut2DSize), 0.5f / k_Lut2DSize, k_Lut2DSize / (k_Lut2DSize - 1f)));

                lutSheet.properties.SetVector(ShaderIDs.ColorFilter, settings.colorFilter.value);

                float hue = settings.hueShift.value / 360f;         // Remap to [-0.5;0.5]
                float sat = settings.saturation.value / 100f + 1f;  // Remap to [0;2]
                float con = settings.contrast.value / 100f + 1f;    // Remap to [0;2]
                lutSheet.properties.SetVector(ShaderIDs.HueSatCon, new Vector3(hue, sat, con));

                lutSheet.properties.SetFloat(ShaderIDs.Brightness, (settings.brightness.value + 100f) / 100f);

                var lift = ColorUtilities.ColorToLift(settings.lift.value * 0.2f);
                var gain = ColorUtilities.ColorToGain(settings.gain.value * 0.8f);
                var invgamma = ColorUtilities.ColorToInverseGamma(settings.gamma.value * 0.8f);
                lutSheet.properties.SetVector(ShaderIDs.Lift, lift);
                lutSheet.properties.SetVector(ShaderIDs.InvGamma, invgamma);
                lutSheet.properties.SetVector(ShaderIDs.Gain, gain);

                context.command.BlitFullscreenTriangle(UnityEngine.Rendering.BuiltinRenderTextureType.None, m_InternalLdrLut, lutSheet, 0);
            }

            Texture lut = m_InternalLdrLut;
            Vector4 lutParams = new Vector4(1f / lut.width, 1f / lut.height, lut.height - 1f, 1);
            sheet.properties.SetTexture(ShaderIDs._UserLut, lut);
            sheet.properties.SetVector(ShaderIDs._UserLut_Params, lutParams);

            if (settings.useBackgroundLut)
            {
                sheet.EnableKeyword("USE_BG_LUT");
                if (settings.backgroundLut.value != null)
                {
                    lut = settings.backgroundLut.value;
                }
                lutParams = new Vector4(1f / lut.width, 1f / lut.height, lut.height - 1f, 0);
                sheet.properties.SetTexture(ShaderIDs._BGLut, lut);
                sheet.properties.SetVector(ShaderIDs._BGLut_Params, lutParams);
                sheet.properties.SetVector(ShaderIDs._BGLut_Blend, new Vector4(settings.backgroundBlendStart.value, settings.backgroundBlendRange.value, 0, settings.blend.value));
            }
            //else
            //{
            //    sheet.DisableKeyword("USE_BG_LUT");
            //}


            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

        void RenderInternal(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.ClearKeywords();
            Texture lut = null;
            //lut = null;
            //Vector4 lutParams = new Vector4(1f / lut.width, 1f / lut.height, lut.height - 1f, settings.blend.value);
            //sheet.properties.SetTexture(ShaderIDs._UserLut, lut);
            //sheet.properties.SetVector(ShaderIDs._UserLut_Params, lutParams);


            if (settings.useBackgroundLut)
            {
                sheet.EnableKeyword("USE_BG_LUT");
                if (settings.backgroundLut.value != null)
                {
                    lut = settings.backgroundLut.value;
                }
                Vector4 lutParams = new Vector4(1f / lut.width, 1f / lut.height, lut.height - 1f, 0);
                sheet.properties.SetTexture(ShaderIDs._BGLut, lut);
                sheet.properties.SetVector(ShaderIDs._BGLut_Params, lutParams);
                sheet.properties.SetVector(ShaderIDs._BGLut_Blend, new Vector4(settings.backgroundBlendStart.value, settings.backgroundBlendRange.value, 0, settings.blend.value));
            }

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 1);
        }

        void CheckInternalStripLut()
        {
            // Check internal lut state, (re)create it if needed
            if (m_InternalLdrLut == null || !m_InternalLdrLut.IsCreated())
            {
                RuntimeUtilities.Destroy(m_InternalLdrLut);

                var format = GetLutFormat();
                m_InternalLdrLut = new RenderTexture(k_Lut2DSize * k_Lut2DSize, k_Lut2DSize, 0, format, RenderTextureReadWrite.Linear)
                {
                    name = "Color Grading Strip Lut",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0,
                    autoGenerateMips = false,
                    useMipMap = false
                };
                m_InternalLdrLut.Create();
            }
        }
        Texture2D GetCurveTexture(bool hdr)
        {
            if (m_GradingCurves == null)
            {
                m_GradingCurves = new Texture2D(Spline.k_Precision, 2, TextureFormat.RGBAHalf, false, true)
                {
                    name = "Internal Curves Texture",
                    hideFlags = HideFlags.DontSave,
                    anisoLevel = 0,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
            }

            var hueVsHueCurve = settings.hueVsHueCurve.value;
            var hueVsSatCurve = settings.hueVsSatCurve.value;
            var satVsSatCurve = settings.satVsSatCurve.value;
            var lumVsSatCurve = settings.lumVsSatCurve.value;
            var masterCurve = settings.masterCurve.value;
            var redCurve = settings.redCurve.value;
            var greenCurve = settings.greenCurve.value;
            var blueCurve = settings.blueCurve.value;

            var pixels = m_Pixels;

            for (int i = 0; i < Spline.k_Precision; i++)
            {
                // Secondary/VS curves
                float x = hueVsHueCurve.cachedData[i];
                float y = hueVsSatCurve.cachedData[i];
                float z = satVsSatCurve.cachedData[i];
                float w = lumVsSatCurve.cachedData[i];
                pixels[i] = new Color(x, y, z, w);

                // YRGB
                if (!hdr)
                {
                    float m = masterCurve.cachedData[i];
                    float r = redCurve.cachedData[i];
                    float g = greenCurve.cachedData[i];
                    float b = blueCurve.cachedData[i];
                    pixels[i + Spline.k_Precision] = new Color(r, g, b, m);
                }
            }

            m_GradingCurves.SetPixels(pixels);
            m_GradingCurves.Apply(false, false);

            return m_GradingCurves;
        }

        public static Vector3 ColorToLinearIntensity(Vector4 color)
        {
            // Highlights
            var H = new Vector3(color.x, color.y, color.z);
            //float lumGain = H.x * 0.2126f + H.y * 0.7152f + H.z * 0.0722f;
            //H = new Vector3(H.x - lumGain, H.y - lumGain, H.z - lumGain);

            float gainOffset = color.w;
            return new Vector3(H.x * gainOffset, H.y * gainOffset, H.z * gainOffset);
        }

        static RenderTextureFormat GetLutFormat()
        {
            var format = RenderTextureFormat.ARGBHalf;

            return format;
        }

        //static TextureFormat GetCurveFormat()
        //{
        //    // Use RGBAHalf if possible, fallback on ARGB32 otherwise
        //    var format = TextureFormat.RGBAHalf;

        //    if (!SystemInfo.SupportsTextureFormat(format))
        //        format = TextureFormat.ARGB32;

        //    return format;
        //}

        public override void Release()
        {
            RuntimeUtilities.Destroy(m_InternalLdrLut);
            m_InternalLdrLut = null;

            RuntimeUtilities.Destroy(m_GradingCurves);
            m_GradingCurves = null;
        }
    }
}

