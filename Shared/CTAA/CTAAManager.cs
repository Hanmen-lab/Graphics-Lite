using Graphics.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graphics.CTAA
{
    public class CTAAManager
    {
        public static CTAASettings settings { get; set; } = new CTAASettings();
        public static CTAA_PC Instance;
        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new CTAASettings();
            if (settings.Enabled)
            {
                if (Instance == null)
                {
                    Instance = Graphics.Instance.CameraSettings.MainCamera.gameObject.AddComponent<CTAA_PC>();
                    //CTAAManager.CTaaSettings.SwitchMode(CTAAManager.CTaaSettings.Mode, true);
                }
            }
            else if (Instance != null)
                UnityEngine.GameObject.DestroyImmediate(Instance);
            if (Instance != null)
                settings.Load(Instance);
        }
    }
}
