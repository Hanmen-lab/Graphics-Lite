using ADV.Commands.Object;
using Graphics.Settings;
using Graphics.Textures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Graphics
{
    // Create an interface that defines what AuraManagerImpl does
    internal interface IAuraManagerImpl
    {
        void Initialize();
        void UpdateSettings();
    }

    // Interface class that doesn't contain references to Aura2API
    public class AuraManager
    {
        public static bool Available { get; private set; }
        public static AuraSettings settings;

        // Reference interface instead of concrete implementation
        private static IAuraManagerImpl _impl;

        internal void Initialize()
        {
            try
            {
                var auraType = Type.GetType("Aura2API.AuraCamera, Aura2_Core");
                Available = auraType != null;
            }
            catch
            {
                Available = false;
            }

            if (settings == null)
            {
                settings = new AuraSettings();
            }

            if (Available)
            {
                // Use a factory method to create the implementation
                _impl = CreateAuraManagerImpl();
                _impl.Initialize();
            }
        }

        // Factory method that keeps Aura2API references isolated, since it's not called unless Aura2API exists
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IAuraManagerImpl CreateAuraManagerImpl()
        {
            return new AuraManagerImpl();
        }

        internal static void UpdateSettings()
        {
            if (settings == null)
                settings = new AuraSettings();

            if (Available && _impl != null)
            {
                _impl.UpdateSettings();
            }
        }
    }
}

namespace Graphics
{
    using Aura2API;

    // This class is only created if Aura2API exists to prevent plugin from failing to load
    internal class AuraManagerImpl : IAuraManagerImpl
    {
        internal AuraSettings settings { get { return AuraManager.settings; } set { AuraManager.settings = value; } }
        internal AuraCamera AuraInstance;
        //internal AuraBaseSettings BaseSettings;
        //internal AuraQualitySettings QualitySettings;

        // Initialize Components
        public void Initialize()
        {
            AuraInstance = Graphics.Instance.CameraSettings.MainCamera.GetComponent<AuraCamera>();
            if (settings == null)
            {
                settings = new AuraSettings();
            }

            if (AuraInstance)
            {
                LoadSettings();
                LoadBaseSettings(AuraInstance.frustumSettings.BaseSettings);
                LoadQualitySettings(AuraInstance.frustumSettings.QualitySettings);
            }
        }

        public void UpdateSettings()
        {
            if (settings == null)
                settings = new AuraSettings();

            if (AuraInstance != null)
            {
                LoadSettings();
                LoadBaseSettings(AuraInstance.frustumSettings.BaseSettings);
                LoadQualitySettings(AuraInstance.frustumSettings.QualitySettings);
            }
        }
        void LoadSettings()
        {
            if (AuraInstance == null)
                return;

            AuraInstance.enabled = settings.Enabled;

            if (!settings.Enabled)
                return;
        }


        void LoadBaseSettings(AuraBaseSettings baseSettings)
        {
            if (baseSettings == null)
                return;

            baseSettings.useDensity = settings.useDensity.value;

            if (settings.density.overrideState)
                baseSettings.density = settings.density.value;
            else
                baseSettings.density = 0.25f;

            baseSettings.useScattering = settings.useScattering.value;

            if (settings.scattering.overrideState)
                baseSettings.scattering = settings.scattering.value;
            else
                baseSettings.scattering = 0.5f;

            baseSettings.useAmbientLighting = settings.useAmbientLighting.value;

            if (settings.ambientLightingStrength.overrideState)
                baseSettings.ambientLightingStrength = settings.ambientLightingStrength.value;
            else
                baseSettings.ambientLightingStrength = 1f;

            baseSettings.useColor = settings.useColor.value;

            baseSettings.color = settings.color;

            if (settings.colorStrength.overrideState)
                baseSettings.colorStrength = settings.colorStrength.value;
            else
                baseSettings.colorStrength = 1f;

            baseSettings.useTint = settings.useTint.value;

            baseSettings.tint = settings.tint;

            if (settings.tintStrength.overrideState)
                baseSettings.tintStrength = settings.tintStrength.value;
            else
                baseSettings.tintStrength = 1f;

            baseSettings.useExtinction = settings.useExtinction.value;

            if (settings.extinction.overrideState)
                baseSettings.extinction = settings.extinction.value;
            else
                baseSettings.extinction = 0.75f;
        }

