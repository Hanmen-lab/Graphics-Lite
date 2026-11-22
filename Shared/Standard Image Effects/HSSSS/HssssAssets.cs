using UnityEngine;
using UnityEngine.Rendering;

namespace Graphics.HSSSS
{
    /// <summary>
    /// HSSSS' shaders and lookup textures, from the bundles embedded in the plugin, and its deferred
    /// shading pass: the skin lighting is done in a deferred pass of its own, which is installed while
    /// an HSSSS component runs and taken out again when the last one stops, putting back whatever
    /// deferred shader was in use before.
    /// </summary>
    internal static class HssssAssets
    {
        public static T Load<T>(string bundle, string path, bool required = true) where T : Object
        {
            return EmbeddedBundles.Load<T>(bundle, path, required);
        }

        private static bool _active;
        private static BuiltinShaderMode _previousMode;
        private static Shader _previousShader;

        /// <summary>Whether HSSSS' deferred pass is installed (set while its component runs).</summary>
        public static bool PassActive
        {
            get { return _active; }
            set
            {
                if (_active == value)
                    return;
                _active = value;
                if (value)
                {
                    Shader shader = Load<Shader>("hssss-deferred.unity3d", "Assets/HSSSS/Shaders/Hidden/InternalDeferredShading.shader", false)
                        ?? Load<Shader>("hssss-deferredstandard.unity3d", "Assets/HSSSS/Shaders/Hidden/InternalDeferredShadingStandard.shader");
                    if (shader == null || shader.isSupported == false)
                    {
                        Graphics.Instance.Log.LogWarning("HSSSS deferred shading is missing or unsupported.");
                        return;
                    }
                    _previousMode = GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredShading);
                    _previousShader = GraphicsSettings.GetCustomShader(BuiltinShaderType.DeferredShading);
                    GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.UseCustom);
                    GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, shader);
                }
                else
                {
                    GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, _previousShader);
                    GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, _previousShader != null ? _previousMode : BuiltinShaderMode.UseBuiltin);
                }
            }
        }
    }
}
