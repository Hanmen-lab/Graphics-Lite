﻿using Graphics.GTAO;
using Graphics.VAO;
using Graphics.Settings;
using Graphics.GlobalFog;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Graphics.Inspector.Util;
using Graphics.AmplifyOcclusion;


namespace Graphics.Inspector
{
    internal static class PostProcessingInspector
    {
        private static Vector2 postScrollView;
        private static FocusPuller focusPuller;
        private static bool _autoFocusEnabled;
        private static float _autoFocusSpeed;
        private static bool _flareCustomize;

        internal static bool AutoFocusEnabled
        {
            get => null != focusPuller && focusPuller.enabled;
            set
            {
                _autoFocusEnabled = value;
                if (null != focusPuller)
                    focusPuller.enabled = true;
            }
        }

        internal static float GetAutoFocusSpeedFromGameSession()
        {
            return focusPuller != null ? focusPuller.Speed : _autoFocusSpeed;
        }

        internal static void SetAutoFocusSpeedToGameSession(float speedReadFromFile)
        {
            _autoFocusSpeed = speedReadFromFile;
            if (focusPuller != null)
            {
                focusPuller.Speed = speedReadFromFile;
            }
        }

        internal static void PlanarOn()
        {

            var reflection = UnityEngine.Object.FindObjectOfType<LuxWater.LuxWater_PlanarReflection>();
            if (null == reflection)
                return;

            reflection.enabled = true;
        }

