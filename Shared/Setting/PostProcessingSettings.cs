﻿using Graphics.CTAA;
using MessagePack;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Graphics.DebugUtils;
using Graphics.XPostProcessing;

// TODO: Turn on Post Processing in main menu.
// TODO: Messagepack clears out layer lists for a frame. Need to figure out to remove temporary solutions
namespace Graphics.Settings
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class PostProcessingSettings
    {
        // Parameters for Messagepack Save
        internal AmbientOcclusionParams paramAmbientOcclusion = new AmbientOcclusionParams();
        internal AutoExposureParams paramAutoExposure = new AutoExposureParams();
        internal BloomParams paramBloom = new BloomParams();
        internal ChromaticAberrationParams paramChromaticAberration = new ChromaticAberrationParams();
        internal ColorGradingParams paramColorGrading = new ColorGradingParams();
        internal DepthOfFieldParams paramDepthOfField = new DepthOfFieldParams();
        //internal GrainLayerParams paramGrainLayer = new GrainLayerParams();
        internal ScreenSpaceReflectionParams paramScreenSpaceReflection = new ScreenSpaceReflectionParams();
        internal VignetteParams paramVignette = new VignetteParams();
        internal MotionBlurParams paramMotionBlur = new MotionBlurParams();
        internal AmplifyOcclusionParams paramAmplifyOcclusion = new AmplifyOcclusionParams();
        internal AgxColorParams paramAgxColor = new AgxColorParams();
        internal SunShaftsHDRParams paramSunShaftsHDR = new SunShaftsHDRParams();
        internal ColorClippingParams paramColorClipping = new ColorClippingParams();

        public bool PostVolumeMap = false;

        public bool filterDithering = true;
        private const string temporalKeyword = "_TEMPORALFILTER_ON";

        public AmbientOcclusionList AOList;
        //public PixelizeList PixList;
        //public GlitchList GList;
        //public BloomList BList;

        public enum Antialiasing
        {
            None = PostProcessLayer.Antialiasing.None,
            FXAA = PostProcessLayer.Antialiasing.FastApproximateAntialiasing,
            SMAA = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing,
            TAA = PostProcessLayer.Antialiasing.TemporalAntialiasing,
            CTAA = 999,
        };

        public enum AmbientOcclusionList
        {
            Legacy,
            VAO,
            GTAO,
            Amplify
        };

        public enum GradingMode
        {
            LDR = UnityEngine.Rendering.PostProcessing.GradingMode.LowDefinitionRange,
            HDR = UnityEngine.Rendering.PostProcessing.GradingMode.HighDefinitionRange,
            AgX = UnityEngine.Rendering.PostProcessing.GradingMode.External
        }
        
        private readonly PostProcessLayer _postProcessLayer;
        internal AmbientOcclusion ambientOcclusionLayer;
        internal AutoExposure autoExposureLayer;
        internal Bloom bloomLayer;
        internal ChromaticAberration chromaticAberrationLayer;
        internal ColorGrading colorGradingLayer;
        internal DepthOfField depthOfFieldLayer;
        //internal Grain grainLayer;
        internal ScreenSpaceReflections screenSpaceReflectionsLayer;
        internal Vignette vignetteLayer;
        internal Camera initialCamera;
        internal MotionBlur motionBlurLayer;
        internal AgXColor agxColorLayer;
        internal AgXColorPost agxColorPostLayer;
        internal SunShaftsHDR sunShaftsHDRLayer;
        internal ColorClipping colorClippingLayer;

#if AI

        internal AmplifyOcclusionEffect amplifyOcclusionComponent;
                [IgnoreMember]
                public AmplifyOcclusionEffect AmplifyOcclusionComponent
                {
                    get
                    {
                        if (initialCamera == null && Camera.main != null)
                            amplifyOcclusionComponent = Camera.main.GetComponent<AmplifyOcclusionEffect>();
                        return amplifyOcclusionComponent;
                    }
                }
        #endif
        public PostProcessingSettings()
        {

            SetupVolume();

        }

        public PostProcessingSettings(Camera camera)
        {

            initialCamera = camera;
            _postProcessLayer = camera.GetComponent<PostProcessLayer>();
            SetupVolume();

        }

        internal PostProcessLayer PostProcessLayer => (_postProcessLayer == null) ? (initialCamera == null ? Camera.main : initialCamera).GetComponent<PostProcessLayer>() : _postProcessLayer;

        internal PostProcessVolume _volume;
        internal PostProcessVolume Volume => Graphics.Instance.GetOrAddComponent<PostProcessVolume>();

        internal PostProcessVolume SetupVolume()
        {
            // Turn off everything, We're not going to use
            foreach (PostProcessVolume postProcessVolume in GameObject.FindObjectsOfType<PostProcessVolume>())
            {
                if (SettingValues.profile == null && (postProcessVolume.name == "PostProcessVolume3D" || postProcessVolume.name == "PostProcessVolume"))
                {
                    SettingValues.profile = GameObject.Instantiate(postProcessVolume.profile);
                    SettingValues.profile.name = "Graphics Post Processing Profile";
                    InitializeProfiles();
                }

                postProcessVolume.weight = 0;
                postProcessVolume.enabled = false;
            }


            if (SettingValues.profile == null)
            {
                // Just in case
                SettingValues.profile = ScriptableObject.CreateInstance<PostProcessProfile>();
                InitializeProfiles();
            }

            _volume = Graphics.Instance.GetOrAddComponent<PostProcessVolume>();
            _volume.enabled = true;
            _volume.isGlobal = true;
            _volume.blendDistance = 0;
            _volume.weight = 1;
            _volume.priority = 1;
            _volume.useGUILayout = true;
            _volume.sharedProfile = SettingValues.profile;
            _volume.profile = SettingValues.profile;
            _volume.gameObject.layer = LayerMask.NameToLayer("PostProcessing");
//#if AI
//            if (initialCamera != null)
//                amplifyOcclusionComponent = initialCamera.GetComponent<AmplifyOcclusionEffect>();
//#endif
            return _volume;
        }
        internal void InitializeProfiles()
        {
            
            if (!SettingValues.profile.TryGetSettings(out chromaticAberrationLayer))
            {
                chromaticAberrationLayer = SettingValues.profile.AddSettings<ChromaticAberration>();
            }

            //if (!SettingValues.profile.TryGetSettings(out grainLayer))
            //{
            //    grainLayer = SettingValues.profile.AddSettings<Grain>();
            //}

            if (!SettingValues.profile.TryGetSettings(out ambientOcclusionLayer))
            {
                ambientOcclusionLayer = SettingValues.profile.AddSettings<AmbientOcclusion>();
            }

            if (!SettingValues.profile.TryGetSettings(out autoExposureLayer))
            {
                autoExposureLayer = SettingValues.profile.AddSettings<AutoExposure>();
            }

            if (!SettingValues.profile.TryGetSettings(out bloomLayer))
            {
                bloomLayer = SettingValues.profile.AddSettings<Bloom>();
            }

            if (!SettingValues.profile.TryGetSettings(out colorGradingLayer))
            {
                colorGradingLayer = SettingValues.profile.AddSettings<ColorGrading>();
            }

            if (!SettingValues.profile.TryGetSettings(out depthOfFieldLayer))
            {
                depthOfFieldLayer = SettingValues.profile.AddSettings<DepthOfField>();
            }

            if (!SettingValues.profile.TryGetSettings(out screenSpaceReflectionsLayer))
            {
                screenSpaceReflectionsLayer = SettingValues.profile.AddSettings<ScreenSpaceReflections>();
            }

            if (!SettingValues.profile.TryGetSettings(out vignetteLayer))
            {
                vignetteLayer = SettingValues.profile.AddSettings<Vignette>();
            }

            if (!SettingValues.profile.TryGetSettings(out motionBlurLayer))
            {
                motionBlurLayer = SettingValues.profile.AddSettings<MotionBlur>();
                motionBlurLayer.enabled.value = false;
            }

            if (!SettingValues.profile.TryGetSettings(out agxColorLayer))
            {
                agxColorLayer = SettingValues.profile.AddSettings<AgXColor>();
                agxColorLayer.enabled.value = false;
            }

            if (!SettingValues.profile.TryGetSettings(out agxColorPostLayer))
            {
                agxColorPostLayer = SettingValues.profile.AddSettings<AgXColorPost>();
                agxColorPostLayer.enabled.value = false;
                //agxColorPostLayer.priority.value = 8;
            }

            depthOfFieldLayer.enabled.value = false; // Make people use Depth of Field Manually

            if (!SettingValues.profile.TryGetSettings(out sunShaftsHDRLayer))
            {
                sunShaftsHDRLayer = SettingValues.profile.AddSettings<SunShaftsHDR>();
                sunShaftsHDRLayer.enabled.value = false;
                sunShaftsHDRLayer.priority.value = 4;
            }

            if (!SettingValues.profile.TryGetSettings(out colorClippingLayer))
            {
                colorClippingLayer = SettingValues.profile.AddSettings<ColorClipping>();
                colorClippingLayer.enabled.value = false;
            }
        }

        internal void ResetVolume()
        {
            if (SettingValues.defaultProfile != null)
            {
                Volume.sharedProfile = SettingValues.defaultProfile;
                Volume.profile = SettingValues.defaultProfile;
            }
        }

        public void SaveParameters()
        {
            if (Volume.profile.TryGetSettings(out AutoExposure autoExposureLayer))
            {
                paramAutoExposure.Save(autoExposureLayer);
            }

            if (Volume.profile.TryGetSettings(out AmbientOcclusion ambientOcclusionLayer))
            {
                paramAmbientOcclusion.Save(ambientOcclusionLayer);
            }

            if (Volume.profile.TryGetSettings(out Bloom bloomLayer))
            {
                paramBloom.Save(bloomLayer);
            }

            if (Volume.profile.TryGetSettings(out DepthOfField depthOfFieldLayer))
            {
                paramDepthOfField.Save(depthOfFieldLayer);
            }

            if (Volume.profile.TryGetSettings(out ChromaticAberration chromaticAberrationLayer))
            {
                paramChromaticAberration.Save(chromaticAberrationLayer);
            }

            if (Volume.profile.TryGetSettings(out ColorGrading colorGradingLayer))
            {
                paramColorGrading.Save(colorGradingLayer);
            }

            //if (Volume.profile.TryGetSettings(out Grain grainLayer))
            //{
            //    paramGrainLayer.Save(grainLayer);
            //}

            if (Volume.profile.TryGetSettings(out ScreenSpaceReflections screenSpaceReflectionsLayer))
            {
                paramScreenSpaceReflection.Save(screenSpaceReflectionsLayer);
            }

            if (Volume.profile.TryGetSettings(out Vignette vignetteLayer))
            {
                paramVignette.Save(vignetteLayer);
            }

            if (Volume.profile.TryGetSettings(out MotionBlur motionBlurLayer))
            {
                paramMotionBlur.Save(motionBlurLayer);
            }

            if (Volume.profile.TryGetSettings(out AgXColor agxColorLayer))
            {
                paramAgxColor.Save(agxColorLayer);
            }

            if (Volume.profile.TryGetSettings(out AgXColorPost agxColorPostLayer))
            {
                paramAgxColor.Save(agxColorPostLayer);
            }

            if (Volume.profile.TryGetSettings(out SunShaftsHDR sunShaftsHDRLayer))
            {
                paramSunShaftsHDR.Save(sunShaftsHDRLayer);
            }

            if (Volume.profile.TryGetSettings(out ColorClipping colorClippingLayer))
            {
                paramColorClipping.Save(colorClippingLayer);
            }
        }

        public void LoadParameters(bool loaddof)
        {
            //if (Volume.profile.TryGetSettings(out AdvancedDepthOfField advancedDepthOfFieldLayer))
            //{
            //    if (loaddof)
            //    {
            //        paramAdvancedDepthOfField.Load(advancedDepthOfFieldLayer);
            //        LogWithDots("[PPS] Advanced Depth of Field", "OK");
            //    }
            //    else
            //    {
            //        LogWithDots("[PPS] Advanced Depth of Field", "SKIP");
            //    }

            //}

            if (Volume.profile.TryGetSettings(out AutoExposure autoExposureLayer))
            {
                paramAutoExposure.Load(autoExposureLayer);
                LogWithDots("[PPS] Auto Exposure", "OK");
            }

            if (Volume.profile.TryGetSettings(out AmbientOcclusion ambientOcclusionLayer))
            {
                paramAmbientOcclusion.Load(ambientOcclusionLayer);
                LogWithDots("[PPS] Ambient Occlusion", "OK");
            }

            if (Volume.profile.TryGetSettings(out Bloom bloomLayer))
            {
                paramBloom.Load(bloomLayer);
                LogWithDots("[PPS] Bloom", "OK");
            }

            if (Volume.profile.TryGetSettings(out DepthOfField depthOfFieldLayer))
            {
                if (loaddof)
                {
                    paramDepthOfField.Load(depthOfFieldLayer);
                    LogWithDots("[PPS] Depth of Field", "OK");
                }
                else
                {
                    LogWithDots("[PPS] Depth of Field", "SKIP");
                }

            }

            if (Volume.profile.TryGetSettings(out ChromaticAberration chromaticAberrationLayer))
            {
                paramChromaticAberration.Load(chromaticAberrationLayer);
                LogWithDots("[PPS] Chromatic Aberration", "OK");
            }

            if (Volume.profile.TryGetSettings(out ColorGrading colorGradingLayer))
            {
                paramColorGrading.Load(colorGradingLayer);
                LogWithDots("[PPS] Color Grading", "OK");
            }

            //if (Volume.profile.TryGetSettings(out Grain grainLayer))
            //{
            //    paramGrainLayer.Load(grainLayer);
            //}

            if (Volume.profile.TryGetSettings(out ScreenSpaceReflections screenSpaceReflectionsLayer))
            {
                paramScreenSpaceReflection.Load(screenSpaceReflectionsLayer);
                LogWithDots("[PPS] Screen Space Reflections", "OK");
            }

            if (Volume.profile.TryGetSettings(out Vignette vignetteLayer))
            {
                paramVignette.Load(vignetteLayer);
                LogWithDots("[PPS] Vignette", "OK");
            }

            if (Volume.profile.TryGetSettings(out MotionBlur motionBlurLayer))
            {
                paramMotionBlur.Load(motionBlurLayer);
                LogWithDots("[PPS] Motion Blur", "OK");
            }

            //if (Volume.profile.TryGetSettings(out TiltShiftBokeh tiltShiftBokehLayer))
            //{
            //    paramTiltShiftBokeh.Load(tiltShiftBokehLayer);
            //    LogWithDots("[PPS] Tilt Shift Bokeh", "OK");
            //}

            if (Volume.profile.TryGetSettings(out SunShaftsHDR sunShaftsHDRLayer))
            {
                paramSunShaftsHDR.Load(sunShaftsHDRLayer);
                LogWithDots("[PPS] Sun Shafts HDR", "OK");
            }

            if (Volume.profile.TryGetSettings(out ColorClipping colorClippingLayer))
            {
                paramColorClipping.Load(colorClippingLayer);
                LogWithDots("[PPS] Color Clipping", "OK");
            }

            //if (Volume.profile.TryGetSettings(out CRTTube crtTubeLayer))
            //{
            //    paramCRTTube.Load(crtTubeLayer);
            //    LogWithDots("[PPS] CRT Tube", "OK");
            //}

            //if (Volume.profile.TryGetSettings(out Pixelize pixelizeLayer))
            //{
            //    paramPixelize.Load(pixelizeLayer);
            //    LogWithDots("[PPS] Pixelize", "OK");
            //}

            //if (Volume.profile.TryGetSettings(out GlitchRGBSplit glitchRGBSplitLayer))
            //{
            //    paramGlitchRGBSplit.Load(glitchRGBSplitLayer);
            //    LogWithDots("[PPS] Glitch RGB Split", "OK");
            //}

            //if (Volume.profile.TryGetSettings(out GlitchImageBlock glitchImageBlockLayer))
            //{
            //    paramGlitchImageBlock.Load(glitchImageBlockLayer);
            //    LogWithDots("[PPS] Glitch Image Block", "OK");
            //}

            //if (Volume.profile.TryGetSettings(out AsciiFxB asciiFxBLayer))
            //{
            //    paramAsciiFxB.Load(asciiFxBLayer);
            //    LogWithDots("[PPS] Ascii B", "OK");
            //}

            //if (Volume.profile.TryGetSettings(out Sharpen3in1 sharpen3in1Layer))
            //{
            //    paramSharpen3in1.Load(sharpen3in1Layer);
            //    LogWithDots("[PPS] Sharpen 3 in 1", "OK");
            //}

            //if (Volume.profile.TryGetSettings(out BeautifyBloom beautifyBloomLayer))
            //{
            //    paramBeautifyBloom.Load(beautifyBloomLayer);
            //    LogWithDots("[PPS] Beautify Bloom", "OK");
            //}

            if (Volume.profile.TryGetSettings(out AgXColor agxColorLayer))
            {
                paramAgxColor.Load(agxColorLayer);
                LogWithDots("[PPS] Pre Color Grading", "OK");
            }

            if (Volume.profile.TryGetSettings(out AgXColorPost agxColorPostLayer))
            {
                paramAgxColor.Load(agxColorPostLayer);

                LogWithDots("[PPS] Post Color Grading", "OK");
            }

            //if (Volume.profile.TryGetSettings(out LightLeaksPPS lightLeaksPPSLayer))
            //{
            //    paramLightLeaksPPS.Load(lightLeaksPPSLayer);
            //    LogWithDots("[PPS] Light Leaks", "OK");
            //}

            //if (Volume.profile.TryGetSettings(out EdgeDetection edgeDetectionLayer))
            //{
            //    paramEdgeDetection.Load(edgeDetectionLayer);
            //    LogWithDots("[PPS] Edge Detection", "OK");
            //}

            //if (Volume.profile.TryGetSettings(out AsciiFxA asciiFxALayer))
            //{
            //    paramAsciiFxA.Load(asciiFxALayer);
            //}

            //if (Volume.profile.TryGetSettings(out RaindropEffectSettings raindropEffectSettingsLayer))
            //{
            //    if (loadrain)
            //    {
            //        paramRaindropEffect.Load(raindropEffectSettingsLayer);
            //        LogWithDots("[PPS] Raindrop Effect", "OK");
            //    }
            //    else
            //    {
            //        LogWithDots("[PPS] Raindrop Effect", "SKIP");
            //    }

            //}

            //BeforeTransparent            
            //if (Volume.profile.TryGetSettings(out HeightFog heightFogLayer))
            //{
            //    if (loadfog)
            //    {
            //        paramHeightFog.Load(heightFogLayer);
            //        LogWithDots("[PPS] Height Fog", "OK");
            //    }
            //    else
            //    {
            //        LogWithDots("[PPS] Height Fog", "SKIP");
            //    }

            //}

            UpdateFilterDithering();
        }

        internal Transform VolumeTriggerSetting => _postProcessLayer.volumeTrigger;

        public LayerMask VolumeLayerSetting => PostProcessLayer.volumeLayer;

        public bool FilterDithering
        {
            get => filterDithering;
            set => filterDithering = value;
        }

        public float JitterSpread
        {
            get => PostProcessLayer.temporalAntialiasing.jitterSpread;
            set => PostProcessLayer.temporalAntialiasing.jitterSpread = value;
        }

        public float StationaryBlending
        {
            get => PostProcessLayer.temporalAntialiasing.stationaryBlending;
            set => PostProcessLayer.temporalAntialiasing.stationaryBlending = value;
        }

        public float MotionBlending
        {
            get => PostProcessLayer.temporalAntialiasing.motionBlending;
            set => PostProcessLayer.temporalAntialiasing.motionBlending = value;
        }

        public float Sharpness
        {
            get => PostProcessLayer.temporalAntialiasing.sharpness;
            set => PostProcessLayer.temporalAntialiasing.sharpness = value;
        }

        public bool FXAAMode
        {
            get => PostProcessLayer.fastApproximateAntialiasing.fastMode;
            set => PostProcessLayer.fastApproximateAntialiasing.fastMode = value;
        }

        public bool FXAAAlpha
        {
            get => PostProcessLayer.fastApproximateAntialiasing.keepAlpha;
            set => PostProcessLayer.fastApproximateAntialiasing.keepAlpha = value;
        }

        public SubpixelMorphologicalAntialiasing.Quality SMAAQuality
        {
            get => PostProcessLayer.subpixelMorphologicalAntialiasing.quality;
            set => PostProcessLayer.subpixelMorphologicalAntialiasing.quality = value;
        }
        //public AmbientOcclusionList AOList;
        public Antialiasing AntialiasingMode
        {
            get
            {
                if (CTAAManager.settings.Enabled)
                    return Antialiasing.CTAA;
                else
                    return (Antialiasing)PostProcessLayer.antialiasingMode;
            }            
            set
            {
                if (value == Antialiasing.CTAA)
                {
                    CTAAManager.settings.Enabled = true;
                    PostProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                    CTAAManager.UpdateSettings();
                }
                else
                {
                    if (CTAAManager.settings.Enabled)
                    {
                        CTAAManager.settings.Enabled = false;
                        CTAAManager.UpdateSettings();
                    }
                    PostProcessLayer.antialiasingMode = (PostProcessLayer.Antialiasing)value;
                }
            }
        }

        public float FocalDistance
        {
            get => depthOfFieldLayer != null ? depthOfFieldLayer.focusDistance.value : 0f;
            set
            {
                if (depthOfFieldLayer != null)
                {
                    depthOfFieldLayer.focusDistance.value = value;
                }
            }
        }

        public AmbientOcclusionParams AmbientOcclusion
        {
            get => paramAmbientOcclusion;
            set => paramAmbientOcclusion = value;
        }
        public AutoExposureParams AutoExposure
        {
            get => paramAutoExposure;
            set => paramAutoExposure = value;
        }
        public BloomParams Bloom
        {
            get => paramBloom;
            set => paramBloom = value;
        }

        public ChromaticAberrationParams ChromaticAberration
        {
            get => paramChromaticAberration;
            set => paramChromaticAberration = value;
        }
        public ColorGradingParams ColorGrading
        {
            get => paramColorGrading;
            set => paramColorGrading = value;
        }
        public DepthOfFieldParams DepthOfField
        {
            get => paramDepthOfField;
            set => paramDepthOfField = value;
        }
        //public GrainLayerParams GrainLayer
        //{
        //    get => paramGrainLayer;
        //    set => paramGrainLayer = value;
        //}
        public ScreenSpaceReflectionParams ScreenSpaceReflection
        {
            get => paramScreenSpaceReflection;
            set => paramScreenSpaceReflection = value;
        }
        public VignetteParams Vignette
        {
            get => paramVignette;
            set => paramVignette = value;
        }
        public MotionBlurParams MotionBlur
        {
            get => paramMotionBlur;
            set => paramMotionBlur = value;
        }
        public AgxColorParams AgxColor
        {
            get => paramAgxColor;
            set => paramAgxColor = value;
        }

        public SunShaftsHDRParams SunShaftsHDR
        {
            get => paramSunShaftsHDR;
            set => paramSunShaftsHDR = value;
        }

        public ColorClippingParams ColorClipping
        {
            get => paramColorClipping;
            set => paramColorClipping = value;
        }

        public void UpdateFilterDithering()
        {
            if (PostProcessLayer.antialiasingMode == PostProcessLayer.Antialiasing.TemporalAntialiasing || AntialiasingMode == Antialiasing.CTAA)
            {
                if (filterDithering && !Shader.IsKeywordEnabled(temporalKeyword))
                {
                    LogWithDots("Dithering", "IGN ANIMATED");
                    Shader.EnableKeyword(temporalKeyword);
                }
                else if (!filterDithering && Shader.IsKeywordEnabled(temporalKeyword))
                {
                    LogWithDots("Dithering", "IGN STATIC");
                    Shader.DisableKeyword(temporalKeyword);
                }
            }
            else
            {
                if (Shader.IsKeywordEnabled(temporalKeyword))
                {
                    LogWithDots("Dithering", "IGN STATIC");
                    Shader.DisableKeyword(temporalKeyword);
                }
            }
        }
    }
}
