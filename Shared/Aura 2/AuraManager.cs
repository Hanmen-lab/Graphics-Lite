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

        // Initialize Components
        internal void Initialize()
        {
            AuraInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<AuraCamera>();
            if (settings == null)
            {
                settings = new AuraSettings();
            }

            settings.Load(AuraInstance);
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new AuraSettings();
            if (AuraInstance != null)
                settings.Load(AuraInstance);
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