        void LoadQualitySettings(AuraQualitySettings qualitySettings)
        {
            if (qualitySettings == null)
                return;

            qualitySettings.displayVolumetricLightingBuffer = settings.displayVolumetricLightingBuffer.value;

            //qualitySettings.enableAutomaticStereoResizing = settings.enableAutomaticStereoResizing.value;

            if (settings.farClipPlaneDistance.overrideState)
                qualitySettings.farClipPlaneDistance = settings.farClipPlaneDistance.value;
            else
                qualitySettings.farClipPlaneDistance = 128f;

            if (settings.depthBiasCoefficient.overrideState)
                qualitySettings.depthBiasCoefficient = settings.depthBiasCoefficient.value;
            else
                qualitySettings.depthBiasCoefficient = 0.35f;

            //qualitySettings.enableDithering = settings.enableDithering.value;
            //qualitySettings.texture3DFiltering = settings.texture3DFiltering;
            qualitySettings.EXPERIMENTAL_enableDenoisingFilter = settings.EXPERIMENTAL_enableDenoisingFilter.value;
            qualitySettings.EXPERIMENTAL_denoisingFilterRange = (Aura2API.DenoisingFilterRange)settings.EXPERIMENTAL_denoisingFilterRange;
            qualitySettings.EXPERIMENTAL_enableBlurFilter = settings.EXPERIMENTAL_enableBlurFilter.value;
            qualitySettings.EXPERIMENTAL_blurFilterRange = (Aura2API.BlurFilterRange)settings.EXPERIMENTAL_blurFilterRange;
            qualitySettings.EXPERIMENTAL_blurFilterType = (Aura2API.BlurFilterType)settings.EXPERIMENTAL_blurFilterType;

            if (settings.EXPERIMENTAL_blurFilterGaussianDeviation.overrideState)
                qualitySettings.EXPERIMENTAL_blurFilterGaussianDeviation = settings.EXPERIMENTAL_blurFilterGaussianDeviation.value;
            else
                qualitySettings.EXPERIMENTAL_blurFilterGaussianDeviation = 0.0025f;

            //qualitySettings.enableTemporalReprojection = settings.enableTemporalReprojection.value;

            //if (temporalReprojectionFactor.overrideState)
            //    qualitySettings.temporalReprojectionFactor = settings.temporalReprojectionFactor.value;
            //else
            //    qualitySettings.temporalReprojectionFactor = 0.95f;

            //qualitySettings.enableOcclusionCulling = settings.enableOcclusionCulling.value;
            //qualitySettings.debugOcclusionCulling = settings.debugOcclusionCulling.value;
            //qualitySettings.occlusionCullingAccuracy = settings.occlusionCullingAccuracy;
            //qualitySettings.enableLightProbes = settings.enableLightProbes.value;
        }
        /*public static void ReorderAura()
        {
            Camera camera = Camera.main;

            // Get components
            var helperScript = camera.GetComponent<Fsr3UpscalerImageEffectHelper>();
            var postProcessLayer = camera.GetComponent<PostProcessLayer>();

            helperScript.enabled = false;
            postProcessLayer.enabled = false;

            if (helperScript == null || postProcessLayer == null) return;

            // Save component with Reflection
            var postProcessData = CopyComponentData(postProcessLayer);

            // Remove components
            DestroyImmediate(helperScript);
            DestroyImmediate(postProcessLayer);

            // Add components back in correct order
            var newHelper = camera.gameObject.AddComponent<Fsr3UpscalerImageEffectHelper>();
            var newPostProcess = camera.gameObject.AddComponent<PostProcessLayer>();
            newHelper.enabled = false;
            newPostProcess.enabled = false;
            // Restore data to PostProcessLayer
            RestoreComponentData(newPostProcess, postProcessData);

            newHelper.enabled = true;
            newPostProcess.enabled = true;
        }*/
    }
}