using Graphics.Settings;
using UnityEngine;

namespace Graphics
{
    // AI-Shoujo's own assembly has types of these names outside any namespace, and those would win
    // over a using directive; aliases inside the namespace keep them this plugin's in both games.
    using VolumetricLight = global::Graphics.VolumetricLights.VolumetricLight;
    using VolumetricLightRenderer = global::Graphics.VolumetricLights.VolumetricLightRenderer;

    /// <summary>
    /// The renderer of the volumetric lights (light shafts in the air) on the main camera, as
    /// HS2PostProcessing has it. Which lights scatter, and how, is set per light in the Lights tab;
    /// this is the pass that draws them all.
    /// </summary>
    public class VolumetricLightManager
    {
        public static VolumetricLightSettings settings;
        private static VolumetricLightRenderer _renderer;
        private static int _seen;

        internal void Initialize()
        {
            if (settings == null)
                settings = new VolumetricLightSettings();
            _renderer = null;
            _seen = 0;
            UpdateSettings();
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new VolumetricLightSettings();
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            if (settings.Enabled == false || camera == null)
            {
                if (_renderer != null)
                    _renderer.enabled = false;
                return;
            }
            if (_renderer == null || _renderer.gameObject != camera.gameObject)
            {
                if (_renderer != null)
                    _renderer.enabled = false;
                _renderer = camera.GetComponent<VolumetricLightRenderer>() ?? camera.gameObject.AddComponent<VolumetricLightRenderer>();
            }
            _renderer.Resolution = (VolumetricLightRenderer.VolumetricResolution)Mathf.Clamp(settings.Resolution, 0, 2);
            _renderer.UseBilateralBlur = settings.BilateralBlur;
            _renderer.VisualizeLightTexture = settings.Visualize;
            _renderer.enabled = true;
            Rearm();
        }

        /// <summary>
        /// The renderer gathers the lights when it is switched on, so lights that scatter since then
        /// (a light switched on, a map loaded after the look) have it gather them again.
        /// </summary>
        internal static void Rearm()
        {
            if (_renderer == null || _renderer.enabled == false)
                return;
            int count = Object.FindObjectsOfType<VolumetricLight>().Length;
            if (count > _seen)
            {
                _renderer.enabled = false;
                _renderer.enabled = true;
            }
            _seen = count;
        }

        /// <summary>Whether any light scatters, for the notice on the renderer's card.</summary>
        internal static bool AnyLightScatters()
        {
            foreach (VolumetricLight v in Object.FindObjectsOfType<VolumetricLight>())
                if (v.enabled)
                    return true;
            return false;
        }

        /// <summary>A light's volumetric component, made when it is asked to scatter.</summary>
        internal static VolumetricLight For(Light light, bool create)
        {
            if (light == null)
                return null;
            VolumetricLight v = light.GetComponent<VolumetricLight>();
            if (v == null && create)
                v = light.gameObject.AddComponent<VolumetricLight>();
            return v;
        }
    }
}
