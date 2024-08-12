using Graphics.Inspector;
using MessagePack;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Graphics.XPostProcessing;
//using Graphics.BeautifyForPPS;
#if AI
using static AmplifyOcclusionBase;
#endif
// Haha
// This is funny
// TODO: Find Better Names and change the names with refactoring tools.
// TODO: Is this the best way to do this?
namespace Graphics.Settings
{
    [MessagePackObject(keyAsPropertyName: true)]
    public struct AutoExposureParams
    {
        public BoolValue enabled;
        public Vector2Value filtering; // vector2
        public FloatValue minLuminance;
        public FloatValue maxLuminance;
        public FloatValue keyValue;
        public EyeAdaptationValue eyeAdaptation; // EyeAdaptationParameter
        public FloatValue speedUp;
        public FloatValue speedDown;
        public void Save(UnityEngine.Rendering.PostProcessing.AutoExposure layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                filtering = new Vector2Value(layer.filtering);
                minLuminance = new FloatValue(layer.minLuminance);
                maxLuminance = new FloatValue(layer.maxLuminance);
                keyValue = new FloatValue(layer.keyValue);
                speedUp = new FloatValue(layer.speedUp);
                speedDown = new FloatValue(layer.speedDown);
                eyeAdaptation = new EyeAdaptationValue(layer.eyeAdaptation);
            }
        }

