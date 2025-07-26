//using BehaviorDesigner.Runtime.Tasks.Unity.UnityLight;
using KKAPI.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering;

//https://github.com/sonicether/SEGI
namespace Graphics.SEGI
{

    [RequireComponent(typeof(Camera))]
    public class SEGI : MonoBehaviour
    {

        #region Parameters
        //[Serializable]
        public enum VoxelResolution
        {
            low = 128,
            high = 256
        }

        public bool updateGI = true;
        public LayerMask giCullingMask;
        public float shadowSpaceSize = 100.0f;
        public Light Sun
        {
            get => RenderSettings.sun;
            set => RenderSettings.sun = value;
        }

        public Color skyColor = Color.grey;

        public float voxelSpaceSize = 100.0f;

        public bool useBilateralFiltering = true;

        [Range(0, 2)]
        public int innerOcclusionLayers = 1;


        [Range(0.01f, 1.0f)]
        public float temporalBlendWeight = 0.1f;


        public VoxelResolution voxelResolution = VoxelResolution.high;

        [Flags]
        public enum DebugTools
        {
            Off = 1 << 0,
            GI = 1 << 1,
            Voxels = 1 << 2,
            SunDepthTexture = 1 << 3

        }
        public DebugTools debugTools = DebugTools.Off;

        public bool visualizeSunDepthTexture = false;
        public bool visualizeGI = false;
        public bool visualizeVoxels = false;

        public bool halfResolution = true;
        public bool stochasticSampling = true;
        public bool infiniteBounces = false;
        public Transform followTransform;
        [Range(1, 128)]
        public int cones = 35;
        [Range(1, 32)]
        public int coneTraceSteps = 8;
        [Range(0.1f, 2.0f)]
        public float coneLength = 1.0f;
        [Range(0.5f, 6.0f)]
        public float coneWidth = 4.0f;
        [Range(0.0f, 4.0f)]
        public float occlusionStrength = 0.5f;
        [Range(0.0f, 4.0f)]
        public float nearOcclusionStrength = 0.0f;
        [Range(0.001f, 4.0f)]
        public float occlusionPower = 0f;
        [Range(0.0f, 4.0f)]
        public float coneTraceBias = 1.0f;
        [Range(0.0f, 4.0f)]
        public float nearLightGain = 0.0f;
        [Range(0.0f, 4.0f)]
        public float giGain = 1.0f;
        [Range(0.0f, 4.0f)]
        public float secondaryBounceGain = 1.0f;
        [Range(0.0f, 16.0f)]
        public float softSunlight = 0.1f;

        [Range(0.0f, 8.0f)]
        public float skyIntensity = 0.7f;

        public bool doReflections = false;
        [Range(12, 128)]
        public int reflectionSteps = 64;
        [Range(0.001f, 4.0f)]
        public float reflectionOcclusionPower = 1.0f;
        [Range(0.0f, 1.0f)]
        public float skyReflectionIntensity = 1.0f;

        public bool voxelAA = false;

        public bool gaussianMipFilter = false;


        [Range(0.1f, 4.0f)]
        public float farOcclusionStrength = 1.0f;
        [Range(0.1f, 4.0f)]
        public float farthestOcclusionStrength = 1.0f;

        [Range(3, 16)]
        public int secondaryCones = 6;
        [Range(0.1f, 4.0f)]
        public float secondaryOcclusionStrength = 1.0f;

        public bool sphericalSkylight = true;

        #endregion

        #region InternalVariables
        object initChecker;
        private static Material material;
        Camera attachedCamera;
        Transform shadowCamTransform;
        Camera shadowCam;
        GameObject shadowCamGameObject;
        //public static Texture2D[] blueNoise;

        private readonly int sunShadowResolution = 256;
        private int prevSunShadowResolution;

        private static Shader segi;
        private static Shader sunDepthShader;
        private const float shadowSpaceDepthRatio = 10.0f;

        int frameCounter = 0;

        RenderTexture sunDepthTexture;
        RenderTexture previousGIResult;
        RenderTexture previousCameraDepth;

        ///<summary>This is a volume texture that is immediately written to in the voxelization shader. The RInt format enables atomic writes to avoid issues where multiple fragments are trying to write to the same voxel in the volume.</summary>
        RenderTexture integerVolume;

        ///<summary>An array of volume textures where each element is a mip/LOD level. Each volume is half the resolution of the previous volume. Separate textures for each mip level are required for manual mip-mapping of the main GI volume texture.</summary>
        RenderTexture[] volumeTextures;

        ///<summary>The secondary volume texture that holds irradiance calculated during the in-volume GI tracing that occurs when Infinite Bounces is enabled. </summary>
        RenderTexture secondaryIrradianceVolume;

        ///<summary>The alternate mip level 0 main volume texture needed to avoid simultaneous read/write errors while performing temporal stabilization on the main voxel volume.</summary>
        //RenderTexture volumeTextureB;

        ///<summary>The current active volume texture that holds GI information to be read during GI tracing.</summary>
        RenderTexture activeVolume;

        ///<summary>The volume texture that holds GI information to be read during GI tracing that was used in the previous frame.</summary>
        //RenderTexture previousActiveVolume;

        ///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture scales depending on whether Voxel AA is enabled to ensure correct voxelization.</summary>
        RenderTexture dummyVoxelTextureAAScaled;

        ///<summary>A 2D texture with the size of [voxel resolution, voxel resolution] that must be used as the active render texture when rendering the scene for voxelization. This texture is always the same size whether Voxel AA is enabled or not.</summary>
        RenderTexture dummyVoxelTextureFixed;

        bool notReadyToRender = false;

        private static Shader voxelizationShader;
        private static Shader voxelTracingShader;

        private static ComputeShader clearCompute;
        private static ComputeShader transferIntsCompute;
        private static ComputeShader mipFilterCompute;

        const int numMipLevels = 6;

        Camera voxelCamera;
        GameObject voxelCameraGO;
        GameObject leftViewPoint;
        GameObject topViewPoint;

        float VoxelScaleFactor
        {
            get
            {
                return (float)voxelResolution / 256.0f;
            }
        }

        Vector3 voxelSpaceOrigin;
        Vector3 previousVoxelSpaceOrigin;
        Vector3 voxelSpaceOriginDelta;


        Quaternion rotationFront = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        Quaternion rotationLeft = new Quaternion(0.0f, 0.7f, 0.0f, 0.7f);
        Quaternion rotationTop = new Quaternion(0.7f, 0.0f, 0.0f, 0.7f);

        //int voxelFlipFlop = 0;


        enum RenderState
        {
            Voxelize,
            Bounce
        }

        RenderState renderState = RenderState.Voxelize;
        #endregion

        #region SupportingObjectsAndProperties
        struct Pass
        {
            public static int DiffuseTrace = 0;
            public static int BilateralBlur = 1;
            public static int BlendWithScene = 2;
            public static int TemporalBlend = 3;
            public static int SpecularTrace = 4;
            public static int GetCameraDepthTexture = 5;
            public static int GetWorldNormals = 6;
            public static int VisualizeGI = 7;
            public static int WriteBlack = 8;
            public static int VisualizeVoxels = 10;
            public static int BilateralUpsample = 11;
        }

