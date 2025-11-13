//LIVENDA CTAA CINEMATIC TEMPORAL ANTI ALIASING
//Copyright Livenda Labs 2019
//CTAA-NXT V2.0
//Original Author: Livenda Labs
////////////////////////////////////////////////////
using Graphics.Settings;
using KKAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using static Graphics.Settings.CTAASettings;

namespace Graphics
{
    [PostProcess(typeof(AACTAA_Renderer), PostProcessEvent.AfterStack, "AA/AACTAA", allowInSceneView: false)]
    public class AACTAA : PostProcessEffectSettings
    {
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            //Graphics.Instance.Log.LogInfo($"!base.IsEnabledAndSupported: {!base.IsEnabledAndSupported(context)}");
            //if (!base.IsEnabledAndSupported(context)) return false;

            var ctaaPC = context.camera.GetComponent<CTAA_PC>();
            //Graphics.Instance.Log.LogInfo($"ctaaPC.isActiveAndEnabled: {ctaaPC.isActiveAndEnabled}");
            return ctaaPC && ctaaPC.isActiveAndEnabled;
        }
    }

    public class AACTAA_Renderer : PostProcessEffectRenderer<AACTAA>
    {
        public override void Render(PostProcessRenderContext ctx)
        {
            var cmd = ctx.command;

            var ctaa = ctx.camera.GetComponent<CTAA_PC>();

            Assert.IsNotNull(ctaa);

            if (ctaa.CTAA_Enabled)
            {
                ctaa.SetCTAA_Parameters();

                //Use Sharpening
                if (ctaa.PreEnhanceEnabled)
                {
                    ctaa.mat_enhance.SetFloat(CTAA_PC.CTAA_ShaderIDs._AEXCTAA, 1.0f / (float)ctaa.mainCamera.pixelWidth);
                    ctaa.mat_enhance.SetFloat(CTAA_PC.CTAA_ShaderIDs._AEYCTAA, 1.0f / (float)ctaa.mainCamera.pixelHeight);
                    ctaa.mat_enhance.SetFloat(CTAA_PC.CTAA_ShaderIDs._AESCTAA, ctaa.preEnhanceStrength);
                    ctaa.mat_enhance.SetFloat(CTAA_PC.CTAA_ShaderIDs._AEMAXCTAA, ctaa.preEnhanceClamp);
                    //UnityEngine.Graphics.Blit(source, afterPreEnhace, mat_enhance, 1);
                    cmd.Blit(ctx.source, ctaa.afterPreEnhace, ctaa.mat_enhance, 1);
                }

                //-----------------------------------------------------------

                //RenderTexture ctaaSource = ctaa.PreEnhanceEnabled ? ctaa.afterPreEnhace : source;
                var ctaaSource = ctaa.PreEnhanceEnabled ? ctaa.afterPreEnhace : ctx.source;

                if (ctaa.firstFrame)
                {
                    //UnityEngine.Graphics.Blit(ctaaSource, rtAccum0);
                    cmd.Blit(ctaaSource, ctaa.rtAccum0);
                    ctaa.firstFrame = false;
                }

                ctaa.ctaaMat.SetFloat(CTAA_PC.CTAA_ShaderIDs._AdaptiveResolve, CTAA_PC.AdaptiveResolve);
                ctaa.ctaaMat.SetVector(CTAA_PC.CTAA_ShaderIDs._ControlParams,
                    new Vector4(1.0f, (float)ctaa.TemporalStability, ctaa.HdrResponse, ctaa.EdgeResponse));

                if (ctaa.swap)
                {
                    ctaa.ctaaMat.SetTexture(CTAA_PC.CTAA_ShaderIDs._Accum, ctaa.rtAccum0);
                    //UnityEngine.Graphics.Blit(ctaaSource, rtAccum1, ctaaMat);
                    //UnityEngine.Graphics.Blit(rtAccum1, destination);
                    cmd.Blit(ctaaSource, ctaa.rtAccum1, ctaa.ctaaMat);
                    cmd.Blit(ctaa.rtAccum1, ctx.destination);
                }
                else
                {
                    ctaa.ctaaMat.SetTexture(CTAA_PC.CTAA_ShaderIDs._Accum, ctaa.rtAccum1);
                    //UnityEngine.Graphics.Blit(ctaaSource, rtAccum0, ctaaMat);
                    //UnityEngine.Graphics.Blit(rtAccum0, destination);
                    cmd.Blit(ctaaSource, ctaa.rtAccum0, ctaa.ctaaMat);
                    cmd.Blit(ctaa.rtAccum0, ctx.destination);
                }

                //-----------------------------------------------------------            

                ctaa.swap = !ctaa.swap;
            }
            else
            {
                //UnityEngine.Graphics.Blit(source, destination);
                cmd.Blit(ctx.source, ctx.destination);
            }
            // This two method will not work as intended because it's in the static context 
            /*//Write vars
            ctaa.WriteScreenSize();
            ctaa.WriteSuperSamplingMode();*/
        }
    }

    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/LIVENDA/CTAA_PC")]
    public class CTAA_PC : MonoBehaviour
    {
        public static class CTAA_ShaderIDs
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

        //-------------------------------------------------------
        //Public Parameters
        //------------------------------------------------------- 

        [Space(5)]
        public bool CTAA_Enabled = true;
        [Header("CTAA Settings")]
        [Range(0, 1)] public float DoFStabilityHack = 1;
        [Tooltip("Number of Frames to Blend via Re-Projection")]
        [Range(3, 16)]
        public int TemporalStability = 6;
        [Space(5)]
        [Tooltip("Anti-Aliasing Response and Strength for HDR Pixels")]
        [Range(0.001f, 4.0f)]
        public float HdrResponse = 1.2f;
        [Space(5)]
        [Tooltip("Amount of AA Blur in Geometric edges")]
        [Range(0.0f, 2.0f)]
        public float EdgeResponse = 0.5f;
        [Space(5)]
        [Tooltip("Amount of Automatic Sharpness added based on relative velocities")]
        [Range(0.0f, 1.5f)]
        public float AdaptiveSharpness = 0.2f;
        [Space(5)]
        [Tooltip("Amount sub-pixel Camera Jitter")]
        [Range(0.0f, 0.5f)]
        public float TemporalJitterScale = 0.475f;
        [Space(5)]
        //It's not a function for gaming. Don't use it.
        [Tooltip("Eliminates Micro Shimmer - (No Dynamic Objects) Suitable for Architectural Visualisation, CAD, Engineering or non-moving objects. Camera can be moved.")]
        public bool AntiShimmerMode = false;
        //Supersample mode can now be changed at runtime via Enum.
        [Tooltip("Super Sample Mode")]
        public CTAA_MODE SupersampleMode = CTAA_MODE.STANDARD;
        [Space(5)]

        public bool MSAA_Control = false;
        public int m_MSAA_Level = 0;

        private Vector4 delValues = new Vector4(0.01f, 2.0f, 0.5f, 0.3f);
        //--------------------------------------------------------------

        public bool PreEnhanceEnabled = true;
        public float preEnhanceStrength = 1.0f;
        public float preEnhanceClamp = 0.005f;
        public const float AdaptiveResolve = 3000.0f;
        private float jitterScale = 1.0f;

        //Masking feature. Seems useless
        //private Shader maskRenderShader;
        //private Material layerPostMat;
        public Material ctaaMat;
        public Material mat_enhance;
        public RenderTexture rtAccum0;
        public RenderTexture rtAccum1;
        public RenderTexture afterPreEnhace;
        private RenderTexture upScaleRT;

        public bool firstFrame;
        public bool swap;
        private int frameCounter;
        internal Vector2 jitterThisFrame;

        private static readonly float[] x_jit = { 0.5f, -0.25f, 0.75f, -0.125f, 0.625f, 0.575f, -0.875f, 0.0625f, -0.3f, 0.75f, -0.25f, -0.625f, 0.325f, 0.975f, -0.075f, 0.625f };
        private static readonly float[] y_jit = { 0.33f, -0.66f, 0.51f, 0.44f, -0.77f, 0.12f, -0.55f, 0.88f, -0.83f, 0.14f, 0.71f, -0.34f, 0.87f, -0.12f, 0.75f, 0.08f };

        public bool moveActive = true;
        public float speed = 0.002f;
        private int count = 0;

        public Camera mainCamera;

        AssetBundle assetBundle;

        //For detecting SS mode and screen size change.
        private CTAA_MODE prev_SupersampleMode;
        private Vector2Int prev_ScreenXY;
        private Vector2Int orig_ScreenXY;


        private void Awake()
        {
            SetResources();

            firstFrame = true;
            swap = true;
            frameCounter = 0;

            SetCTAA_Parameters();

            //mainCamera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            SetResources();

            firstFrame = true;
            swap = true;
            frameCounter = 0;

            SetCTAA_Parameters();

            mainCamera = GetComponent<Camera>();
            mainCamera.depthTextureMode |= DepthTextureMode.Depth;
            mainCamera.depthTextureMode |= DepthTextureMode.MotionVectors;
        }

        private void Start()
        {
            mainCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
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

        /*private void OnPreRender()
        {
            SetCTAA_Parameters();
        }*/

        private void OnPreCull()
        {
            if (CTAA_Enabled == false)
                return;
            //SetCB();
            if (ChangedSuperSamplingMode() ||
                ChangedScreenSize() ||
                NullRT())
            {
                SetCTAA_Cameras();
                SetRT();
            }
            if (SupersampleMode > CTAA_MODE.STANDARD)
            {
                mainCamera.targetTexture = upScaleRT;
            }
            JitterCam();
        }

        /*private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (Enabled)
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
        }*/

        private void OnPostRender()
        {
            //Write vars
            WriteScreenSize();
            WriteSuperSamplingMode();
        }

        private void OnDisable()
        {
            ClearResources();
            ClearRT();
            ClearCTAA_Cameras();

            // TODO: why do this here?
            mainCamera.ResetWorldToCameraMatrix();     // < ----- Unity 2017 Up
            mainCamera.ResetProjectionMatrix();        // < ----- Unity 2017 Up

            mainCamera.nonJitteredProjectionMatrix = mainCamera.projectionMatrix;

        }

        private void OnDestroy()
        {
            ClearResources();
            ClearRT();
            ClearCTAA_Cameras();
        }

        public void SetCTAA_Parameters()
        {
            PreEnhanceEnabled = AdaptiveSharpness > 0.01 ? true : false;
            preEnhanceStrength = Mathf.Lerp(0.2f, 2.0f, AdaptiveSharpness);
            preEnhanceClamp = Mathf.Lerp(0.005f, 0.12f, AdaptiveSharpness);
            jitterScale = TemporalJitterScale;
            //It's not a function for gaming. Don't use it.
            ctaaMat.SetFloat(CTAA_ShaderIDs._AntiShimmer, (AntiShimmerMode ? 1.0f : 0.0f));
            ctaaMat.SetVector(CTAA_ShaderIDs._delValues, delValues);
        }

        private void SetResources()
        {
            if (assetBundle == null)
            {
                //V2 style
                //assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("ctaa.unity3d"));
                //V3 style
                assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("ctaav3.unity3d"));
            }

            // V2 style
            //if (ctaaMat == null) ctaaMat = new Material(assetBundle.LoadAsset<Shader>("assets/shaders/ctaa_pc.shader"));
            //if (mat_enhance == null) mat_enhance = new Material(assetBundle.LoadAsset<Shader>("assets/shaders/ctaa_enhance_pc.shader"));

            // V3 style
            if (ctaaMat == null) ctaaMat = new Material(assetBundle.LoadAsset<Shader>("Assets/CTAAV3/CTAA_PC.shader")) { hideFlags = HideFlags.HideAndDontSave };
            if (mat_enhance == null) mat_enhance = new Material(assetBundle.LoadAsset<Shader>("Assets/CTAAV3/CTAA_Enhance_PC.shader")) { hideFlags = HideFlags.HideAndDontSave };
        }

        private void ClearResources()
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
                                     RenderTextureFormat renderTextureFormat, RenderTextureReadWrite renderTextureReadWrite,
                                     FilterMode filterMode, TextureWrapMode wrapMode, bool preCreateRT = true)
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
                    mainCamera.targetTexture = null;
            }

            var targetRT = mainCamera.targetTexture;
            if (targetRT)
            {
                mainCamera.targetTexture = null;
                orig_ScreenXY.Set(targetRT.width, targetRT.height);
                mainCamera.targetTexture = targetRT;
            }
            else
            {
                mainCamera.targetTexture = null;
                orig_ScreenXY.Set(mainCamera.pixelWidth, mainCamera.pixelHeight);
            }

            m_LayerRenderCam.enabled = (SupersampleMode > CTAA_MODE.STANDARD) ? true : false;
            int depth = 0;
            bool preCreate = true;

            // TODO: Check RenderTextureFormat, RenderTextureReadWrite, FilterMode, and TextureWrapMode again.
            UpdateRT(ref upScaleRT, "RT_Upscale",
                (SupersampleMode > CTAA_MODE.STANDARD) ? orig_ScreenXY.x << 1 : orig_ScreenXY.x,
                (SupersampleMode > CTAA_MODE.STANDARD) ? orig_ScreenXY.y << 1 : orig_ScreenXY.y,
                depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Repeat, preCreate);
            UpdateRT(ref rtAccum0, "RT_Accum0",
                (SupersampleMode == CTAA_MODE.CINA_ULTRA) ? orig_ScreenXY.x << 1 : orig_ScreenXY.x,
                (SupersampleMode == CTAA_MODE.CINA_ULTRA) ? orig_ScreenXY.y << 1 : orig_ScreenXY.y,
                depth, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Repeat, preCreate);
            UpdateRT(ref rtAccum1, "RT_Accum1",
                (SupersampleMode == CTAA_MODE.CINA_ULTRA) ? orig_ScreenXY.x << 1 : orig_ScreenXY.x,
                (SupersampleMode == CTAA_MODE.CINA_ULTRA) ? orig_ScreenXY.y << 1 : orig_ScreenXY.y,
                depth, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Repeat, preCreate);
            UpdateRT(ref afterPreEnhace, "RT_AfterPreEnhace",
                (SupersampleMode > CTAA_MODE.STANDARD) ? orig_ScreenXY.x << 1 : orig_ScreenXY.x,
                (SupersampleMode > CTAA_MODE.STANDARD) ? orig_ScreenXY.y << 1 : orig_ScreenXY.y,
                depth, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Clamp, preCreate);

            if (SupersampleMode > CTAA_MODE.STANDARD)
            {
                Graphics.Instance.Log.LogDebug($"CTAA updated. Supersample Mode: {prev_SupersampleMode} => {SupersampleMode} " +
                          $"Source Size: {orig_ScreenXY.x}x{orig_ScreenXY.y} => Upscaled Size: {orig_ScreenXY.x << 1}x{orig_ScreenXY.y << 1}");
            }
            else
            {
                Graphics.Instance.Log.LogDebug($"CTAA updated. Supersample Mode: {prev_SupersampleMode} => {SupersampleMode} " +
                          $"Source Size: {orig_ScreenXY.x}x{orig_ScreenXY.y}");
            }
        }

        private void ClearRT()
        {
            if (rtAccum0 != null) DestroyImmediate(rtAccum0); rtAccum0 = null;
            if (rtAccum1 != null) DestroyImmediate(rtAccum1); rtAccum1 = null;
            if (afterPreEnhace != null) DestroyImmediate(afterPreEnhace); afterPreEnhace = null;

            mainCamera.targetTexture = null;
            if (upScaleRT != null) DestroyImmediate(upScaleRT); upScaleRT = null;
        }

        private bool ChangedSuperSamplingMode() => prev_SupersampleMode != SupersampleMode;
        public void WriteSuperSamplingMode() => prev_SupersampleMode = SupersampleMode;
        private bool ChangedScreenSize() => (prev_ScreenXY.x != mainCamera.pixelWidth) || (prev_ScreenXY.y != mainCamera.pixelHeight);
        public void WriteScreenSize() { prev_ScreenXY.x = mainCamera.pixelWidth; prev_ScreenXY.y = mainCamera.pixelHeight; }
        private bool NullRT() => (upScaleRT == null) || (rtAccum0 == null) || (rtAccum1 == null) || (afterPreEnhace == null);

        #region CTAA Subcamera
        RenderPostCTAA renderPostCTAA;
        Camera m_LayerRenderCam;
        //Masking feature. Seems useless
        //Camera m_LayerMaskCam;
        public LayerMask m_ExcludeLayers;
        public bool m_LayerMaskingEnabled = true;

        private void JitterCam()
        {
            mainCamera.ResetWorldToCameraMatrix();     // < ----- Unity 2017 Up
            mainCamera.ResetProjectionMatrix();        // < ----- Unity 2017 Up

            mainCamera.nonJitteredProjectionMatrix = mainCamera.projectionMatrix;

            //mainCamera.ResetWorldToCameraMatrix();	// < ----- Unity 5.6 
            //mainCamera.ResetProjectionMatrix();      // < ----- Unity 5.6 

            Matrix4x4 matrixx = mainCamera.projectionMatrix;
            float num = x_jit[this.frameCounter] * jitterScale;
            float num2 = y_jit[this.frameCounter] * jitterScale;
            matrixx.m02 += ((num * 2f) - 1f) / mainCamera.pixelRect.width;
            matrixx.m12 += ((num2 * 2f) - 1f) / mainCamera.pixelRect.height;
            this.frameCounter++;
            this.frameCounter = this.frameCounter % 16;
            mainCamera.projectionMatrix = matrixx;
        }

        private void SetCTAA_Cameras()
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
                m_LayerRenderCam.CopyFrom(mainCamera);

                m_LayerRenderCam.transform.position = transform.position;
                m_LayerRenderCam.transform.rotation = transform.rotation;
                m_LayerRenderCam.cullingMask = m_ExcludeLayers;
                m_LayerRenderCam.depth = mainCamera.depth + 1;
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

        private void ClearCTAA_Cameras()
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

        public RenderTexture GetCTAA_Render()
        {
            return upScaleRT;
        }

        #endregion
    }
}