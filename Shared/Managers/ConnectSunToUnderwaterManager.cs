using Graphics.Settings;
using LuxWater;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics
{
    public class ConnectSunToUnderwaterManager
    {
        public static Settings.ConnectSunToUnderwaterSettings settings;

        internal static ConnectSunToUnderwater ConnectorInstance;
        private static List<ConnectSunToUnderwater> otherConnectorInstances = new List<ConnectSunToUnderwater>();

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

        public static void SwapInstance(ConnectSunToUnderwater oldInstance, ConnectSunToUnderwater newInstance)
        {
            if (ConnectorInstance == oldInstance)
                ConnectorInstance = newInstance;
            else
            {
                otherConnectorInstances.Remove(oldInstance);
                otherConnectorInstances.Add(newInstance);
            }
            UpdateSettings();
        }

        public static void RegisterAdditionalInstance(ConnectSunToUnderwater otherInstance)
        {
            if (!otherConnectorInstances.Contains(otherInstance))
            {
                otherConnectorInstances.Add(otherInstance);
                ConnectSunToUnderwaterManager.CopySettingsToOtherInstances();
            }
        }
        public static void UpdateSettings()
        {
            EnsureInstanceRunning();

            if (settings == null)
                settings = new ConnectSunToUnderwaterSettings();
            if (ConnectorInstance != null)
                settings.Load(ConnectorInstance);

            CopySettingsToOtherInstances();
        }

        internal static void CopySettingsToOtherInstances()
        {
            foreach (ConnectSunToUnderwater otherInstance in otherConnectorInstances)
            {
                settings.Load(otherInstance);
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroyConnectorInstance(ConnectorInstance);
            for (int i = otherConnectorInstances.Count - 1; i >= 0; i--)
            {
                ConnectSunToUnderwater otherInstance = otherConnectorInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.enabled = ConnectorInstance.enabled;
                    DestroyConnectorInstance(otherInstance);

                }
            }
        }

        public static void DestroyConnectorInstance(ConnectSunToUnderwater ConnectorInstance)
        {
            otherConnectorInstances.Remove(ConnectorInstance);
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
            if (ConnectorInstance == null || camera.GetComponent<ConnectSunToUnderwater>() == null)
            {
                ConnectorInstance = camera.GetOrAddComponent<ConnectSunToUnderwater>();
            }
        }
    }
}
