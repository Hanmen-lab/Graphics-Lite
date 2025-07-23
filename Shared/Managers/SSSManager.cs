using Graphics.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    public class SSSManager
    {
        public static SSSSettings settings;

        internal static SSS SSSInstance;
        private static readonly List<SSS> otherSSSInstances = new List<SSS>();

        // Initialize Components
        internal void Initialize()
        {
            SSSInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<SSS>();
            if (settings == null)
            {
                settings = new SSSSettings();
            }

            settings.Load(SSSInstance);
            CopySettingsToOtherInstances();
        }

        // When user enabled the option
        internal void Start()
        {
        }

        public static void RegisterAdditionalInstance(SSS otherInstance)
        {
            if (!otherSSSInstances.Contains(otherInstance))
            {
                otherSSSInstances.Add(otherInstance);
                SSSManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new SSSSettings();
            if (SSSInstance != null)
                settings.Load(SSSInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (SSS otherInstance in otherSSSInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroySSSInstance(SSSInstance);
            for (int i = otherSSSInstances.Count - 1; i >= 0; i--)
            {
                SSS otherInstance = otherSSSInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = SSSInstance.enabled;
                    DestroySSSInstance(otherInstance);

                }
            }
        }

        public static void DestroySSSInstance(SSS SSSInstance)
        {
            otherSSSInstances.Remove(SSSInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (SSSInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                SSSInstance = camera.GetOrAddComponent<SSS>();
            }
        }
    }
}
