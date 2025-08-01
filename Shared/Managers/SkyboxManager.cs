using Graphics.Settings;
using Graphics.Textures;
using KKAPI.Utilities;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static Graphics.Settings.CameraSettings;
using static Graphics.DebugUtils;

namespace Graphics
{
    //[MessagePackObject(true)]
    //public struct SkyboxParams
    //{
    //    public float exposure;
    //    public float rotation;
    //    public Color tint;
    //    public string selectedCubeMap;
    //    public bool projection;
    //    public float horizon;
    //    public float scale;

    //    public SkyboxParams(float exposure, float rotation, Color tint, string selectedCubeMap, bool projection, float horizon, float scale)
    //    {
    //        this.exposure = exposure;
    //        this.rotation = rotation;
    //        this.tint = tint;
    //        this.selectedCubeMap = selectedCubeMap;
    //        this.projection = projection;
    //        this.horizon = horizon;
    //        this.scale = scale;
    //    }
    //};

    internal class SkyboxManager : TextureManager
    {
        private static readonly int _Exposure = Shader.PropertyToID("_Exposure");
        private static readonly int _Rotation = Shader.PropertyToID("_Rotation");
        private static readonly int _Tint = Shader.PropertyToID("_Tint");
        private static readonly int _Horizon = Shader.PropertyToID("_Horizon");
        private static readonly int _Scale = Shader.PropertyToID("_Scale");
        private static readonly int _Projection = Shader.PropertyToID("_Projection");

        private static readonly Texture2D _defaultPreviewTexture = new Texture2D(128, 128, TextureFormat.ARGB32, false);

        public static SkyboxSettings dynSkyboxSetting;

        public static FourPointGradientSkyboxSetting dynFourPointGradientSettings;
        public static HemisphereGradientSkyboxSetting dynHemisphereGradientSettings;
        public static AIOSkySettings dynAIOSkySetting;
        public static TwoPointColorSkyboxSettings dynTwoPointGradientSettings;
        public static ProceduralSkyboxSettings dynProceduralSkySettings;
        public static GroundProjectionSkyboxSettings groundProjectionSkyboxSettings;


        public SkyboxParams skyboxParams = new SkyboxParams(1f, 0f, new Color32(128, 128, 128, 128), ""/*, false, 0f, 50f*/);
        public Material Skyboxbg { get; set; }
        public Material Skybox { get; set; }

        public Material MapSkybox { get; set; }

        internal static string noCubemap = "No skybox";

        private string selectedCubeMap = noCubemap;

        private ReflectionProbe _probe;
        private GameObject _probeGameObject;

        internal ReflectionProbe DefaultProbe => DefaultReflectionProbe();

        internal Graphics Parent { get; set; }
        internal Camera Camera => Parent.CameraSettings.MainCamera;

        private static AssetBundle replacementBundle;
        private Shader ReplacementShader;

        public bool Update { get; set; }
        public bool PresetUpdate { get; set; }

        internal BepInEx.Logging.ManualLogSource Logger { get; set; }

        private static void LoadImage(Texture2D texture, byte[] tex)
        {
            ImageConversion.LoadImage(texture, tex);
            texture.anisoLevel = 1;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
        }

        public void ReplaceShader()
        {
            if (replacementBundle == null) replacementBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("skyboxground.unity3d"));
            if (ReplacementShader == null) ReplacementShader = replacementBundle.LoadAsset<Shader>("assets/shaders/groundprojection.shader");

            Material newSkybox = new Material(ReplacementShader);
            Material newSkyboxbg = new Material(ReplacementShader);

            newSkybox.CopyPropertiesFromMaterial(Skybox);
            //newSkybox.mainTexture = Skybox.mainTexture;

            newSkyboxbg.CopyPropertiesFromMaterial(Skyboxbg);
            //newSkyboxbg.mainTexture = Skyboxbg.mainTexture;
            GroundProjectionSkyboxSettings projectionSkyboxSettings = new GroundProjectionSkyboxSettings();

