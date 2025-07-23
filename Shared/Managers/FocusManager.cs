using Graphics.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics
{
    public class FocusManager
    {
        public static FocusSettings settings;

        internal static FocusPuller FocusInstance;

        // Initialize Components
        internal void Initialize()
        {
            FocusInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<FocusPuller>();
            if (settings == null)
            {
                settings = new FocusSettings();
            }

            settings.Load(FocusInstance);
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new FocusSettings();
            if (FocusInstance != null)
                settings.Load(FocusInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (FocusInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                FocusInstance = camera.GetOrAddComponent<FocusPuller>();
            }
        }
    }
}
