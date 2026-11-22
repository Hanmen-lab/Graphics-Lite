using UnityEngine;

namespace Graphics.HSSSS
{
    /// <summary>
    /// HSSSS' shaders and lookup textures, from the bundles embedded in the plugin, and its deferred
    /// shading pass: the skin lighting is done in a deferred pass of its own, claimed in the shared
    /// deferred slot (see DeferredShadingSlot) while an HSSSS component runs.
    /// </summary>
    internal static class HssssAssets
    {
        public static T Load<T>(string bundle, string path, bool required = true) where T : Object
        {
            return EmbeddedBundles.Load<T>(bundle, path, required);
        }

        private static bool _active;

        /// <summary>Whether HSSSS' deferred pass is installed (set while its component runs).</summary>
        public static bool PassActive
        {
            get { return _active; }
            set
            {
                if (_active == value)
                    return;
                _active = value;
                if (value == false)
                {
                    DeferredShadingSlot.Hssss = null;
                    return;
                }
                Shader shader = Load<Shader>("hssss-deferred.unity3d", "Assets/HSSSS/Shaders/Hidden/InternalDeferredShading.shader", false)
                    ?? Load<Shader>("hssss-deferredstandard.unity3d", "Assets/HSSSS/Shaders/Hidden/InternalDeferredShadingStandard.shader");
                if (shader == null || shader.isSupported == false)
                {
                    Graphics.Instance.Log.LogWarning("HSSSS deferred shading is missing or unsupported.");
                    return;
                }
                DeferredShadingSlot.Hssss = shader;
            }
        }
    }
}