            Skybox = newSkybox;
            Skyboxbg = newSkyboxbg;
            projectionSkyboxSettings.Load();
        }

        public void ApplySkybox()
        {
            Parent.LightingSettings.SkyboxSetting = Skybox;
            Parent.LightingSettings.AmbientModeSetting = LightingSettings.AIAmbientMode.Skybox;
            Parent.LightingSettings.ReflectionMode = DefaultReflectionMode.Skybox;
            Skybox sky = Camera.GetOrAddComponent<Skybox>();
            sky.enabled = true;
            sky.material = Skyboxbg;
            Parent.CameraSettings.ClearFlag = AICameraClearFlags.Skybox;
        }

        public void ApplySkyboxParams()
        {
            if (Skyboxbg != null)
            {
                if (Skyboxbg.HasProperty(_Exposure)) Skyboxbg.SetFloat(_Exposure, skyboxParams.exposure);
                if (Skyboxbg.HasProperty(_Rotation)) Skyboxbg.SetFloat(_Rotation, skyboxParams.rotation);
                if (Skyboxbg.HasProperty(_Tint)) Skyboxbg.SetColor(_Tint, skyboxParams.tint);

                //if (Skyboxbg.IsKeywordEnabled("_GROUNDPROJECTION"))
                //{
                //    Skyboxbg.EnableKeyword("_GROUNDPROJECTION");
                //}
                //else
                //{
                //    Skyboxbg.DisableKeyword("_GROUNDPROJECTION");
                //}

                //if (Skyboxbg.HasProperty(_Horizon)) Skyboxbg.SetFloat(_Horizon, skyboxParams.horizon);
                //if (Skyboxbg.HasProperty(_Scale)) Skyboxbg.SetFloat(_Scale, skyboxParams.scale);

            }
            if (Skybox != null)
            {
                if (Skybox.HasProperty(_Exposure)) Skybox.SetFloat(_Exposure, skyboxParams.exposure);
                if (Skybox.HasProperty(_Tint)) Skybox.SetColor(_Tint, skyboxParams.tint);
                if (Skybox.HasProperty(_Rotation)) Skybox.SetFloat(_Rotation, skyboxParams.rotation);

                //if (Skybox.IsKeywordEnabled("_GROUNDPROJECTION"))
                //{
                //    Skybox.EnableKeyword("_GROUNDPROJECTION");
                //}
                //else
                //{
                //    Skybox.DisableKeyword("_GROUNDPROJECTION");
                //}

                //if (Skybox.HasProperty(_Horizon)) Skybox.SetFloat(_Horizon, skyboxParams.horizon);
                //if (Skybox.HasProperty(_Scale)) Skybox.SetFloat(_Scale, skyboxParams.scale);
            }
        }
        public void SaveMapSkyBox()
        {
            //Skybox sky = camera.GetComponent<Skybox>();
            //MapSkybox = null == sky ? RenderSettings.skybox : sky.material;
            //MapSkybox = RenderSettings.skybox;
            MapSkybox = Parent.LightingSettings.SkyboxSetting;
        }
        bool Loading = false;
        public void LoadSkyboxParams()
        {
            Loading = true;
            Exposure = skyboxParams.exposure;
            Tint = skyboxParams.tint;
            Rotation = skyboxParams.rotation;
            CurrentTexturePath = skyboxParams.selectedCubeMap;
            //Projection = skyboxParams.projection;
            //Horizon = skyboxParams.horizon;
            //Scale = skyboxParams.scale;
        }

