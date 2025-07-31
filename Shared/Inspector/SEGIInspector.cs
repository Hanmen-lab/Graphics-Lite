using UnityEngine;
using static Graphics.Inspector.Util;
using Graphics.Settings;
using Graphics.SEGI;

namespace Graphics.Inspector
{
    internal static class SEGIInspector
    {
        private static Vector2 segiScrollView;
        private static SEGI.SEGI segi = SEGIManager.SEGIInstance;

        internal static void Draw(LightManager lightManager, GlobalSettings renderSettings)
        {
            GUIStyle BoxPadding = new GUIStyle(GUI.skin.box);
            BoxPadding.padding = new RectOffset(0, 0, 0, 0);
            BoxPadding.normal.background = null;

            GUIStyle EmptyBox = new GUIStyle(GUI.skin.box);
            EmptyBox.padding = new RectOffset(25, 20, 0, 0);
            EmptyBox.normal.background = null;

            GUIStyle SmallBox = new GUIStyle(GUI.skin.box);
            SmallBox.normal.background = null;

            GUIStyle TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(0, 0, 0, 0);

            if (SEGIManager.settings != null)
            {
                SEGISettings segiSettings = SEGIManager.settings;

                GUILayout.BeginVertical(TabContent);
                GUILayout.BeginVertical(EmptyBox);

                if (Graphics.Instance.CameraSettings.RenderingPath != CameraSettings.AIRenderingPath.Deferred)
                {
                    GUI.enabled = false;
                }
                GUILayout.Space(35);
                Switch(renderSettings.FontSize, "SEGI (Deferred Only)", segiSettings.enabled, true, enabled =>
                {
                    segiSettings.enabled = enabled;
                    SEGIManager.UpdateSettings();
                });

                if (segiSettings.enabled)
                {
                    GUILayout.Space(30);
                    Toggle("Update GI", segiSettings.updateGI, true, update => { segiSettings.updateGI = update; SEGIManager.UpdateSettings(); });
                    Label("You can disable 'Update GI' in the static scenes after the adjustements are done to boost performance.", "", false);
                    GUILayout.Space(10);
                    Label("DEBUG TOOLS", "", true);
                    GUILayout.Space(2);
                    Selection("Mode:", segiSettings.debugTools, debugtools => { segiSettings.debugTools = debugtools; SEGIManager.UpdateSettings(); });
                    GUILayout.Space(10);
                    Label("VRAM Usage:", segi.VramUsage.ToString("N2") + " MB");
                    GUILayout.Space(10);
                    GUILayout.EndVertical();
                    segiScrollView = GUILayout.BeginScrollView(segiScrollView);
                    GUILayout.BeginVertical(EmptyBox);
                    {
                        GUILayout.Space(20);
                        Label("MAIN CONFIGURATION", "", true);
                        GUILayout.Space(2);
                        Selection("Voxel Resolution", segiSettings.voxelResolution, resolution => { segiSettings.voxelResolution = resolution; SEGIManager.UpdateSettings(); });
                        Toggle("Voxel AA", segiSettings.voxelAA, false, aa => { segiSettings.voxelAA = aa; SEGIManager.UpdateSettings(); });
                        Toggle("Infinite Bounces (Slow)", segiSettings.infiniteBounces, false, bounce => { segiSettings.infiniteBounces = bounce; SEGIManager.UpdateSettings(); });
                        Toggle("Gaussian Mip Filter", segiSettings.gaussianMipFilter, false, filter => { segiSettings.gaussianMipFilter = filter; SEGIManager.UpdateSettings(); });
                        Text("Voxel Space Size", segiSettings.voxelSpaceSize, "N0", size => { segiSettings.voxelSpaceSize = size; SEGIManager.UpdateSettings(); });
                        Text("Shadow Space Size", segiSettings.shadowSpaceSize, "N0", size => { segiSettings.shadowSpaceSize = size; SEGIManager.UpdateSettings(); });
                        SelectionMask("GI Culling Mask", segiSettings.giCullingMask, mask => { segiSettings.giCullingMask = mask; SEGIManager.UpdateSettings(); });

                        GUILayout.Space(30);
                        Label("TRACING PROPERTIES", "", true);
                        GUILayout.Space(2);
                        Slider("Temporal Blend Weight", segiSettings.temporalBlendWeight, 0f, 1f, "N2", weight => { segiSettings.temporalBlendWeight = weight; SEGIManager.UpdateSettings(); });
                        Toggle("Bilateral Filtering", segiSettings.useBilateralFiltering, false, filter => { segiSettings.useBilateralFiltering = filter; SEGIManager.UpdateSettings(); });
                        Toggle("Half Resolution (Fast)", segiSettings.halfResolution, false, half => { segiSettings.halfResolution = half; SEGIManager.UpdateSettings(); });
                        Toggle("Stochastic Sampling", segiSettings.stochasticSampling, false, sampling => { segiSettings.stochasticSampling = sampling; SEGIManager.UpdateSettings(); });
                        Slider("Cones", segiSettings.cones, 1, 128, cones => { segiSettings.cones = cones; SEGIManager.UpdateSettings(); });
                        if (segiSettings.infiniteBounces)
                            Slider("Secondary Cones", segiSettings.secondaryCones, 3, 16, cones => { segiSettings.secondaryCones = cones; SEGIManager.UpdateSettings(); });
                        Slider("Cones Trace Steps", segiSettings.coneTraceSteps, 1, 32, cones => { segiSettings.coneTraceSteps = cones; SEGIManager.UpdateSettings(); });
                        Slider("Cones Length", segiSettings.coneLength, 0.1f, 2f, "N2", cones => { segiSettings.coneLength = cones; SEGIManager.UpdateSettings(); });
                        Slider("Cones Width", segiSettings.coneWidth, 0.5f, 6f, "N2", cones => { segiSettings.coneWidth = cones; SEGIManager.UpdateSettings(); });
                        Slider("Cones Trace Bias", segiSettings.coneTraceBias, 0f, 4f, "N2", cones => { segiSettings.coneTraceBias = cones; SEGIManager.UpdateSettings(); });
                        GUILayout.Space(30);

                        Label("SUN PROPERTIES", "", true);
                        GUILayout.Space(2);
                        Slider("Soft Sunlight", segiSettings.softSunlight, 0f, 16f, "N1", soft => { segiSettings.softSunlight = soft; SEGIManager.UpdateSettings(); });
                        GUILayout.Space(5);
                        LightSelector(lightManager, "Sun Source", RenderSettings.sun, light => { RenderSettings.sun = light; SEGIManager.UpdateSettings(); ConnectSunToUnderwater.ConnectSun(); });

                        GUILayout.Space(30);
                        Label("SKY PROPERTIES", "", true);
                        GUILayout.Space(2);
                        SliderColor("Sky Colour", segiSettings.skyColor, colour => { segiSettings.skyColor = colour; SEGIManager.UpdateSettings(); });
                        GUILayout.Space(5);
                        Slider("Sky Intensity", segiSettings.skyIntensity, 0f, 2f, "N2", intensity => { segiSettings.skyIntensity = intensity; SEGIManager.UpdateSettings(); });
                        Toggle("Spherical Skylight", segiSettings.sphericalSkylight, false, spherical => { segiSettings.sphericalSkylight = spherical; SEGIManager.UpdateSettings(); });
                        GUILayout.Space(30);

                        Label("AMBIENT OCCLUSION", "", true);
                        GUILayout.Space(2);
                        Slider("Inner Occlusion Layers", segiSettings.innerOcclusionLayers, 0, 2, layers => { segiSettings.innerOcclusionLayers = layers; SEGIManager.UpdateSettings(); });
                        Slider("Occlusion Strength", segiSettings.occlusionStrength, 0f, 4f, "N2", cones => { segiSettings.occlusionStrength = cones; SEGIManager.UpdateSettings(); });
                        Slider("Near Occlusion Strength", segiSettings.nearOcclusionStrength, 0f, 4f, "N2", cones => { segiSettings.nearOcclusionStrength = cones; SEGIManager.UpdateSettings(); });
                        Slider("Far Occlusion Strength", segiSettings.farOcclusionStrength, 0f, 4f, "N2", cones => { segiSettings.farOcclusionStrength = cones; SEGIManager.UpdateSettings(); });
                        Slider("Farthest Occlusion Strength", segiSettings.farthestOcclusionStrength, 0f, 4f, "N2", cones => { segiSettings.farthestOcclusionStrength = cones; SEGIManager.UpdateSettings(); });
                        Slider("Occlusion Power", segiSettings.occlusionPower, 0.001f, 4f, "N2", cones => { segiSettings.occlusionPower = cones; SEGIManager.UpdateSettings(); });
                        if (segiSettings.infiniteBounces)
                            Slider("Secondary Occlusion Strength", segiSettings.secondaryOcclusionStrength, 0.1f, 4f, "N2", cones => { segiSettings.secondaryOcclusionStrength = cones; SEGIManager.UpdateSettings(); });
                        GUILayout.Space(30);
                        Label("GI PROPERTIES", "", true);
                        GUILayout.Space(2);
                        Slider("Near Light Gain", segiSettings.nearLightGain, 0f, 4f, "N2", cones => { segiSettings.nearLightGain = cones; SEGIManager.UpdateSettings(); });
                        Slider("GI Gain", segiSettings.giGain, 0f, 4f, "N2", cones => { segiSettings.giGain = cones; SEGIManager.UpdateSettings(); });
                        if (segiSettings.infiniteBounces)
                            Slider("Secondary Bounce Gain", segiSettings.secondaryBounceGain, 0f, 4f, "N2", cones => { segiSettings.secondaryBounceGain = cones; SEGIManager.UpdateSettings(); });
                        GUILayout.Space(30);
                        Label("REFLECTIONS", "", true);
                        GUILayout.Space(2);
                        Toggle("Do Reflections", segiSettings.doReflections, false, reflections => { segiSettings.doReflections = reflections; SEGIManager.UpdateSettings(); });
                        Slider("Reflection Steps", segiSettings.reflectionSteps, 12, 128, cones => { segiSettings.reflectionSteps = cones; SEGIManager.UpdateSettings(); });
                        Slider("Reflection Occlusion Power", segiSettings.reflectionOcclusionPower, 0.001f, 4f, "N2", cones => { segiSettings.reflectionOcclusionPower = cones; SEGIManager.UpdateSettings(); });
                        Slider("Sky Reflection Intensity", segiSettings.skyReflectionIntensity, 0f, 1f, "N2", intensity => { segiSettings.skyReflectionIntensity = intensity; SEGIManager.UpdateSettings(); });
                        GUILayout.Space(30);
                        Label("TROUBLESHOOTING:", "", true);
                        GUILayout.Space(5);
                        Label("Supported shaders:", "", false);
                        Label("Character: Next-Gen Body, Next Gen Face, Next Gen Eyes Vanilla/Deepdive", "", false);
                        Label("Clothing: Hanmen/Clothes True Cutoff", "", false);
                        Label("Accessories: Hanmen/Item Cutoff", "", false);
                        Label("Studio Items: Hanmen/Item Cutoff or Standard.", "", false);
                        GUILayout.Space(10);
                        LabelColorRed("Not Supported: AIT/Clothes True, AIT/Item, AIT/Skin True Face, ShaderForge/hair08, all Alpha Shaders... etc.", "", false);
                        GUILayout.Space(10);
                    }

                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    GUILayout.ExpandHeight(true);
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    GUILayout.EndVertical();
                    GUILayout.ExpandHeight(true);
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                }

                GUILayout.EndVertical();
                GUI.enabled = true;
            }

        }

    }

}