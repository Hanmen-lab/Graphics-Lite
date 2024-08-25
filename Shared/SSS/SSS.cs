using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;
using Illusion.Extensions;
using System.Linq;

namespace Graphics
{
    [RequireComponent(typeof(Camera))]
    public class SSS : MonoBehaviour
    {
        //public bool DebugTab;
        //public bool BlurTab;
        //public bool DitherTab;
        //public bool ResourcesTab;

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
        public bool DEBUG_DISTANCE;
        [SerializeField] [Range(0, 1)] public float DepthTest = 0.3f, NormalTest = 0.3f, ProfileColorTest = .05f, ProfileRadiusTest = .05f;
        public bool DitherEdgeTest;
        [Range(1, 1.2f)] public float EdgeOffset = 1.1f;
        public bool FixPixelLeaks;
        private int InitialpixelLights;
        private ShadowQuality InitialShadows;
        private Vector2 m_TextureSize;

        private Dictionary<string, GameObject> LightingCameraGOs;
        private Dictionary<string, GameObject> ProfileCameraGOs;

        public int maxDistance = 10000;
        public Texture NoiseTexture;
        public bool ProfilePerObject;

        public Shader ProfileShader, LightingPassShader;
        [Range(0, 10f)] public bool ShowCameras;
        public bool ShowGUI;
        private SSS_convolution sss_convolution;

        public bool MirrorSSS;

        [HideInInspector] public RenderTexture SSS_ProfileTex, SSS_ProfileTexR, LightingTex, LightingTexBlurred, LightingTexR, LightingTexBlurredR;
        public Color sssColor = Color.yellow;

        public ToggleTexture toggleTexture = ToggleTexture.None;
        public bool UseProfileTest;
        public bool Enabled { get; set; }
        internal float Downsampling { get; set; }

        internal float ScatteringRadius { get; set; }

        internal int ScatteringIterations { get; set; }

        internal int ShaderIterations { get; set; }

        internal bool Dither { get; set; }

        internal float DitherIntensity { get; set; }

        internal float DitherScale { get; set; }

        static readonly int _SSS_ProfileTexId = Shader.PropertyToID("SSS_ProfileTex");
        static readonly int _SSS_ProfileTexRId = Shader.PropertyToID("SSS_ProfileTexR");
        static readonly int _LightingTexId = Shader.PropertyToID("LightingTex");
        static readonly int _LightingTexBlurredId = Shader.PropertyToID("LightingTexBlurred");
        static readonly int _LightingTexRId = Shader.PropertyToID("LightingTex");
        static readonly int _LightingTexBlurredRId = Shader.PropertyToID("LightingTexBlurred");
        static readonly int _DepthTestId = Shader.PropertyToID("DepthTest");
        static readonly int _maxDistanceId = Shader.PropertyToID("maxDistance");
        static readonly int _NormalTestId = Shader.PropertyToID("NormalTest");
        static readonly int _ProfileColorTestId = Shader.PropertyToID("ProfileColorTest");
        static readonly int _ProfileRadiusTestId = Shader.PropertyToID("ProfileRadiusTest");
        static readonly int _EdgeOffsetId = Shader.PropertyToID("EdgeOffset");
        static readonly int _sssColorId = Shader.PropertyToID("sssColor");
        static readonly int _DitherScaleId = Shader.PropertyToID("DitherScale");
        static readonly int _DitherIntensityId = Shader.PropertyToID("DitherIntensity");
        static readonly int _NoiseTextureId = Shader.PropertyToID("NoiseTexture");
        static readonly string _SceneView = "SCENE_VIEW";
        static readonly string _SSS_profiles = "SSS_PROFILES";