        public void SaveSkyboxParams()
        {
            skyboxParams.exposure = Exposure;
            skyboxParams.tint = Tint;
            skyboxParams.rotation = Rotation;
            skyboxParams.selectedCubeMap = CurrentTexturePath;
            //skyboxParams.projection = Projection;
            //skyboxParams.horizon = Horizon;
            //skyboxParams.scale = Scale;
        }
        public void TurnOffCubeMap(Camera camera)
        {
            //RenderSettings.skybox = MapSkybox;
            Parent.LightingSettings.SkyboxSetting = MapSkybox;
            Skybox sky = camera.GetComponent<Skybox>();
            if (null != sky)
            {
                Destroy(sky);
            }

            if (null == MapSkybox)
            {
                //Parent.LightingSettings.AmbientModeSetting = LightingSettings.AIAmbientMode.Trilight;
                Parent.CameraSettings.ClearFlag = AICameraClearFlags.Colour;
            }
            MapSkybox = null;
        }
        public float Exposure
        {
            get => Skybox.GetFloat(_Exposure);
            set
            {
                Skybox?.SetFloat(_Exposure, value);
                Skyboxbg?.SetFloat(_Exposure, value);
                skyboxParams.exposure = value;
            }
        }
        public Color Tint
        {
            get => Skybox.GetColor(_Tint);
            set
            {
                Skybox?.SetColor(_Tint, value);
                Skyboxbg?.SetColor(_Tint, value);
                skyboxParams.tint = value;
            }
        }
        public float Rotation
        {
            get => Skybox.GetFloat(_Rotation);
            set
            {
                Skyboxbg?.SetFloat(_Rotation, value);
                Skybox?.SetFloat(_Rotation, value);
                skyboxParams.rotation = value;
            }
        }

        //public float Scale
        //{
        //    get => Skybox.GetFloat(_Scale);
        //    set
        //    {
        //        Skyboxbg?.SetFloat(_Scale, value);
        //        Skybox?.SetFloat(_Scale, value);
        //        skyboxParams.scale = value;
        //    }
        //}

        //public float Horizon
        //{
        //    get => Skybox.GetFloat(_Horizon);
        //    set
        //    {
        //        Skyboxbg?.SetFloat(_Horizon, value);
        //        Skybox?.SetFloat(_Horizon, value);
        //        skyboxParams.horizon = value;
        //    }
        //}

        //public bool Projection
        //{
        //    get => Skybox.IsKeywordEnabled("_GROUNDPROJECTION");
        //    set
        //    {
        //        if (value)
        //        {
        //            Skybox?.EnableKeyword("_GROUNDPROJECTION");
        //            Skyboxbg?.EnableKeyword("_GROUNDPROJECTION");
        //        }
        //        else
        //        {
        //            Skybox?.DisableKeyword("_GROUNDPROJECTION");
        //            Skyboxbg?.DisableKeyword("_GROUNDPROJECTION");
        //        }
        //        skyboxParams.projection = value;

        //    }
        //}

        internal override IEnumerator LoadTexture(string filePath, Action<Texture> _)
        {
            SkyboxManager skyboxManager = this;

            if (filePath == "") yield break;

            if (filePath.Contains(".cube"))
            {
                int num = filePath.LastIndexOf('\\');
                if (num == -1)
                    num = filePath.LastIndexOf('/');
                string fileSearchPattern = filePath.Substring(num + 1);

                LogWithDots("Skybox", Path.GetFileNameWithoutExtension(fileSearchPattern));
                List<string> files = Util.GetFiles(skyboxManager.AssetPath, fileSearchPattern);
                if (files.Count != 0)
                {
                    filePath = files[0];
                }
                else
                {
                    yield break;
                }
            }

            yield return new WaitUntil(() => HasAssetsLoaded); // Check if preview is loading the bundle.
            AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
            yield return assetBundleCreateRequest;
            AssetBundle cubemapbundle = assetBundleCreateRequest.assetBundle;
            AssetBundleRequest bundleRequest = assetBundleCreateRequest.assetBundle.LoadAssetAsync<Material>("skybox");
            yield return bundleRequest;
            Skybox = bundleRequest.asset as Material;
            AssetBundleRequest bundleRequestBG = assetBundleCreateRequest.assetBundle.LoadAssetAsync<Material>("skybox-bg");
            yield return bundleRequestBG;
            Skyboxbg = bundleRequestBG.asset as Material;
            if (Skyboxbg == null)
            {
                Skyboxbg = Skybox;
            }

            cubemapbundle.Unload(false);
            cubemapbundle = null;
            bundleRequestBG = null;
            bundleRequest = null;
            assetBundleCreateRequest = null;
            if (Skybox.shader.name == "Skybox/Cubemap")
            {
                ReplaceShader();
            }
            ApplySkybox();
            yield return null;
            ApplySkyboxParams();

            Update = true;
            Resources.UnloadUnusedAssets();
        }