        internal static void Draw(LightManager lightManager, PostProcessingSettings settings, GlobalSettings renderSettings, PostProcessingManager postprocessingManager, bool showAdvanced)
        {


            GUIStyle EmptyBox = new GUIStyle(GUI.skin.box);
            EmptyBox.padding = new RectOffset(20, 0, 3, 13);
            EmptyBox.normal.background = null;

            GUIStyle SmallBox = new GUIStyle(GUI.skin.box);
            SmallBox.normal.background = null;
            SmallBox.fixedWidth = 60;

            GUIStyle TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2.9f), Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f));

            GUIStyle SmallTab = new GUIStyle(GUIStyles.tabcontent);
            SmallTab.padding = new RectOffset(Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f));
            SmallTab.margin = new RectOffset(0, 0, Mathf.RoundToInt(renderSettings.FontSize * 0.3f), Mathf.RoundToInt(renderSettings.FontSize * 0.3f));

            GUILayout.BeginVertical(TabContent);
            {
                GUILayout.Space(10);
                Label("POST PROCESSING", "", true);
                GUILayout.Space(1);
                if (showAdvanced)
                {
                    Label("Volume blending", "", true);
                    GUILayout.Space(1);
                    string trigger = null != settings && null != settings.VolumeTriggerSetting ? settings.VolumeTriggerSetting.name : "";
                    Label("Trigger", trigger);
                    Label("Layer", LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(settings.VolumeLayerSetting.value, 2))));
                    GUILayout.Space(10);
                }
                string volumeLabel = "Post Process Volume";
                if (showAdvanced)
                {
                    volumeLabel = "Post Process Volumes";
                }
                Label(volumeLabel, "", true);
                GUILayout.Space(10);

                PostProcessVolume volume = settings.Volume;
                GUILayout.Space(10);
                Slider("Weight", volume.weight, 0f, 1f, "N1", weight => volume.weight = weight);

            }
            GUILayout.EndVertical();
            postScrollView = GUILayout.BeginScrollView(postScrollView);
            GUILayout.BeginVertical(SmallTab);
            Selection2("AMBIENT OCCLUSION", settings.AOList, aol => settings.AOList = aol);
            if (PostProcessingSettings.AmbientOcclusionList.Legacy == settings.AOList)
            {
                if (settings.ambientOcclusionLayer != null)
                {
                    GUILayout.Space(30);
                    Toggle("Enable", settings.ambientOcclusionLayer.enabled.value, true, enabled => settings.ambientOcclusionLayer.active = settings.ambientOcclusionLayer.enabled.value = enabled);
                    if (settings.ambientOcclusionLayer.enabled.value)
                    {

                        GUILayout.Space(30);
                        Selection("Mode", settings.ambientOcclusionLayer.mode.value, mode => settings.ambientOcclusionLayer.mode.value = mode);
                        Slider("Intensity", settings.ambientOcclusionLayer.intensity.value, 0f, 4f, "N2",
                            intensity => settings.ambientOcclusionLayer.intensity.value = intensity, settings.ambientOcclusionLayer.intensity.overrideState,
                            overrideState => settings.ambientOcclusionLayer.intensity.overrideState = overrideState);

                        if (AmbientOcclusionMode.MultiScaleVolumetricObscurance == settings.ambientOcclusionLayer.mode.value)
                        {
                            Slider("Thickness Modifier", settings.ambientOcclusionLayer.thicknessModifier.value, 1f, 10f, "N2",
                                thickness => settings.ambientOcclusionLayer.thicknessModifier.value = thickness, settings.ambientOcclusionLayer.thicknessModifier.overrideState,
                                overrideState => settings.ambientOcclusionLayer.thicknessModifier.overrideState = overrideState);
                        }
                        else if (AmbientOcclusionMode.ScalableAmbientObscurance == settings.ambientOcclusionLayer.mode.value)
                        {
                            Slider("Radius", settings.ambientOcclusionLayer.radius.value, 1f, 10f, "N2",
                                radius => settings.ambientOcclusionLayer.radius.value = radius, settings.ambientOcclusionLayer.radius.overrideState,
                                overrideState => settings.ambientOcclusionLayer.radius.overrideState = overrideState);
                        }

                        SliderColor("Colour", settings.ambientOcclusionLayer.color.value,
                            colour => settings.ambientOcclusionLayer.color.value = colour, false, settings.ambientOcclusionLayer.color.overrideState,
                            overrideState => settings.ambientOcclusionLayer.color.overrideState = overrideState);
                        Toggle("Ambient Only", settings.ambientOcclusionLayer.ambientOnly.value, false, ambient => settings.ambientOcclusionLayer.ambientOnly.value = ambient);
                    }

                }
            }
            if (PostProcessingSettings.AmbientOcclusionList.VAO == settings.AOList)
            {
                if (VAOManager.settings != null)
                {
                    VAOSettings vaoSettings = VAOManager.settings;
                    GUILayout.Space(30);

                    Toggle("Enable", vaoSettings.Enabled, true, enabled => { vaoSettings.Enabled = enabled; VAOManager.UpdateSettings(); });
                    if (vaoSettings.Enabled)
                    {
                        GUILayout.Space(30);
                        Label("Basic Settings:", "", true);
                        Slider("Radius", vaoSettings.Radius.value, 0f, 0.5f, "N2", radius => { vaoSettings.Radius.value = radius; VAOManager.UpdateSettings(); }, vaoSettings.Radius.overrideState, overrideState => { vaoSettings.Radius.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        Slider("Power", vaoSettings.Power.value, 0f, 2f, "N2", power => { vaoSettings.Power.value = power; VAOManager.UpdateSettings(); }, vaoSettings.Power.overrideState, overrideState => { vaoSettings.Power.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        Slider("Presence", vaoSettings.Presence.value, 0f, 1f, "N2", presence => { vaoSettings.Presence.value = presence; VAOManager.UpdateSettings(); }, vaoSettings.Presence.overrideState, overrideState => { vaoSettings.Presence.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        Slider("Detail", vaoSettings.DetailAmountVAO.value, 0f, 1f, "N2", detail => { vaoSettings.DetailAmountVAO.value = detail; VAOManager.UpdateSettings(); }, vaoSettings.DetailAmountVAO.overrideState, overrideState => { vaoSettings.DetailAmountVAO.overrideState = overrideState; VAOManager.UpdateSettings(); });

                        Selection("Quality", vaoSettings.DetailQuality, quality => { vaoSettings.DetailQuality = quality; VAOManager.UpdateSettings(); });
                        Selection("Algorithm", vaoSettings.Algorithm, algorithm => { vaoSettings.Algorithm = algorithm; VAOManager.UpdateSettings(); });

                        if (VAOEffectCommandBuffer.AlgorithmType.StandardVAO == vaoSettings.Algorithm)
                        {
                            Slider("Thickness", vaoSettings.Thickness.value, 0f, 1.0f, "N2", thickness => { vaoSettings.Thickness.value = thickness; VAOManager.UpdateSettings(); }, vaoSettings.Thickness.overrideState, overrideState => { vaoSettings.Thickness.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        }
                        else if (VAOEffectCommandBuffer.AlgorithmType.RaycastAO == vaoSettings.Algorithm)
                        {

                            Slider("Bias", vaoSettings.SSAOBias.value, 0f, 0.1f, "N2", bias => { vaoSettings.SSAOBias.value = bias; VAOManager.UpdateSettings(); }, vaoSettings.SSAOBias.overrideState, overrideState => { vaoSettings.SSAOBias.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        }

                        Slider("BordersAO", vaoSettings.BordersIntensity.value, 0f, 1f, "N2", borders => { vaoSettings.BordersIntensity.value = borders; VAOManager.UpdateSettings(); }, vaoSettings.BordersIntensity.overrideState, overrideState => { vaoSettings.BordersIntensity.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        Label("", "", true);
                        Toggle("Limit Max Radius", vaoSettings.MaxRadiusEnabled.value, true, limitmaxradius => { vaoSettings.MaxRadiusEnabled.value = limitmaxradius; VAOManager.UpdateSettings(); });

                        if (vaoSettings.MaxRadiusEnabled.value)
                        {
                            Slider("MaxRadius", vaoSettings.MaxRadius.value, 0f, 3f, "N2", maxradius => { vaoSettings.MaxRadius.value = maxradius; VAOManager.UpdateSettings(); }, vaoSettings.MaxRadius.overrideState, overrideState => { vaoSettings.MaxRadius.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        }

                        Selection("Distance Falloff", vaoSettings.DistanceFalloffMode, distancefalloff => { vaoSettings.DistanceFalloffMode = distancefalloff; VAOManager.UpdateSettings(); });
                        if (vaoSettings.DistanceFalloffMode == VAOEffectCommandBuffer.DistanceFalloffModeType.Absolute)
                        {
                            Slider("Distance Falloff Start Absolute", vaoSettings.DistanceFalloffStartAbsolute.value, 50f, 5000f, "N0", distanceFalloffStartAbsolute => { vaoSettings.DistanceFalloffStartAbsolute.value = distanceFalloffStartAbsolute; VAOManager.UpdateSettings(); }, vaoSettings.DistanceFalloffStartAbsolute.overrideState, overrideState => { vaoSettings.DistanceFalloffStartAbsolute.overrideState = overrideState; VAOManager.UpdateSettings(); }); ;
                            Slider("Distance Falloff Speed Absolute", vaoSettings.DistanceFalloffSpeedAbsolute.value, 15f, 300f, "N0", distanceFalloffSpeedAbsolute => { vaoSettings.DistanceFalloffSpeedAbsolute.value = distanceFalloffSpeedAbsolute; VAOManager.UpdateSettings(); }, vaoSettings.DistanceFalloffSpeedAbsolute.overrideState, overrideState => { vaoSettings.DistanceFalloffSpeedAbsolute.overrideState = overrideState; VAOManager.UpdateSettings(); }); ;
                        }
                        else if (vaoSettings.DistanceFalloffMode == VAOEffectCommandBuffer.DistanceFalloffModeType.Relative)
                        {
                            Slider("Distance Falloff Start Relative", vaoSettings.DistanceFalloffStartRelative.value, 0.01f, 1.0f, "N2", distanceFalloffStartRelative => { vaoSettings.DistanceFalloffStartRelative.value = distanceFalloffStartRelative; VAOManager.UpdateSettings(); }, vaoSettings.DistanceFalloffStartRelative.overrideState, overrideState => { vaoSettings.DistanceFalloffStartRelative.overrideState = overrideState; VAOManager.UpdateSettings(); }); ;
                            Slider("Distance Falloff Speed Relative", vaoSettings.DistanceFalloffSpeedRelative.value, 0.01f, 1.0f, "N2", distanceFalloffSpeedRelative => { vaoSettings.DistanceFalloffSpeedRelative.value = distanceFalloffSpeedRelative; VAOManager.UpdateSettings(); }, vaoSettings.DistanceFalloffSpeedRelative.overrideState, overrideState => { vaoSettings.DistanceFalloffSpeedRelative.overrideState = overrideState; VAOManager.UpdateSettings(); }); ;
                        }

                        Label("", "", true);
                        Label("Coloring Settings:", "", true);
                        Selection("Effect Mode", vaoSettings.Mode, effectmode => { vaoSettings.Mode = effectmode; VAOManager.UpdateSettings(); });

                        if (VAOEffectCommandBuffer.EffectMode.ColorTint == vaoSettings.Mode)
                        {
                            Label("", "", true);
                            SliderColor("Color Tint", vaoSettings.ColorTint, colortint => { vaoSettings.ColorTint = colortint; VAOManager.UpdateSettings(); });
                        }
                        else if (VAOEffectCommandBuffer.EffectMode.ColorBleed == vaoSettings.Mode)
                        {
                            Label("Color Bleed Settings:", "", true);
                            Slider("Power", vaoSettings.ColorBleedPower.value, 0f, 10f, "N2", colorbleedpower => { vaoSettings.ColorBleedPower.value = colorbleedpower; VAOManager.UpdateSettings(); }, vaoSettings.ColorBleedPower.overrideState, overrideState => { vaoSettings.ColorBleedPower.overrideState = overrideState; VAOManager.UpdateSettings(); });
                            Slider("Presence", vaoSettings.ColorBleedPresence.value, 0f, 10f, "N2", colorbleedpresence => { vaoSettings.ColorBleedPresence.value = colorbleedpresence; VAOManager.UpdateSettings(); }, vaoSettings.ColorBleedPresence.overrideState, overrideState => { vaoSettings.ColorBleedPresence.overrideState = overrideState; VAOManager.UpdateSettings(); });
                            Selection("Texture Format", vaoSettings.IntermediateScreenTextureFormat, intermediatetextureformat => { vaoSettings.IntermediateScreenTextureFormat = intermediatetextureformat; VAOManager.UpdateSettings(); });
                            Toggle("Same Color Hue Attenuation", vaoSettings.ColorbleedHueSuppresionEnabled.value, true, huesuppresion => { vaoSettings.ColorbleedHueSuppresionEnabled.value = huesuppresion; VAOManager.UpdateSettings(); });

                            if (vaoSettings.ColorbleedHueSuppresionEnabled.value)
                            {
                                Label("Hue Filter", "", true);
                                Slider("Tolerance", vaoSettings.ColorBleedHueSuppresionThreshold.value, 0f, 50f, "N2", colorbleedhuesuppresionthreshold => { vaoSettings.ColorBleedHueSuppresionThreshold.value = colorbleedhuesuppresionthreshold; VAOManager.UpdateSettings(); }, vaoSettings.ColorBleedHueSuppresionThreshold.overrideState, overrideState => { vaoSettings.ColorBleedHueSuppresionThreshold.overrideState = overrideState; VAOManager.UpdateSettings(); });
                                Slider("Softness", vaoSettings.ColorBleedHueSuppresionWidth.value, 0f, 10f, "N2", colorbleedhuesuppresionwidth => { vaoSettings.ColorBleedHueSuppresionWidth.value = colorbleedhuesuppresionwidth; VAOManager.UpdateSettings(); }, vaoSettings.ColorBleedHueSuppresionWidth.overrideState, overrideState => { vaoSettings.ColorBleedHueSuppresionWidth.overrideState = overrideState; VAOManager.UpdateSettings(); });
                                Label("Saturation Filter", "", true);
                                Slider("Threshold", vaoSettings.ColorBleedHueSuppresionSaturationThreshold.value, 0f, 1f, "N2", colorbleedhuesuppresionthreshold => { vaoSettings.ColorBleedHueSuppresionSaturationThreshold.value = colorbleedhuesuppresionthreshold; VAOManager.UpdateSettings(); }, vaoSettings.ColorBleedHueSuppresionSaturationThreshold.overrideState, overrideState => { vaoSettings.ColorBleedHueSuppresionSaturationThreshold.overrideState = overrideState; VAOManager.UpdateSettings(); });
                                Slider("Softness", vaoSettings.ColorBleedHueSuppresionSaturationWidth.value, 0f, 1f, "N2", colorbleedhuesuppresionsaturationwidth => { vaoSettings.ColorBleedHueSuppresionSaturationWidth.value = colorbleedhuesuppresionsaturationwidth; VAOManager.UpdateSettings(); }, vaoSettings.ColorBleedHueSuppresionSaturationWidth.overrideState, overrideState => { vaoSettings.ColorBleedHueSuppresionSaturationWidth.overrideState = overrideState; VAOManager.UpdateSettings(); });
                                Slider("Brightness", vaoSettings.ColorBleedHueSuppresionBrightness.value, 0f, 1f, "N2", colorbleedhuesuppresionbrightness => { vaoSettings.ColorBleedHueSuppresionBrightness.value = colorbleedhuesuppresionbrightness; VAOManager.UpdateSettings(); }, vaoSettings.ColorBleedHueSuppresionBrightness.overrideState, overrideState => { vaoSettings.ColorBleedHueSuppresionBrightness.overrideState = overrideState; VAOManager.UpdateSettings(); });
                            }
                            //Causing plugin crush. Actual veriable is Int, Probably need conversion to enum.
                            //Selection("Quality", vaoSettings.ColorBleedQuality, colorbleedquality => vaoSettings.ColorBleedQuality = colorbleedquality);
                            Selection("Dampen Self Bleeding", vaoSettings.ColorBleedSelfOcclusionFixLevel, colorbleedocclusionfixlevel => { vaoSettings.ColorBleedSelfOcclusionFixLevel = colorbleedocclusionfixlevel; VAOManager.UpdateSettings(); });
                            Toggle("Skip Backfaces", vaoSettings.GiBackfaces.value, true, gibackfaces => { vaoSettings.GiBackfaces.value = gibackfaces; VAOManager.UpdateSettings(); });
                        }

                        Label("", "", true);
                        Label("Performance Settings:", "", true);
                        Toggle("Temporal Filtering", vaoSettings.EnableTemporalFiltering.value, true, temporalfiltering => { vaoSettings.EnableTemporalFiltering.value = temporalfiltering; VAOManager.UpdateSettings(); });
                        Selection("Adaptive Sampling", vaoSettings.AdaptiveType, adaptivetype => { vaoSettings.AdaptiveType = adaptivetype; VAOManager.UpdateSettings(); });

                        if (vaoSettings.EnableTemporalFiltering.value)
                        {
                        }
                        else
                        {
                            Selection("Downsampled Pre-Pass", vaoSettings.CullingPrepassMode, cullingprepass => { vaoSettings.CullingPrepassMode = cullingprepass; VAOManager.UpdateSettings(); });
                        }

                        // Causing plugin crush!
                        // Selection("Downsampling", vaoSettings.Downsampling, downsampling => vaoSettings.Downsampling = downsampling);

                        Selection("Hierarchical Buffers", vaoSettings.HierarchicalBufferState, hierarchicalbuffers => { vaoSettings.HierarchicalBufferState = hierarchicalbuffers; VAOManager.UpdateSettings(); });

                        if (vaoSettings.EnableTemporalFiltering.value)
                        {
                        }
                        else
                        {
                            Selection("Detail Quality", vaoSettings.DetailQuality, detailquality => { vaoSettings.DetailQuality = detailquality; VAOManager.UpdateSettings(); });
                        }

                        Label("", "", true);
                        Label("Rendering Settings:", "", true);
                        Toggle("Command Buffer", vaoSettings.CommandBufferEnabled.value, true, commandbuffer => { vaoSettings.CommandBufferEnabled.value = commandbuffer; VAOManager.UpdateSettings(); });
                        Selection("Normal Source", vaoSettings.NormalsSource, normalsource => { vaoSettings.NormalsSource = normalsource; VAOManager.UpdateSettings(); });

                        if (Graphics.Instance.CameraSettings.RenderingPath != CameraSettings.AIRenderingPath.Deferred)
                        {
                            Label("Rendering Mode: FORWARD", "", true);
                            Toggle("High Precision Depth Buffer", vaoSettings.UsePreciseDepthBuffer.value, true, useprecisiondepthbuffer => { vaoSettings.UsePreciseDepthBuffer.value = useprecisiondepthbuffer; VAOManager.UpdateSettings(); });
                        }
                        else
                        {
                            Label("", "", true);
                            Label("Rendering Mode: DEFERRED", "", true);
                            Selection("Cmd Buffer Integration", vaoSettings.VaoCameraEvent, vaocameraevent => { vaoSettings.VaoCameraEvent = vaocameraevent; VAOManager.UpdateSettings(); });
                            Toggle("G-Buffer Depth & Normals", vaoSettings.UseGBuffer.value, true, usegbuffer => { vaoSettings.UseGBuffer.value = usegbuffer; VAOManager.UpdateSettings(); });
                        }

                        Selection("Far Plane Source", vaoSettings.FarPlaneSource, farplanesource => { vaoSettings.FarPlaneSource = farplanesource; VAOManager.UpdateSettings(); });

                        Label("", "", true);
                        Toggle("Luma Sensitivity", vaoSettings.IsLumaSensitive.value, true, lumasensitive => { vaoSettings.IsLumaSensitive.value = lumasensitive; VAOManager.UpdateSettings(); });

                        if (vaoSettings.IsLumaSensitive.value)
                        {
                            Selection("Luminance Mode", vaoSettings.LuminanceMode, luminancemode => { vaoSettings.LuminanceMode = luminancemode; VAOManager.UpdateSettings(); });
                            Slider("Threshold (HDR)", vaoSettings.LumaThreshold.value, 0f, 10f, "N2", lumathreshold => { vaoSettings.LumaThreshold.value = lumathreshold; VAOManager.UpdateSettings(); }, vaoSettings.LumaThreshold.overrideState, overrideState => { vaoSettings.LumaThreshold.overrideState = overrideState; VAOManager.UpdateSettings(); });
                            Slider("Falloff Width", vaoSettings.LumaKneeWidth.value, 0f, 10f, "N2", lumakneewidth => { vaoSettings.LumaKneeWidth.value = lumakneewidth; VAOManager.UpdateSettings(); }, vaoSettings.LumaKneeWidth.overrideState, overrideState => { vaoSettings.LumaKneeWidth.overrideState = overrideState; VAOManager.UpdateSettings(); });
                            Slider("Falloff Softness", vaoSettings.LumaKneeLinearity.value, 1f, 10f, "N2", lumakneelinearity => { vaoSettings.LumaKneeLinearity.value = lumakneelinearity; VAOManager.UpdateSettings(); }, vaoSettings.LumaKneeLinearity.overrideState, overrideState => { vaoSettings.LumaKneeLinearity.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        }
                        Label("", "", true);
                        Selection("Blur Quality", vaoSettings.BlurQuality, blurQuality => { vaoSettings.BlurQuality = blurQuality; VAOManager.UpdateSettings(); });
                        Selection("Blur Mode", vaoSettings.BlurMode, blurMode => { vaoSettings.BlurMode = blurMode; VAOManager.UpdateSettings(); });
                        if (VAOEffectCommandBuffer.BlurModeType.Enhanced == vaoSettings.BlurMode)
                        {
                            Label("Enhanced Blur Settings:", "", true);
                            Slider("Blur Size", vaoSettings.EnhancedBlurSize.value, 3, 17, "N2", enhancedblursize => { vaoSettings.EnhancedBlurSize.value = (int)enhancedblursize; VAOManager.UpdateSettings(); }, vaoSettings.EnhancedBlurSize.overrideState, overrideState => { vaoSettings.EnhancedBlurSize.overrideState = overrideState; VAOManager.UpdateSettings(); });
                            Slider("Blur Sharpness", vaoSettings.EnhancedBlurDeviation.value, 0.01f, 3.0f, "N2", enhancedblurdeviation => { vaoSettings.EnhancedBlurDeviation.value = enhancedblurdeviation; VAOManager.UpdateSettings(); }, vaoSettings.EnhancedBlurDeviation.overrideState, overrideState => { vaoSettings.EnhancedBlurDeviation.overrideState = overrideState; VAOManager.UpdateSettings(); });
                        }
                        Label("", "", true);
                        Toggle("Debug Mode:", vaoSettings.OutputAOOnly.value, true, outputaoonly => { vaoSettings.OutputAOOnly.value = outputaoonly; VAOManager.UpdateSettings(); });
                        Label("", "", true);
                    }
                }
            }
            if (PostProcessingSettings.AmbientOcclusionList.GTAO == settings.AOList)
            {
                if (GTAOManager.settings != null)
                {
                    GTAOSettings gtaoSettings = GTAOManager.settings;
                    GUILayout.Space(30);
                    if (Graphics.Instance.CameraSettings.RenderingPath != CameraSettings.AIRenderingPath.Deferred)
                    {
                        if (gtaoSettings.Enabled)
                        {
                            gtaoSettings.Enabled = false;
                            GTAOManager.UpdateSettings();
                        }
                        Label("GTAO - Available in Deferred Rendering Mode Only", "", false);
                    }
                    else
                    {
                        Toggle("Enable", gtaoSettings.Enabled, true, enabled => { gtaoSettings.Enabled = enabled; GTAOManager.UpdateSettings(); });

                        if (gtaoSettings.Enabled)
                        {
                            GUILayout.Space(30);
                            Slider("Intensity", gtaoSettings.Intensity.value, 0f, 1f, "N2", intensity => { gtaoSettings.Intensity.value = intensity; GTAOManager.UpdateSettings(); }, gtaoSettings.Intensity.overrideState, overrideState => { gtaoSettings.Intensity.overrideState = overrideState; GTAOManager.UpdateSettings(); });
                            Slider("Power", gtaoSettings.Power.value, 1f, 8f, "N2", power => { gtaoSettings.Power.value = power; GTAOManager.UpdateSettings(); }, gtaoSettings.Power.overrideState, overrideState => { gtaoSettings.Power.overrideState = overrideState; GTAOManager.UpdateSettings(); });
                            Slider("Radius", gtaoSettings.Radius.value, 1f, 5f, "N2", radius => { gtaoSettings.Radius.value = radius; GTAOManager.UpdateSettings(); }, gtaoSettings.Radius.overrideState, overrideState => { gtaoSettings.Radius.overrideState = overrideState; GTAOManager.UpdateSettings(); });

                            Slider("Sharpeness", gtaoSettings.Sharpeness.value, 0f, 1f, "N2", sharpeness => { gtaoSettings.Sharpeness.value = sharpeness; GTAOManager.UpdateSettings(); }, gtaoSettings.Sharpeness.overrideState, overrideState => { gtaoSettings.Sharpeness.overrideState = overrideState; GTAOManager.UpdateSettings(); });
                            Slider("DirSampler", gtaoSettings.DirSampler.value, 1, 4, dirSampler => { gtaoSettings.DirSampler.value = dirSampler; GTAOManager.UpdateSettings(); }, gtaoSettings.DirSampler.overrideState, overrideState => { gtaoSettings.DirSampler.overrideState = overrideState; GTAOManager.UpdateSettings(); });
                            Slider("SliceSampler", gtaoSettings.SliceSampler.value, 1, 8, sliceSampler => { gtaoSettings.SliceSampler.value = sliceSampler; GTAOManager.UpdateSettings(); }, gtaoSettings.SliceSampler.overrideState, overrideState => { gtaoSettings.SliceSampler.overrideState = overrideState; GTAOManager.UpdateSettings(); });

                            Slider("TemporalScale", gtaoSettings.TemporalScale.value, 1f, 5f, "N2", temporalScale => { gtaoSettings.TemporalScale.value = temporalScale; GTAOManager.UpdateSettings(); }, gtaoSettings.TemporalScale.overrideState, overrideState => { gtaoSettings.TemporalScale.overrideState = overrideState; GTAOManager.UpdateSettings(); });
                            Slider("TemporalResponse", gtaoSettings.TemporalResponse.value, 0f, 1f, "N2", temporalResponse => { gtaoSettings.TemporalResponse.value = temporalResponse; GTAOManager.UpdateSettings(); }, gtaoSettings.TemporalResponse.overrideState, overrideState => { gtaoSettings.TemporalResponse.overrideState = overrideState; GTAOManager.UpdateSettings(); });
                            Toggle("MultiBounce", gtaoSettings.MultiBounce.value, true, multiBounce => { gtaoSettings.MultiBounce.value = multiBounce; GTAOManager.UpdateSettings(); });
                            GUILayout.Space(10);
                            Selection("Debug", gtaoSettings.Debug, debug => { gtaoSettings.Debug = debug; GTAOManager.UpdateSettings(); });
                        }
                    }
                }
            }
            if (PostProcessingSettings.AmbientOcclusionList.Amplify == settings.AOList)
            {
                if (AmplifyOccManager.settings != null)
                {
                    AmplifyOccSettings amplifyOccSettings = AmplifyOccManager.settings;
                    GUILayout.Space(30);
                    Toggle("Enable", amplifyOccSettings.Enabled, true, enabled => { amplifyOccSettings.Enabled = enabled; AmplifyOccManager.UpdateSettings(); });
                    if (amplifyOccSettings.Enabled)
                    {
                        GUILayout.Space(30);
                        Selection("Apply Method", amplifyOccSettings.ApplyMethod, apply => { amplifyOccSettings.ApplyMethod = apply; AmplifyOccManager.UpdateSettings(); });
                        Selection("PerPixel Normals", amplifyOccSettings.PerPixelNormals, normals => { amplifyOccSettings.PerPixelNormals = normals; AmplifyOccManager.UpdateSettings(); });
                        Selection("Sample Count", amplifyOccSettings.SampleCount, samples => { amplifyOccSettings.SampleCount = samples; AmplifyOccManager.UpdateSettings(); });

                        Slider("Bias", amplifyOccSettings.Bias.value, 0f, 0.99f, "N2", bias => { amplifyOccSettings.Bias.value = bias; AmplifyOccManager.UpdateSettings(); });
                        Slider("Intensity", amplifyOccSettings.Intensity.value, 0f, 4f, "N2", intensity => { amplifyOccSettings.Intensity.value = intensity; AmplifyOccManager.UpdateSettings(); });
                        SliderColor("Tint", amplifyOccSettings.Tint, colour => { amplifyOccSettings.Tint = colour; AmplifyOccManager.UpdateSettings(); });
                        Slider("Radius", amplifyOccSettings.Radius.value, 0f, 32f, "N2", radius => { amplifyOccSettings.Radius.value = radius; AmplifyOccManager.UpdateSettings(); });
                        Slider("Power Exponent", amplifyOccSettings.PowerExponent.value, 0f, 16f, "N2", powerExponent => { amplifyOccSettings.PowerExponent.value = powerExponent; AmplifyOccManager.UpdateSettings(); });
                        Slider("Thickness", amplifyOccSettings.Thickness.value, 0f, 1f, "N2", thickness => { amplifyOccSettings.Thickness.value = thickness; AmplifyOccManager.UpdateSettings(); });
                        GUILayout.Space(10);
                        Toggle("Cache Aware", amplifyOccSettings.CacheAware.value, false, aware => { amplifyOccSettings.CacheAware.value = aware; AmplifyOccManager.UpdateSettings(); });
                        Toggle("Downsample", amplifyOccSettings.Downsample.value, false, sample => { amplifyOccSettings.Downsample.value = sample; AmplifyOccManager.UpdateSettings(); });
                        GUILayout.Space(10);
                        Toggle("BILATERAL BLUR", amplifyOccSettings.BlurEnabled.value, true, blurenabled => amplifyOccSettings.BlurEnabled.value = blurenabled);
                        if (amplifyOccSettings.BlurEnabled.value)
                        {
                            GUILayout.Space(5);
                            Slider("Blur Sharpness", amplifyOccSettings.BlurSharpness.value, 0f, 20f, "N2", blurSharpness => { amplifyOccSettings.BlurSharpness.value = blurSharpness; AmplifyOccManager.UpdateSettings(); });
                            Slider("Blur Passes", amplifyOccSettings.BlurPasses.value, 1, 4, blurPasses => { amplifyOccSettings.BlurPasses.value = blurPasses; AmplifyOccManager.UpdateSettings(); });
                            Slider("Blur Radius", amplifyOccSettings.BlurRadius.value, 1, 4, blurRadius => { amplifyOccSettings.BlurRadius.value = blurRadius; AmplifyOccManager.UpdateSettings(); });
                        }
                        GUILayout.Space(10);
                        Toggle("TEMPORAL FILTER", amplifyOccSettings.FilterEnabled.value, true, enabled => { amplifyOccSettings.FilterEnabled.value = enabled; AmplifyOccManager.UpdateSettings(); });
                        if (amplifyOccSettings.FilterEnabled.value)
                        {
                            GUILayout.Space(5);
                            Slider("Filter Blending", amplifyOccSettings.FilterBlending.value, 0f, 1f, "N2", filterBlending => { amplifyOccSettings.FilterBlending.value = filterBlending; AmplifyOccManager.UpdateSettings(); });
                            Slider("Filter Response", amplifyOccSettings.FilterResponse.value, 0f, 1f, "N2", filterResponse => { amplifyOccSettings.FilterResponse.value = filterResponse; AmplifyOccManager.UpdateSettings(); });
                        }
                        GUILayout.Space(10);
                        Toggle("DISTANCE FADE", amplifyOccSettings.FadeEnabled.value, true, enabled => { amplifyOccSettings.FadeEnabled.value = enabled; AmplifyOccManager.UpdateSettings(); });
                        if (amplifyOccSettings.FadeEnabled.value)
                        {
                            GUILayout.Space(5);
                            Slider("Fade Length", amplifyOccSettings.FadeLength.value, 0f, 100f, "N2", fadeLength => { amplifyOccSettings.FadeLength.value = fadeLength; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade Start", amplifyOccSettings.FadeStart.value, 0f, 100f, "N2", fadeStart => { amplifyOccSettings.FadeStart.value = fadeStart; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade To Intensity", amplifyOccSettings.FadeToIntensity.value, 0f, 1f, "N2", fadeToIntensity => { amplifyOccSettings.FadeToIntensity.value = fadeToIntensity; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade To Power Exponent", amplifyOccSettings.FadeToPowerExponent.value, 0f, 16f, "N2", fadeToPowerExponent => { amplifyOccSettings.FadeToPowerExponent.value = fadeToPowerExponent; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade To Radius", amplifyOccSettings.FadeToRadius.value, 0f, 32f, "N2", fadeToRadius => { amplifyOccSettings.FadeToRadius.value = fadeToRadius; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade To Thickness", amplifyOccSettings.FadeToThickness.value, 0f, 1f, "N2", fadeToThickness => { amplifyOccSettings.FadeToThickness.value = fadeToThickness; AmplifyOccManager.UpdateSettings(); });
                            SliderColor("Fade To Tint", amplifyOccSettings.FadeToTint, colour => { amplifyOccSettings.FadeToTint = colour; AmplifyOccManager.UpdateSettings(); });
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            if (settings.bloomLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "BLOOM", settings.bloomLayer.enabled.value, true, enabled => settings.bloomLayer.active = settings.bloomLayer.enabled.value = enabled);
                if (settings.bloomLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Slider("Intensity", settings.bloomLayer.intensity.value, 0f, 10f, "N1", intensity => settings.bloomLayer.intensity.value = intensity,
                        settings.bloomLayer.intensity.overrideState, overrideState => settings.bloomLayer.intensity.overrideState = overrideState);
                    Slider("Threshold", settings.bloomLayer.threshold.value, 0f, 10f, "N1", threshold => settings.bloomLayer.threshold.value = threshold,
                        settings.bloomLayer.threshold.overrideState, overrideState => settings.bloomLayer.threshold.overrideState = overrideState);
                    Slider("SoftKnee", settings.bloomLayer.softKnee.value, 0f, 1f, "N1", softKnee => settings.bloomLayer.softKnee.value = softKnee,
                        settings.bloomLayer.softKnee.overrideState, overrideState => settings.bloomLayer.softKnee.overrideState = overrideState);
                    Slider("Clamp", settings.bloomLayer.clamp.value, 0, 65472, "N0", clamp => settings.bloomLayer.clamp.value = clamp,
                        settings.bloomLayer.clamp.overrideState, overrideState => settings.bloomLayer.clamp.overrideState = overrideState);
                    Slider("Diffusion", (int)settings.bloomLayer.diffusion.value, 1, 10, "N0", diffusion => settings.bloomLayer.diffusion.value = diffusion,
                        settings.bloomLayer.diffusion.overrideState, overrideState => settings.bloomLayer.diffusion.overrideState = overrideState);
                    Slider("AnamorphicRatio", settings.bloomLayer.anamorphicRatio.value, -1, 1, "N1", anamorphicRatio => settings.bloomLayer.anamorphicRatio.value = anamorphicRatio,
                        settings.bloomLayer.anamorphicRatio.overrideState, overrideState => settings.bloomLayer.anamorphicRatio.overrideState = overrideState);
                    GUILayout.Space(5);
                    SliderColor("Colour", settings.bloomLayer.color.value, colour => { settings.bloomLayer.color.value = colour; }, settings.bloomLayer.color.overrideState,
                        settings.bloomLayer.color.overrideState, overrideState => settings.bloomLayer.color.overrideState = overrideState);
                    GUILayout.Space(5);
                    Toggle("Fast Mode", settings.bloomLayer.fastMode.value, false, fastMode => settings.bloomLayer.fastMode.value = fastMode);
                    GUILayout.Space(5);
                    int lensDirtIndex = SelectionTexture("Lens Dirt", postprocessingManager.CurrentLensDirtTextureIndex, postprocessingManager.LensDirtPreviews, Inspector.Width / 105,
                        settings.bloomLayer.dirtTexture.overrideState, overrideState => settings.bloomLayer.dirtTexture.overrideState = overrideState, GUIStyles.Skin.box);
                    if (-1 != lensDirtIndex && lensDirtIndex != postprocessingManager.CurrentLensDirtTextureIndex)
                    {
                        postprocessingManager.LoadLensDirtTexture(lensDirtIndex, dirtTexture => settings.bloomLayer.dirtTexture.value = dirtTexture);
                    }
                    Text("Dirt Intensity", settings.bloomLayer.dirtIntensity.value, "N2", value => settings.bloomLayer.dirtIntensity.value = value,
                        settings.bloomLayer.dirtIntensity.overrideState, overrideState => settings.bloomLayer.dirtIntensity.overrideState = overrideState);
                }
                GUILayout.EndVertical();
            }

            if (settings.colorGradingLayer)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "COLOR GRADING", settings.colorGradingLayer.enabled.value, true, enabled =>
                {
                    settings.colorGradingLayer.active = settings.colorGradingLayer.enabled.value = enabled;
                });
                if (settings.colorGradingLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Selection("Mode", (PostProcessingSettings.GradingMode)settings.colorGradingLayer.gradingMode.value, mode => settings.colorGradingLayer.gradingMode.value = (UnityEngine.Rendering.PostProcessing.GradingMode)mode);
                    if (GradingMode.External != settings.colorGradingLayer.gradingMode.value)
                    {
                        settings.agxColorLayer.active = false; settings.agxColorLayer.enabled.Override(false);
                        if (GradingMode.LowDefinitionRange == settings.colorGradingLayer.gradingMode.value)
                        {
                            Selection("LUT", postprocessingManager.CurrentLUTName, postprocessingManager.LUTNames,
                                lut => { if (lut != postprocessingManager.CurrentLUTName) { settings.colorGradingLayer.ldrLut.value = postprocessingManager.LoadLUT(lut); } }, Inspector.Width / 200,
                                settings.colorGradingLayer.ldrLut.overrideState, overrideState => settings.colorGradingLayer.ldrLut.overrideState = overrideState);
                            Slider("LUT Blend", settings.colorGradingLayer.ldrLutContribution.value, 0, 1, "N3", ldrLutContribution => settings.colorGradingLayer.ldrLutContribution.value = ldrLutContribution,
                                settings.colorGradingLayer.ldrLutContribution.overrideState, overrideState => settings.colorGradingLayer.ldrLutContribution.overrideState = overrideState);
                        }
                        else
                        {
                            Selection("Tonemapping", settings.colorGradingLayer.tonemapper.value, mode => settings.colorGradingLayer.tonemapper.value = mode);
                        }
                        GUILayout.Space(30);
                        Label("WHITE BALANCE", "", true);
                        GUILayout.Space(10);
                        Slider("Temperature", settings.colorGradingLayer.temperature.value, -100, 100, "N1", temperature => settings.colorGradingLayer.temperature.value = temperature,
                            settings.colorGradingLayer.temperature.overrideState, overrideState => settings.colorGradingLayer.temperature.overrideState = overrideState);
                        Slider("Tint", settings.colorGradingLayer.tint.value, -100, 100, "N1", tint => settings.colorGradingLayer.tint.value = tint,
                            settings.colorGradingLayer.tint.overrideState, overrideState => settings.colorGradingLayer.tint.overrideState = overrideState);
                        GUILayout.Space(10);
                        Label("TONE", "", true);
                        GUILayout.Space(10);
                        if (GradingMode.HighDefinitionRange == settings.colorGradingLayer.gradingMode.value)
                        {
                            Slider("Post-exposure (EV)", settings.colorGradingLayer.postExposure.value, -3, 3, "N2", value => settings.colorGradingLayer.postExposure.value = value, settings.colorGradingLayer.postExposure.overrideState, overrideState => settings.colorGradingLayer.postExposure.overrideState = overrideState);
                        }
                        Slider("Hue Shift", settings.colorGradingLayer.hueShift.value, -180, 180, "N1", hueShift => settings.colorGradingLayer.hueShift.value = hueShift,
                            settings.colorGradingLayer.hueShift.overrideState, overrideState => settings.colorGradingLayer.hueShift.overrideState = overrideState);
                        Slider("Saturation", settings.colorGradingLayer.saturation.value, -100, 100, "N1", saturation => settings.colorGradingLayer.saturation.value = saturation,
                            settings.colorGradingLayer.saturation.overrideState, overrideState => settings.colorGradingLayer.saturation.overrideState = overrideState);
                        if (GradingMode.LowDefinitionRange == settings.colorGradingLayer.gradingMode.value)
                        {
                            Slider("Brightness", settings.colorGradingLayer.brightness.value, -100, 100, "N1", brightness => settings.colorGradingLayer.brightness.value = brightness,
                                settings.colorGradingLayer.brightness.overrideState, overrideState => settings.colorGradingLayer.brightness.overrideState = overrideState);
                        }
                        Slider("Contrast", settings.colorGradingLayer.contrast.value, -100, 100, "N1", contrast => settings.colorGradingLayer.contrast.value = contrast,
                            settings.colorGradingLayer.contrast.overrideState, overrideState => settings.colorGradingLayer.contrast.overrideState = overrideState);
                        SliderColor("Lift", settings.colorGradingLayer.lift.value, colour => settings.colorGradingLayer.lift.value = colour, false,
                            settings.colorGradingLayer.lift.overrideState, overrideState => settings.colorGradingLayer.lift.overrideState = overrideState, "Lift range", -1.5f, 3f);
                        SliderColor("Gamma", settings.colorGradingLayer.gamma.value, colour => settings.colorGradingLayer.gamma.value = colour, false,
                            settings.colorGradingLayer.gamma.overrideState, overrideSate => settings.colorGradingLayer.gamma.overrideState = overrideSate, "Gamma range", -1.5f, 3f);
                        SliderColor("Gain", settings.colorGradingLayer.gain.value, colour => settings.colorGradingLayer.gain.value = colour, false,
                            settings.colorGradingLayer.gain.overrideState, overrideSate => settings.colorGradingLayer.gain.overrideState = overrideSate, "Gain range", -1.5f, 3f);
                    }
                    else
                    {
                        settings.agxColorLayer.active = true; settings.agxColorLayer.enabled.Override(true);
                        Selection("Enable", postprocessingManager.Current3DLUTName, postprocessingManager.LUT3DNames,
                            lut3d => { if (lut3d != postprocessingManager.Current3DLUTName) { settings.colorGradingLayer.externalLut.value = postprocessingManager.Load3DLUT(lut3d); } }, 2,
                            settings.colorGradingLayer.externalLut.overrideState, overrideState => settings.colorGradingLayer.externalLut.overrideState = overrideState);
                        GUILayout.Space(30);
                        if (settings.agxColorLayer != null)
                        {                
                            if (settings.agxColorLayer.enabled.value)
                            {
                                GUILayout.Space(30);
                                Label("WHITE BALANCE", "", true);
                                GUILayout.Space(10);
                                Slider("Temperature", settings.agxColorLayer.temperature.value, -1f, 1f, "N2", temperature => settings.agxColorLayer.temperature.value = temperature,
                                    settings.agxColorLayer.temperature.overrideState, overrideState => settings.agxColorLayer.temperature.overrideState = overrideState);
                                Slider("Tint", settings.agxColorLayer.tint.value, -1f, 1f, "N2", tint => settings.agxColorLayer.tint.value = tint,
                                    settings.agxColorLayer.tint.overrideState, overrideState => settings.agxColorLayer.tint.overrideState = overrideState);
                                GUILayout.Space(10);
                                Label("TONE", "", true);
                                GUILayout.Space(10);
                                Slider("Saturation", settings.agxColorLayer.saturation.value, 0f, 2f, "N2", saturation => settings.agxColorLayer.saturation.value = saturation,
                                    settings.agxColorLayer.saturation.overrideState, overrideState => settings.agxColorLayer.saturation.overrideState = overrideState);
                                Slider("Brightness", settings.agxColorLayer.brightness.value, -0.9f, 1f, "N2", brightness => settings.agxColorLayer.brightness.value = brightness,
                                    settings.agxColorLayer.brightness.overrideState, overrideState => settings.agxColorLayer.brightness.overrideState = overrideState);
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
            }

            if (settings.autoExposureLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "AUTO EXPOSURE", settings.autoExposureLayer.enabled.value, true, enabled => settings.autoExposureLayer.active = settings.autoExposureLayer.enabled.value = enabled);
                if (settings.autoExposureLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Toggle("Histogram Filtering (%)", settings.autoExposureLayer.filtering.overrideState, false, overrideState => settings.autoExposureLayer.filtering.overrideState = overrideState);
                    Vector2 filteringRange = settings.autoExposureLayer.filtering.value;
                    Slider("Lower Bound", filteringRange.x, 1f, Math.Min(filteringRange.y, 99f), "N0", filtering => filteringRange.x = filtering, settings.autoExposureLayer.filtering.overrideState);
                    Slider("Upper Bound", filteringRange.y, Math.Max(filteringRange.x, 1f), 99f, "N0", filtering => filteringRange.y = filtering, settings.autoExposureLayer.filtering.overrideState);
                    settings.autoExposureLayer.filtering.value = filteringRange;
                    Slider("Min Luminance (EV)", settings.autoExposureLayer.minLuminance.value, -9f, 9f, "N0",
                        luminance => settings.autoExposureLayer.minLuminance.value = luminance, settings.autoExposureLayer.minLuminance.overrideState,
                        overrideState => settings.autoExposureLayer.minLuminance.overrideState = overrideState);
                    Slider("Max Luminance (EV)", settings.autoExposureLayer.maxLuminance.value, -9f, 9f, "N0",
                        luminance => settings.autoExposureLayer.maxLuminance.value = luminance, settings.autoExposureLayer.maxLuminance.overrideState,
                        overrideState => settings.autoExposureLayer.maxLuminance.overrideState = overrideState);
                    GUILayout.Space(5);
                    Toggle("Eye Adaptation", settings.autoExposureLayer.eyeAdaptation.overrideState, false, overrideState => settings.autoExposureLayer.eyeAdaptation.overrideState = overrideState);
                    Selection("Type", settings.autoExposureLayer.eyeAdaptation.value, type => settings.autoExposureLayer.eyeAdaptation.value = type, -1,
                        settings.autoExposureLayer.eyeAdaptation.overrideState);
                    Slider("Speed from light to dark", settings.autoExposureLayer.speedUp.value, 0f, 10f, "N1",
                        luminance => settings.autoExposureLayer.speedUp.value = luminance, settings.autoExposureLayer.speedUp.overrideState,
                        overrideState => settings.autoExposureLayer.speedUp.overrideState = overrideState);
                    Slider("Speed from dark to light", settings.autoExposureLayer.speedDown.value, 0f, 10f, "N1",
                        luminance => settings.autoExposureLayer.speedDown.value = luminance, settings.autoExposureLayer.speedDown.overrideState,
                        overrideState => settings.autoExposureLayer.speedDown.overrideState = overrideState);
                }
                GUILayout.EndVertical();
            }

            if (settings.chromaticAberrationLayer)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "CHROMATIC ABERRATION", settings.chromaticAberrationLayer.enabled.value, true, enabled => settings.chromaticAberrationLayer.active = settings.chromaticAberrationLayer.enabled.value = enabled);
                if (settings.chromaticAberrationLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Slider("Intensity", settings.chromaticAberrationLayer.intensity.value, 0f, 5f, "N3", intensity => settings.chromaticAberrationLayer.intensity.value = intensity,
                        settings.chromaticAberrationLayer.intensity.overrideState, overrideState => settings.chromaticAberrationLayer.intensity.overrideState = overrideState);
                    Toggle("Fast Mode", settings.chromaticAberrationLayer.fastMode.value, false, fastMode => settings.chromaticAberrationLayer.fastMode.value = fastMode);
                }
                GUILayout.EndVertical();
            }

            if (settings.depthOfFieldLayer)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "DEPTH OF FIELD", settings.depthOfFieldLayer.enabled.value, true, enabled => settings.depthOfFieldLayer.active = settings.depthOfFieldLayer.enabled.value = enabled);
                if (settings.depthOfFieldLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    if (null != Graphics.Instance.CameraSettings.MainCamera && null == focusPuller)
                    {
                        focusPuller = Graphics.Instance.CameraSettings.MainCamera.gameObject.GetOrAddComponent<FocusPuller>();
                        if (null != focusPuller)
                        {
                            focusPuller.enabled = _autoFocusEnabled;
                            focusPuller.Speed = _autoFocusSpeed;
                        }
                    }

                    if (null != focusPuller)
                    {
                        Toggle("Auto Focus", focusPuller.enabled, false, enabled => focusPuller.enabled = enabled);
                        Slider("Auto Focus Speed", focusPuller.Speed, FocusPuller.MinSpeed, FocusPuller.MaxSpeed, "N2", speed => focusPuller.Speed = speed, focusPuller.enabled);
                    }
                    Slider("Focal Distance", settings.depthOfFieldLayer.focusDistance.value, 0.1f, 1000f, "N2", focusDistance => settings.depthOfFieldLayer.focusDistance.value = focusDistance,
                        settings.depthOfFieldLayer.focusDistance.overrideState && !focusPuller.enabled, overrideState => settings.depthOfFieldLayer.focusDistance.overrideState = overrideState);
                    Slider("Aperture", settings.depthOfFieldLayer.aperture.value, 1f, 22f, "N1", aperture => settings.depthOfFieldLayer.aperture.value = aperture,
                        settings.depthOfFieldLayer.aperture.overrideState, overrideState => settings.depthOfFieldLayer.aperture.overrideState = overrideState);
                    Slider("Focal Length", settings.depthOfFieldLayer.focalLength.value, 10f, 600f, "N0", focalLength => settings.depthOfFieldLayer.focalLength.value = focalLength,
                        settings.depthOfFieldLayer.focalLength.overrideState, overrideState => settings.depthOfFieldLayer.focalLength.overrideState = overrideState);
                    Selection("Max Blur Size", settings.depthOfFieldLayer.kernelSize.value, kernelSize => settings.depthOfFieldLayer.kernelSize.value = kernelSize, -1,
                        settings.depthOfFieldLayer.kernelSize.overrideState, overrideState => settings.depthOfFieldLayer.kernelSize.overrideState = overrideState);

                    if (showAdvanced)
                    {
                        GUI.enabled = false;
                        Label("Max Distance", focusPuller.MaxDistance.ToString());
                        Dimension("Target Position", focusPuller.TargetPosition);
                        GUI.enabled = true;
                    }
                }
                GUILayout.EndVertical();
            }

            //if (settings.beautifyDoFLayer != null)
            //{
            //    GUILayout.BeginVertical(SmallTab);
            //    Switch(renderSettings.FontSize, "BEAUTIFY DEPTH OF FIELD", settings.beautifyDoFLayer.enabled.value, true, enabled => settings.beautifyDoFLayer.active = settings.beautifyDoFLayer.enabled.value = enabled);
            //    if (settings.beautifyDoFLayer.enabled.value)
            //    {
            //        GUILayout.Space(30);

            //        Toggle("Debug", settings.beautifyDoFLayer.depthOfFieldDebug, false, debug => settings.beautifyDoFLayer.depthOfFieldDebug.Override(debug));

            //        Selection("Focus Mode", settings.beautifyDoFLayer.depthOfFieldFocusMode.value, focusmode => settings.beautifyDoFLayer.depthOfFieldFocusMode.value = focusmode, 3,
            //            settings.beautifyDoFLayer.depthOfFieldFocusMode.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldFocusMode.overrideState = overrideState);
            //        GUILayout.Space(5);
            //        if (BeautifyDoFFocusMode.AutoFocus == settings.beautifyDoFLayer.depthOfFieldFocusMode.value)
            //        {
            //            Dimension("Auto Focus Point", settings.beautifyDoFLayer.depthofFieldAutofocusViewportPoint.value, pos => { settings.beautifyDoFLayer.depthofFieldAutofocusViewportPoint.value = pos; settings.beautifyDoFLayer.depthofFieldAutofocusViewportPoint.Override(pos); });
            //            GUILayout.Space(5);
            //            Slider("Autofocus Min Distance", settings.beautifyDoFLayer.depthOfFieldAutofocusMinDistance.value, 0, 1000f, "N0", minDistance => settings.beautifyDoFLayer.depthOfFieldAutofocusMinDistance.value = minDistance,
            //                settings.beautifyDoFLayer.depthOfFieldAutofocusMinDistance.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldAutofocusMinDistance.overrideState = overrideState);
            //            Slider("Autofocus Max Distance", settings.beautifyDoFLayer.depthOfFieldAutofocusMaxDistance.value, 0, 10000f, "N0", maxDistance => settings.beautifyDoFLayer.depthOfFieldAutofocusMaxDistance.value = maxDistance,
            //                settings.beautifyDoFLayer.depthOfFieldAutofocusMaxDistance.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldAutofocusMaxDistance.overrideState = overrideState);
            //            Slider("Focus Speed", settings.beautifyDoFLayer.depthOfFieldFocusSpeed.value, 0.001f, 5f, "N2", speed => settings.beautifyDoFLayer.depthOfFieldFocusSpeed.value = speed,
            //                settings.beautifyDoFLayer.depthOfFieldFocusSpeed.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldFocusSpeed.overrideState = overrideState);
            //            GUILayout.Space(10);
            //            SelectionMask("Autofocus Layer Mask", settings.beautifyDoFLayer.depthOfFieldAutofocusLayerMask, layerMask => settings.beautifyDoFLayer.depthOfFieldAutofocusLayerMask = layerMask);
            //        }

            //        GUILayout.Space(10);

            //        SelectionMask("Exclusion Layer Mask", settings.beautifyDoFLayer.depthOfFieldExclusionLayerMask, exclayerMask => settings.beautifyDoFLayer.depthOfFieldExclusionLayerMask = exclayerMask);

            //        GUILayout.Space(5);
            //        //Selection("Exclusion Layer Mask", settings.beautifyDoFLayer.depthOfFieldExclusionLayerMask.value, layerMask => settings.beautifyDoFLayer.depthOfFieldExclusionLayerMask.value = layerMask, 0,
            //        //    settings.beautifyDoFLayer.depthOfFieldExclusionLayerMask.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldExclusionLayerMask.overrideState = overrideState);
            //        Slider("Exclusion Downsampling", settings.beautifyDoFLayer.depthOfFieldExclusionLayerMaskDownsampling.value, 1, 4, downsampling => settings.beautifyDoFLayer.depthOfFieldExclusionLayerMaskDownsampling.value = downsampling,
            //            settings.beautifyDoFLayer.depthOfFieldExclusionLayerMaskDownsampling.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldExclusionLayerMaskDownsampling.overrideState = overrideState);
            //        Toggle("Transparency Support", settings.beautifyDoFLayer.depthOfFieldTransparencySupport, false, transparencySupport => settings.beautifyDoFLayer.depthOfFieldTransparencySupport.Override(transparencySupport));
            //        //Selection("Transparency Layer Mask", settings.beautifyDoFLayer.depthOfFieldTransparencyLayerMask.value, layerMask => settings.beautifyDoFLayer.depthOfFieldTransparencyLayerMask.value = layerMask, -1,
            //        //    settings.beautifyDoFLayer.depthOfFieldTransparencyLayerMask.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldTransparencyLayerMask.overrideState = overrideState);                    
            //        Slider("Transparency Downsampling", settings.beautifyDoFLayer.depthOfFieldTransparencySupportDownsampling.value, 1, 4, downsampling => settings.beautifyDoFLayer.depthOfFieldTransparencySupportDownsampling.value = downsampling,
            //            settings.beautifyDoFLayer.depthOfFieldTransparencySupportDownsampling.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldTransparencySupportDownsampling.overrideState = overrideState);
            //        Slider("Exclusion Bias", settings.beautifyDoFLayer.depthOfFieldExclusionBias.value, 0.9f, 1f, "N2", bias => settings.beautifyDoFLayer.depthOfFieldExclusionBias.value = bias,
            //            settings.beautifyDoFLayer.depthOfFieldExclusionBias.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldExclusionBias.overrideState = overrideState);
            //        Slider("Distance", settings.beautifyDoFLayer.depthOfFieldDistance.value, 1f, 100f, "N0", distance => settings.beautifyDoFLayer.depthOfFieldDistance.value = distance,
            //            settings.beautifyDoFLayer.depthOfFieldDistance.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldDistance.overrideState = overrideState);

            //        Slider("Downsampling", settings.beautifyDoFLayer.depthOfFieldDownsampling.value, 1, 5, downsampling => settings.beautifyDoFLayer.depthOfFieldDownsampling.value = downsampling,
            //            settings.beautifyDoFLayer.depthOfFieldDownsampling.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldDownsampling.overrideState = overrideState);
            //        Slider("Max Samples", settings.beautifyDoFLayer.depthOfFieldMaxSamples.value, 2, 16, samples => settings.beautifyDoFLayer.depthOfFieldMaxSamples.value = samples,
            //            settings.beautifyDoFLayer.depthOfFieldMaxSamples.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldMaxSamples.overrideState = overrideState);
            //        Slider("Focal Length", settings.beautifyDoFLayer.depthOfFieldFocalLength.value, 0.005f, 0.5f, "N3", focalLength => settings.beautifyDoFLayer.depthOfFieldFocalLength.value = focalLength,
            //            settings.beautifyDoFLayer.depthOfFieldFocalLength.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldFocalLength.overrideState = overrideState);
            //        Slider("Aperture", settings.beautifyDoFLayer.depthOfFieldAperture.value, 1f, 22f, "N1", aperture => settings.beautifyDoFLayer.depthOfFieldAperture.value = aperture,
            //            settings.beautifyDoFLayer.depthOfFieldAperture.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldAperture.overrideState = overrideState);
            //        Toggle("Foreground Blur", settings.beautifyDoFLayer.depthOfFieldForegroundBlur.value, false, foregroundBlur => settings.beautifyDoFLayer.depthOfFieldForegroundBlur.value = foregroundBlur);
            //        Toggle("Foreground Blur HQ", settings.beautifyDoFLayer.depthOfFieldForegroundBlurHQ.value, false, foregroundBlurHQ => settings.beautifyDoFLayer.depthOfFieldForegroundBlurHQ.value = foregroundBlurHQ);
            //        Slider("Foreground Distance", settings.beautifyDoFLayer.depthOfFieldForegroundDistance.value, 0, 1, "N2", distance => settings.beautifyDoFLayer.depthOfFieldForegroundDistance.value = distance,
            //            settings.beautifyDoFLayer.depthOfFieldForegroundDistance.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldForegroundDistance.overrideState = overrideState);
            //        Toggle("Bokeh", settings.beautifyDoFLayer.depthOfFieldBokeh, false, bokeh => settings.beautifyDoFLayer.depthOfFieldDebug.Override(bokeh));
            //        Slider("Bokeh Threshold", settings.beautifyDoFLayer.depthOfFieldBokehThreshold.value, 0.5f, 3f, "N1", threshold => settings.beautifyDoFLayer.depthOfFieldBokehThreshold.value = threshold,
            //            settings.beautifyDoFLayer.depthOfFieldBokehThreshold.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldBokehThreshold.overrideState = overrideState);
            //        Slider("Bokeh Intensity", settings.beautifyDoFLayer.depthOfFieldBokehIntensity.value, 0, 8f, "N1", intensity => settings.beautifyDoFLayer.depthOfFieldBokehIntensity.value = intensity,
            //            settings.beautifyDoFLayer.depthOfFieldBokehIntensity.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldBokehIntensity.overrideState = overrideState);
            //        Slider("Max Brightness", settings.beautifyDoFLayer.depthOfFieldMaxBrightness.value, 0, 1000f, "N0", brightness => settings.beautifyDoFLayer.depthOfFieldMaxBrightness.value = brightness,
            //            settings.beautifyDoFLayer.depthOfFieldMaxBrightness.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldMaxBrightness.overrideState = overrideState);
            //        Slider("Max Distance", settings.beautifyDoFLayer.depthOfFieldMaxDistance.value, 0, 1f, "N2", distance => settings.beautifyDoFLayer.depthOfFieldMaxDistance.value = distance,
            //            settings.beautifyDoFLayer.depthOfFieldMaxDistance.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldMaxDistance.overrideState = overrideState);

            //        Selection("Filter Mode", settings.beautifyDoFLayer.depthOfFieldFilterMode.value, filtermode => settings.beautifyDoFLayer.depthOfFieldFilterMode.value = filtermode, 3,
            //            settings.beautifyDoFLayer.depthOfFieldFilterMode.overrideState, overrideState => settings.beautifyDoFLayer.depthOfFieldFilterMode.overrideState = overrideState);
            //    }

            //    GUILayout.EndVertical();
            //}

            if (settings.grainLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "GRAIN", settings.grainLayer.enabled.value, true, enabled => settings.grainLayer.active = settings.grainLayer.enabled.value = enabled);
                if (settings.grainLayer.enabled.value)
                {
                    GUILayout.Space(10);
                    Toggle("Colored", settings.grainLayer.colored.overrideState, false, overrideState => settings.grainLayer.colored.overrideState = overrideState);
                    Slider("Intensity", settings.grainLayer.intensity.value, 0f, 20f, "N2", intensity => settings.grainLayer.intensity.value = intensity,
                        settings.grainLayer.intensity.overrideState, overrideState => settings.grainLayer.intensity.overrideState = overrideState);
                    Slider("Size", settings.grainLayer.size.value, 0f, 10f, "N0", focalLength => settings.grainLayer.size.value = focalLength,
                        settings.grainLayer.size.overrideState, overrideState => settings.grainLayer.size.overrideState = overrideState);
                    Slider("Luminance Contribution", settings.grainLayer.lumContrib.value, 0f, 22f, "N1", lumContrib => settings.grainLayer.lumContrib.value = lumContrib,
                        settings.grainLayer.lumContrib.overrideState, overrideState => settings.grainLayer.lumContrib.overrideState = overrideState);
                    LabelColorRed("Warning:", "Grain can greatly increase ghosting artifacts.");
                }
                GUILayout.EndVertical();
            }

            if (settings.screenSpaceReflectionsLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "SSR", settings.screenSpaceReflectionsLayer.enabled.value, true, enabled => settings.screenSpaceReflectionsLayer.active = settings.screenSpaceReflectionsLayer.enabled.value = enabled);
                if (settings.screenSpaceReflectionsLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Selection("Preset", settings.screenSpaceReflectionsLayer.preset.value, preset => settings.screenSpaceReflectionsLayer.preset.value = preset, 4,
                        settings.screenSpaceReflectionsLayer.preset.overrideState, overrideState => settings.screenSpaceReflectionsLayer.preset.overrideState = overrideState);

                    if (ScreenSpaceReflectionPreset.Custom == settings.screenSpaceReflectionsLayer.preset.value)
                    {
                        Slider("Maximum Iteration Count", settings.screenSpaceReflectionsLayer.maximumIterationCount.value, 0, 256, iteration => settings.screenSpaceReflectionsLayer.maximumIterationCount.value = iteration,
                            settings.screenSpaceReflectionsLayer.maximumIterationCount.overrideState, overrideState => settings.screenSpaceReflectionsLayer.maximumIterationCount.overrideState = overrideState);

                        Slider("Thickness", settings.screenSpaceReflectionsLayer.thickness.value, 1f, 64f, "N1", thickness => settings.screenSpaceReflectionsLayer.thickness.value = thickness,
                            settings.screenSpaceReflectionsLayer.thickness.overrideState, overrideState => settings.screenSpaceReflectionsLayer.thickness.overrideState = overrideState);

                        Selection("Resolution", settings.screenSpaceReflectionsLayer.resolution.value, resolution => settings.screenSpaceReflectionsLayer.resolution.value = resolution, -1,
                            settings.screenSpaceReflectionsLayer.resolution.overrideState, overrideState => settings.screenSpaceReflectionsLayer.resolution.overrideState = overrideState);
                    }

                    Text("Maximum March Distance", settings.screenSpaceReflectionsLayer.maximumMarchDistance.value, "N2", value => settings.screenSpaceReflectionsLayer.maximumMarchDistance.value = value,
                        settings.screenSpaceReflectionsLayer.maximumMarchDistance.overrideState, overrideState => settings.screenSpaceReflectionsLayer.maximumMarchDistance.overrideState = overrideState);
                    Slider("Distance Fade", settings.screenSpaceReflectionsLayer.distanceFade, 0f, 1f, "N3", fade => settings.screenSpaceReflectionsLayer.distanceFade.value = fade,
                        settings.screenSpaceReflectionsLayer.distanceFade.overrideState, overrideState => settings.screenSpaceReflectionsLayer.distanceFade.overrideState = overrideState);
                    Slider("Vignette", settings.screenSpaceReflectionsLayer.vignette.value, 0f, 1f, "N3", vignette => settings.screenSpaceReflectionsLayer.vignette.value = vignette,
                        settings.screenSpaceReflectionsLayer.vignette.overrideState, overrideState => settings.screenSpaceReflectionsLayer.vignette.overrideState = overrideState);
                }
                GUILayout.EndVertical();
            }

            //if (ShinySSRRManager.settings != null)
            //{
            //    ShinySSRRSettings ShinySSRRSettings = ShinySSRRManager.settings;
            //    //GUILayout.Space(30);
            //    GUILayout.BeginVertical(SmallTab);
            //    Switch(renderSettings.FontSize, "SHINY SSRR", ShinySSRRSettings.Enabled, true, enabled => { ShinySSRRSettings.Enabled = enabled; ShinySSRRManager.UpdateSettings(); });

            //    if (ShinySSRRSettings.Enabled)
            //    {
            //        GUILayout.Space(30);
            //        Label("GENERAL SETTINGS", "", true);
            //        Slider("Intensity", ShinySSRRSettings.reflectionsMultiplier.value, 0f, 4f, "N2", reflectionsmultiplier => { ShinySSRRSettings.reflectionsMultiplier.value = reflectionsmultiplier; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.reflectionsMultiplier.overrideState, overrideState => { ShinySSRRSettings.reflectionsMultiplier.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        //Toggle("Show In Scene View", ShinySSRRSettings.showInSceneView.value, true, showinsceneview => { ShinySSRRSettings.showInSceneView.value = showinsceneview; ShinySSRRManager.UpdateSettings(); });
            //        GUILayout.Space(30);
            //        Label("QUALITY SETTINGS", "", true);
            //        Slider("Sample Count", ShinySSRRSettings.sampleCount.value, 4, 128, samplecount => { ShinySSRRSettings.sampleCount.value = samplecount; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.sampleCount.overrideState, overrideState => { ShinySSRRSettings.sampleCount.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Max Ray Length", ShinySSRRSettings.maxRayLength.value, 1f, 64f, "N2", maxraylength => { ShinySSRRSettings.maxRayLength.value = maxraylength; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.maxRayLength.overrideState, overrideState => { ShinySSRRSettings.maxRayLength.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Binary Search Iterations", ShinySSRRSettings.binarySearchIterations.value, 1, 16, binarysearchiterations => { ShinySSRRSettings.binarySearchIterations.value = binarysearchiterations; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.binarySearchIterations.overrideState, overrideState => { ShinySSRRSettings.binarySearchIterations.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Toggle("Compute Back Faces", ShinySSRRSettings.computeBackFaces.value, false, computebackfaces => { ShinySSRRSettings.computeBackFaces.value = computebackfaces; ShinySSRRManager.UpdateSettings(); });
            //        if (ShinySSRRSettings.computeBackFaces.value != false)
            //        {
            //            Slider("Thickness Minimum", ShinySSRRSettings.thicknessMinimum.value, 0f, 1f, "N2", thicknessminimum => { ShinySSRRSettings.thicknessMinimum.value = thicknessminimum; ShinySSRRManager.UpdateSettings(); },
            //                ShinySSRRSettings.thicknessMinimum.overrideState, overrideState => { ShinySSRRSettings.thicknessMinimum.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        }
            //        Slider("Thickness", ShinySSRRSettings.thickness.value, 0f, 1f, "N2", thickness => { ShinySSRRSettings.thickness.value = thickness; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.thickness.overrideState, overrideState => { ShinySSRRSettings.thickness.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Toggle("Refine Thickness", ShinySSRRSettings.refineThickness.value, false, refinethickness => { ShinySSRRSettings.refineThickness.value = refinethickness; ShinySSRRManager.UpdateSettings(); });
            //        if (ShinySSRRSettings.refineThickness.value != false)
            //        {
            //            Slider("Thickness Fine", ShinySSRRSettings.thicknessFine.value, 0f, 1f, "N2", thicknessfine => { ShinySSRRSettings.thicknessFine.value = thicknessfine; ShinySSRRManager.UpdateSettings(); },
            //                ShinySSRRSettings.thicknessFine.overrideState, overrideState => { ShinySSRRSettings.thicknessFine.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        }
            //        Slider("Jitter", ShinySSRRSettings.jitter.value, 0f, 1f, "N2", jitter => { ShinySSRRSettings.jitter.value = jitter; ShinySSRRManager.UpdateSettings(); },
            //                                   ShinySSRRSettings.jitter.overrideState, overrideState => { ShinySSRRSettings.jitter.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Toggle("Animated Jitter", ShinySSRRSettings.animatedJitter.value, false, animatedjitter => { ShinySSRRSettings.animatedJitter.value = animatedjitter; ShinySSRRManager.UpdateSettings(); });
            //        Toggle("Temporal Filter", ShinySSRRSettings.temporalFilter.value, false, temporalfilter => { ShinySSRRSettings.temporalFilter.value = temporalfilter; ShinySSRRManager.UpdateSettings(); });
            //        if (ShinySSRRSettings.temporalFilter.value != false)
            //        {
            //            Slider("Temporal Filter Response Speed", ShinySSRRSettings.temporalFilterResponseSpeed.value, 0f, 1f, "N2", temporalfilterresponsespeed => { ShinySSRRSettings.temporalFilterResponseSpeed.value = temporalfilterresponsespeed; ShinySSRRManager.UpdateSettings(); },
            //                ShinySSRRSettings.temporalFilterResponseSpeed.overrideState, overrideState => { ShinySSRRSettings.temporalFilterResponseSpeed.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        }
            //        Slider("Downsampling", ShinySSRRSettings.downsampling.value, 1, 4, downsampling => { ShinySSRRSettings.downsampling.value = downsampling; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.downsampling.overrideState, overrideState => { ShinySSRRSettings.downsampling.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        GUILayout.Space(30);
            //        Label("REFLECTION INTENSITY", "", true);
            //        Slider("Smoothness Threshold", ShinySSRRSettings.smoothnessThreshold.value, 0f, 1f, "N2", smoothnessthreshold => { ShinySSRRSettings.smoothnessThreshold.value = smoothnessthreshold; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.smoothnessThreshold.overrideState, overrideState => { ShinySSRRSettings.smoothnessThreshold.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Reflections Min Intensity", ShinySSRRSettings.reflectionsMinIntensity.value, 0f, 1f, "N2", reflectionsminintensity => { ShinySSRRSettings.reflectionsMinIntensity.value = reflectionsminintensity; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.reflectionsMinIntensity.overrideState, overrideState => { ShinySSRRSettings.reflectionsMinIntensity.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Reflections Max Intensity", ShinySSRRSettings.reflectionsMaxIntensity.value, 0f, 1f, "N2", reflectionsmaxintensity => { ShinySSRRSettings.reflectionsMaxIntensity.value = reflectionsmaxintensity; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.reflectionsMaxIntensity.overrideState, overrideState => { ShinySSRRSettings.reflectionsMaxIntensity.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Fresnel", ShinySSRRSettings.fresnel.value, 0f, 1f, "N2", fresnel => { ShinySSRRSettings.fresnel.value = fresnel; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.fresnel.overrideState, overrideState => { ShinySSRRSettings.fresnel.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Decay", ShinySSRRSettings.decay.value, 0f, 4f, "N2", decay => { ShinySSRRSettings.decay.value = decay; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.decay.overrideState, overrideState => { ShinySSRRSettings.decay.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Toggle("Specular Control", ShinySSRRSettings.specularControl.value, false, specularcontrol => { ShinySSRRSettings.specularControl.value = specularcontrol; ShinySSRRManager.UpdateSettings(); });
            //        if (ShinySSRRSettings.temporalFilter.value != false)
            //        {
            //            Slider("Specular Soften Power", ShinySSRRSettings.specularSoftenPower.value, 0f, 32f, "N2", specularsoftenpower => { ShinySSRRSettings.specularSoftenPower.value = specularsoftenpower; ShinySSRRManager.UpdateSettings(); },
            //                ShinySSRRSettings.specularSoftenPower.overrideState, overrideState => { ShinySSRRSettings.specularSoftenPower.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        }
            //        Slider("Vignette Size", ShinySSRRSettings.vignetteSize.value, 0f, 2f, "N2", vignettesize => { ShinySSRRSettings.vignetteSize.value = vignettesize; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.vignetteSize.overrideState, overrideState => { ShinySSRRSettings.vignetteSize.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Vignette Power", ShinySSRRSettings.vignettePower.value, 0f, 2f, "N2", vignettepower => { ShinySSRRSettings.vignettePower.value = vignettepower; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.vignettePower.overrideState, overrideState => { ShinySSRRSettings.vignettePower.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        GUILayout.Space(30);
            //        Label("REFLECTION SHARPNESS", "", true);
            //        Slider("Fuzzyness", ShinySSRRSettings.fuzzyness.value, 0f, 1f, "N2", fuzzyness => { ShinySSRRSettings.fuzzyness.value = fuzzyness; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.fuzzyness.overrideState, overrideState => { ShinySSRRSettings.fuzzyness.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Contact Hardening", ShinySSRRSettings.contactHardening.value, 0f, 1f, "N2", contacthardening => { ShinySSRRSettings.contactHardening.value = contacthardening; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.contactHardening.overrideState, overrideState => { ShinySSRRSettings.contactHardening.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Minimum Blur", ShinySSRRSettings.minimumBlur.value, 0f, 1f, "N2", minimumblur => { ShinySSRRSettings.minimumBlur.value = minimumblur; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.minimumBlur.overrideState, overrideState => { ShinySSRRSettings.minimumBlur.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        Slider("Blur Downsampling", ShinySSRRSettings.blurDownsampling.value, 1, 4, blurdownsampling => { ShinySSRRSettings.blurDownsampling.value = blurdownsampling; ShinySSRRManager.UpdateSettings(); },
            //            ShinySSRRSettings.blurDownsampling.overrideState, overrideState => { ShinySSRRSettings.blurDownsampling.overrideState = overrideState; ShinySSRRManager.UpdateSettings(); });
            //        GUILayout.Space(30);
            //        Label("ADVANCED OPTIONS", "", true);
            //        Selection("Output Mode", ShinySSRRSettings.outputMode, debugtoscreen => { ShinySSRRSettings.outputMode = debugtoscreen; ShinySSRRManager.UpdateSettings(); }, -1);
            //        Toggle("Low Precision", ShinySSRRSettings.lowPrecision.value, true, lowprecision => { ShinySSRRSettings.lowPrecision.value = lowprecision; ShinySSRRManager.UpdateSettings(); });
            //        Toggle("Stop NaN", ShinySSRRSettings.stopNaN.value, true, stopnan => { ShinySSRRSettings.stopNaN.value = stopnan; ShinySSRRManager.UpdateSettings(); });
            //        Toggle("Stencil Check", ShinySSRRSettings.stencilCheck.value, true, stencilcheck => { ShinySSRRSettings.stencilCheck.value = stencilcheck; ShinySSRRManager.UpdateSettings(); });

            //    }
            //    GUILayout.EndVertical();
            //}

            if (settings.vignetteLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "VIGNETTE", settings.vignetteLayer.enabled.value, true, enabled => settings.vignetteLayer.active = settings.vignetteLayer.enabled.value = enabled);
                if (settings.vignetteLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Selection("Mode", settings.vignetteLayer.mode.value, mode => settings.vignetteLayer.mode.value = mode, -1,
                        settings.vignetteLayer.mode.overrideState, overrideState => settings.vignetteLayer.mode.overrideState = overrideState);
                    SliderColor("Colour", settings.vignetteLayer.color.value, colour => settings.vignetteLayer.color.value = colour, false,
                        settings.vignetteLayer.color.overrideState, overrideState => settings.vignetteLayer.color.overrideState = overrideState);
                    Slider("Intensity", settings.vignetteLayer.intensity, 0f, 1f, "N3", fade => settings.vignetteLayer.intensity.value = fade,
                        settings.vignetteLayer.intensity.overrideState, overrideState => settings.vignetteLayer.intensity.overrideState = overrideState);
                    Slider("Smoothness", settings.vignetteLayer.smoothness.value, 0.01f, 1f, "N3", vignette => settings.vignetteLayer.smoothness.value = vignette,
                        settings.vignetteLayer.smoothness.overrideState, overrideState => settings.vignetteLayer.smoothness.overrideState = overrideState);
                    Slider("Roundness", settings.vignetteLayer.roundness.value, 0f, 1f, "N3", vignette => settings.vignetteLayer.roundness.value = vignette,
                        settings.vignetteLayer.roundness.overrideState, overrideState => settings.vignetteLayer.roundness.overrideState = overrideState);
                    Toggle("Rounded", settings.vignetteLayer.rounded, settings.vignetteLayer.rounded.overrideState, rounded => settings.vignetteLayer.rounded.value = rounded);
                }
                GUILayout.EndVertical();
            }

            if (settings.motionBlurLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "MOTION BLUR", settings.motionBlurLayer.enabled.value, true, enabled => settings.motionBlurLayer.active = settings.motionBlurLayer.enabled.value = enabled);
                if (settings.motionBlurLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Slider("Shutter Angle", settings.motionBlurLayer.shutterAngle.value, 0f, 360f, "N2", intensity => settings.motionBlurLayer.shutterAngle.value = intensity,
                        settings.motionBlurLayer.shutterAngle.overrideState, overrideState => settings.motionBlurLayer.shutterAngle.overrideState = overrideState);
                    Slider("Sample Count", settings.motionBlurLayer.sampleCount.value, 4, 32, intensity => settings.motionBlurLayer.sampleCount.value = intensity,
                        settings.motionBlurLayer.sampleCount.overrideState, overrideState => settings.motionBlurLayer.sampleCount.overrideState = overrideState);
                }
                GUILayout.EndVertical();
            }

            if (GlobalFogManager.settings != null)
            {
                GlobalFogSettings globalfogSettings = GlobalFogManager.settings;
                GUILayout.BeginVertical(SmallTab);
                Switch(renderSettings.FontSize, "GLOBAL FOG", globalfogSettings.Enabled, true, enabled => { globalfogSettings.Enabled = enabled; GlobalFogManager.UpdateSettings(); });
                if (globalfogSettings.Enabled)
                {
                    GUILayout.Space(30);
                    Toggle("Distance Fog", globalfogSettings.distanceFog.value, false, distanceFog => { globalfogSettings.distanceFog.value = distanceFog; GlobalFogManager.UpdateSettings(); });
                    Toggle("Exclude Far Pixels", globalfogSettings.excludeFarPixels.value, false, excludeFarPixels => { globalfogSettings.excludeFarPixels.value = excludeFarPixels; GlobalFogManager.UpdateSettings(); });
                    Toggle("Use Radial Distance", globalfogSettings.useRadialDistance.value, false, useRadialDistance => { globalfogSettings.useRadialDistance.value = useRadialDistance; GlobalFogManager.UpdateSettings(); });

                    Slider("Height", globalfogSettings.height.value, 1f, 100f, "N0", height => { globalfogSettings.height.value = height; GlobalFogManager.UpdateSettings(); });
                    Slider("Height Density", globalfogSettings.heightDensity.value, 0.001f, 10f, "N3", density => { globalfogSettings.heightDensity.value = density; GlobalFogManager.UpdateSettings(); });
                    Slider("Start Distance", globalfogSettings.startDistance.value, 1f, 100f, "N0", start => { globalfogSettings.startDistance.value = start; GlobalFogManager.UpdateSettings(); });

                    SliderColor("Fog Color", globalfogSettings.fogColor, colour => { globalfogSettings.fogColor = colour; GlobalFogManager.UpdateSettings(); });
                    Selection("Fog Mode", globalfogSettings.fogMode, mode => { globalfogSettings.fogMode = mode; GlobalFogManager.UpdateSettings(); });
                }

                GUILayout.EndVertical();
            }

            //if (LuxWater_UnderWaterRenderingManager.settings != null)
            //{
            //    UnderWaterRenderingSettings underwaterSettings = LuxWater_UnderWaterRenderingManager.settings;
            //    WaterVolumeTriggerSettings triggerSettings = LuxWater_WaterVolumeTriggerManager.settings;
            //    ConnectSunToUnderwaterSettings connectSettings = ConnectSunToUnderwaterManager.settings;
            //    UnderWaterBlurSettings blurSettings = LuxWater_UnderwaterBlurManager.settings;
            //    GUILayout.BeginVertical(SmallTab);

            //    Switch(renderSettings.FontSize, "UNDERWATER RENDERING", underwaterSettings.Enabled, true, enabled =>
            //    {
            //        underwaterSettings.Enabled = enabled;
            //        triggerSettings.Enabled = enabled;
            //        connectSettings.Enabled = enabled;
            //        LuxWater_UnderWaterRenderingManager.UpdateSettings();
            //        LuxWater_WaterVolumeTriggerManager.UpdateSettings();
            //        ConnectSunToUnderwaterManager.UpdateSettings();
            //    });

            //    if (underwaterSettings.Enabled)
            //    {
            //        GUILayout.Space(30);
            //        Label("Info", "Requires Water Volume");
            //        GUILayout.Space(5);
            //        LightSelector(lightManager, "Sun Source", RenderSettings.sun, light => 
            //        {   
            //            RenderSettings.sun = light; 
            //            ConnectSunToUnderwater.ConnectSun();
            //        });

            //        GUILayout.Space(30);
            //        Toggle("Underwater Blur", blurSettings.Enabled, false, underwaterBlur => { blurSettings.Enabled = underwaterBlur; LuxWater_UnderwaterBlurManager.UpdateSettings(); });
            //        if (blurSettings.Enabled)
            //        {
            //            Slider("Blur Spread", blurSettings.BlurSpread.value, 0.1f, 1f, "N2", spread => { blurSettings.BlurSpread.value = spread; LuxWater_UnderwaterBlurManager.UpdateSettings(); });
            //            Slider("Blur Down Sample", blurSettings.BlurDownSample.value, 1, 8, downSample => { blurSettings.BlurDownSample.value = downSample; LuxWater_UnderwaterBlurManager.UpdateSettings(); });
            //            Slider("Blur Iterations", blurSettings.BlurIterations.value, 1, 8, iterations => { blurSettings.BlurIterations.value = iterations; LuxWater_UnderwaterBlurManager.UpdateSettings(); });
            //        }
            //    }

            //    GUILayout.EndVertical();
            //}

            GUILayout.EndScrollView();

        }
    }
}