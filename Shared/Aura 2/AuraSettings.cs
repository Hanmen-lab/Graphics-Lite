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

        public void Load(AuraCamera component)
        {
            if (component == null)
                return;

            component.enabled = Enabled;

            if (!Enabled)
                return;
        }

        public void LoadBaseSettings(AuraBaseSettings baseSettings)
        {
            if (baseSettings == null)
                return;

            baseSettings.useDensity = useDensity.value;

            if (density.overrideState)
                baseSettings.density = density.value;
            else
                baseSettings.density = 0.25f;

            baseSettings.useScattering = useScattering.value;

            if (scattering.overrideState)
                baseSettings.scattering = scattering.value;
            else
                baseSettings.scattering = 0.5f;

            baseSettings.useAmbientLighting = useAmbientLighting.value;

            if (ambientLightingStrength.overrideState)
                baseSettings.ambientLightingStrength = ambientLightingStrength.value;
            else
                baseSettings.ambientLightingStrength = 1f;

            baseSettings.useColor = useColor.value;

            baseSettings.color = color;

            if (colorStrength.overrideState)
                baseSettings.colorStrength = colorStrength.value;
            else
                baseSettings.colorStrength = 1f;

            baseSettings.useTint = useTint.value;

            baseSettings.tint = tint;

            if (tintStrength.overrideState)
                baseSettings.tintStrength = tintStrength.value;
            else
                baseSettings.tintStrength = 1f;

            baseSettings.useExtinction = useExtinction.value;

            if (extinction.overrideState)
                baseSettings.extinction = extinction.value;
            else
                baseSettings.extinction = 0.75f;
        }

        public void LoadQualitySettings(AuraQualitySettings qualitySettings)
        {
            if (qualitySettings == null)
                return;

            qualitySettings.displayVolumetricLightingBuffer = displayVolumetricLightingBuffer.value;

            //qualitySettings.enableAutomaticStereoResizing = enableAutomaticStereoResizing.value;

            if (farClipPlaneDistance.overrideState)
                qualitySettings.farClipPlaneDistance = farClipPlaneDistance.value;
            else
                qualitySettings.farClipPlaneDistance = 128f;

            if (depthBiasCoefficient.overrideState)
                qualitySettings.depthBiasCoefficient = depthBiasCoefficient.value;
            else
                qualitySettings.depthBiasCoefficient = 0.35f;

            //qualitySettings.enableDithering = enableDithering.value;
            //qualitySettings.texture3DFiltering = texture3DFiltering;
            qualitySettings.EXPERIMENTAL_enableDenoisingFilter = EXPERIMENTAL_enableDenoisingFilter.value;
            qualitySettings.EXPERIMENTAL_denoisingFilterRange = EXPERIMENTAL_denoisingFilterRange;
            qualitySettings.EXPERIMENTAL_enableBlurFilter = EXPERIMENTAL_enableBlurFilter.value;
            qualitySettings.EXPERIMENTAL_blurFilterRange = EXPERIMENTAL_blurFilterRange;
            qualitySettings.EXPERIMENTAL_blurFilterType = EXPERIMENTAL_blurFilterType;

            if (EXPERIMENTAL_blurFilterGaussianDeviation.overrideState)
                qualitySettings.EXPERIMENTAL_blurFilterGaussianDeviation = EXPERIMENTAL_blurFilterGaussianDeviation.value;
            else
                qualitySettings.EXPERIMENTAL_blurFilterGaussianDeviation = 0.0025f;

            //qualitySettings.enableTemporalReprojection = enableTemporalReprojection.value;

            //if (temporalReprojectionFactor.overrideState)
            //    qualitySettings.temporalReprojectionFactor = temporalReprojectionFactor.value;
            //else
            //    qualitySettings.temporalReprojectionFactor = 0.95f;

            //qualitySettings.enableOcclusionCulling = enableOcclusionCulling.value;
            //qualitySettings.debugOcclusionCulling = debugOcclusionCulling.value;
            //qualitySettings.occlusionCullingAccuracy = occlusionCullingAccuracy;
            //qualitySettings.enableLightProbes = enableLightProbes.value;
        }
    }
}
