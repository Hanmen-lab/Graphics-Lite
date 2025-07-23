using Graphics.Settings;
using LuxWater;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics
{
    public class LuxWater_UnderwaterBlurManager
    {
        public static UnderWaterBlurSettings settings;

        internal static LuxWater_UnderWaterBlur UnderwaterBlurInstance;
        private static List<LuxWater_UnderWaterBlur> otherUnderwaterBlurInstances = new List<LuxWater_UnderWaterBlur>();

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

        public static void SwapInstance(LuxWater_UnderWaterBlur oldInstance, LuxWater_UnderWaterBlur newInstance)
        {
            if (UnderwaterBlurInstance == oldInstance)
                UnderwaterBlurInstance = newInstance;
            else
            {
                otherUnderwaterBlurInstances.Remove(oldInstance);
                otherUnderwaterBlurInstances.Add(newInstance);
            }
            UpdateSettings();
        }

        public static void RegisterAdditionalInstance(LuxWater_UnderWaterBlur otherInstance)
        {
            if (!otherUnderwaterBlurInstances.Contains(otherInstance))
            {
                otherUnderwaterBlurInstances.Add(otherInstance);
                LuxWater_UnderwaterBlurManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            EnsureInstanceRunning();

            if (settings == null)
                settings = new UnderWaterBlurSettings();
            if (UnderwaterBlurInstance != null)
                settings.Load(UnderwaterBlurInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (LuxWater_UnderWaterBlur otherInstance in otherUnderwaterBlurInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroyUnderwaterBlurInstance(UnderwaterBlurInstance);
            for (int i = otherUnderwaterBlurInstances.Count - 1; i >= 0; i--)
            {
                LuxWater_UnderWaterBlur otherInstance = otherUnderwaterBlurInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = UnderwaterBlurInstance.enabled;
                    DestroyUnderwaterBlurInstance(otherInstance);

                }
            }
        }

        public static void DestroyUnderwaterBlurInstance(LuxWater_UnderWaterBlur UnderwaterBlurInstance)
        {
            otherUnderwaterBlurInstances.Remove(UnderwaterBlurInstance);
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
            if (UnderwaterBlurInstance == null || camera.GetComponent<LuxWater_UnderWaterBlur>() == null)
            {
                UnderwaterBlurInstance = camera.GetOrAddComponent<LuxWater_UnderWaterBlur>();
            }
        }
    }
}
