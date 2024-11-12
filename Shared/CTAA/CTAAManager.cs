using Graphics.Settings;
using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Graphics.CTAA
{
    public class CTAAManager
    {
        public static CTAASettings settings { get; set; } = new CTAASettings();
        public static CTAA_PC Instance;

        internal void Initialize()
        {
            var ctaaInstances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<CTAA_PC>(true);

            if (ctaaInstances.Length > 0)
            {
                foreach (var instance in ctaaInstances)
                {
                    settings.Load(instance);
                }
            }
            else
            {
                Instance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<CTAA_PC>();
                settings.Load(Instance);
            }
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new CTAASettings();

            var ctaaInstances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<CTAA_PC>(true);

            if (ctaaInstances.Length > 0)
            {
                foreach (var instance in ctaaInstances)
                {
                    settings.Load(instance);
                }
            }
            else
            {
                Instance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<CTAA_PC>();
                settings = new CTAASettings();
                settings.Load(Instance);
            }
        }

        public void CheckInstance()
        {
            var ctaaInstances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<CTAA_PC>(true);

            if (ctaaInstances.Length > 0)
            {
                Instance = ctaaInstances[0];
            }
            else
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                Instance = camera.GetOrAddComponent<CTAA_PC>();
            }
        }
    }
}
