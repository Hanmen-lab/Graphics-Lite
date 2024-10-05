using Graphics.CTAA;
using MessagePack;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
        internal GrainLayerParams paramGrainLayer = new GrainLayerParams();
        internal ScreenSpaceReflectionParams paramScreenSpaceReflection = new ScreenSpaceReflectionParams();
        internal VignetteParams paramVignette = new VignetteParams();
        internal MotionBlurParams paramMotionBlur = new MotionBlurParams();
        internal AmplifyOcclusionParams paramAmplifyOcclusion = new AmplifyOcclusionParams();
        //internal TiltShiftBokehParams paramTiltShiftBokeh = new TiltShiftBokehParams();
        //internal SunShaftsHDRParams paramSunShaftsHDR = new SunShaftsHDRParams();
        //internal CRTTubeParams paramCRTTube = new CRTTubeParams();
        //internal PixelizeQuadParams paramPixelizeQuad = new PixelizeQuadParams();
        //internal PixelizeLedParams paramPixelizeLed = new PixelizeLedParams();
        //internal PixelizeHexagonParams paramPixelizeHexagon = new PixelizeHexagonParams();
        //internal GlitchRGBSplitV5Params paramGlitchRGBSplitV5 = new GlitchRGBSplitV5Params();
        //internal GlitchRGBSplitV4Params paramGlitchRGBSplitV4 = new GlitchRGBSplitV4Params();
        //internal GlitchRGBSplitV3Params paramGlitchRGBSplitV3 = new GlitchRGBSplitV3Params();
        //internal GlitchRGBSplitV2Params paramGlitchRGBSplitV2 = new GlitchRGBSplitV2Params();
        //internal GlitchRGBSplitParams paramGlitchRGBSplit = new GlitchRGBSplitParams();
        //internal GlitchImageBlockParams paramGlitchImageBlock = new GlitchImageBlockParams();
        //internal GlitchImageBlockV2Params paramGlitchImageBlockV2 = new GlitchImageBlockV2Params();
        //internal GlitchImageBlockV3Params paramGlitchImageBlockV3 = new GlitchImageBlockV3Params();
        //internal GlitchImageBlockV4Params paramGlitchImageBlockV4 = new GlitchImageBlockV4Params();
        //internal BeautifyBloomParams paramBeautifyBloom = new BeautifyBloomParams();
        //internal LightLeaksPPSParams paramLightLeaksPPS = new LightLeaksPPSParams();
        //internal BeautifyDoFParams paramBeautifyDoF = new BeautifyDoFParams();
        internal AgxColorParams paramAgxColor = new AgxColorParams();

        public bool PostVolumeMap = false;

        public bool filterDithering = true;

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

        //public enum PixelizeList
        //{
        //    None,
        //    LED,
        //    Quad,
        //    Hex
        //};

        //public enum GlitchList
        //{
        //    None,
        //    RGBSplit,
        //    RGBSplitV2,
        //    RGBSplitV3,
        //    RGBSplitV4,
        //    RGBSplitV5,
        //    ImageBlock,
        //    ImageBlockV2,
        //    ImageBlockV3,
        //    ImageBlockV4
        //};

        //public enum BloomList
        //{
        //    None,
        //    Legacy,
        //    Beautify
        //};


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
        internal Grain grainLayer;
        internal ScreenSpaceReflections screenSpaceReflectionsLayer;
        internal Vignette vignetteLayer;
        internal Camera initialCamera;
        internal MotionBlur motionBlurLayer;
        internal AgxColor agxColorLayer;

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

            if (!SettingValues.profile.TryGetSettings(out grainLayer))
            {
                grainLayer = SettingValues.profile.AddSettings<Grain>();
            }

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
                agxColorLayer = SettingValues.profile.AddSettings<AgxColor>();
                agxColorLayer.enabled.value = false;
            }

            depthOfFieldLayer.enabled.value = false; // Make people use Depth of Field Manually
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

            if (Volume.profile.TryGetSettings(out Grain grainLayer))
            {
                paramGrainLayer.Save(grainLayer);
            }

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

            if (Volume.profile.TryGetSettings(out AgxColor agxColorLayer))
            {
                paramAgxColor.Save(agxColorLayer);
            }
        }

        public void LoadParameters()
        {
            if (Volume.profile.TryGetSettings(out AutoExposure autoExposureLayer))
            {
                paramAutoExposure.Load(autoExposureLayer);
            }

            if (Volume.profile.TryGetSettings(out AmbientOcclusion ambientOcclusionLayer))
            {
                paramAmbientOcclusion.Load(ambientOcclusionLayer);
            }

            if (Volume.profile.TryGetSettings(out Bloom bloomLayer))
            {
                paramBloom.Load(bloomLayer);
            }

            if (Volume.profile.TryGetSettings(out DepthOfField depthOfFieldLayer))
            {
                paramDepthOfField.Load(depthOfFieldLayer);
            }

            if (Volume.profile.TryGetSettings(out ChromaticAberration chromaticAberrationLayer))
            {
                paramChromaticAberration.Load(chromaticAberrationLayer);
            }

            if (Volume.profile.TryGetSettings(out ColorGrading colorGradingLayer))
            {
                paramColorGrading.Load(colorGradingLayer);
            }

            if (Volume.profile.TryGetSettings(out Grain grainLayer))
            {
                paramGrainLayer.Load(grainLayer);
            }

            if (Volume.profile.TryGetSettings(out ScreenSpaceReflections screenSpaceReflectionsLayer))
            {
                paramScreenSpaceReflection.Load(screenSpaceReflectionsLayer);
            }

            if (Volume.profile.TryGetSettings(out Vignette vignetteLayer))
            {
                paramVignette.Load(vignetteLayer);
            }

            if (Volume.profile.TryGetSettings(out MotionBlur motionBlurLayer))
            {
                paramMotionBlur.Load(motionBlurLayer);
            }

            if (Volume.profile.TryGetSettings(out AgxColor agxColorLayer))
            {
                paramAgxColor.Load(agxColorLayer);
            }
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
        public GrainLayerParams GrainLayer
        {
            get => paramGrainLayer;
            set => paramGrainLayer = value;
        }
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
    }
}