        internal IEnumerator LoadTextureHook(string filePath, Action<Texture> _, bool Loading)
        {
            yield return LoadTexture(filePath, _);

            if (!Loading)
            {
                // dynSkyboxSetting is only being used for setting up parameters from preset after assetbundle loading.
                if (dynAIOSkySetting == null)
                    dynAIOSkySetting = new AIOSkySettings();
                dynAIOSkySetting.Save();

                if (dynHemisphereGradientSettings == null)
                    dynHemisphereGradientSettings = new HemisphereGradientSkyboxSetting();
                dynHemisphereGradientSettings.Save();

                if (dynFourPointGradientSettings == null)
                    dynFourPointGradientSettings = new FourPointGradientSkyboxSetting();
                dynFourPointGradientSettings.Save();

                if (dynTwoPointGradientSettings == null)
                    dynTwoPointGradientSettings = new TwoPointColorSkyboxSettings();
                dynTwoPointGradientSettings.Save();

                if (dynProceduralSkySettings == null)
                    dynProceduralSkySettings = new ProceduralSkyboxSettings();
                dynProceduralSkySettings.Save();

                if (groundProjectionSkyboxSettings == null)
                    groundProjectionSkyboxSettings = new GroundProjectionSkyboxSettings();
                groundProjectionSkyboxSettings.Save();
            }
            UpdateAIOSkySettings();
            UpdateHemisphereGradientSettings();
            UpdateFourPointGradientSettings();
            UpdateTwoPointGradientSettings();
            UpdateProceduralSkySettings();
            UpdateGroundProjectionSkySettings();
        }

        //internal string CurrentCubeMap
        internal override string CurrentTexturePath
        {
            get => selectedCubeMap;
            set
            {
                if (null == value)
                {
                    Loading = false;
                    return;
                }

                if (value.All(char.IsWhiteSpace))
                {
                    value = noCubemap;
                }

                //if cubemap is changed
                if (value != selectedCubeMap || PresetUpdate)
                {
                    //switch off cubemap
                    if (noCubemap == value)
                    {
                        TurnOffCubeMap(Camera);
                        if (KKAPI.GameMode.Maker == KKAPI.KoikatuAPI.GetCurrentGameMode())
                        {
                            ToggleCharaMakerBG(true);
                        }

                        Update = true;
                    }
                    else
                    {
                        //if current skybox isn't set to custom cubemap
                        if (noCubemap == selectedCubeMap)
                        {
                            //TODO - need to save cubemap from Map when Map changes too!
                            if (null != Parent.LightingSettings.SkyboxSetting && "skybox" != Parent.LightingSettings.SkyboxSetting.name)//CubeMapNames.IndexOf(RenderSettings.skybox.name))
                            {
                                //save the skybox in scene/map
                                SaveMapSkyBox();
                            }
                        }
                        if (KKAPI.GameMode.Maker == KKAPI.KoikatuAPI.GetCurrentGameMode())
                        {
                            ToggleCharaMakerBG(false);
                        }
                        StartCoroutine(LoadTextureHook(value, null, Loading));
                    }
                    selectedCubeMap = value;
                    skyboxParams.selectedCubeMap = value;
                    PresetUpdate = false;
                }
                Loading = false;
            }
        }

