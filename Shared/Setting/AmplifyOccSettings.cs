using MessagePack;
using Graphics.AmplifyOcclusion;
using AOE = Graphics.AmplifyOcclusion.AmplifyOcclusionEffect;
using UnityEngine;
using ADV.Commands.Base;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class AmplifyOccSettings
    {
        public bool Enabled = false;
        public ApplicationMethod ApplyMethod = ApplicationMethod.Deferred;
        public enum ApplicationMethod
        {
            PostEffect = 0,
            Deferred,
            Debug
        }
        public PerPixelNormalSource PerPixelNormals = PerPixelNormalSource.Camera;
        public enum PerPixelNormalSource
        {
            None = 0,
            Camera,
            GBuffer,
            OctaEncoded,
        }
        public SampleCountLevel SampleCount = SampleCountLevel.High;
        public enum SampleCountLevel
        {
            Low = 0,
            Medium,
            High,
            VeryHigh
        }
        public FloatValue Intensity = new FloatValue (1.0f, false);
        public Color Tint = new Color(0, 0, 0);
        public FloatValue Radius = new FloatValue(2.0f, false);
        public FloatValue PowerExponent = new FloatValue(1.8f, false);
        public FloatValue Bias = new FloatValue(0.05f, false);
        public FloatValue Thickness = new FloatValue(1.0f, false);
        public BoolValue Downsample = new BoolValue(true, false);
        public BoolValue CacheAware = new BoolValue(true, false);
        public BoolValue FadeEnabled = new BoolValue(false, false);
        public FloatValue FadeStart = new FloatValue(100.0f, false);
        public FloatValue FadeLength = new FloatValue(50.0f, false);
        public FloatValue FadeToIntensity = new FloatValue(0.0f, false);
        public Color FadeToTint = Color.black;
        public FloatValue FadeToRadius = new FloatValue(2.0f, false);
        public FloatValue FadeToPowerExponent = new FloatValue(1.0f, false);
        public FloatValue FadeToThickness = new FloatValue(1.0f, false);
        public BoolValue BlurEnabled = new BoolValue(true, false);
        public IntValue BlurRadius = new IntValue(3, false);
        public IntValue BlurPasses = new IntValue(1, false);
        public FloatValue BlurSharpness = new FloatValue(15.0f, false);
        public BoolValue FilterEnabled = new BoolValue(true, false);
        public BoolValue FilterDownsample = new BoolValue(true, false);
        public FloatValue FilterBlending = new FloatValue(0.80f, false);
        public FloatValue FilterResponse = new FloatValue(0.50f, false);

        public void Load(AmplifyOcclusionEffect amplifyocc)
        {
            if (amplifyocc == null)
                return;

            amplifyocc.enabled = Enabled;
            amplifyocc.ApplyMethod = (AmplifyOcclusionEffect.ApplicationMethod)ApplyMethod;
            amplifyocc.PerPixelNormals = (AmplifyOcclusionEffect.PerPixelNormalSource)PerPixelNormals;
            //amplifyocc.SampleCount = (AmplifyOcclusionEffect.SampleCountLevel)SampleCount;
            amplifyocc.Intensity = Intensity.value;
            amplifyocc.Tint = Tint;
            amplifyocc.Radius = Radius.value;
            amplifyocc.PowerExponent = PowerExponent.value;
            amplifyocc.Bias = Bias.value;
            amplifyocc.Thickness = Thickness.value;
            amplifyocc.Downsample = Downsample.value;
            amplifyocc.CacheAware = CacheAware.value;
            amplifyocc.FadeEnabled = FadeEnabled.value;
            amplifyocc.FadeStart = FadeStart.value;
            amplifyocc.FadeLength = FadeLength.value;
            amplifyocc.FadeToIntensity = FadeToIntensity.value;
            amplifyocc.FadeToTint = FadeToTint;
            amplifyocc.FadeToRadius = FadeToRadius.value;
            amplifyocc.FadeToPowerExponent = FadeToPowerExponent.value;
            amplifyocc.FadeToThickness = FadeToThickness.value;
            amplifyocc.BlurEnabled = BlurEnabled.value;
            amplifyocc.BlurRadius = BlurRadius.value;
            amplifyocc.BlurPasses = BlurPasses.value;
            amplifyocc.BlurSharpness = BlurSharpness.value;
            amplifyocc.FilterEnabled = FilterEnabled.value;
            //amplifyocc.FilterDownsample = FilterDownsample.value;
            amplifyocc.FilterBlending = FilterBlending.value;
            amplifyocc.FilterResponse = FilterResponse.value;

        }

        public void Save(AmplifyOcclusionEffect amplifyocc)
        {
            if (amplifyocc == null)
                return;

            Enabled = amplifyocc.enabled;
            Intensity.value = amplifyocc.Radius;
            Tint = amplifyocc.Tint;
            Radius.value = amplifyocc.Radius;
            PowerExponent.value = amplifyocc.PowerExponent;
            Bias.value = amplifyocc.Bias;
            Thickness.value = amplifyocc.Thickness;
            Downsample.value = amplifyocc.Downsample;
            CacheAware.value = amplifyocc.CacheAware;
            FadeEnabled.value = amplifyocc.FadeEnabled;
            FadeStart.value = amplifyocc.FadeStart;
            FadeLength.value = amplifyocc.FadeLength;
            FadeToIntensity.value = amplifyocc.FadeToIntensity;
            FadeToTint = amplifyocc.FadeToTint;
            FadeToRadius.value = amplifyocc.FadeToRadius;
            FadeToPowerExponent.value = amplifyocc.FadeToPowerExponent;
            FadeToThickness.value = amplifyocc.FadeToThickness;
            BlurEnabled.value = amplifyocc.BlurEnabled;
            BlurRadius.value = amplifyocc.BlurRadius;
            BlurPasses.value = amplifyocc.BlurPasses;
            BlurSharpness.value = amplifyocc.BlurSharpness;
            FilterEnabled.value = amplifyocc.FilterEnabled;
            //FilterDownsample.value = amplifyocc.FilterDownsample;
            FilterBlending.value = amplifyocc.FilterBlending;
            FilterResponse.value = amplifyocc.FilterResponse;
        }
    }
}
