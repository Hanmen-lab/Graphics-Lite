using Graphics.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace Graphics.SEGI
{
    public class SEGIManager
    {
        public static SEGISettings settings;

        internal static SEGI SEGIInstance;
        private static readonly List<SEGI> otherSEGIInstances = new List<SEGI>();

        // Initialize Components
        internal void Initialize()
        {
            SEGIInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<SEGI>();
            if (settings == null)
            {
                settings = new SEGISettings();
            }

            settings.Load(SEGIInstance);
            CopySettingsToOtherInstances();
        }

        // When user enabled the option
        internal void Start()
        {
        }

        public static void RegisterAdditionalInstance(SEGI otherInstance)
        {
            if (!otherSEGIInstances.Contains(otherInstance))
            {
                otherSEGIInstances.Add(otherInstance);
                SEGIManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new SEGISettings();
            if (SEGIInstance != null)
            {
                settings.Load(SEGIInstance);
            }

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (SEGI otherInstance in otherSEGIInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroySEGIInstance(SEGIInstance);
            for (int i = otherSEGIInstances.Count - 1; i >= 0; i--)
            {
                SEGI otherInstance = otherSEGIInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = SEGIInstance.enabled;
                    DestroySEGIInstance(otherInstance);

                }
            }
        }

        public static void DestroySEGIInstance(SEGI SEGIInstance)
        {
            otherSEGIInstances.Remove(SEGIInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (SEGIInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                SEGIInstance = camera.GetOrAddComponent<SEGI>();
            }
        }
    }


}
