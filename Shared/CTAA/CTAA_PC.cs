//LIVENDA CTAA CINEMATIC TEMPORAL ANTI ALIASING
//Copyright Livenda Labs 2019
//CTAA-NXT V2.0
//Original Author: Livenda Labs
////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using KKAPI.Utilities;
using System.Collections.Generic;
using static Graphics.Settings.CTAASettings;

namespace Graphics.CTAA
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/LIVENDA/CTAA_PC")]
    public class CTAA_PC : MonoBehaviour
    {
        //CTAA Standard => None
        [Space(5)]
        public bool CTAA_Enabled = true;
        [Header("CTAA Settings")]
        //Supersample mode can now be changed at runtime via Enum.
        [Tooltip("Super Sample Mode")]
        public CTAA_MODE SupersampleMode = CTAA_MODE.STANDARD;
        [Space(5)]
        [Tooltip("Number of Frames to Blend via Re-Projection")]
        [Range(3, 16)] public int TemporalStability = 6;
        [Space(5)]
        [Tooltip("Anti-Aliasing Response and Strength for HDR Pixels")]
        [Range(0.001f, 4.0f)] public float HdrResponse = 1.2f;
        [Space(5)]
        [Tooltip("Amount of AA Blur in Geometric edges")]
        [Range(0.0f, 2.0f)] public float EdgeResponse = 0.5f;
        [Space(5)]
        [Tooltip("Amount of Automatic Sharpness added based on relative velocities")]
        [Range(0.0f, 1.5f)] public float AdaptiveSharpness = 0.2f;
        [Space(5)]
        [Tooltip("Amount sub-pixel Camera Jitter")]
        [Range(0.0f, 0.5f)] public float TemporalJitterScale = 0.475f;
        [Space(5)]
        //It's not a function for gaming. Don't use it.
        //[Tooltip("Eliminates Micro Shimmer - (No Dynamic Objects) Suitable for Architectural Visualisation, CAD, Engineering or non-moving objects. Camera can be moved.")]
        //public bool AntiShimmerMode = false;

        public bool MSAA_Control = false;
        public int m_MSAA_Level = 0;

        private bool PreEnhanceEnabled = true;
        private float preEnhanceStrength = 1.0f;
        private float preEnhanceClamp = 0.005f;
        const float AdaptiveResolve = 3000.0f;
        private float jitterScale = 1.0f;

        //Masking feature. Seems useless
        //private Shader maskRenderShader;
        //private Material layerPostMat;
        private Material ctaaMat;
        private Material mat_enhance;
        private RenderTexture rtAccum0;
        private RenderTexture rtAccum1;
        private RenderTexture afterPreEnhace;
        private RenderTexture upScaleRT;

        //For detecting SS mode change.
        private CTAA_MODE prev_SupersampleMode;
        //For detecting screen size change.
        private Vector2Int prev_ScreenXY;
        private Vector2Int orig_ScreenXY;
        private bool firstFrame;
        private bool swap;
        private int frameCounter;

        private static readonly float[] x_jit = { 0.5f, -0.25f, 0.75f, -0.125f, 0.625f, 0.575f, -0.875f, 0.0625f, -0.3f, 0.75f, -0.25f, -0.625f, 0.325f, 0.975f, -0.075f, 0.625f };
        private static readonly float[] y_jit = { 0.33f, -0.66f, 0.51f, 0.44f, -0.77f, 0.12f, -0.55f, 0.88f, -0.83f, 0.14f, 0.71f, -0.34f, 0.87f, -0.12f, 0.75f, 0.08f };

        public bool moveActive = true;
        public float speed = 0.002f;
        private int count = 0;

        public Camera MainCamera;
        void SetCTAA_Parameters()
        {
            PreEnhanceEnabled = AdaptiveSharpness > 0.01 ? true : false;
            preEnhanceStrength = Mathf.Lerp(0.2f, 2.0f, AdaptiveSharpness);
            preEnhanceClamp = Mathf.Lerp(0.005f, 0.12f, AdaptiveSharpness);
            jitterScale = TemporalJitterScale;
            //It's not a function for gaming. Don't use it.
            //ctaaMat.SetFloat(CTAA_ShaderIDs._AntiShimmer, (AntiShimmerMode ? 1.0f : 0.0f));
            ctaaMat.SetVector(CTAA_ShaderIDs._delValues, new Vector4(0.01f, 2.0f, 0.5f, 0.3f));
        }
        AssetBundle assetBundle;
        void SetResources()
        {
            if (assetBundle == null)
                assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("ctaa.unity3d"));

            if (ctaaMat == null) ctaaMat = new Material(assetBundle.LoadAsset<Shader>("assets/shaders/ctaa_pc.shader"));
            if (mat_enhance == null) mat_enhance = new Material(assetBundle.LoadAsset<Shader>("assets/shaders/ctaa_enhance_pc.shader"));
        }
        void ClearResources()
        {
            if (ctaaMat != null) DestroyMaterial(ctaaMat);
            if (mat_enhance != null) DestroyMaterial(mat_enhance);
            //Masking feature. Seems useless
            //if (layerPostMat != null)     DestroyMaterial(layerPostMat);
        }
        private static void DestroyMaterial(Material mat)
        {
            if (mat != null)
            {
                Object.DestroyImmediate(mat);
                mat = null;
            }
        }
        private static void UpdateRT(ref RenderTexture rt, string rtName, int screenParamsX, int screenParamsY, int depth,
                                     RenderTextureFormat renderTextureFormat, RenderTextureReadWrite renderTextureReadWrite, FilterMode filterMode, TextureWrapMode wrapMode,
                                     bool preCreateRT = true)
        {
            if (rt != null)
            {
                RenderTexture.active = null;
                rt.Release();
                rt = null;
            }
            rt = new RenderTexture(screenParamsX, screenParamsY, depth, renderTextureFormat, renderTextureReadWrite);
            rt.name = rtName;
            rt.filterMode = filterMode;
            rt.wrapMode = wrapMode;
            if (preCreateRT)
                rt.Create();
        }
        private void SetRT()
        {
            if (ChangedSuperSamplingMode())
            {
                if (SupersampleMode == CTAA_MODE.STANDARD)
                    MainCamera.targetTexture = null;
            }

            var targetRT = MainCamera.targetTexture;
            if (targetRT)
            {
                MainCamera.targetTexture = null;
                orig_ScreenXY.Set(MainCamera.pixelWidth, MainCamera.pixelHeight);
                MainCamera.targetTexture = targetRT;
            }
            else
            {
                MainCamera.targetTexture = null;
                orig_ScreenXY.Set(MainCamera.pixelWidth, MainCamera.pixelHeight);
            }

            m_LayerRenderCam.enabled = (SupersampleMode > CTAA_MODE.STANDARD) ? true : false;
            int depth = 0;
            bool preCreate = true;
            UpdateRT(ref upScaleRT, "RT_Upscale",
                (SupersampleMode > CTAA_MODE.STANDARD) ? orig_ScreenXY.x << 1 : orig_ScreenXY.x,
                (SupersampleMode > CTAA_MODE.STANDARD) ? orig_ScreenXY.y << 1 : orig_ScreenXY.y,
                depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Clamp, preCreate);
            UpdateRT(ref rtAccum0, "RT_Accum0",
                (SupersampleMode == CTAA_MODE.CINA_ULTRA) ? orig_ScreenXY.x << 1 : orig_ScreenXY.x,
                (SupersampleMode == CTAA_MODE.CINA_ULTRA) ? orig_ScreenXY.y << 1 : orig_ScreenXY.y,
                depth, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Clamp, preCreate);
            UpdateRT(ref rtAccum1, "RT_Accum1",
                (SupersampleMode == CTAA_MODE.CINA_ULTRA) ? orig_ScreenXY.x << 1 : orig_ScreenXY.x,
                (SupersampleMode == CTAA_MODE.CINA_ULTRA) ? orig_ScreenXY.y << 1 : orig_ScreenXY.y,
                depth, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Clamp, preCreate);
            UpdateRT(ref afterPreEnhace, "RT_AfterPreEnhace",
                (SupersampleMode > CTAA_MODE.STANDARD) ? orig_ScreenXY.x << 1 : orig_ScreenXY.x,
                (SupersampleMode > CTAA_MODE.STANDARD) ? orig_ScreenXY.y << 1 : orig_ScreenXY.y,
                depth, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Clamp, preCreate);

            if (SupersampleMode > CTAA_MODE.STANDARD)
            {
                Debug.Log($"CTAA updated. Supersample Mode: {prev_SupersampleMode} => {SupersampleMode} " +
                          $"Source Size: {orig_ScreenXY.x}x{orig_ScreenXY.y} => Upscaled Size: {orig_ScreenXY.x << 1}x{orig_ScreenXY.y << 1}");
            }
            else
            {
                Debug.Log($"CTAA updated. Supersample Mode: {prev_SupersampleMode} => {SupersampleMode} " +
                          $"Source Size: {orig_ScreenXY.x}x{orig_ScreenXY.y}");
            }
        }
        void ClearRT()
        {
            if (rtAccum0 != null) DestroyImmediate(rtAccum0); rtAccum0 = null;
            if (rtAccum1 != null) DestroyImmediate(rtAccum1); rtAccum1 = null;
            if (afterPreEnhace != null) DestroyImmediate(afterPreEnhace); afterPreEnhace = null;

            GetComponent<Camera>().targetTexture = null;
            if (upScaleRT != null) DestroyImmediate(upScaleRT); upScaleRT = null;
        }
        void Awake()
        {
            MainCamera = GetComponent<Camera>();

            SetResources();

            firstFrame = true;
            swap = true;
            frameCounter = 0;

            SetCTAA_Parameters();
        }
        void OnEnable()
        {
            MainCamera = GetComponent<Camera>();

            SetResources();

            firstFrame = true;
            swap = true;
            frameCounter = 0;

            SetCTAA_Parameters();

            MainCamera.depthTextureMode |= DepthTextureMode.Depth;
            MainCamera.depthTextureMode |= DepthTextureMode.MotionVectors;
        }
        void LateUpdate()
        {
            if (moveActive)
            {
                if (count < 2)
                {
                    this.transform.position += new Vector3(0, 1.0f * speed, 0);
                    count++;
                }
                else
                {
                    if (count < 4)
                    {
                        this.transform.position -= new Vector3(0, 1.0f * speed, 0);
                        count++;
                    }
                    else
                    {
                        moveActive = false;
                    }
                }

            }
        }
        private void OnDisable()
        {
            ClearCTAACameras();
            ClearResources();
            ClearRT();
        }
        private void OnDestroy()
        {
            ClearCTAACameras();
            ClearResources();
            ClearRT();
        }
        private void Start()
        {
            MainCamera = GetComponent<Camera>();
        }
        private bool ChangedSuperSamplingMode() => prev_SupersampleMode != SupersampleMode;
        private void WriteSuperSamplingMode() => prev_SupersampleMode = SupersampleMode;
        private bool ChangedScreenSize() => (prev_ScreenXY.x != MainCamera.pixelWidth) || (prev_ScreenXY.y != MainCamera.pixelHeight);
        private void WriteScreenSize() { prev_ScreenXY.x = MainCamera.pixelWidth; prev_ScreenXY.y = MainCamera.pixelHeight; }
        private bool NullRT() => (upScaleRT == null) || (rtAccum0 == null) || (rtAccum1 == null) || (afterPreEnhace == null);
        void OnPreCull()
        {
            if (CTAA_Enabled == false)
                return;
            //SetCB();
            if (ChangedSuperSamplingMode() ||
                ChangedScreenSize() ||
                NullRT())
            {
                SetCTAACameras();
                SetRT();
            }
            if (SupersampleMode > CTAA_MODE.STANDARD)
            {
                MainCamera.targetTexture = upScaleRT;
            }
            jitterCam();
        }
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CTAA_Enabled)
            {
                SetCTAA_Parameters();
                //Use Sharpening
                if (PreEnhanceEnabled)
                {
                    mat_enhance.SetFloat(CTAA_ShaderIDs._AEXCTAA, 1.0f / (float)MainCamera.pixelWidth);
                    mat_enhance.SetFloat(CTAA_ShaderIDs._AEYCTAA, 1.0f / (float)MainCamera.pixelHeight);
                    mat_enhance.SetFloat(CTAA_ShaderIDs._AESCTAA, preEnhanceStrength);
                    mat_enhance.SetFloat(CTAA_ShaderIDs._AEMAXCTAA, preEnhanceClamp);
                    UnityEngine.Graphics.Blit(source, afterPreEnhace, mat_enhance, 1);
                }
                //-----------------------------------------------------------
                RenderTexture ctaaSource = PreEnhanceEnabled ? afterPreEnhace : source;
                if (firstFrame)
                {
                    UnityEngine.Graphics.Blit(ctaaSource, rtAccum0);
                    firstFrame = false;
                }

                ctaaMat.SetFloat(CTAA_ShaderIDs._AdaptiveResolve, AdaptiveResolve);
                ctaaMat.SetVector(CTAA_ShaderIDs._ControlParams, new Vector4(1.0f, (float)TemporalStability, HdrResponse, EdgeResponse));

                if (swap)
                {
                    ctaaMat.SetTexture(CTAA_ShaderIDs._Accum, rtAccum0);
                    UnityEngine.Graphics.Blit(ctaaSource, rtAccum1, ctaaMat);
                    UnityEngine.Graphics.Blit(rtAccum1, destination);
                }
                else
                {
                    ctaaMat.SetTexture(CTAA_ShaderIDs._Accum, rtAccum1);
                    UnityEngine.Graphics.Blit(ctaaSource, rtAccum0, ctaaMat);
                    UnityEngine.Graphics.Blit(rtAccum0, destination);
                }

                //-----------------------------------------------------------            
                swap = !swap;
            }
            else
            {
                UnityEngine.Graphics.Blit(source, destination);
            }
            //Write vars
            WriteScreenSize();
            WriteSuperSamplingMode();
        }
        private static class CTAA_ShaderIDs
        {
            internal static readonly int _Accum = Shader.PropertyToID("_Accum");
            internal static readonly int _AdaptiveResolve = Shader.PropertyToID("_AdaptiveResolve");
            internal static readonly int _ControlParams = Shader.PropertyToID("_ControlParams");
            internal static readonly int _AEXCTAA = Shader.PropertyToID("_AEXCTAA");
            internal static readonly int _AEYCTAA = Shader.PropertyToID("_AEYCTAA");
            internal static readonly int _AESCTAA = Shader.PropertyToID("_AESCTAA");
            internal static readonly int _AEMAXCTAA = Shader.PropertyToID("_AEMAXCTAA");
            internal static readonly int _AntiShimmer = Shader.PropertyToID("_AntiShimmer");
            internal static readonly int _delValues = Shader.PropertyToID("_delValues");
        }

        #region CTAA Subcamera
        RenderPostCTAA renderPostCTAA;
        Camera m_LayerRenderCam;
        //Masking feature. Seems useless
        //Camera m_LayerMaskCam;
        public LayerMask m_ExcludeLayers;
        public bool m_LayerMaskingEnabled = true;
        void jitterCam()
        {
            MainCamera.ResetWorldToCameraMatrix();     // < ----- Unity 2017 Up
            MainCamera.ResetProjectionMatrix();        // < ----- Unity 2017 Up

            MainCamera.nonJitteredProjectionMatrix = MainCamera.projectionMatrix;

            Matrix4x4 matrixx = MainCamera.projectionMatrix;
            float num = x_jit[this.frameCounter] * jitterScale;
            float num2 = y_jit[this.frameCounter] * jitterScale;
            matrixx.m02 += ((num * 2f) - 1f) / MainCamera.pixelRect.width;
            matrixx.m12 += ((num2 * 2f) - 1f) / MainCamera.pixelRect.height;
            this.frameCounter++;
            this.frameCounter = this.frameCounter % 16;
            MainCamera.projectionMatrix = matrixx;

        }
        void SetCTAACameras()
        {
            //Masking feature. Seems useless
            //===================
            //Layer Mask System
            //===================
            /*if (!m_LayerMaskCam)
            {
                GameObject go = new GameObject("LayerMaskRenderCam");
                m_LayerMaskCam = go.AddComponent<Camera>();
                m_LayerMaskCam.CopyFrom(MainCamera);
                m_LayerMaskCam.transform.position = transform.position;
                m_LayerMaskCam.transform.rotation = transform.rotation;
                LayerMask someMask = ~0;

                m_LayerMaskCam.cullingMask = someMask;
                m_LayerMaskCam.depth = MainCamera.depth + 1;
                m_LayerMaskCam.clearFlags = CameraClearFlags.Depth;
                m_LayerMaskCam.depthTextureMode = DepthTextureMode.None;
                m_LayerMaskCam.targetTexture = null;
                m_LayerMaskCam.allowMSAA = false;
                m_LayerMaskCam.enabled = false;
                m_LayerMaskCam.renderingPath = RenderingPath.Forward;
                //go.hideFlags = HideFlags.HideAndDontSave;
            }*/

            //=====================
            //Layer Compose System
            //=====================
            if (!m_LayerRenderCam)
            {
                GameObject go = new GameObject("LayerRenderCam");
                m_LayerRenderCam = go.AddComponent<Camera>();
                m_LayerRenderCam.CopyFrom(MainCamera);

                m_LayerRenderCam.transform.position = transform.position;
                m_LayerRenderCam.transform.rotation = transform.rotation;
                m_LayerRenderCam.cullingMask = m_ExcludeLayers;
                m_LayerRenderCam.depth = MainCamera.depth + 1;
                m_LayerRenderCam.clearFlags = CameraClearFlags.Depth;
                m_LayerRenderCam.depthTextureMode = DepthTextureMode.None;
                m_LayerRenderCam.targetTexture = null;

                m_LayerRenderCam.gameObject.AddComponent<RenderPostCTAA>();

                renderPostCTAA = m_LayerRenderCam.gameObject.GetComponent<RenderPostCTAA>();

                renderPostCTAA.ctaaPC = GetComponent<CTAA_PC>();
                renderPostCTAA.ctaaCamTransform = this.transform;
                //Masking feature. Seems useless
                //renderPostCTAA.MaskRenderCam = m_LayerMaskCam.GetComponent<Camera>();
                //renderPostCTAA.maskRenderShader = this.maskRenderShader;
                //renderPostCTAA.layerPostMat = this.layerPostMat;
                //renderPostCTAA.layerMaskingEnabled = m_LayerMaskingEnabled;
                //go.hideFlags = HideFlags.HideAndDontSave;

            }
        }
        void ClearCTAACameras()
        {
            if (m_LayerRenderCam != null)
            {
                m_LayerRenderCam.targetTexture = null;
                Destroy(m_LayerRenderCam.gameObject);
            }
            //Masking feature. Seems useless
            /*if (m_LayerMaskCam != null)
            {
                m_LayerMaskCam.targetTexture = null;
                Destroy(m_LayerMaskCam.gameObject);
            }*/
        }
        public RenderTexture getCTAA_Render()
        {
            return upScaleRT;
        }
        #endregion
    }
}