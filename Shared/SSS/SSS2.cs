using Graphics.CTAA;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.XR;
using static SSS_convolution;

namespace Graphics
{
    [RequireComponent(typeof(Camera))]
    public class SSS : MonoBehaviour
    {
        public enum ToggleTexture
        {
            LightingTex,
            LightingTexBlurred,
            ProfileTex,
            None
        }

        private Shader _lightingPass;
        private Shader _profile;
        private Shader _separableSSS;
        private Camera cam;
        private bool _debugDistance;
        [SerializeField][Range(0, 1)] public float _depthTest = 0.3f;
        private bool _ditherEdgeTest;
        [Range(1, 1.2f)] private float _edgeOffset = 1.1f;
        private bool _fixPixelLeaks;

        private Dictionary<string, GameObject> LightingCameraGOs = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> ProfileCameraGOs = new Dictionary<string, GameObject>();

        private int _maxDistance = 10000;
        private Texture _noiseTexture;
        private bool _profilePerObject;

        public Shader ProfileShader, LightingPassShader;
        [Range(0, 10)] public bool ShowCameras;
        public bool ShowGUI;
        private SSS_convolution sss_convolution;
        private CTAA_PC ctaa;
        public bool MirrorSSS;

        [HideInInspector] public RenderTexture SSS_ProfileTex, SSS_ProfileTexR, LightingTex, LightingTexBlurred, LightingTexR, LightingTexBlurredR;
        private Color _sssColor = Color.yellow;

        public ToggleTexture toggleTexture = ToggleTexture.None;

        [SerializeField][Range(0, 1)] public float ProfileRadiusTest = 0.05f;
        private bool _useProfileTest;

        [SerializeField][Range(0, 1)] private float _normalTest = 0.3f;
        [SerializeField][Range(0, 1)] public float _profileColorTest = 0.05f;
        private int _shaderIterations;
        private bool _dither;
        private float _ditherScale;
        private float _ditherIntensity;

        public bool Enabled { get; set; }

        internal float Downsampling { get; set; }

        internal float ScatteringRadius { get; set; }

        internal int ScatteringIterations { get; set; }

        internal float DepthTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _depthTest;
            set { _depthTest = Mathf.Max(0.0002f, value); sss_convolution?.BlurMaterial.SetFloat("DepthTest", _depthTest * 0.05f); }
        }

