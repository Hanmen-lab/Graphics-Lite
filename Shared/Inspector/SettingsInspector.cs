//using BehaviorDesigner.Runtime.Tasks.Unity.UnityBehaviour;
using Graphics.GTAO;
using Graphics.SEGI;
using Graphics.Settings;
using UnityEngine;
using static Graphics.Inspector.Util;
using KKAPI;

namespace Graphics.Inspector
{
    internal static class SettingsInspector
    {
        private const float FOVMin = 10f;
        private const float FOVMax = 120f;
        private const float FOVDefault = 23.5f;
        private static Vector2 settingstScrollView;
        public delegate void RenderingPathChangedHandler();

        //public static event RenderingPathChangedHandler RenderingPathChanged;

        private static void OnRenderingPathChanged()
        {
            GTAOSettings gtaoSettings = GTAOManager.settings;
            SEGISettings segiSettings = SEGIManager.settings;

            if (Graphics.Instance.CameraSettings.RenderingPath != CameraSettings.AIRenderingPath.Deferred)
            {
                if (gtaoSettings.Enabled)
                {
                    gtaoSettings.Enabled = false;
                    GTAO.GTAOManager.UpdateSettings();
                    Graphics.Instance.Log.LogMessage("[Graphics] GTAO only works in Deferred Rendering mode. Disabled.");
                }

                if (segiSettings.enabled)
                {
                    segiSettings.enabled = false;
                    SEGI.SEGIManager.UpdateSettings();
                    Graphics.Instance.Log.LogMessage("[Graphics] SEGI only works in Deferred Rendering mode. Disabled.");
                }

            }

        }

