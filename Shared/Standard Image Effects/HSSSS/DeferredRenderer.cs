// HSSSS - pre-integrated and screen-space skin scattering with transmission, as Graphics shipped it
// (Shared/Standard Image Effects/Deferred Renderer); ported. It needs its own deferred shading
// pass, which HanmenSkin installs while any instance runs (HssssAssets.PassActive).
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Graphics;

namespace Graphics.HSSSS
{
    //[DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    internal class DeferredRenderer : MonoBehaviour
    {
        public enum LUTProfile
        {
            none,
            nvidia1,
            nvidia2
        }

        private LUTProfile _lutProfile = LUTProfile.none;
        public LUTProfile lutProfile
        {
            get => _lutProfile;
            set
            {
                if (_lutProfile != value)
                {
                    _lutProfile = value;
                    MarkDirty();
                }
            }
        }

        private float _skinLutIntensity = 1.0f;
        public float skinLutIntensity
        {
            get => _skinLutIntensity;
            set
            {
                if (_skinLutIntensity != value)
                {
                    _skinLutIntensity = value;
                    MarkDirty();
                }
            }
        }

        private float _skinLutBias = 0.0f;
        public float skinLutBias
        {
            get => _skinLutBias;
            set
            {
                if (_skinLutBias != value)
                {
                    _skinLutBias = value;
                    MarkDirty();
                }
            }
        }

        private float _skinLutScale = 0.5f;
        public float skinLutScale
        {
            get => _skinLutScale; 
            set
            {
                if (_skinLutScale != value)
                {
                    _skinLutScale = value;
                    MarkDirty();
                }
            }
        }

        private float _shadowLutBias = 0.0f;
        public float shadowLutBias
        {
            get => _shadowLutBias;
            set
            {
                if (_shadowLutBias != value)
                {
                    _shadowLutBias = value;
                    MarkDirty();
                }
            }
        }

        private float _shadowLutScale = 1.0f;
        public float shadowLutScale
        {
            get => _shadowLutScale;
            set
            {
                if (_shadowLutScale != value)
                {
                    _shadowLutScale = value;
                    MarkDirty();
                }
            }
        }

        private bool _screenSpaceSSS = true;
        public bool screenSpaceSSS
        {
            get => _screenSpaceSSS;
            set
            {
                if (_screenSpaceSSS != value)
                {
                    _screenSpaceSSS = value;
                    MarkDirty();
                }
            }
        }

        private float _sssBlurWeight = 1.0f;
        public float sssBlurWeight
        {
            get => _sssBlurWeight;
            set
            {
                if (_sssBlurWeight != value)
                {
                    _sssBlurWeight = value;
                    ResetCommandBuffers();
                }
            }
        }

        private float _sssBlurRadius = 50f;
        public float sssBlurRadius
        {
            get => _sssBlurRadius;
            set
            {
                if (_sssBlurRadius != value)
                {
                    _sssBlurRadius = value;
                    MarkDirty();
                }
            }
        }

        private float _sssBlurDepthRange = 0.0f;
        public float sssBlurDepthRange
        {
            get => _sssBlurDepthRange;
            set
            {
                if (_sssBlurDepthRange != value)
                {
                    _sssBlurDepthRange = value;
                    MarkDirty();
                }
            }
        }

        private int _sssBlurIter = 3;
        public int sssBlurIter
        {
            get => _sssBlurIter;
            set
            {
                if (_sssBlurIter != value)
                {
                    _sssBlurIter = value;
                    MarkDirty();
                }
            }
        }

        private bool _sssBlurAlbedo = true;
        public bool sssBlurAlbedo
        {
            get => _sssBlurAlbedo;
            set
            {
                if (_sssBlurAlbedo != value)
                {
                    _sssBlurAlbedo = value;
                    MarkDirty();
                }
            }
        }

        private Vector3 _colorBleedWeights = new Vector3(0.40f, 0.15f, 0.20f);
        public Vector3 colorBleedWeights
        {
            get => _colorBleedWeights;
            set
            {
                if (_colorBleedWeights != value)
                {
                    _colorBleedWeights = value;
                    MarkDirty();
                }
            }
        }

        private Vector3 _transAbsorption = new Vector3(-8.00f, -48.0f, -64.0f);
        public Vector3 transAbsorption
        {
            get => _transAbsorption;
            set
            {
                if (_transAbsorption != value)
                {
                    _transAbsorption = value;
                    MarkDirty();
                }
            }
        }

        private bool _bakedThickness = true;
        public bool bakedThickness
        {
            get => _bakedThickness;
            set
            {
                if (_bakedThickness != value)
                {
                    _bakedThickness = value;
                    MarkDirty();
                }
            }
        }

        private float _transWeight = 1.0f;
        public float transWeight
        {
            get => _transWeight;
            set
            {
                if (_transWeight != value)
                {
                    _transWeight = value;
                    MarkDirty();
                }
            }
        }

        private float _transShadowWeight = 1.0f;
        public float transShadowWeight
        {
            get => _transShadowWeight;
            set
            {
                if (_transShadowWeight != value)
                {
                    _transShadowWeight = value;
                    MarkDirty();
                }
            }
        }

        private float _transDistortion = 0.5f;
        public float transDistortion
        {
            get => _transDistortion;
            set
            {
                if (_transDistortion != value)
                {
                    _transDistortion = value;
                    MarkDirty();
                }
            }
        }

        private float _transFalloff = 2.0f;
        public float transFalloff
        {
            get => _transFalloff;
            set
            {
                if (_transFalloff != value)
                {
                    _transFalloff = value;
                    MarkDirty();
                }
            }
        }

        private float _thicknessBias = 0.5f;
        public float thicknessBias
        {
            get => _thicknessBias;
            set
            {
                if (_thicknessBias != value)
                {
                    _thicknessBias = value;
                    MarkDirty();
                }
            }
        }

        private bool _microDetails = false;
        public bool microDetails
        {
            get => _microDetails;
            set
            {
                if (_microDetails != value)
                {
                    _microDetails = value;
                    MarkDirty();
                }
            }
        }

        [Range(0, 1)]
        private float _normalTest = 0.85f;
        public float normalTest
        {
            get => _normalTest;
            set
            {
                if (_normalTest != value)
                {
                    _normalTest = value;
                    MarkDirty();
                }
            }
        }

        private float _depthTest = 0.85f;
        public float depthTest
        {
            get => _depthTest;
            set
            {
                if (_depthTest != value)
                {
                    _depthTest = value;
                    MarkDirty();
                }
            }
        }

        //microDetailWeight_1 = new KeyValue<int, float>(Shader.PropertyToID("_DetailNormalMapScale_2"), 0.5f),
        //microDetailWeight_2 = new KeyValue<int, float>(Shader.PropertyToID("_DetailNormalMapScale_3"), 0.5f),
        //microDetailOcclusion = new KeyValue<int, float>(Shader.PropertyToID("_PoreOcclusionStrength"), 0.5f),

        //public float microDetailTiling = 64.0f;
        [Range(0, 1)]
        private float _microDetailTiling = 0.5f;
        public float microDetailTiling
        {
            get => _microDetailTiling;
            set
            {
                if (_microDetailTiling != value)
                {
                    _microDetailTiling = value;
                    MarkDirty();
                }
            }
        }

        // Shared across instances and loaded once per session: the deferred shader is
        // installed globally, so these must survive any single component's destruction.
        private static Texture2D
            nvidiaDiffuse,
            nvidiaShadow,
            deepScatter;

        private Camera mCamera;

        private CommandBuffer copyBuffer;
        private CommandBuffer blurBuffer;
        private CommandBuffer maskBuffer;
        private CommandBuffer releaseBuffer;

        private const string CopyBufferName = "HSSSS.SSSPrePass";
        private const string BlurBufferName = "HSSSS.SSSMainPass";
        private const string MaskBufferName = "HSSSS.SkinMask";
        private const string ReleaseBufferName = "HSSSS.SkinMaskRelease";

        private static Material prePass;
        private static Material mainPass;

        private static int count;
        //private ComputeShader specularSeparationCS;

        private struct RGBTextures
        {
            public RenderTexture R;
            public RenderTexture G;
            public RenderTexture B;
        }

        private int cachedBlurIter = -1;
        private int cachedWidth = -1;
        private int cachedHeight = -1;
        private bool propertiesDirty = true;

        private RGBTextures specular;
        private static Shader prePassShader, mainPassShader;

        private static class ShaderIDs
        {
            public static readonly int FrameCount = Shader.PropertyToID("_FrameCount");
            public static readonly int WorldToView = Shader.PropertyToID("_WorldToViewMatrix");
            public static readonly int ViewToWorld = Shader.PropertyToID("_ViewToWorldMatrix");
            public static readonly int ViewToClip = Shader.PropertyToID("_ViewToClipMatrix");
            public static readonly int ClipToView = Shader.PropertyToID("_ClipToViewMatrix");
            public static readonly int DeferredSkinParams = Shader.PropertyToID("_DeferredSkinParams");
            public static readonly int DeferredShadowParams = Shader.PropertyToID("_DeferredShadowParams");
            public static readonly int DeferredBlurredNormalsParams = Shader.PropertyToID("_DeferredBlurredNormalsParams");
            public static readonly int DeferredSkinColorBleedAoWeights = Shader.PropertyToID("_DeferredSkinColorBleedAoWeights");
            public static readonly int DeferredSkinTransmissionAbsorption = Shader.PropertyToID("_DeferredSkinTransmissionAbsorption");
            public static readonly int DeferredBlurredNormalBuffer = Shader.PropertyToID("_DeferredBlurredNormalBuffer");
            public static readonly int DeferredTransmissionParams = Shader.PropertyToID("_DeferredTransmissionParams");
            public static readonly int DeferredSkinLut = Shader.PropertyToID("_DeferredSkinLut");
            public static readonly int DeferredShadowLut = Shader.PropertyToID("_DeferredShadowLut");
            public static readonly int BlurAlbedoTexture = Shader.PropertyToID("_BlurAlbedoTexture");
            public static readonly int DeferredTransmissionLut = Shader.PropertyToID("_DeferredTransmissionLut");
            public static readonly int DeferredThicknessBias = Shader.PropertyToID("_DeferredThicknessBias");
            public static readonly int copyRT = Shader.PropertyToID("_DeferredTransmissionBuffer");
            public static readonly int flipRT = Shader.PropertyToID("_TemporaryFlipRenderTexture");
            public static readonly int flopRT = Shader.PropertyToID("_TemporaryFlopRenderTexture");
            // Snapshot of GBuffer2 taken BEFORE decals get to modify it - see MaskBufferName.
            public static readonly int skinMaskRT = Shader.PropertyToID("_HSSSSSkinMaskRT");
            public static readonly int SkinMaskBuffer = Shader.PropertyToID("_HSSSSSkinMaskBuffer");
            public static readonly int frameCount = Shader.PropertyToID("_FrameCount");
        }

        public void Awake()
        {
            // Load shaders and LUTs once per session; the bundles are unloaded immediately
            // after extraction (loaded assets stay alive), so repeated component creation on
            // scene/camera changes no longer re-loads or leaks the in-memory bundles.
            if (prePassShader == null)
            {
                prePassShader = HssssAssets.Load<Shader>("hssss-pass.unity3d", "Assets/HSSSS/Shaders/PostFX/SSS/SSSPrePass.shader");
                mainPassShader = HssssAssets.Load<Shader>("hssss-pass.unity3d", "Assets/HSSSS/Shaders/PostFX/SSS/SSSMainPass.shader");
                nvidiaDiffuse = HssssAssets.Load<Texture2D>("hssss-luts.unity3d", "Assets/HSSSS/Resources/Textures/LUT/FaceWorksSkinLUT.bmp");
                nvidiaShadow = HssssAssets.Load<Texture2D>("hssss-luts.unity3d", "Assets/HSSSS/Resources/Textures/LUT/FaceWorksShadowLUT.bmp");
                deepScatter = HssssAssets.Load<Texture2D>("hssss-luts.unity3d", "Assets/HSSSS/Resources/Textures/LUT/DeepScatterLUT.png");
            }

            if (prePass == null)
                prePass = new Material(prePassShader);
            if (mainPass == null)
                mainPass = new Material(mainPassShader);

            count = 1;
        }

        // Main camera + maker card capture camera can both hold an instance;
        // the global HSSSS state must only flip off when the last one goes away.
        private static int activeInstances;

        public void OnEnable()
        {
            this.mCamera = GetComponent<Camera>();
            if (!CanRenderHSSSS())
            {
                enabled = false;
                return;
            }

            activeInstances++;
            HssssAssets.PassActive = true;
            propertiesDirty = true;

            this.SetupCommandBuffers();
        }

        private void Update()
        {
            // Optimized version - update properties only on change
            if (propertiesDirty)
            {
                RefreshProperties();
                //Console.WriteLine("DeferredRenderer properties updated.");
                propertiesDirty = false;
            }

            if (cachedBlurIter != sssBlurIter)
            {
                cachedBlurIter = sssBlurIter;
                RemoveCommandBuffers();
                SetupCommandBuffers();

                if (screenSpaceSSS)
                {
                    RemoveSpecularRT();
                }
            }
        }

        private void OnDestroy()
        {
            // prePass/mainPass and the extracted shaders/LUTs are static session-wide
            // resources — destroying them here would break any other (or future) instance,
            // e.g. when a scene change creates the new main camera before the old one dies.
            RemoveCommandBuffers();
            RemoveSpecularRT();
        }

        public void OnDisable()
        {
            activeInstances = Mathf.Max(0, activeInstances - 1);
            if (activeInstances == 0)
            {
                HssssAssets.PassActive = false;
            }
            RemoveCommandBuffers();
            RemoveSpecularRT();
        }

        //Original method.
        private void OnPreRender()
        {
            Shader.SetGlobalInt(ShaderIDs.frameCount, count);
            count = (count + 1) % 64;

            //Update projection matrices (CRITICAL!)
            Shader.SetGlobalMatrix(ShaderIDs.WorldToView, mCamera.worldToCameraMatrix);
            Shader.SetGlobalMatrix(ShaderIDs.ViewToWorld, mCamera.worldToCameraMatrix.inverse);
            Shader.SetGlobalMatrix(ShaderIDs.ViewToClip, mCamera.projectionMatrix);
            Shader.SetGlobalMatrix(ShaderIDs.ClipToView, mCamera.projectionMatrix.inverse);

            if (screenSpaceSSS)
            {
                SetupSpecularRT();
            }
        }

        private void OnPostRender() //Original
        {
            if (screenSpaceSSS)
            {
                // The specular buffers are cached and reused across frames now —
                // only the UAV bindings are cleared here.
                ClearRandomWriteTargets();
            }
        }

        private static RenderTexture CreateSpecularRT(int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                autoGenerateMips = false
            };
            rt.Create();
            return rt;
        }

