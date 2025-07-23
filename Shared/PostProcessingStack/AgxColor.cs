using KKAPI.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;


namespace Graphics
{
    [Serializable]
    [PostProcess(typeof(AgXColorRenderer), PostProcessEvent.BeforeStack, "Custom/UserLUT")]
    public sealed class AgXColor : PostProcessEffectSettings
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
        [DisplayName("Red"), Range(-200f, 200f), Tooltip("Modify influence of the red channel in the overall mix.")]
        public FloatParameter mixerRedOutRedIn = new FloatParameter { value = 100f };
        [DisplayName("Green"), Range(-200f, 200f), Tooltip("Modify influence of the green channel in the overall mix.")]
        public FloatParameter mixerRedOutGreenIn = new FloatParameter { value = 0f };
        [DisplayName("Blue"), Range(-200f, 200f), Tooltip("Modify influence of the blue channel in the overall mix.")]
        public FloatParameter mixerRedOutBlueIn = new FloatParameter { value = 0f };
        [DisplayName("Red"), Range(-200f, 200f), Tooltip("Modify influence of the red channel in the overall mix.")]
        public FloatParameter mixerGreenOutRedIn = new FloatParameter { value = 0f };
        [DisplayName("Green"), Range(-200f, 200f), Tooltip("Modify influence of the green channel in the overall mix.")]
        public FloatParameter mixerGreenOutGreenIn = new FloatParameter { value = 100f };
        [DisplayName("Blue"), Range(-200f, 200f), Tooltip("Modify influence of the blue channel in the overall mix.")]
        public FloatParameter mixerGreenOutBlueIn = new FloatParameter { value = 0f };
        [DisplayName("Red"), Range(-200f, 200f), Tooltip("Modify influence of the red channel in the overall mix.")]
        public FloatParameter mixerBlueOutRedIn = new FloatParameter { value = 0f };
        [DisplayName("Green"), Range(-200f, 200f), Tooltip("Modify influence of the green channel in the overall mix.")]
        public FloatParameter mixerBlueOutGreenIn = new FloatParameter { value = 0f };
        [DisplayName("Blue"), Range(-200f, 200f), Tooltip("Modify influence of the blue channel in the overall mix.")]
        public FloatParameter mixerBlueOutBlueIn = new FloatParameter { value = 100f };
        [DisplayName("Offset"), Tooltip("Controls the darkest portions of the render.")]
        public Vector4Parameter offset = new Vector4Parameter { value = new Vector4(1f, 1f, 1f, 0f) };
        [DisplayName("Power"), Tooltip("Power function that controls mid-range tones.")]
        public Vector4Parameter power = new Vector4Parameter { value = new Vector4(1f, 1f, 1f, 1f) };
        [DisplayName("Slope"), Tooltip("Controls the lightest portions of the render.")]
        public Vector4Parameter slope = new Vector4Parameter { value = new Vector4(1f, 1f, 1f, 1f) };
        [DisplayName("Color Boost"), Range(-0f, 1f), Tooltip("Adjusts the saturation of the lower saturated areas, without changing the highly saturated areas.")]
        public FloatParameter colorBoost = new FloatParameter { value = 0f };
        [DisplayName("Perceptual"), Range(0f, 1f), Tooltip("Use perceptual color space for the curves, instead of linear RGB.")]
        public FloatParameter perceptual = new FloatParameter { value = 0.5f };
        [DisplayName("Exposure"), Range(0f, 1f), Tooltip("Sets exposure pre-transform.")]
        public FloatParameter exposure = new FloatParameter { value = 0f };
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
        //[Range(0f, 1f), Tooltip("LUT blend")]
        //public FloatParameter blend = new FloatParameter { value = 1.0f };
        //[DisplayName("Use Background LUT"), Tooltip("Activate background LUT")]
        //public BoolParameter useBackgroundLut = new BoolParameter { value = false };
        //[DisplayName("Background LUT"), Tooltip("Background Lookup Texture, 1024x32")]
        //public TextureParameter backgroundLut = new TextureParameter { value = null, defaultState = TextureParameterDefault.Lut2D };
        //[Tooltip("Distance at which blend to background starts")]
        //public FloatParameter backgroundBlendStart = new FloatParameter { value = 50.0f };
        //[Tooltip("Range of blending default LUT to background LUT (from start of blend)")]
        //public FloatParameter backgroundBlendRange = new FloatParameter { value = 10.0f };
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
        public enum ChannelMixer
        {
            Red = 0,
            Green = 1,
            Blue = 2,
        }
        public ChannelMixer channelMixer = ChannelMixer.Red;
    }

    public sealed class AgXColorRenderer : PostProcessEffectRenderer<AgXColor>
    {
        private Shader lutbaker, shader;
        AssetBundle assetbundle;
        RenderTexture m_InternalLdrLut;
        Texture2D m_GradingCurves;
        const int k_Lut2DSize = 32;
        readonly Color[] m_Pixels = new Color[Spline.k_Precision * 2];

        static class ShaderIDs
        {
            internal static readonly int PostExposure = Shader.PropertyToID("_PostExposure");
            internal static readonly int ColorBalance = Shader.PropertyToID("_ColorBalance");
            internal static readonly int ColorFilter = Shader.PropertyToID("_ColorFilter");
            internal static readonly int HueSatCon = Shader.PropertyToID("_HueSatCon");
            internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
            internal static readonly int ChannelMixerRed = Shader.PropertyToID("_ChannelMixerRed");
            internal static readonly int ChannelMixerGreen = Shader.PropertyToID("_ChannelMixerGreen");
            internal static readonly int ChannelMixerBlue = Shader.PropertyToID("_ChannelMixerBlue");
            internal static readonly int Offset = Shader.PropertyToID("_Offset");
            internal static readonly int Power = Shader.PropertyToID("_Power");
            internal static readonly int Slope = Shader.PropertyToID("_Slope");
            internal static readonly int Curves = Shader.PropertyToID("_Curves");
            internal static readonly int CustomToneCurve = Shader.PropertyToID("_CustomToneCurve");
            internal static readonly int ToeSegmentA = Shader.PropertyToID("_ToeSegmentA");
            internal static readonly int ToeSegmentB = Shader.PropertyToID("_ToeSegmentB");
            internal static readonly int MidSegmentA = Shader.PropertyToID("_MidSegmentA");
            internal static readonly int MidSegmentB = Shader.PropertyToID("_MidSegmentB");
            internal static readonly int ShoSegmentA = Shader.PropertyToID("_ShoSegmentA");
            internal static readonly int ShoSegmentB = Shader.PropertyToID("_ShoSegmentB");
            internal static readonly int Lut2D = Shader.PropertyToID("_Lut2D");
            internal static readonly int Lut3D = Shader.PropertyToID("_Lut3D");
            internal static readonly int Lut3D_Params = Shader.PropertyToID("_Lut3D_Params");
            internal static readonly int Lut2D_Params = Shader.PropertyToID("_Lut2D_Params");
            internal static readonly int _UserLut = Shader.PropertyToID("_UserLut");
            internal static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");
            //internal static readonly int _BGLut = Shader.PropertyToID("_BGLut");
            //internal static readonly int _BGLut_Params = Shader.PropertyToID("_BGLut_Params");
            //internal static readonly int _BGLut_Blend = Shader.PropertyToID("_BGLut_Blend");
            internal static readonly int ColorBoost = Shader.PropertyToID("_ColorBoost");
            internal static readonly int Perceptual = Shader.PropertyToID("_Perceptual");
            internal static readonly int Exposure = Shader.PropertyToID("_Exposure");
        }

        //public override DepthTextureMode GetCameraFlags()
        //{
        //    if (settings.useBackgroundLut.value)
        //        return DepthTextureMode.Depth;
        //    return DepthTextureMode.None;
        //}

        public override void Init()
        {
            if (lutbaker != null) return;

            assetbundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("userlut.unity3d"));
            if (assetbundle == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load asset bunle 'userlut.unity3d'");

            lutbaker = assetbundle.LoadAsset<Shader>("Assets/X-PostProcessing/Effects/AgXColor/Shaders/PreCorrection.shader");
            if (lutbaker == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load shader 'HanmenColorGrading.shader'");

            shader = assetbundle.LoadAsset<Shader>("Assets/X-PostProcessing/Effects/AgXColor/Shaders/UserLUT.shader");
            if (shader == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load shader 'HanmenColorGrading.shader'");

            assetbundle.Unload(false);
        }

        public override void Render(PostProcessRenderContext context)
        {
            //if (settings.external)
            //{
            RenderExternal(context);
            //}
            //else
            //{

            //    RenderInternal(context);
            //}
        }

        void RenderExternal(PostProcessRenderContext context)
        {
            //var sheet = context.propertySheets.Get(shader);
            //sheet.ClearKeywords();
            //{
            //CheckInternalStripLut();

            // Lut setup
            var lutSheet = context.propertySheets.Get(lutbaker);
            lutSheet.ClearKeywords();

            lutSheet.properties.SetVector(ShaderIDs.Lut2D_Params, new Vector4(k_Lut2DSize, 0.5f / (k_Lut2DSize * k_Lut2DSize), 0.5f / k_Lut2DSize, k_Lut2DSize / (k_Lut2DSize - 1f)));

            var colorBalance = ColorUtilities.ComputeColorBalance(settings.temperature.value, settings.tint.value);
            lutSheet.properties.SetVector(ShaderIDs.ColorBalance, colorBalance);
            lutSheet.properties.SetVector(ShaderIDs.ColorFilter, settings.colorFilter.value);

            float hue = settings.hueShift.value / 360f;         // Remap to [-0.5;0.5]
            float sat = settings.saturation.value / 100f + 1f;  // Remap to [0;2]
            float con = settings.contrast.value / 100f + 1f;    // Remap to [0;2]
            lutSheet.properties.SetVector(ShaderIDs.HueSatCon, new Vector3(hue, sat, con));

            var channelMixerR = new Vector3(settings.mixerRedOutRedIn, settings.mixerRedOutGreenIn, settings.mixerRedOutBlueIn);
            var channelMixerG = new Vector3(settings.mixerGreenOutRedIn, settings.mixerGreenOutGreenIn, settings.mixerGreenOutBlueIn);
            var channelMixerB = new Vector3(settings.mixerBlueOutRedIn, settings.mixerBlueOutGreenIn, settings.mixerBlueOutBlueIn);
            lutSheet.properties.SetVector(ShaderIDs.ChannelMixerRed, channelMixerR / 100f);            // Remap to [-2;2]
            lutSheet.properties.SetVector(ShaderIDs.ChannelMixerGreen, channelMixerG / 100f);
            lutSheet.properties.SetVector(ShaderIDs.ChannelMixerBlue, channelMixerB / 100f);

            //convert to color with intensity linearly.
            var offset = ColorToLinearIntensity(settings.offset.value);
            var power = ColorToLinearIntensity(settings.power.value);
            var slope = ColorToLinearIntensity(settings.slope.value);

            lutSheet.properties.SetVector(ShaderIDs.Offset, offset);
            lutSheet.properties.SetVector(ShaderIDs.Power, power);
            lutSheet.properties.SetVector(ShaderIDs.Slope, slope);

            lutSheet.properties.SetFloat(ShaderIDs.ColorBoost, settings.colorBoost.value);
            lutSheet.properties.SetFloat(ShaderIDs.Perceptual, settings.perceptual.value);
            lutSheet.properties.SetFloat(ShaderIDs.Exposure, settings.exposure.value);

            lutSheet.properties.SetFloat(ShaderIDs.Brightness, (settings.brightness.value + 100f) / 100f);
            lutSheet.properties.SetTexture(ShaderIDs.Curves, GetCurveTexture(false));

            //Generate LUT
            //context.command.BlitFullscreenTriangle(UnityEngine.Rendering.BuiltinRenderTextureType.None, m_InternalLdrLut, lutSheet, 0);
            //}

            //Texture lut = m_InternalLdrLut;
            //Vector4 lutParams = new Vector4(1f / lut.width, 1f / lut.height, lut.height - 1f, 1);
            //sheet.properties.SetTexture(ShaderIDs._UserLut, lut);
            //sheet.properties.SetVector(ShaderIDs._UserLut_Params, lutParams);

            context.command.BlitFullscreenTriangle(context.source, context.destination, lutSheet, 0);
        }

        //void RenderInternal(PostProcessRenderContext context)
        //{
        //    var sheet = context.propertySheets.Get(shader);
        //    sheet.ClearKeywords();
        //    Texture lut = null;
        //lut = null;
        //Vector4 lutParams = new Vector4(1f / lut.width, 1f / lut.height, lut.height - 1f, settings.blend.value);
        //sheet.properties.SetTexture(ShaderIDs._UserLut, lut);
        //sheet.properties.SetVector(ShaderIDs._UserLut_Params, lutParams);


        //if (settings.useBackgroundLut)
        //{
        //    sheet.EnableKeyword("USE_BG_LUT");
        //    if (settings.backgroundLut.value != null)
        //    {
        //        lut = settings.backgroundLut.value;
        //    }
        //    Vector4 lutParams = new Vector4(1f / lut.width, 1f / lut.height, lut.height - 1f, 0);
        //    sheet.properties.SetTexture(ShaderIDs._BGLut, lut);
        //    sheet.properties.SetVector(ShaderIDs._BGLut_Params, lutParams);
        //    sheet.properties.SetVector(ShaderIDs._BGLut_Blend, new Vector4(settings.backgroundBlendStart.value, settings.backgroundBlendRange.value, 0, 0));
        //}

        //    context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 1);
        //}

        //void CheckInternalStripLut()
        //{
        //    // Check internal lut state, (re)create it if needed
        //    if (m_InternalLdrLut == null || !m_InternalLdrLut.IsCreated())
        //    {
        //        RuntimeUtilities.Destroy(m_InternalLdrLut);

        //        var format = GetLutFormat();
        //        m_InternalLdrLut = new RenderTexture(k_Lut2DSize * k_Lut2DSize, k_Lut2DSize, 0, format, RenderTextureReadWrite.Linear)
        //        {
        //            name = "Color Grading Strip Lut",
        //            hideFlags = HideFlags.DontSave,
        //            filterMode = FilterMode.Bilinear,
        //            wrapMode = TextureWrapMode.Clamp,
        //            anisoLevel = 0,
        //            autoGenerateMips = false,
        //            useMipMap = false
        //        };
        //        m_InternalLdrLut.Create();
        //    }
        //}
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

