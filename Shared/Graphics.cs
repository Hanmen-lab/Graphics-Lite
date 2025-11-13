using BepInEx;
using BepInEx.Configuration;
using Graphics.GTAO;
using Graphics.AmplifyOcclusion;
using Graphics.VAO;
//using Graphics.AmplifyBloom;
using Graphics.CTAA;
using Graphics.Hooks;
using Graphics.SEGI;
using Graphics.Inspector;
using Graphics.Patch;
using Graphics.Settings;
using Graphics.Textures;
using Graphics.GlobalFog;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Graphics.DebugUtils;
using UnityEngine.Rendering;

namespace Graphics
{
    [BepInIncompatibility("dhhai4mod"), BepInIncompatibility("HS2_HDSaveCard"), BepInIncompatibility("8484093f-f32f-47ab-857d-484370c226b7")]
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(ExtensibleSaveFormat.ExtendedSave.GUID)]
    public partial class Graphics : BaseUnityPlugin
    {
        public const string GUID = "ore.graphics";
        public const string PluginName = "Graphics";
        public const string Version = "2.2.0";

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

        public static ConfigEntry<KeyCode> ConfigShortcut { get; private set; }
        public static ConfigEntry<string> ConfigCubeMapPath { get; private set; }
        public static ConfigEntry<string> ConfigPresetPath { get; private set; }
        public static ConfigEntry<string> ConfigLensDirtPath { get; private set; }
        //public static ConfigEntry<string> ConfigStarburstPath { get; private set; }
        public static ConfigEntry<string> ConfigLocalizationPath { get; private set; }
        public static ConfigEntry<LocalizationManager.Language> ConfigLanguage { get; private set; }
        public static ConfigEntry<int> ConfigFontSize { get; internal set; }
        public static ConfigEntry<int> ConfigWindowWidth { get; internal set; }
        public static ConfigEntry<int> ConfigWindowHeight { get; internal set; }
        public static ConfigEntry<int> ConfigWindowOffsetX { get; internal set; }
        public static ConfigEntry<int> ConfigWindowOffsetY { get; internal set; }
        public static ConfigEntry<bool> ScreenshotOverride { get; internal set; }
        public static ConfigEntry<ShadowResolutionOverride> customShadowResolutionOverride { get; internal set; }
        public static ConfigEntry<ShadowCascadesOverride> customShadowCascadesOverride { get; internal set; }

        public static ConfigEntry<bool> loadSkybox { get; internal set; }
        public static ConfigEntry<bool> loadSEGI { get; internal set; }
        public static ConfigEntry<bool> loadSSS { get; internal set; }
        public static ConfigEntry<bool> loadShadows { get; internal set; }
        public static ConfigEntry<bool> loadAura { get; internal set; }
        public static ConfigEntry<bool> loadVolumetrics { get; internal set; }
        public static ConfigEntry<bool> loadLuxwater { get; internal set; }
        public static ConfigEntry<bool> loadHeightFog { get; internal set; }
        public static ConfigEntry<bool> loadDoF { get; internal set; }
        public static ConfigEntry<bool> loadRain { get; internal set; }

        public Preset preset;

        private bool _showGUI;
        private bool _isLoaded = false;
        private CursorLockMode _previousCursorLockState;
        private bool _previousCursorVisible;
#if AI
        private float _previousTimeScale;
#endif 
        private SkyboxManager _skyboxManager;
        private LightManager _lightManager;
        private PostProcessingManager _postProcessingManager;
        private PresetManager _presetManager;
        private CTAAManager _ctaaManager;
        private SSSManager _sssManager;
        private SEGIManager _segiManager;
        private GlobalFogManager _globalfogManager;
        private LuxWater_UnderWaterRenderingManager _underwaterManager;
        private LuxWater_WaterVolumeTriggerManager _waterVolumeTriggerManager;
        private ConnectSunToUnderwaterManager _connectorManager;
        private LuxWater_UnderwaterBlurManager _underwaterBlur;
        private GTAOManager _gtaoManager;
        private AmplifyOccManager _amplifyoccManager;
        private VAOManager _vaoManager;
        private DitheredShadowsManager _ditheredshadowsManager;
        private FocusManager _focusManager;
        private AuraManager _auraManager;
        private FilmGrainManager _filmGrainManager;
        private DecalsSystemManager _decalsSystemManager;
        private TemporalScreenshotManager _temporalScreenshotManager;

        private Inspector.Inspector _inspector;

        private HoohSmartphoneScanner smartphoneScanner;

        internal GlobalSettings Settings { get; private set; }
        internal CameraSettings CameraSettings { get; private set; }
        internal LightingSettings LightingSettings { get; private set; }
        internal PostProcessingSettings PostProcessingSettings { get; private set; }
        internal AmplifyOccSettings AmplifyOcclusionSettings { get; private set; }

        //internal SSSSettings SSSSettings { get; private set; }
        //internal SEGISettings SEGISettings { get; private set; }

        internal BepInEx.Logging.ManualLogSource Log => Logger;

        public static Graphics Instance { get; private set; }

        public Graphics()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Can only create one instance of Graphics");
            }
            Log.LogInfo("Graphics Init");

            Instance = this;
            string rootFolder = BepInEx.Paths.GameRootPath;
            ConfigShortcut = Config.Bind("Keybind", "Keyboard Shortcut", KeyCode.F5, new ConfigDescription("Keyboard Shortcut"));
            ConfigPresetPath = Config.Bind("Folders Config", "Preset Location", rootFolder + @"\presets\", new ConfigDescription("Where presets are stored"));
            ConfigCubeMapPath = Config.Bind("Folders Config", "Cubemap path", rootFolder + @"\cubemaps\", new ConfigDescription("Where cubemaps are stored"));
            ConfigLensDirtPath = Config.Bind("Folders Config", "Lens dirt texture path", rootFolder + @"\lensdirts\", new ConfigDescription("Where lens dirt textures are stored"));
            //ConfigFrostTexPath = Config.Bind("Folders Config", "FrostFX texture path", rootFolder + @"\frostFX\", new ConfigDescription("Where frostFX textures are stored"));
            ConfigLocalizationPath = Config.Bind("Folders Config", "Localization path", rootFolder + @"\BepInEx\plugins\Graphics\Resources\", new ConfigDescription("Where localizations are stored"));
            ConfigLanguage = Config.Bind("UI", "Language", LocalizationManager.DefaultLanguage(), new ConfigDescription("Default Language"));
            ConfigFontSize = Config.Bind("UI", "Font Size", 12, new ConfigDescription("Font Size"));
            GUIStyles.FontSize = ConfigFontSize.Value;
            ConfigWindowWidth = Config.Bind("UI", "Window Width", 750, new ConfigDescription("Window Width"));
            Inspector.Inspector.Width = ConfigWindowWidth.Value;
            ConfigWindowHeight = Config.Bind("UI", "Window Height", 1024, new ConfigDescription("Window Height"));
            Inspector.Inspector.Height = ConfigWindowHeight.Value;
            ConfigWindowOffsetX = Config.Bind("UI", "Window Position Offset X", (Screen.width - ConfigWindowWidth.Value) / 2, new ConfigDescription("Window Position Offset X"));
            Inspector.Inspector.StartOffsetX = ConfigWindowOffsetX.Value;
            ConfigWindowOffsetY = Config.Bind("UI", "Window Position Offset Y", (Screen.height - ConfigWindowHeight.Value) / 2, new ConfigDescription("Window Position Offset Y"));
            Inspector.Inspector.StartOffsetY = ConfigWindowOffsetY.Value;
            ScreenshotOverride = Config.Bind("Screenshot Settings", "Enable New Screenshot Engine.", true, new ConfigDescription("Override Screenshot Manager Render function with Temporal Screenshot for CTAA compatibility. (Must disable Alpha in Screenshot Manager)"));
            customShadowResolutionOverride = Config.Bind("Screenshot Settings", "ShadowResolutionOverride", ShadowResolutionOverride.NoOverride, "Override shadow resolution (0 = no override)");
            customShadowCascadesOverride = Config.Bind("Screenshot Settings", "ShadowCascadesOverride", ShadowCascadesOverride.NoOverride, "Override shadow cascades (5 = no override)");

            loadSkybox = Config.Bind("Preset Filters", "Load Preset Skybox", false, new ConfigDescription("Load Preset Skybox"));
            loadSEGI = Config.Bind("Preset Filters", "Load Preset SEGI", false, new ConfigDescription("Load Preset SEGI"));
            loadSSS = Config.Bind("Preset Filters", "Load Preset SSS", true, new ConfigDescription("Load Preset SSS"));
            loadShadows = Config.Bind("Preset Filters", "Load Preset Shadows", false, new ConfigDescription("Load Preset Shadows"));
            loadAura = Config.Bind("Preset Filters", "Load Preset Aura", false, new ConfigDescription("Load Preset Aura"));
            loadVolumetrics = Config.Bind("Preset Filters", "Load Preset Volumetrics", false, new ConfigDescription("Load Preset Volumetrics"));
            loadLuxwater = Config.Bind("Preset Filters", "Load Preset LuxWater", false, new ConfigDescription("Load Preset LuxWater"));
            loadHeightFog = Config.Bind("Preset Filters", "Load Preset Height Fog", true, new ConfigDescription("Load Preset Height Fog"));
            loadDoF = Config.Bind("Preset Filters", "Load Preset Depth of Field", false, new ConfigDescription("Load Preset Depth of Field"));
            loadRain = Config.Bind("Preset Filters", "Load Preset Rain", true, new ConfigDescription("Load Preset Rain"));
        }

        private void Awake()
        {
            CharacterApi.RegisterExtraBehaviour<CharaController>(GUID);
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneController>(GUID);
            SceneManager.sceneLoaded += OnSceneLoaded;
            StudioHooks.Map_OnLoadAfter();

            TemporalScreenshotManager.Hooks.PatchScreenshotManager();
            if (KKAPI.Studio.StudioAPI.InsideStudio)
                CreatePngScreenController.Hooks.PatchCreatePngScreenController();

            CreatePngScreenController.Hooks.PatchCreatePngController();
        }

        private IEnumerator Start()
        {
            Log.LogInfo("Starting");
            if (IsStudio()) StudioHooks.Init();
            yield return new WaitUntil(IsLoaded);
            Settings = new GlobalSettings();
            CameraSettings = new CameraSettings();
            LightingSettings = new LightingSettings();
            PostProcessingSettings = new PostProcessingSettings(Graphics.Instance.CameraSettings.MainCamera);

            _postProcessingManager = Instance.GetOrAddComponent<PostProcessingManager>();
            _postProcessingManager.Parent = this;
            _postProcessingManager.LensDirtTexturesPath = ConfigLensDirtPath.Value;
           
            DontDestroyOnLoad(_postProcessingManager);

#if AI
            //do nothing
#else
            _decalsSystemManager = new DecalsSystemManager();
            _decalsSystemManager.Initialize();
#endif

            _ctaaManager = new CTAAManager();
            _ctaaManager.Initialize();

            _auraManager = new AuraManager();
            _auraManager.Initialize();

            _sssManager = new SSSManager();
            _sssManager.Initialize();
            SSSMirrorHooks.InitializeMirrorHooks();

            _segiManager = new SEGIManager();
            _segiManager.Initialize();

            _gtaoManager = new GTAOManager();
            _gtaoManager.Initialize();

            _amplifyoccManager = new AmplifyOccManager();
            _amplifyoccManager.Initialize();

            _vaoManager = new VAOManager();
            _vaoManager.Initialize();

            _globalfogManager = new GlobalFogManager();
            _globalfogManager.Initialize();

            _underwaterManager = new LuxWater_UnderWaterRenderingManager();
            _underwaterManager.Initialize();

            _waterVolumeTriggerManager = new LuxWater_WaterVolumeTriggerManager();
            _waterVolumeTriggerManager.Initialize();

            _connectorManager = new ConnectSunToUnderwaterManager();
            _connectorManager.Initialize();

            _underwaterBlur = new LuxWater_UnderwaterBlurManager();
            _underwaterBlur.Initialize();

            _focusManager = new FocusManager();
            _focusManager.Initialize();

            _ditheredshadowsManager = new DitheredShadowsManager();
            _ditheredshadowsManager.Initialize();

            _temporalScreenshotManager = new TemporalScreenshotManager();
            _temporalScreenshotManager.Initialize();

            if (KKAPI.Studio.StudioAPI.InsideStudio)
                smartphoneScanner = this.gameObject.AddComponent<HoohSmartphoneScanner>();

            Log.LogInfo("Mirror Hooks GO");

            if (KKAPI.Studio.StudioAPI.InsideStudio)
                StudioReset.InitializeStudioHooks();

            RemoveDeprecatedEffects();

            //NVIDIA.Ansel ansel = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<Ansel>();
            //Destroy(ansel);
            //Graphics.Instance.CameraSettings.MainCamera.gameObject.AddComponent<NVIDIA.Ansel>();
            //ansel.Start();

            _filmGrainManager = new FilmGrainManager();
            _filmGrainManager.Initialize();

            _skyboxManager = Instance.GetOrAddComponent<SkyboxManager>();
            _skyboxManager.Parent = this;
            _skyboxManager.AssetPath = ConfigCubeMapPath.Value;
            _skyboxManager.Logger = Logger;
            _skyboxManager.Initialize();
            DontDestroyOnLoad(_skyboxManager);

            LocalizationManager.Parent = this;
            LocalizationManager.CurrentLanguage = ConfigLanguage.Value;

            _lightManager = new LightManager(this);
            _lightManager.Light();
            _presetManager = new PresetManager(ConfigPresetPath.Value, this);

            // Load PCSS Assets
            yield return new WaitUntil(PCSSLight.LoadAssets);
            // Load SEGI Assets
            yield return new WaitUntil(SEGI.SEGI.LoadAssets);
            // Set up the reflection shader
            yield return new WaitUntil(LoadAssets);

            _inspector = new Inspector.Inspector(this);
            _isLoaded = true;
            float _fov = CameraSettings.MainCamera.fieldOfView; // Grab initial FOV before we override in the preset
            _presetManager.LoadDefaultForCurrentGameMode();
            CameraSettings.Fov = (float)_fov; // put it back
            CameraSettings.MainCamera.fieldOfView = _fov;

            PostProcessingSettings.UpdateFilterDithering();

            LogWithDots("Graphics", "ONLINE");

            if (KKAPI.Studio.StudioAPI.InsideStudio)
            {
                Texture2D gIconTex = new Texture2D(32, 32);
                byte[] texData = ResourceUtils.GetEmbeddedResource("icon_camera_vsmall.png");
                ImageConversion.LoadImage(gIconTex, texData);
                studioToolbarToggle = KKAPI.Studio.UI.CustomToolbarButtons.AddLeftToolbarToggle(gIconTex, false, active => {
                    Show = active;
                });
            }

            // Straight 2 Maker Support Fix
            if (KKAPI.Maker.MakerAPI.InsideMaker)
            {
                OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            }
        }

        internal KKAPI.Studio.UI.ToolbarToggle studioToolbarToggle;
        internal SkyboxManager SkyboxManager => _skyboxManager;
        internal PostProcessingManager PostProcessingManager => _postProcessingManager;
        internal LightManager LightManager => _lightManager;
        internal PresetManager PresetManager => _presetManager;

        internal static bool LoadAssets()
        {
            AssetBundle assetref = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("defref.unity3d"));
            Shader reflectionShader = assetref.LoadAsset<Shader>("Assets/GTAO/Shaders/GTRO-DeferredReflections.shader");

            UnityEngine.Object.DontDestroyOnLoad(reflectionShader);
            assetref.Unload(false);

            GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredReflections, BuiltinShaderMode.UseCustom);
            GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredReflections, reflectionShader);
            LogWithDots("Deferred Reflections Shader", "INSTALLED");
            return true;
        }

        internal void OnGUI()
        {
            if (Show)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                GUISkin originalSkin = GUI.skin;
                GUI.skin = GUIStyles.Skin;
                _inspector.DrawWindow();
                GUI.skin = originalSkin;                
            }

            if (Event.current?.type == EventType.KeyDown && ConfigShortcut.Value == Event.current?.keyCode)
            {
                ToggleGUI();
            }
        }

        private void RemoveDeprecatedEffects()
        {
            UnityStandardAssets.ImageEffects.DepthOfField old_depthOfField = Graphics.Instance.CameraSettings.MainCamera.GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
            if (old_depthOfField != null)
            {
                DestroyImmediate(old_depthOfField);
            }

            UnityStandardAssets.ImageEffects.SunShafts old_sunShafts = Graphics.Instance.CameraSettings.MainCamera.GetComponent<UnityStandardAssets.ImageEffects.SunShafts>();
            if (old_sunShafts != null)
            {
                DestroyImmediate(old_sunShafts);
            }

            Graphics.Instance.Log.LogInfo("Removed old image effects from Main Camera");
        }

        internal void Update()
        {
            if (!_isLoaded)
            {
                return;
            }

            if (Show)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }           
        }

        internal void LateUpdate()
        {
            if (Show)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }           
        }

        internal bool IsStudio()
        {
            return GameMode.Studio == KoikatuAPI.GetCurrentGameMode();
        }

        // Pulsing Reflection Probes Support
        public float ReflectionProbesPulseTimer { get; set; } = 2.0f;

        private bool pulseReflectionProbes;
        public bool PulseReflectionProbes
        {
            get => pulseReflectionProbes;
            set
            {
                pulseReflectionProbes = value;
                if (pulseReflectionProbes)
                {
                    if (ReflectionProbesPulseTimer == 0.0f)
                        ReflectionProbesPulseTimer = 2.0f;

                    StartPulseRealtimeReflectionCoroutine();
                }
            }
        }

        private bool realtimeReflectionPulseCoroutineRunning = false;
        internal void StartPulseRealtimeReflectionCoroutine()
        {
            if (!realtimeReflectionPulseCoroutineRunning) {                
                StartCoroutine(DoRealtimeReflectionPulse());
                realtimeReflectionPulseCoroutineRunning = true;
            }
        }

        internal IEnumerator DoRealtimeReflectionPulse()
        {
            while (PulseReflectionProbes)
            {
                QualitySettings.realtimeReflectionProbes = true;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                yield return null;
                QualitySettings.realtimeReflectionProbes = false;
                yield return new WaitForSeconds(ReflectionProbesPulseTimer);                
            }
            realtimeReflectionPulseCoroutineRunning = false;
        }

        public void ToggleGUI()
        {
            if (studioToolbarToggle == null)
                Show = !Show;
            else
                //studioToolbarToggle.Toggled.OnNext(!studioToolbarToggle.Toggled.Value);
                Show = !Show;
        }

        public bool Show
        {
            get => _showGUI;
            set
            {
                if (_showGUI != value)
                {
                    if (value)
                    {
#if AI
                        if (KKAPI.KoikatuAPI.GetCurrentGameMode() == KKAPI.GameMode.MainGame)
                        {
                            _previousTimeScale = Time.timeScale;
                            Time.timeScale = 0;
                        }
#endif
                        _previousCursorLockState = Cursor.lockState;
                        _previousCursorVisible = Cursor.visible;
                    }
                    else
                    {
#if AI
                        if (KKAPI.KoikatuAPI.GetCurrentGameMode() == KKAPI.GameMode.MainGame)
                        {
                            Time.timeScale = _previousTimeScale;
                        }
#endif
                        if (!_previousCursorVisible || _previousCursorLockState != CursorLockMode.None)
                        {
                            Cursor.lockState = _previousCursorLockState;
                            Cursor.visible = _previousCursorVisible;
                        }
                    }
                    
                    _showGUI = value;
                }
            }
        }
    }
}
