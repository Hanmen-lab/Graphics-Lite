using Graphics.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics
{
    public class DitheredShadowsManager
    {
        public static Settings.DitheredShadowsSettings settings;

        internal static DitheredShadows DitheredShadowsInstance;
        private static readonly List<DitheredShadows> otherDitheredShadowsInstances = new List<DitheredShadows>();

        // Initialize Components
        internal void Initialize()
        {
            DitheredShadowsInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<DitheredShadows>();
            if (settings == null)
            {
                settings = new DitheredShadowsSettings();
            }

            settings.Load(DitheredShadowsInstance);
            CopySettingsToOtherInstances();
        }

        // When user enabled the option
        internal void Start()
        {
        }

        public static void RegisterAdditionalInstance(DitheredShadows otherInstance)
        {
            if (!otherDitheredShadowsInstances.Contains(otherInstance))
            {
                otherDitheredShadowsInstances.Add(otherInstance);
                DitheredShadowsManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new DitheredShadowsSettings();
            if (DitheredShadowsInstance != null)
                settings.Load(DitheredShadowsInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (DitheredShadows otherInstance in otherDitheredShadowsInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroyDitheredShadowsInstance(DitheredShadowsInstance);
            for (int i = otherDitheredShadowsInstances.Count - 1; i >= 0; i--)
            {
                DitheredShadows otherInstance = otherDitheredShadowsInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = DitheredShadowsInstance.enabled;
                    DestroyDitheredShadowsInstance(otherInstance);

                }
            }
        }

        public static void DestroyDitheredShadowsInstance(DitheredShadows DitheredShadowsInstance)
        {
            otherDitheredShadowsInstances.Remove(DitheredShadowsInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (DitheredShadowsInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                DitheredShadowsInstance = camera.GetOrAddComponent<DitheredShadows>();
            }
        }
    }
}
