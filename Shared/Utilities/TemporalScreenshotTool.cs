using BepInEx;
using CharaCustom;
//using FidelityFX.FSR3;
using Graphics.CTAA;
//using Graphics.FSR3;
using Graphics.SEGI;
using Graphics.Settings;
using HarmonyLib;
using KKAPI;
using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
//using static Graphics.VolumetricLightRenderer;
using static Graphics.DebugUtils;

namespace Graphics
{
    public class TemporalScreenshotTool : MonoBehaviour
    {
        public enum ScreenshotColorFormat
        {
            _32Bit,
            _8Bit
        }

        public enum ShadowResolutionOverride
        {
            NoOverride = 0,
            _4096 = 4096,
            _8192 = 8192,
            _16K = 16384,
            _32K = 32768
        }

        public enum ShadowCascadesOverride
        {
            NoOverride = 5,
            NoCascades = 0,
            TwoCascades = 2,
            FourCascades = 4
        }

        public Camera _camera;
        public bool _forceRemoveAlpha = true;
        public ScreenshotColorFormat _colorFormat = ScreenshotColorFormat._32Bit;
        public int warmupFrames = 32;
        public int _stopFrame = 0;
        public string _outputDirectory;
        public float _screenshotTimeStep = 1f / 60f;
        public bool _captureRenderDoc;
        public bool captureSEGIfullscreen = true;
        public bool enableCTAASuperSampling = false;
        //public bool highQualityNGSS = true;
        public bool highResReflectionProbes = true;
        public bool highResSSR = true;
        //public bool highQualityVolumetrics = true;

        public ShadowResolutionOverride customShadowResolutionOverride;
        public ShadowCascadesOverride customShadowCascadesOverride;

        // private
        public RenderTexture cameraRenderTarget;
        private RenderTexture downsizedTarget;
        private RenderTexture oldTarget;
        private Texture2D cpuTarget;
        private ComputeBuffer textureDataBuffer;
        private CommandBuffer cmd;
        
        // UI componentss
        private GameObject loadingOverlay;
        private Canvas loadingCanvas;
        //private Image loadingImage;
        private LoadingAnimation loadingAnimation;
        private List<Canvas> _disabledCanvases = new List<Canvas>();
        readonly Font font = Font.CreateDynamicFontFromOSFont("Segoe UI", 24);

        PostProcessingSettings postProcessingSettings = Graphics.Instance.PostProcessingSettings;
        private PostProcessingSettings.Antialiasing origAAMode;
        private bool hasTemporal;
        private int _captureFrames;
        private float origJitterSpread;
        private float origStationaryBlending;
        private float origMotionBlending;
        private float origSharpness;
        private float origTimeScale;
        private int origStability;
        private float origAdaptiveSharpness;
        private float origTemporalJitterScale;
        ////private static CTAASettings ctaa;
        private bool enableSupersampling = false;
        private bool origSEGIresolution;
        private bool enableSegIresolution;
        private bool origAxis;
        private bool origWindowState;
        //private bool raindropEnabled;
        //private ShadowMapResolution origNGSS_SHADOWS_RESOLUTION;
        //private int origNGSS_SAMPLING_TEST;
        //private int origNGSS_SAMPLING_FILTER;
        //private float origNGSS_SAMPLING_DISTANCE;
        //private ShadowResolution origShadowResolution;
        private Dictionary<ReflectionProbe, int> originalResolutions = new Dictionary<ReflectionProbe, int>();
        //private Fsr3Upscaler.QualityMode origFSRquality;

        private ScreenSpaceReflectionPreset origSSRPreset;

        //private ResMode origDepthMode;
        //private ResMode origRayMode;
        //private ResMode origResolveMode;
        //private bool origRayReuse;
        //private bool origUseMipMap;

        //private VolumetricResolution origvolumetricResolution;
        //private Dictionary<Light, int> originalVolumetricSampleCount = new Dictionary<Light, int>();


        AssetBundle assetBundle, loadingBundle;
        ComputeShader copyShader;
        Shader loadingShader;

        public static readonly int shaderId_IsScreenshotActive = Shader.PropertyToID("_IsScreenshotActive"); // Equals to 0 if game is running normally, not 0 if screenshot is being made
        public static readonly int shaderId_ScreenshotToolFrame = Shader.PropertyToID("_ScreenshotToolFrame"); // Frame counter, equals to 0 unless screenshot is being made, in which case increments by 1 every take
        public static readonly int shaderId_InvariantTime = Shader.PropertyToID("_InvariantTime");       // (t/20, t, t*2, t*3) except ticks when screenshot is made using _screenshotTimeStep
        public static readonly int shaderId_InvariantFrameCount = Shader.PropertyToID("_InvariantFrame");       // (t/20, t, t*2, t*3) except ticks when screenshot is made using _screenshotTimeStep
        private static readonly int shaderId_MotionIsZero = Shader.PropertyToID("_MotionIsZero"); // Hack to stop motion vectors in CTAA

