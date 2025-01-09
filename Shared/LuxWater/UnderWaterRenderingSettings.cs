using LuxWater;
using MessagePack;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class UnderWaterRenderingSettings
    {
        //public bool EnableDeepwaterLighting = false;
        //public float DefaultWaterSurfacePosition = 0.0f;
        //public float DirectionalLightingFadeRange = 64.0f;
        //public float FogLightingFadeRange = 64.0f;
        //public bool EnableAdvancedDeferredFog = false;
        //public float FogDepthShift = 1.0f;
        //public float FogEdgeBlending = 0.125f;
        public bool Enabled = false;
        public BoolValue EnableDeepwaterLighting = new BoolValue(false, false);
        public FloatValue DefaultWaterSurfacePosition = new FloatValue(0.0f, false);
        public FloatValue DirectionalLightingFadeRange = new FloatValue(64.0f, false);
        public FloatValue FogLightingFadeRange = new FloatValue(64.0f, false);
        public BoolValue EnableAdvancedDeferredFog = new BoolValue(false, false);
        public FloatValue FogDepthShift = new FloatValue(1.0f, false);
        public FloatValue FogEdgeBlending = new FloatValue(0.125f, false);

        public void Load(LuxWater_UnderWaterRendering underwater)
        {
            if (underwater == null)
                return;

            underwater.enabled = Enabled;
            underwater.EnableDeepwaterLighting = EnableDeepwaterLighting.value;
            underwater.DefaultWaterSurfacePosition = DefaultWaterSurfacePosition.value;
            underwater.DirectionalLightingFadeRange = DirectionalLightingFadeRange.value;
            underwater.FogLightingFadeRange = FogLightingFadeRange.value;
            underwater.EnableAdvancedDeferredFog = EnableAdvancedDeferredFog.value;
            underwater.FogDepthShift = FogDepthShift.value;
            underwater.FogEdgeBlending = FogEdgeBlending.value;

        }
        public void Save(LuxWater_UnderWaterRendering underwater)
        {
            if (underwater == null)
                return;

            Enabled = underwater.enabled;
            EnableDeepwaterLighting.value = underwater.EnableDeepwaterLighting;
            DefaultWaterSurfacePosition.value = underwater.DefaultWaterSurfacePosition;
            DirectionalLightingFadeRange.value = underwater.DirectionalLightingFadeRange;
            FogLightingFadeRange.value = underwater.FogLightingFadeRange;
            EnableAdvancedDeferredFog.value = underwater.EnableAdvancedDeferredFog;
            FogDepthShift.value = underwater.FogDepthShift;
            FogEdgeBlending.value = underwater.FogEdgeBlending;
        }
    }
}