        public void Load(UnityEngine.Rendering.PostProcessing.AutoExposure layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                filtering.Fill(layer.filtering);
                minLuminance.Fill(layer.minLuminance);
                maxLuminance.Fill(layer.maxLuminance);
                keyValue.Fill(layer.keyValue);
                speedUp.Fill(layer.speedUp);
                speedDown.Fill(layer.speedDown);
                eyeAdaptation.Fill(layer.eyeAdaptation);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct AmbientOcclusionParams
    {
        public BoolValue enabled;
        public AmbientOcclusionModeValue mode;
        public FloatValue intensity;
        public ColorValue color;
        public BoolValue ambientOnly;
        public FloatValue noiseFilterTolerance;
        public FloatValue blurTolerance;
        public FloatValue upsampleTolerance;
        public FloatValue thicknessModifier;
        public FloatValue directLightingStrength;
        public FloatValue radius;
        public AmbientOcclusionQualityValue quality;
        public void Save(UnityEngine.Rendering.PostProcessing.AmbientOcclusion layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                mode = new AmbientOcclusionModeValue(layer.mode);
                intensity = new FloatValue(layer.intensity);
                color = new ColorValue(layer.color);
                ambientOnly = new BoolValue(layer.ambientOnly);
                noiseFilterTolerance = new FloatValue(layer.noiseFilterTolerance);
                blurTolerance = new FloatValue(layer.blurTolerance);
                upsampleTolerance = new FloatValue(layer.upsampleTolerance);
                thicknessModifier = new FloatValue(layer.thicknessModifier);
                directLightingStrength = new FloatValue(layer.directLightingStrength);
                radius = new FloatValue(layer.radius);
                quality = new AmbientOcclusionQualityValue(layer.quality);
            }
        }

        public void Load(UnityEngine.Rendering.PostProcessing.AmbientOcclusion layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                mode.Fill(layer.mode);
                intensity.Fill(layer.intensity);
                color.Fill(layer.color);
                ambientOnly.Fill(layer.ambientOnly);
                noiseFilterTolerance.Fill(layer.noiseFilterTolerance);
                blurTolerance.Fill(layer.blurTolerance);
                upsampleTolerance.Fill(layer.upsampleTolerance);
                thicknessModifier.Fill(layer.thicknessModifier);
                directLightingStrength.Fill(layer.directLightingStrength);
                radius.Fill(layer.radius);
                quality.Fill(layer.quality);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct AmplifyOcclusionParams
    {
        public bool enabled;
        public int ApplyMethod;
        public float FilterResponse;
        public float FilterBlending;
        public bool FilterEnabled;
        public float BlurSharpness;
        public int BlurPasses;
        public int BlurRadius;
        public bool BlurEnabled;
        public float FadeToThickness;
        public float FadeToPowerExponent;
        public float FadeToRadius;
        public Color FadeToTint;
        public float FadeLength;
        public float FadeToIntensity;
        public bool FadeEnabled;
        public bool CacheAware;
        public bool Downsample;
        public float Thickness;
        public float Bias;
        public float PowerExponent;
        public float Radius;
        public Color Tint;
        public float Intensity;
        public int PerPixelNormals;
        public int SampleCount;
        public float FadeStart;

#if AI
        public void Save(AmplifyOcclusionEffect component)
        {
            if (component != null)
            {
                this.enabled = component.enabled;
                this.ApplyMethod = (int)component.ApplyMethod;
                this.FilterResponse = component.FilterResponse;
                this.FilterBlending = component.FilterBlending;
                this.FilterEnabled = component.FilterEnabled;
                this.BlurSharpness = component.BlurSharpness;
                this.BlurPasses = component.BlurPasses;
                this.BlurRadius = component.BlurRadius;
                this.BlurEnabled = component.BlurEnabled;
                this.FadeToThickness = component.FadeToThickness;
                this.FadeToPowerExponent = component.FadeToPowerExponent;
                this.FadeToRadius = component.FadeToRadius;
                this.FadeToTint = component.FadeToTint;
                this.FadeLength = component.FadeLength;
                this.FadeToIntensity = component.FadeToIntensity;
                this.FadeEnabled = component.FadeEnabled;
                this.CacheAware = component.CacheAware;
                this.Downsample = component.Downsample;
                this.Thickness = component.Thickness;
                this.Bias = component.Bias;
                this.PowerExponent = component.PowerExponent;
                this.Radius = component.Radius;
                this.Tint = component.Tint;
                this.Intensity = component.Intensity;
                this.PerPixelNormals = (int)component.PerPixelNormals;
                this.SampleCount = (int)component.SampleCount;
                this.FadeStart = component.FadeStart;
            }
        }
        public void Load(AmplifyOcclusionEffect component)
        {
            if (component != null)
            {
                component.enabled = this.enabled;
                component.ApplyMethod = (ApplicationMethod)this.ApplyMethod;
                component.FilterResponse = this.FilterResponse;
                component.FilterBlending = this.FilterBlending;
                component.FilterEnabled = this.FilterEnabled;
                component.BlurSharpness = this.BlurSharpness;
                component.BlurPasses = this.BlurPasses;
                component.BlurRadius = this.BlurRadius;
                component.BlurEnabled = this.BlurEnabled;
                component.FadeToThickness = this.FadeToThickness;
                component.FadeToPowerExponent = this.FadeToPowerExponent;
                component.FadeToRadius = this.FadeToRadius;
                component.FadeToTint = this.FadeToTint;
                component.FadeLength = this.FadeLength;
                component.FadeToIntensity = this.FadeToIntensity;
                component.FadeEnabled = this.FadeEnabled;
                component.CacheAware = this.CacheAware;
                component.Downsample = this.Downsample;
                component.Thickness = this.Thickness;
                component.Bias = this.Bias;
                component.PowerExponent = this.PowerExponent;
                component.Radius = this.Radius;
                component.Tint = this.Tint;
                component.Intensity = this.Intensity;
                component.PerPixelNormals = (PerPixelNormalSource)this.PerPixelNormals;
                component.SampleCount = (SampleCountLevel)this.SampleCount;
                component.FadeStart = this.FadeStart;
            }
        }
#endif
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct BloomParams
    {
        public BoolValue enabled;
        public FloatValue intensity;
        public FloatValue threshold;
        public FloatValue softKnee;
        public FloatValue clamp;
        public FloatValue diffusion;
        public FloatValue anamorphicRatio;
        public ColorValue color;
        public BoolValue fastMode;
        public FloatValue dirtIntensity;
        public string dirtTexture;
        public bool dirtState;

        public void Save(UnityEngine.Rendering.PostProcessing.Bloom layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                intensity = new FloatValue(layer.intensity);
                threshold = new FloatValue(layer.threshold);
                softKnee = new FloatValue(layer.softKnee);
                clamp = new FloatValue(layer.clamp);
                diffusion = new FloatValue(layer.diffusion);
                anamorphicRatio = new FloatValue(layer.anamorphicRatio);
                color = new ColorValue(layer.color);
                fastMode = new BoolValue(layer.fastMode);
                dirtIntensity = new FloatValue(layer.dirtIntensity);
                dirtTexture = Graphics.Instance.PostProcessingManager.CurrentLensDirtTexturePath;
                dirtState = layer.dirtTexture.overrideState;
            }
        }
        public void Load(UnityEngine.Rendering.PostProcessing.Bloom layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                intensity.Fill(layer.intensity);
                threshold.Fill(layer.threshold);
                softKnee.Fill(layer.softKnee);
                clamp.Fill(layer.clamp);
                diffusion.Fill(layer.diffusion);
                anamorphicRatio.Fill(layer.anamorphicRatio);
                color.Fill(layer.color);
                fastMode.Fill(layer.fastMode);
                dirtIntensity.Fill(layer.dirtIntensity);
                layer.dirtTexture.overrideState = dirtState;
                Graphics.Instance.PostProcessingManager.LoadLensDirtTexture(dirtTexture, dirtTexture => layer.dirtTexture.value = dirtTexture);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct ChromaticAberrationParams
    {
        public BoolValue enabled;
        public FloatValue intensity;
        public BoolValue fastMode;

        public string spectralLut;
        public void Save(UnityEngine.Rendering.PostProcessing.ChromaticAberration layer, string spectralLutPath = "")
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                intensity = new FloatValue(layer.intensity);
                fastMode = new BoolValue(layer.fastMode);
                //Save Texture path.
                //spectralLut = spectralLutPath;
            }
        }
        public void Load(UnityEngine.Rendering.PostProcessing.ChromaticAberration layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                intensity.Fill(layer.intensity);
                fastMode.Fill(layer.fastMode);

                //Load texture from the path.
                //layer.spectralLutPath.value = spectralLut;
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct ColorGradingParams
    {
        //internal SplineParameter redCurve; // Figure out messagepack parser later.
        //internal SplineParameter greenCurve; // Figure out messagepack parser later.
        //internal SplineParameter blueCurve; // Figure out messagepack parser later.
        //internal SplineParameter hueVsHueCurve; // Figure out messagepack parser later.
        //internal SplineParameter hueVsSatCurve; // Figure out messagepack parser later.
        //internal SplineParameter satVsSatCurve; // Figure out messagepack parser later.
        //internal SplineParameter lumVsSatCurve; // Figure out messagepack parser later.
        //internal SplineParameter masterCurve; // Figure out messagepack parser later.

        public BoolValue enabled;
        public GradingModeValue gradingMode;
        public FloatValue mixerGreenOutGreenIn;
        public FloatValue mixerGreenOutBlueIn;
        public FloatValue mixerBlueOutRedIn;
        public FloatValue mixerBlueOutGreenIn;
        public FloatValue mixerBlueOutBlueIn;
        public Vector4Value lift;
        public Vector4Value gamma;
        public FloatValue mixerGreenOutRedIn;
        public Vector4Value gain;
        public FloatValue mixerRedOutBlueIn;
        public FloatValue mixerRedOutGreenIn;
        public FloatValue toneCurveToeStrength;
        public FloatValue toneCurveToeLength;
        public FloatValue toneCurveShoulderStrength;
        public FloatValue toneCurveShoulderLength;
        public FloatValue toneCurveShoulderAngle;
        public FloatValue toneCurveGamma;
        public FloatValue mixerRedOutRedIn;
        public TonemapperValue tonemapper;
        public FloatValue ldrLutContribution;
        public FloatValue tint;
        public ColorValue colorFilter;
        public FloatValue hueShift;
        public FloatValue saturation;
        public FloatValue brightness;
        public FloatValue postExposure;
        public FloatValue contrast;
        public FloatValue temperature;
        public IntValue ldrLutIndex;
        public IntValue externalLutIndex;
        public string externalLutPath; // Formerly Texture.

        public void Save(UnityEngine.Rendering.PostProcessing.ColorGrading layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                gradingMode = new GradingModeValue(layer.gradingMode);
                mixerGreenOutGreenIn = new FloatValue(layer.mixerGreenOutGreenIn);
                mixerGreenOutBlueIn = new FloatValue(layer.mixerGreenOutBlueIn);
                mixerBlueOutRedIn = new FloatValue(layer.mixerBlueOutRedIn);
                mixerBlueOutGreenIn = new FloatValue(layer.mixerBlueOutGreenIn);
                mixerBlueOutBlueIn = new FloatValue(layer.mixerBlueOutBlueIn);
                lift = new Vector4Value(layer.lift);
                gamma = new Vector4Value(layer.gamma);
                mixerGreenOutRedIn = new FloatValue(layer.mixerGreenOutRedIn);
                gain = new Vector4Value(layer.gain);
                mixerRedOutBlueIn = new FloatValue(layer.mixerRedOutBlueIn);
                mixerRedOutGreenIn = new FloatValue(layer.mixerRedOutGreenIn);
                toneCurveToeStrength = new FloatValue(layer.toneCurveToeStrength);
                toneCurveToeLength = new FloatValue(layer.toneCurveToeLength);
                toneCurveShoulderStrength = new FloatValue(layer.toneCurveShoulderStrength);
                toneCurveShoulderLength = new FloatValue(layer.toneCurveShoulderLength);
                toneCurveShoulderAngle = new FloatValue(layer.toneCurveShoulderAngle);
                toneCurveGamma = new FloatValue(layer.toneCurveGamma);
                mixerRedOutRedIn = new FloatValue(layer.mixerRedOutRedIn);
                tonemapper = new TonemapperValue(layer.tonemapper);
                ldrLutContribution = new FloatValue(layer.ldrLutContribution);
                tint = new FloatValue(layer.tint);
                colorFilter = new ColorValue(layer.colorFilter);
                hueShift = new FloatValue(layer.hueShift);
                saturation = new FloatValue(layer.saturation);
                brightness = new FloatValue(layer.brightness);
                postExposure = new FloatValue(layer.postExposure);
                contrast = new FloatValue(layer.contrast);
                temperature = new FloatValue(layer.temperature);                
                ldrLutIndex = new IntValue(Graphics.Instance.PostProcessingManager.CurrentLUTIndex, layer.ldrLut.overrideState);
                externalLutIndex = new IntValue(Graphics.Instance.PostProcessingManager.Current3DLUTIndex, layer.externalLut.overrideState);
            }
        }
        public void Load(UnityEngine.Rendering.PostProcessing.ColorGrading layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                gradingMode.Fill(layer.gradingMode);
                mixerGreenOutGreenIn.Fill(layer.mixerGreenOutGreenIn);
                mixerGreenOutBlueIn.Fill(layer.mixerGreenOutBlueIn);
                mixerBlueOutRedIn.Fill(layer.mixerBlueOutRedIn);
                mixerBlueOutGreenIn.Fill(layer.mixerBlueOutGreenIn);
                mixerBlueOutBlueIn.Fill(layer.mixerBlueOutBlueIn);
                lift.Fill(layer.lift);
                gamma.Fill(layer.gamma);
                mixerGreenOutRedIn.Fill(layer.mixerGreenOutRedIn);
                gain.Fill(layer.gain);
                mixerRedOutBlueIn.Fill(layer.mixerRedOutBlueIn);
                mixerRedOutGreenIn.Fill(layer.mixerRedOutGreenIn);
                toneCurveToeStrength.Fill(layer.toneCurveToeStrength);
                toneCurveToeLength.Fill(layer.toneCurveToeLength);
                toneCurveShoulderStrength.Fill(layer.toneCurveShoulderStrength);
                toneCurveShoulderLength.Fill(layer.toneCurveShoulderLength);
                toneCurveShoulderAngle.Fill(layer.toneCurveShoulderAngle);
                toneCurveGamma.Fill(layer.toneCurveGamma);
                mixerRedOutRedIn.Fill(layer.mixerRedOutRedIn);
                tonemapper.Fill(layer.tonemapper);
                ldrLutContribution.Fill(layer.ldrLutContribution);
                tint.Fill(layer.tint);
                colorFilter.Fill(layer.colorFilter);
                hueShift.Fill(layer.hueShift);
                saturation.Fill(layer.saturation);
                brightness.Fill(layer.brightness);
                postExposure.Fill(layer.postExposure);
                contrast.Fill(layer.contrast);
                temperature.Fill(layer.temperature);
                Graphics.Instance.StartCoroutine(WaitForLut(layer));
            }
        }
        // If a LUT is set as a Studio default, then this might run before LUTs are loaded, so we wait until they've loaded.
        public System.Collections.IEnumerator WaitForLut(UnityEngine.Rendering.PostProcessing.ColorGrading layer)
        {
            while (!Graphics.Instance.PostProcessingManager.LUTReady())
                yield return null;
            layer.ldrLut.value = Graphics.Instance.PostProcessingManager.LoadLUT(ldrLutIndex.value);
            layer.ldrLut.overrideState = ldrLutIndex.overrideState;
            layer.externalLut.value = Graphics.Instance.PostProcessingManager.Load3DLUT(externalLutIndex.value);
            layer.externalLut.overrideState = externalLutIndex.overrideState;
            yield break;
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct DepthOfFieldParams
    {
        public BoolValue enabled;
        public float focusSpeed;
        public FloatValue focusDistance;
        public FloatValue aperture;
        public FloatValue focalLength;
        public KernelSizeValue kernelSize;
        public bool focusPuller;

        public void Save(UnityEngine.Rendering.PostProcessing.DepthOfField layer)
        {
            if (layer != null)
            {
                focusPuller = PostProcessingInspector.AutoFocusEnabled;
                focusSpeed = PostProcessingInspector.GetAutoFocusSpeedFromGameSession(); 
                enabled = new BoolValue(layer.enabled);
                focusDistance = new FloatValue(layer.focusDistance);
                aperture = new FloatValue(layer.aperture);
                focalLength = new FloatValue(layer.focalLength);
                kernelSize = new KernelSizeValue(layer.kernelSize);
            }
        }

        public void Load(UnityEngine.Rendering.PostProcessing.DepthOfField layer)
        {
            if (layer != null)
            {
                PostProcessingInspector.AutoFocusEnabled = focusPuller;
                PostProcessingInspector.SetAutoFocusSpeedToGameSession(focusSpeed);
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                focusDistance.Fill(layer.focusDistance);
                aperture.Fill(layer.aperture);
                focalLength.Fill(layer.focalLength);
                kernelSize.Fill(layer.kernelSize);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct GrainLayerParams
    {
        public BoolValue enabled;
        public BoolValue colored;
        public FloatValue intensity;
        public FloatValue size;
        public FloatValue lumContrib;

        public void Save(UnityEngine.Rendering.PostProcessing.Grain layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                colored = new BoolValue(layer.colored);
                intensity = new FloatValue(layer.intensity);
                size = new FloatValue(layer.size);
                lumContrib = new FloatValue(layer.lumContrib);
            }
        }
        public void Load(UnityEngine.Rendering.PostProcessing.Grain layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                colored.Fill(layer.colored);
                intensity.Fill(layer.intensity);
                size.Fill(layer.size);
                lumContrib.Fill(layer.lumContrib);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct ScreenSpaceReflectionParams
    {
        public BoolValue enabled;
        public ScreenSpaceReflectionPresetValue preset;
        public IntValue maximumIterationCount;
        public ScreenSpaceReflectionResolutionValue resolution;
        public FloatValue thickness;
        public FloatValue maximumMarchDistance;
        public FloatValue distanceFade;
        public FloatValue vignette;

        public void Save(UnityEngine.Rendering.PostProcessing.ScreenSpaceReflections layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                preset = new ScreenSpaceReflectionPresetValue(layer.preset);
                maximumIterationCount = new IntValue(layer.maximumIterationCount);
                resolution = new ScreenSpaceReflectionResolutionValue(layer.resolution);
                thickness = new FloatValue(layer.thickness);
                maximumMarchDistance = new FloatValue(layer.maximumMarchDistance);
                distanceFade = new FloatValue(layer.distanceFade);
                vignette = new FloatValue(layer.vignette);
            }
        }
        public void Load(UnityEngine.Rendering.PostProcessing.ScreenSpaceReflections layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                preset.Fill(layer.preset);
                maximumIterationCount.Fill(layer.maximumIterationCount);
                resolution.Fill(layer.resolution);
                thickness.Fill(layer.thickness);
                maximumMarchDistance.Fill(layer.maximumMarchDistance);
                distanceFade.Fill(layer.distanceFade);
                vignette.Fill(layer.vignette);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct VignetteParams
    {
        public BoolValue enabled;
        public VignetteModeValue mode;
        public ColorValue color; //vector3
        public Vector2Value center; //vector2
        public FloatValue intensity;
        public FloatValue smoothness;
        public FloatValue roundness;
        public BoolValue rounded;
        public FloatValue opacity;
        public string mask; //Mask Texture

        public void Save(UnityEngine.Rendering.PostProcessing.Vignette layer, string maskPath = "")
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                mode = new VignetteModeValue(layer.mode);
                color = new ColorValue(layer.color);
                center = new Vector2Value(layer.center);
                intensity = new FloatValue(layer.intensity);
                smoothness = new FloatValue(layer.smoothness);
                roundness = new FloatValue(layer.roundness);
                rounded = new BoolValue(layer.rounded);
                opacity = new FloatValue(layer.opacity);

                //Save path from the post process object?
                //mask = maskPath;
            }
        }
        public void Load(UnityEngine.Rendering.PostProcessing.Vignette layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                mode.Fill(layer.mode);
                color.Fill(layer.color);
                center.Fill(layer.center);
                intensity.Fill(layer.intensity);
                smoothness.Fill(layer.smoothness);
                roundness.Fill(layer.roundness);
                rounded.Fill(layer.rounded);
                opacity.Fill(layer.opacity);

                // Load Texture from the Path.
                // layer.mask.value = mask;
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct MotionBlurParams
    {
        public BoolValue enabled;
        public FloatValue shutterAngle;
        public IntValue sampleCount;

        public void Save(UnityEngine.Rendering.PostProcessing.MotionBlur layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                shutterAngle = new FloatValue(layer.shutterAngle);
                sampleCount = new IntValue(layer.sampleCount);
            }
        }
        public void Load(UnityEngine.Rendering.PostProcessing.MotionBlur layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                shutterAngle.Fill(layer.shutterAngle);
                sampleCount.Fill(layer.sampleCount);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public struct LensDistortionParams
    {
        public BoolValue enabled;
        public FloatValue intensity;
        public FloatValue intensityX;
        public FloatValue intensityY;
        public FloatValue centerX;
        public FloatValue centerY;
        public FloatValue scale;

        public void Save(UnityEngine.Rendering.PostProcessing.LensDistortion layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                intensity = new FloatValue(layer.intensity);
                intensityX = new FloatValue(layer.intensityX);
                intensityY = new FloatValue(layer.intensityY);
                centerX = new FloatValue(layer.centerX);
                centerY = new FloatValue(layer.centerY);
                scale = new FloatValue(layer.scale);
            }
        }
        public void Load(UnityEngine.Rendering.PostProcessing.LensDistortion layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                intensity.Fill(layer.intensity);
                intensityX.Fill(layer.intensityX);
                intensityY.Fill(layer.intensityY);
                centerX.Fill(layer.centerX);
                centerY.Fill(layer.centerY);
                scale.Fill(layer.scale);
            }
        }
    }

    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct TiltShiftBokehParams
    //{
    //    public BoolValue enabled;
    //    public BoolValue preview;
    //    public FloatValue offset;
    //    public FloatValue area;
    //    public FloatValue spread;
    //    public IntValue samples;
    //    public FloatValue radius;
    //    public BoolValue useDistortion;
    //    public FloatValue cubicDistortion;
    //    public FloatValue distortionScale;

    //    public void Save(TiltShiftBokeh layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            preview = new BoolValue(layer.preview);
    //            offset = new FloatValue(layer.offset);
    //            area = new FloatValue(layer.area);
    //            spread = new FloatValue(layer.spread);
    //            samples = new IntValue(layer.samples);
    //            radius = new FloatValue(layer.radius);
    //            useDistortion = new BoolValue(layer.useDistortion);
    //            cubicDistortion = new FloatValue(layer.cubicDistortion);
    //            distortionScale = new FloatValue(layer.distortionScale);
    //        }
    //    }
    //    public void Load(TiltShiftBokeh layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            preview.Fill(layer.preview);
    //            offset.Fill(layer.offset);
    //            if (area.value == 0)
    //                area.value = 1f;
    //            area.Fill(layer.area);
    //            if (spread.value == 0)
    //                spread.value = 1f;
    //            spread.Fill(layer.spread);
    //            if (samples.value == 0)
    //                samples.value = 32;
    //            samples.Fill(layer.samples);
    //            if (radius.value == 0)
    //                radius.value = 1f;
    //            radius.Fill(layer.radius);
    //            useDistortion.Fill(layer.useDistortion);
    //            if (cubicDistortion.value == 0)
    //                cubicDistortion.value = 5f;
    //            cubicDistortion.Fill(layer.cubicDistortion);
    //            if (distortionScale.value == 0)
    //                distortionScale.value = 1f;
    //            distortionScale.Fill(layer.distortionScale);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct SunShaftsHDRParams
    //{
    //    public BoolValue enabled;
    //    public FloatValue blend;
    //    //public SunShaftsResolution resolution;
    //    //public ShaftsScreenBlendMode screenBlendMode;
    //    public Vector3Value sunTransform;
    //    public IntValue radialBlurIterations;
    //    public ColorValue sunColor;
    //    public ColorValue sunThreshold;
    //    public FloatValue sunShaftBlurRadius;
    //    public FloatValue sunShaftIntensity;
    //    public FloatValue maxRadius;
    //    public BoolValue useDepthTexture;
    //    public BoolValue connectSun;

    //    public void Save(SunShaftsHDR layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            blend = new FloatValue(layer.blend);
    //            sunTransform = new Vector3Value(layer.sunTransform);
    //            radialBlurIterations = new IntValue(layer.radialBlurIterations);
    //            sunColor = new ColorValue(layer.sunColor);
    //            sunThreshold = new ColorValue(layer.sunThreshold);
    //            sunShaftBlurRadius = new FloatValue(layer.sunShaftBlurRadius);
    //            sunShaftIntensity = new FloatValue(layer.sunShaftIntensity);
    //            maxRadius = new FloatValue(layer.maxRadius);
    //            useDepthTexture = new BoolValue(layer.useDepthTexture);
    //            connectSun = new BoolValue(layer.connectSun);
    //        }
    //    }
    //    public void Load(SunShaftsHDR layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            blend.Fill(layer.blend);
    //            if (sunTransform.value == null)
    //                sunTransform.value = new float[] { 0f, 0f, 0f };
    //            sunTransform.Fill(layer.sunTransform);
    //            if (radialBlurIterations.value == 0)
    //                radialBlurIterations.value = 2;
    //            radialBlurIterations.Fill(layer.radialBlurIterations);
    //            if (sunColor.value == null)
    //                sunColor.value = new float[] { 1f, 1f, 1f };
    //            sunColor.Fill(layer.sunColor);
    //            if (sunThreshold.value == null)
    //                sunThreshold.value = new float[] { 0.87f, 0.74f, 0.65f };
    //            sunThreshold.Fill(layer.sunThreshold);
    //            if (sunShaftBlurRadius.value == 0)
    //                sunShaftBlurRadius.value = 2.5f;
    //            sunShaftBlurRadius.Fill(layer.sunShaftBlurRadius);
    //            if (sunShaftIntensity.value == 0)
    //                sunShaftIntensity.value = 0.5f;
    //            sunShaftIntensity.Fill(layer.sunShaftIntensity);
    //            if (maxRadius.value == 0)
    //                maxRadius.value = 0.75f;
    //            maxRadius.Fill(layer.maxRadius);
    //            useDepthTexture.Fill(layer.useDepthTexture);
    //            connectSun.Fill(layer.connectSun);
    //        }
    //    }
    //}

    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct CRTTubeParams
    //{

    //    public BoolValue enabled;
    //    public FloatValue bleeding;
    //    public FloatValue fringing;
    //    public FloatValue scanline;


    //    public void Save(CRTTube layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            bleeding = new FloatValue(layer.bleeding);
    //            fringing = new FloatValue(layer.fringing);
    //            scanline = new FloatValue(layer.scanline);
    //        }
    //    }
    //    public void Load(CRTTube layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (bleeding.value == 0)
    //                bleeding.value = 0.5f;
    //            bleeding.Fill(layer.bleeding);
    //            if (fringing.value == 0)
    //                fringing.value = 0.5f;
    //            fringing.Fill(layer.fringing);
    //            if (scanline.value == 0)
    //                scanline.value = 0.5f;
    //            scanline.Fill(layer.scanline);
    //        }
    //    }
    //}

    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct PixelizeQuadParams
    //{

    //    public BoolValue enabled;
    //    public FloatValue pixelSize;
    //    public BoolValue useAutoScreenRatio;
    //    public FloatValue pixelRatio;
    //    public FloatValue pixelScaleX;
    //    public FloatValue pixelScaleY;

    //    public void Save(PixelizeQuad layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            pixelSize = new FloatValue(layer.pixelSize);
    //            useAutoScreenRatio = new BoolValue(layer.useAutoScreenRatio);
    //            pixelRatio = new FloatValue(layer.pixelRatio);
    //            pixelScaleX = new FloatValue(layer.pixelScaleX);
    //            pixelScaleY = new FloatValue(layer.pixelScaleY);
    //        }
    //    }
    //    public void Load(PixelizeQuad layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (pixelSize.value == 0)
    //                pixelSize.value = 0.01f;
    //            pixelSize.Fill(layer.pixelSize);
    //            useAutoScreenRatio.Fill(layer.useAutoScreenRatio);
    //            if (pixelRatio.value == 0)
    //                pixelRatio.value = 1f;
    //            pixelRatio.Fill(layer.pixelRatio);
    //            if (pixelScaleX.value == 0)
    //                pixelScaleX.value = 1f;
    //            pixelScaleX.Fill(layer.pixelScaleX);
    //            if (pixelScaleY.value == 0)
    //                pixelScaleY.value = 1f;
    //            pixelScaleY.Fill(layer.pixelScaleY);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct PixelizeLedParams
    //{

    //    public BoolValue enabled;
    //    public FloatValue pixelSize;
    //    public BoolValue useAutoScreenRatio;
    //    public FloatValue ledRadius;
    //    public FloatValue pixelRatio;
    //    public ColorValue BackgroundColor;

    //    public void Save(PixelizeLed layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            pixelSize = new FloatValue(layer.pixelSize);
    //            useAutoScreenRatio = new BoolValue(layer.useAutoScreenRatio);
    //            ledRadius = new FloatValue(layer.ledRadius);
    //            pixelRatio = new FloatValue(layer.pixelRatio);
    //            BackgroundColor = new ColorValue(layer.BackgroundColor);

    //        }
    //    }
    //    public void Load(PixelizeLed layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (pixelSize.value == 0)
    //                pixelSize.value = 0.5f;
    //            pixelSize.Fill(layer.pixelSize);
    //            useAutoScreenRatio.Fill(layer.useAutoScreenRatio);
    //            if (ledRadius.value == 0)
    //                ledRadius.value = 1.0f;
    //            ledRadius.Fill(layer.ledRadius);
    //            if (pixelRatio.value == 0)
    //                pixelRatio.value = 1f;
    //            pixelRatio.Fill(layer.pixelRatio);
    //            if (BackgroundColor.value == null)
    //                BackgroundColor.value = new float[] { 0f, 0f, 0f };
    //            BackgroundColor.Fill(layer.BackgroundColor);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct PixelizeHexagonParams
    //{

    //    public BoolValue enabled;
    //    public FloatValue pixelSize;
    //    public BoolValue useAutoScreenRatio;
    //    public FloatValue pixelRatio;
    //    public FloatValue pixelScaleX;
    //    public FloatValue pixelScaleY;
    //    public FloatValue gridWidth;

    //    public void Save(PixelizeHexagon layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            pixelSize = new FloatValue(layer.pixelSize);
    //            useAutoScreenRatio = new BoolValue(layer.useAutoScreenRatio);
    //            pixelRatio = new FloatValue(layer.pixelRatio);
    //            pixelScaleX = new FloatValue(layer.pixelScaleX);
    //            pixelScaleY = new FloatValue(layer.pixelScaleY);
    //            gridWidth = new FloatValue(layer.gridWidth);

    //        }
    //    }
    //    public void Load(PixelizeHexagon layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (pixelSize.value == 0)
    //                pixelSize.value = 0.05f;
    //            pixelSize.Fill(layer.pixelSize);
    //            useAutoScreenRatio.Fill(layer.useAutoScreenRatio);
    //            if (pixelRatio.value == 0)
    //                pixelRatio.value = 1f;
    //            pixelRatio.Fill(layer.pixelRatio);
    //            if (pixelScaleX.value == 0)
    //                pixelScaleX.value = 1f;
    //            pixelScaleX.Fill(layer.pixelScaleX);
    //            if (pixelScaleY.value == 0)
    //                pixelScaleY.value = 1f;
    //            pixelScaleY.Fill(layer.pixelScaleY);
    //            if (gridWidth.value == 0)
    //                gridWidth.value = 0.1f;
    //            gridWidth.Fill(layer.gridWidth);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchRGBSplitV5Params
    //{
    //    public BoolValue enabled;
    //    public FloatValue Amplitude;
    //    public FloatValue Speed;

    //    public void Save(GlitchRGBSplitV5 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            Amplitude = new FloatValue(layer.Amplitude);
    //            Speed = new FloatValue(layer.Speed);
    //        }
    //    }
    //    public void Load(GlitchRGBSplitV5 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (Amplitude.value == 0)
    //                Amplitude.value = 3.0f;
    //            Amplitude.Fill(layer.Amplitude);
    //            if (Speed.value == 0)
    //                Speed.value = 0.1f;
    //            Speed.Fill(layer.Speed);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchRGBSplitV4Params
    //{
    //    public BoolValue enabled;
    //    public FloatValue intensity;
    //    public FloatValue speed;
    //    public SplitDirectionValue SplitDirection;

    //    public void Save(GlitchRGBSplitV4 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            intensity = new FloatValue(layer.intensity);
    //            speed = new FloatValue(layer.speed);
    //            SplitDirection = new SplitDirectionValue(layer.SplitDirection);
    //        }
    //    }
    //    public void Load(GlitchRGBSplitV4 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (intensity.value == 0)
    //                intensity.value = 3.0f;
    //            intensity.Fill(layer.intensity);
    //            if (speed.value == 0)
    //                speed.value = 0.5f;
    //            speed.Fill(layer.speed);
    //            SplitDirection.Fill(layer.SplitDirection);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchRGBSplitV3Params
    //{
    //    public BoolValue enabled;
    //    public FloatValue Frequency;
    //    public FloatValue Speed;
    //    public FloatValue Amount;
    //    public SplitDirectionValue SplitDirection;
    //    public IntervalTypeValue intervalType;

    //    public void Save(GlitchRGBSplitV3 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            Frequency = new FloatValue(layer.Frequency);
    //            Speed = new FloatValue(layer.Speed);
    //            Amount = new FloatValue(layer.Amount);
    //            SplitDirection = new SplitDirectionValue(layer.SplitDirection);
    //            intervalType = new IntervalTypeValue(layer.intervalType);
    //        }
    //    }
    //    public void Load(GlitchRGBSplitV3 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (Frequency.value == 0)
    //                Frequency.value = 3.0f;
    //            Frequency.Fill(layer.Frequency);
    //            if (Speed.value == 0)
    //                Speed.value = 20.0f;
    //            Speed.Fill(layer.Speed);
    //            if (Amount.value == 0)
    //                Amount.value = 30.0f;
    //            Amount.Fill(layer.Amount);
    //            SplitDirection.Fill(layer.SplitDirection);
    //            intervalType.Fill(layer.intervalType);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchRGBSplitV2Params
    //{
    //    public BoolValue enabled;
    //    public FloatValue Amount;
    //    public FloatValue Amplitude;
    //    public FloatValue Speed;
    //    public SplitDirectionValue SplitDirection;

    //    public void Save(GlitchRGBSplitV2 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            SplitDirection = new SplitDirectionValue(layer.SplitDirection);
    //            Amount = new FloatValue(layer.Amount);
    //            Amplitude = new FloatValue(layer.Amplitude);
    //            Speed = new FloatValue(layer.Speed);
    //        }
    //    }
    //    public void Load(GlitchRGBSplitV2 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            SplitDirection.Fill(layer.SplitDirection);
    //            if (Amount.value == 0)
    //                Amount.value = 5.0f;
    //            Amount.Fill(layer.Amount);
    //            if (Amplitude.value == 0)
    //                Amplitude.value = 3.0f;
    //            Amplitude.Fill(layer.Amplitude);
    //            if (Speed.value == 0)
    //                Speed.value = 1.0f;
    //            Speed.Fill(layer.Speed);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchRGBSplitParams
    //{
    //    public BoolValue enabled;
    //    public FloatValue Fading;
    //    public FloatValue Amount;
    //    public FloatValue Speed;
    //    public FloatValue CenterFading;
    //    public FloatValue AmountR;
    //    public FloatValue AmountB;

    //    public SplitDirectionValue SplitDirection;

    //    public void Save(GlitchRGBSplit layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            SplitDirection = new SplitDirectionValue(layer.SplitDirection);
    //            Fading = new FloatValue(layer.Fading);
    //            Amount = new FloatValue(layer.Amount);
    //            Speed = new FloatValue(layer.Speed);
    //            CenterFading = new FloatValue(layer.CenterFading);
    //            AmountR = new FloatValue(layer.AmountR);
    //            AmountB = new FloatValue(layer.AmountB);
    //        }
    //    }
    //    public void Load(GlitchRGBSplit layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            SplitDirection.Fill(layer.SplitDirection);
    //            if (Fading.value == 0)
    //                Fading.value = 1.0f;
    //            Fading.Fill(layer.Fading);
    //            if (Amount.value == 0)
    //                Amount.value = 1.0f;
    //            Amount.Fill(layer.Amount);
    //            if (Speed.value == 0)
    //                Speed.value = 1.0f;
    //            Speed.Fill(layer.Speed);
    //            if (CenterFading.value == 0)
    //                CenterFading.value = 1.0f;
    //            CenterFading.Fill(layer.CenterFading);
    //            if (AmountR.value == 0)
    //                AmountR.value = 1.0f;
    //            AmountR.Fill(layer.AmountR);
    //            if (AmountB.value == 0)
    //                AmountB.value = 1.0f;
    //            AmountB.Fill(layer.AmountB);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchImageBlockParams
    //{
    //    public BoolValue enabled;
    //    public BoolValue active;
    //    public FloatValue Fade;
    //    public FloatValue Speed;
    //    public FloatValue Amount;
    //    public FloatValue BlockLayer1_U;
    //    public FloatValue BlockLayer1_V;
    //    public FloatValue BlockLayer2_U;
    //    public FloatValue BlockLayer2_V;
    //    public FloatValue BlockLayer1_Indensity;
    //    public FloatValue BlockLayer2_Indensity;
    //    public FloatValue RGBSplitIndensity;
    //    public BoolValue BlockVisualizeDebug;

    //    public void Save(GlitchImageBlock layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            Fade = new FloatValue(layer.Fade);
    //            Speed = new FloatValue(layer.Speed);
    //            Amount = new FloatValue(layer.Amount);
    //            BlockLayer1_U = new FloatValue(layer.BlockLayer1_U);
    //            BlockLayer1_V = new FloatValue(layer.BlockLayer1_V);
    //            BlockLayer2_U = new FloatValue(layer.BlockLayer2_U);
    //            BlockLayer2_V = new FloatValue(layer.BlockLayer2_V);
    //            BlockLayer1_Indensity = new FloatValue(layer.BlockLayer1_Indensity);
    //            BlockLayer2_Indensity = new FloatValue(layer.BlockLayer2_Indensity);
    //            RGBSplitIndensity = new FloatValue(layer.RGBSplitIndensity);
    //            BlockVisualizeDebug = new BoolValue(layer.BlockVisualizeDebug);
    //        }
    //    }
    //    public void Load(GlitchImageBlock layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (Fade.value == 0)
    //                Fade.value = 1.0f;
    //            Fade.Fill(layer.Fade);
    //            if (Speed.value == 0)
    //                Speed.value = 0.5f;
    //            Speed.Fill(layer.Speed);
    //            if (Amount.value == 0)
    //                Amount.value = 1.0f;
    //            Amount.Fill(layer.Amount);
    //            if (BlockLayer1_U.value == 0)
    //                BlockLayer1_U.value = 9.0f;
    //            BlockLayer1_U.Fill(layer.BlockLayer1_U);
    //            if (BlockLayer1_V.value == 0)
    //                BlockLayer1_V.value = 9.0f;
    //            BlockLayer1_V.Fill(layer.BlockLayer1_V);
    //            if (BlockLayer2_U.value == 0)
    //                BlockLayer2_U.value = 5.0f;
    //            BlockLayer2_U.Fill(layer.BlockLayer2_U);
    //            if (BlockLayer2_V.value == 0)
    //                BlockLayer2_V.value = 5.0f;
    //            BlockLayer2_V.Fill(layer.BlockLayer2_V);
    //            if (BlockLayer1_Indensity.value == 0)
    //                BlockLayer1_Indensity.value = 8.0f;
    //            BlockLayer1_Indensity.Fill(layer.BlockLayer1_Indensity);
    //            if (BlockLayer2_Indensity.value == 0)
    //                BlockLayer2_Indensity.value = 4.0f;
    //            BlockLayer2_Indensity.Fill(layer.BlockLayer2_Indensity);
    //            if (RGBSplitIndensity.value == 0)
    //                RGBSplitIndensity.value = 0.5f;
    //            RGBSplitIndensity.Fill(layer.RGBSplitIndensity);
    //            BlockVisualizeDebug.Fill(layer.BlockVisualizeDebug);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchImageBlockV2Params
    //{
    //    public BoolValue enabled;
    //    public FloatValue Fade;
    //    public FloatValue Speed;
    //    public FloatValue Amount;
    //    public FloatValue BlockLayer1_U;
    //    public FloatValue BlockLayer1_V;
    //    public FloatValue BlockLayer1_Indensity;
    //    public FloatValue RGBSplitIndensity;
    //    public BoolValue BlockVisualizeDebug;

    //    public void Save(GlitchImageBlockV2 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            Fade = new FloatValue(layer.Fade);
    //            Speed = new FloatValue(layer.Speed);
    //            Amount = new FloatValue(layer.Amount);
    //            BlockLayer1_U = new FloatValue(layer.BlockLayer1_U);
    //            BlockLayer1_V = new FloatValue(layer.BlockLayer1_V);
    //            BlockLayer1_Indensity = new FloatValue(layer.BlockLayer1_Indensity);
    //            RGBSplitIndensity = new FloatValue(layer.RGBSplitIndensity);
    //            BlockVisualizeDebug = new BoolValue(layer.BlockVisualizeDebug);
    //        }
    //    }
    //    public void Load(GlitchImageBlockV2 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (Fade.value == 0)
    //                Fade.value = 1.0f;
    //            Fade.Fill(layer.Fade);
    //            if (Speed.value == 0)
    //                Speed.value = 0.5f;
    //            Speed.Fill(layer.Speed);
    //            if (Amount.value == 0)
    //                Amount.value = 1.0f;
    //            Amount.Fill(layer.Amount);
    //            if (BlockLayer1_U.value == 0)
    //                BlockLayer1_U.value = 2.0f;
    //            BlockLayer1_U.Fill(layer.BlockLayer1_U);
    //            if (BlockLayer1_V.value == 0)
    //                BlockLayer1_V.value = 16.0f;
    //            BlockLayer1_V.Fill(layer.BlockLayer1_V);
    //            if (BlockLayer1_Indensity.value == 0)
    //                BlockLayer1_Indensity.value = 8.0f;
    //            BlockLayer1_Indensity.Fill(layer.BlockLayer1_Indensity);
    //            if (RGBSplitIndensity.value == 0)
    //                RGBSplitIndensity.value = 2.0f;
    //            RGBSplitIndensity.Fill(layer.RGBSplitIndensity);
    //            BlockVisualizeDebug.Fill(layer.BlockVisualizeDebug);
    //        }
    //    }
    //}
    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchImageBlockV3Params
    //{
    //    public BoolValue enabled;
    //    public FloatValue Speed;
    //    public FloatValue BlockSize;


    //    public void Save(GlitchImageBlockV3 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            Speed = new FloatValue(layer.Speed);
    //            BlockSize = new FloatValue(layer.BlockSize);

    //        }
    //    }
    //    public void Load(GlitchImageBlockV3 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (Speed.value == 0)
    //                Speed.value = 10.0f;
    //            Speed.Fill(layer.Speed);

    //        }
    //    }
    //}

    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct GlitchImageBlockV4Params
    //{
    //    public BoolValue enabled;
    //    public FloatValue Speed;
    //    public FloatValue BlockSize;
    //    public FloatValue MaxRGBSplitX;
    //    public FloatValue MaxRGBSplitY;

    //    public void Save(GlitchImageBlockV4 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            Speed = new FloatValue(layer.Speed);
    //            BlockSize = new FloatValue(layer.BlockSize);
    //            MaxRGBSplitX = new FloatValue(layer.MaxRGBSplitX);
    //            MaxRGBSplitY = new FloatValue(layer.MaxRGBSplitY);
    //        }
    //    }

    //    public void Load(GlitchImageBlockV4 layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (Speed.value == 0)
    //                Speed.value = 10.0f;
    //            Speed.Fill(layer.Speed);
    //            if (BlockSize.value == 0)
    //                BlockSize.value = 8.0f;
    //            BlockSize.Fill(layer.BlockSize);
    //            if (MaxRGBSplitX.value == 0)
    //                MaxRGBSplitX.value = 2.0f;
    //            MaxRGBSplitX.Fill(layer.MaxRGBSplitX);
    //            if (MaxRGBSplitY.value == 0)
    //                MaxRGBSplitY.value = 2.0f;
    //            MaxRGBSplitY.Fill(layer.MaxRGBSplitY);

    //        }
    //    }
    //}

    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct BeautifyBloomParams
    //{
    //    public BoolValue enabled;
    //    public FloatValue bloomIntensity;
    //    public FloatValue bloomThreshold;
    //    public FloatValue bloomMaxBrightness;
    //    public FloatValue bloomDepthAtten;
    //    public BoolValue bloomAntiflicker;
    //    public BoolValue bloomUltra;
    //    public BoolValue bloomDebug;
    //    public BoolValue bloomCustomize;
    //    public FloatValue bloomWeight0;
    //    public FloatValue bloomWeight1;
    //    public FloatValue bloomWeight2;
    //    public FloatValue bloomWeight3;
    //    public FloatValue bloomWeight4;
    //    public FloatValue bloomWeight5;
    //    public FloatValue bloomBoost0;
    //    public FloatValue bloomBoost1;
    //    public FloatValue bloomBoost2;
    //    public FloatValue bloomBoost3;
    //    public FloatValue bloomBoost4;
    //    public FloatValue bloomBoost5;
    //    public FloatValue anamorphicFlaresIntensity;
    //    public FloatValue anamorphicFlaresThreshold;
    //    public BoolValue anamorphicFlaresVertical;
    //    public FloatValue anamorphicFlaresSpread;
    //    public FloatValue anamorphicFlaresDepthAtten;
    //    public BoolValue anamorphicFlaresAntiflicker;
    //    public BoolValue anamorphicFlaresUltra;
    //    public ColorValue anamorphicFlaresTint;
    //    public FloatValue sunFlaresIntensity;
    //    public ColorValue sunFlaresTint;
    //    public FloatValue sunFlaresSolarWindSpeed;
    //    public BoolValue sunFlaresRotationDeadZone;
    //    public IntValue sunFlaresDownsampling;
    //    //public BeautifyLayerMaskValue sunFlaresLayerMask;
    //    public FloatValue sunFlaresSunIntensity;
    //    public FloatValue sunFlaresSunDiskSize;
    //    public FloatValue sunFlaresSunRayDiffractionIntensity;
    //    public FloatValue sunFlaresSunRayDiffractionThreshold;
    //    public FloatValue sunFlaresCoronaRays1Length;
    //    public FloatValue sunFlaresCoronaRays1Streaks;
    //    public FloatValue sunFlaresCoronaRays1Spread;
    //    public FloatValue sunFlaresCoronaRays1AngleOffset;
    //    public FloatValue sunFlaresCoronaRays2Length;
    //    public FloatValue sunFlaresCoronaRays2Streaks;
    //    public FloatValue sunFlaresCoronaRays2Spread;
    //    public FloatValue sunFlaresCoronaRays2AngleOffset;
    //    public FloatValue sunFlaresGhosts1Size;
    //    public FloatValue sunFlaresGhosts1Offset;
    //    public FloatValue sunFlaresGhosts1Brightness;
    //    public FloatValue sunFlaresGhosts2Size;
    //    public FloatValue sunFlaresGhosts2Offset;
    //    public FloatValue sunFlaresGhosts2Brightness;
    //    public FloatValue sunFlaresGhosts3Size;
    //    public FloatValue sunFlaresGhosts3Brightness;
    //    public FloatValue sunFlaresGhosts3Offset;
    //    public FloatValue sunFlaresGhosts4Size;
    //    public FloatValue sunFlaresGhosts4Offset;
    //    public FloatValue sunFlaresGhosts4Brightness;
    //    public FloatValue sunFlaresHaloOffset;
    //    public FloatValue sunFlaresHaloAmplitude;
    //    public FloatValue sunFlaresHaloIntensity;

    //    public FloatValue lensDirtIntensity;
    //    public FloatValue lensDirtThreshold;
    //    public IntValue lensDirtSpread;
    //    public string lensDirtTexture;
    //    public bool lensDirtState;


    //    public void Save(BeautifyBloom layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            bloomIntensity = new FloatValue(layer.bloomIntensity);
    //            bloomThreshold = new FloatValue(layer.bloomThreshold);
    //            bloomMaxBrightness = new FloatValue(layer.bloomMaxBrightness);
    //            bloomDepthAtten = new FloatValue(layer.bloomDepthAtten);
    //            bloomAntiflicker = new BoolValue(layer.bloomAntiflicker);
    //            bloomUltra = new BoolValue(layer.bloomUltra);
    //            bloomDebug = new BoolValue(layer.bloomDebug);
    //            bloomCustomize = new BoolValue(layer.bloomCustomize);
    //            bloomWeight0 = new FloatValue(layer.bloomWeight0);
    //            bloomWeight1 = new FloatValue(layer.bloomWeight1);
    //            bloomWeight2 = new FloatValue(layer.bloomWeight2);
    //            bloomWeight3 = new FloatValue(layer.bloomWeight3);
    //            bloomWeight4 = new FloatValue(layer.bloomWeight4);
    //            bloomWeight5 = new FloatValue(layer.bloomWeight5);
    //            bloomBoost0 = new FloatValue(layer.bloomBoost0);
    //            bloomBoost1 = new FloatValue(layer.bloomBoost1);
    //            bloomBoost2 = new FloatValue(layer.bloomBoost2);
    //            bloomBoost3 = new FloatValue(layer.bloomBoost3);
    //            bloomBoost4 = new FloatValue(layer.bloomBoost4);
    //            bloomBoost5 = new FloatValue(layer.bloomBoost5);
    //            anamorphicFlaresIntensity = new FloatValue(layer.anamorphicFlaresIntensity);
    //            anamorphicFlaresThreshold = new FloatValue(layer.anamorphicFlaresThreshold);
    //            anamorphicFlaresVertical = new BoolValue(layer.anamorphicFlaresVertical);
    //            anamorphicFlaresSpread = new FloatValue(layer.anamorphicFlaresSpread);
    //            anamorphicFlaresDepthAtten = new FloatValue(layer.anamorphicFlaresDepthAtten);
    //            anamorphicFlaresAntiflicker = new BoolValue(layer.anamorphicFlaresAntiflicker);
    //            anamorphicFlaresUltra = new BoolValue(layer.anamorphicFlaresUltra);
    //            anamorphicFlaresTint = new ColorValue(layer.anamorphicFlaresTint);
    //            sunFlaresIntensity = new FloatValue(layer.sunFlaresIntensity);
    //            sunFlaresTint = new ColorValue(layer.sunFlaresTint);
    //            sunFlaresSolarWindSpeed = new FloatValue(layer.sunFlaresSolarWindSpeed);
    //            sunFlaresRotationDeadZone = new BoolValue(layer.sunFlaresRotationDeadZone);
    //            sunFlaresDownsampling = new IntValue(layer.sunFlaresDownsampling);
    //            //sunFlaresLayerMask = new BeautifyLayerMaskValue(layer.sunFlaresLayerMask);
    //            sunFlaresSunIntensity = new FloatValue(layer.sunFlaresSunIntensity);
    //            sunFlaresSunDiskSize = new FloatValue(layer.sunFlaresSunDiskSize);
    //            sunFlaresSunRayDiffractionIntensity = new FloatValue(layer.sunFlaresSunRayDiffractionIntensity);
    //            sunFlaresSunRayDiffractionThreshold = new FloatValue(layer.sunFlaresSunRayDiffractionThreshold);
    //            sunFlaresCoronaRays1Length = new FloatValue(layer.sunFlaresCoronaRays1Length);
    //            sunFlaresCoronaRays1Streaks = new FloatValue(layer.sunFlaresCoronaRays1Streaks);
    //            sunFlaresCoronaRays1Spread = new FloatValue(layer.sunFlaresCoronaRays1Spread);
    //            sunFlaresCoronaRays1AngleOffset = new FloatValue(layer.sunFlaresCoronaRays1AngleOffset);
    //            sunFlaresCoronaRays2Length = new FloatValue(layer.sunFlaresCoronaRays2Length);
    //            sunFlaresCoronaRays2Streaks = new FloatValue(layer.sunFlaresCoronaRays2Streaks);
    //            sunFlaresCoronaRays2Spread = new FloatValue(layer.sunFlaresCoronaRays2Spread);
    //            sunFlaresCoronaRays2AngleOffset = new FloatValue(layer.sunFlaresCoronaRays2AngleOffset);
    //            sunFlaresGhosts1Size = new FloatValue(layer.sunFlaresGhosts1Size);
    //            sunFlaresGhosts1Offset = new FloatValue(layer.sunFlaresGhosts1Offset);
    //            sunFlaresGhosts1Brightness = new FloatValue(layer.sunFlaresGhosts1Brightness);
    //            sunFlaresGhosts2Size = new FloatValue(layer.sunFlaresGhosts2Size);
    //            sunFlaresGhosts2Offset = new FloatValue(layer.sunFlaresGhosts2Offset);
    //            sunFlaresGhosts2Brightness = new FloatValue(layer.sunFlaresGhosts2Brightness);
    //            sunFlaresGhosts3Size = new FloatValue(layer.sunFlaresGhosts3Size);
    //            sunFlaresGhosts3Brightness = new FloatValue(layer.sunFlaresGhosts3Brightness);
    //            sunFlaresGhosts3Offset = new FloatValue(layer.sunFlaresGhosts3Offset);
    //            sunFlaresGhosts4Size = new FloatValue(layer.sunFlaresGhosts4Size);
    //            sunFlaresGhosts4Offset = new FloatValue(layer.sunFlaresGhosts4Offset);
    //            sunFlaresGhosts4Brightness = new FloatValue(layer.sunFlaresGhosts4Brightness);
    //            sunFlaresHaloOffset = new FloatValue(layer.sunFlaresHaloOffset);
    //            sunFlaresHaloAmplitude = new FloatValue(layer.sunFlaresHaloAmplitude);
    //            sunFlaresHaloIntensity = new FloatValue(layer.sunFlaresHaloIntensity);
    //            lensDirtIntensity = new FloatValue(layer.lensDirtIntensity);
    //            lensDirtThreshold = new FloatValue(layer.lensDirtThreshold);
    //            lensDirtSpread = new IntValue(layer.lensDirtSpread);
    //            lensDirtTexture = Graphics.Instance.PostProcessingManager.CurrentLensDirtTexturePath;
    //            lensDirtState = layer.lensDirtTexture.overrideState;
    //        }
    //    }

    //    public void Load(BeautifyBloom layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            if (bloomIntensity.value == 0)
    //                bloomIntensity.value = 0.5f;
    //            bloomIntensity.Fill(layer.bloomIntensity);
    //            if (bloomThreshold.value == 0)
    //                bloomThreshold.value = 0.75f;
    //            bloomThreshold.Fill(layer.bloomThreshold);
    //            if (bloomMaxBrightness.value == 0)
    //                bloomMaxBrightness.value = 1000.0f;
    //            bloomMaxBrightness.Fill(layer.bloomMaxBrightness);
    //            if (bloomDepthAtten.value == 0)
    //                bloomDepthAtten.value = 0.0f;
    //            bloomDepthAtten.Fill(layer.bloomDepthAtten);
    //            bloomAntiflicker.Fill(layer.bloomAntiflicker);
    //            bloomUltra.Fill(layer.bloomUltra);
    //            bloomDebug.Fill(layer.bloomDebug);
    //            bloomCustomize.Fill(layer.bloomCustomize);
    //            if (bloomWeight0.value == 0)
    //                bloomWeight0.value = 0.5f;
    //            bloomWeight0.Fill(layer.bloomWeight0);
    //            if (bloomWeight1.value == 0)
    //                bloomWeight1.value = 0.5f;
    //            bloomWeight1.Fill(layer.bloomWeight1);
    //            if (bloomWeight2.value == 0)
    //                bloomWeight2.value = 0.5f;
    //            bloomWeight2.Fill(layer.bloomWeight2);
    //            if (bloomWeight3.value == 0)
    //                bloomWeight3.value = 0.5f;
    //            bloomWeight3.Fill(layer.bloomWeight3);
    //            if (bloomWeight4.value == 0)
    //                bloomWeight4.value = 0.5f;
    //            bloomWeight4.Fill(layer.bloomWeight4);
    //            if (bloomWeight5.value == 0)
    //                bloomWeight5.value = 0.5f;
    //            bloomWeight5.Fill(layer.bloomWeight5);
    //            if (bloomBoost0.value == 0)
    //                bloomBoost0.value = 0.0f;
    //            bloomBoost0.Fill(layer.bloomBoost0);
    //            if (bloomBoost1.value == 0)
    //                bloomBoost1.value = 0.0f;
    //            bloomBoost1.Fill(layer.bloomBoost1);
    //            if (bloomBoost2.value == 0)
    //                bloomBoost2.value = 0.0f;
    //            bloomBoost2.Fill(layer.bloomBoost2);
    //            if (bloomBoost3.value == 0)
    //                bloomBoost3.value = 0.0f;
    //            bloomBoost3.Fill(layer.bloomBoost3);
    //            if (bloomBoost4.value == 0)
    //                bloomBoost4.value = 0.0f;
    //            bloomBoost4.Fill(layer.bloomBoost4);
    //            if (bloomBoost5.value == 0)
    //                bloomBoost5.value = 0.0f;
    //            bloomBoost5.Fill(layer.bloomBoost5);
    //            if (anamorphicFlaresIntensity.value == 0)
    //                anamorphicFlaresIntensity.value = 0.0f;
    //            anamorphicFlaresIntensity.Fill(layer.anamorphicFlaresIntensity);
    //            if (anamorphicFlaresThreshold.value == 0)
    //                anamorphicFlaresThreshold.value = 0.75f;
    //            anamorphicFlaresThreshold.Fill(layer.anamorphicFlaresThreshold);
    //            anamorphicFlaresVertical.Fill(layer.anamorphicFlaresVertical);
    //            if (anamorphicFlaresSpread.value == 0)
    //                anamorphicFlaresSpread.value = 1.0f;
    //            anamorphicFlaresSpread.Fill(layer.anamorphicFlaresSpread);
    //            if (anamorphicFlaresDepthAtten.value == 0)
    //                anamorphicFlaresDepthAtten.value = 0.0f;
    //            anamorphicFlaresDepthAtten.Fill(layer.anamorphicFlaresDepthAtten);
    //            anamorphicFlaresAntiflicker.Fill(layer.anamorphicFlaresAntiflicker);
    //            anamorphicFlaresUltra.Fill(layer.anamorphicFlaresUltra);
    //            if (anamorphicFlaresTint.value == null)
    //                anamorphicFlaresTint.value = new float[] { 1f, 1f, 1f };
    //            anamorphicFlaresTint.Fill(layer.anamorphicFlaresTint);
    //            if (sunFlaresIntensity.value == 0)
    //                sunFlaresIntensity.value = 0.0f;
    //            sunFlaresIntensity.Fill(layer.sunFlaresIntensity);
    //            if (sunFlaresTint.value == null)
    //                sunFlaresTint.value = new float[] { 1f, 1f, 1f };
    //            sunFlaresTint.Fill(layer.sunFlaresTint);
    //            if (sunFlaresSolarWindSpeed.value == 0)
    //                sunFlaresSolarWindSpeed.value = 0.01f;
    //            sunFlaresSolarWindSpeed.Fill(layer.sunFlaresSolarWindSpeed);
    //            sunFlaresRotationDeadZone.Fill(layer.sunFlaresRotationDeadZone);
    //            if (sunFlaresDownsampling.value == 0)
    //                sunFlaresDownsampling.value = 1;
    //            sunFlaresDownsampling.Fill(layer.sunFlaresDownsampling);
    //            //sunFlaresLayerMask.Fill(layer.sunFlaresLayerMask);
    //            if (sunFlaresSunIntensity.value == 0)
    //                sunFlaresSunIntensity.value = 0.1f;
    //            sunFlaresSunIntensity.Fill(layer.sunFlaresSunIntensity);
    //            if (sunFlaresSunDiskSize.value == 0)
    //                sunFlaresSunDiskSize.value = 0.05f;
    //            sunFlaresSunDiskSize.Fill(layer.sunFlaresSunDiskSize);
    //            if (sunFlaresSunRayDiffractionIntensity.value == 0)
    //                sunFlaresSunRayDiffractionIntensity.value = 3.5f;
    //            sunFlaresSunRayDiffractionIntensity.Fill(layer.sunFlaresSunRayDiffractionIntensity);
    //            if (sunFlaresSunRayDiffractionThreshold.value == 0)
    //                sunFlaresSunRayDiffractionThreshold.value = 0.13f;
    //            sunFlaresSunRayDiffractionThreshold.Fill(layer.sunFlaresSunRayDiffractionThreshold);
    //            if (sunFlaresCoronaRays1Length.value == 0)
    //                sunFlaresCoronaRays1Length.value = 0.02f;
    //            sunFlaresCoronaRays1Length.Fill(layer.sunFlaresCoronaRays1Length);
    //            if (sunFlaresCoronaRays1Streaks.value == 0)
    //                sunFlaresCoronaRays1Streaks.value = 12;
    //            sunFlaresCoronaRays1Streaks.Fill(layer.sunFlaresCoronaRays1Streaks);
    //            if (sunFlaresCoronaRays1Spread.value == 0)
    //                sunFlaresCoronaRays1Spread.value = 0.001f;
    //            sunFlaresCoronaRays1Spread.Fill(layer.sunFlaresCoronaRays1Spread);
    //            if (sunFlaresCoronaRays1AngleOffset.value == 0)
    //                sunFlaresCoronaRays1AngleOffset.value = 0f;
    //            sunFlaresCoronaRays1AngleOffset.Fill(layer.sunFlaresCoronaRays1AngleOffset);
    //            if (sunFlaresCoronaRays2Length.value == 0)
    //                sunFlaresCoronaRays2Length.value = 0.05f;
    //            sunFlaresCoronaRays2Length.Fill(layer.sunFlaresCoronaRays2Length);
    //            if (sunFlaresCoronaRays2Streaks.value == 0)
    //                sunFlaresCoronaRays2Streaks.value = 12f;
    //            sunFlaresCoronaRays2Streaks.Fill(layer.sunFlaresCoronaRays2Streaks);
    //            if (sunFlaresCoronaRays2Spread.value == 0)
    //                sunFlaresCoronaRays2Spread.value = 0.1f;
    //            sunFlaresCoronaRays2Spread.Fill(layer.sunFlaresCoronaRays2Spread);
    //            if (sunFlaresCoronaRays2AngleOffset.value == 0)
    //                sunFlaresCoronaRays2AngleOffset.value = 0f;
    //            sunFlaresCoronaRays2AngleOffset.Fill(layer.sunFlaresCoronaRays2AngleOffset);
    //            if (sunFlaresGhosts1Size.value == 0)
    //                sunFlaresGhosts1Size.value = 0.03f;
    //            sunFlaresGhosts1Size.Fill(layer.sunFlaresGhosts1Size);
    //            if (sunFlaresGhosts1Offset.value == 0)
    //                sunFlaresGhosts1Offset.value = 1.04f;
    //            sunFlaresGhosts1Offset.Fill(layer.sunFlaresGhosts1Offset);
    //            if (sunFlaresGhosts1Brightness.value == 0)
    //                sunFlaresGhosts1Brightness.value = 0.037f;
    //            sunFlaresGhosts1Brightness.Fill(layer.sunFlaresGhosts1Brightness);
    //            if (sunFlaresGhosts2Size.value == 0)
    //                sunFlaresGhosts2Size.value = 0.1f;
    //            sunFlaresGhosts2Size.Fill(layer.sunFlaresGhosts2Size);
    //            if (sunFlaresGhosts2Offset.value == 0)
    //                sunFlaresGhosts2Offset.value = 0.71f;
    //            sunFlaresGhosts2Offset.Fill(layer.sunFlaresGhosts2Offset);
    //            if (sunFlaresGhosts2Brightness.value == 0)
    //                sunFlaresGhosts2Brightness.value = 0.03f;
    //            sunFlaresGhosts2Brightness.Fill(layer.sunFlaresGhosts2Brightness);
    //            if (sunFlaresGhosts3Size.value == 0)
    //                sunFlaresGhosts3Size.value = 0.24f;
    //            sunFlaresGhosts3Size.Fill(layer.sunFlaresGhosts3Size);
    //            if (sunFlaresGhosts3Brightness.value == 0)
    //                sunFlaresGhosts3Brightness.value = 0.025f;
    //            sunFlaresGhosts3Brightness.Fill(layer.sunFlaresGhosts3Brightness);
    //            if (sunFlaresGhosts3Offset.value == 0)
    //                sunFlaresGhosts3Offset.value = 0.31f;
    //            sunFlaresGhosts3Offset.Fill(layer.sunFlaresGhosts3Offset);
    //            if (sunFlaresGhosts4Size.value == 0)
    //                sunFlaresGhosts4Size.value = 0.016f;
    //            sunFlaresGhosts4Size.Fill(layer.sunFlaresGhosts4Size);
    //            if (sunFlaresGhosts4Offset.value == 0)
    //                sunFlaresGhosts4Offset.value = 0f;
    //            sunFlaresGhosts4Offset.Fill(layer.sunFlaresGhosts4Offset);
    //            if (sunFlaresGhosts4Brightness.value == 0)
    //                sunFlaresGhosts4Brightness.value = 0.017f;
    //            sunFlaresGhosts4Brightness.Fill(layer.sunFlaresGhosts4Brightness);
    //            if (sunFlaresHaloOffset.value == 0)
    //                sunFlaresHaloOffset.value = 0.3f;
    //            sunFlaresHaloOffset.Fill(layer.sunFlaresHaloOffset);
    //            if (sunFlaresHaloAmplitude.value == 0)
    //                sunFlaresHaloAmplitude.value = 30f;
    //            sunFlaresHaloAmplitude.Fill(layer.sunFlaresHaloAmplitude);
    //            if (sunFlaresHaloIntensity.value == 0)
    //                sunFlaresHaloIntensity.value = 0.2f;
    //            sunFlaresHaloIntensity.Fill(layer.sunFlaresHaloIntensity);
    //            if (lensDirtIntensity.value == 0)
    //                lensDirtIntensity.value = 0.0f;
    //            lensDirtIntensity.Fill(layer.lensDirtIntensity);
    //            if (lensDirtThreshold.value == 0)
    //                lensDirtThreshold.value = 0.5f;
    //            lensDirtThreshold.Fill(layer.lensDirtThreshold);
    //            if (lensDirtSpread.value == 0)
    //                lensDirtSpread.value = 3;
    //            lensDirtSpread.Fill(layer.lensDirtSpread);
    //            layer.lensDirtTexture.overrideState = lensDirtState;
    //            Graphics.Instance.PostProcessingManager.LoadLensDirtTexture(lensDirtTexture, dirtTexture => layer.lensDirtTexture.value = dirtTexture);
    //        }   
    //    }
    //}

    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct BeautifyDoFParams
    //{
    //    public BoolValue enabled;
    //    public BoolValue depthOfFieldDebug;
    //    public FloatValue depthOfFieldAutofocusMinDistance;
    //    public FloatValue depthOfFieldAutofocusMaxDistance;
    //    public Vector2Value depthofFieldAutofocusViewportPoint;
    //    public LayerMask depthOfFieldAutofocusLayerMask;
    //    public LayerMask depthOfFieldExclusionLayerMask;
    //    public IntValue depthOfFieldExclusionLayerMaskDownsampling;
    //    public BoolValue depthOfFieldTransparencySupport;
    //    public LayerMask depthOfFieldTransparencyLayerMask;
    //    public IntValue depthOfFieldTransparencySupportDownsampling;
    //    public FloatValue depthOfFieldExclusionBias;
    //    public FloatValue depthOfFieldDistance;
    //    public FloatValue depthOfFieldFocusSpeed;
    //    public IntValue depthOfFieldDownsampling;
    //    public IntValue depthOfFieldMaxSamples;
    //    public FloatValue depthOfFieldFocalLength;
    //    public FloatValue depthOfFieldAperture;
    //    public BoolValue depthOfFieldForegroundBlur;
    //    public BoolValue depthOfFieldForegroundBlurHQ;
    //    public FloatValue depthOfFieldForegroundDistance;
    //    public BoolValue depthOfFieldBokeh;
    //    public FloatValue depthOfFieldBokehThreshold;
    //    public FloatValue depthOfFieldBokehIntensity;
    //    public FloatValue depthOfFieldMaxBrightness;
    //    public FloatValue depthOfFieldMaxDistance;
    //    public BeautifyDoFFocusModeValue depthOfFieldFocusMode;
    //    public BeautifyDoFFilterModeValue depthOfFieldFilterMode;

    //    public void Save(BeautifyDoF layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            depthOfFieldDebug = new BoolValue(layer.depthOfFieldDebug);
    //            depthOfFieldFocusMode = new BeautifyDoFFocusModeValue(layer.depthOfFieldFocusMode);
    //            depthOfFieldAutofocusMinDistance = new FloatValue(layer.depthOfFieldAutofocusMinDistance);
    //            depthOfFieldAutofocusMaxDistance = new FloatValue(layer.depthOfFieldAutofocusMaxDistance);
    //            depthofFieldAutofocusViewportPoint = new Vector2Value(layer.depthofFieldAutofocusViewportPoint);

    //            //depthOfFieldAutofocusLayerMask = new BeautifyDoFlayerMaskValue(layer.depthOfFieldAutofocusLayerMask);
    //            //depthOfFieldExclusionLayerMask = new BeautifyDoFlayerMaskValue(layer.depthOfFieldExclusionLayerMask);
    //            depthOfFieldExclusionLayerMaskDownsampling = new IntValue(layer.depthOfFieldExclusionLayerMaskDownsampling);
    //            depthOfFieldTransparencySupport = new BoolValue(layer.depthOfFieldTransparencySupport);
    //            //depthOfFieldTransparencyLayerMask = new BeautifyDoFlayerMaskValue(layer.depthOfFieldTransparencyLayerMask);
    //            depthOfFieldTransparencySupportDownsampling = new IntValue(layer.depthOfFieldTransparencySupportDownsampling);
    //            depthOfFieldExclusionBias = new FloatValue(layer.depthOfFieldExclusionBias);
    //            depthOfFieldDistance = new FloatValue(layer.depthOfFieldDistance);
    //            depthOfFieldFocusSpeed = new FloatValue(layer.depthOfFieldFocusSpeed);
    //            depthOfFieldDownsampling = new IntValue(layer.depthOfFieldDownsampling);
    //            depthOfFieldMaxSamples = new IntValue(layer.depthOfFieldMaxSamples);
    //            depthOfFieldFocalLength = new FloatValue(layer.depthOfFieldFocalLength);
    //            depthOfFieldAperture = new FloatValue(layer.depthOfFieldAperture);
    //            depthOfFieldForegroundBlur = new BoolValue(layer.depthOfFieldForegroundBlur);
    //            depthOfFieldForegroundBlurHQ = new BoolValue(layer.depthOfFieldForegroundBlurHQ);
    //            depthOfFieldForegroundDistance = new FloatValue(layer.depthOfFieldForegroundDistance);
    //            depthOfFieldBokeh = new BoolValue(layer.depthOfFieldBokeh);
    //            depthOfFieldBokehThreshold = new FloatValue(layer.depthOfFieldBokehThreshold);
    //            depthOfFieldBokehIntensity = new FloatValue(layer.depthOfFieldBokehIntensity);
    //            depthOfFieldMaxBrightness = new FloatValue(layer.depthOfFieldMaxBrightness);
    //            depthOfFieldMaxDistance = new FloatValue(layer.depthOfFieldMaxDistance);
    //            depthOfFieldFilterMode = new BeautifyDoFFilterModeValue(layer.depthOfFieldFilterMode);
    //        }
    //    }

    //    public void Load(BeautifyDoF layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            depthOfFieldDebug.Fill(layer.depthOfFieldDebug);
    //            depthOfFieldFocusMode.Fill(layer.depthOfFieldFocusMode);
    //            if (depthOfFieldAutofocusMinDistance.value == 0)
    //                depthOfFieldAutofocusMinDistance.value = 0.0f;
    //            depthOfFieldAutofocusMinDistance.Fill(layer.depthOfFieldAutofocusMinDistance);
    //            if (depthOfFieldAutofocusMaxDistance.value == 0)
    //                depthOfFieldAutofocusMaxDistance.value = 10000.0f;
    //            depthOfFieldAutofocusMaxDistance.Fill(layer.depthOfFieldAutofocusMaxDistance);
    //            if (depthofFieldAutofocusViewportPoint.value == null)
    //                depthofFieldAutofocusViewportPoint.value = new float[] { 0.5f, 0.5f };
    //            depthofFieldAutofocusViewportPoint.Fill(layer.depthofFieldAutofocusViewportPoint);
    //            //depthOfFieldAutofocusLayerMask.Fill(layer.depthOfFieldAutofocusLayerMask);
    //            //depthOfFieldExclusionLayerMask.Fill(layer.depthOfFieldExclusionLayerMask);

    //            if (depthOfFieldExclusionLayerMaskDownsampling.value == 0)
    //                depthOfFieldExclusionLayerMaskDownsampling.value = 1;
    //            depthOfFieldExclusionLayerMaskDownsampling.Fill(layer.depthOfFieldExclusionLayerMaskDownsampling);

    //            depthOfFieldTransparencySupport.Fill(layer.depthOfFieldTransparencySupport);
    //            //depthOfFieldTransparencyLayerMask.Fill(layer.depthOfFieldTransparencyLayerMask);
    //            if (depthOfFieldTransparencySupportDownsampling.value == 0)
    //                depthOfFieldTransparencySupportDownsampling.value = 1;
    //            depthOfFieldTransparencySupportDownsampling.Fill(layer.depthOfFieldTransparencySupportDownsampling);
    //            if (depthOfFieldExclusionBias.value == 0)
    //                depthOfFieldExclusionBias.value = 0.99f;
    //            depthOfFieldExclusionBias.Fill(layer.depthOfFieldExclusionBias);
    //            if (depthOfFieldDistance.value == 0)
    //                depthOfFieldDistance.value = 1.0f;
    //            depthOfFieldDistance.Fill(layer.depthOfFieldDistance);
    //            if (depthOfFieldFocusSpeed.value == 0)
    //                depthOfFieldFocusSpeed.value = 1.0f;
    //            depthOfFieldFocusSpeed.Fill(layer.depthOfFieldFocusSpeed);
    //            if (depthOfFieldDownsampling.value == 0)
    //                depthOfFieldDownsampling.value = 2;
    //            depthOfFieldDownsampling.Fill(layer.depthOfFieldDownsampling);
    //            if (depthOfFieldMaxSamples.value == 0)
    //                depthOfFieldMaxSamples.value = 4;
    //            depthOfFieldMaxSamples.Fill(layer.depthOfFieldMaxSamples);
    //            if (depthOfFieldFocalLength.value == 0)
    //                depthOfFieldFocalLength.value = 0.05f;
    //            depthOfFieldFocalLength.Fill(layer.depthOfFieldFocalLength);
    //            if (depthOfFieldAperture.value == 0)
    //                depthOfFieldAperture.value = 2.8f;
    //            depthOfFieldAperture.Fill(layer.depthOfFieldAperture);
    //            depthOfFieldForegroundBlur.Fill(layer.depthOfFieldForegroundBlur);
    //            depthOfFieldForegroundBlurHQ.Fill(layer.depthOfFieldForegroundBlurHQ);
    //            if (depthOfFieldForegroundDistance.value == 0)
    //                depthOfFieldForegroundDistance.value = 0.25f;
    //            depthOfFieldForegroundDistance.Fill(layer.depthOfFieldForegroundDistance);
    //            depthOfFieldBokeh.Fill(layer.depthOfFieldBokeh);
    //            if (depthOfFieldBokehThreshold.value == 0)
    //                depthOfFieldBokehThreshold.value = 1.0f;
    //            depthOfFieldBokehThreshold.Fill(layer.depthOfFieldBokehThreshold);
    //            if (depthOfFieldBokehIntensity.value == 0)
    //                depthOfFieldBokehIntensity.value = 2.0f;
    //            depthOfFieldBokehIntensity.Fill(layer.depthOfFieldBokehIntensity);
    //            if (depthOfFieldMaxBrightness.value == 0)
    //                depthOfFieldMaxBrightness.value = 1000.0f;
    //            depthOfFieldMaxBrightness.Fill(layer.depthOfFieldMaxBrightness);
    //            if (depthOfFieldMaxDistance.value == 0)
    //                depthOfFieldMaxDistance.value = 1.0f;
    //            depthOfFieldMaxDistance.Fill(layer.depthOfFieldMaxDistance);
    //            depthOfFieldFilterMode.Fill(layer.depthOfFieldFilterMode);
    //        }
    //    }
    //}

    //[MessagePackObject(keyAsPropertyName: true)]
    //public struct LightLeaksPPSParams
    //{
    //    public BoolValue enabled;
    //    public BoolValue useScreenBlendmode;
    //    public FloatValue intensity;
    //    public FloatValue redContribution;
    //    public FloatValue yellowContribution;
    //    public FloatValue blueContribution;
    //    public FloatValue moveSpeed;

    //    public void Save(LightLeaksPPS layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled = new BoolValue(layer.enabled);
    //            useScreenBlendmode = new BoolValue(layer.useScreenBlendmode);
    //            intensity = new FloatValue(layer.intensity);
    //            redContribution = new FloatValue(layer.redContribution);
    //            yellowContribution = new FloatValue(layer.yellowContribution);
    //            blueContribution = new FloatValue(layer.blueContribution);
    //            moveSpeed = new FloatValue(layer.moveSpeed);
    //        }
    //    }

    //    public void Load(LightLeaksPPS layer)
    //    {
    //        if (layer != null)
    //        {
    //            enabled.Fill(layer.enabled);
    //            layer.active = layer.enabled.value;
    //            useScreenBlendmode.Fill(layer.useScreenBlendmode);
    //            if (intensity.value == 0)
    //                intensity.value = 0.1f;
    //            intensity.Fill(layer.intensity);
    //            if (redContribution.value == 0)
    //                redContribution.value = 1.0f;
    //            redContribution.Fill(layer.redContribution);
    //            if (yellowContribution.value == 0)
    //                yellowContribution.value = 1.0f;
    //            yellowContribution.Fill(layer.yellowContribution);
    //            if (blueContribution.value == 0)
    //                blueContribution.value = 1.0f;
    //            blueContribution.Fill(layer.blueContribution);
    //            if (moveSpeed.value == 0)
    //                moveSpeed.value = 1.5f;
    //            moveSpeed.Fill(layer.moveSpeed);
    //        }
    //    }
    //}
    [MessagePackObject(keyAsPropertyName: true)]
    public struct AgxColorParams
    {
        public BoolValue enabled;
        public FloatValue temperature;
        public FloatValue tint;
        public FloatValue saturation;
        public FloatValue brightness;

        public void Save(AgxColor layer)
        {
            if (layer != null)
            {
                enabled = new BoolValue(layer.enabled);
                temperature = new FloatValue(layer.temperature);
                tint = new FloatValue(layer.tint);
                saturation = new FloatValue(layer.saturation);
                brightness = new FloatValue(layer.brightness);

            }
        }

        public void Load(AgxColor layer)
        {
            if (layer != null)
            {
                enabled.Fill(layer.enabled);
                layer.active = layer.enabled.value;
                if (temperature.value == 0)
                    temperature.value = 0.0f;
                temperature.Fill(layer.temperature);
                if (tint.value == 0)
                    tint.value = 0.0f;
                tint.Fill(layer.tint);
                if (saturation.value == 0)
                    saturation.value = 1.0f;
                saturation.Fill(layer.saturation);
                if (brightness.value == 0)
                    brightness.value = 0.0f;
                brightness.Fill(layer.brightness);

            }
        }
    }
}