        internal override string SearchPattern { get => "*.cube"; set => throw new System.NotImplementedException(); }
        internal override Texture CurrentTexture { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        internal override IEnumerator LoadPreview(string filePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string pngFilePath = Path.Combine(Path.GetDirectoryName(filePath), fileNameWithoutExtension + ".png");


            if (!File.Exists(pngFilePath))
            {
                AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
                yield return assetBundleCreateRequest;
                AssetBundle cubemapbundle = assetBundleCreateRequest?.assetBundle;
                AssetBundleRequest bundleRequest = assetBundleCreateRequest?.assetBundle?.LoadAssetAsync<Cubemap>("skybox");
                yield return bundleRequest;
                if (null == bundleRequest || null == bundleRequest.asset)
                {
                    _assetsToLoad--;
                    yield break;
                }
                Cubemap cubemap = bundleRequest.asset as Cubemap;
                Texture2D texture = new Texture2D(cubemap.width, cubemap.height);
                Color[] CubeMapColors = cubemap.GetPixels(CubemapFace.PositiveX);
                texture.SetPixels(CubeMapColors);
                Util.ResizeTexture(texture, 128, 128, true);

                //Save to PNG
                byte[] TextureBytes = texture.EncodeToPNG();
                File.WriteAllBytes(pngFilePath, TextureBytes);

                Previews.Add(texture);
                TexturePaths.Add(filePath);
                cubemapbundle.Unload(false);
                cubemapbundle = null;
                bundleRequest = null;
                assetBundleCreateRequest = null;
                CubeMapColors = null;
                texture = null;
                //_assetsToLoad--;
                _assetsToLoad--;
                Graphics.Instance.Log.LogMessage($"Preview for {filePath} generated.");
            }
            else
            {
                byte[] textureByte = File.ReadAllBytes(pngFilePath);
                Texture2D texture = KKAPI.Utilities.TextureUtils.LoadTexture(textureByte);
                Util.ResizeTexture(texture, 128, 128, false);

                Previews.Add(texture);
                TexturePaths.Add(filePath);
                texture = null;
                _assetsToLoad--;

            }
        }

        internal ReflectionProbe DefaultReflectionProbe()
        {
            if (null == _probeGameObject || null == _probe)
            {
                SetupDefaultReflectionProbe(Graphics.Instance.LightingSettings);
            }

            return _probe;
        }

        internal void SetupDefaultReflectionProbe(LightingSettings lights, bool forceDefaultCreation = false)
        {
            ReflectionProbe[] rps = GetReflectinProbes();

            bool needDefaultProbe = !(rps.Where(probe => probe.mode == ReflectionProbeMode.Realtime && probe != _probe).ToArray().Length > 0);
            if (needDefaultProbe || forceDefaultCreation)
            {
                if (null == _probeGameObject || null == _probe)
                {
                    Graphics.Instance.Log.LogInfo($"Creating Default Reflection Probe");
                    _probeGameObject = new GameObject("Default Reflection Probe");
                    _probe = _probeGameObject.GetOrAddComponent<ReflectionProbe>();
                    _probe.mode = ReflectionProbeMode.Realtime;
                    DontDestroyOnLoad(_probeGameObject);
                    DontDestroyOnLoad(_probe);
                }

                if (lights.DefaultReflectionProbeSettings != null)
                {
                    lights.DefaultReflectionProbeSettings.ApplySettings(_probe);
                }
                else
                {
                    _probe.boxProjection = false;
                    _probe.intensity = 1f;
                    _probe.importance = 0;
                    _probe.resolution = 512;
                    _probe.backgroundColor = Color.white;
                    _probe.hdr = true;
                    _probe.clearFlags = ReflectionProbeClearFlags.Skybox;
                    _probe.cullingMask = Camera.cullingMask;
                    _probe.size = new Vector3(100, 100, 100);
                    _probe.nearClipPlane = 0.01f;
                    _probe.transform.position = new Vector3(0, 0, 0);
                    _probe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
                    _probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
                    lights.DefaultReflectionProbeSettings = new ReflectionProbeSettings();
                    lights.DefaultReflectionProbeSettings.FillSettings(_probe);
                }
            }
            else if (_probeGameObject != null && _probe != null)
            {
                Graphics.Instance.Log.LogInfo($"Disabling Default Reflection Probe");
                DestroyImmediate(_probeGameObject);
            }
        }

        internal ReflectionProbe[] GetReflectinProbes()
        {
            return GameObject.FindObjectsOfType<ReflectionProbe>();
        }

        public IEnumerator UpdateEnvironment()//BepInEx.Logging.ManualLogSource logger)
        {
            while (true)
            {
                yield return null;
                if (Update)
                {
                    DynamicGI.UpdateEnvironment();
                    ReflectionProbe[] rps = GetReflectinProbes();
                    for (int i = 0; i < rps.Length; i++)
                    {
                        rps[i].RenderProbe();
                    }
                    Update = false;
                }
            }
        }

        internal static void ToggleCharaMakerBG(bool active)
        {
            CharaCustom.CharaCustom characustom = GameObject.FindObjectOfType<CharaCustom.CharaCustom>();
            if (null == characustom)
            {
                return;
            }

            Transform bgt = characustom.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "p_ai_mi_createBG00_00");
            bgt?.gameObject.SetActive(active);
        }