        public struct SystemSupported
        {
            public bool hdrTextures;
            public bool rIntTextures;
            public bool dx11;
            public bool volumeTextures;
            public bool postShader;
            public bool sunDepthShader;
            public bool voxelizationShader;
            public bool tracingShader;

            public bool FullFunctionality
            {
                get
                {
                    return hdrTextures && rIntTextures && dx11 && volumeTextures && postShader && sunDepthShader && voxelizationShader && tracingShader;
                }
            }
        }

        /// <summary>
        /// Contains info on system compatibility of required hardware functionality
        /// </summary>
        public SystemSupported systemSupported;

        /// <summary>
        /// Estimates the VRAM usage of all the render textures used to render GI.
        /// </summary>
        public float VramUsage
        {
            get
            {
                long v = 0;

                if (sunDepthTexture != null)
                    v += sunDepthTexture.width * sunDepthTexture.height * (16 + 16);

                if (previousGIResult != null)
                    v += previousGIResult.width * previousGIResult.height * 16 * 4;

                if (previousCameraDepth != null)
                    v += previousCameraDepth.width * previousCameraDepth.height * 32;

                if (integerVolume != null)
                    v += integerVolume.width * integerVolume.height * integerVolume.volumeDepth * 32;

                if (volumeTextures != null)
                {
                    for (int i = 0; i < volumeTextures.Length; i++)
                    {
                        if (volumeTextures[i] != null)
                            v += volumeTextures[i].width * volumeTextures[i].height * volumeTextures[i].volumeDepth * 16 * 4;
                    }
                }

                if (secondaryIrradianceVolume != null)
                    v += secondaryIrradianceVolume.width * secondaryIrradianceVolume.height * secondaryIrradianceVolume.volumeDepth * 16 * 4;

                //if (volumeTextureB != null)
                //    v += volumeTextureB.width * volumeTextureB.height * volumeTextureB.volumeDepth * 16 * 4;

                if (dummyVoxelTextureAAScaled != null)
                    v += dummyVoxelTextureAAScaled.width * dummyVoxelTextureAAScaled.height * 8;

                if (dummyVoxelTextureFixed != null)
                    v += dummyVoxelTextureFixed.width * dummyVoxelTextureFixed.height * 8;

                float vram = (v / 8388608.0f);

                return vram;
            }
        }

        int MipFilterKernel
        {
            get
            {
                return gaussianMipFilter ? 1 : 0;
            }
        }

        int DummyVoxelResolution
        {
            get
            {
                return (int)voxelResolution * (voxelAA ? 2 : 1);
            }
        }

        int GiRenderRes
        {
            get
            {
                return halfResolution ? 2 : 1;
            }
        }

        #endregion

        #region ShaderIDs

        static readonly int _SEGIVoxelAAId = Shader.PropertyToID("SEGIVoxelAA");
        static readonly int _SEGIVoxelSpaceOriginDeltaId = Shader.PropertyToID("SEGIVoxelSpaceOriginDelta");
        static readonly int _SEGIVoxelResolutionId = Shader.PropertyToID("SEGIVoxelResolution");
        static readonly int _SEGIVoxelToGIProjectionId = Shader.PropertyToID("SEGIVoxelToGIProjection");
        static readonly int _SEGISunlightVectorId = Shader.PropertyToID("SEGISunlightVector");
        static readonly int _GISunColorId = Shader.PropertyToID("GISunColor");
        static readonly int _SEGISkyColorId = Shader.PropertyToID("SEGISkyColor");
        static readonly int _GIGainId = Shader.PropertyToID("GIGain");
        static readonly int _SEGISecondaryBounceGainId = Shader.PropertyToID("SEGISecondaryBounceGain");
        static readonly int _SEGISoftSunlightId = Shader.PropertyToID("SEGISoftSunlight");
        static readonly int _SEGISphericalSkylightId = Shader.PropertyToID("SEGISphericalSkylight");
        static readonly int _SEGIInnerOcclusionLayersId = Shader.PropertyToID("SEGIInnerOcclusionLayers");
        static readonly int _SEGISunDepthId = Shader.PropertyToID("SEGISunDepth");
        static readonly int _RG0Id = Shader.PropertyToID("RG0");
        static readonly int _ResId = Shader.PropertyToID("Res");
        static readonly int _WorldToCameraId = Shader.PropertyToID("WorldToCamera");
        static readonly int _SEGIVoxelViewFrontId = Shader.PropertyToID("SEGIVoxelViewFront");
        static readonly int _SEGIVoxelViewLeftId = Shader.PropertyToID("SEGIVoxelViewLeft");
        static readonly int _SEGIVoxelViewTopId = Shader.PropertyToID("SEGIVoxelViewTop");
        static readonly int _SEGIWorldToVoxelId = Shader.PropertyToID("SEGIWorldToVoxel");
        static readonly int _SEGIVoxelProjectionId = Shader.PropertyToID("SEGIVoxelProjection");
        static readonly int _SEGIVoxelProjectionInverseId = Shader.PropertyToID("SEGIVoxelProjectionInverse");
        static readonly int _ResolutionId = Shader.PropertyToID("Resolution");
        static readonly int _SEGIVolumeTexture1Id = Shader.PropertyToID("SEGIVolumeTexture1");
        static readonly int _ResultId = Shader.PropertyToID("Result");
        static readonly int _VoxelAAId = Shader.PropertyToID("VoxelAA");
        static readonly int _VoxelOriginDeltaId = Shader.PropertyToID("VoxelOriginDelta");
        static readonly int[] _SEGIVolumeLevelIds = new int[numMipLevels];
        static readonly int _SEGIVolumeLevel0Id = Shader.PropertyToID("SEGIVolumeLevel0");
        static readonly int _destinationResId = Shader.PropertyToID("destinationRes");
        static readonly int _SourceId = Shader.PropertyToID("Source");
        static readonly int _DestinationId = Shader.PropertyToID("Destination");
        static readonly int _SEGISecondaryConesId = Shader.PropertyToID("SEGISecondaryCones");
        static readonly int _SEGISecondaryOcclusionStrengthId = Shader.PropertyToID("SEGISecondaryOcclusionStrength");
        static readonly int _CameraToWorldId = Shader.PropertyToID("CameraToWorld");
        static readonly int _ProjectionMatrixInverseId = Shader.PropertyToID("ProjectionMatrixInverse");
        static readonly int _ProjectionMatrixId = Shader.PropertyToID("ProjectionMatrix");
        static readonly int _FrameSwitchId = Shader.PropertyToID("FrameSwitch");
        static readonly int _SEGIFrameSwitchId = Shader.PropertyToID("SEGIFrameSwitch");
        static readonly int _CameraPositionId = Shader.PropertyToID("CameraPosition");
        static readonly int _DeltaTimeId = Shader.PropertyToID("DeltaTime");
        static readonly int _StochasticSamplingId = Shader.PropertyToID("StochasticSampling");
        static readonly int _TraceDirectionsId = Shader.PropertyToID("TraceDirections");
        static readonly int _TraceStepsId = Shader.PropertyToID("TraceSteps");
        static readonly int _TraceLengthId = Shader.PropertyToID("TraceLength");
        static readonly int _ConeSizeId = Shader.PropertyToID("ConeSize");
        static readonly int _OcclusionStrengthId = Shader.PropertyToID("OcclusionStrength");
        static readonly int _OcclusionPowerId = Shader.PropertyToID("OcclusionPower");
        static readonly int _ConeTraceBiasId = Shader.PropertyToID("ConeTraceBias");
        static readonly int _NearLightGainId = Shader.PropertyToID("NearLightGain");
        static readonly int _NearOcclusionStrengthId = Shader.PropertyToID("NearOcclusionStrength");
        static readonly int _DoReflectionsId = Shader.PropertyToID("DoReflections");
        static readonly int _HalfResolutionId = Shader.PropertyToID("HalfResolution");
        static readonly int _ReflectionStepsId = Shader.PropertyToID("ReflectionSteps");
        static readonly int _ReflectionOcclusionPowerId = Shader.PropertyToID("ReflectionOcclusionPower");
        static readonly int _SkyReflectionIntensityId = Shader.PropertyToID("SkyReflectionIntensity");
        static readonly int _FarOcclusionStrengthId = Shader.PropertyToID("FarOcclusionStrength");
        static readonly int _FarthestOcclusionStrengthId = Shader.PropertyToID("FarthestOcclusionStrength");
        static readonly int _NoiseTextureId = Shader.PropertyToID("NoiseTexture");
        static readonly int _BlendWeightId = Shader.PropertyToID("BlendWeight");
        static readonly int _SEGIVoxelScaleFactorId = Shader.PropertyToID("SEGIVoxelScaleFactor");
        static readonly int _KernelId = Shader.PropertyToID("Kernel");
        static readonly int _CurrentDepthId = Shader.PropertyToID("CurrentDepth");
        static readonly int _CurrentNormalId = Shader.PropertyToID("CurrentNormal");
        static readonly int _PreviousGITextureId = Shader.PropertyToID("PreviousGITexture");
        static readonly int _PreviousDepthId = Shader.PropertyToID("PreviousDepth");
        static readonly int _ReflectionsId = Shader.PropertyToID("Reflections");
        static readonly int _GITextureId = Shader.PropertyToID("GITexture");
        static readonly int _ProjectionPrevId = Shader.PropertyToID("ProjectionPrev");
        static readonly int _ProjectionPrevInverseId = Shader.PropertyToID("ProjectionPrevInverse");
        static readonly int _WorldToCameraPrevId = Shader.PropertyToID("WorldToCameraPrev");
        static readonly int _CameraToWorldPrevId = Shader.PropertyToID("CameraToWorldPrev");
        static readonly int _CameraPositionPrevId = Shader.PropertyToID("CameraPositionPrev");
        static readonly int gi1ID = Shader.PropertyToID("gi1");
        static readonly int gi2ID = Shader.PropertyToID("gi2");
        static readonly int gi3ID = Shader.PropertyToID("gi3");
        static readonly int gi4ID = Shader.PropertyToID("gi4");
        static readonly int reflectionsID = Shader.PropertyToID("reflections");
        static readonly int currentDepthID = Shader.PropertyToID("currentDepth");
        static readonly int currentNormalID = Shader.PropertyToID("currentNormal");

