using LuxWater;
using MessagePack;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class WaterVolumeTriggerSettings
    {
        public bool Enabled = false;
        public BoolValue EnableDeepwaterLighting = new BoolValue(false, false);
        public FloatValue DefaultWaterSurfacePosition = new FloatValue(0.0f, false);
        public FloatValue DirectionalLightingFadeRange = new FloatValue(64.0f, false);
        public FloatValue FogLightingFadeRange = new FloatValue(64.0f, false);
        public BoolValue FindSunOnEnable = new BoolValue(false, false);


        public void Load(LuxWater_WaterVolumeTrigger underwater)
        {
            if (underwater == null)
                return;

            underwater.enabled = Enabled;
            //underwater.EnableDeepwaterLighting = EnableDeepwaterLighting.value;
            //underwater.DefaultWaterSurfacePosition = DefaultWaterSurfacePosition.value;
            //underwater.DirectionalLightingFadeRange = DirectionalLightingFadeRange.value;
            //underwater.FogLightingFadeRange = FogLightingFadeRange.value;
            //underwater.FindSunOnEnable = FindSunOnEnable.value;
        }
        public void Save(LuxWater_WaterVolumeTrigger underwater)
        {
            if (underwater == null)
                return;

            Enabled = underwater.enabled;
            //EnableDeepwaterLighting.value = underwater.EnableDeepwaterLighting;
            //DefaultWaterSurfacePosition.value = underwater.DefaultWaterSurfacePosition;
            //DirectionalLightingFadeRange.value = underwater.DirectionalLightingFadeRange;
            //FogLightingFadeRange.value = underwater.FogLightingFadeRange;
            //FindSunOnEnable.value = underwater.FindSunOnEnable;

        }
    }
}