        internal void Start()
        {
            SetupDefaultReflectionProbe(Graphics.Instance.LightingSettings);
            StartCoroutine(UpdateEnvironment());
        }

        internal void Initialize()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (dynAIOSkySetting == null)
            {
                dynAIOSkySetting = new AIOSkySettings();
            }
            dynAIOSkySetting.Load();

            if (dynHemisphereGradientSettings == null)
            {
                dynHemisphereGradientSettings = new HemisphereGradientSkyboxSetting();
            }
            dynHemisphereGradientSettings.Load();

            if (dynFourPointGradientSettings == null)
            {
                dynFourPointGradientSettings = new FourPointGradientSkyboxSetting();
            }
            dynFourPointGradientSettings.Load();

            if (dynTwoPointGradientSettings == null)
            {
                dynTwoPointGradientSettings = new TwoPointColorSkyboxSettings();
            }
            dynTwoPointGradientSettings.Load();

            if (dynProceduralSkySettings == null)
            {
                dynProceduralSkySettings = new ProceduralSkyboxSettings();
            }
            dynProceduralSkySettings.Load();

            if (groundProjectionSkyboxSettings == null)
            {
                groundProjectionSkyboxSettings = new GroundProjectionSkyboxSettings();
            }
            groundProjectionSkyboxSettings.Load();
        }

        public static void UpdateAIOSkySettings()
        {
            if (dynAIOSkySetting == null)
                dynAIOSkySetting = new AIOSkySettings();

            dynAIOSkySetting.Load();
        }
        public static void UpdateHemisphereGradientSettings()
        {
            if (dynHemisphereGradientSettings == null)
                dynHemisphereGradientSettings = new HemisphereGradientSkyboxSetting();

            dynHemisphereGradientSettings.Load();
        }
        public static void UpdateFourPointGradientSettings()
        {
            if (dynFourPointGradientSettings == null)
                dynFourPointGradientSettings = new FourPointGradientSkyboxSetting();

            dynFourPointGradientSettings.Load();
        }
        public static void UpdateTwoPointGradientSettings()
        {
            if (dynTwoPointGradientSettings == null)
                dynTwoPointGradientSettings = new TwoPointColorSkyboxSettings();

            dynTwoPointGradientSettings.Load();
        }
        public static void UpdateProceduralSkySettings()
        {
            if (dynProceduralSkySettings == null)
                dynProceduralSkySettings = new ProceduralSkyboxSettings();

            dynProceduralSkySettings.Load();
        }

        public static void UpdateGroundProjectionSkySettings()
        {
            if (groundProjectionSkyboxSettings == null)
                groundProjectionSkyboxSettings = new GroundProjectionSkyboxSettings();

            groundProjectionSkyboxSettings.Load();
        }
    }
}
