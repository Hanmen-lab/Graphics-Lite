using Graphics.Settings;
using LuxWater;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics
{
    public class LuxWater_WaterVolumeTriggerManager
    {
        public static Settings.WaterVolumeTriggerSettings settings;

        internal static LuxWater_WaterVolumeTrigger WaterVolumeTriggerInstance;
        private static List<LuxWater_WaterVolumeTrigger> otherWaterVolumeTriggerInstances = new List<LuxWater_WaterVolumeTrigger>();

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

        public static void SwapInstance(LuxWater_WaterVolumeTrigger oldInstance, LuxWater_WaterVolumeTrigger newInstance)
        {
            if (WaterVolumeTriggerInstance == oldInstance)
                WaterVolumeTriggerInstance = newInstance;
            else
            {
                otherWaterVolumeTriggerInstances.Remove(oldInstance);
                otherWaterVolumeTriggerInstances.Add(newInstance);
            }
            UpdateSettings();
        }

        public static void RegisterAdditionalInstance(LuxWater_WaterVolumeTrigger otherInstance)
        {
            if (!otherWaterVolumeTriggerInstances.Contains(otherInstance))
            {
                otherWaterVolumeTriggerInstances.Add(otherInstance);
                LuxWater_WaterVolumeTriggerManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            EnsureInstanceRunning();

            if (settings == null)
                settings = new WaterVolumeTriggerSettings();
            if (WaterVolumeTriggerInstance != null)
                settings.Load(WaterVolumeTriggerInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (LuxWater_WaterVolumeTrigger otherInstance in otherWaterVolumeTriggerInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroyWaterVolumeTriggerInstance(WaterVolumeTriggerInstance);
            for (int i = otherWaterVolumeTriggerInstances.Count - 1; i >= 0; i--)
            {
                LuxWater_WaterVolumeTrigger otherInstance = otherWaterVolumeTriggerInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = WaterVolumeTriggerInstance.enabled;
                    DestroyWaterVolumeTriggerInstance(otherInstance);

                }
            }
        }

        public static void DestroyWaterVolumeTriggerInstance(LuxWater_WaterVolumeTrigger WaterVolumeTriggerInstance)
        {
            otherWaterVolumeTriggerInstances.Remove(WaterVolumeTriggerInstance);
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
            if (WaterVolumeTriggerInstance == null || camera.GetComponent<LuxWater_WaterVolumeTrigger>() == null)
            {
                WaterVolumeTriggerInstance = camera.GetOrAddComponent<LuxWater_WaterVolumeTrigger>();
            }
        }
    }
}
