using ADV.Backup;
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
            LightingTex = 1,
            LightingTexBlurred = 2,
            ProfileTex = 3,
            None = 0
        }

        private Camera cam;
        //public Camera lightingCamera;
        //private Camera profileCamera;
        private Camera shadowCamera;

        private bool _debugDistance;
        [SerializeField][Range(0, 1)] public float _depthTest = 0.3f;
        private bool _ditherEdgeTest;
        [Range(1, 1.2f)] private float _edgeOffset = 1.1f;
        private bool _fixPixelLeaks;

        private Dictionary<string, GameObject> LightingCameraGOs = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> ProfileCameraGOs = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> ShadowCameraGOs = new Dictionary<string, GameObject>();

        private int _maxDistance = 10000;
        private Texture _noiseTexture;
        private bool _profilePerObject;

        public Shader ProfileShader, LightingPassShader, SeparableSSSShader;
        [Range(0, 10)] public bool ShowCameras;
        public bool ShowGUI;
        public SSS_convolution sss_convolution;
        private CTAA_PC ctaa;
        //private NVIDIA.Ansel ansel;
        //private SEGI.SEGI segi;

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

        static readonly int _SSS_ProfileTexId = Shader.PropertyToID("SSS_ProfileTex");
        static readonly int _SSS_ProfileTexRId = Shader.PropertyToID("SSS_ProfileTexR");
        static readonly int _LightingTexId = Shader.PropertyToID("LightingTex");
        static readonly int _LightingTexBlurredId = Shader.PropertyToID("LightingTexBlurred");
        static readonly int _LightingTexRId = Shader.PropertyToID("LightingTexR");
        static readonly int _LightingTexBlurredRId = Shader.PropertyToID("LightingTexBlurredR");
        static readonly int _DepthTestId = Shader.PropertyToID("DepthTest");
        static readonly int _maxDistanceId = Shader.PropertyToID("maxDistance");
        static readonly int _NormalTestId = Shader.PropertyToID("NormalTest");
        static readonly int _ProfileColorTestId = Shader.PropertyToID("ProfileColorTest");
        static readonly int _EdgeOffsetId = Shader.PropertyToID("EdgeOffset");
        static readonly int _sssColorId = Shader.PropertyToID("sssColor");
        static readonly int _DitherScaleId = Shader.PropertyToID("DitherScale");
        static readonly int _DitherIntensityId = Shader.PropertyToID("DitherIntensity");
        static readonly int _NoiseTextureId = Shader.PropertyToID("NoiseTexture");
        static readonly int _SSS_NUM_SAMPLESID = Shader.PropertyToID("_SSS_NUM_SAMPLES");

        static readonly string _SCENE_VIEW = "SCENE_VIEW";
        static readonly string _SSS_PROFILES = "SSS_PROFILES";
        static readonly string _PROFILE_TEST = "PROFILE_TEST";
        static readonly string _DEBUG_DISTANCE = "DEBUG_DISTANCE";
        static readonly string _RANDOMIZED_ROTATION = "RANDOMIZED_ROTATION";
        static readonly string _DITHER_EDGE_TEST = "DITHER_EDGE_TEST";
        static readonly string _OFFSET_EDGE_TEST = "OFFSET_EDGE_TEST";

        #region layer

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

        internal float Downsampling { get; set; }
        internal float ScatteringRadius { get; set; }
        internal int ScatteringIterations { get; set; }
        internal LayerMask SSS_Layer { get; set; }

        internal float DepthTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _depthTest;
            set
            {
                _depthTest = Mathf.Max(0.0002f, value);
                sss_convolution?.BlurMaterial.SetFloat(_DepthTestId, _depthTest * 0.05f);
            }
        }

        internal int maxDistance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maxDistance;
            set
            {
                _maxDistance = Mathf.Max(0, value);
                sss_convolution?.BlurMaterial.SetFloat(_maxDistanceId, _maxDistance);
            }
        }

        internal float NormalTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _normalTest;
            set { _normalTest = Mathf.Max(0.001f, value); sss_convolution?.BlurMaterial.SetFloat(_NormalTestId, _normalTest); }
        }

        internal float ProfileColorTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _profileColorTest;
            set { _profileColorTest = Mathf.Max(0.001f, value); sss_convolution?.BlurMaterial.SetFloat(_ProfileColorTestId, _profileColorTest); }
        }

        internal float EdgeOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _edgeOffset;
            set { _edgeOffset = value; sss_convolution?.BlurMaterial.SetFloat(_EdgeOffsetId, _edgeOffset); }
        }

        internal int ShaderIterations
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _shaderIterations;
            set { _shaderIterations = value; sss_convolution?.BlurMaterial.SetInt(_SSS_NUM_SAMPLESID, _shaderIterations + 1); }
        }

        internal Color sssColor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sssColor;
            set { _sssColor = value; sss_convolution?.BlurMaterial.SetColor(_sssColorId, _sssColor); }
        }

        internal bool Dither
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dither;
            set { _dither = value; sss_convolution?.BlurMaterial.EnableKeyword(_RANDOMIZED_ROTATION, value); }
        }

        internal float DitherScale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ditherScale;
            set { _ditherScale = value; sss_convolution?.BlurMaterial.SetFloat(_DitherScaleId, _ditherScale); }
        }

        internal float DitherIntensity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ditherIntensity;
            set { _ditherIntensity = value; sss_convolution?.BlurMaterial.SetFloat(_DitherIntensityId, _ditherIntensity); }
        }

        internal Texture NoiseTexture
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _noiseTexture;
            set { _noiseTexture = value; sss_convolution?.BlurMaterial.SetTexture(_NoiseTextureId, _noiseTexture); }
        }

        internal bool UseProfileTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _useProfileTest;
            set { _useProfileTest = value; sss_convolution?.BlurMaterial.EnableKeyword(_PROFILE_TEST, value && _profilePerObject); }
        }

        internal bool ProfilePerObject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _profilePerObject;
            set { _profilePerObject = value; sss_convolution?.BlurMaterial.EnableKeyword(_PROFILE_TEST, value && _useProfileTest); }
        }

        internal bool DebugDistance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _debugDistance;
            set { _debugDistance = value; sss_convolution?.BlurMaterial.EnableKeyword(_DEBUG_DISTANCE, value); }
        }

        internal bool FixPixelLeaks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _fixPixelLeaks;
            set { _fixPixelLeaks = value; sss_convolution?.BlurMaterial.EnableKeyword(_OFFSET_EDGE_TEST, value); }
        }

        internal bool DitherEdgeTest
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ditherEdgeTest;
            set { _ditherEdgeTest = value; sss_convolution?.BlurMaterial.EnableKeyword(_DITHER_EDGE_TEST, value); }
        }

        private void Awake()
        {
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("sss"));
            LightingPassShader = assetBundle.LoadAsset<Shader>("Assets/SSS/Resources/LightingPass.shader");
            SeparableSSSShader = assetBundle.LoadAsset<Shader>("Assets/SSS/Resources/SeparableSSS.shader");
            ProfileShader = assetBundle.LoadAsset<Shader>("Assets/SSS/Resources/SSS_Profile.shader");
            assetBundle.Unload(false);
            NoiseTexture = ResourceUtils.GetEmbeddedResource("BlueNoise256RGB.png").LoadTexture();
            //    ScatteringRadius = 0.2f;
            //    Downsampling = 1;
            //    ScatteringIterations = 5;
            //    ShaderIterations = 10;
            //    Enabled = false;
            //    Dither = true;
            //    DitherIntensity = 1;
            //    DitherScale = 0.1f;
        }

        private void OnEnable()
        {
            SSS_Layer = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Chara")) | (1 << LayerMask.NameToLayer("Map"));

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
            Shader.EnableKeyword(_SCENE_VIEW);
            if (ProfilePerObject)
            {
                Shader.EnableKeyword(_SSS_PROFILES);
            }

            if (sss_convolution == null)
            {
                sss_convolution = cam.gameObject.GetComponent<SSS_convolution>();
            }

        }

        private void OnPreRender()
        {
            if (enabled && cam != (object)null)
            {
                Shader.DisableKeyword(_SCENE_VIEW);
                //LightingPassShader ??= Shader.Find("Hidden/LightingPass") ?? _lightingPass;
                //ProfileShader ??= Shader.Find("Hidden/SSS_Profile") ?? _profile;

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
                    Shader.EnableKeyword(_SSS_PROFILES);
                    profileCamera.cullingMask = SSS_Layer;
                    profileCamera.backgroundColor = Color.black;
                    profileCamera.clearFlags = CameraClearFlags.Color;

                    if (!cam.stereoEnabled)
                    {
                        // Mono
                        GetProfileRT(ref SSS_ProfileTex, (int)textureSize.x, (int)textureSize.y, "SSS_ProfileTex");
                        Util.RenderToTarget(profileCamera, SSS_ProfileTex, ProfileShader);
                        Shader.SetGlobalTexture(_SSS_ProfileTexId, SSS_ProfileTex);
                        Shader.SetGlobalTexture(_SSS_ProfileTexRId, SSS_ProfileTex);
                    }
                    else if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                    {
                        // Left eye
                        profileCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                        profileCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                        GetProfileRT(ref SSS_ProfileTex, (int)textureSize.x, (int)textureSize.y, "SSS_ProfileTex");
                        Util.RenderToTarget(profileCamera, SSS_ProfileTex, ProfileShader);
                        Shader.SetGlobalTexture(_SSS_ProfileTexId, SSS_ProfileTex);
                    }
                    else
                    {
                        profileCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                        profileCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                        GetProfileRT(ref SSS_ProfileTexR, (int)textureSize.x, (int)textureSize.y, "SSS_ProfileTexR");
                        Util.RenderToTarget(profileCamera, SSS_ProfileTexR, ProfileShader);
                        Shader.SetGlobalTexture(_SSS_ProfileTexRId, SSS_ProfileTexR);
                    }

                    QualitySettings.pixelLightCount = initialpixelLights;
                    QualitySettings.shadows = initialShadows;
                }
                else
                {
                    Shader.DisableKeyword(_SSS_PROFILES);

                    SafeDestroy(ref SSS_ProfileTex);
                    SafeDestroy(ref SSS_ProfileTexR);
                }
                #endregion

                #region Render Lighting
                UpdateCameraModes(cam, lightingCamera);
                lightingCamera.allowHDR = cam.allowHDR;

                lightingCamera.depthTextureMode = DepthTextureMode.DepthNormals;
                lightingCamera.backgroundColor = cam.backgroundColor;
                lightingCamera.clearFlags = cam.clearFlags;
                lightingCamera.cullingMask = SSS_Layer;
                lightingCamera.depth = -846;

                //if(!sss_convolution)
                //{
                //    sss_convolution = lightingCamera.gameObject.GetComponent<SSS_convolution>();
                //}

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
                        Shader.SetGlobalTexture(_LightingTexBlurredId, LightingTexBlurred);
                        Shader.SetGlobalTexture(_LightingTexId, LightingTex);
                        Shader.SetGlobalTexture(_LightingTexBlurredRId, LightingTexBlurred);
                        Shader.SetGlobalTexture(_LightingTexRId, LightingTex);
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
                        Shader.SetGlobalTexture(_LightingTexBlurredId, LightingTexBlurred);
                        Shader.SetGlobalTexture(_LightingTexId, LightingTex);
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
                        Shader.SetGlobalTexture(_LightingTexBlurredRId, LightingTexBlurredR);
                        Shader.SetGlobalTexture(_LightingTexRId, LightingTexR);
                    }
                }
                #endregion

                UpdateShadowCamera(cam, shadowCamera);
            }
            else
            {
                if (sss_buffers_viewer && sss_buffers_viewer.enabled)
                {
                    sss_buffers_viewer.enabled = false;
                }
            }

            #region Debug
            if (sss_buffers_viewer && enabled)
            {
                switch (toggleTexture)
                {
                    case ToggleTexture.LightingTex:
                        if (!cam.stereoEnabled)
                        {
                            sss_buffers_viewer.InputBuffer = LightingTex;
                        }
                        else if (cam.stereoEnabled && cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                        {
                            sss_buffers_viewer.InputBuffer = LightingTex;
                        }
                        else
                        {
                            sss_buffers_viewer.InputBuffer = LightingTexR;
                        }
                        sss_buffers_viewer.enabled = true;
                        return;
                    case ToggleTexture.LightingTexBlurred:
                        if (!cam.stereoEnabled)
                        {
                            sss_buffers_viewer.InputBuffer = LightingTexBlurred;
                        }
                        else if (cam.stereoEnabled && cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                        {
                            sss_buffers_viewer.InputBuffer = LightingTexBlurred;
                        }
                        else
                        {
                            sss_buffers_viewer.InputBuffer = LightingTexBlurredR;
                        }
                        sss_buffers_viewer.enabled = true;
                        return;
                    case ToggleTexture.ProfileTex:
                        if (!cam.stereoEnabled)
                        {
                            sss_buffers_viewer.InputBuffer = SSS_ProfileTex;
                        }
                        else if (cam.stereoEnabled && cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                        {
                            sss_buffers_viewer.InputBuffer = SSS_ProfileTex;
                        }
                        else
                        {
                            sss_buffers_viewer.InputBuffer = SSS_ProfileTexR;
                        }
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
            Shader.EnableKeyword(_SCENE_VIEW);

            foreach (var kvp in LightingCameraGOs.Where(kvp => !kvp.Value).ToArray())
                LightingCameraGOs.Remove(kvp.Key);

            foreach (var kvp in ProfileCameraGOs.Where(kvp => !kvp.Value).ToArray())
                ProfileCameraGOs.Remove(kvp.Key);

            foreach (var kvp in ShadowCameraGOs.Where(kvp => !kvp.Value).ToArray())
                ShadowCameraGOs.Remove(kvp.Key);
        }

        private void OnDisable()
        {
            //Shader.EnableKeyword("UNITY_STEREO_EYE");
            Shader.EnableKeyword(_SCENE_VIEW);
            Cleanup();
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

                    if (!ctaa)
                    {
                        ctaa = lightingCameraGO.AddComponent<CTAA_PC>();
                        ctaa.enabled = CTAAManager.settings.Enabled;
                    }

                    if (!sss_convolution)
                        sss_convolution = lightingCameraGO.AddComponent<SSS_convolution>();

                    if (!sss_convolution.BlurShader)
                        sss_convolution.BlurShader = SeparableSSSShader;

                    if (sss_convolution.BlurShader)
                    {
                        BlurMaterials blurMaterial = new BlurMaterials(sss_convolution.BlurShader);

                        blurMaterial.hideFlags = HideFlags.HideAndDontSave;
                        blurMaterial.SetFloat(_DepthTestId, _depthTest * 0.05f);
                        blurMaterial.SetFloat(_maxDistanceId, _maxDistance);
                        blurMaterial.SetFloat(_NormalTestId, _normalTest);
                        blurMaterial.SetFloat(_ProfileColorTestId, _profileColorTest);
                        blurMaterial.SetFloat(_EdgeOffsetId, _edgeOffset);
                        blurMaterial.SetInt(_SSS_NUM_SAMPLESID, _shaderIterations + 1);
                        blurMaterial.SetColor(_sssColorId, _sssColor);
                        blurMaterial.EnableKeyword(_RANDOMIZED_ROTATION, _dither);
                        blurMaterial.SetFloat(_DitherScaleId, _ditherScale);
                        blurMaterial.SetFloat(_DitherIntensityId, _ditherIntensity);
                        blurMaterial.SetTexture(_NoiseTextureId, _noiseTexture);
                        blurMaterial.EnableKeyword(_PROFILE_TEST, _useProfileTest);
                        blurMaterial.EnableKeyword(_DEBUG_DISTANCE, _debugDistance);
                        blurMaterial.EnableKeyword(_OFFSET_EDGE_TEST, _fixPixelLeaks);
                        blurMaterial.EnableKeyword(_DITHER_EDGE_TEST, _ditherEdgeTest);
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

        // Cleanup all the objects we possibly have created
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
            if (ShadowCameraGOs != null)
            {
                foreach (var go in ShadowCameraGOs.Values)
                    SafeDestroy(go);
                ShadowCameraGOs.Clear();
            }

            SafeDestroy(ref LightingTex);
            SafeDestroy(ref LightingTexBlurred);
            SafeDestroy(ref LightingTexR);
            SafeDestroy(ref LightingTexBlurredR);
            SafeDestroy(ref SSS_ProfileTex);
            SafeDestroy(ref SSS_ProfileTexR);
        }

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

        private void UpdateShadowCamera(Camera src, Camera dest)
        {
            if (dest == (object)null)
            {
                return;
            }

            //if (MirrorSSS)
            //{
            //    dest.CopyFrom(src);
            //}

            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.stereoTargetEye = src.stereoTargetEye;
            dest.orthographic = src.orthographic;
            dest.aspect = src.aspect;
            //dest.renderingPath = RenderingPath.Forward;
            dest.orthographicSize = src.orthographicSize;

            if (/*src.stereoEnabled || */!src.usePhysicalProperties)
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