        void Awake()
        {
            assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("temporalscreen.unity3d"));
            copyShader = assetBundle.LoadAsset<ComputeShader>("Assets/temporal screenshot/Resources/RawTextureCopy.compute");
            loadingBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("loading.unity3d"));
            loadingShader = loadingBundle.LoadAsset<Shader>("Assets/LoadingShader/LoadingShader.shader");
            _camera = GetComponent<Camera>();

            CreateLoadingUI(); // Prepare loading screen on startup
        }

        public void MakeScreenshot()
        {
            if (cameraRenderTarget)
                return;

            if (_captureRenderDoc)
                CaptureRenderDoc();

            StartCoroutine(MakeScreenshotRoutine());
        }

        IEnumerator MakeScreenshotRoutine()
        {
            ShowLoadingScreen();
            SaveSettings();
            SetSettings(); // Set settings for screenshot
            SetRenderTextures(); // Prepare render textures
            CreateCommandBuffer(); // Set Command buffer to hold render texture

            // Wait for Reflection Probes to update if needed
            if (highResReflectionProbes)
            {
                yield return StartCoroutine(UpdateReflectionProbeResolutionCoroutine(true));
            }

            if (hasTemporal)
            {
                Graphics.Instance.Log.LogInfo($"Warmup...");
                // Warmup frames
                for (int i = 0; i < _captureFrames; i++)
                {
                    // Faking Time
                    //Shader.SetGlobalVector(shaderId_InvariantTime, new Vector4(time / 20f, time, time * 2f, time * 3f));
                    //Shader.SetGlobalFloat(shaderId_MotionIsZero, 1);
                    //Shader.SetGlobalFloat(shaderId_ScreenshotToolFrame, i);
                    //Shader.SetGlobalVector(shaderId_InvariantFrameCount, new Vector4(i, 0, 0, 0));

                    // Stop time if needed
                    //if (i < _stopFrame)
                    //    Time.timeScale = 0f;
                    //else if (i == _stopFrame)
                    //    Time.timeScale = 1f; // Restore Time at frame

                    loadingAnimation.UpdateProgress(i + 1, _captureFrames, "Warmup");
                    //time += _screenshotTimeStep;

                    yield return null;
                }

                yield return new WaitForEndOfFrame();
            }
            else
            {
                _camera.Render();
                yield return new WaitForEndOfFrame();
            }


            PlayCaptureSound(); //Play sound
            Graphics.Instance.Log.LogInfo("Capture!");
            // Time manipulation
            //Shader.SetGlobalFloat(shaderId_IsScreenshotActive, 0);
            //Shader.SetGlobalFloat(shaderId_MotionIsZero, 0);
            //Shader.SetGlobalFloat(shaderId_ScreenshotToolFrame, 0);

            SaveScreenshotToFile(cmd); // Save to File
            yield return new WaitForEndOfFrame();

            _camera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, cmd);
            cmd.Release();
            cmd = null;

            RestoreSettings();

            // Reflection Probes
            if (highResReflectionProbes)
            {
                // Set text
                loadingAnimation.SetText("Resetting Reflection Probes...");
                Graphics.Instance.Log.LogInfo("Resetting Reflection Probes...");
                yield return StartCoroutine(UpdateReflectionProbeResolutionCoroutine(false));
            }
            HideLoadingScreen();
        }



        void CreateCommandBuffer()
        {
            if (cmd == null)
                cmd = new CommandBuffer();
            cmd.Clear();
            //cmd.SetRenderTarget(cameraRenderTarget);
            //cmd.ClearRenderTarget(true, true, Color.black);
            //cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            //UnityEngine.Graphics.ExecuteCommandBuffer(cmd);
        }

        void ShowLoadingScreen()
        {
            // Disable all UI
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();

            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.gameObject != loadingCanvas && canvas.enabled)
                {
                    canvas.enabled = false;
                    _disabledCanvases.Add(canvas);
                }
            }

            // Disable Axis gizmo
            if (KKAPI.KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                origAxis = Singleton<Studio.Studio>.Instance.workInfo.visibleAxis;        
                Singleton<StudioScene>.Instance.cameraInfo.axis = false;
            }

            origWindowState = Graphics.Instance.Show;
            Graphics.Instance.Show = false;


            // Show loading screen
            loadingOverlay.SetActive(true);
            //loadingAnimation.SetText("Preparing capture...");
        }

        void HideLoadingScreen()
        {
            // Hide loading screen
            loadingAnimation.SetText("Saving screenshot...");
            loadingOverlay.SetActive(false);

            // Restore Canvases
            foreach (Canvas canvas in _disabledCanvases)
            {
                if (canvas != null)
                    canvas.enabled = true;
            }
            _disabledCanvases.Clear();
        }

        void SetRenderTextures()
        {

            // Set text
            loadingAnimation.SetText("Preparing Render Textures...");
            TemporalScreenshotManager.Hooks.GetDimensions(out Vector2Int _screenshotResolution, out int _supersampling);

            //Disable supersampling for CTAA as it has its own mode for that
            if (postProcessingSettings.AntialiasingMode == PostProcessingSettings.Antialiasing.CTAA /*|| (postProcessingSettings.AntialiasingMode == PostProcessingSettings.Antialiasing.FSR3)*/)
            {
                _supersampling = 1;
            }

            int w = _screenshotResolution.x * _supersampling;
            int h = _screenshotResolution.y * _supersampling;

            var screenshotRenderFormat = GraphicsFormat.R16G16B16A16_UNorm;
            var screenshotTextureFormat = TextureFormat.RGBAFloat;

            cameraRenderTarget = new RenderTexture(w, h, 32, screenshotRenderFormat);
            cameraRenderTarget.name = "Screenshot Render Target";

            CTAASettings ctaaSettings = CTAAManager.settings;

            enableSupersampling = _supersampling > 1;
            if (enableSupersampling)
            {
                downsizedTarget = new RenderTexture(_screenshotResolution.x, _screenshotResolution.y, 0, screenshotRenderFormat);
            }

            cpuTarget = new Texture2D(_screenshotResolution.x, _screenshotResolution.y, screenshotTextureFormat, false);
            cpuTarget.name = "CPU Screenshot Target";

            textureDataBuffer = new ComputeBuffer(_screenshotResolution.x * _screenshotResolution.y, sizeof(float) * 4, ComputeBufferType.Default);
            _camera.targetTexture = cameraRenderTarget;
            switch (origAAMode)
            {
                //case PostProcessingSettings.Antialiasing.FSR3:
                //    FSR3Manager.Instance.IsScreenshotFired = true;
                //    break;
                default:
                    // nothing to do yet...
                    break;
            }
        }

        void SaveScreenshotToFile(CommandBuffer cmd)
        {
            loadingAnimation.SetText("Saving data to image...");

            RenderTexture finalTarget = enableSupersampling ? downsizedTarget : cameraRenderTarget;

            //cmd.Clear();
            if (enableSupersampling)
            {
                cmd.Blit(cameraRenderTarget, downsizedTarget);
            }

            if (origAAMode == PostProcessingSettings.Antialiasing.CTAA)
                cmd.Blit(BuiltinRenderTextureType.CameraTarget, finalTarget);

            cmd.SetComputeBufferParam(copyShader, 0, "Output", textureDataBuffer);
            cmd.SetComputeTextureParam(copyShader, 0, "Input", finalTarget);
            cmd.SetComputeFloatParam(copyShader, "ForceRemoveAlpha", _forceRemoveAlpha ? 1f : 0f);

            cmd.DispatchCompute(copyShader, 0, (finalTarget.width + 7) / 8, (finalTarget.height + 7) / 8, 1);
            cmd.RequestAsyncReadback(textureDataBuffer, (read) =>
            {
                var textureData = read.GetData<Vector4>();

                Color[] pix = new Color[textureData.Length];
                for (int i = 0; i < textureData.Length; i++)
                {
                    Vector4 p = textureData[i];
                    pix[i].r = p.x;
                    pix[i].g = p.y;
                    pix[i].b = p.z;
                    pix[i].a = p.w;
                }

                cpuTarget.SetPixels(pix);

                byte[] data;
                string filename = TemporalScreenshotManager.Hooks.GetUniqueFilename(out string extension, out int _jpegQuality);

                LogScreenshotMessage("rendered", filename);

                switch (extension)
                {
                    case "jpg":
                        data = cpuTarget.EncodeToJPG(_jpegQuality);
                        break;
                    case "png":
                        data = cpuTarget.EncodeToPNG();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                File.WriteAllBytes(filename, data);

                Destroy(cameraRenderTarget);
                cameraRenderTarget = null;
                Destroy(cpuTarget);
                cpuTarget = null;

                if (downsizedTarget)
                {
                    Destroy(downsizedTarget);
                    downsizedTarget = null;
                }

                textureDataBuffer.Dispose();
                textureDataBuffer = null;
            });

            //UnityEngine.Graphics.ExecuteCommandBuffer(cmd);
            _camera.AddCommandBuffer(CameraEvent.AfterImageEffects, cmd);
        }

        void SaveSettings()
        {
            oldTarget = _camera.targetTexture;
            origTimeScale = Time.timeScale;

            //origShadowResolution = QualitySettings.shadowResolution;

            switch (postProcessingSettings.AntialiasingMode)
            {
                case PostProcessingSettings.Antialiasing.CTAA:
                    var ctaaSettings = CTAAManager.settings;
                    origStability = ctaaSettings.TemporalStability.value;
                    origAdaptiveSharpness = ctaaSettings.AdaptiveSharpness.value;
                    origTemporalJitterScale = ctaaSettings.TemporalJitterScale.value;
                    break;

                case PostProcessingSettings.Antialiasing.TAA:
                    origAAMode = postProcessingSettings.AntialiasingMode;
                    origJitterSpread = postProcessingSettings.JitterSpread;
                    origStationaryBlending = postProcessingSettings.StationaryBlending;
                    origMotionBlending = postProcessingSettings.MotionBlending;
                    origSharpness = postProcessingSettings.Sharpness;
                    break;

                //case PostProcessingSettings.Antialiasing.FSR3:
                //    origFSRquality = FSR3Manager.Settings.QualityMode;
                //    break;
            }

            //if (highQualityNGSS)
            //{
            //    // Save NGSS Settings
            //    var ngssSettings = LightManager.ngsssettings;
            //    if (ngssSettings.Enabled)
            //    {
            //        //origNGSS_SHADOWS_RESOLUTION = (ShadowMapResolution)ngssSettings.NGSS_SHADOWS_RESOLUTION;
            //        origNGSS_SAMPLING_TEST = ngssSettings.NGSS_SAMPLING_TEST.value;
            //        origNGSS_SAMPLING_FILTER = ngssSettings.NGSS_SAMPLING_FILTER.value;
            //        origNGSS_SAMPLING_DISTANCE = ngssSettings.NGSS_SAMPLING_DISTANCE.value;
            //    }
            //}

            // Save SSR Settings
            if (highResSSR)
            {
                if (Graphics.Instance.PostProcessingSettings.screenSpaceReflectionsLayer.active)
                {
                    var ssrSettings = Graphics.Instance.PostProcessingSettings.screenSpaceReflectionsLayer;
                    origSSRPreset = ssrSettings.preset.value;
                }

                //var stochasticSSRSettings = StochasticSSRManager.settings;
                //if (stochasticSSRSettings.Enabled)
                //{
                //    origDepthMode = (ResMode)stochasticSSRSettings.depthMode;
                //    origRayMode = (ResMode)stochasticSSRSettings.rayMode;
                //    origResolveMode = (ResMode)stochasticSSRSettings.resolveMode;
                //    origRayReuse = stochasticSSRSettings.rayReuse.value;
                //    origUseMipMap = stochasticSSRSettings.useMipMap.value;
                //}
            }

            //// Save Volumetric Settings
            //if (highQualityVolumetrics && VolumetricLightManager.settings.Enabled)
            //{
            //    var volumetricSettings = VolumetricLightManager.settings;
            //    origvolumetricResolution = (VolumetricResolution)volumetricSettings.Resolution;

            //    var lights = GameObject.FindObjectsOfType<Light>();

            //    foreach (var light in lights)
            //    {
            //        var volumetricLight = light.GetComponent<VolumetricLight>();
            //        if (volumetricLight != null && volumetricLight.enabled)
            //        {
            //            originalVolumetricSampleCount[light] = volumetricLight.SampleCount;
            //        }
            //    }
            //}

            // Save SEGI Settings  
            var segiSettings = SEGIManager.settings;
            if (captureSEGIfullscreen && segiSettings.enabled)
                origSEGIresolution = segiSettings.halfResolution;

            //// Raindrop effect
            //raindropEnabled = Graphics.Instance.PostProcessingSettings.raindropEffectSettingsLayer.active;
        }

        void SetSettings()
        {
            origAAMode = postProcessingSettings.AntialiasingMode;

            switch (origAAMode)
            {
                case PostProcessingSettings.Antialiasing.TAA:
                    hasTemporal = true;
                    _captureFrames = warmupFrames;

                    // Batch assignment для TAA
                    var taaSettings = postProcessingSettings;
                    taaSettings.JitterSpread = 0.87f;
                    LogWithDotsLight("TAA Jitter Spread", "0.87");
                    taaSettings.StationaryBlending = 0.87f;
                    LogWithDotsLight("TAA Stationary Blending", "0.87");
                    taaSettings.MotionBlending = 0.87f;
                    LogWithDotsLight("TAA Motion Blending", "0.87");
                    taaSettings.Sharpness = 0f;
                    LogWithDotsLight("TAA Sharpness", "0");
                    break;

                case PostProcessingSettings.Antialiasing.CTAA:
                    hasTemporal = true;
                    _captureFrames = warmupFrames;

                    var ctaaSettings = CTAAManager.settings;
                    ctaaSettings.TemporalJitterScale.value = 0.5f;
                    LogWithDotsLight("CTAA Temporal Jitter Scale", "0.5");
                    ctaaSettings.TemporalStability.value = 3;
                    LogWithDotsLight("CTAA Temporal Stability", "3");
                    ctaaSettings.AdaptiveSharpness.value = 0f;
                    LogWithDotsLight("CTAA Adaptive Sharpness", "0");

                    if (enableCTAASuperSampling)
                    {
                        ctaaSettings.SupersampleMode = CTAASettings.CTAA_MODE.CINA_ULTRA;
                        LogWithDotsLight("CTAA Supersample Mode", CTAASettings.CTAA_MODE.CINA_ULTRA.ToString());
                    }
                    CTAAManager.UpdateSettings();
                    break;

                //case PostProcessingSettings.Antialiasing.FSR3:
                //    hasTemporal = true;
                //    _captureFrames = warmupFrames;

                //    var fsr3Settings = FSR3Manager.Settings;
                //    if (fsr3Settings.QualityMode != Fsr3Upscaler.QualityMode.NativeAA)
                //    {
                //        fsr3Settings.QualityMode = Fsr3Upscaler.QualityMode.NativeAA;
                //        FSR3Manager.UpdateSettings();
                //        LogWithDotsLight("FSR3 Quality Mode", Fsr3Upscaler.QualityMode.NativeAA.ToString());
                //    }
                //    break;

                default:
                    hasTemporal = false;
                    _captureFrames = 1;
                    break;
            }

            SEGISettings segiSettings = SEGIManager.settings;
            if (segiSettings.enabled && captureSEGIfullscreen)
            {
                segiSettings.halfResolution = false;
                SEGIManager.UpdateSettings();
                LogWithDotsLight("SEGI Fullscreen Resolution", "ENABLED");
            }

            //if (highQualityNGSS)
            //{
            //    NGSSSettings ngssSettings = LightManager.ngsssettings;
            //    if (ngssSettings != null && ngssSettings.Enabled)
            //    {
            //        //ngssSettings.Enabled = highQualityNGSS;
            //        //ngssSettings.NGSS_SHADOWS_RESOLUTION = NGSSSettings.ShadowMapResolution._16K;
            //        ngssSettings.NGSS_SAMPLING_TEST.value = 32;
            //        LogWithDotsLight("NGSS Sampling Test", "32");
            //        ngssSettings.NGSS_SAMPLING_FILTER.value = 128;
            //        LogWithDotsLight("NGSS Sampling Filter", "128");
            //        ngssSettings.NGSS_SAMPLING_DISTANCE.value = 500f;
            //        LogWithDotsLight("NGSS Sampling Distance", "500");
            //        LightManager.UpdateNGSSSettings();
            //    }
            //}

            if (highResSSR)
            {
                if (Graphics.Instance.PostProcessingSettings.screenSpaceReflectionsLayer.active)
                {
                    var ssrSettings = Graphics.Instance.PostProcessingSettings.screenSpaceReflectionsLayer;
                    //ssrSettings.preset.value = ScreenSpaceReflectionPreset.Overkill;
                    ssrSettings.preset.Override(ScreenSpaceReflectionPreset.Overkill);
                    LogWithDotsLight("Screen Space Reflections Preset", ScreenSpaceReflectionPreset.Overkill.ToString());
                }

                //var stochasticSSRSettings = StochasticSSRManager.settings;
                //if (stochasticSSRSettings.Enabled)
                //{
                //    //Graphics.Instance.Log.LogInfo("Setting full resolution Stochastic SSR for screenshot...");
                //    stochasticSSRSettings.depthMode = (StochasticSSRSettings.ResMode)ResMode.fullRes;
                //    LogWithDotsLight("Stochastic SSR Depth Mode", ResMode.fullRes.ToString());
                //    stochasticSSRSettings.rayMode = (StochasticSSRSettings.ResMode)ResMode.fullRes;
                //    LogWithDotsLight("Stochastic SSR Ray Mode", ResMode.fullRes.ToString());
                //    stochasticSSRSettings.resolveMode = (StochasticSSRSettings.ResMode)ResMode.fullRes;
                //    LogWithDotsLight("Stochastic SSR Resolve Mode", ResMode.fullRes.ToString());
                //    stochasticSSRSettings.rayReuse.value = false;
                //    LogWithDotsLight("Stochastic SSR Ray Reuse", "DISABLED");
                //    stochasticSSRSettings.useMipMap.value = false;
                //    LogWithDotsLight("Stochastic SSR Use MipMap", "DISABLED");
                //    StochasticSSRManager.UpdateSettings();
                //}
            }

            //if (highQualityVolumetrics && VolumetricLightManager.settings.Enabled)
            //{

            //    var volumetricSettings = VolumetricLightManager.settings;
            //    volumetricSettings.Resolution = (VolumetricLightSettings.VolumetricResolution)VolumetricResolution.Full;
            //    VolumetricLightManager.UpdateSettings();
            //    LogWithDotsLight("Volumetric Light Renderer", VolumetricResolution.Full.ToString());

            //    //Graphics.Instance.Log.LogInfo("Setting 'SampleCount': ");
            //    foreach (var light in GameObject.FindObjectsOfType<Light>())
            //    {
            //        var volumetricLight = light.GetComponent<VolumetricLight>();
            //        if (volumetricLight != null && volumetricLight.enabled)
            //        {
            //            volumetricLight.SampleCount = 32;
            //            //Graphics.Instance.Log.LogInfo("Setting 32 Samples to: "+ light.name);
            //            LogWithDotsLight(light.name+" Volumetric Sample Count","32");
            //        }
            //    }
            //}

            //Graphics.Instance.PostProcessingSettings.raindropEffectSettingsLayer.active = false;
        }

        public void RestoreSettings()
        {
            // Restore Camera
            _camera.targetTexture = oldTarget;
            postProcessingSettings.AntialiasingMode = origAAMode;

            // Restore Axis gizmo state
            if (KKAPI.KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                Singleton<StudioScene>.Instance.cameraInfo.axis = origAxis;
            }

            // Restore antialiasing-specific settings
            switch (origAAMode)
            {
                case PostProcessingSettings.Antialiasing.TAA:
                    Graphics.Instance.Log.LogInfo("Restoring TAA settings...");
                    postProcessingSettings.JitterSpread = origJitterSpread;
                    LogWithDotsLight("TAA Jitter Spread", origJitterSpread.ToString());
                    postProcessingSettings.StationaryBlending = origStationaryBlending;
                    LogWithDotsLight("TAA Stationary Blending", origStationaryBlending.ToString());
                    postProcessingSettings.MotionBlending = origMotionBlending;
                    LogWithDotsLight("TAA Motion Blending", origMotionBlending.ToString());
                    postProcessingSettings.Sharpness = origSharpness;
                    LogWithDotsLight("TAA Sharpness", origSharpness.ToString());
                    break;

                //case PostProcessingSettings.Antialiasing.FSR3:
                //    Graphics.Instance.Log.LogInfo("Restoring FSR3 settings...");
                //    FSR3Manager.Instance.IsScreenshotFired = false;
                //    var fsr3Settings = FSR3Manager.Settings;
                //    fsr3Settings.QualityMode = origFSRquality;
                //    FSR3Manager.UpdateSettings();
                //    LogWithDotsLight("FSR3 Quality Mode", origFSRquality.ToString());
                //    break;
            }

            // Restore CTAA Settings
            var ctaaSettings = CTAAManager.settings;
            if (ctaaSettings.Enabled)
            {
                Graphics.Instance.Log.LogInfo("Restoring CTAA settings...");
                ctaaSettings.TemporalStability.value = origStability;
                LogWithDotsLight("CTAA Temporal Stability", origStability.ToString());
                ctaaSettings.AdaptiveSharpness.value = origAdaptiveSharpness;
                LogWithDotsLight("CTAA Adaptive Sharpness", origAdaptiveSharpness.ToString());
                ctaaSettings.TemporalJitterScale.value = origTemporalJitterScale;
                LogWithDotsLight("CTAA Temporal Jitter Scale", origTemporalJitterScale.ToString());
                ctaaSettings.SupersampleMode = CTAASettings.CTAA_MODE.STANDARD;
                CTAAManager.UpdateSettings();
                LogWithDotsLight("CTAA Temporal Stability", origStability.ToString());
            }

            // Restore SEGI Settings    
            var segiSettings = SEGIManager.settings;
            if (segiSettings.enabled && captureSEGIfullscreen)
            {
                Graphics.Instance.Log.LogInfo("Restoring SEGI settings...");
                segiSettings.halfResolution = origSEGIresolution;
                SEGIManager.UpdateSettings();
                LogWithDotsLight("SEGI Fullscreen Resolution", origSEGIresolution ? "DISABLED" : "ENABLED");
            }

            ////Restore NGSS Settings
            //if (highQualityNGSS && LightManager.ngsssettings.Enabled)
            //{
            //    Graphics.Instance.Log.LogInfo("Restoring NGSS settings...");
            //    var ngssSettings = LightManager.ngsssettings;
            //    if (ngssSettings != null)
            //    {
            //        //ngssSettings.NGSS_SHADOWS_RESOLUTION = (NGSSSettings.ShadowMapResolution)origNGSS_SHADOWS_RESOLUTION;
            //        ngssSettings.NGSS_SAMPLING_TEST.value = origNGSS_SAMPLING_TEST;
            //        LogWithDotsLight("NGSS Sampling Test", origNGSS_SAMPLING_TEST.ToString());
            //        ngssSettings.NGSS_SAMPLING_FILTER.value = origNGSS_SAMPLING_FILTER;
            //        LogWithDotsLight("NGSS Sampling Filter", origNGSS_SAMPLING_FILTER.ToString());
            //        ngssSettings.NGSS_SAMPLING_DISTANCE.value = origNGSS_SAMPLING_DISTANCE;
            //        LogWithDotsLight("NGSS Sampling Distance", origNGSS_SAMPLING_DISTANCE.ToString());
            //        LightManager.UpdateNGSSSettings();
            //    }
            //}

            // Restore SSR Settings
            if (highResSSR)
            {
                if (Graphics.Instance.PostProcessingSettings.screenSpaceReflectionsLayer.active)
                {
                    Graphics.Instance.Log.LogInfo("Restoring Screen Space Reflection preset...");
                    var ssrSettings = Graphics.Instance.PostProcessingSettings.screenSpaceReflectionsLayer;
                    ssrSettings.preset.Override(origSSRPreset);
                    LogWithDotsLight("Screen Space Reflections Preset", origSSRPreset.ToString());
                }

                //var stochasticSSRSettings = StochasticSSRManager.settings;
                //if (stochasticSSRSettings.Enabled)
                //{
                //    Graphics.Instance.Log.LogInfo("Restoring Stochastic SSR settings...");
                //    stochasticSSRSettings.depthMode = (StochasticSSRSettings.ResMode)origDepthMode;
                //    LogWithDotsLight("Stochastic SSR Depth Mode", origDepthMode.ToString());
                //    stochasticSSRSettings.rayMode = (StochasticSSRSettings.ResMode)origRayMode;
                //    LogWithDotsLight("Stochastic SSR Ray Mode", origRayMode.ToString());
                //    stochasticSSRSettings.resolveMode = (StochasticSSRSettings.ResMode)origResolveMode;
                //    LogWithDotsLight("Stochastic SSR Resolve Mode", origResolveMode.ToString());
                //    stochasticSSRSettings.rayReuse.value = origRayReuse;
                //    LogWithDotsLight("Stochastic SSR Ray Reuse", origRayReuse ? "ENABLED" : "DISABLED");
                //    stochasticSSRSettings.useMipMap.value = origUseMipMap;
                //    LogWithDotsLight("Stochastic SSR Use MipMap", origUseMipMap ? "ENABLED" : "DISABLED");
                //    StochasticSSRManager.UpdateSettings();
                //}
            }

            //// Restore Volumetric Settings
            //if (highQualityVolumetrics && VolumetricLightManager.settings.Enabled)
            //{
            //    Graphics.Instance.Log.LogInfo("Restoring Volumetric Lights settings...");

            //    var volumetricSettings = VolumetricLightManager.settings;
            //    volumetricSettings.Resolution = (VolumetricLightSettings.VolumetricResolution)origvolumetricResolution;
            //    VolumetricLightManager.UpdateSettings();
            //    LogWithDotsLight("Volumetric Light Renderer", origvolumetricResolution.ToString());

            //    var lights = GameObject.FindObjectsOfType<Light>();
            //    foreach (var light in lights)
            //    {
            //        var volumetricLight = light.GetComponent<VolumetricLight>();
            //        if (volumetricLight != null && volumetricLight.enabled)
            //        {
            //            if (originalVolumetricSampleCount.ContainsKey(light))
            //            {
            //                volumetricLight.SampleCount = originalVolumetricSampleCount[light];
            //                LogWithDotsLight(light.name + " Volumetric Sample Count", originalVolumetricSampleCount[light].ToString());
            //            }
            //        }
            //    }
            //}

            //// Restore raindrop
            //Graphics.Instance.PostProcessingSettings.raindropEffectSettingsLayer.active = raindropEnabled;

            Graphics.Instance.Show = origWindowState;
        }

        private IEnumerator UpdateReflectionProbeResolutionCoroutine(bool highRes)
        {
            loadingAnimation.SetText("Preparing Reflection Probes...");
            //Graphics.Instance.Log.LogInfo("Preparing Reflection Probes...");

            SkyboxManager skyboxManager = Graphics.Instance.SkyboxManager;
            ReflectionProbe[] rps = skyboxManager.GetReflectinProbes();

            GlobalSettings globalSettings = Graphics.Instance.Settings;
            globalSettings.RealtimeReflectionProbes = true;

            foreach (ReflectionProbe rp in rps)
            {
                if (rp.mode == UnityEngine.Rendering.ReflectionProbeMode.Realtime && rp.intensity > 0f)
                {
                    if (highRes)
                    {
                        originalResolutions[rp] = rp.resolution;
                        rp.resolution = 2048;
                        rp.RenderProbe();
                        LogWithDotsLight(rp.name+ " Resolution", "2048");
                    }
                    else
                    {
                        if (originalResolutions.ContainsKey(rp))
                        {
                            rp.resolution = originalResolutions[rp];
                            rp.RenderProbe();
                            LogWithDotsLight(rp.name + " Resolution", originalResolutions[rp].ToString());
                        }
                    }

                    // Wait for the end of frame to ensure the probe has time to update
                    yield return new WaitForEndOfFrame();
                }
            }

            globalSettings.RealtimeReflectionProbes = false;
        }

        void CaptureRenderDoc()
        {
#if UNITY_EDITOR
        var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        var window = UnityEditor.EditorWindow.GetWindow(gameViewType);
        
        var m_Host = gameViewType.GetField("m_Parent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var host = m_Host.GetValue(window);
        
        host.GetType().GetMethod("CaptureRenderDocScene", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(host, new object[] { });
#endif
        }

        private static void LogScreenshotMessage(string kind, string filename)
        {
            if (filename.StartsWith(Paths.GameRootPath, StringComparison.OrdinalIgnoreCase))
                filename = filename.Substring(Paths.GameRootPath.Length);

            Graphics.Instance.Log.LogMessage($"Writing {kind} screenshot to {filename}");
        }

        private static void PlayCaptureSound()
        {
#if AI
            Singleton<Manager.Resources>.Instance.SoundPack.Play(AIProject.SoundPack.SystemSE.Photo);
#elif HS2
            if (Hooks.SoundWasPlayed)
                Hooks.SoundWasPlayed = false;
            else
                Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.photo);
#else
            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.photo);
#endif
        }

        void CreateLoadingUI()
        {
            // Создаем Canvas для загрузки
            GameObject canvasObj = new GameObject("LoadingCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Полноэкранный фон
            GameObject backgroundObj = new GameObject("LoadingBackground");
            backgroundObj.transform.SetParent(canvasObj.transform, false);
            Image backgroundImage = backgroundObj.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.7f);
            RectTransform bgRect = backgroundImage.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // Круг загрузки
            GameObject circleObj = new GameObject("LoadingCircle");
            circleObj.transform.SetParent(canvasObj.transform, false);
            Image loadingImage = circleObj.AddComponent<Image>();
            loadingImage.material = new Material(loadingShader);
            
            RectTransform circleRect = loadingImage.rectTransform;
            circleRect.anchorMin = new Vector2(0.5f, 0.5f);
            circleRect.anchorMax = new Vector2(0.5f, 0.5f);
            circleRect.anchoredPosition = new Vector2(0, 50); // Немного выше центра

            float circleSize = 200f;
            circleRect.sizeDelta = new Vector2(circleSize, circleSize);

            // Белая текстура для круга
            Texture2D whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
            loadingImage.sprite = Sprite.Create(whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);

            // Текст прогресса
            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(canvasObj.transform, false);
            Text progressText = textObj.AddComponent<Text>();

            // Настройка текста
            progressText.text = "Loading...";
            progressText.font = font;
            progressText.fontSize = 24;
            progressText.color = Color.white;
            progressText.alignment = TextAnchor.MiddleCenter;

            RectTransform textRect = progressText.rectTransform;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0, -80); // Под кругом
            textRect.sizeDelta = new Vector2(400, 60);

            // Компонент анимации
            loadingAnimation = circleObj.AddComponent<LoadingAnimation>();
            loadingAnimation.loadingImage = loadingImage;
            loadingAnimation.loadingShader = loadingShader;
            loadingAnimation.progressText = progressText; // Добавляем ссылку на текст

            loadingOverlay = canvasObj;
            loadingOverlay.SetActive(false);
        }

        public IEnumerator MakeThumbnailRoutine(Camera cam)
        {
            ShowLoadingScreen();
            for (int i = 0; i < 16; i++)
            {
                // Faking Time
                //Shader.SetGlobalVector(shaderId_InvariantTime, new Vector4(time / 20f, time, time * 2f, time * 3f));
                //Shader.SetGlobalFloat(shaderId_MotionIsZero, 1);
                //Shader.SetGlobalFloat(shaderId_ScreenshotToolFrame, i);
                //Shader.SetGlobalVector(shaderId_InvariantFrameCount, new Vector4(i, 0, 0, 0));

                loadingAnimation.UpdateProgress(i + 1, 16, "Warmup");
                //time += _screenshotTimeStep;

                yield return null;
            }

            yield return new WaitForEndOfFrame();
            HideLoadingScreen();
        }

        public IEnumerator MakeThumbnailRoutineWithCallback(Camera camera, System.Action onComplete)
        {
            ShowLoadingScreen();
            for (int i = 0; i < 16; i++)
            {
                // Faking Time
                //Shader.SetGlobalVector(shaderId_InvariantTime, new Vector4(time / 20f, time, time * 2f, time * 3f));
                //Shader.SetGlobalFloat(shaderId_MotionIsZero, 1);
                //Shader.SetGlobalFloat(shaderId_ScreenshotToolFrame, i);
                //Shader.SetGlobalVector(shaderId_InvariantFrameCount, new Vector4(i, 0, 0, 0));

                loadingAnimation.UpdateProgress(i + 1, 16, "Warmup");
                //time += _screenshotTimeStep;

                yield return null;
            }

            yield return new WaitForEndOfFrame();
            HideLoadingScreen();
            // Вызвать callback после завершения
            onComplete?.Invoke();
        }

        // Broken
        //void ResetComponents()
        //{
        //var components = GetComponents<Behaviour>();

        //for (int i = 0; i < components.Length; i++)
        //{
        //    if (components[i].enabled && components[i] != this)
        //    {
        //        components[i].enabled = false;
        //    }
        //    else
        //    {
        //        components[i] = null;
        //    }
        //}
        //for (int i = 0; i < components.Length; i++)
        //{
        //    if (components[i] && components[i] != this)
        //    {
        //        components[i].enabled = true;
        //    }
        //}
        //}
    }

}

