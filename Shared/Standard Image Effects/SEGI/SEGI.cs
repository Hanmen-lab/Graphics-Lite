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
            SunDepthTexture = 1 << 3,
            Reflections = 1 << 4
        }
        public DebugTools debugTools = DebugTools.Off;

        public bool halfResolution = true;
        public bool stochasticSampling = true;
        public bool infiniteBounces = false;
        public bool reflectionDownsampling = false;
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

        public bool doReflections = true;
        [Range(12, 128)]
        public int reflectionSteps = 32;
        [Range(0.001f, 4.0f)]
        public float reflectionOcclusionPower = 0.2f;
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
        public bool initChecker = false;
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
        RenderTexture gi1, gi2, gi3, gi4; //These are used to store the results of GI tracing for temporal stabilization and blending. They are not used in the current implementation but can be useful for future improvements.

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

        //bool notReadyToRender = false;

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

        private CommandBuffer ComputeSEGI, ApplySEGI, DebugSEGI;

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

        int ReflectionRes
        {
            get
            {
                return reflectionDownsampling ? 2 : 1;
            }
        }

        #endregion

        #region ShaderIDs
        readonly struct ID
        {
            public static readonly int SEGIVoxelAA = Shader.PropertyToID("SEGIVoxelAA");
            public static readonly int SEGIVoxelSpaceOriginDelta = Shader.PropertyToID("SEGIVoxelSpaceOriginDelta");
            public static readonly int SEGIVoxelResolution = Shader.PropertyToID("SEGIVoxelResolution");
            public static readonly int SEGIVoxelToGIProjection = Shader.PropertyToID("SEGIVoxelToGIProjection");
            public static readonly int SEGISunlightVector = Shader.PropertyToID("SEGISunlightVector");
            public static readonly int GISunColor = Shader.PropertyToID("GISunColor");
            public static readonly int SEGISkyColor = Shader.PropertyToID("SEGISkyColor");
            public static readonly int GIGain = Shader.PropertyToID("GIGain");
            public static readonly int SEGISecondaryBounceGain = Shader.PropertyToID("SEGISecondaryBounceGain");
            public static readonly int SEGISoftSunlight = Shader.PropertyToID("SEGISoftSunlight");
            public static readonly int SEGISphericalSkylight = Shader.PropertyToID("SEGISphericalSkylight");
            public static readonly int SEGIInnerOcclusionLayers = Shader.PropertyToID("SEGIInnerOcclusionLayers");
            public static readonly int SEGISunDepth = Shader.PropertyToID("SEGISunDepth");
            public static readonly int RG0 = Shader.PropertyToID("RG0");
            public static readonly int Res = Shader.PropertyToID("Res");
            public static readonly int WorldToCamera = Shader.PropertyToID("WorldToCamera");
            public static readonly int SEGIVoxelViewFront = Shader.PropertyToID("SEGIVoxelViewFront");
            public static readonly int SEGIVoxelViewLeft = Shader.PropertyToID("SEGIVoxelViewLeft");
            public static readonly int SEGIVoxelViewTop = Shader.PropertyToID("SEGIVoxelViewTop");
            public static readonly int SEGIWorldToVoxel = Shader.PropertyToID("SEGIWorldToVoxel");
            public static readonly int SEGIVoxelProjection = Shader.PropertyToID("SEGIVoxelProjection");
            public static readonly int SEGIVoxelProjectionInverse = Shader.PropertyToID("SEGIVoxelProjectionInverse");
            public static readonly int Resolution = Shader.PropertyToID("Resolution");
            public static readonly int SEGIVolumeTexture1 = Shader.PropertyToID("SEGIVolumeTexture1");
            public static readonly int Result = Shader.PropertyToID("Result");
            public static readonly int VoxelAA = Shader.PropertyToID("VoxelAA");
            public static readonly int VoxelOriginDelta = Shader.PropertyToID("VoxelOriginDelta");
            public static readonly int[] SEGIVolumeLevels = new int[numMipLevels];
            public static readonly string[] SEGI_VOLUME_LEVEL_NAMES = new string[numMipLevels];
            public static readonly int SEGIVolumeLevel0 = Shader.PropertyToID("SEGIVolumeLevel0");
            public static readonly int destinationRes = Shader.PropertyToID("destinationRes");
            public static readonly int Source = Shader.PropertyToID("Source");
            public static readonly int Destination = Shader.PropertyToID("Destination");
            public static readonly int SEGISecondaryCones = Shader.PropertyToID("SEGISecondaryCones");
            public static readonly int SEGISecondaryOcclusionStrength = Shader.PropertyToID("SEGISecondaryOcclusionStrength");
            public static readonly int CameraToWorld = Shader.PropertyToID("CameraToWorld");
            public static readonly int ProjectionMatrixInverse = Shader.PropertyToID("ProjectionMatrixInverse");
            public static readonly int ProjectionMatrix = Shader.PropertyToID("ProjectionMatrix");
            public static readonly int FrameSwitch = Shader.PropertyToID("FrameSwitch");
            public static readonly int SEGIFrameSwitch = Shader.PropertyToID("SEGIFrameSwitch");
            public static readonly int CameraPosition = Shader.PropertyToID("CameraPosition");
            public static readonly int DeltaTime = Shader.PropertyToID("DeltaTime");
            public static readonly int StochasticSampling = Shader.PropertyToID("StochasticSampling");
            public static readonly int TraceDirections = Shader.PropertyToID("TraceDirections");
            public static readonly int TraceSteps = Shader.PropertyToID("TraceSteps");
            public static readonly int TraceLength = Shader.PropertyToID("TraceLength");
            public static readonly int ConeSize = Shader.PropertyToID("ConeSize");
            public static readonly int OcclusionStrength = Shader.PropertyToID("OcclusionStrength");
            public static readonly int OcclusionPower = Shader.PropertyToID("OcclusionPower");
            public static readonly int ConeTraceBias = Shader.PropertyToID("ConeTraceBias");
            public static readonly int NearLightGain = Shader.PropertyToID("NearLightGain");
            public static readonly int NearOcclusionStrength = Shader.PropertyToID("NearOcclusionStrength");
            public static readonly int DoReflections = Shader.PropertyToID("DoReflections");
            public static readonly int HalfResolution = Shader.PropertyToID("HalfResolution");
            public static readonly int ReflectionDownsampling = Shader.PropertyToID("ReflectionDownsampling");
            public static readonly int ReflectionSteps = Shader.PropertyToID("ReflectionSteps");
            public static readonly int ReflectionOcclusionPower = Shader.PropertyToID("ReflectionOcclusionPower");
            public static readonly int SkyReflectionIntensity = Shader.PropertyToID("SkyReflectionIntensity");
            public static readonly int FarOcclusionStrength = Shader.PropertyToID("FarOcclusionStrength");
            public static readonly int FarthestOcclusionStrength = Shader.PropertyToID("FarthestOcclusionStrength");
            //static readonly int _NoiseTextureId = Shader.PropertyToID("NoiseTexture");
            public static readonly int BlendWeight = Shader.PropertyToID("BlendWeight");
            public static readonly int SEGIVoxelScaleFactor = Shader.PropertyToID("SEGIVoxelScaleFactor");
            public static readonly int Kernel = Shader.PropertyToID("Kernel");
            public static readonly int currentDepth = Shader.PropertyToID("CurrentDepth");
            public static readonly int currentNormal = Shader.PropertyToID("CurrentNormal");
            public static readonly int PreviousGITexture = Shader.PropertyToID("PreviousGITexture");
            public static readonly int PreviousDepth = Shader.PropertyToID("PreviousDepth");
            //static readonly int _ReflectionsId = Shader.PropertyToID("Reflections");
            public static readonly int GITexture = Shader.PropertyToID("GITexture");
            public static readonly int ProjectionPrev = Shader.PropertyToID("ProjectionPrev");
            public static readonly int ProjectionPrevInverse = Shader.PropertyToID("ProjectionPrevInverse");
            public static readonly int WorldToCameraPrev = Shader.PropertyToID("WorldToCameraPrev");
            public static readonly int CameraToWorldPrev = Shader.PropertyToID("CameraToWorldPrev");
            public static readonly int CameraPositionPrev = Shader.PropertyToID("CameraPositionPrev");
            public static readonly int gi1 = Shader.PropertyToID("gi1");
            public static readonly int gi2 = Shader.PropertyToID("gi2");
            public static readonly int gi3 = Shader.PropertyToID("gi3");
            public static readonly int gi4 = Shader.PropertyToID("gi4");
            public static readonly int reflections = Shader.PropertyToID("reflections");
            public static readonly int currentSceneColor = Shader.PropertyToID("currentSceneColor");
            public static readonly int SegiReflections = Shader.PropertyToID("SegiReflections");

        }


        #endregion

        private void Awake()
        {
            // Move _SEGIVolumeLevelIds[] to here to make it as static that use in OnPreRender(), that line looks as below
            // _SEGIVolumeLevelIds[i + 1] = Shader.PropertyToID("SEGIVolumeLevel" + (i + 1).ToString());
            // I don't know why, but  _SEGIVolumeLevelIds[0] are not used because of _SEGIVolumeLevel0, I guess?
            for (int i = 0; i < numMipLevels; i++)
            {
                ID.SEGI_VOLUME_LEVEL_NAMES[i] = "SEGIVolumeLevel" + i.ToString();
                ID.SEGIVolumeLevels[i] = Shader.PropertyToID(ID.SEGI_VOLUME_LEVEL_NAMES[i]);
                /*Graphics.Instance.Log.LogInfo($"_SEGI_VOLUME_LEVEL_NAMES: {_SEGI_VOLUME_LEVEL_NAMES[i]}," +
                    $"_SEGIVolumeLevelIds: {_SEGIVolumeLevelIds[i]}");*/
            }
        }

        /*private void Start()
        {
            //InitCheck();
        }*/

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

        void SetupCommandBuffers()
        {
            if (attachedCamera && ComputeSEGI == null)
            {
                ComputeSEGI = new CommandBuffer { name = "SEGI Compute Buffer" };
            }
            else
            {
                ComputeSEGI.Clear();
            }
            if (attachedCamera && ApplySEGI == null)
            {
                ApplySEGI = new CommandBuffer { name = "SEGI Apply Buffer" };
            }
            else
            {
                ApplySEGI.Clear();
            }
            if (attachedCamera && DebugSEGI == null)
            {
                DebugSEGI = new CommandBuffer { name = "SEGI Debug Buffer" };
            }
            else
            {
                DebugSEGI.Clear();
            }

            //Get Scene Color
            ApplySEGI.GetTemporaryRT(ID.currentSceneColor, attachedCamera.pixelWidth, attachedCamera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.DefaultHDR);
            ApplySEGI.Blit(BuiltinRenderTextureType.CameraTarget, ID.currentSceneColor);


            //If Visualize Voxels is enabled, just render the voxel visualization shader pass and return
            if ((debugTools & DebugTools.Voxels) != 0)
            {
                DebugSEGI.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, material, Pass.VisualizeVoxels);
                //return;
            }
            else if ((debugTools & DebugTools.GI) != 0)
            {
                //Visualize the GI result
                DebugSEGI.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, material, Pass.VisualizeGI);
                //return;
            }
            else if ((debugTools & DebugTools.SunDepthTexture) != 0)
            {
                DebugSEGI.Blit(sunDepthTexture, BuiltinRenderTextureType.CameraTarget);
                //return;
            }

            //Setup temporary textures
            ComputeSEGI.GetTemporaryRT(ID.gi1, attachedCamera.pixelWidth / GiRenderRes, attachedCamera.pixelHeight / GiRenderRes, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);
            ComputeSEGI.GetTemporaryRT(ID.gi2, attachedCamera.pixelWidth / GiRenderRes, attachedCamera.pixelHeight / GiRenderRes, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);

            //If reflections are enabled, create a temporary render buffer to hold them
            if (doReflections)
            {
                ComputeSEGI.GetTemporaryRT(ID.reflections, attachedCamera.pixelWidth / ReflectionRes, attachedCamera.pixelHeight / ReflectionRes, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);
                DebugSEGI.GetTemporaryRT(ID.reflections, attachedCamera.pixelWidth / ReflectionRes, attachedCamera.pixelHeight / ReflectionRes, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);
            }

            //Setup textures to hold the current camera depth and normal
            ComputeSEGI.GetTemporaryRT(ID.currentDepth, attachedCamera.pixelWidth / GiRenderRes, attachedCamera.pixelHeight / GiRenderRes, 0, FilterMode.Point, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            ComputeSEGI.GetTemporaryRT(ID.currentNormal, attachedCamera.pixelWidth / GiRenderRes, attachedCamera.pixelHeight / GiRenderRes, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

            //Get the camera depth and normals
            ComputeSEGI.Blit(BuiltinRenderTextureType.CameraTarget, ID.currentDepth, material, Pass.GetCameraDepthTexture);
            ComputeSEGI.SetGlobalTexture("CurrentDepth", ID.currentDepth);
            ComputeSEGI.Blit(BuiltinRenderTextureType.CameraTarget, ID.currentNormal, material, Pass.GetWorldNormals);
            ComputeSEGI.SetGlobalTexture("CurrentNormal", ID.currentNormal);

            ////Set the previous GI result and camera depth textures to access them in the shader
            ComputeSEGI.SetGlobalTexture(ID.PreviousGITexture, previousGIResult);
            ComputeSEGI.SetGlobalTexture(ID.PreviousDepth, previousCameraDepth);

            //Render diffuse GI tracing result
            ComputeSEGI.Blit(BuiltinRenderTextureType.CameraTarget, ID.gi2, material, Pass.DiffuseTrace);
            if (doReflections)
            {
                //Render GI reflections result
                ComputeSEGI.Blit(BuiltinRenderTextureType.CameraTarget, ID.reflections, material, Pass.SpecularTrace);
                ComputeSEGI.SetGlobalTexture(ID.SegiReflections, ID.reflections);

                if ((debugTools & DebugTools.Reflections) != 0)
                {
                    DebugSEGI.Blit(ID.reflections, BuiltinRenderTextureType.CameraTarget);
                }
            }

            //Perform bilateral filtering
            if (useBilateralFiltering)
            {
                Vector2[] kernels = new Vector2[] { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f) };
                for (int i = 0; i < kernels.Length; i++)
                {
                    ComputeSEGI.SetGlobalVector(ID.Kernel, kernels[i]);
                    // Чередуем источники между gi1ID и gi2ID
                    if (i % 2 == 0)
                    {
                        ComputeSEGI.Blit(ID.gi2, ID.gi1, material, Pass.BilateralBlur);
                    }
                    else
                    {
                        ComputeSEGI.Blit(ID.gi1, ID.gi2, material, Pass.BilateralBlur);
                    }
                }
            }

            //If Half Resolution tracing is enabled
            if (GiRenderRes == 2)
            {
                ComputeSEGI.ReleaseTemporaryRT(ID.gi1);

                //Setup temporary textures
                ComputeSEGI.GetTemporaryRT(ID.gi3, attachedCamera.pixelWidth, attachedCamera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);
                ComputeSEGI.GetTemporaryRT(ID.gi4, attachedCamera.pixelWidth, attachedCamera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGBHalf);

                //Prepare the half-resolution diffuse GI result to be bilaterally upsampled
                ComputeSEGI.Blit(ID.gi2, ID.gi4);
                ComputeSEGI.ReleaseTemporaryRT(ID.gi2);

                //Perform bilateral upsampling on half-resolution diffuse GI result
                ComputeSEGI.SetGlobalVector(ID.Kernel, new Vector2(1.0f, 0.0f));
                ComputeSEGI.Blit(ID.gi4, ID.gi3, material, Pass.BilateralUpsample);
                ComputeSEGI.SetGlobalVector(ID.Kernel, new Vector2(0.0f, 1.0f));

                //Perform temporal reprojection and blending
                if (temporalBlendWeight < 1.0f)
                {
                    ComputeSEGI.Blit(ID.gi3, ID.gi4);
                    ComputeSEGI.Blit(ID.gi4, ID.gi3, material, Pass.TemporalBlend);
                    ComputeSEGI.Blit(ID.gi3, previousGIResult);
                    ComputeSEGI.Blit(BuiltinRenderTextureType.CameraTarget, previousCameraDepth, material, Pass.GetCameraDepthTexture);
                }

                //Set GI Texture
                ComputeSEGI.SetGlobalTexture(ID.GITexture, ID.gi3);

                //Perform final Blit
                ApplySEGI.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, material, Pass.BlendWithScene);

                //Release temporary textures
                ComputeSEGI.ReleaseTemporaryRT(ID.gi3);
                ComputeSEGI.ReleaseTemporaryRT(ID.gi4);

            }
            else
            {
                //Perform temporal reprojection and blending
                if (temporalBlendWeight < 1.0f)
                {
                    ComputeSEGI.Blit(ID.gi2, ID.gi1, material, Pass.TemporalBlend);
                    ComputeSEGI.Blit(ID.gi1, previousGIResult);
                    ComputeSEGI.Blit(BuiltinRenderTextureType.CameraTarget, previousCameraDepth, material, Pass.GetCameraDepthTexture);
                }

                //Actually apply the GI to the scene using gbuffer data
                ComputeSEGI.SetGlobalTexture(ID.GITexture, temporalBlendWeight < 1.0f ? ID.gi1 : ID.gi2);

                //Blend the GI result with the scene
                ApplySEGI.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, material, Pass.BlendWithScene);

                //Release temporary textures
                ComputeSEGI.ReleaseTemporaryRT(ID.gi1);
                ComputeSEGI.ReleaseTemporaryRT(ID.gi2);
            }

            ComputeSEGI.ReleaseTemporaryRT(ID.currentDepth);
            ComputeSEGI.ReleaseTemporaryRT(ID.currentNormal);

            //Release scene color
            ApplySEGI.ReleaseTemporaryRT(ID.currentSceneColor);

            //Release the temporary reflections result texture
            if (doReflections)
            {
                ComputeSEGI.ReleaseTemporaryRT(ID.reflections);
                DebugSEGI.ReleaseTemporaryRT(ID.reflections);
            }

            attachedCamera.AddCommandBuffer(CameraEvent.BeforeReflections, ComputeSEGI);
            attachedCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, ApplySEGI);
            attachedCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, DebugSEGI);
        }

        void RemoveCommandBuffers()
        {
            if (attachedCamera && ComputeSEGI != null)
            {
                //ComputeSEGI.Clear();
                attachedCamera.RemoveCommandBuffer(CameraEvent.BeforeReflections, ComputeSEGI);
                //ComputeSEGI.Dispose();
                //ComputeSEGI = null;
            }
                
            if (attachedCamera && ApplySEGI != null)
            {
                //ApplySEGI.Clear(); 
                attachedCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, ApplySEGI);
                //ApplySEGI.Dispose();
                //ApplySEGI = null;
            }

            if (attachedCamera && DebugSEGI != null)
            {
                //DebugSEGI.Clear(); 
                attachedCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, DebugSEGI);
                //DebugSEGI.Dispose();
                //DebugSEGI = null;
            }
        }

        private void OnEnable()
        {
            //Shader.EnableKeyword("SS_SEGI");
            InitCheck();
            ResizeRenderTextures();
            //SetupCommandBuffers();
            //CheckSupport();

            //RenderShadows renderShadows = attachedCamera.GetComponent<RenderShadows>();
            //if (renderShadows != null)
            //{
            //    renderShadows.enabled = true;
            //}

        }

        private void OnDisable()
        {
            RemoveCommandBuffers();
            Cleanup();
            //Shader.DisableKeyword("SS_SEGI");
            //RenderShadows renderShadows = attachedCamera.GetComponent<RenderShadows>();
            //if (renderShadows != null)
            //{
            //    renderShadows.enabled = false;
            //}

            Shader.SetGlobalInt(ID.DoReflections, 0);
        }

        /*private void Update()
        {
            //if (notReadyToRender)
            //    return;
        }*/

        private void OnPreRender()
        {
            //Force reinitialization to make sure that everything is working properly if one of the cameras was unexpectedly destroyed
            if (!voxelCamera || !shadowCam)
                initChecker = false;

            /*TODO: When toggle Infinite Boundces, secondaryIrradianceVolume must be initialized.
             * I think this If statement should be in UI eventListener part.
             */
            /*if (infiniteBounces == true && secondaryIrradianceVolume == null)
            {
                initChecker = null;
            }*/

            InitCheck();

            /*if (notReadyToRender)
                return;*/

            if (!updateGI)
            {
                RenderSEGI();
                return;
            }

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

            //Cache the previous active render texture to avoid issues with other Unity rendering going on
            RenderTexture previousActive = RenderTexture.active;

            Shader.SetGlobalInt(ID.SEGIVoxelAA, voxelAA ? 1 : 0);

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
                Shader.SetGlobalVector(ID.SEGIVoxelSpaceOriginDelta, voxelSpaceOriginDelta / voxelSpaceSize);

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
                //voxelCameraGO.transform.position = voxelSpaceOrigin - Vector3.forward * voxelSpaceSize * 0.5f;
                //voxelCameraGO.transform.rotation = rotationFront;
                voxelCameraGO.transform.SetPositionAndRotation(voxelSpaceOrigin - Vector3.forward * (voxelSpaceSize * 0.5f), rotationFront);

                //leftViewPoint.transform.position = voxelSpaceOrigin + Vector3.left * voxelSpaceSize * 0.5f;
                //leftViewPoint.transform.rotation = rotationLeft;
                leftViewPoint.transform.SetPositionAndRotation(voxelSpaceOrigin + Vector3.left * (voxelSpaceSize * 0.5f), rotationLeft);

                //topViewPoint.transform.position = voxelSpaceOrigin + Vector3.up * voxelSpaceSize * 0.5f;
                //topViewPoint.transform.rotation = rotationTop;
                topViewPoint.transform.SetPositionAndRotation(voxelSpaceOrigin + Vector3.up * (voxelSpaceSize * 0.5f), rotationTop);

                //Set matrices needed for voxelization
                Shader.SetGlobalMatrix(ID.WorldToCamera, attachedCamera.worldToCameraMatrix);
                Shader.SetGlobalMatrix(ID.SEGIVoxelViewFront, TransformViewMatrix(voxelCamera.transform.worldToLocalMatrix));
                Shader.SetGlobalMatrix(ID.SEGIVoxelViewLeft, TransformViewMatrix(leftViewPoint.transform.worldToLocalMatrix));
                Shader.SetGlobalMatrix(ID.SEGIVoxelViewTop, TransformViewMatrix(topViewPoint.transform.worldToLocalMatrix));
                Shader.SetGlobalMatrix(ID.SEGIWorldToVoxel, voxelCamera.worldToCameraMatrix);
                Shader.SetGlobalMatrix(ID.SEGIVoxelProjection, voxelCamera.projectionMatrix);
                Shader.SetGlobalMatrix(ID.SEGIVoxelProjectionInverse, voxelCamera.projectionMatrix.inverse);
                Shader.SetGlobalInt(ID.SEGIVoxelResolution, (int)voxelResolution);
                Matrix4x4 voxelToGIProjection = (shadowCam.projectionMatrix) * (shadowCam.worldToCameraMatrix) * (voxelCamera.cameraToWorldMatrix);
                Shader.SetGlobalMatrix(ID.SEGIVoxelToGIProjection, voxelToGIProjection);
                Shader.SetGlobalVector(ID.SEGISunlightVector, Sun ? Vector3.Normalize(Sun.transform.forward) : Vector3.up);
                //Set paramteters
                //Shader.SetGlobalColor("GISunColor", sun == null ? Color.black : new Color(Mathf.Pow(sun.color.r, 2.2f), Mathf.Pow(sun.color.g, 2.2f), Mathf.Pow(sun.color.b, 2.2f), Mathf.Pow(sun.intensity, 2.2f)));
                //Shader.SetGlobalColor(_SEGISkyColor, new Color(Mathf.Pow(skyColor.r * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.g * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.b * skyIntensity * 0.5f, 2.2f), Mathf.Pow(skyColor.a, 2.2f)));
                Shader.SetGlobalColor(ID.GISunColor, Sun == null ? Color.black.linear : new Color(Sun.color.r, Sun.color.g, Sun.color.b, Sun.intensity).linear);
                Shader.SetGlobalColor(ID.SEGISkyColor, new Color(skyColor.r * skyIntensity, skyColor.g * skyIntensity, skyColor.b * skyIntensity, skyColor.a).linear);
                Shader.SetGlobalFloat(ID.GIGain, giGain);
                Shader.SetGlobalFloat(ID.SEGISecondaryBounceGain, infiniteBounces ? secondaryBounceGain : 0.0f);
                Shader.SetGlobalFloat(ID.SEGISoftSunlight, softSunlight);
                Shader.SetGlobalInt(ID.SEGISphericalSkylight, sphericalSkylight ? 1 : 0);
                Shader.SetGlobalInt(ID.SEGIInnerOcclusionLayers, innerOcclusionLayers);

                //Render the depth texture from the sun's perspective in order to inject sunlight with shadows during voxelization
                if (Sun != null)
                {
                    shadowCam.cullingMask = giCullingMask;

                    Vector3? shadowCamPosition = voxelSpaceOrigin + (Vector3.Normalize(-Sun.transform.forward) * (shadowSpaceSize * 0.5f * shadowSpaceDepthRatio));
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

                    Shader.SetGlobalTexture(ID.SEGISunDepth, sunDepthTexture);
                }

                //Clear the volume texture that is immediately written to in the voxelization scene shader
                clearCompute.SetTexture(0, ID.RG0, integerVolume);
                clearCompute.SetInt(ID.Res, (int)voxelResolution);
                clearCompute.Dispatch(0, (int)voxelResolution / 16, (int)voxelResolution / 16, 1);

                //Render the scene with the voxel proxy camera object with the voxelization shader to voxelize the scene to the volume integer texture
                UnityEngine.Graphics.SetRandomWriteTarget(1, integerVolume);
                voxelCamera.targetTexture = dummyVoxelTextureAAScaled;
                voxelCamera.RenderWithShader(voxelizationShader, "");
                UnityEngine.Graphics.ClearRandomWriteTargets();

                //Transfer the data from the volume integer texture to the main volume texture used for GI tracing. 
                transferIntsCompute.SetTexture(0, ID.Result, activeVolume);
                //transferIntsCompute.SetTexture(0, _PrevResult, previousActiveVolume);
                transferIntsCompute.SetTexture(0, ID.RG0, integerVolume);
                transferIntsCompute.SetInt(ID.VoxelAA, voxelAA ? 1 : 0);
                transferIntsCompute.SetInt(ID.Resolution, (int)voxelResolution);
                transferIntsCompute.SetVector(ID.VoxelOriginDelta, (voxelSpaceOriginDelta / voxelSpaceSize) * (int)voxelResolution);
                transferIntsCompute.Dispatch(0, (int)voxelResolution / 16, (int)voxelResolution / 16, 1);

                Shader.SetGlobalTexture(ID.SEGIVolumeLevel0, activeVolume);

                //Manually filter/render mip maps
                for (int i = 0; i < numMipLevels - 1; i++)
                {
                    RenderTexture source = volumeTextures[i];

                    if (i == 0)
                    {
                        source = activeVolume;
                    }

                    int destinationRes = (int)voxelResolution >> (i + 1);

                    /*TODO: This IF statement below is applied to handle the incorrect operation of dividing 4 by 8
                     * when the Voxel Resolution is a Low value(128), when an integer greater than or equal to 0 is desired.
                     * Since this is a temporary measure, we need to figure out the Mipmap intention of this loop later
                     * and handle the exception properly
                     */
                    if (destinationRes == 4)
                    {
                        destinationRes = 8;
                    }
                    mipFilterCompute.SetInt(ID.destinationRes, destinationRes);
                    mipFilterCompute.SetTexture(MipFilterKernel, ID.Source, source);
                    mipFilterCompute.SetTexture(MipFilterKernel, ID.Destination, volumeTextures[i + 1]);
                    mipFilterCompute.Dispatch(MipFilterKernel, destinationRes / 8, destinationRes / 8, 1);
                    //Shader.SetGlobalTexture("SEGIVolumeLevel" + (i + 1).ToString(), volumeTextures[i + 1]);
                    //_SEGIVolumeLevelIds[i + 1] = Shader.PropertyToID("SEGIVolumeLevel" + (i + 1).ToString());
                    Shader.SetGlobalTexture(ID.SEGIVolumeLevels[i + 1], volumeTextures[i + 1]);
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
                clearCompute.SetTexture(0, ID.RG0, integerVolume);
                clearCompute.Dispatch(0, (int)voxelResolution / 16, (int)voxelResolution / 16, 1);

                //Set secondary tracing parameters
                Shader.SetGlobalInt(ID.SEGISecondaryCones, secondaryCones);
                Shader.SetGlobalFloat(ID.SEGISecondaryOcclusionStrength, secondaryOcclusionStrength);

                //Render the scene from the voxel camera object with the voxel tracing shader to render a bounce of GI into the irradiance volume
                UnityEngine.Graphics.SetRandomWriteTarget(1, integerVolume);
                voxelCamera.targetTexture = dummyVoxelTextureFixed;
                voxelCamera.RenderWithShader(voxelTracingShader, "");
                UnityEngine.Graphics.ClearRandomWriteTargets();


                //Transfer the data from the volume integer texture to the irradiance volume texture. This result is added to the next main voxelization pass to create a feedback loop for infinite bounces
                transferIntsCompute.SetTexture(1, ID.Result, secondaryIrradianceVolume);
                transferIntsCompute.SetTexture(1, ID.RG0, integerVolume);
                //transferIntsCompute.SetInt("Resolution", (int)voxelResolution);
                transferIntsCompute.SetInt(ID.Resolution, (int)voxelResolution);
                transferIntsCompute.Dispatch(1, (int)voxelResolution / 16, (int)voxelResolution / 16, 1);

                //Shader.SetGlobalTexture("SEGIVolumeTexture1", secondaryIrradianceVolume);                
                Shader.SetGlobalTexture(ID.SEGIVolumeTexture1, secondaryIrradianceVolume);
                renderState = RenderState.Voxelize;
            }

            RenderTexture.active = previousActive;

            RenderSEGI();
        }


        private void RenderSEGI()
        {
            //ComputeSEGI.Clear();
            //ApplySEGI.Clear();
            //DebugSEGI.Clear();


            //Set parameters
            Shader.SetGlobalFloat(ID.SEGIVoxelScaleFactor, VoxelScaleFactor);
            material.SetMatrix(ID.CameraToWorld, attachedCamera.cameraToWorldMatrix);
            material.SetMatrix(ID.WorldToCamera, attachedCamera.worldToCameraMatrix);
            material.SetMatrix(ID.ProjectionMatrixInverse, attachedCamera.projectionMatrix.inverse);
            material.SetMatrix(ID.ProjectionMatrix, attachedCamera.projectionMatrix);
            material.SetInt(ID.FrameSwitch, frameCounter);
            Shader.SetGlobalInt(ID.SEGIFrameSwitch, frameCounter);
            material.SetVector(ID.CameraPosition, transform.position);
            material.SetFloat(ID.DeltaTime, Time.deltaTime);
            material.SetInt(ID.StochasticSampling, stochasticSampling ? 1 : 0);
            material.SetInt(ID.TraceDirections, cones);
            material.SetInt(ID.TraceSteps, coneTraceSteps);
            material.SetFloat(ID.TraceLength, coneLength);
            material.SetFloat(ID.ConeSize, coneWidth);
            material.SetFloat(ID.OcclusionStrength, occlusionStrength);
            material.SetFloat(ID.OcclusionPower, occlusionPower);
            material.SetFloat(ID.ConeTraceBias, coneTraceBias);
            material.SetFloat(ID.GIGain, giGain);
            material.SetFloat(ID.NearLightGain, nearLightGain);
            material.SetFloat(ID.NearOcclusionStrength, nearOcclusionStrength);
            Shader.SetGlobalInt(ID.DoReflections, doReflections ? 1 : 0);
            //material.SetInt(ID.DoReflections, doReflections ? 1 : 0);
            material.SetInt(ID.HalfResolution, halfResolution ? 1 : 0);
            material.SetInt(ID.ReflectionSteps, reflectionSteps);
            material.SetFloat(ID.ReflectionOcclusionPower, reflectionOcclusionPower);
            material.SetFloat(ID.SkyReflectionIntensity, skyReflectionIntensity);
            material.SetFloat(ID.FarOcclusionStrength, farOcclusionStrength);
            material.SetFloat(ID.FarthestOcclusionStrength, farthestOcclusionStrength);
            material.SetFloat(ID.BlendWeight, temporalBlendWeight);

            //Set matrices/vectors for use during temporal reprojection
            material.SetMatrix(ID.ProjectionPrev, attachedCamera.projectionMatrix);
            material.SetMatrix(ID.ProjectionPrevInverse, attachedCamera.projectionMatrix.inverse);
            material.SetMatrix(ID.WorldToCameraPrev, attachedCamera.worldToCameraMatrix);
            material.SetMatrix(ID.CameraToWorldPrev, attachedCamera.cameraToWorldMatrix);
            material.SetVector(ID.CameraPositionPrev, transform.position);

            //Set the frame counter for the next frame	
            frameCounter = (frameCounter + 1) % (64);
        }

        private void InitCheck()
        {
            if (initChecker == false)
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

            //Refresh CommandBuffers
            RefreshCommandBuffers();

            //Set the render state value to Voxelize as the default value.
            renderState = RenderState.Voxelize;

            initChecker = true;
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
            
            CleanupTextures();

            if (ComputeSEGI != null)
            {
                ComputeSEGI.Dispose();
                ComputeSEGI = null;
            }
            if (ApplySEGI != null)
            {
                ApplySEGI.Dispose();
                ApplySEGI = null;
            }
            if (DebugSEGI != null)
            {
                DebugSEGI.Dispose();
                DebugSEGI = null;
            }

            initChecker = false;
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

        public void RefreshCommandBuffers()
        {
            //ComputeSEGI.Clear();
            //ApplySEGI.Clear();
            //DebugSEGI.Clear();

            RemoveCommandBuffers();
            SetupCommandBuffers();
        }

    }
}