using System.Diagnostics.CodeAnalysis;
using MessagePack;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class SSSSettings
    {
        public enum ToggleTexture
        {
            LightingTex = 1,
            LightingTexBlurred = 2,
            ProfileTex = 3,
            None = 0
        }

        public bool Enabled;
        //public int presetVersion = -1;
        public float BlurSize = 2.5f;
        public Color sssColor = Color.red;
        public bool Debug = false;
        public bool DebugDistance = false;
        public float DepthTest;
        public bool Dither;
        public float DitherIntensity;
        public float DitherScale;
        public float DownscaleFactor = 1; // Default Resolution by default.
        public bool EdgeDitherNoise;
        public bool FixPixelLeaks;
        public float FixPixelLeaksNormal;
        public int LayerBitMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Chara")) | (1 << LayerMask.NameToLayer("Map"));
        public int MaxDistance = 10000;
        public float NormalTest;
        public int ProcessIterations = 1;
        public float ProfileColorTest;
        public bool ProfilePerObject = true;
        public float ProfileRadiusTest;
        public bool ProfileTest;
        public int ShaderIterations = 1;
        public ToggleTexture ViewBuffer;
        public bool CTAAEnable;

        //public LayerMask SSS_Layer = (1 << LayerMask.NameToLayer("Chara")) | (1 << LayerMask.NameToLayer("Map"));

        public void Load(SSS instance)
        {
            if (instance == null) return;

            instance.enabled = Enabled;
            instance.ProfilePerObject = ProfilePerObject;
            instance.sssColor = sssColor;
            instance.ScatteringRadius = BlurSize;
            instance.ScatteringIterations = ProcessIterations;
            instance.ShaderIterations = ShaderIterations;
            instance.Downsampling = DownscaleFactor;
            instance.maxDistance = MaxDistance;
            if (LayerBitMask == (1 << LayerMask.NameToLayer("Default")))
                LayerBitMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Chara")) | (1 << LayerMask.NameToLayer("Map"));
            instance.SSS_Layer = LayerBitMask;
            instance.Dither = Dither;
            instance.DitherIntensity = DitherIntensity;
            instance.DitherScale = DitherScale;
            instance.toggleTexture = (SSS.ToggleTexture)ViewBuffer;
            instance.DepthTest = DepthTest;
            instance.NormalTest = NormalTest;
            instance.DitherEdgeTest = EdgeDitherNoise;
            instance.FixPixelLeaks = FixPixelLeaks;
            instance.EdgeOffset = FixPixelLeaksNormal;
            instance.UseProfileTest = ProfileTest;
            instance.ProfileColorTest = ProfileColorTest;
            instance.ProfileRadiusTest = ProfileRadiusTest;

            //if (presetVersion < 2)
            //{
            //    instance.toggleTexture = SSS.ToggleTexture.None;
            //    instance.SSS_Layer = instance.SSS_Layer | (1 << LayerMask.NameToLayer("Chara")) | (1 << LayerMask.NameToLayer("Map"));
            //    instance.ProfilePerObject = true;
            //}

            //Graphics.Instance.SSSManager.CopySettingsToOtherInstances();
        }

        public void Save(SSS instance)
        {
            if (instance is null) return;

            Enabled = instance.enabled;
            ProfilePerObject = instance.ProfilePerObject;
            sssColor = instance.sssColor;
            BlurSize = instance.ScatteringRadius;
            ProcessIterations = instance.ScatteringIterations;
            ShaderIterations = instance.ShaderIterations;
            DownscaleFactor = instance.Downsampling;
            MaxDistance = instance.maxDistance;
            //LayerBitMask = instance.SSS_Layer;
            Dither = instance.Dither;
            DitherIntensity = instance.DitherIntensity;
            DitherScale = instance.DitherScale;
            ViewBuffer = (ToggleTexture)instance.toggleTexture;
            DepthTest = instance.DepthTest;
            NormalTest = instance.NormalTest;
            EdgeDitherNoise = instance.DitherEdgeTest;
            FixPixelLeaks = instance.FixPixelLeaks;
            FixPixelLeaksNormal = instance.EdgeOffset;
            ProfileTest = instance.UseProfileTest;
            ProfileColorTest = instance.ProfileColorTest;
            ProfileRadiusTest = instance.ProfileRadiusTest;

            //presetVersion = 2;
        }

        //private void RescueWithHelicopter()
        //{
        //    BlurSize = Mathf.Clamp(BlurSize, 0, 100);
        //    ProcessIterations = Mathf.Clamp(ProcessIterations, 1, 100);
        //    ShaderIterations = Mathf.Clamp(ShaderIterations, 1, 100);
        //    DownscaleFactor = Mathf.Clamp(DownscaleFactor, 0.5f, 100);
        //}
    }
}