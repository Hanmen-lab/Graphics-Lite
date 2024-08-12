using Graphics.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics.AmplifyOcclusion
{
    public class AmplifyOccManager
    {
        public static Settings.AmplifyOccSettings settings;

        internal static AmplifyOcclusionEffect AmplifyOccInstance;
        private static readonly List<AmplifyOcclusionEffect> otherAmplifyOccInstances = new List<AmplifyOcclusionEffect>();

        // Initialize Components
        internal void Initialize()
        {
            AmplifyOccInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<AmplifyOcclusionEffect>();
            if (settings == null)
            {
                settings = new AmplifyOccSettings();
            }

            settings.Load(AmplifyOccInstance);
            CopySettingsToOtherInstances();
        }

        // When user enabled the option
        internal void Start()
        {
        }

        public static void RegisterAdditionalInstance(AmplifyOcclusionEffect otherInstance)
        {
            if (!otherAmplifyOccInstances.Contains(otherInstance))
            {
                otherAmplifyOccInstances.Add(otherInstance);
                AmplifyOccManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new AmplifyOccSettings();
            if (AmplifyOccInstance != null)
                settings.Load(AmplifyOccInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (AmplifyOcclusionEffect otherInstance in otherAmplifyOccInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroyAmplifyOccInstance(AmplifyOccInstance);
            for (int i = otherAmplifyOccInstances.Count - 1; i >= 0; i--)
            {
                AmplifyOcclusionEffect otherInstance = otherAmplifyOccInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = AmplifyOccInstance.enabled;
                    DestroyAmplifyOccInstance(otherInstance);

                }
            }
        }

        public static void DestroyAmplifyOccInstance(AmplifyOcclusionEffect AmplifyOccInstance)
        {
            otherAmplifyOccInstances.Remove(AmplifyOccInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (AmplifyOccInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                AmplifyOccInstance = camera.GetOrAddComponent<AmplifyOcclusionEffect>();
            }
        }
    }
}