        internal static void Draw(CameraSettings cameraSettings, GlobalSettings renderingSettings, bool showAdvanced)
        {

            settingstScrollView = GUILayout.BeginScrollView(settingstScrollView);
            GUILayout.BeginVertical(GUIStyles.tabcontent);
            {
                Label("VERSION:", Graphics.Version, true);
                GUILayout.Space(10);
                Toggle("Show Advanced Settings", renderingSettings.ShowAdvancedSettings, false, advanced => renderingSettings.ShowAdvancedSettings = advanced);
                GUILayout.Space(25);
                //GUILayout.Space(25);
                Label("CAMERA", "", true);
                GUILayout.Space(2);
                cameraSettings.ClearFlag = Selection("Clear Flags", cameraSettings.ClearFlag, flag => cameraSettings.ClearFlag = flag);
                if (showAdvanced)
                {
                    //changing studio camera's culling mask breaks studio, possibly due to cinemachine
                    GUI.enabled = false;
                    SelectionMask("Culling Mask", cameraSettings.CullingMask, mask => cameraSettings.CullingMask = mask);
                    GUI.enabled = true;
                }
                Slider("Near Clipping Plane", cameraSettings.NearClipPlane, 0.01f, 15000f, "N0", ncp => { cameraSettings.NearClipPlane = ncp; });
                Slider("Far Clipping Plane", cameraSettings.FarClipPlane, 0.01f, 150000f, "N0", ncp => { cameraSettings.FarClipPlane = ncp; });
                Selection("Rendering Path", cameraSettings.RenderingPath, path =>
                {
                    cameraSettings.RenderingPath = path;
                    OnRenderingPathChanged();
                });

                Slider("Field of View", cameraSettings.Fov, FOVMin, FOVMax, "N0", fov => { cameraSettings.Fov = fov; });
                Toggle("Occlusion Culling", cameraSettings.OcculsionCulling, false, culling => cameraSettings.OcculsionCulling = culling);
                Toggle("Allow HDR", cameraSettings.HDR, false, hdr => cameraSettings.HDR = hdr);
                //Toggle("Allow MSAA (Forward Only)", cameraSettings.MSAA, false, msaa => cameraSettings.MSAA = msaa);
                Toggle("Allow Dynamic Resolution", cameraSettings.DynamicResolution, false, dynamic => cameraSettings.DynamicResolution = dynamic);
                GUILayout.Space(25);
                Label("RENDERING", "", true);
                GUILayout.Space(1);
                Label("Colour Space", QualitySettings.activeColorSpace.ToString());
                Label("Quality Level", QualitySettings.names[QualitySettings.GetQualityLevel()]);
                Text("Pixel Light Count", renderingSettings.PixelLightCount, count => renderingSettings.PixelLightCount = count);
                Selection("Anisotropic Textures", renderingSettings.AnisotropicFiltering, filtering => renderingSettings.AnisotropicFiltering = filtering);
                //Slider("MSAA Multiplier", renderingSettings.AntiAliasing, 0, 8, aa => renderingSettings.AntiAliasing = aa);
                Toggle("Realtime Reflection Probes", renderingSettings.RealtimeReflectionProbes, false, realtime => renderingSettings.RealtimeReflectionProbes = realtime);
                Toggle("Pulse Realtime Reflection Probes", renderingSettings.PulseReflectionProbes, false, pulse => renderingSettings.PulseReflectionProbes = pulse);
                if (renderingSettings.PulseReflectionProbes)
                    Slider("Pulse Timing (Secs)", renderingSettings.PulseReflectionTimer, .25f, 10f, "N1", prt => { renderingSettings.PulseReflectionTimer = prt; });
                GUILayout.Space(25);
                Label("SHADOWS", "", true);
                GUILayout.Space(1);
                Selection("Shadowmask Mode", renderingSettings.ShadowmaskModeSetting, mode => renderingSettings.ShadowmaskModeSetting = mode);
                Selection("Shadows", renderingSettings.ShadowQualitySetting, setting => renderingSettings.ShadowQualitySetting = setting);
                Selection("Shadow Resolution", renderingSettings.ShadowResolutionSetting, resolution => renderingSettings.ShadowResolutionSetting = resolution);
                Selection("Shadow Projection", renderingSettings.ShadowProjectionSetting, projection => renderingSettings.ShadowProjectionSetting = projection);
                Text("Shadow Distance", renderingSettings.ShadowDistance, "N0", distance => renderingSettings.ShadowDistance = distance);
                Text("Shadow Near Plane Offset", renderingSettings.ShadowNearPlaneOffset, "N0", offset => renderingSettings.ShadowNearPlaneOffset = offset);
                GUILayout.Space(10);
                Toggle("Use PCSS (Experimental)", renderingSettings.UsePCSS, false, pcss => renderingSettings.UsePCSS = pcss);
                if (renderingSettings.UsePCSS)
                {
                    Slider("Blocker Sample Count", PCSSLight.Blocker_SampleCount, 1, 64, count => PCSSLight.Blocker_SampleCount = count);
                    Slider("PCF Sample Count", PCSSLight.PCF_SampleCount, 1, 64, count => PCSSLight.PCF_SampleCount = count);
                    Slider("Softness", PCSSLight.Softness, 0f, 7.5f, "N2", softness => PCSSLight.Softness = softness);
                    Slider("Softness Falloff", PCSSLight.SoftnessFalloff, 0f, 5f, "N2", softnessFalloff => PCSSLight.SoftnessFalloff = softnessFalloff);
                    Slider("Blocker Gradient Bias", PCSSLight.Blocker_GradientBias, 0f, 1f, "N2", bias => PCSSLight.Blocker_GradientBias = bias);
                    Slider("PCF Gradient Bias", PCSSLight.PCF_GradientBias, 0f, 1f, "N2", bias => PCSSLight.PCF_GradientBias = bias);
                    Slider("Max Static Gradient Bias", PCSSLight.MaxStaticGradientBias, 0f, 0.15f, "N2", bias => PCSSLight.MaxStaticGradientBias = bias);
                    Slider("Cascade Blend Distance", PCSSLight.CascadeBlendDistance, 0f, 1f, "N2", distance => PCSSLight.CascadeBlendDistance = distance);
                }
                GUILayout.Space(25);
                if (DitheredShadowsManager.settings != null)
                {
                    DitheredShadowsSettings ditheredSettings = DitheredShadowsManager.settings;

                    Toggle("Dithered Shadows", ditheredSettings.Enabled, false, enabled =>
                    {
                        ditheredSettings.Enabled = enabled;
                        DitheredShadowsManager.UpdateSettings();
                    });

                    if (ditheredSettings.Enabled)
                    {
                        GUILayout.Space(10);
                        Slider("Point Size", ditheredSettings.point_size.value, 0.0f, 0.1f, "N2", point => { ditheredSettings.point_size.value = point; DitheredShadowsManager.UpdateSettings(); });
                        Slider("Directional Size", ditheredSettings.direction_size.value, 0.0f, 0.1f, "N2", direction => { ditheredSettings.direction_size.value = direction; DitheredShadowsManager.UpdateSettings(); });
                        Slider("Spot Size", ditheredSettings.spot_size.value, 0.0f, 0.1f, "N2", spot => { ditheredSettings.spot_size.value = spot; DitheredShadowsManager.UpdateSettings(); });
                    }

                }

                GUILayout.Space(25);

                if (DecalsSystemManager.settings != null)
                {
                    DeferredDecalsSettings decalsSettings = DecalsSystemManager.settings;

                    ToggleAlt("DEFERRED DECALS", decalsSettings.enabled, true, enabled => { decalsSettings.enabled = enabled; DecalsSystemManager.UpdateSettings(); });

                    if (decalsSettings.enabled)
                    {
                        GUILayout.Space(30);
                        ToggleAlt("Lock Rebuild", decalsSettings.LockRebuild.value, false, lockRebuild => { decalsSettings.LockRebuild.value = lockRebuild; DecalsSystemManager.UpdateSettings(); });
                        //Selection("Terrain Decals", decalsSettings.TerrainDecals, terrainDecals => { decalsSettings.TerrainDecals = terrainDecals; DecalsSystemManager.UpdateSettings(); });
                        //if (decalsSettings.TerrainDecals != DeferredDecalsSettings.TerrainDecalsType.None)
                        //{
                        //    Slider("Terrain Height Map Size", decalsSettings.TerrainHeightMapSize.value, 256, 4096, size => { decalsSettings.TerrainHeightMapSize.value = size; DecalsSystemManager.UpdateSettings(); },
                        //        decalsSettings.TerrainHeightMapSize.overrideState, overrideState => { decalsSettings.TerrainHeightMapSize.overrideState = overrideState; DecalsSystemManager.UpdateSettings(); });
                        //}
                        ToggleAlt("Use Exclusion Mask", decalsSettings.UseExclusionMask.value, false, useExclusionMask => { decalsSettings.UseExclusionMask.value = useExclusionMask; DecalsSystemManager.UpdateSettings(); });
                        if (decalsSettings.UseExclusionMask.value)
                            SelectionMask("Exclusion Mask", decalsSettings.ExclusionMask, mask => { decalsSettings.ExclusionMask = mask; DecalsSystemManager.UpdateSettings(); });
                        ToggleAlt("Frustum Culling", decalsSettings.FrustumCulling.value, false, frustumCulling => { decalsSettings.FrustumCulling.value = frustumCulling; DecalsSystemManager.UpdateSettings(); });
                        ToggleAlt("Distance Culling", decalsSettings.DistanceCulling.value, false, distanceCulling => { decalsSettings.DistanceCulling.value = distanceCulling; DecalsSystemManager.UpdateSettings(); });
                        Slider("Start Fade Distance", decalsSettings.StartFadeDistance.value, 0f, 1000f, "N0", startFadeDistance => { decalsSettings.StartFadeDistance.value = startFadeDistance; DecalsSystemManager.UpdateSettings(); },
                            decalsSettings.StartFadeDistance.overrideState, overrideState => { decalsSettings.StartFadeDistance.overrideState = overrideState; DecalsSystemManager.UpdateSettings(); });
                        Slider("Fade Length", decalsSettings.FadeLength.value, 0f, 100f, "N0", fadeLength => { decalsSettings.FadeLength.value = fadeLength; DecalsSystemManager.UpdateSettings(); },
                            decalsSettings.FadeLength.overrideState, overrideState => { decalsSettings.FadeLength.overrideState = overrideState; DecalsSystemManager.UpdateSettings(); });

                        //ToggleAlt("Draw Decal Gizmos", decalsSettings.DrawDecalGizmos.value, false, drawDecalGizmos => { decalsSettings.DrawDecalGizmos.value = drawDecalGizmos; DecalsSystemManager.UpdateSettings(); });
                    }
                }

                GUILayout.Space(25);
                Label("UI", "", true);
                GUILayout.Space(1);
                Selection("Language", LocalizationManager.CurrentLanguage, language => LocalizationManager.CurrentLanguage = language);
                Slider("Font Size", renderingSettings.FontSize, 12, 24, size => renderingSettings.FontSize = size);
                Slider("Window Width", Inspector.Width, 400, Screen.width / 2, size => Inspector.Width = size);
                Slider("Window Height", Inspector.Height, 400, Screen.height, size => Inspector.Height = size);
                GUILayout.Space(25);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}
