using Aura2API;
using MessagePack;
using UnityEngine;


namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class AuraSettings
    {
        //component
        public bool Enabled = false;

        //base settings
        public BoolValue useDensity = new BoolValue(true, false);
        public FloatValue density = new FloatValue(0.25f, false);
        public BoolValue useScattering = new BoolValue(true, false);
        public FloatValue scattering = new FloatValue(0.5f, false);
        public BoolValue useAmbientLighting = new BoolValue(true, false);
        public FloatValue ambientLightingStrength = new FloatValue(1f, false);
        public BoolValue useColor = new BoolValue(false, false);
        public Color color = Color.cyan * 0.5f;
        public FloatValue colorStrength = new FloatValue(1f, false);
        public BoolValue useTint = new BoolValue(false, false);
        public Color tint = Color.yellow;
        public FloatValue tintStrength = new FloatValue(1f, false);
        public BoolValue useExtinction = new BoolValue(false, false);
        public FloatValue extinction = new FloatValue(0.75f, false);

        ////quality settings
        public BoolValue displayVolumetricLightingBuffer = new BoolValue(false, false);
        //public BoolValue enableAutomaticStereoResizing = new BoolValue(true, false);
        public FloatValue farClipPlaneDistance = new FloatValue(128f, false);
        public FloatValue depthBiasCoefficient = new FloatValue(0.35f, false);
        //public BoolValue enableDithering = new BoolValue(true, false);
        //public Texture3DFiltering texture3DFiltering = Texture3DFiltering.Cubic;
        public BoolValue EXPERIMENTAL_enableDenoisingFilter = new BoolValue(true, false);
        public DenoisingFilterRange EXPERIMENTAL_denoisingFilterRange = DenoisingFilterRange.TwoNeighbours;
        public BoolValue EXPERIMENTAL_enableBlurFilter = new BoolValue(true, false);
        public BlurFilterRange EXPERIMENTAL_blurFilterRange;
        public BlurFilterType EXPERIMENTAL_blurFilterType;
        public FloatValue EXPERIMENTAL_blurFilterGaussianDeviation = new FloatValue(0.0025f, false);
        //public BoolValue enableTemporalReprojection = new BoolValue(true, false);
        //public FloatValue temporalReprojectionFactor = new FloatValue(0.95f, false);
        //public BoolValue enableOcclusionCulling = new BoolValue(true, false);
        //public BoolValue debugOcclusionCulling = new BoolValue(false, false);
        //public OcclusionCullingAccuracy occlusionCullingAccuracy;
        //public BoolValue enableLightProbes = new BoolValue(true, false);
    }
    // Use our own mirror types so we can keep settings even if the user doesn't have Aura2 installed.
    public enum DenoisingFilterRange
    {
        OneNeighbour,
        TwoNeighbours,
        ThreeNeighbours
    }
    public enum BlurFilterRange
    {
        OneNeighbour,
        ThreeNeighbours,
        FiveNeighbours
    }
    public enum BlurFilterType
    {
        Box,
        Gaussian
    }
}
