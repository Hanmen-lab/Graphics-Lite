using ADV.Commands.Object;
using Graphics.Settings;
using Graphics.Textures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Knife.DeferredDecals;


namespace Graphics
{
    public class DecalsSystemManager
    {
        public static DeferredDecalsSettings settings;

        internal static DeferredDecalsSystem DeferredDecalsSystemInstance;

        // Initialize Components
        internal void Initialize()
        {
            DeferredDecalsSystemInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<DeferredDecalsSystem>();
            if (settings == null)
            {
                settings = new DeferredDecalsSettings();
            }

            settings.Load(DeferredDecalsSystemInstance);

            DeferredDecalsSystemInstance.enabled = true;
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new DeferredDecalsSettings();
            if (DeferredDecalsSystemInstance != null)
                settings.Load(DeferredDecalsSystemInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (DeferredDecalsSystemInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                DeferredDecalsSystemInstance = camera.GetOrAddComponent<DeferredDecalsSystem>();
            }

            DeferredDecalsSystemInstance.enabled = true;
        }

    }
}
