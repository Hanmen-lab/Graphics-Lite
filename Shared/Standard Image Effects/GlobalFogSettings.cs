
using MessagePack;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class GlobalFogSettings
    {
        public bool Enabled = false;
        public BoolValue distanceFog = new BoolValue(false, false);
        public BoolValue excludeFarPixels = new BoolValue(true, false);
        public BoolValue useRadialDistance = new BoolValue(true, false);
        public FloatValue height = new FloatValue(25f, false);
        public FloatValue heightDensity = new FloatValue(1.0f, false);
        public FloatValue startDistance = new FloatValue(0f, false);
        public Color fogColor =  new Color(0.5f, 0.5f, 0.5f);
        public FogMode fogMode = FogMode.ExponentialSquared;
        public enum FogMode
        {
            Linear = 1,
            Exponential,
            ExponentialSquared
        }

        public void Load(UnityStandardAssets.ImageEffects.GlobalFog fog)
        {
            if (fog == null)
                return;

            fog.enabled = Enabled;
            fog.distanceFog = distanceFog.value;
            fog.excludeFarPixels = excludeFarPixels.value;
            fog.useRadialDistance = useRadialDistance.value;
            fog.height = height.value;
            fog.heightDensity = heightDensity.value;
            fog.startDistance = startDistance.value;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = (UnityEngine.FogMode)fogMode;

        }
        public void Save(UnityStandardAssets.ImageEffects.GlobalFog fog)
        {
            if (fog == null)
                return;

            Enabled = fog.enabled;
            distanceFog.value = fog.distanceFog;
            excludeFarPixels.value = fog.excludeFarPixels;
            useRadialDistance.value = fog.useRadialDistance;
            height.value = fog.height;
            heightDensity.value = fog.heightDensity;
            startDistance.value = fog.startDistance;
            fogColor = RenderSettings.fogColor;
            fogMode = (FogMode)RenderSettings.fogMode;

        }
    }
}
