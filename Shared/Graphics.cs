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
    [BepInIncompatibility("dhhai4mod")]
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(ExtensibleSaveFormat.ExtendedSave.GUID)]
    public partial class Graphics : BaseUnityPlugin
    {
        public const string GUID = "ore.graphics";
        public const string PluginName = "Graphics";
        public const string Version = "2.0.0";

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
            ConfigShortcut = Config.Bind("Config", "Keyboard Shortcut", KeyCode.F5, new ConfigDescription("Keyboard Shortcut"));
            ConfigPresetPath = Config.Bind("Config", "Preset Location", rootFolder + @"\presets\", new ConfigDescription("Where presets are stored"));
            ConfigCubeMapPath = Config.Bind("Config", "Cubemap path", rootFolder + @"\cubemaps\", new ConfigDescription("Where cubemaps are stored"));
            ConfigLensDirtPath = Config.Bind("Config", "Lens dirt texture path", rootFolder + @"\lensdirts\", new ConfigDescription("Where lens dirt textures are stored"));
            //ConfigStarburstPath = Config.Bind("Config", "Starburst texture path", rootFolder + @"\starbursts\", new ConfigDescription("Where Starburst textures are stored"));
            ConfigLocalizationPath = Config.Bind("Config", "Localization path", rootFolder + @"\BepInEx\plugins\Graphics\Resources\", new ConfigDescription("Where localizations are stored"));
            ConfigLanguage = Config.Bind("Config", "Language", LocalizationManager.DefaultLanguage(), new ConfigDescription("Default Language"));
            ConfigFontSize = Config.Bind("Config", "Font Size", 12, new ConfigDescription("Font Size"));
            GUIStyles.FontSize = ConfigFontSize.Value;
            ConfigWindowWidth = Config.Bind("Config", "Window Width", 750, new ConfigDescription("Window Width"));
            Inspector.Inspector.Width = ConfigWindowWidth.Value;
            ConfigWindowHeight = Config.Bind("Config", "Window Height", 1024, new ConfigDescription("Window Height"));
            Inspector.Inspector.Height = ConfigWindowHeight.Value;
            ConfigWindowOffsetX = Config.Bind("Config", "Window Position Offset X", (Screen.width - ConfigWindowWidth.Value) / 2, new ConfigDescription("Window Position Offset X"));
            Inspector.Inspector.StartOffsetX = ConfigWindowOffsetX.Value;
            ConfigWindowOffsetY = Config.Bind("Config", "Window Position Offset Y", (Screen.height - ConfigWindowHeight.Value) / 2, new ConfigDescription("Window Position Offset Y"));
            Inspector.Inspector.StartOffsetY = ConfigWindowOffsetY.Value;
        }

        private void Awake()
        {
            CharacterApi.RegisterExtraBehaviour<CharaController>(GUID);
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneController>(GUID);
            SceneManager.sceneLoaded += OnSceneLoaded;
            StudioHooks.Map_OnLoadAfter();
        }

        private IEnumerator Start()
        {
            Log.LogInfo("Starting");
            if (IsStudio()) StudioHooks.Init();
            yield return new WaitUntil(IsLoaded);
            Settings = new GlobalSettings();
            CameraSettings = new CameraSettings();
            LightingSettings = new LightingSettings();

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

            PostProcessingSettings = new PostProcessingSettings(CameraSettings.MainCamera);
            _postProcessingManager = Instance.GetOrAddComponent<PostProcessingManager>();
            _postProcessingManager.Parent = this;
            _postProcessingManager.LensDirtTexturesPath = ConfigLensDirtPath.Value;
            PostProcessingSettings.UpdateFilterDithering();
            DontDestroyOnLoad(_postProcessingManager);

            _ditheredshadowsManager = new DitheredShadowsManager();
            _ditheredshadowsManager.Initialize();

            _decalsSystemManager = new DecalsSystemManager();
            _decalsSystemManager.Initialize();

            if (KKAPI.Studio.StudioAPI.InsideStudio)
                smartphoneScanner = this.gameObject.AddComponent<HoohSmartphoneScanner>();

            Log.LogInfo("Mirror Hooks GO");

            if (KKAPI.Studio.StudioAPI.InsideStudio)
                StudioReset.InitializeStudioHooks();

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
                studioToolbarToggle.Value = !Show;
        }

        private bool Show
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
