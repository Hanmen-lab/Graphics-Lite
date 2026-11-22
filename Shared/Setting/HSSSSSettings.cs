using Graphics.HSSSS;
using MessagePack;
using UnityEngine;

namespace Graphics.Settings
{
    /// <summary>HSSSS' settings, saved in presets and scenes with the rest of the look.</summary>
    [MessagePackObject(true)]
    public class HSSSSSettings
    {
        public bool Enabled = false;

        // Scattering
        public bool ScreenSpaceSSS = true;
        public float BlurRadius = 50f;
        public float BlurDepthRange = 0f;
        public int BlurIterations = 3;
        public float DepthTest = 0.85f;
        public float NormalTest = 0.85f;
        public bool BlurAlbedo = false;

        // Translucency
        public bool BakedThickness = true;
        public float TransWeight = 1f;
        public float TransShadowWeight = 1f;
        public float TransDistortion = 0.5f;
        public float TransFalloff = 2f;
        public float ThicknessBias = 0.5f;
        public Vector3 TransAbsorption = new Vector3(-8f, -48f, -64f);

        // Skin LUT
        public int LutProfile = 0;
        public float LutIntensity = 1f;
        public float LutBias = 0.5f;
        public float LutScale = 0.5f;
        public float ShadowLutBias = 0f;
        public float ShadowLutScale = 1f;
        public float BlurWeight = 1f;
        public Vector3 ColorBleed = new Vector3(0.4f, 0.15f, 0.2f);

        internal void Load(DeferredRenderer d)
        {
            if (d == null)
                return;
            d.lutProfile = (DeferredRenderer.LUTProfile)Mathf.Clamp(LutProfile, 0, 2);
            d.skinLutIntensity = LutIntensity;
            d.skinLutBias = LutBias;
            d.skinLutScale = LutScale;
            d.shadowLutBias = ShadowLutBias;
            d.shadowLutScale = ShadowLutScale;
            d.screenSpaceSSS = ScreenSpaceSSS;
            d.sssBlurWeight = BlurWeight;
            d.sssBlurRadius = BlurRadius;
            d.sssBlurDepthRange = BlurDepthRange;
            d.normalTest = NormalTest;
            d.depthTest = DepthTest;
            d.sssBlurIter = Mathf.Max(1, BlurIterations);
            d.sssBlurAlbedo = BlurAlbedo;
            d.colorBleedWeights = ColorBleed;
            d.transAbsorption = TransAbsorption;
            d.bakedThickness = BakedThickness;
            d.transWeight = TransWeight;
            d.transShadowWeight = TransShadowWeight;
            d.transDistortion = TransDistortion;
            d.transFalloff = TransFalloff;
            d.thicknessBias = ThicknessBias;
            // Everything again, command buffers included: switching the screen-space blur on or off
            // needs new buffers, not only new values.
            if (d.isActiveAndEnabled)
                d.UpdateSkinSettings();
            else
                d.MarkDirty();
        }
    }
}