        private void SetupSpecularRT()
        {
            int width = mCamera.pixelWidth;
            int height = mCamera.pixelHeight;

            if (mCamera.targetTexture)
            {
                width = mCamera.targetTexture.width;
                height = mCamera.targetTexture.height;
            }

            // Allocating three full-resolution textures every frame caused both a large
            // per-frame cost and an unbounded pile-up of released-but-undestroyed
            // RenderTexture objects. Reuse the buffers and only recreate on resolution change.
            if (specular.R == null || cachedWidth != width || cachedHeight != height)
            {
                RemoveSpecularRT();

                cachedWidth = width;
                cachedHeight = height;

                specular.R = CreateSpecularRT(width, height);
                specular.G = CreateSpecularRT(width, height);
                specular.B = CreateSpecularRT(width, height);
            }
            else
            {
                // Recreate native textures if they were lost (device reset, alt-tab, etc.)
                if (!specular.R.IsCreated()) specular.R.Create();
                if (!specular.G.IsCreated()) specular.G.Create();
                if (!specular.B.IsCreated()) specular.B.Create();
            }

            // Bind EVERY render, not just on creation: with two HSSSS cameras (main + planar
            // reflection) a creation-time-only binding left the globals pointing at whichever
            // instance allocated last, so the other camera's SSS collect pass read the wrong
            // camera's specular buffers.
            specular.R.SetGlobalShaderProperty("_SpecularBufferR");
            specular.G.SetGlobalShaderProperty("_SpecularBufferG");
            specular.B.SetGlobalShaderProperty("_SpecularBufferB");

            // Clear last frame's contents before the lighting pass writes into them
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = specular.R;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = specular.G;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = specular.B;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = rt;

            //ClearRandomWriteTargets();
            SetRandomWriteTarget(1, specular.R);
            SetRandomWriteTarget(2, specular.G);
            SetRandomWriteTarget(3, specular.B);
        }

