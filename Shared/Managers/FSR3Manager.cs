using FidelityFX;
using FidelityFX.FSR3;
using Graphics.Settings;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UniRx.Triggers;
using UnityEngine;

namespace Graphics.FSR3
{
    public class FSR3Manager
    {
        public static FSR3Settings Settings { get; set; } = new FSR3Settings();
        public static Fsr3UpscalerImageEffect Instance;
        public static Fsr3UpscalerImageEffectHelper HelperInstance;

        internal void Initialize()
        {
            var fsr3Instances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<Fsr3UpscalerImageEffect>(true);

            if (fsr3Instances.Length > 0)
            {
                foreach (var instance in fsr3Instances)
                {
                    Settings.Load(instance);
                }
            }
            else
            {
                Instance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<Fsr3UpscalerImageEffect>();
                Settings.Load(Instance);
            }
        }

        public static void UpdateSettings()
        {
            if (Settings == null)
                Settings = new FSR3Settings();

            var fsr3Instances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<Fsr3UpscalerImageEffect>(true);

            if (fsr3Instances.Length > 0)
            {
                foreach (var instance in fsr3Instances)
                {
                    Settings.Load(instance);
                }
            }
            else
            {
                Instance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<Fsr3UpscalerImageEffect>();
                Settings = new FSR3Settings();
                Settings.Load(Instance);
            }
        }

        public void CheckInstance()
        {
            var fsr3Instances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<Fsr3UpscalerImageEffect>(true);

            if (fsr3Instances.Length > 0)
            {
                Instance = fsr3Instances[0];
            }
            else
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                Instance = camera.GetOrAddComponent<Fsr3UpscalerImageEffect>();
            }
        }
    }

    public class FSR3HelperManager
    {
        public static Fsr3UpscalerImageEffectHelper HelperInstance;

        internal void Initialize()
        {
            HelperInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<Fsr3UpscalerImageEffectHelper>();
        }

        public void CheckInstance()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            HelperInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<Fsr3UpscalerImageEffectHelper>();
            HelperInstance = camera.GetOrAddComponent<Fsr3UpscalerImageEffectHelper>();
        }
    }
}
