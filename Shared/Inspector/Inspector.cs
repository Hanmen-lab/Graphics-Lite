using ADV.Commands.Base;
using Graphics.AmplifyOcclusion;
using Graphics.GlobalFog;
using Graphics.GTAO;
using Graphics.Settings;
using Graphics.VAO;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Graphics.Inspector
{
    internal class Inspector
    {
        public static int adaptiveColumns = 3;
        public static int adaptivePreviewColumns = 6;
        public static int maxTextWidth = 480;



        public enum Tab
        {
            Environmental,
            Probes,
            Lights,
            GI,
            PostProcessing,
            AntiAliasing,
            SSS,
            Presets,
            Settings,
            Screenshot
        }

        public enum PostProcessingSubTab
        {
            Volume,
            AmbientOcclusion,
            Bloom,
            ColorGrading,
            SSR,
            AutoExposure,
            ChromaticAberration,
            DepthOfField,
            Grain,
            Vignette,
            MotionBlur,
            SunShafts,
            GlobalFog,
            LuxWater,
            Aura,
            VolumetricLights
        }

        public static readonly Dictionary<Tab, string> DisplayNames = new Dictionary<Tab, string>
        {
            { Tab.Environmental, "Environment" },
            { Tab.Probes, "Probes" },
            { Tab.Lights, "Lights" },
            { Tab.GI, "Global Illumination" },
            { Tab.PostProcessing, "Post Processing" },
            { Tab.AntiAliasing, "Anti-Aliasing" },
            { Tab.SSS, "SSS" },
            { Tab.Presets, "Presets" },
            { Tab.Settings, "Settings" },
            { Tab.Screenshot, "Screenshot" }
        };

        public static readonly Dictionary<PostProcessingSubTab, string> PostProcessingDisplayNames = new Dictionary<PostProcessingSubTab, string>
        {
            { PostProcessingSubTab.Volume, "Volume" },
            { PostProcessingSubTab.AmbientOcclusion, "Ambient Occlusion" },
            { PostProcessingSubTab.Bloom, "Bloom" },
            { PostProcessingSubTab.ColorGrading, "Color Grading" },
            { PostProcessingSubTab.SSR, "SSR" },
            { PostProcessingSubTab.AutoExposure, "Auto Exposure" },
            { PostProcessingSubTab.ChromaticAberration, "Chromatic Effects" },
            { PostProcessingSubTab.DepthOfField, "Depth Of Field" },
            { PostProcessingSubTab.Grain, "Grain" },
            { PostProcessingSubTab.Vignette, "Vignette" },
            { PostProcessingSubTab.MotionBlur, "Motion Blur" },
            { PostProcessingSubTab.SunShafts, "Sun Shafts HDR" },
            { PostProcessingSubTab.GlobalFog, "Global Fog" },
            { PostProcessingSubTab.LuxWater, "Lux Water" },
            { PostProcessingSubTab.Aura, "Aura" },
            { PostProcessingSubTab.VolumetricLights, "Volumetric Lights" }
        };

        internal Tab SelectedTab { get; set; }
        internal PostProcessingSubTab SelectedPostProcessingSubTab { get; set; }
        internal int SelectedProbeIndex { get; set; }
        internal Graphics Parent { get; set; }

        internal Inspector(Graphics parent)
        {
            Parent = parent;

            if (StartOffsetX + 20 > Screen.width || StartOffsetY + 20 > Screen.height)
            {
                StartOffsetX = (Screen.width - Graphics.ConfigWindowWidth.Value) / 2;
                StartOffsetY = (Screen.height - Graphics.ConfigWindowHeight.Value) / 2;
            }

            if (StartOffsetX < 0) StartOffsetX = 0;
            if (StartOffsetY < 0) StartOffsetY = 0;
        }

        internal static int Width
        {
            get => Graphics.ConfigWindowWidth.Value;
            set { Graphics.ConfigWindowWidth.Value = value; }
        }

        internal static int Height
        {
            get => Graphics.ConfigWindowHeight.Value;
            set { Graphics.ConfigWindowHeight.Value = value; }
        }

        internal static int StartOffsetX
        {
            get => Graphics.ConfigWindowOffsetX.Value;
            set => Graphics.ConfigWindowOffsetX.Value = value;
        }

        internal static int StartOffsetY
        {
            get => Graphics.ConfigWindowOffsetY.Value;
            set => Graphics.ConfigWindowOffsetY.Value = value;
        }

        internal void DrawTabs()
        {
            switch (SelectedTab)
            {
                case Tab.Environmental:
                    LightingInspector.Draw(Parent.LightingSettings, Parent.SkyboxManager, Parent.LightManager, Parent.Settings, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.Lights:
                    LightInspector.Draw(Parent.Settings, Parent.LightManager, Parent.LightingSettings, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.Probes:
                    ReflectionProbeInspector.Draw(Parent.LightingSettings, Parent.SkyboxManager, Parent.Settings, Parent.Settings.ShowAdvancedSettings, SelectedProbeIndex);
                    break;
                case Tab.GI:
                    SEGIInspector.Draw(Parent.LightManager, Parent.Settings);
                    break;
                case Tab.PostProcessing:
                    PostProcessingInspector.Draw(Parent.LightManager, Parent.PostProcessingSettings, Parent.Settings, Parent.PostProcessingManager, Parent.Settings.ShowAdvancedSettings, SelectedPostProcessingSubTab);
                    break;
                case Tab.AntiAliasing:
                    AntiAliasingInspector.Draw(Parent.Settings, Parent.CameraSettings, Parent.PostProcessingSettings, Parent.PostProcessingManager, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.SSS:
                    SSSInspector.Draw(Parent.Settings);
                    break;
                case Tab.Presets:
                    PresetInspector.Draw(Parent.PresetManager, Parent.Settings, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.Settings:
                    SettingsInspector.Draw(Parent.CameraSettings, Parent.Settings, Parent.LightManager, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.Screenshot:
                    ScreenshotInspector.Draw(Parent.Settings);
                    break;
            }
        }

        /// <summary>The scene's reflection probes by name, for the rail.</summary>
        internal string[] ProbeNames()
        {
            ReflectionProbe[] probes = Parent.SkyboxManager != null ? Parent.SkyboxManager.GetReflectinProbes() : null;
            if (probes == null || probes.Length == 0)
                return new string[0];
            if (SelectedProbeIndex >= probes.Length)
                SelectedProbeIndex = 0;
            string[] names = new string[probes.Length];
            for (int i = 0; i < probes.Length; i++)
            {
                string probeName = probes[i] != null ? probes[i].name : $"Probe {i}";
                if (probeName.Length > 18)
                    probeName = probeName.Substring(0, 18) + "...";
                names[i] = probeName;
            }
            return names;
        }

        /// <summary>Which page the window shows: one per tab, and one per post-processing sub-tab.</summary>
        internal string PageKey => SelectedTab == Tab.PostProcessing ? "PostProcessing/" + SelectedPostProcessingSubTab : SelectedTab.ToString();

        internal bool IsPostProcessingEffectEnabled(PostProcessingSubTab subTab)
        {
            return IsPostProcessingEffectEnabled(subTab, Parent.PostProcessingSettings);
        }

        private bool IsPostProcessingEffectEnabled(PostProcessingSubTab subTab, PostProcessingSettings settings)
        {
            if (settings == null)
                return false;

            switch (subTab)
            {
                case PostProcessingSubTab.Volume:
                    return settings.Volume != null;

                case PostProcessingSubTab.AmbientOcclusion:
                    return settings.ambientOcclusionLayer != null && settings.ambientOcclusionLayer.enabled.value
                        || VAOManager.settings != null && VAOManager.settings.Enabled
                        || GTAOManager.settings != null && GTAOManager.settings.Enabled
                        || AmplifyOccManager.settings != null && AmplifyOccManager.settings.Enabled;

                case PostProcessingSubTab.Bloom:
                    return settings.bloomLayer != null && settings.bloomLayer.enabled.value;

                case PostProcessingSubTab.ColorGrading:
                    return settings.colorGradingLayer != null && settings.colorGradingLayer.enabled.value
                        || settings.agxColorLayer != null && settings.agxColorLayer.enabled.value
                        || settings.agxColorPostLayer != null && settings.agxColorPostLayer.enabled.value
                        || settings.colorClippingLayer != null && settings.colorClippingLayer.enabled.value;

                case PostProcessingSubTab.SSR:
                    return settings.screenSpaceReflectionsLayer != null && settings.screenSpaceReflectionsLayer.enabled.value;

                case PostProcessingSubTab.AutoExposure:
                    return settings.autoExposureLayer != null && settings.autoExposureLayer.enabled.value;

                case PostProcessingSubTab.ChromaticAberration:
                    return settings.chromaticAberrationLayer != null && settings.chromaticAberrationLayer.enabled.value;

                case PostProcessingSubTab.DepthOfField:
                    return settings.depthOfFieldLayer != null && settings.depthOfFieldLayer.enabled.value;

                case PostProcessingSubTab.Grain:
                    return FilmGrainManager.settings != null && FilmGrainManager.settings.enabled;

                case PostProcessingSubTab.Vignette:
                    return settings.vignetteLayer != null && settings.vignetteLayer.enabled.value;

                case PostProcessingSubTab.MotionBlur:
                    return settings.motionBlurLayer != null && settings.motionBlurLayer.enabled.value;

                case PostProcessingSubTab.SunShafts:
                    return settings.sunShaftsHDRLayer != null && settings.sunShaftsHDRLayer.enabled.value;

                case PostProcessingSubTab.GlobalFog:
                    return GlobalFogManager.settings != null && GlobalFogManager.settings.Enabled;

                case PostProcessingSubTab.LuxWater:
                    return LuxWater_UnderWaterRenderingManager.settings != null && LuxWater_UnderWaterRenderingManager.settings.Enabled;

                case PostProcessingSubTab.Aura:
                    return AuraManager.Available && AuraManager.settings != null && AuraManager.settings.Enabled;

                case PostProcessingSubTab.VolumetricLights:
                    return VolumetricLightManager.settings != null && VolumetricLightManager.settings.Enabled;

                default:
                    return false;
            }
        }
    }
}