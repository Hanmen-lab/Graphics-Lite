using Graphics.Settings;
using LuxWater;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics
{
    public class LuxWater_UnderWaterRenderingManager
    {
        public static Settings.UnderWaterRenderingSettings settings;

        internal static LuxWater_UnderWaterRendering UnderwaterInstance;
        private static List<LuxWater_UnderWaterRendering> otherUnderwaterInstances = new List<LuxWater_UnderWaterRendering>();

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

        public static void SwapInstance(LuxWater_UnderWaterRendering oldInstance, LuxWater_UnderWaterRendering newInstance)
        {
            if (UnderwaterInstance == oldInstance)
                UnderwaterInstance = newInstance;
            else
            {
                otherUnderwaterInstances.Remove(oldInstance);
                otherUnderwaterInstances.Add(newInstance);
            }
            UpdateSettings();
        }

        public static void RegisterAdditionalInstance(LuxWater_UnderWaterRendering otherInstance)
        {
            if (!otherUnderwaterInstances.Contains(otherInstance))
            {
                otherUnderwaterInstances.Add(otherInstance);
                LuxWater_UnderWaterRenderingManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            EnsureInstanceRunning();

            if (settings == null)
                settings = new UnderWaterRenderingSettings();
            if (UnderwaterInstance != null)
                settings.Load(UnderwaterInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (LuxWater_UnderWaterRendering otherInstance in otherUnderwaterInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroyUnderwaterInstance(UnderwaterInstance);
            for (int i = otherUnderwaterInstances.Count - 1; i >= 0; i--)
            {
                LuxWater_UnderWaterRendering otherInstance = otherUnderwaterInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = UnderwaterInstance.enabled;
                    DestroyUnderwaterInstance(otherInstance);

                }
            }
        }

        public static void DestroyUnderwaterInstance(LuxWater_UnderWaterRendering UnderwaterInstance)
        {
            otherUnderwaterInstances.Remove(UnderwaterInstance);
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
            if (UnderwaterInstance == null || camera.GetComponent<LuxWater_UnderWaterRendering>() == null)
            {
                UnderwaterInstance = camera.GetOrAddComponent<LuxWater_UnderWaterRendering>();
            }
        }
    }
}
