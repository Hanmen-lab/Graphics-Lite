using ADV.Commands.Object;
using Graphics.Settings;
using Graphics.Textures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Aura2API;

namespace Graphics
{
    public class AuraManager
    {
        public static AuraSettings settings;

        internal static AuraCamera AuraInstance;
        internal static AuraBaseSettings BaseSettings;
        internal static AuraQualitySettings QualitySettings;

        // Initialize Components
        internal void Initialize()
        {
            AuraInstance = Graphics.Instance.CameraSettings.MainCamera.GetComponent<AuraCamera>();
            if (settings == null)
            {
                settings = new AuraSettings();
            }

            if (AuraInstance)
            {
                settings.Load(AuraInstance);
                settings.LoadBaseSettings(AuraInstance.frustumSettings.BaseSettings);
                settings.LoadQualitySettings(AuraInstance.frustumSettings.QualitySettings);
            }
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new AuraSettings();

            if (AuraInstance != null)
            {
                settings.Load(AuraInstance);
                settings.LoadBaseSettings(AuraInstance.frustumSettings.BaseSettings);
                settings.LoadQualitySettings(AuraInstance.frustumSettings.QualitySettings);
            }
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (AuraInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                AuraInstance = camera.GetOrAddComponent<AuraCamera>();
            }
        }
    }
}
