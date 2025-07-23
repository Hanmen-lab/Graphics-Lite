using KKAPI.Utilities;
using System;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Graphics.CTAA;
using Graphics.Settings;

namespace Graphics
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Rendering/FilmGrain")]
    public class FilmGrain : MonoBehaviour
    {
        [Tooltip("Enable the use of colored grain.")]
        public bool colored = true;

        [Range(0f, 1f), Tooltip("Grain strength. Higher means more visible grain.")]
        public float intensity  = 0f;

        [Range(0.3f, 3f), Tooltip("Grain particle size.")]
        public float size = 1f;

        [Range(0f, 1f), DisplayName("Luminance Contribution"), Tooltip("Controls the noisiness response curve based on scene luminance. Lower values mean less noise in dark areas.")]
        public float lumContrib = 0.8f;

        private CTAA_PC ctaaPC;
        private float multiplier = 1f;
        AssetBundle assetBundle;
        Shader grainShader, bakerShader;
        Material grainMaterial, bakerMaterial;
        RenderTexture m_GrainLookupRT;
        const int k_SampleCount = 1024;
        int m_SampleIndex;

        void Init()
        {
            assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("grain.unity3d"));
            grainShader = assetBundle.LoadAsset<Shader>("Assets/FilmGrain/Shaders/FilmGrain.shader");
            bakerShader = assetBundle.LoadAsset<Shader>("Assets/FilmGrain/Shaders/GrainBaker.shader");

            if (grainMaterial == null)
            {
                //assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("grain.unity3d"));
                //grainShader = assetBundle.LoadAsset<Shader>("Assets/FilmGrain/Shaders/FilmGrain.shader");
                grainMaterial = new Material(grainShader);
                grainMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            if (bakerMaterial == null)
            {
                //assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("grain.unity3d"));
                //bakerShader = assetBundle.LoadAsset<Shader>("Assets/FilmGrain/Shaders/GrainBaker.shader");
                bakerMaterial = new Material(bakerShader);
                bakerMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
        }
        readonly struct ShaderIDs
        {
            internal static readonly int Grain_Params1 = Shader.PropertyToID("_Grain_Params1");
            internal static readonly int Grain_Params2 = Shader.PropertyToID("_Grain_Params2");
            internal static readonly int GrainTex = Shader.PropertyToID("_GrainTex");
            internal static readonly int Phase = Shader.PropertyToID("_Phase");
        }

        void Awake()
        {
            Init();
        }

        void OnEnable()
        {
            Init();
        }

        void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
        {
            ctaaPC = Graphics.Instance.CameraSettings.MainCamera.GetComponent<CTAA_PC>();

            if (ctaaPC != null && ctaaPC.SupersampleMode != CTAASettings.CTAA_MODE.STANDARD)
                multiplier = 2f;
            else
                multiplier = 1f;

#if POSTFX_DEBUG_STATIC_GRAIN
            // Chosen by a fair dice roll
            float time = 4f;
            float rndOffsetX = 0f;
            float rndOffsetY = 0f;
#else
            float time = Time.realtimeSinceStartup;
            float rndOffsetX = HaltonSeq.Get(m_SampleIndex & 1023, 2);
            float rndOffsetY = HaltonSeq.Get(m_SampleIndex & 1023, 3);

            if (++m_SampleIndex >= k_SampleCount)
                m_SampleIndex = 0;
#endif

            // Generate the grain lut for the current frame first
            if (m_GrainLookupRT == null || !m_GrainLookupRT.IsCreated())
            {
                RuntimeUtilities.Destroy(m_GrainLookupRT);

                m_GrainLookupRT = new RenderTexture(128, 128, 0, GetLookupFormat())
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Repeat,
                    anisoLevel = 0,
                    name = "Grain Lookup Texture"
                };

                m_GrainLookupRT.Create();
            }

            bakerMaterial.SetFloat(ShaderIDs.Phase, time % 10f);

            UnityEngine.Graphics.Blit(sourceTexture, m_GrainLookupRT, bakerMaterial, colored ? 1 : 0);

            // Send everything to the uber shader
            grainMaterial.SetTexture(ShaderIDs.GrainTex, m_GrainLookupRT);
            grainMaterial.SetVector(ShaderIDs.Grain_Params1, new Vector2(lumContrib, intensity * 20f));
            grainMaterial.SetVector(ShaderIDs.Grain_Params2, new Vector4((float)sourceTexture.width / (float)m_GrainLookupRT.width / (size * multiplier), (float)sourceTexture.height / (float)m_GrainLookupRT.height / (size * multiplier), rndOffsetX, rndOffsetY));

            UnityEngine.Graphics.Blit(sourceTexture, destTexture, grainMaterial);
        }
        RenderTextureFormat GetLookupFormat()
        {
            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
                return RenderTextureFormat.ARGBHalf;

            return RenderTextureFormat.ARGB32;
        }

        void OnDisable()
        {
            if (grainMaterial)
            {
                DestroyImmediate(grainMaterial);
            }
            if (bakerMaterial)
            {
                DestroyImmediate(bakerMaterial);
            }
            Release();
        }

        void Release()
        {
            RuntimeUtilities.Destroy(m_GrainLookupRT);
            m_GrainLookupRT = null;
            m_SampleIndex = 0;
        }

    }
}