        private void CreateCameras(Camera currentCamera, out Camera ProfileCamera, out Camera LightingCamera)
        {
            if (LightingCameraGOs == null)
            {
                LightingCameraGOs = new Dictionary<string, GameObject>();
            }
            if (ProfileCameraGOs == null)
            { 
                ProfileCameraGOs = new Dictionary<string, GameObject>();
            }

            ProfileCamera = null;
            if (ProfilePerObject)
            {                
                string profileGOName = $"SSS Profile Camera for {currentCamera.gameObject.name}-{currentCamera.gameObject.GetInstanceID()}";
                ProfileCameraGOs.TryGetValue(profileGOName, out GameObject ProfileCameraGO);
                if (!ProfileCameraGO)
                {
                    ProfileCameraGO = GameObject.Find(profileGOName);
                    if (!ProfileCameraGO)
                    {
                        ProfileCameraGO = new GameObject(profileGOName, typeof(Camera));
                        ProfileCameraGO.transform.parent = transform;
                        ProfileCameraGO.transform.localPosition = Vector3.zero;
                        ProfileCameraGO.transform.localEulerAngles = Vector3.zero;

                        ProfileCamera = ProfileCameraGO.GetComponent<Camera>();
                        ProfileCamera.backgroundColor = Color.black;
                        ProfileCamera.enabled = false;
                        ProfileCamera.depth = -254;
                        ProfileCamera.allowMSAA = false;
                    }

                    ProfileCameraGOs[profileGOName] = ProfileCameraGO;
                }

                ProfileCamera = ProfileCameraGO.GetComponent<Camera>();
                ProfileCamera.backgroundColor = Color.black;
                ProfileCamera.depth = -254;
            }

            // Camera for lighting
            LightingCamera = null;

            string lightingGOName = $"SSS Lighting Camera for {currentCamera.gameObject.name}-{currentCamera.gameObject.GetInstanceID()}";
            LightingCameraGOs.TryGetValue(lightingGOName, out GameObject LightingCameraGO);
            if (!LightingCameraGO)
            {                
                LightingCameraGO = GameObject.Find(lightingGOName);
                if (!LightingCameraGO)
                {
                    LightingCameraGO = new GameObject(lightingGOName, typeof(Camera));
                    LightingCameraGO.transform.parent = transform;
                    LightingCameraGO.transform.localPosition = Vector3.zero;
                    LightingCameraGO.transform.localEulerAngles = Vector3.zero;

                    LightingCamera = LightingCameraGO.GetComponent<Camera>();
                    LightingCamera.enabled = false;
                    LightingCamera.depth = -846;
                    sss_convolution = LightingCameraGO.AddComponent<SSS_convolution>();
                    sss_convolution.BlurShader = Shader.Find("Hidden/SeparableSSS");
                    if (null == sss_convolution.BlurShader && null != _separableSSS)
                    {
                        sss_convolution.BlurShader = _separableSSS;
                    }
                }
                LightingCameraGOs[lightingGOName] = LightingCameraGO;
            }
            LightingCamera = LightingCameraGO.GetComponent<Camera>();
            LightingCamera.allowMSAA = currentCamera.allowMSAA;
            LightingCamera.backgroundColor = currentCamera.backgroundColor;
            LightingCamera.clearFlags = currentCamera.clearFlags;
            LightingCamera.cullingMask = currentCamera.cullingMask;
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

            if (cam.GetComponent<SSS_buffers_viewer>() == null)
            {
                sss_buffers_viewer = cam.gameObject.AddComponent<SSS_buffers_viewer>();
            }

            if (sss_buffers_viewer == null)
            {
                sss_buffers_viewer = cam.gameObject.GetComponent<SSS_buffers_viewer>();
            }

            sss_buffers_viewer.hideFlags = HideFlags.HideAndDontSave;
            //Make things work on load if only scene view is active
            Shader.EnableKeyword(_SceneView);
            if (ProfilePerObject)
            {
                Shader.EnableKeyword(_SSS_profiles);
            }
        }

