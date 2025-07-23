using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Graphics
{
    [ExecuteInEditMode]
    [System.Serializable]
    public class DitheredShadows : MonoBehaviour
    {

        public static Shader deferredShading, screenSpaceShadows;
        public static Texture2D noise;
        AssetBundle assetBundle;

        private bool _point = true;
        private bool _direction = true;
        private bool _spot = true;

        public bool point
        {
            get
            {
                return _point;
            }
            set
            {
                _point = value;
                UpdateGraphics();
            }
        }
        public bool direction
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value;
                UpdateGraphics();
            }
        }
        public bool spot
        {
            get
            {
                return _spot;
            }
            set
            {
                _spot = value;
                UpdateGraphics();
            }
        }

        public float point_size = 0.03f;
        public float direction_size = 0.03f;
        public float spot_size = 0.03f;

        const BuiltinShaderType DS = BuiltinShaderType.DeferredShading;
        const BuiltinShaderType SSS = BuiltinShaderType.ScreenSpaceShadows;
        const BuiltinShaderMode CUSTOM = BuiltinShaderMode.UseCustom;
        const BuiltinShaderMode BUILT_IN = BuiltinShaderMode.UseBuiltin;

        static class ShaderIDs
        {
            internal static readonly int Noise = Shader.PropertyToID("_Noise");
            internal static readonly int DitherPoint = Shader.PropertyToID("_DitherPoint");
            internal static readonly int DitherDirection = Shader.PropertyToID("_DitherDirection");
            internal static readonly int DitherSpot = Shader.PropertyToID("_DitherSpot");
        }

        private void OnEnable()
        {
            UpdateGraphics();
        }

        private void UpdateGraphics()
        {
            //Load shaders from Assetbundle
            assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("ditheredshadows.unity3d"));
            if (assetBundle == null) 
                Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load asset bunle 'ditheredshadows.unity3d'");      
            if (deferredShading == null)
                deferredShading = assetBundle.LoadAsset<Shader>("Assets/DitheredShadows/Resources/Internal-DeferredShading-Dithering.shader");           
            if (screenSpaceShadows == null)
                screenSpaceShadows = assetBundle.LoadAsset<Shader>("Assets/DitheredShadows/Resources/Internal-ScreenSpaceShadows-Dithering.shader");
            byte[] textureByte = ResourceUtils.GetEmbeddedResource("BlueNoise256Greyscale.png");
            if (noise == null)
                noise = TextureUtils.LoadTexture(textureByte);

            GraphicsSettings.SetShaderMode(DS, ((_point || _spot) && enabled) ? CUSTOM : BUILT_IN);
            GraphicsSettings.SetCustomShader(DS, ((_point || _spot) && enabled) ? deferredShading : null);
            GraphicsSettings.SetShaderMode(SSS, (direction && enabled) ? CUSTOM : BUILT_IN);
            GraphicsSettings.SetCustomShader(SSS, (direction && enabled) ? screenSpaceShadows : null);
            UpdateShaders();
        }

        private void UpdateShaders()
        {
            Shader.SetGlobalTexture(ShaderIDs.Noise, noise);
            Shader.SetGlobalFloat(ShaderIDs.DitherPoint, (_point && enabled) ? point_size : 0);
            Shader.SetGlobalFloat(ShaderIDs.DitherDirection, (_direction && enabled) ? direction_size : 0);
            Shader.SetGlobalFloat(ShaderIDs.DitherSpot, (_spot && enabled) ? spot_size : 0);
        }

        void Update()
        {
            UpdateShaders();
        }
        private void OnDisable()
        {
            UpdateGraphics();
        }
    }
}   