        #endregion

        private void Start()
        {
            InitCheck();
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled)
                return;

            Color prevColor = Gizmos.color;
            Gizmos.color = new Color(1.0f, 0.25f, 0.0f, 0.5f);

            Gizmos.DrawCube(voxelSpaceOrigin, new Vector3(voxelSpaceSize, voxelSpaceSize, voxelSpaceSize));

            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.1f);

            Gizmos.color = prevColor;
        }

        private void OnEnable()
        {
            //Shader.EnableKeyword("SS_SEGI");
            InitCheck();
            ResizeRenderTextures();
            //CheckSupport();

            //RenderShadows renderShadows = attachedCamera.GetComponent<RenderShadows>();
            //NGSS_FrustumShadows frustumShadows = attachedCamera.GetComponent<NGSS_FrustumShadows>();

            //if (renderShadows != null)
            //{
            //    renderShadows.enabled = true;
            //}
        }

        private void OnDisable()
        {
            Shader.DisableKeyword("SS_SEGI");
            Cleanup();
            //RenderShadows renderShadows = attachedCamera.GetComponent<RenderShadows>();
            //NGSS_FrustumShadows frustumShadows = attachedCamera.GetComponent<NGSS_FrustumShadows>();

            //if (renderShadows != null && !frustumShadows.enabled)
            //{
            //    renderShadows.enabled = false;
            //}

        }

        private void Update()
        {
            if (notReadyToRender)
                return;

            int currentCameraWidth = attachedCamera.pixelWidth;
            int currentCameraHeight = attachedCamera.pixelHeight;
            int currentSunShadowResolution = (int)sunShadowResolution;

            bool renderTexturesNeedResize = previousGIResult == null ||
                                             previousGIResult.width != currentCameraWidth ||
                                             previousGIResult.height != currentCameraHeight;

            if (renderTexturesNeedResize)
            {
                ResizeRenderTextures();
            }

            if (currentSunShadowResolution != prevSunShadowResolution)
            {
                ResizeSunShadowBuffer();
                prevSunShadowResolution = currentSunShadowResolution;
            }

            if (volumeTextures[0].width != (int)voxelResolution)
            {
                CreateVolumeTextures();
            }

            if (dummyVoxelTextureAAScaled.width != DummyVoxelResolution)
            {
                ResizeDummyTexture();
            }
        }

        private void OnPreRender()
        {
            //Force reinitialization to make sure that everything is working properly if one of the cameras was unexpectedly destroyed
            if (!voxelCamera || !shadowCam)
                initChecker = null;


            InitCheck();

            if (notReadyToRender)
                return;

            if (!updateGI)
            {
                return;
            }

            //Cache the previous active render texture to avoid issues with other Unity rendering going on
            RenderTexture previousActive = RenderTexture.active;

            Shader.SetGlobalInt(_SEGIVoxelAAId, voxelAA ? 1 : 0);

            //Main voxelization work
            if (renderState == RenderState.Voxelize)
            {
                activeVolume = volumeTextures[0];             //Flip-flopping volume textures to avoid simultaneous read and write errors in shaders
                //previousActiveVolume = volumeTextures[0];

                //float voxelTexel = voxelSpaceSize / (int)voxelResolution;			//Calculate the size of a voxel texel in world-space units

                //Setup the voxel volume origin position
                float interval = voxelSpaceSize / 8.0f;                                             //The interval at which the voxel volume will be "locked" in world-space
                Vector3 origin;
                if (followTransform)
                {
                    origin = followTransform.position;
                }
                else
                {
                    //GI is still flickering a bit when the scene view and the game view are opened at the same time
                    origin = transform.position + transform.forward * voxelSpaceSize / 4.0f;
                }
                //Lock the voxel volume origin based on the interval
                voxelSpaceOrigin = new Vector3(Mathf.Round(origin.x / interval) * interval, Mathf.Round(origin.y / interval) * interval, Mathf.Round(origin.z / interval) * interval);

                //Calculate how much the voxel origin has moved since last voxelization pass. Used for scrolling voxel data in shaders to avoid ghosting when the voxel volume moves in the world
                voxelSpaceOriginDelta = voxelSpaceOrigin - previousVoxelSpaceOrigin;
                Shader.SetGlobalVector(_SEGIVoxelSpaceOriginDeltaId, voxelSpaceOriginDelta / voxelSpaceSize);

                previousVoxelSpaceOrigin = voxelSpaceOrigin;

                //Set the voxel camera (proxy camera used to render the scene for voxelization) parameters
                voxelCamera.enabled = false;
                voxelCamera.aspect = 1;
                voxelCamera.orthographic = true;
                voxelCamera.orthographicSize = voxelSpaceSize * 0.5f;
                voxelCamera.nearClipPlane = 0.0f;
                voxelCamera.farClipPlane = voxelSpaceSize;
                voxelCamera.depth = -2;
                voxelCamera.renderingPath = RenderingPath.Forward;
                voxelCamera.clearFlags = CameraClearFlags.Color;
                voxelCamera.backgroundColor = Color.black;
                voxelCamera.cullingMask = giCullingMask;

                //Move the voxel camera game object and other related objects to the above calculated voxel space origin
                voxelCameraGO.transform.position = voxelSpaceOrigin - Vector3.forward * voxelSpaceSize * 0.5f;
                voxelCameraGO.transform.rotation = rotationFront;

                leftViewPoint.transform.position = voxelSpaceOrigin + Vector3.left * voxelSpaceSize * 0.5f;
                leftViewPoint.transform.rotation = rotationLeft;
                topViewPoint.transform.position = voxelSpaceOrigin + Vector3.up * voxelSpaceSize * 0.5f;
                topViewPoint.transform.rotation = rotationTop;

                //Set matrices needed for voxelization
                Shader.SetGlobalMatrix(_WorldToCameraId, attachedCamera.worldToCameraMatrix);
                Shader.SetGlobalMatrix(_SEGIVoxelViewFrontId, TransformViewMatrix(voxelCamera.transform.worldToLocalMatrix));
                Shader.SetGlobalMatrix(_SEGIVoxelViewLeftId, TransformViewMatrix(leftViewPoint.transform.worldToLocalMatrix));
                Shader.SetGlobalMatrix(_SEGIVoxelViewTopId, TransformViewMatrix(topViewPoint.transform.worldToLocalMatrix));
                Shader.SetGlobalMatrix(_SEGIWorldToVoxelId, voxelCamera.worldToCameraMatrix);
                Shader.SetGlobalMatrix(_SEGIVoxelProjectionId, voxelCamera.projectionMatrix);
                Shader.SetGlobalMatrix(_SEGIVoxelProjectionInverseId, voxelCamera.projectionMatrix.inverse);
                Shader.SetGlobalInt(_SEGIVoxelResolutionId, (int)voxelResolution);
                Matrix4x4 voxelToGIProjection = (shadowCam.projectionMatrix) * (shadowCam.worldToCameraMatrix) * (voxelCamera.cameraToWorldMatrix);
                Shader.SetGlobalMatrix(_SEGIVoxelToGIProjectionId, voxelToGIProjection);
                Shader.SetGlobalVector(_SEGISunlightVectorId, Sun ? Vector3.Normalize(Sun.transform.forward) : Vector3.up);
                //Set paramteters
                //Shader.SetGlobalColor("GISunColor", sun == null ? Color.black : new Color(Mathf.Pow(sun.color.r, 2.2f), Mathf.Pow(sun.color.g, 2.2f), Mathf.Pow(sun.color.b, 2.2f), Mathf.Pow(sun.intensity, 2.2f)));
                //Shader.SetGlobalColor(_SEGISkyColorId, new Color(Mathf.Pow(skyColor.r * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.g * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.b * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.a, 2.2f)));
                Shader.SetGlobalColor(_GISunColorId, Sun == null ? Color.black.linear : new Color(Sun.color.r, Sun.color.g, Sun.color.b, Sun.intensity).linear);
                Shader.SetGlobalColor(_SEGISkyColorId, new Color(skyColor.r * skyIntensity, skyColor.g * skyIntensity, skyColor.b * skyIntensity, skyColor.a).linear);
                Shader.SetGlobalFloat(_GIGainId, giGain);
                Shader.SetGlobalFloat(_SEGISecondaryBounceGainId, infiniteBounces ? secondaryBounceGain : 0.0f);
                Shader.SetGlobalFloat(_SEGISoftSunlightId, softSunlight);
                Shader.SetGlobalInt(_SEGISphericalSkylightId, sphericalSkylight ? 1 : 0);
                Shader.SetGlobalInt(_SEGIInnerOcclusionLayersId, innerOcclusionLayers);

                //Render the depth texture from the sun's perspective in order to inject sunlight with shadows during voxelization
                if (Sun != null)
                {
                    shadowCam.cullingMask = giCullingMask;

                    Vector3? shadowCamPosition = voxelSpaceOrigin + (Vector3.Normalize(-Sun.transform.forward) * shadowSpaceSize * 0.5f * shadowSpaceDepthRatio);
                    if (shadowCamPosition.HasValue)
                    {
                        shadowCamTransform.position = (Vector3)shadowCamPosition;
                    }

                    else
                    {
                        shadowCamTransform.position = Vector3.zero;
                    }
                    shadowCamTransform.LookAt(voxelSpaceOrigin, Vector3.up);

                    shadowCam.renderingPath = RenderingPath.Forward;
                    shadowCam.depthTextureMode = DepthTextureMode.None;

                    shadowCam.orthographicSize = shadowSpaceSize;
                    shadowCam.farClipPlane = shadowSpaceSize * 2.0f * shadowSpaceDepthRatio;

                    UnityEngine.Graphics.SetRenderTarget(sunDepthTexture);
                    shadowCam.SetTargetBuffers(sunDepthTexture.colorBuffer, sunDepthTexture.depthBuffer);

                    shadowCam.RenderWithShader(sunDepthShader, "");

                    Shader.SetGlobalTexture(_SEGISunDepthId, sunDepthTexture);
                }

                //Clear the volume texture that is immediately written to in the voxelization scene shader
                clearCompute.SetTexture(0, _RG0Id, integerVolume);
                clearCompute.SetInt(_ResId, (int)voxelResolution);
                clearCompute.Dispatch(0, (int)voxelResolution / 16, (int)voxelResolution / 16, 1);

                //Render the scene with the voxel proxy camera object with the voxelization shader to voxelize the scene to the volume integer texture
                UnityEngine.Graphics.SetRandomWriteTarget(1, integerVolume);
                voxelCamera.targetTexture = dummyVoxelTextureAAScaled;
                voxelCamera.RenderWithShader(voxelizationShader, "");
                UnityEngine.Graphics.ClearRandomWriteTargets();

                //Transfer the data from the volume integer texture to the main volume texture used for GI tracing. 
                transferIntsCompute.SetTexture(0, _ResultId, activeVolume);
                //transferIntsCompute.SetTexture(0, _PrevResultId, previousActiveVolume);
                transferIntsCompute.SetTexture(0, _RG0Id, integerVolume);
                transferIntsCompute.SetInt(_VoxelAAId, voxelAA ? 1 : 0);
                transferIntsCompute.SetInt(_ResolutionId, (int)voxelResolution);
                transferIntsCompute.SetVector(_VoxelOriginDeltaId, (voxelSpaceOriginDelta / voxelSpaceSize) * (int)voxelResolution);
                transferIntsCompute.Dispatch(0, (int)voxelResolution / 16, (int)voxelResolution / 16, 1);

                Shader.SetGlobalTexture(_SEGIVolumeLevel0Id, activeVolume);

                //Manually filter/render mip maps
                for (int i = 0; i < numMipLevels - 1; i++)
                {
                    RenderTexture source = volumeTextures[i];

                    if (i == 0)
                    {
                        source = activeVolume;
                    }

                    int destinationRes = (int)voxelResolution >> (i + 1);
                    mipFilterCompute.SetInt(_destinationResId, destinationRes);
                    mipFilterCompute.SetTexture(MipFilterKernel, _SourceId, source);
                    mipFilterCompute.SetTexture(MipFilterKernel, _DestinationId, volumeTextures[i + 1]);
                    mipFilterCompute.Dispatch(MipFilterKernel, destinationRes / 8, destinationRes / 8, 1);
                    //Shader.SetGlobalTexture("SEGIVolumeLevel" + (i + 1).ToString(), volumeTextures[i + 1]);

                    _SEGIVolumeLevelIds[i + 1] = Shader.PropertyToID("SEGIVolumeLevel" + (i + 1).ToString());
                    Shader.SetGlobalTexture(_SEGIVolumeLevelIds[i + 1], volumeTextures[i + 1]);
                }

                //Advance the voxel flip flop counter
                //voxelFlipFlop += 1;
                //voxelFlipFlop %= 2;

                if (infiniteBounces)
                {
                    renderState = RenderState.Bounce;
                }
            }
            else if (renderState == RenderState.Bounce)
            {

                //Clear the volume texture that is immediately written to in the voxelization scene shader
                clearCompute.SetTexture(0, _RG0Id, integerVolume);
                clearCompute.Dispatch(0, (int)voxelResolution / 16, (int)voxelResolution / 16, 1);

                //Set secondary tracing parameters
                Shader.SetGlobalInt(_SEGISecondaryConesId, secondaryCones);
                Shader.SetGlobalFloat(_SEGISecondaryOcclusionStrengthId, secondaryOcclusionStrength);

                //Render the scene from the voxel camera object with the voxel tracing shader to render a bounce of GI into the irradiance volume
                UnityEngine.Graphics.SetRandomWriteTarget(1, integerVolume);
                voxelCamera.targetTexture = dummyVoxelTextureFixed;
                voxelCamera.RenderWithShader(voxelTracingShader, "");
                UnityEngine.Graphics.ClearRandomWriteTargets();


                //Transfer the data from the volume integer texture to the irradiance volume texture. This result is added to the next main voxelization pass to create a feedback loop for infinite bounces
                transferIntsCompute.SetTexture(1, _ResultId, secondaryIrradianceVolume);
                transferIntsCompute.SetTexture(1, _RG0Id, integerVolume);
                //transferIntsCompute.SetInt("Resolution", (int)voxelResolution);
                transferIntsCompute.SetInt(_ResolutionId, (int)voxelResolution);
                transferIntsCompute.Dispatch(1, (int)voxelResolution / 16, (int)voxelResolution / 16, 1);

                //Shader.SetGlobalTexture("SEGIVolumeTexture1", secondaryIrradianceVolume);                
                Shader.SetGlobalTexture(_SEGIVolumeTexture1Id, secondaryIrradianceVolume);
                renderState = RenderState.Voxelize;
            }

            RenderTexture.active = previousActive;
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (notReadyToRender)
            {
                UnityEngine.Graphics.Blit(source, destination);
                return;
            }

            //Set parameters
            Shader.SetGlobalFloat(_SEGIVoxelScaleFactorId, VoxelScaleFactor);
            material.SetMatrix(_CameraToWorldId, attachedCamera.cameraToWorldMatrix);
            material.SetMatrix(_WorldToCameraId, attachedCamera.worldToCameraMatrix);
            material.SetMatrix(_ProjectionMatrixInverseId, attachedCamera.projectionMatrix.inverse);
            material.SetMatrix(_ProjectionMatrixId, attachedCamera.projectionMatrix);
            material.SetInt(_FrameSwitchId, frameCounter);
            Shader.SetGlobalInt(_SEGIFrameSwitchId, frameCounter);
            material.SetVector(_CameraPositionId, transform.position);
            material.SetFloat(_DeltaTimeId, Time.deltaTime);
            material.SetInt(_StochasticSamplingId, stochasticSampling ? 1 : 0);
            material.SetInt(_TraceDirectionsId, cones);
            material.SetInt(_TraceStepsId, coneTraceSteps);
            material.SetFloat(_TraceLengthId, coneLength);
            material.SetFloat(_ConeSizeId, coneWidth);
            material.SetFloat(_OcclusionStrengthId, occlusionStrength);
            material.SetFloat(_OcclusionPowerId, occlusionPower);
            material.SetFloat(_ConeTraceBiasId, coneTraceBias);
            material.SetFloat(_GIGainId, giGain);
            material.SetFloat(_NearLightGainId, nearLightGain);
            material.SetFloat(_NearOcclusionStrengthId, nearOcclusionStrength);
            material.SetInt(_DoReflectionsId, doReflections ? 1 : 0);
            material.SetInt(_HalfResolutionId, halfResolution ? 1 : 0);
            material.SetInt(_ReflectionStepsId, reflectionSteps);
            material.SetFloat(_ReflectionOcclusionPowerId, reflectionOcclusionPower);
            material.SetFloat(_SkyReflectionIntensityId, skyReflectionIntensity);
            material.SetFloat(_FarOcclusionStrengthId, farOcclusionStrength);
            material.SetFloat(_FarthestOcclusionStrengthId, farthestOcclusionStrength);
            material.SetFloat(_BlendWeightId, temporalBlendWeight);

            //If Visualize Voxels is enabled, just render the voxel visualization shader pass and return
            if ((debugTools & DebugTools.Voxels) != 0)
            {
                UnityEngine.Graphics.Blit(source, destination, material, Pass.VisualizeVoxels);
                return;
            }

            //Setup temporary textures
            RenderTexture gi1 = RenderTexture.GetTemporary(source.width / GiRenderRes, source.height / GiRenderRes, 0, RenderTextureFormat.ARGBHalf);
            RenderTexture gi2 = RenderTexture.GetTemporary(source.width / GiRenderRes, source.height / GiRenderRes, 0, RenderTextureFormat.ARGBHalf);
            RenderTexture reflections = null;

            //If reflections are enabled, create a temporary render buffer to hold them
            if (doReflections)
            {
                reflections = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
            }

            //Setup textures to hold the current camera depth and normal
            RenderTexture currentDepth = RenderTexture.GetTemporary(source.width / GiRenderRes, source.height / GiRenderRes, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            currentDepth.filterMode = FilterMode.Point;

            RenderTexture currentNormal = RenderTexture.GetTemporary(source.width / GiRenderRes, source.height / GiRenderRes, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            currentNormal.filterMode = FilterMode.Point;

            //Get the camera depth and normals
            UnityEngine.Graphics.Blit(source, currentDepth, material, Pass.GetCameraDepthTexture);
            material.SetTexture(_CurrentDepthId, currentDepth);
            UnityEngine.Graphics.Blit(source, currentNormal, material, Pass.GetWorldNormals);
            material.SetTexture(_CurrentNormalId, currentNormal);

            //Set the previous GI result and camera depth textures to access them in the shader
            material.SetTexture(_PreviousGITextureId, previousGIResult);
            Shader.SetGlobalTexture(_PreviousGITextureId, previousGIResult);
            material.SetTexture(_PreviousDepthId, previousCameraDepth);

            //Render diffuse GI tracing result
            UnityEngine.Graphics.Blit(source, gi2, material, Pass.DiffuseTrace);
            if (doReflections)
            {
                //Render GI reflections result
                UnityEngine.Graphics.Blit(source, reflections, material, Pass.SpecularTrace);
                material.SetTexture(_ReflectionsId, reflections);
            }

            //_KernelId
            //Perform bilateral filtering
            if (useBilateralFiltering)
            {
                material.SetVector(_KernelId, new Vector2(0.0f, 1.0f));
                UnityEngine.Graphics.Blit(gi2, gi1, material, Pass.BilateralBlur);

                material.SetVector(_KernelId, new Vector2(1.0f, 0.0f));
                UnityEngine.Graphics.Blit(gi1, gi2, material, Pass.BilateralBlur);

                material.SetVector(_KernelId, new Vector2(0.0f, 1.0f));
                UnityEngine.Graphics.Blit(gi2, gi1, material, Pass.BilateralBlur);

                material.SetVector(_KernelId, new Vector2(1.0f, 0.0f));
                UnityEngine.Graphics.Blit(gi1, gi2, material, Pass.BilateralBlur);
            }

            //If Half Resolution tracing is enabled
            if (GiRenderRes == 2)
            {
                RenderTexture.ReleaseTemporary(gi1);

                //Setup temporary textures
                RenderTexture gi3 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);
                RenderTexture gi4 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBHalf);


                //Prepare the half-resolution diffuse GI result to be bilaterally upsampled
                gi2.filterMode = FilterMode.Point;
                UnityEngine.Graphics.Blit(gi2, gi4);

                RenderTexture.ReleaseTemporary(gi2);

                gi4.filterMode = FilterMode.Point;
                gi3.filterMode = FilterMode.Point;


                //Perform bilateral upsampling on half-resolution diffuse GI result
                material.SetVector(_KernelId, new Vector2(1.0f, 0.0f));
                UnityEngine.Graphics.Blit(gi4, gi3, material, Pass.BilateralUpsample);
                material.SetVector(_KernelId, new Vector2(0.0f, 1.0f));

                //Perform temporal reprojection and blending
                if (temporalBlendWeight < 1.0f)
                {
                    UnityEngine.Graphics.Blit(gi3, gi4);
                    UnityEngine.Graphics.Blit(gi4, gi3, material, Pass.TemporalBlend);
                    UnityEngine.Graphics.Blit(gi3, previousGIResult);
                    UnityEngine.Graphics.Blit(source, previousCameraDepth, material, Pass.GetCameraDepthTexture);
                }

                //Set the result to be accessed in the shader
                material.SetTexture(_GITextureId, gi3);

                //Actually apply the GI to the scene using gbuffer data
                UnityEngine.Graphics.Blit(source, destination, material, (debugTools & DebugTools.GI) != 0 ? Pass.VisualizeGI : Pass.BlendWithScene);

                //Release temporary textures
                RenderTexture.ReleaseTemporary(gi3);
                RenderTexture.ReleaseTemporary(gi4);
            }
            else    //If Half Resolution tracing is disabled
            {
                //Perform temporal reprojection and blending
                if (temporalBlendWeight < 1.0f)
                {
                    UnityEngine.Graphics.Blit(gi2, gi1, material, Pass.TemporalBlend);
                    UnityEngine.Graphics.Blit(gi1, previousGIResult);
                    UnityEngine.Graphics.Blit(source, previousCameraDepth, material, Pass.GetCameraDepthTexture);
                }

                //Actually apply the GI to the scene using gbuffer data
                material.SetTexture(_GITextureId, temporalBlendWeight < 1.0f ? gi1 : gi2);
                UnityEngine.Graphics.Blit(source, destination, material, (debugTools & DebugTools.GI) != 0 ? Pass.VisualizeGI : Pass.BlendWithScene);

                //Release temporary textures
                RenderTexture.ReleaseTemporary(gi1);
                RenderTexture.ReleaseTemporary(gi2);
            }

            //Release temporary textures
            RenderTexture.ReleaseTemporary(currentDepth);
            RenderTexture.ReleaseTemporary(currentNormal);

            //Visualize the sun depth texture
            if ((debugTools & DebugTools.SunDepthTexture) != 0)
                UnityEngine.Graphics.Blit(sunDepthTexture, destination);


            //Release the temporary reflections result texture
            if (doReflections)
            {
                RenderTexture.ReleaseTemporary(reflections);
            }

            //Set matrices/vectors for use during temporal reprojection
            material.SetMatrix(_ProjectionPrevId, attachedCamera.projectionMatrix);
            material.SetMatrix(_ProjectionPrevInverseId, attachedCamera.projectionMatrix.inverse);
            material.SetMatrix(_WorldToCameraPrevId, attachedCamera.worldToCameraMatrix);
            material.SetMatrix(_CameraToWorldPrevId, attachedCamera.cameraToWorldMatrix);
            material.SetVector(_CameraPositionPrevId, transform.position);

            //Advance the frame counter
            frameCounter = (frameCounter + 1) % (64);
        }

        private void InitCheck()
        {
            if (initChecker == null)
            {
                Init();
            }
        }

        private void CreateVolumeTextures()
        {
            if (volumeTextures != null)
            {
                for (int i = 0; i < numMipLevels; i++)
                {
                    if (volumeTextures[i] != null)
                    {
                        volumeTextures[i].DiscardContents();
                        volumeTextures[i].Release();
                        DestroyImmediate(volumeTextures[i]);
                    }
                }
            }

            volumeTextures = new RenderTexture[numMipLevels];

            for (int i = 0; i < numMipLevels; i++)
            {
                int resolution = (int)voxelResolution >> i;
                volumeTextures[i] = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
                {
                    dimension = TextureDimension.Tex3D,
                    volumeDepth = resolution,
                    enableRandomWrite = true,
                    filterMode = FilterMode.Bilinear,
                    autoGenerateMips = false,
                    useMipMap = false
                };
                volumeTextures[i].Create();
                volumeTextures[i].hideFlags = HideFlags.HideAndDontSave;
            }

            //if (volumeTextureB)
            //{
            //    volumeTextureB.DiscardContents();
            //    volumeTextureB.Release();
            //    DestroyImmediate(volumeTextureB);
            //}
            //volumeTextureB = new RenderTexture((int)voxelResolution, (int)voxelResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
            //{
            //    dimension = TextureDimension.Tex3D,
            //    volumeDepth = (int)voxelResolution,
            //    enableRandomWrite = true,
            //    filterMode = FilterMode.Bilinear,
            //    autoGenerateMips = false,
            //    useMipMap = false
            //};
            //volumeTextureB.Create();
            //volumeTextureB.hideFlags = HideFlags.HideAndDontSave;

            if (secondaryIrradianceVolume)
            {
                secondaryIrradianceVolume.DiscardContents();
                secondaryIrradianceVolume.Release();
                DestroyImmediate(secondaryIrradianceVolume);
            }

            if (infiniteBounces)
            {
                secondaryIrradianceVolume = new RenderTexture((int)voxelResolution, (int)voxelResolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
                {
                    dimension = TextureDimension.Tex3D,
                    volumeDepth = (int)voxelResolution,
                    enableRandomWrite = true,
                    filterMode = FilterMode.Point,
                    autoGenerateMips = false,
                    useMipMap = false,
                    antiAliasing = 1
                };
                secondaryIrradianceVolume.Create();
                secondaryIrradianceVolume.hideFlags = HideFlags.HideAndDontSave;
            }

            if (integerVolume)
            {
                integerVolume.DiscardContents();
                integerVolume.Release();
                DestroyImmediate(integerVolume);
            }
            integerVolume = new RenderTexture((int)voxelResolution, (int)voxelResolution, 0, RenderTextureFormat.RInt, RenderTextureReadWrite.Linear)
            {
                dimension = TextureDimension.Tex3D,
                volumeDepth = (int)voxelResolution,
                enableRandomWrite = true,
                filterMode = FilterMode.Point
            };
            integerVolume.Create();
            integerVolume.hideFlags = HideFlags.HideAndDontSave;

            ResizeDummyTexture();
        }

        private void ResizeDummyTexture()
        {
            if (dummyVoxelTextureAAScaled)
            {
                dummyVoxelTextureAAScaled.DiscardContents();
                dummyVoxelTextureAAScaled.Release();
                DestroyImmediate(dummyVoxelTextureAAScaled);
            }
            dummyVoxelTextureAAScaled = new RenderTexture(DummyVoxelResolution, DummyVoxelResolution, 0, RenderTextureFormat.R8);
            dummyVoxelTextureAAScaled.Create();
            dummyVoxelTextureAAScaled.hideFlags = HideFlags.HideAndDontSave;

            if (dummyVoxelTextureFixed)
            {
                dummyVoxelTextureFixed.DiscardContents();
                dummyVoxelTextureFixed.Release();
                DestroyImmediate(dummyVoxelTextureFixed);
            }
            dummyVoxelTextureFixed = new RenderTexture((int)voxelResolution, (int)voxelResolution, 0, RenderTextureFormat.R8);
            dummyVoxelTextureFixed.Create();
            dummyVoxelTextureFixed.hideFlags = HideFlags.HideAndDontSave;
        }

        internal static bool LoadAssets()
        {
            AssetBundle assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("segi.unity3d"));
            sunDepthShader = assetBundle.LoadAsset<Shader>("Assets/SEGI/Resources/SEGIRenderSunDepth.shader");
            if (!sunDepthShader) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load SEGIRenderSunDepth");
            UnityEngine.Object.DontDestroyOnLoad(sunDepthShader);
            clearCompute = assetBundle.LoadAsset<ComputeShader>("Assets/SEGI/Resources/SEGIClear.compute");
            if (!clearCompute) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load SEGIClear");
            UnityEngine.Object.DontDestroyOnLoad(clearCompute);
            transferIntsCompute = assetBundle.LoadAsset<ComputeShader>("Assets/SEGI/Resources/SEGITransferInts.compute");
            if (!transferIntsCompute) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load SEGITransferInts");
            UnityEngine.Object.DontDestroyOnLoad(transferIntsCompute);
            mipFilterCompute = assetBundle.LoadAsset<ComputeShader>("Assets/SEGI/Resources/SEGIMipFilter.compute");
            if (!mipFilterCompute) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load SEGIMipFilter");
            UnityEngine.Object.DontDestroyOnLoad(mipFilterCompute);
            voxelizationShader = assetBundle.LoadAsset<Shader>("Assets/SEGI/Resources/SEGIVoxelizeScene.shader");
            if (!voxelizationShader) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load SEGIVoxelizeScene");
            UnityEngine.Object.DontDestroyOnLoad(voxelizationShader);
            voxelTracingShader = assetBundle.LoadAsset<Shader>("Assets/SEGI/Resources/SEGITraceScene.shader");
            if (!voxelTracingShader) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load SEGITraceScene");
            UnityEngine.Object.DontDestroyOnLoad(voxelTracingShader);

            if (!material)
            {
                segi = assetBundle.LoadAsset<Shader>("Assets/SEGI/Resources/SEGI.shader");
                if (!segi)
                {
                    Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load SEGI");
                    return false;
                }
                material = new Material(segi)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                UnityEngine.Object.DontDestroyOnLoad(segi);
                UnityEngine.Object.DontDestroyOnLoad(material);
            }
            assetBundle.Unload(false);

            return true;
        }

        private void Init()
        {
            //Get the camera attached to this game object
            attachedCamera = this.GetComponent<Camera>();
            attachedCamera.depthTextureMode |= DepthTextureMode.Depth;
            attachedCamera.depthTextureMode |= DepthTextureMode.MotionVectors;

            giCullingMask = attachedCamera.cullingMask;

            //Find the proxy shadow rendering camera if it exists
            GameObject scgo = GameObject.Find("SEGI_SHADOWCAM");

            //If not, create it
            if (!scgo)
            {
                shadowCamGameObject = new GameObject("SEGI_SHADOWCAM");
                shadowCam = shadowCamGameObject.AddComponent<Camera>();
                shadowCamGameObject.hideFlags = HideFlags.HideAndDontSave;


                shadowCam.enabled = false;
                shadowCam.aspect = 1;
                shadowCam.depth = attachedCamera.depth - 1;
                shadowCam.orthographic = true;
                shadowCam.orthographicSize = shadowSpaceSize;
                shadowCam.clearFlags = CameraClearFlags.SolidColor;
                shadowCam.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                shadowCam.farClipPlane = shadowSpaceSize * 2.0f * shadowSpaceDepthRatio;
                shadowCam.cullingMask = giCullingMask;
                shadowCam.useOcclusionCulling = false;

                shadowCamTransform = shadowCamGameObject.transform;
            }
            else    //Otherwise, it already exists, just get it
            {
                shadowCamGameObject = scgo;
                shadowCam = scgo.GetComponent<Camera>();
                shadowCamTransform = shadowCamGameObject.transform;
            }

            //Create the proxy camera objects responsible for rendering the scene to voxelize the scene. If they already exist, destroy them
            GameObject vcgo = GameObject.Find("SEGI_VOXEL_CAMERA");

            if (!vcgo)
            {
                voxelCameraGO = new GameObject("SEGI_VOXEL_CAMERA")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                voxelCamera = voxelCameraGO.AddComponent<Camera>();
                voxelCamera.enabled = false;
                voxelCamera.aspect = 1;
                voxelCamera.orthographic = true;
                voxelCamera.orthographicSize = voxelSpaceSize * 0.5f;
                voxelCamera.nearClipPlane = 0.0f;
                voxelCamera.farClipPlane = voxelSpaceSize;
                voxelCamera.depth = -2;
                voxelCamera.renderingPath = RenderingPath.Forward;
                voxelCamera.clearFlags = CameraClearFlags.Color;
                voxelCamera.backgroundColor = Color.black;
                voxelCamera.useOcclusionCulling = false;
            }
            else
            {
                voxelCameraGO = vcgo;
                voxelCamera = vcgo.GetComponent<Camera>();
            }

            GameObject lvp = GameObject.Find("SEGI_LEFT_VOXEL_VIEW");

            if (!lvp)
            {
                leftViewPoint = new GameObject("SEGI_LEFT_VOXEL_VIEW")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            else
            {
                leftViewPoint = lvp;
            }

            GameObject tvp = GameObject.Find("SEGI_TOP_VOXEL_VIEW");

            if (!tvp)
            {
                topViewPoint = new GameObject("SEGI_TOP_VOXEL_VIEW")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            else
            {
                topViewPoint = tvp;
            }

            //Setup sun depth texture
            if (sunDepthTexture)
            {
                sunDepthTexture.DiscardContents();
                sunDepthTexture.Release();
                DestroyImmediate(sunDepthTexture);
            }
            sunDepthTexture = new RenderTexture(sunShadowResolution, sunShadowResolution, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            sunDepthTexture.Create();
            sunDepthTexture.hideFlags = HideFlags.HideAndDontSave;

            //Create the volume textures
            CreateVolumeTextures();

            initChecker = new bool();
        }

        private void CheckSupport()
        {
            systemSupported.hdrTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
            systemSupported.rIntTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RInt);
            systemSupported.dx11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
            systemSupported.volumeTextures = SystemInfo.supports3DTextures;

            systemSupported.postShader = material.shader.isSupported;
            systemSupported.sunDepthShader = sunDepthShader.isSupported;
            systemSupported.voxelizationShader = voxelizationShader.isSupported;
            systemSupported.tracingShader = voxelTracingShader.isSupported;

            if (!systemSupported.FullFunctionality)
            {
                Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "SEGI is not supported on the current platform. Check for shader compile errors in SEGI/Resources");
                enabled = false;
            }
        }

        private void CleanupTexture(ref RenderTexture texture)
        {
            if (texture != null)
            {
                texture.DiscardContents();
                texture.Release();
                DestroyImmediate(texture);
                texture = null; // Set the texture reference to null after cleanup
            }
        }

        private void CleanupTextures()
        {
            CleanupTexture(ref sunDepthTexture);
            CleanupTexture(ref previousGIResult);
            CleanupTexture(ref previousCameraDepth);
            CleanupTexture(ref integerVolume);
            for (int i = 0; i < volumeTextures.Length; i++)
            {
                CleanupTexture(ref volumeTextures[i]);
            }
            CleanupTexture(ref secondaryIrradianceVolume);
            //CleanupTexture(ref volumeTextureB);
            CleanupTexture(ref dummyVoxelTextureAAScaled);
            CleanupTexture(ref dummyVoxelTextureFixed);
        }

        private void Cleanup()
        {
            //DestroyImmediate(material);
            DestroyImmediate(voxelCameraGO);
            DestroyImmediate(leftViewPoint);
            DestroyImmediate(topViewPoint);
            DestroyImmediate(shadowCamGameObject);
            initChecker = null;

            CleanupTextures();
        }

        private void ResizeRenderTextures()
        {
            if (previousGIResult)
            {
                previousGIResult.DiscardContents();
                previousGIResult.Release();
                DestroyImmediate(previousGIResult);
            }

            int width = attachedCamera.pixelWidth == 0 ? 2 : attachedCamera.pixelWidth;
            int height = attachedCamera.pixelHeight == 0 ? 2 : attachedCamera.pixelHeight;

            previousGIResult = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                useMipMap = true,
                autoGenerateMips = false
            };
            previousGIResult.Create();
            previousGIResult.hideFlags = HideFlags.HideAndDontSave;

            if (previousCameraDepth)
            {
                previousCameraDepth.DiscardContents();
                previousCameraDepth.Release();
                DestroyImmediate(previousCameraDepth);
            }
            previousCameraDepth = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            previousCameraDepth.Create();
            previousCameraDepth.hideFlags = HideFlags.HideAndDontSave;
        }

        private void ResizeSunShadowBuffer()
        {

            if (sunDepthTexture)
            {
                sunDepthTexture.DiscardContents();
                sunDepthTexture.Release();
                DestroyImmediate(sunDepthTexture);
            }
            sunDepthTexture = new RenderTexture(sunShadowResolution, sunShadowResolution, 16, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            sunDepthTexture.Create();
            sunDepthTexture.hideFlags = HideFlags.HideAndDontSave;
        }

        private Matrix4x4 TransformViewMatrix(Matrix4x4 mat)
        {
            //Since the third column of the view matrix needs to be reversed if using reversed z-buffer, do so here
            if (SystemInfo.usesReversedZBuffer)
            {
                mat[2, 0] = -mat[2, 0];
                mat[2, 1] = -mat[2, 1];
                mat[2, 2] = -mat[2, 2];
                mat[2, 3] = -mat[2, 3];
            }
            return mat;
        }

    }
}