        private void OnPreRender()
        {
            Camera LightingCamera = null;

            if (Enabled && cam != null && cam is object)
            {
                Shader.DisableKeyword(_SceneView);
                if (LightingPassShader is null)
                {
                    LightingPassShader = Shader.Find("Hidden/LightingPass");
                    if (LightingPassShader is null && _lightingPass is object)
                    {
                        LightingPassShader = _lightingPass;
                    }    
                }

                if (ProfileShader is null)
                {
                    ProfileShader = Shader.Find("Hidden/SSS_Profile");
                    if (ProfileShader is null && _profile is object)
                    {
                        ProfileShader = _profile;
                    }
                }

                if (cam.stereoEnabled)
                {
                    m_TextureSize.x = XRSettings.eyeTextureWidth / Downsampling;
                    m_TextureSize.y = XRSettings.eyeTextureHeight / Downsampling;
                }
                else
                {
                    m_TextureSize.x = cam.pixelWidth / Downsampling;
                    m_TextureSize.y = cam.pixelHeight / Downsampling;
                }

                CreateCameras(cam, out Camera ProfileCamera, out LightingCamera); //inlined Camera ProfileCamera declaration
                #region Render Profile
                if (ProfilePerObject && ProfileCamera is object)
                {
                    UpdateCameraModes(cam, ProfileCamera);

                    ProfileCamera.depth = -254;
                    ProfileCamera.allowMSAA = false;

                    //ProfileCamera.allowHDR = false;
                    ////humm, removes a lot of artifacts when far away
                    ///

                    InitialpixelLights = QualitySettings.pixelLightCount;
                    InitialShadows = QualitySettings.shadows;
                    QualitySettings.pixelLightCount = 0;
                    QualitySettings.shadows = ShadowQuality.Disable;
                    Shader.EnableKeyword(_SSS_profiles);
                    ProfileCamera.cullingMask = SSS_Layer;
                    ProfileCamera.backgroundColor = Color.black;
                    ProfileCamera.clearFlags = CameraClearFlags.SolidColor;

                    if (cam.stereoEnabled)
                    {    //Left eye   
                        if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                        {                            
                            ProfileCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                            ProfileCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);

                            GetProfileRT(ref SSS_ProfileTex, (int)m_TextureSize.x, (int)m_TextureSize.y, "SSS_ProfileTex");
                            Util.RenderToTarget(ProfileCamera, SSS_ProfileTex, ProfileShader);
                            Shader.SetGlobalTexture(_SSS_ProfileTexId, SSS_ProfileTex);

                        }
                        else if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
                        {
                            ProfileCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                            ProfileCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

                            GetProfileRT(ref SSS_ProfileTexR, (int)m_TextureSize.x, (int)m_TextureSize.y, "SSS_ProfileTexR");
                            Util.RenderToTarget(ProfileCamera, SSS_ProfileTexR, ProfileShader);
                            Shader.SetGlobalTexture(_SSS_ProfileTexRId, SSS_ProfileTexR);

                        }
                    }
                    else
                    {
                        //Mono
              //          ProfileCamera.projectionMatrix = cam.projectionMatrix;//avoid frustum jitter from taa
              //          ProfileCamera.worldToCameraMatrix = cam.worldToCameraMatrix;

                        GetProfileRT(ref SSS_ProfileTex, (int)m_TextureSize.x, (int)m_TextureSize.y, "SSS_ProfileTex");
                        Util.RenderToTarget(ProfileCamera, SSS_ProfileTex, ProfileShader);
                        Shader.SetGlobalTexture(_SSS_ProfileTexId, SSS_ProfileTex);
                        Shader.SetGlobalTexture(_SSS_ProfileTexRId, SSS_ProfileTex);
                    }

                    QualitySettings.pixelLightCount = InitialpixelLights;
                    QualitySettings.shadows = InitialShadows;
                }
                else
                {
                    Shader.DisableKeyword(_SSS_profiles);

                    SafeDestroy(SSS_ProfileTex);
                    SafeDestroy(SSS_ProfileTexR);
                }

                #endregion

                #region Render Lighting


                UpdateCameraModes(cam, LightingCamera);
                LightingCamera.allowHDR = cam.allowHDR;


                //if (GTAO.GTAOManager.settings.Enabled)
                //{
                //    if (LightingCamera.GetComponent<GTAO.GroundTruthAmbientOcclusion>() == null)
                //    {                        
                //        GTAO.GroundTruthAmbientOcclusion gtao = LightingCamera.GetOrAddComponent<GTAO.GroundTruthAmbientOcclusion>();
                //        GTAO.GTAOManager.RegisterAdditionalInstance(gtao);
                //        GTAO.GTAOManager.CopySettingsToOtherInstances();
                //    }
                //}
                //else
                //{
                //    if (LightingCamera.GetComponent<GTAO.GroundTruthAmbientOcclusion>() != null)
                //    {
                //        GTAO.GroundTruthAmbientOcclusion gtao = LightingCamera.GetOrAddComponent<GTAO.GroundTruthAmbientOcclusion>();
                //        GTAO.GTAOManager.DestroyGTAOInstance(gtao);
                //        Destroy(gtao);
                //    }
                //}
                //if (VAO.VAOManager.settings.Enabled)
                //{
                //    if (LightingCamera.gameObject.GetComponent<VAO.VAOEffectCommandBuffer>() == null && LightingCamera.gameObject.GetComponent<VAO.VAOEffect>() == null)
                //    {
                //        VAO.VAOEffect vao = LightingCamera.gameObject.AddComponent<VAO.VAOEffect>();
                //        VAO.VAOManager.RegisterAdditionalInstance(vao);
                //    }
                //}
                //else
                //{
                //    if (LightingCamera.gameObject.GetComponent<VAO.VAOEffectCommandBuffer>() != null || LightingCamera.gameObject.GetComponent<VAO.VAOEffect>() != null)
                //    {
                //        VAO.VAOEffectCommandBuffer vao = LightingCamera.GetComponent<VAO.VAOEffectCommandBuffer>();
                //        if (vao != null)
                //        {
                //            VAO.VAOManager.DestroyVAOInstance(vao);
                //            Destroy(vao);
                //        }
                //        else
                //        {
                //            vao = LightingCamera.GetComponent<VAO.VAOEffect>();
                //            if (vao != null)
                //            {
                //                VAO.VAOManager.DestroyVAOInstance(vao);
                //                Destroy(vao);
                //            }
                //        }
                //    }
                //}


                // if (SurfaceScattering)
                {
                    if (sss_convolution is null)
                    {
                        sss_convolution = LightingCamera.gameObject.GetComponent<SSS_convolution>();
                    }

                    if (sss_convolution && sss_convolution._BlurMaterial)
                    {
                        sss_convolution._BlurMaterial.SetFloat(_DepthTestId, Mathf.Max(.00001f, DepthTest / 20));
                        maxDistance = Mathf.Max(0, maxDistance);
                        sss_convolution._BlurMaterial.SetFloat(_maxDistanceId, maxDistance);
                        sss_convolution._BlurMaterial.SetFloat(_NormalTestId, Mathf.Max(.001f, NormalTest));
                        sss_convolution._BlurMaterial.SetFloat(_ProfileColorTestId, Mathf.Max(.001f, ProfileColorTest));
                        sss_convolution._BlurMaterial.SetFloat(_ProfileRadiusTestId, Mathf.Max(.001f, ProfileRadiusTest));
                        sss_convolution._BlurMaterial.SetFloat(_EdgeOffsetId, EdgeOffset);
                        sss_convolution._BlurMaterial.SetInt("_SSS_NUM_SAMPLES", ShaderIterations + 1);
                        sss_convolution._BlurMaterial.SetColor(_sssColorId, sssColor);

                        if (Dither)
                        {
                            sss_convolution._BlurMaterial.EnableKeyword("RANDOMIZED_ROTATION");
                            sss_convolution._BlurMaterial.SetFloat(_DitherScaleId, DitherScale);
                            //sss_convolution._BlurMaterial.SetFloat("DitherSpeed", DitherSpeed * 10);
                            sss_convolution._BlurMaterial.SetFloat(_DitherIntensityId, DitherIntensity);

                            if (NoiseTexture)
                            {
                                sss_convolution._BlurMaterial.SetTexture(_NoiseTextureId, NoiseTexture);
                            }
                            else
                            {
                                Debug.Log("Noise texture not available");
                            }
                        }
                        else sss_convolution._BlurMaterial.DisableKeyword("RANDOMIZED_ROTATION");

                        if (UseProfileTest && ProfilePerObject)
                            sss_convolution._BlurMaterial.EnableKeyword("PROFILE_TEST");
                        else
                            sss_convolution._BlurMaterial.DisableKeyword("PROFILE_TEST");

                        if (DEBUG_DISTANCE)
                            sss_convolution._BlurMaterial.EnableKeyword("DEBUG_DISTANCE");
                        else
                            sss_convolution._BlurMaterial.DisableKeyword("DEBUG_DISTANCE");

                        if (FixPixelLeaks)
                            sss_convolution._BlurMaterial.EnableKeyword("OFFSET_EDGE_TEST");
                        else
                            sss_convolution._BlurMaterial.DisableKeyword("OFFSET_EDGE_TEST");

                        if (DitherEdgeTest)
                            sss_convolution._BlurMaterial.EnableKeyword("DITHER_EDGE_TEST");
                        else
                            sss_convolution._BlurMaterial.DisableKeyword("DITHER_EDGE_TEST"); 
                    }

                    LightingCamera.backgroundColor = cam.backgroundColor;
                    LightingCamera.clearFlags = cam.clearFlags;
                    LightingCamera.cullingMask = SSS_Layer;
                    LightingCamera.depth = -846;
                    sss_convolution.iterations = ScatteringIterations;

                    if (cam.stereoEnabled)
                    { 
                        sss_convolution.BlurRadius = (ScatteringRadius / Downsampling) / 2; 
                    }
                    else
                    { 
                        sss_convolution.BlurRadius = (ScatteringRadius / Downsampling); 
                    }

                    LightingCamera.depthTextureMode = DepthTextureMode.DepthNormals;
                    if (cam.stereoEnabled)
                    {
                        if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                        {
                            LightingCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                            LightingCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);

                            GetRT(ref LightingTex, (int)m_TextureSize.x, (int)m_TextureSize.y, "LightingTexture");
                            GetRT(ref LightingTexBlurred, (int)m_TextureSize.x, (int)m_TextureSize.y, "SSSLightingTextureBlurred");
                            sss_convolution.blurred = LightingTexBlurred;
                            sss_convolution.rtFormat = LightingTex.format;
                            if (LightingPassShader is object)
                            {
                                Util.RenderToTarget(LightingCamera, LightingTex, LightingPassShader);
                                Shader.SetGlobalTexture(_LightingTexBlurredId, LightingTexBlurred);
                                Shader.SetGlobalTexture(_LightingTexId, LightingTex);
                            }
                        }
                        else if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
                        {
                            LightingCamera.projectionMatrix = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                            LightingCamera.worldToCameraMatrix = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

                            GetRT(ref LightingTexR, (int)m_TextureSize.x, (int)m_TextureSize.y, "LightingTextureR");
                            GetRT(ref LightingTexBlurredR, (int)m_TextureSize.x, (int)m_TextureSize.y, "SSSLightingTextureBlurredR");
                            sss_convolution.blurred = LightingTexBlurredR;
                            sss_convolution.rtFormat = LightingTexR.format;
                            if (LightingPassShader is object)
                            {
                                Util.RenderToTarget(LightingCamera, LightingTexR, LightingPassShader);
                                Shader.SetGlobalTexture(_LightingTexBlurredRId, LightingTexBlurredR);
                                Shader.SetGlobalTexture(_LightingTexRId, LightingTexR);
                            }
                        }
                    }
                    else
                    {
                        //LightingCamera.projectionMatrix = cam.projectionMatrix;
                        //LightingCamera.worldToCameraMatrix = cam.worldToCameraMatrix;

                        GetRT(ref LightingTex, (int)m_TextureSize.x, (int)m_TextureSize.y, "LightingTexture");
                        GetRT(ref LightingTexBlurred, (int)m_TextureSize.x, (int)m_TextureSize.y, "SSSLightingTextureBlurred");
                        sss_convolution.blurred = LightingTexBlurred;
                        sss_convolution.rtFormat = LightingTex.format;
                        if (LightingPassShader is object)
                        {
                            Util.RenderToTarget(LightingCamera, LightingTex, LightingPassShader);
                            Shader.SetGlobalTexture(_LightingTexBlurredId, LightingTexBlurred);
                            Shader.SetGlobalTexture(_LightingTexId, LightingTex);
                            Shader.SetGlobalTexture(_LightingTexBlurredRId, LightingTexBlurred);
                            Shader.SetGlobalTexture(_LightingTexRId, LightingTex);
                        }
                    }

                }
                #endregion
            }
            else
            {
                if (LightingCamera != null)
                {
                    LightingCamera.depthTextureMode = DepthTextureMode.None;
                }

                if (sss_buffers_viewer != null && sss_buffers_viewer.enabled)
                {
                    sss_buffers_viewer.enabled = false;
                }
            }

