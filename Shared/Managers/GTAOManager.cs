using Graphics.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics.GTAO
{
    public class GTAOManager
    {
        public static Settings.GTAOSettings settings;

        internal static GroundTruthAmbientOcclusion GTAOInstance;
        private static readonly List<GroundTruthAmbientOcclusion> otherGTAOInstances = new List<GroundTruthAmbientOcclusion>();

        // Initialize Components
        internal void Initialize()
        {
            GTAOInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<GroundTruthAmbientOcclusion>();
            if (settings == null)
            {
                settings = new GTAOSettings();
            }

            settings.Load(GTAOInstance);
            CopySettingsToOtherInstances();
        }

        // When user enabled the option
        internal void Start()
        {
        }

        public static void RegisterAdditionalInstance(GroundTruthAmbientOcclusion otherInstance)
        {
            if (!otherGTAOInstances.Contains(otherInstance))
            {
                otherGTAOInstances.Add(otherInstance);
                GTAOManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new GTAOSettings();
            if (GTAOInstance != null)
                settings.Load(GTAOInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (GroundTruthAmbientOcclusion otherInstance in otherGTAOInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroyGTAOInstance(GTAOInstance);
            for (int i = otherGTAOInstances.Count - 1; i >= 0; i--)
            {
                GroundTruthAmbientOcclusion otherInstance = otherGTAOInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = GTAOInstance.enabled;
                    DestroyGTAOInstance(otherInstance);

                }
            }
        }

        public static void DestroyGTAOInstance(GroundTruthAmbientOcclusion GTAOInstance)
        {
            otherGTAOInstances.Remove(GTAOInstance);
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (GTAOInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                GTAOInstance = camera.GetOrAddComponent<GroundTruthAmbientOcclusion>();
            }
        }
    }
}