        private void RemoveSpecularRT()
        {
            ClearRandomWriteTargets();

            if (specular.R)
            {
                specular.R.Release();
                DestroyImmediate(specular.R);
                specular.R = null;
            }

            if (specular.G)
            {
                specular.G.Release();
                DestroyImmediate(specular.G);
                specular.G = null;
            }

            if (specular.B)
            {
                specular.B.Release();
                DestroyImmediate(specular.B);
                specular.B = null;
            }

        }

        #region Properties Control
        private void RefreshSkinProperties()
        {
            Shader.SetGlobalVector(ShaderIDs.DeferredSkinParams,
                new Vector4(
                    skinLutIntensity,
                    skinLutBias,
                    skinLutScale,
                    sssBlurWeight
                    ));
            Shader.SetGlobalVector(ShaderIDs.DeferredShadowParams,
                new Vector2(
                    shadowLutBias,
                    shadowLutScale
                    ));
            Shader.SetGlobalVector(ShaderIDs.DeferredSkinColorBleedAoWeights, colorBleedWeights);
            Shader.SetGlobalVector(ShaderIDs.DeferredSkinTransmissionAbsorption, transAbsorption);
        }

        private void RefreshBlurProperties()
        {
            mainPass.SetVector(ShaderIDs.DeferredBlurredNormalsParams,
                new Vector4(
                    sssBlurRadius,
                    sssBlurDepthRange * 100.0f,
                    normalTest,
                    depthTest * 0.05f
                    ));
            mainPass.SetInt(ShaderIDs.BlurAlbedoTexture, sssBlurAlbedo ? 1 : 0);
        }