            #region Debug
            if (sss_buffers_viewer != null && Enabled)
                switch (toggleTexture)
                {
                    case ToggleTexture.LightingTex:
                        sss_buffers_viewer.InputBuffer = LightingTex;
                        sss_buffers_viewer.enabled = true;
                        break;
                    case ToggleTexture.LightingTexBlurred:
                        sss_buffers_viewer.InputBuffer = LightingTexBlurred;
                        sss_buffers_viewer.enabled = true;
                        break;
                    case ToggleTexture.ProfileTex:
                        sss_buffers_viewer.InputBuffer = SSS_ProfileTex;
                        sss_buffers_viewer.enabled = true;
                        break;
                    case ToggleTexture.None:
                        sss_buffers_viewer.enabled = false;
                        break;
                }
            #endregion

        }

        private void OnPostRender()
        {
            Shader.EnableKeyword(_SceneView);

            List<string> keysToDestroy = new List<string>();
            LightingCameraGOs?.Keys.ToList().ForEach(s =>
                {
                    if (LightingCameraGOs[s]) keysToDestroy.Add(s);
                });
            keysToDestroy.ForEach(s => LightingCameraGOs.Remove(s));

            keysToDestroy.Clear();
            ProfileCameraGOs?.Keys.ToList().ForEach(s =>
                {
                    if (ProfileCameraGOs[s]) keysToDestroy.Add(s);
                });
            keysToDestroy.ForEach(s => ProfileCameraGOs.Remove(s));

        }

        private void SafeDestroy(Object obj)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
            //obj = null; unnecessary assignment.
        }

        private void Cleanup()
        {
            if (LightingCameraGOs != null)
            {
                LightingCameraGOs.Values.ToList().ForEach(go => SafeDestroy(go));
                LightingCameraGOs.Clear();
            }
            if (ProfileCameraGOs != null)
            {
                ProfileCameraGOs.Values.ToList().ForEach(go => SafeDestroy(go));
                ProfileCameraGOs.Clear();
            }

            SafeDestroy(LightingTex);
            SafeDestroy(LightingTexBlurred);
            SafeDestroy(LightingTexR);
            SafeDestroy(LightingTexBlurredR);
        }

        // Cleanup all the objects we possibly have created
        private void OnDisable()
        {
            // Shader.EnableKeyword("UNITY_STEREO_EYE");
            Shader.EnableKeyword(_SceneView);
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
            if (dest is null)
            {
                return;
            }

            if (MirrorSSS)
                dest.CopyFrom(src);

            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.stereoTargetEye = src.stereoTargetEye;
            dest.orthographic = src.orthographic;
            dest.aspect = src.aspect;
            dest.renderingPath = RenderingPath.Forward;
            dest.orthographicSize = src.orthographicSize;
            if (src.stereoEnabled == false)
            {
                if (src.usePhysicalProperties == false)
                {
                    dest.fieldOfView = src.fieldOfView;
                }
                else
                {
                    dest.usePhysicalProperties = src.usePhysicalProperties;
                    dest.projectionMatrix = src.projectionMatrix;
                }
            }


            if (src.stereoEnabled && dest.fieldOfView != src.fieldOfView)
            {
                dest.fieldOfView = src.fieldOfView;
            }
        }

        protected RenderTextureReadWrite GetRTReadWrite()
        {
            //return RenderTextureReadWrite.Default;
            return cam.allowHDR ? RenderTextureReadWrite.Default : RenderTextureReadWrite.Linear;
        }

        protected RenderTextureFormat GetRTFormat()
        {
            return cam.allowHDR ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.Default;
        }

        protected void GetRT(ref RenderTexture rt, int x, int y, string name)
        {
            if (x <= 0 || y <= 0)
            {
                return; // Below-equal zero request will crash the game.
            }

            ReleaseRT(rt);
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
            ReleaseRT(rt);
            //if (cam.allowMSAA && QualitySettings.antiAliasing > 0 && AllowMSAA)
            //    rt = RenderTexture.GetTemporary(x, y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, QualitySettings.antiAliasing);
            //else
            rt = RenderTexture.GetTemporary(x, y, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            rt.filterMode = FilterMode.Point;
            rt.autoGenerateMips = false;
            rt.name = name;
            rt.wrapMode = TextureWrapMode.Clamp;
        }

        private void ReleaseRT(RenderTexture rt)
        {
            if (rt is object)
            {
                RenderTexture.ReleaseTemporary(rt);
                //rt = null; unnecessary assignment.
            }
        }

        #endregion
    }
}