using Graphics.HSSSS;
using Graphics.Settings;
using UnityEngine;

namespace Graphics
{
    /// <summary>
    /// HSSSS on the main camera: pre-integrated skin lighting in its own deferred pass, with a
    /// screen-space blur and translucency. It needs the Deferred rendering path. It and the screen
    /// space SSS are one or the other, as in Graphics' SSS tab.
    /// </summary>
    public class HSSSSManager
    {
        public static HSSSSSettings settings;

        internal static DeferredRenderer Instance;

        internal void Initialize()
        {
            if (settings == null)
                settings = new HSSSSSettings();
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            if (camera == null)
                return;
            Instance = camera.GetComponent<DeferredRenderer>();
            if (Instance == null && settings.Enabled)
                Instance = camera.gameObject.AddComponent<DeferredRenderer>();
            UpdateSettings();
        }

        public static bool Deferred
        {
            get
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                return camera != null && camera.actualRenderingPath == RenderingPath.DeferredShading;
            }
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new HSSSSSettings();
            bool want = settings.Enabled && Deferred;
            if (Instance == null && want)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                if (camera != null)
                    Instance = camera.GetOrAddComponent<DeferredRenderer>();
            }
            if (Instance == null)
                return;
            settings.Load(Instance);
            if (Instance.enabled != want)
                Instance.enabled = want;
        }
    }
}
