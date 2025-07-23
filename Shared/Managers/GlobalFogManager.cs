using Graphics.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace Graphics.GlobalFog
{
    public class GlobalFogManager
    {
        public static Settings.GlobalFogSettings settings;

        internal static UnityStandardAssets.ImageEffects.GlobalFog FogInstance;
        private static List<UnityStandardAssets.ImageEffects.GlobalFog> otherFogInstances = new List<UnityStandardAssets.ImageEffects.GlobalFog>();

        // Initialize Components
        internal void Initialize()
        {
            EnsureInstanceRunning();
            UpdateSettings();
        }

        // When user enabled the option
        internal void Start()
        {
        }

        public static void SwapInstance(UnityStandardAssets.ImageEffects.GlobalFog oldInstance, UnityStandardAssets.ImageEffects.GlobalFog newInstance)
        {
            if (FogInstance == oldInstance)
                FogInstance = newInstance;
            else
            {
                otherFogInstances.Remove(oldInstance);
                otherFogInstances.Add(newInstance);
            }
            UpdateSettings();
        }

        public static void RegisterAdditionalInstance(UnityStandardAssets.ImageEffects.GlobalFog otherInstance)
        {
            if (!otherFogInstances.Contains(otherInstance))
            {
                otherFogInstances.Add(otherInstance);
                GlobalFogManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            EnsureInstanceRunning();

            if (settings == null)
                settings = new GlobalFogSettings();
            if (FogInstance != null)
                settings.Load(FogInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (UnityStandardAssets.ImageEffects.GlobalFog otherInstance in otherFogInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroyFogInstance(FogInstance);
            for (int i = otherFogInstances.Count - 1; i >= 0; i--)
            {
                UnityStandardAssets.ImageEffects.GlobalFog otherInstance = otherFogInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = FogInstance.enabled;
                    DestroyFogInstance(otherInstance);

                }
            }
        }

        public static void DestroyFogInstance(UnityStandardAssets.ImageEffects.GlobalFog FogInstance)
        {
            otherFogInstances.Remove(FogInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            EnsureInstanceRunning();
            UpdateSettings();
        }

        private static void EnsureInstanceRunning()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            if (FogInstance == null || camera.GetComponent<UnityStandardAssets.ImageEffects.GlobalFog>() == null)
            {
                FogInstance = camera.GetOrAddComponent<UnityStandardAssets.ImageEffects.GlobalFog>();
            }
        }
    }
}