        private void RefreshLookupProperties()
        {
            Shader.DisableKeyword("_FACEWORKS_TYPE1");
            Shader.DisableKeyword("_FACEWORKS_TYPE2");
            Shader.DisableKeyword("_SCREENSPACE_SSS");
            Shader.DisableKeyword("_NO_SKIN_LUT");

            // lookup texture replacement
            switch (lutProfile)
            {
                case LUTProfile.none:

                    Shader.SetGlobalTexture(ShaderIDs.DeferredSkinLut, null);
                    Shader.SetGlobalTexture(ShaderIDs.DeferredShadowLut, null);
                    Shader.DisableKeyword("_FACEWORKS_TYPE1");
                    Shader.DisableKeyword("_FACEWORKS_TYPE2");
                    // Tells the skin BRDF there is no LUT to pre-integrate with, so it uses plain
                    // diffuse instead of sampling the null texture bound just above. Without this the
                    // shader read an unbound sampler and the skin's whole scattering term - and any
                    // decal drawn on it - came out wrong, which is why only the nvidia profiles
                    // looked correct.
                    Shader.EnableKeyword("_NO_SKIN_LUT");
                    break;

                case LUTProfile.nvidia1:
                    Shader.EnableKeyword("_FACEWORKS_TYPE1");
                    Shader.SetGlobalTexture(ShaderIDs.DeferredSkinLut, nvidiaDiffuse);
                    break;

                case LUTProfile.nvidia2:
                    Shader.EnableKeyword("_FACEWORKS_TYPE2");
                    Shader.SetGlobalTexture(ShaderIDs.DeferredSkinLut, nvidiaDiffuse);
                    Shader.SetGlobalTexture(ShaderIDs.DeferredShadowLut, nvidiaShadow);
                    break;

                //case LUTProfile.jimenez:
                //    Shader.EnableKeyword("_SCREENSPACE_SSS");
                //    break;

            }
            if (screenSpaceSSS)
            {
                Shader.EnableKeyword("_SCREENSPACE_SSS");
            }
        }