        internal int MaxDistance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maxDistance;
            set { _maxDistance = Mathf.Max(0, value); sss_convolution?.BlurMaterial.SetFloat("maxDistance", _maxDistance); }
        }

        internal float NormalTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _normalTest;
            set { _normalTest = Mathf.Max(0.001f, value); sss_convolution?.BlurMaterial.SetFloat("NormalTest", _normalTest); }
        }

        internal float ProfileColorTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _profileColorTest;
            set { _profileColorTest = Mathf.Max(0.001f, value); sss_convolution?.BlurMaterial.SetFloat("ProfileColorTest", _profileColorTest); }
        }

        internal float EdgeOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _edgeOffset;
            set { _edgeOffset = value; sss_convolution?.BlurMaterial.SetFloat("EdgeOffset", _edgeOffset); }
        }

        internal int ShaderIterations
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _shaderIterations;
            set { _shaderIterations = value; sss_convolution?.BlurMaterial.SetInt("_SSS_NUM_SAMPLES", _shaderIterations + 1); }
        }

        internal Color sssColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sssColor;
            set { _sssColor = value; sss_convolution?.BlurMaterial.SetColor("sssColor", _sssColor); }
        }

        internal bool Dither
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dither;
            set { _dither = value; sss_convolution?.BlurMaterial.EnableKeyword("RANDOMIZED_ROTATION", value); }
        }

        internal float DitherScale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ditherScale;
            set { _ditherScale = value; sss_convolution?.BlurMaterial.SetFloat("DitherScale", _ditherScale); }
        }

        internal float DitherIntensity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ditherIntensity;
            set { _ditherIntensity = value; sss_convolution?.BlurMaterial.SetFloat("DitherIntensity", _ditherIntensity); }
        }

        internal Texture NoiseTexture
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _noiseTexture;
            set { _noiseTexture = value; sss_convolution?.BlurMaterial.SetTexture("NoiseTexture", _noiseTexture); }
        }

        internal bool UseProfileTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _useProfileTest;
            set { _useProfileTest = value; sss_convolution?.BlurMaterial.EnableKeyword("PROFILE_TEST", value && _profilePerObject); }
        }

        internal bool ProfilePerObject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _profilePerObject;
            set { _profilePerObject = value; sss_convolution?.BlurMaterial.EnableKeyword("PROFILE_TEST", value && _useProfileTest); }
        }

        internal bool DebugDistance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _debugDistance;
            set { _debugDistance = value; sss_convolution?.BlurMaterial.EnableKeyword("DEBUG_DISTANCE", value); }
        }

        internal bool FixPixelLeaks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fixPixelLeaks;
            set { _fixPixelLeaks = value; sss_convolution?.BlurMaterial.EnableKeyword("OFFSET_EDGE_TEST", value); }
        }

        internal bool DitherEdgeTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ditherEdgeTest;
            set { _ditherEdgeTest = value; sss_convolution?.BlurMaterial.EnableKeyword("DITHER_EDGE_TEST", value); }
        }

        private void CreateCameras(Camera currentCamera, out Camera profileCamera, out Camera lightingCamera)
        {
            profileCamera = null;
            if (ProfilePerObject)
            {
                string profileGOName = $"SSS Profile Camera for {currentCamera.gameObject.name}-{currentCamera.gameObject.GetInstanceID()}";
                if (!ProfileCameraGOs.TryGetValue(profileGOName, out GameObject profileCameraGO) || !profileCameraGO)
                {
                    profileCameraGO = GameObject.Find(profileGOName);
                    if (!profileCameraGO)
                    {
                        profileCameraGO = new GameObject(profileGOName, new Type[] { typeof(Camera) });
                        profileCameraGO.transform.parent = transform;
                        profileCameraGO.transform.localPosition = Vector3.zero;
                        profileCameraGO.transform.localEulerAngles = Vector3.zero;
                        profileCamera = profileCameraGO.GetComponent<Camera>();
                        profileCamera.backgroundColor = Color.black;
                        profileCamera.enabled = false;
                        profileCamera.depth = -254;
                        profileCamera.allowMSAA = false;
                    }

                    ProfileCameraGOs[profileGOName] = profileCameraGO;
                }

                if (!profileCamera)
                {
                    profileCamera = profileCameraGO.GetComponent<Camera>();
                }
                profileCamera.backgroundColor = Color.black;
                profileCamera.depth = -254;
            }

            // Camera for lighting
            lightingCamera = null;

            string lightingGOName = $"SSS Lighting Camera for {currentCamera.gameObject.name}-{currentCamera.gameObject.GetInstanceID()}";
            if (!LightingCameraGOs.TryGetValue(lightingGOName, out GameObject lightingCameraGO) || !lightingCameraGO)
            {
                lightingCameraGO = GameObject.Find(lightingGOName);
                if (!lightingCameraGO)
                {
                    lightingCameraGO = new GameObject(lightingGOName, new Type[] { typeof(Camera) });
                    lightingCameraGO.transform.parent = transform;
                    lightingCameraGO.transform.localPosition = Vector3.zero;
                    lightingCameraGO.transform.localEulerAngles = Vector3.zero;
                    lightingCamera = lightingCameraGO.GetComponent<Camera>();
                    lightingCamera.enabled = false;
                    lightingCamera.depth = -846;
                    ctaa = lightingCameraGO.AddComponent<CTAA_PC>();
                    sss_convolution = lightingCameraGO.AddComponent<SSS_convolution>();
                    sss_convolution.BlurShader = Shader.Find("Hidden/SeparableSSS") ?? _separableSSS;
                    if (sss_convolution.BlurShader)
                    {
                        BlurMaterials blurMaterial = new BlurMaterials(sss_convolution.BlurShader);
                        blurMaterial.hideFlags = HideFlags.HideAndDontSave;
                        blurMaterial.SetFloat("DepthTest", _depthTest * 0.05f);
                        blurMaterial.SetFloat("maxDistance", _maxDistance);
                        blurMaterial.SetFloat("NormalTest", _normalTest);
                        blurMaterial.SetFloat("ProfileColorTest", _profileColorTest);
                        blurMaterial.SetFloat("EdgeOffset", _edgeOffset);
                        blurMaterial.SetInt("_SSS_NUM_SAMPLES", _shaderIterations + 1);
                        blurMaterial.SetColor("sssColor", _sssColor);
                        blurMaterial.EnableKeyword("RANDOMIZED_ROTATION", _dither);
                        blurMaterial.SetFloat("DitherScale", _ditherScale);
                        blurMaterial.SetFloat("DitherIntensity", _ditherIntensity);
                        blurMaterial.SetTexture("NoiseTexture", _noiseTexture);
                        blurMaterial.EnableKeyword("PROFILE_TEST", _useProfileTest);
                        blurMaterial.EnableKeyword("DEBUG_DISTANCE", _debugDistance);
                        blurMaterial.EnableKeyword("OFFSET_EDGE_TEST", _fixPixelLeaks);
                        blurMaterial.EnableKeyword("DITHER_EDGE_TEST", _ditherEdgeTest);
                        sss_convolution.BlurMaterial = blurMaterial;
                    }
                }
                LightingCameraGOs[lightingGOName] = lightingCameraGO;
            }
            if (!lightingCamera)
            {
                lightingCamera = lightingCameraGO.GetComponent<Camera>();
            }
            lightingCamera.allowMSAA = currentCamera.allowMSAA;
            lightingCamera.backgroundColor = currentCamera.backgroundColor;
            lightingCamera.clearFlags = currentCamera.clearFlags;
            lightingCamera.cullingMask = currentCamera.cullingMask;
        }

        private void Awake()
        {
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("sss"));
            _lightingPass = assetBundle.LoadAsset<Shader>("Assets/SSS/Resources/LightingPass.shader");
            _separableSSS = assetBundle.LoadAsset<Shader>("Assets/SSS/Resources/SeparableSSS.shader");
            _profile = assetBundle.LoadAsset<Shader>("Assets/SSS/Resources/SSS_Profile.shader");
            assetBundle.Unload(false);
            NoiseTexture = ResourceUtils.GetEmbeddedResource("BlueNoise256RGB.png").LoadTexture();
            ScatteringRadius = 0.2f;
            Downsampling = 1;
            ScatteringIterations = 5;
            ShaderIterations = 10;
            Enabled = false;
            Dither = true;
            DitherIntensity = 1;
            DitherScale = 0.1f;
        }

        private void OnEnable()
        {
            if (SSS_Layer == 0)
            {
                SSS_Layer = LayerMask.NameToLayer("Chara");
                Graphics.Instance.Log.LogInfo($"Setting SSS layer from Nothing to {LayerMask.LayerToName(SSS_Layer)}");
            }

            //optional
            //Utilities.CreateLayer("SSS pass");
            // SetSSS_Layer(_SSS_LayerName);
            cam = GetComponent<Camera>();

            if (!cam.GetComponent<SSS_buffers_viewer>())
            {
                sss_buffers_viewer = cam.gameObject.AddComponent<SSS_buffers_viewer>();
            }

            if (!sss_buffers_viewer)
            {
                sss_buffers_viewer = cam.gameObject.GetComponent<SSS_buffers_viewer>();
            }

            sss_buffers_viewer.hideFlags = HideFlags.HideAndDontSave;
            //Make things work on load if only scene view is active
            Shader.EnableKeyword("SCENE_VIEW");
            if (ProfilePerObject)
            {
                Shader.EnableKeyword("SSS_PROFILES");
            }
        }

        private void OnPreRender()
        {
            if (Enabled && cam != (object)null)
            {
                Shader.DisableKeyword("SCENE_VIEW");
                LightingPassShader ??= Shader.Find("Hidden/LightingPass") ?? _lightingPass;
                ProfileShader ??= Shader.Find("Hidden/SSS_Profile") ?? _profile;

                Vector2 textureSize;
                if (!cam.stereoEnabled)
                {
                    textureSize.x = cam.pixelWidth / Downsampling;
                    textureSize.y = cam.pixelHeight / Downsampling;
                }
                else
                {
                    textureSize.x = XRSettings.eyeTextureWidth / Downsampling;
                    textureSize.y = XRSettings.eyeTextureHeight / Downsampling;
                }

                CreateCameras(cam, out Camera profileCamera, out Camera lightingCamera);

                #region Render Profile
                if (ProfilePerObject && profileCamera != (object)null)
                {
                    UpdateCameraModes(cam, profileCamera);
                    profileCamera.depth = -254;

                    //ProfileCamera.allowHDR = false;
                    ////humm, removes a lot of artifacts when far away
                    ///

                    profileCamera.allowMSAA = false;
                    int initialpixelLights = QualitySettings.pixelLightCount;
                    ShadowQuality initialShadows = QualitySettings.shadows;
                    QualitySettings.pixelLightCount = 0;
                    QualitySettings.shadows = ShadowQuality.Disable;
                    Shader.EnableKeyword("SSS_PROFILES");
                    profileCamera.cullingMask = SSS_Layer;
                    profileCamera.backgroundColor = Color.black;
                    profileCamera.clearFlags = CameraClearFlags.Color;

                    if (!cam.stereoEnabled)
                    {
                        // Mono
                        GetProfileRT(ref SSS_ProfileTex, (int)textureSize.x, (int)textureSize.y, "SSS_ProfileTex");
                        Util.RenderToTarget(profileCamera, SSS_ProfileTex, ProfileShader);
                        Shader.SetGlobalTexture("SSS_ProfileTex", SSS_ProfileTex);
                        Shader.SetGlobalTexture("SSS_ProfileTexR", SSS_ProfileTex);
                    }
                    else if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                    {
                        // Left eye
                        profileCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                        profileCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                        GetProfileRT(ref SSS_ProfileTex, (int)textureSize.x, (int)textureSize.y, "SSS_ProfileTex");
                        Util.RenderToTarget(profileCamera, SSS_ProfileTex, ProfileShader);
                        Shader.SetGlobalTexture("SSS_ProfileTex", SSS_ProfileTex);
                    }
                    else
                    {
                        profileCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                        profileCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                        GetProfileRT(ref SSS_ProfileTexR, (int)textureSize.x, (int)textureSize.y, "SSS_ProfileTexR");
                        Util.RenderToTarget(profileCamera, SSS_ProfileTexR, ProfileShader);
                        Shader.SetGlobalTexture("SSS_ProfileTexR", SSS_ProfileTexR);
                    }

                    QualitySettings.pixelLightCount = initialpixelLights;
                    QualitySettings.shadows = initialShadows;
                }
                else
                {
                    Shader.DisableKeyword("SSS_PROFILES");

                    SafeDestroy(ref SSS_ProfileTex);
                    SafeDestroy(ref SSS_ProfileTexR);
                }
                #endregion

                #region Render Lighting
                UpdateCameraModes(cam, lightingCamera);
                lightingCamera.allowHDR = cam.allowHDR;

                if (GTAO.GTAOManager.settings.Enabled)
                {
                    if (!lightingCamera.GetComponent<GTAO.GroundTruthAmbientOcclusion>())
                    {
                        GTAO.GroundTruthAmbientOcclusion gtao = lightingCamera.GetOrAddComponent<GTAO.GroundTruthAmbientOcclusion>();
                        GTAO.GTAOManager.RegisterAdditionalInstance(gtao);
                        GTAO.GTAOManager.CopySettingsToOtherInstances();
                    }
                }
                else
                {
                    if (lightingCamera.GetComponent<GTAO.GroundTruthAmbientOcclusion>())
                    {
                        GTAO.GroundTruthAmbientOcclusion gtao = lightingCamera.GetOrAddComponent<GTAO.GroundTruthAmbientOcclusion>();
                        GTAO.GTAOManager.DestroyGTAOInstance(gtao);
                        Destroy(gtao);
                    }
                }
                if (VAO.VAOManager.settings.Enabled)
                {
                    if (!lightingCamera.gameObject.GetComponent<VAO.VAOEffectCommandBuffer>() && !lightingCamera.gameObject.GetComponent<VAO.VAOEffect>())
                    {
                        VAO.VAOEffect vao = lightingCamera.gameObject.AddComponent<VAO.VAOEffect>();
                        VAO.VAOManager.RegisterAdditionalInstance(vao);
                    }
                }
                else
                {
                    if (lightingCamera.gameObject.GetComponent<VAO.VAOEffectCommandBuffer>() || lightingCamera.gameObject.GetComponent<VAO.VAOEffect>())
                    {
                        VAO.VAOEffectCommandBuffer vao = lightingCamera.GetComponent<VAO.VAOEffectCommandBuffer>() ?? lightingCamera.GetComponent<VAO.VAOEffect>();
                        if (vao)
                        {
                            VAO.VAOManager.DestroyVAOInstance(vao);
                            Destroy(vao);
                        }
                    }
                }
                ctaa ??= lightingCamera.gameObject.GetComponent<CTAA.CTAA_PC>();
                ctaa.enabled = CTAAManager.settings.Enabled;
                ctaa.TemporalStability = CTAAManager.settings.TemporalStability.value;
                ctaa.HdrResponse = CTAAManager.settings.HdrResponse.value;
                ctaa.EdgeResponse = CTAAManager.settings.EdgeResponse.value;
                ctaa.AdaptiveSharpness = CTAAManager.settings.AdaptiveSharpness.value;
                ctaa.TemporalJitterScale = CTAAManager.settings.TemporalJitterScale.value;
                ctaa.SupersampleMode = CTAAManager.settings.Mode;

                lightingCamera.depthTextureMode = DepthTextureMode.DepthNormals;
                lightingCamera.backgroundColor = cam.backgroundColor;
                lightingCamera.clearFlags = cam.clearFlags;
                lightingCamera.cullingMask = SSS_Layer;
                lightingCamera.depth = -846;

                sss_convolution ??= lightingCamera.gameObject.GetComponent<SSS_convolution>();

                sss_convolution.iterations = ScatteringIterations;
                if (!cam.stereoEnabled)
                {
                    GetRT(ref LightingTex, (int)textureSize.x, (int)textureSize.y, "LightingTexture");
                    GetRT(ref LightingTexBlurred, (int)textureSize.x, (int)textureSize.y, "SSSLightingTextureBlurred");
                    sss_convolution.BlurRadius = ScatteringRadius / Downsampling;
                    sss_convolution.blurred = LightingTexBlurred;
                    sss_convolution.rtFormat = LightingTex.format;
                    if (LightingPassShader != (object)null)
                    {
                        Util.RenderToTarget(lightingCamera, LightingTex, LightingPassShader);
                        Shader.SetGlobalTexture("LightingTexBlurred", LightingTexBlurred);
                        Shader.SetGlobalTexture("LightingTex", LightingTex);
                        Shader.SetGlobalTexture("LightingTexBlurredR", LightingTexBlurred);
                        Shader.SetGlobalTexture("LightingTexR", LightingTex);
                    }
                }
                else if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                {
                    lightingCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                    lightingCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                    GetRT(ref LightingTex, (int)textureSize.x, (int)textureSize.y, "LightingTexture");
                    GetRT(ref LightingTexBlurred, (int)textureSize.x, (int)textureSize.y, "SSSLightingTextureBlurred");
                    sss_convolution.BlurRadius = ScatteringRadius / Downsampling * 0.5f;
                    sss_convolution.blurred = LightingTexBlurred;
                    sss_convolution.rtFormat = LightingTex.format;
                    if (LightingPassShader != (object)null)
                    {
                        Util.RenderToTarget(lightingCamera, LightingTex, LightingPassShader);
                        Shader.SetGlobalTexture("LightingTexBlurred", LightingTexBlurred);
                        Shader.SetGlobalTexture("LightingTex", LightingTex);
                    }
                }
                else
                {
                    lightingCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                    lightingCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                    GetRT(ref LightingTexR, (int)textureSize.x, (int)textureSize.y, "LightingTextureR");
                    GetRT(ref LightingTexBlurredR, (int)textureSize.x, (int)textureSize.y, "SSSLightingTextureBlurredR");
                    sss_convolution.BlurRadius = ScatteringRadius / Downsampling * 0.5f;
                    sss_convolution.blurred = LightingTexBlurredR;
                    sss_convolution.rtFormat = LightingTexR.format;
                    if (LightingPassShader != (object)null)
                    {
                        Util.RenderToTarget(lightingCamera, LightingTexR, LightingPassShader);
                        Shader.SetGlobalTexture("LightingTexBlurredR", LightingTexBlurredR);
                        Shader.SetGlobalTexture("LightingTexR", LightingTexR);
                    }
                }
                #endregion
            }
            else
            {
                if (sss_buffers_viewer && sss_buffers_viewer.enabled)
                {
                    sss_buffers_viewer.enabled = false;
                }
            }

            #region Debug
            if (sss_buffers_viewer && Enabled)
            {
                switch (toggleTexture)
                {
                    case ToggleTexture.LightingTex:
                        sss_buffers_viewer.InputBuffer = LightingTex;
                        sss_buffers_viewer.enabled = true;
                        return;
                    case ToggleTexture.LightingTexBlurred:
                        sss_buffers_viewer.InputBuffer = LightingTexBlurred;
                        sss_buffers_viewer.enabled = true;
                        return;
                    case ToggleTexture.ProfileTex:
                        sss_buffers_viewer.InputBuffer = SSS_ProfileTex;
                        sss_buffers_viewer.enabled = true;
                        return;
                    case ToggleTexture.None:
                        sss_buffers_viewer.enabled = false;
                        break;
                    default:
                        return;
                }
            }
            #endregion
        }

        private void OnPostRender()
        {
            Shader.EnableKeyword("SCENE_VIEW");

            foreach (var kvp in LightingCameraGOs.Where(kvp => !kvp.Value).ToArray())
                LightingCameraGOs.Remove(kvp.Key);

            foreach (var kvp in ProfileCameraGOs.Where(kvp => !kvp.Value).ToArray())
                ProfileCameraGOs.Remove(kvp.Key);
        }

        private void SafeDestroy(UnityEngine.Object obj)
        {
            if (obj)
            {
                DestroyImmediate(obj);
            }
        }

        private void SafeDestroy<T>(ref T obj) where T : UnityEngine.Object
        {
            if (obj)
            {
                DestroyImmediate(obj);
            }
            obj = null;
        }

        private void Cleanup()
        {
            if (LightingCameraGOs != null)
            {
                foreach (var go in LightingCameraGOs.Values)
                    SafeDestroy(go);
                LightingCameraGOs.Clear();
            }
            if (ProfileCameraGOs != null)
            {
                foreach (var go in ProfileCameraGOs.Values)
                    SafeDestroy(go);
                ProfileCameraGOs.Clear();
            }

            SafeDestroy(ref LightingTex);
            SafeDestroy(ref LightingTexBlurred);
            SafeDestroy(ref LightingTexR);
            SafeDestroy(ref LightingTexBlurredR);
            SafeDestroy(ref SSS_ProfileTex);
            SafeDestroy(ref SSS_ProfileTexR);
        }

        // Cleanup all the objects we possibly have created
        private void OnDisable()
        {
            //Shader.EnableKeyword("UNITY_STEREO_EYE");
            Shader.EnableKeyword("SCENE_VIEW");
            Cleanup();
        }

        #region layer
        [HideInInspector] public LayerMask SSS_Layer;
        private SSS_buffers_viewer sss_buffers_viewer;

        //[SerializeField]
        //[HideInInspector]
        //string _SSS_LayerName = "SSS pass";
        //public string SSS_LayerName
        //{
        //    get { return _SSS_LayerName; }
        //    set
        //    {
        //        if (_SSS_LayerName != value)
        //            SetSSS_Layer(value);
        //    }
        //}

        //void SetSSS_Layer(string NewSSS_LayerName)
        //{
        //    _SSS_LayerName = NewSSS_LayerName;
        //    SSS_Layer = 1 << LayerMask.NameToLayer(_SSS_LayerName);
        //}
        #endregion

        #region RT formats and camera settings
        private void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == (object)null)
            {
                return;
            }

            if (MirrorSSS)
            {
                dest.CopyFrom(src);
            }

            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.stereoTargetEye = src.stereoTargetEye;
            dest.orthographic = src.orthographic;
            dest.aspect = src.aspect;
            dest.renderingPath = RenderingPath.Forward;
            dest.orthographicSize = src.orthographicSize;

            if (src.stereoEnabled || !src.usePhysicalProperties)
            {
                dest.fieldOfView = src.fieldOfView;
            }
            else
            {
                dest.usePhysicalProperties = src.usePhysicalProperties;
                dest.projectionMatrix = src.projectionMatrix;
            }
        }

        protected RenderTextureReadWrite GetRTReadWrite()
        {
            //return RenderTextureReadWrite.Default;
            return !cam.allowHDR ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
        }

        protected RenderTextureFormat GetRTFormat()
        {
            return !cam.allowHDR ? RenderTextureFormat.Default : RenderTextureFormat.ARGBFloat;
        }

        protected void GetRT(ref RenderTexture rt, int x, int y, string name)
        {
            if (x <= 0 || y <= 0)
            {
                return; // Below-equal zero request will crash the game.
            }

            ReleaseRT(ref rt);
            if (cam.allowMSAA && QualitySettings.antiAliasing > 0)
            {
                sss_convolution.AllowMSAA = cam.allowMSAA;
                rt = RenderTexture.GetTemporary(x, y, 24, GetRTFormat(), GetRTReadWrite(), QualitySettings.antiAliasing);
            }
            else
            {
                rt = RenderTexture.GetTemporary(x, y, 24, GetRTFormat(), GetRTReadWrite());
            }

            rt.filterMode = FilterMode.Bilinear;
            //rt.autoGenerateMips = false;
            rt.name = name;
            rt.wrapMode = TextureWrapMode.Clamp;
        }

        protected void GetProfileRT(ref RenderTexture rt, int x, int y, string name)
        {
            ReleaseRT(ref rt);
            //if (cam.allowMSAA && QualitySettings.antiAliasing > 0 && AllowMSAA)
            //    rt = RenderTexture.GetTemporary(x, y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, QualitySettings.antiAliasing);
            //else
            rt = RenderTexture.GetTemporary(x, y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            rt.filterMode = FilterMode.Point;
            rt.autoGenerateMips = false;
            rt.name = name;
            rt.wrapMode = TextureWrapMode.Clamp;
        }

        private void ReleaseRT(ref RenderTexture rt)
        {
            if (rt != (object)null)
            {
                RenderTexture.ReleaseTemporary(rt);
            }
            rt = null;
        }
        #endregion
    }
}
