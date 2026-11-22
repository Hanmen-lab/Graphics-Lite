using KKAPI.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    /// <summary>
    /// Assets from the asset bundles embedded in the plugin, each bundle loaded once and kept, each
    /// asset loaded once and kept for the session.
    /// </summary>
    internal static class EmbeddedBundles
    {
        private static readonly Dictionary<string, AssetBundle> _bundles = new Dictionary<string, AssetBundle>();
        private static readonly Dictionary<string, Object> _assets = new Dictionary<string, Object>();

        public static T Load<T>(string bundle, string path, bool required = true) where T : Object
        {
            string key = bundle + "|" + path;
            Object cached;
            if (_assets.TryGetValue(key, out cached) && cached != null)
                return cached as T;

            AssetBundle b;
            if (_bundles.TryGetValue(bundle, out b) == false)
            {
                b = null;
                try
                {
                    byte[] bytes = ResourceUtils.GetEmbeddedResource(bundle);
                    if (bytes != null)
                        b = AssetBundle.LoadFromMemory(bytes);
                }
                catch (System.Exception e)
                {
                    Graphics.Instance.Log.LogError("Could not load " + bundle + ": " + e.Message);
                }
                _bundles[bundle] = b;
            }

            T asset = b != null ? b.LoadAsset<T>(path) : null;
            if (asset != null)
                Object.DontDestroyOnLoad(asset);
            else if (required)
                Graphics.Instance.Log.LogError("Missing " + typeof(T).Name + " '" + path + "' in " + bundle + ".");
            _assets[key] = asset;
            return asset;
        }
    }
}