        private void RefreshTransmissionProperties()
        {
            Shader.DisableKeyword("_BAKED_THICKNESS");

            if (bakedThickness)
            {
                Shader.EnableKeyword("_BAKED_THICKNESS");
            }

            else
            {
                Shader.SetGlobalTexture(ShaderIDs.DeferredTransmissionLut, deepScatter);
                Shader.SetGlobalFloat(ShaderIDs.DeferredThicknessBias, thicknessBias * 0.01f);
            }

            Shader.SetGlobalVector(ShaderIDs.DeferredTransmissionParams,
                new Vector4(
                    transWeight,
                    transFalloff,
                    transDistortion,
                    transShadowWeight
                    ));
        }

        private void RefreshProperties()
        {
            this.RefreshSkinProperties();
            this.RefreshBlurProperties();
            this.RefreshLookupProperties();
            this.RefreshTransmissionProperties();
        }
        #endregion

        #region Commandbuffer Control

        private bool CanRenderHSSSS()
        {
            if (!isActiveAndEnabled)
                return false;

            if (!mCamera)
                mCamera = GetComponent<Camera>();

            return mCamera && mCamera.actualRenderingPath == RenderingPath.DeferredShading;
        }

        private void SetupDiffuseBlurBuffer()
        {
            //int copyRT = Shader.PropertyToID("_DeferredTransmissionBuffer");
            //int flipRT = Shader.PropertyToID("_TemporaryFlipRenderTexture");
            //int flopRT = Shader.PropertyToID("_TemporaryFlopRenderTexture");

            ///////////////////////////////////
            ///////////////////////////////////
            // transmission & ambient lights //
            ///////////////////////////////////
            ///////////////////////////////////

            this.copyBuffer = new CommandBuffer() { name = CopyBufferName }; //Original Method

            // get temporary render textures
            this.copyBuffer.GetTemporaryRT(ShaderIDs.copyRT, -1, -1, 0, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
            // extract thickness map from g-buffer 3
            this.copyBuffer.Blit(BuiltinRenderTextureType.CameraTarget, ShaderIDs.copyRT, prePass, 0);
            // AfterGBuffer, NOT BeforeLighting. SSSPrePass reads the emission buffer's ALPHA as the
            // transmission thickness, and the decal system writes its own opacity into exactly that
            // alpha at BeforeReflections (outEmission = float4(rgb, col.a), alpha-blended). Reading
            // it after the decals had run meant a decal's opacity became the skin's thickness, so
            // the whole decal footprint lit up as maximum translucency with a halo where its alpha
            // fell off. The G-buffer is complete here and no decal has touched it yet.
            this.mCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, copyBuffer);


            ///////////////////////////////////
            ///////////////////////////////////
            //// screen space diffuse blur ////
            ///////////////////////////////////
            ///////////////////////////////////
            ///
            blurBuffer = new CommandBuffer() { name = BlurBufferName };

            // get temporary render textures
            blurBuffer.GetTemporaryRT(ShaderIDs.flipRT, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            blurBuffer.GetTemporaryRT(ShaderIDs.flopRT, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

            blurBuffer.Blit(BuiltinRenderTextureType.CurrentActive, ShaderIDs.flipRT, mainPass, 0);

            // separable blur
            for (int i = 0; i < sssBlurIter; i++)
            {
                blurBuffer.Blit(ShaderIDs.flipRT, ShaderIDs.flopRT, mainPass, 1);
                blurBuffer.Blit(ShaderIDs.flopRT, ShaderIDs.flipRT, mainPass, 2);
            }

            // collect all lighting
            blurBuffer.Blit(ShaderIDs.flipRT, ShaderIDs.flopRT, mainPass, 3);

            // to camera target
            blurBuffer.Blit(ShaderIDs.flopRT, BuiltinRenderTextureType.CameraTarget);

            // release render textures
            blurBuffer.ReleaseTemporaryRT(ShaderIDs.flipRT);
            blurBuffer.ReleaseTemporaryRT(ShaderIDs.flopRT);
            blurBuffer.ReleaseTemporaryRT(ShaderIDs.copyRT);

            // add command buffer
            mCamera.AddCommandBuffer(CameraEvent.AfterFinalPass, blurBuffer);
        }

        private void SetupNormalBlurBuffer()
        {
            ///////////////////////////////////////
            ///////////////////////////////////////
            //// transmission & ambient lights ////
            ///////////////////////////////////////
            ///////////////////////////////////////

            //int copyRT = Shader.PropertyToID("_DeferredTransmissionBuffer");

            copyBuffer = new CommandBuffer() { name = CopyBufferName };
            // get temporary render textures
            copyBuffer.GetTemporaryRT(ShaderIDs.copyRT, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            // extract thickness map from g-buffer 3
            copyBuffer.Blit(BuiltinRenderTextureType.CameraTarget, ShaderIDs.copyRT, prePass, 0);
            // AfterGBuffer, not BeforeLighting - see the note on the other copyBuffer above. The
            // decal system overwrites the emission alpha this reads as thickness.
            mCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, copyBuffer);

            //////////////////////////////////
            //////////////////////////////////
            //// screen space normal blur ////
            //////////////////////////////////
            //////////////////////////////////

            //////////////////////////////////
            //// skin mask snapshot       ////
            //////////////////////////////////
            //
            // HSSSS decides what to blur from GBuffer2's ALPHA: NormalBlur treats alpha == 0 as skin
            // and anything above 0 as "leave alone". Nothing else in stock Unity deferred uses that
            // channel - but the decal system does. Its passes write
            //     outNormal = float4(normal * 0.5 + 0.5, normalBlendFactor)
            // into GBuffer2 with Blend SrcAlpha OneMinusSrcAlpha, so normalBlendFactor lands
            // directly in the skin mask:
            //   - Normal Mask on        -> alpha = diffuse.a, so the decal footprint reads as
            //                              NOT-skin, the blur stops there but continues around it,
            //                              and a ring appears at the boundary.
            //   - Normal Edge Blending  -> alpha = 1 - dist^3, a radial gradient, so the blur fades
            //                              in and out by radius and the intended edge blend is lost.
            // With SSS off nothing reads that channel, which is why decals look right there.
            //
            // Decals run at BeforeReflections and the blur at BeforeLighting, so by the time HSSSS
            // reads the mask it has already been overwritten. Snapshot it at AfterGBuffer instead -
            // the G-buffer is complete and no decal has touched it yet.
            maskBuffer = new CommandBuffer() { name = MaskBufferName };
            // ARGB32, and do NOT "match GBuffer2" by switching this to ARGB2101010.
            //
            // That was tried and regressed everything: the profile ID read back wrong, so the rings
            // returned on skin AND on ordinary objects, under every LUT profile. ARGB2101010 carries
            // only two bits of alpha and a default Blit into it does not reliably preserve them,
            // which destroys the very value this snapshot exists to protect. Eight-bit alpha holds
            // the four profile levels exactly.
            //
            // The cost is that RGB is quantised from 10 bits to 8, so the decal test in
            // StandardSkin.cginc must tolerate roughly 0.014 of difference - see the threshold there.
            maskBuffer.GetTemporaryRT(ShaderIDs.skinMaskRT, -1, -1, 0, FilterMode.Point,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            maskBuffer.Blit(BuiltinRenderTextureType.GBuffer2, ShaderIDs.skinMaskRT);
            maskBuffer.SetGlobalTexture(ShaderIDs.SkinMaskBuffer, ShaderIDs.skinMaskRT);
            this.mCamera.AddCommandBuffer(CameraEvent.AfterGBuffer, maskBuffer);

            blurBuffer = new CommandBuffer() { name = BlurBufferName };

            if (sssBlurIter > 0)
            {
                //int flipRT = Shader.PropertyToID("_TemporaryFlipRenderTexture");
                //int flopRT = Shader.PropertyToID("_TemporaryFlopRenderTexture");

                blurBuffer.GetTemporaryRT(ShaderIDs.flipRT, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);
                blurBuffer.GetTemporaryRT(ShaderIDs.flopRT, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010, RenderTextureReadWrite.Linear);

                blurBuffer.Blit(BuiltinRenderTextureType.GBuffer2, ShaderIDs.flipRT, mainPass, 4);
                blurBuffer.Blit(ShaderIDs.flipRT, ShaderIDs.flopRT, mainPass, 5);

                for (int i = 1; i < sssBlurIter; i++)
                {
                    blurBuffer.Blit(ShaderIDs.flopRT, ShaderIDs.flipRT, mainPass, 4);
                    blurBuffer.Blit(ShaderIDs.flipRT, ShaderIDs.flopRT, mainPass, 5);
                }

                blurBuffer.SetGlobalTexture(ShaderIDs.DeferredBlurredNormalBuffer, ShaderIDs.flopRT);

                blurBuffer.ReleaseTemporaryRT(ShaderIDs.flipRT);
                blurBuffer.ReleaseTemporaryRT(ShaderIDs.flopRT);
            }

            else
            {
                blurBuffer.SetGlobalTexture(ShaderIDs.DeferredBlurredNormalBuffer, BuiltinRenderTextureType.GBuffer2);
            }

            this.mCamera.AddCommandBuffer(CameraEvent.BeforeLighting, blurBuffer);

            // Released at AfterEverything, NOT in blurBuffer and NOT at AfterLighting.
            //
            // Two passes read this snapshot and they sit at opposite ends of the frame: the deferred
            // SHADING pass (just after BeforeLighting) takes its scattering profile from it, and the
            // screen-space SSS diffuse split - SetupDiffuseBlurBuffer, at AfterFinalPass - takes its
            // skin mask from it. AfterLighting was late enough for the first and far too early for
            // the second, which would have handed the blur a dead texture.
            releaseBuffer = new CommandBuffer() { name = ReleaseBufferName };
            releaseBuffer.ReleaseTemporaryRT(ShaderIDs.skinMaskRT);
            this.mCamera.AddCommandBuffer(CameraEvent.AfterEverything, releaseBuffer);
        }

        public void ResetCommandBuffers()
        {
            this.RemoveCommandBuffers();
            if (!CanRenderHSSSS())
                return;

            this.SetupCommandBuffers();
        }
        private void SetupCommandBuffers()
        {
            if (!CanRenderHSSSS())
                return;

            //else
            //{
                // buffer 1: screen space scattering
                if (screenSpaceSSS)
                {
                    this.SetupDiffuseBlurBuffer();
                }

                // buffer 2: pre-integrated scattering
                //else
                //{
                    this.SetupNormalBlurBuffer();
            //    }
            //}
        }

        private void RemoveCommandBuffers()
        {
            if (!mCamera)
            {
                this.copyBuffer = null;
                this.blurBuffer = null;
                this.maskBuffer = null;
                this.releaseBuffer = null;
                return;
            }

            // AfterLighting is swept as well as AfterEverything: the release buffer used to live
            // there, so a camera set up by an older build can still be carrying one.
            foreach (CameraEvent stage in new[] { CameraEvent.AfterEverything, CameraEvent.AfterLighting })
            {
                foreach (CommandBuffer buffer in this.mCamera.GetCommandBuffers(stage))
                {
                    if (buffer.name == ReleaseBufferName)
                    {
                        this.mCamera.RemoveCommandBuffer(stage, buffer);
                        buffer.Release();
                    }
                }
            }

            // Both the skin-mask snapshot and the thickness extraction live here now, ahead of the
            // decal system's writes at BeforeReflections.
            foreach (CommandBuffer buffer in this.mCamera.GetCommandBuffers(CameraEvent.AfterGBuffer))
            {
                if (buffer.name == MaskBufferName || buffer.name == CopyBufferName)
                {
                    this.mCamera.RemoveCommandBuffer(CameraEvent.AfterGBuffer, buffer);
                    buffer.Release();
                }
            }

            foreach (CommandBuffer buffer in this.mCamera.GetCommandBuffers(CameraEvent.BeforeLighting))
            {
                if (buffer.name == CopyBufferName || buffer.name == BlurBufferName)
                {
                    this.mCamera.RemoveCommandBuffer(CameraEvent.BeforeLighting, buffer);
                    buffer.Release();
                }
            }

            foreach (CommandBuffer buffer in this.mCamera.GetCommandBuffers(CameraEvent.AfterFinalPass))
            {
                if (buffer.name == BlurBufferName)
                {
                    this.mCamera.RemoveCommandBuffer(CameraEvent.AfterFinalPass, buffer);
                    buffer.Release();
                }
            }

            this.copyBuffer = null;
            this.blurBuffer = null;
            this.maskBuffer = null;
            this.releaseBuffer = null;
        }
        #endregion

        #region Interfaces
        public void UpdateSkinSettings()
        {
            this.RefreshSkinProperties();
            this.RefreshBlurProperties();
            this.RefreshLookupProperties();
            this.RefreshTransmissionProperties();

            this.RemoveCommandBuffers();
            if (CanRenderHSSSS())
                this.SetupCommandBuffers();

            if (screenSpaceSSS)
            {
                this.RemoveSpecularRT();
            }
        }

        public void MarkDirty() => propertiesDirty = true;

        #endregion


    }

}
