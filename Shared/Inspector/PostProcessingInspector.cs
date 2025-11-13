using Graphics.GTAO;
using Graphics.VAO;
using Graphics.Settings;
using Graphics.GlobalFog;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Graphics.Inspector.Util;
using Graphics.AmplifyOcclusion;
using static Illusion.Game.Utils;
using System.Collections.Generic;


namespace Graphics.Inspector
{
    internal static class PostProcessingInspector
    {
        private static Vector2 postScrollView;
        private static bool _flareCustomize;

        private static float switchHeight;
        private static int cachedFontSize = -1;
        private static int paddingL, paddingR, margin, space;
        private static GUIStyle TabContent, SmallTab, SwitchLabel;

        static void UpdateCachedValues(GlobalSettings renderSettings)
        {
            if (cachedFontSize == renderSettings.FontSize)
                return;

            cachedFontSize = renderSettings.FontSize;

            paddingL = Mathf.RoundToInt(renderSettings.FontSize * 2f);
            paddingR = Mathf.RoundToInt(renderSettings.FontSize * 2.9f);
            margin = Mathf.RoundToInt(renderSettings.FontSize * 0.3f);
            space = paddingL + paddingR + 14;
            switchHeight = renderSettings.FontSize * 2.5f;

            TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(paddingL, paddingR, paddingL, paddingL);

            SmallTab = new GUIStyle(GUIStyles.tabcontent);
            SmallTab.padding = new RectOffset(paddingL, paddingL, paddingL, paddingL);
            SmallTab.margin = new RectOffset(0, 0, margin, margin);

            SwitchLabel = new GUIStyle(GUIStyles.switchlabel);
            SwitchLabel.fixedHeight = switchHeight;
        }

        internal static void Draw(LightManager lightManager, PostProcessingSettings settings, GlobalSettings renderSettings, PostProcessingManager postprocessingManager, bool showAdvanced)
        {

            UpdateCachedValues(renderSettings);

            GUILayout.BeginVertical(TabContent);
            {
                //GUILayout.Space(10);
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

            // Draw Ambient Occlusion
            DrawAmbientOcclusion(settings);

            // Draw Bloom
            DrawBloom(settings, postprocessingManager);

            // Draw Sun Shafts HDR
            DrawSunShaftsHDR(lightManager, settings, renderSettings);

            // Draw Color Grading
            DrawColorGrading(settings, postprocessingManager);

            // Draw Screen Space Reflections
            DrawScreenSpaceReflections(settings, renderSettings);

            // Draw Auto Exposure
            DrawAutoExposure(settings);

            // Draw Chromatic Aberration Layer
            DrawChromaticAberrationLayer(settings, postprocessingManager);

            // Draw Depth Of Field Layer
            DrawDepthOfFieldLayer(settings);

            // Draw Grain Layer
            DrawGrainLayer();

            // Draw Vignette Layer
            DrawVignetteLayer(settings);

            // Draw Motion Blur Layer
            DrawMotionBlurLayer(settings);

            // DrawGlobalFog
            DrawGlobalFogLayer();

            // Draw Lux Water
            DrawLuxWaterLayer(lightManager);

            // Draw Aura Layer
            DrawAuraLayer();

            GUILayout.EndScrollView();
        }


        private static void DrawAmbientOcclusion(PostProcessingSettings settings)
        {
            GUILayout.BeginVertical(SmallTab);

            PostProcessingSettings.AmbientOcclusionList AOList;
            // Check state every frame for backwards compatibility with scenes that might have multiple enabled.
            if (settings.ambientOcclusionLayer != null && settings.ambientOcclusionLayer.active && settings.ambientOcclusionLayer.enabled.value)
                AOList = PostProcessingSettings.AmbientOcclusionList.Legacy;
            else if (VAOManager.settings != null && VAOManager.settings.Enabled)
                AOList = PostProcessingSettings.AmbientOcclusionList.VAO;
            else if (GTAOManager.settings != null && GTAOManager.settings.Enabled)
                AOList = PostProcessingSettings.AmbientOcclusionList.GTAO;
            else if (AmplifyOccManager.settings != null && AmplifyOccManager.settings.Enabled)
                AOList = PostProcessingSettings.AmbientOcclusionList.Amplify;
            else
                AOList = PostProcessingSettings.AmbientOcclusionList.None;
            Action<PostProcessingSettings.AmbientOcclusionList> AOEnable = aol =>
            {
                //Console.WriteLine(aol);
                if (settings.ambientOcclusionLayer != null)
                    settings.ambientOcclusionLayer.active = settings.ambientOcclusionLayer.enabled.value = PostProcessingSettings.AmbientOcclusionList.Legacy == aol;
                if (VAOManager.settings != null)
                {
                    var n = PostProcessingSettings.AmbientOcclusionList.VAO == aol;
                    var s = VAOManager.settings.Enabled == n;
                    VAOManager.settings.Enabled = n;
                    if (!s) VAOManager.UpdateSettings();
                }
                if (GTAOManager.settings != null)
                {
                    var n = PostProcessingSettings.AmbientOcclusionList.GTAO == aol;
                    var s = GTAOManager.settings.Enabled == n;
                    GTAOManager.settings.Enabled = n;
                    if (!s) GTAOManager.UpdateSettings();
                }
                if (AmplifyOccManager.settings != null)
                {
                    var n = PostProcessingSettings.AmbientOcclusionList.Amplify == aol;
                    var s = AmplifyOccManager.settings.Enabled == n;
                    AmplifyOccManager.settings.Enabled = n;
                    if (!s) AmplifyOccManager.UpdateSettings();
                }
            };
            SelectionAO("AMBIENT OCCLUSION", AOList, AOEnable, 5);
            if (PostProcessingSettings.AmbientOcclusionList.Legacy == AOList)
            {
                if (settings.ambientOcclusionLayer != null)
                {
                    //GUILayout.Space(30);

                    //ToggleAlt("Enable", settings.ambientOcclusionLayer.enabled.value, true, AOEnable);
                    //ToggleAlt("Enable", settings.ambientOcclusionLayer.enabled.value, true, enabled => settings.ambientOcclusionLayer.active = settings.ambientOcclusionLayer.enabled.value = enabled);
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
                        ToggleAlt("Ambient Only", settings.ambientOcclusionLayer.ambientOnly.value, false, ambient => settings.ambientOcclusionLayer.ambientOnly.value = ambient);
                    }

                }
            }
            if (PostProcessingSettings.AmbientOcclusionList.VAO == AOList)
            {
                if (VAOManager.settings != null)
                {
                    VAOSettings vaoSettings = VAOManager.settings;
                    //GUILayout.Space(30);

                    //ToggleAlt("Enable", vaoSettings.Enabled, true, AOEnable);
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
                        ToggleAlt("Limit Max Radius", vaoSettings.MaxRadiusEnabled.value, true, limitmaxradius => { vaoSettings.MaxRadiusEnabled.value = limitmaxradius; VAOManager.UpdateSettings(); });

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
                            ToggleAlt("Same Color Hue Attenuation", vaoSettings.ColorbleedHueSuppresionEnabled.value, true, huesuppresion => { vaoSettings.ColorbleedHueSuppresionEnabled.value = huesuppresion; VAOManager.UpdateSettings(); });

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
                            ToggleAlt("Skip Backfaces", vaoSettings.GiBackfaces.value, true, gibackfaces => { vaoSettings.GiBackfaces.value = gibackfaces; VAOManager.UpdateSettings(); });
                        }
                        Label("", "", true);
                        Label("Performance Settings:", "", true);
                        ToggleAlt("Temporal Filtering", vaoSettings.EnableTemporalFiltering.value, true, temporalfiltering => { vaoSettings.EnableTemporalFiltering.value = temporalfiltering; VAOManager.UpdateSettings(); });
                        Selection("Adaptive Sampling", vaoSettings.AdaptiveType, adaptivetype => { vaoSettings.AdaptiveType = adaptivetype; VAOManager.UpdateSettings(); });
                        if (vaoSettings.EnableTemporalFiltering.value)
                        {
                        }
                        else
                        {
                            Selection("Downsampled Pre-Pass", vaoSettings.CullingPrepassMode, cullingprepass => { vaoSettings.CullingPrepassMode = cullingprepass; VAOManager.UpdateSettings(); });
                        }
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
                        ToggleAlt("Command Buffer", vaoSettings.CommandBufferEnabled.value, true, commandbuffer => { vaoSettings.CommandBufferEnabled.value = commandbuffer; VAOManager.UpdateSettings(); });
                        Selection("Normal Source", vaoSettings.NormalsSource, normalsource => { vaoSettings.NormalsSource = normalsource; VAOManager.UpdateSettings(); });

                        if (Graphics.Instance.CameraSettings.RenderingPath != CameraSettings.AIRenderingPath.Deferred)
                        {
                            Label("Rendering Mode: FORWARD", "", true);
                            ToggleAlt("High Precision Depth Buffer", vaoSettings.UsePreciseDepthBuffer.value, true, useprecisiondepthbuffer => { vaoSettings.UsePreciseDepthBuffer.value = useprecisiondepthbuffer; VAOManager.UpdateSettings(); });
                        }
                        else
                        {
                            Label("", "", true);
                            Label("Rendering Mode: DEFERRED", "", true);
                            Selection("Cmd Buffer Integration", vaoSettings.VaoCameraEvent, vaocameraevent => { vaoSettings.VaoCameraEvent = vaocameraevent; VAOManager.UpdateSettings(); });
                            ToggleAlt("G-Buffer Depth & Normals", vaoSettings.UseGBuffer.value, true, usegbuffer => { vaoSettings.UseGBuffer.value = usegbuffer; VAOManager.UpdateSettings(); });
                        }
                        Selection("Far Plane Source", vaoSettings.FarPlaneSource, farplanesource => { vaoSettings.FarPlaneSource = farplanesource; VAOManager.UpdateSettings(); });
                        Label("", "", true);
                        ToggleAlt("Luma Sensitivity", vaoSettings.IsLumaSensitive.value, true, lumasensitive => { vaoSettings.IsLumaSensitive.value = lumasensitive; VAOManager.UpdateSettings(); });
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
                        ToggleAlt("Debug Mode:", vaoSettings.OutputAOOnly.value, true, outputaoonly => { vaoSettings.OutputAOOnly.value = outputaoonly; VAOManager.UpdateSettings(); });
                        Label("", "", true);
                    }
                }
            }

            if (PostProcessingSettings.AmbientOcclusionList.GTAO == AOList)
            {
                if (GTAOManager.settings != null)
                {
                    GTAOSettings gtaoSettings = GTAOManager.settings;
                    if (Graphics.Instance.CameraSettings.RenderingPath != CameraSettings.AIRenderingPath.Deferred)
                    {
                        if (gtaoSettings.Enabled)
                        {
                            gtaoSettings.Enabled = false;
                            GTAOManager.UpdateSettings();
                        }
                        GUILayout.Space(30);
                        Label("GTAO - Available in Deferred Rendering Mode Only", "", false);
                    }
                    else
                    {

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
                            ToggleAlt("MultiBounce", gtaoSettings.MultiBounce.value, true, multiBounce => { gtaoSettings.MultiBounce.value = multiBounce; GTAOManager.UpdateSettings(); });
                            GUILayout.Space(10);
                            Selection("Debug", gtaoSettings.Debug, debug => { gtaoSettings.Debug = debug; GTAOManager.UpdateSettings(); });
                        }
                    }
                }
            }
            if (PostProcessingSettings.AmbientOcclusionList.Amplify == AOList)
            {
                if (AmplifyOccManager.settings != null)
                {
                    AmplifyOccSettings amplifyOccSettings = AmplifyOccManager.settings;
                    //GUILayout.Space(30);
                    //ToggleAlt("Enable", amplifyOccSettings.Enabled, true, AOEnable);
                    //ToggleAlt("Enable", amplifyOccSettings.Enabled, true, enabled => { amplifyOccSettings.Enabled = enabled; AmplifyOccManager.UpdateSettings(); });
                    if (amplifyOccSettings.Enabled)
                    {
                        GUILayout.Space(30);
                        SelectionApply("Apply Method", amplifyOccSettings.ApplyMethod, apply => { amplifyOccSettings.ApplyMethod = apply; AmplifyOccManager.UpdateSettings(); }, 3);
                        SelectionNormals("PerPixel Normals", amplifyOccSettings.PerPixelNormals, normals => { amplifyOccSettings.PerPixelNormals = normals; AmplifyOccManager.UpdateSettings(); }, 4);
                        Selection("Sample Count", amplifyOccSettings.SampleCount, samples => { amplifyOccSettings.SampleCount = samples; AmplifyOccManager.UpdateSettings(); }, 4);

                        Slider("Bias", amplifyOccSettings.Bias.value, 0f, 0.99f, "N2", bias => { amplifyOccSettings.Bias.value = bias; AmplifyOccManager.UpdateSettings(); });
                        Slider("Intensity", amplifyOccSettings.Intensity.value, 0f, 4f, "N2", intensity => { amplifyOccSettings.Intensity.value = intensity; AmplifyOccManager.UpdateSettings(); });
                        SliderColor("Tint", amplifyOccSettings.Tint, colour => { amplifyOccSettings.Tint = colour; AmplifyOccManager.UpdateSettings(); });
                        Slider("Radius", amplifyOccSettings.Radius.value, 0f, 32f, "N0", radius => { amplifyOccSettings.Radius.value = radius; AmplifyOccManager.UpdateSettings(); });
                        Slider("Power Exponent", amplifyOccSettings.PowerExponent.value, 1f, 16f, "N1", powerExponent => { amplifyOccSettings.PowerExponent.value = powerExponent; AmplifyOccManager.UpdateSettings(); });
                        Slider("Thickness", amplifyOccSettings.Thickness.value, 0f, 1f, "N2", thickness => { amplifyOccSettings.Thickness.value = thickness; AmplifyOccManager.UpdateSettings(); });
                        GUILayout.Space(10);
                        ToggleAlt("Cache Aware", amplifyOccSettings.CacheAware.value, false, aware => { amplifyOccSettings.CacheAware.value = aware; AmplifyOccManager.UpdateSettings(); });
                        ToggleAlt("Downsample", amplifyOccSettings.Downsample.value, false, sample => { amplifyOccSettings.Downsample.value = sample; AmplifyOccManager.UpdateSettings(); });
                        GUILayout.Space(10);
                        ToggleAlt("BILATERAL BLUR", amplifyOccSettings.BlurEnabled.value, true, blurenabled => { amplifyOccSettings.BlurEnabled.value = blurenabled; AmplifyOccManager.UpdateSettings(); });
                        if (amplifyOccSettings.BlurEnabled.value)
                        {
                            GUILayout.Space(5);
                            Slider("Blur Sharpness", amplifyOccSettings.BlurSharpness.value, 0f, 20f, "N1", blurSharpness => { amplifyOccSettings.BlurSharpness.value = blurSharpness; AmplifyOccManager.UpdateSettings(); });
                            Slider("Blur Passes", amplifyOccSettings.BlurPasses.value, 1, 4, blurPasses => { amplifyOccSettings.BlurPasses.value = blurPasses; AmplifyOccManager.UpdateSettings(); });
                            Slider("Blur Radius", amplifyOccSettings.BlurRadius.value, 1, 4, blurRadius => { amplifyOccSettings.BlurRadius.value = blurRadius; AmplifyOccManager.UpdateSettings(); });
                        }
                        GUILayout.Space(10);
                        ToggleAlt("TEMPORAL FILTER", amplifyOccSettings.FilterEnabled.value, true, enabled => { amplifyOccSettings.FilterEnabled.value = enabled; AmplifyOccManager.UpdateSettings(); });
                        if (amplifyOccSettings.FilterEnabled.value)
                        {
                            GUILayout.Space(5);
                            Slider("Filter Blending", amplifyOccSettings.FilterBlending.value, 0f, 1f, "N2", filterBlending => { amplifyOccSettings.FilterBlending.value = filterBlending; AmplifyOccManager.UpdateSettings(); });
                            Slider("Filter Response", amplifyOccSettings.FilterResponse.value, 0f, 1f, "N2", filterResponse => { amplifyOccSettings.FilterResponse.value = filterResponse; AmplifyOccManager.UpdateSettings(); });
                        }
                        GUILayout.Space(10);
                        ToggleAlt("DISTANCE FADE", amplifyOccSettings.FadeEnabled.value, true, enabled => { amplifyOccSettings.FadeEnabled.value = enabled; AmplifyOccManager.UpdateSettings(); });
                        if (amplifyOccSettings.FadeEnabled.value)
                        {
                            GUILayout.Space(5);
                            Slider("Fade Length", amplifyOccSettings.FadeLength.value, 0f, 100f, "N1", fadeLength => { amplifyOccSettings.FadeLength.value = fadeLength; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade Start", amplifyOccSettings.FadeStart.value, 0f, 100f, "N1", fadeStart => { amplifyOccSettings.FadeStart.value = fadeStart; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade To Intensity", amplifyOccSettings.FadeToIntensity.value, 0f, 1f, "N2", fadeToIntensity => { amplifyOccSettings.FadeToIntensity.value = fadeToIntensity; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade To Power Exponent", amplifyOccSettings.FadeToPowerExponent.value, 0f, 16f, "N2", fadeToPowerExponent => { amplifyOccSettings.FadeToPowerExponent.value = fadeToPowerExponent; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade To Radius", amplifyOccSettings.FadeToRadius.value, 0f, 32f, "N1", fadeToRadius => { amplifyOccSettings.FadeToRadius.value = fadeToRadius; AmplifyOccManager.UpdateSettings(); });
                            Slider("Fade To Thickness", amplifyOccSettings.FadeToThickness.value, 0f, 1f, "N2", fadeToThickness => { amplifyOccSettings.FadeToThickness.value = fadeToThickness; AmplifyOccManager.UpdateSettings(); });
                            SliderColor("Fade To Tint", amplifyOccSettings.FadeToTint, colour => { amplifyOccSettings.FadeToTint = colour; AmplifyOccManager.UpdateSettings(); });
                        }
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private static void DrawBloom(PostProcessingSettings settings, PostProcessingManager postprocessingManager)
        {
            if (settings.bloomLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch("BLOOM", settings.bloomLayer.enabled.value, true, enabled => settings.bloomLayer.active = settings.bloomLayer.enabled.value = enabled);
                if (settings.bloomLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Slider("Intensity", settings.bloomLayer.intensity.value, 0f, 10f, "N1", intensity => settings.bloomLayer.intensity.value = intensity,
                        settings.bloomLayer.intensity.overrideState, overrideState => settings.bloomLayer.intensity.overrideState = overrideState);
                    Slider("Threshold", settings.bloomLayer.threshold.value, 0f, 10f, "N1", threshold => settings.bloomLayer.threshold.value = threshold,
                        settings.bloomLayer.threshold.overrideState, overrideState => settings.bloomLayer.threshold.overrideState = overrideState);
                    Slider("SoftKnee", settings.bloomLayer.softKnee.value, 0f, 1f, "N1", softKnee => settings.bloomLayer.softKnee.value = softKnee,
                        settings.bloomLayer.softKnee.overrideState, overrideState => settings.bloomLayer.softKnee.overrideState = overrideState);
                    Slider("Clamp", settings.bloomLayer.clamp.value, 1, 1000, "N0", clamp => settings.bloomLayer.clamp.value = clamp,
                        settings.bloomLayer.clamp.overrideState, overrideState => settings.bloomLayer.clamp.overrideState = overrideState);
                    Slider("Diffusion", (int)settings.bloomLayer.diffusion.value, 1, 10, "N0", diffusion => settings.bloomLayer.diffusion.value = diffusion,
                        settings.bloomLayer.diffusion.overrideState, overrideState => settings.bloomLayer.diffusion.overrideState = overrideState);
                    Slider("AnamorphicRatio", settings.bloomLayer.anamorphicRatio.value, -1, 1, "N1", anamorphicRatio => settings.bloomLayer.anamorphicRatio.value = anamorphicRatio,
                        settings.bloomLayer.anamorphicRatio.overrideState, overrideState => settings.bloomLayer.anamorphicRatio.overrideState = overrideState);
                    GUILayout.Space(5);
                    SliderColor("Colour", settings.bloomLayer.color.value, colour => { settings.bloomLayer.color.value = colour; }, settings.bloomLayer.color.overrideState,
                        settings.bloomLayer.color.overrideState, overrideState => settings.bloomLayer.color.overrideState = overrideState);
                    GUILayout.Space(5);
                    ToggleAlt("Fast Mode", settings.bloomLayer.fastMode.value, false, fastMode => settings.bloomLayer.fastMode.value = fastMode);
                    GUILayout.Space(5);
                    int lensDirtIndex = SelectionTexture("Lens Dirt", postprocessingManager.CurrentLensDirtTextureIndex, postprocessingManager.LensDirtPreviews, space,
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
        }

        private static void DrawSunShaftsHDR(LightManager lightManager, PostProcessingSettings settings, GlobalSettings renderSettings)
        {
            GUILayout.BeginVertical(SmallTab);

            if (settings.sunShaftsHDRLayer != null)
            {
                Switch("SUN SHAFTS HDR", settings.sunShaftsHDRLayer.enabled, true, enabled => { settings.sunShaftsHDRLayer.active = enabled; settings.sunShaftsHDRLayer.enabled.Override(enabled); });
                if (settings.sunShaftsHDRLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    ToggleAlt("Use Global Sun", settings.sunShaftsHDRLayer.connectSun, false, connectsun => settings.sunShaftsHDRLayer.connectSun.Override(connectsun));
                    GUILayout.Space(5);
                    if (settings.sunShaftsHDRLayer.connectSun)
                    {
                        LightSelector(lightManager, "Sun Source", RenderSettings.sun, light =>
                        {
                            RenderSettings.sun = light;
                            ConnectSunToUnderwater.ConnectSun();
                        });
                    }
                    else
                    {
                        Dimension("Source Position", settings.sunShaftsHDRLayer.sunTransform.value, pos => { settings.sunShaftsHDRLayer.sunTransform.value = pos; settings.sunShaftsHDRLayer.sunTransform.Override(pos); });
                        GUILayout.Space(5);
                    }
                    GUILayout.Space(5);
                    SliderColor("Color", settings.sunShaftsHDRLayer.sunColor.value, colour => { settings.sunShaftsHDRLayer.sunColor.value = colour; }, settings.sunShaftsHDRLayer.sunColor.overrideState,
                        settings.sunShaftsHDRLayer.sunColor.overrideState, overrideState => settings.sunShaftsHDRLayer.sunColor.overrideState = overrideState);
                    GUILayout.Space(5);
                    SliderColor("Threshold", settings.sunShaftsHDRLayer.sunThreshold.value, threshold => { settings.sunShaftsHDRLayer.sunThreshold.value = threshold; }, settings.sunShaftsHDRLayer.sunThreshold.overrideState,
                        settings.sunShaftsHDRLayer.sunThreshold.overrideState, overrideState => settings.sunShaftsHDRLayer.sunThreshold.overrideState = overrideState);
                    GUILayout.Space(5);
                    Slider("Intensity", settings.sunShaftsHDRLayer.sunShaftIntensity.value, 0f, 10f, "N1", intensity => settings.sunShaftsHDRLayer.sunShaftIntensity.value = intensity,
                        settings.sunShaftsHDRLayer.sunShaftIntensity.overrideState, overrideState => settings.sunShaftsHDRLayer.sunShaftIntensity.overrideState = overrideState);
                    Slider("Range", settings.sunShaftsHDRLayer.sunShaftBlurRadius.value, 0f, 10f, "N1", blurradius => settings.sunShaftsHDRLayer.sunShaftBlurRadius.value = blurradius,
                        settings.sunShaftsHDRLayer.sunShaftBlurRadius.overrideState, overrideState => settings.sunShaftsHDRLayer.sunShaftBlurRadius.overrideState = overrideState);
                    Slider("Range Iterations", settings.sunShaftsHDRLayer.radialBlurIterations.value, 1, 3, iterations => settings.sunShaftsHDRLayer.radialBlurIterations.value = iterations,
                        settings.sunShaftsHDRLayer.radialBlurIterations.overrideState, overrideState => settings.sunShaftsHDRLayer.radialBlurIterations.overrideState = overrideState);
                    Slider("Max Radius", settings.sunShaftsHDRLayer.maxRadius.value, 0.1f, 1f, "N2", maxradius => settings.sunShaftsHDRLayer.maxRadius.value = maxradius,
                        settings.sunShaftsHDRLayer.maxRadius.overrideState, overrideState => settings.sunShaftsHDRLayer.maxRadius.overrideState = overrideState);
                    ToggleAlt("Use Depth Texture", settings.sunShaftsHDRLayer.useDepthTexture, false, useDepthTexture => settings.sunShaftsHDRLayer.useDepthTexture.Override(useDepthTexture));
                }
            }

            GUILayout.EndVertical();
        }

        private static void DrawColorGrading(PostProcessingSettings settings, PostProcessingManager postprocessingManager)
        {
            GUILayout.BeginVertical(SmallTab);

            if (settings.colorGradingLayer)
            {
                Switch("COLOR GRADING", settings.colorGradingLayer.enabled.value, true, enabled =>
                {
                    settings.colorGradingLayer.active = settings.colorGradingLayer.enabled;
                    settings.agxColorLayer.active = settings.colorGradingLayer.enabled.value = enabled;
                    settings.agxColorPostLayer.active = settings.colorGradingLayer.enabled.value = enabled;
                });
                if (settings.colorGradingLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Selection("Mode", (PostProcessingSettings.GradingMode)settings.colorGradingLayer.gradingMode.value, mode =>
                    {
                        settings.colorGradingLayer.gradingMode.value = (GradingMode)mode;
                        if (GradingMode.External != settings.colorGradingLayer.gradingMode.value)
                        {
                            settings.agxColorLayer.active = false;
                            settings.agxColorLayer.enabled.Override(false);
                            settings.agxColorPostLayer.external.Override(false);
                            settings.agxColorPostLayer.active = false;
                            settings.agxColorPostLayer.enabled.Override(false);
                        }
                        else
                        {
                            settings.agxColorLayer.active = true;
                            settings.agxColorLayer.enabled.Override(true);
                            settings.agxColorPostLayer.external.Override(true);
                            settings.agxColorPostLayer.active = true;
                            settings.agxColorPostLayer.enabled.Override(true);
                        }
                    });
                    if (GradingMode.External != settings.colorGradingLayer.gradingMode.value)
                    {

                        if (GradingMode.LowDefinitionRange == settings.colorGradingLayer.gradingMode.value)
                        {
                            Selection("LUT", postprocessingManager.CurrentLUTName, postprocessingManager.LUTNames,
                                lut => { if (lut != postprocessingManager.CurrentLUTName) { settings.colorGradingLayer.ldrLut.Override(postprocessingManager.LoadLUT(lut)); } }, 3);
                            Slider("LUT Blend", settings.colorGradingLayer.ldrLutContribution.value, 0, 1, "N2", ldrLutContribution => settings.colorGradingLayer.ldrLutContribution.value = ldrLutContribution,
                                settings.colorGradingLayer.ldrLutContribution.overrideState, overrideState => settings.colorGradingLayer.ldrLutContribution.overrideState = overrideState);
                        }
                        else
                        {
                            Selection("Tonemapping", settings.colorGradingLayer.tonemapper.value, mode => settings.colorGradingLayer.tonemapper.value = mode);
                        }

                        GUILayout.Space(30);
                        Label("WHITE BALANCE", "", true);
                        GUILayout.Space(10);
                        SliderColorTemp("Temperature", settings.colorGradingLayer.temperature.value, -100, 100, "N0", temperature => settings.colorGradingLayer.temperature.value = temperature,
                            settings.colorGradingLayer.temperature.overrideState, overrideState => settings.colorGradingLayer.temperature.overrideState = overrideState);
                        SliderColorTint("Tint", settings.colorGradingLayer.tint.value, -100, 100, "N0", tint => settings.colorGradingLayer.tint.value = tint,
                            settings.colorGradingLayer.tint.overrideState, overrideState => settings.colorGradingLayer.tint.overrideState = overrideState);

                        GUILayout.Space(30);
                        Label("TONE", "", true);
                        GUILayout.Space(10);
                        if (GradingMode.HighDefinitionRange == settings.colorGradingLayer.gradingMode.value)
                        {
                            SliderAlpha("Post-exposure (EV)", settings.colorGradingLayer.postExposure.value, -3, 3, "N2", false, value => settings.colorGradingLayer.postExposure.value = value, settings.colorGradingLayer.postExposure.overrideState, overrideState => settings.colorGradingLayer.postExposure.overrideState = overrideState);
                        }
                        SliderColorHue("Hue Shift", settings.colorGradingLayer.hueShift.value, -180, 180, "N0", hueShift => settings.colorGradingLayer.hueShift.value = hueShift,
                            settings.colorGradingLayer.hueShift.overrideState, overrideState => settings.colorGradingLayer.hueShift.overrideState = overrideState);
                        SliderColorVib("Saturation", settings.colorGradingLayer.saturation.value, -100, 100, "N0", saturation => settings.colorGradingLayer.saturation.value = saturation,
                            settings.colorGradingLayer.saturation.overrideState, overrideState => settings.colorGradingLayer.saturation.overrideState = overrideState);
                        if (GradingMode.LowDefinitionRange == settings.colorGradingLayer.gradingMode.value)
                        {
                            SliderAlpha("Brightness", settings.colorGradingLayer.brightness.value, -100, 100, "N0", false, brightness => settings.colorGradingLayer.brightness.value = brightness,
                                settings.colorGradingLayer.brightness.overrideState, overrideState => settings.colorGradingLayer.brightness.overrideState = overrideState);
                        }
                        SliderAlpha("Contrast", settings.colorGradingLayer.contrast.value, -100, 100, "N0", true, contrast => settings.colorGradingLayer.contrast.value = contrast,
                            settings.colorGradingLayer.contrast.overrideState, overrideState => settings.colorGradingLayer.contrast.overrideState = overrideState);

                        GUILayout.Space(30);
                        Label("CHANNEL MIXER", "", true);
                        GUILayout.Space(10);
                        Selection("Channel", settings.agxColorLayer.channelMixer, mode => settings.agxColorLayer.channelMixer = mode);
                        if (settings.agxColorLayer.channelMixer == AgXColor.ChannelMixer.Red)
                        {
                            GUILayout.Space(10);
                            Slider("Red", settings.agxColorLayer.mixerRedOutRedIn.value, -200, 200, "N0", red => settings.agxColorLayer.mixerRedOutRedIn.value = red,
                                settings.agxColorLayer.mixerRedOutRedIn.overrideState, overrideState => settings.agxColorLayer.mixerRedOutRedIn.overrideState = overrideState);
                            Slider("Green", settings.agxColorLayer.mixerRedOutGreenIn.value, -200, 200, "N0", green => settings.agxColorLayer.mixerRedOutGreenIn.value = green,
                                settings.agxColorLayer.mixerRedOutGreenIn.overrideState, overrideState => settings.agxColorLayer.mixerRedOutGreenIn.overrideState = overrideState);
                            Slider("Blue", settings.agxColorLayer.mixerRedOutBlueIn.value, -200, 200, "N0", blue => settings.agxColorLayer.mixerRedOutBlueIn.value = blue,
                                settings.agxColorLayer.mixerRedOutBlueIn.overrideState, overrideState => settings.agxColorLayer.mixerRedOutBlueIn.overrideState = overrideState);
                        }
                        if (settings.agxColorLayer.channelMixer == AgXColor.ChannelMixer.Green)
                        {
                            GUILayout.Space(10);
                            Slider("Red", settings.agxColorLayer.mixerGreenOutRedIn.value, -200, 200, "N0", red => settings.agxColorLayer.mixerGreenOutRedIn.value = red,
                                settings.agxColorLayer.mixerGreenOutRedIn.overrideState, overrideState => settings.agxColorLayer.mixerGreenOutRedIn.overrideState = overrideState);
                            Slider("Green", settings.agxColorLayer.mixerGreenOutGreenIn.value, -200, 200, "N0", green => settings.agxColorLayer.mixerGreenOutGreenIn.value = green,
                                settings.agxColorLayer.mixerGreenOutGreenIn.overrideState, overrideState => settings.agxColorLayer.mixerGreenOutGreenIn.overrideState = overrideState);
                            Slider("Blue", settings.agxColorLayer.mixerGreenOutBlueIn.value, -200, 200, "N0", blue => settings.agxColorLayer.mixerGreenOutBlueIn.value = blue,
                                settings.agxColorLayer.mixerGreenOutBlueIn.overrideState, overrideState => settings.agxColorLayer.mixerGreenOutBlueIn.overrideState = overrideState);
                        }
                        if (settings.agxColorLayer.channelMixer == AgXColor.ChannelMixer.Blue)
                        {
                            GUILayout.Space(10);
                            Slider("Red", settings.agxColorLayer.mixerBlueOutRedIn.value, -200, 200, "N0", red => settings.agxColorLayer.mixerBlueOutRedIn.value = red,
                                settings.agxColorLayer.mixerBlueOutRedIn.overrideState, overrideState => settings.agxColorLayer.mixerBlueOutRedIn.overrideState = overrideState);
                            Slider("Green", settings.agxColorLayer.mixerBlueOutGreenIn.value, -200, 200, "N0", green => settings.agxColorLayer.mixerBlueOutGreenIn.value = green,
                                settings.agxColorLayer.mixerBlueOutGreenIn.overrideState, overrideState => settings.agxColorLayer.mixerBlueOutGreenIn.overrideState = overrideState);
                            Slider("Blue", settings.agxColorLayer.mixerBlueOutBlueIn.value, -200, 200, "N0", blue => settings.agxColorLayer.mixerBlueOutBlueIn.value = blue,
                                settings.agxColorLayer.mixerBlueOutBlueIn.overrideState, overrideState => settings.agxColorLayer.mixerBlueOutBlueIn.overrideState = overrideState);
                        }

                        GUILayout.Space(30);
                        Label("COLOR BALANCE", "", true);
                        GUILayout.Space(10);
                        SliderColor("Lift", settings.colorGradingLayer.lift.value, colour => settings.colorGradingLayer.lift.value = colour, false,
                            settings.colorGradingLayer.lift.overrideState, overrideState => settings.colorGradingLayer.lift.overrideState = overrideState, "Value", -1.5f, 3f);
                        SliderColor("Gamma", settings.colorGradingLayer.gamma.value, colour => settings.colorGradingLayer.gamma.value = colour, false,
                            settings.colorGradingLayer.gamma.overrideState, overrideSate => settings.colorGradingLayer.gamma.overrideState = overrideSate, "Value", -1.5f, 3f);
                        SliderColor("Gain", settings.colorGradingLayer.gain.value, colour => settings.colorGradingLayer.gain.value = colour, false,
                            settings.colorGradingLayer.gain.overrideState, overrideSate => settings.colorGradingLayer.gain.overrideState = overrideSate, "Value", -1.5f, 3f);

                        GUILayout.Space(30);
                        if (settings.colorClippingLayer != null)
                        {
                            ToggleAlt("Debug Clipping", settings.colorClippingLayer.enabled, false, enabled => { settings.colorClippingLayer.active = enabled; settings.colorClippingLayer.enabled.Override(enabled); });
                            if (settings.colorClippingLayer.enabled.value)
                            {
                                GUILayout.Space(10);
                                Slider("Shadow Threshold", settings.colorClippingLayer.shadowThreshold.value, 0.001f, 0.1f, "N3", shadowThreshold => settings.colorClippingLayer.shadowThreshold.value = shadowThreshold,
                                    settings.colorClippingLayer.shadowThreshold.overrideState, overrideState => settings.colorClippingLayer.shadowThreshold.overrideState = overrideState);
                                Slider("Highlight Threshold", settings.colorClippingLayer.highlightThreshold.value, 0.9f, 1f, "N3", highlightThreshold => settings.colorClippingLayer.highlightThreshold.value = highlightThreshold,
                                    settings.colorClippingLayer.highlightThreshold.overrideState, overrideState => settings.colorClippingLayer.highlightThreshold.overrideState = overrideState);
                                ToggleAlt("Show Shadows", settings.colorClippingLayer.showShadows, false, showShadows => settings.colorClippingLayer.showShadows.Override(showShadows));
                                ToggleAlt("Show Highlights", settings.colorClippingLayer.showHighlights, false, showHighlights => settings.colorClippingLayer.showHighlights.Override(showHighlights));

                            }
                        }
                    }
                    else
                    {
                        //settings.agxColorLayer.active = true;
                        //settings.agxColorLayer.enabled.Override(true);
                        GUILayout.Space(30);

                        if (settings.agxColorLayer != null && settings.agxColorPostLayer != null)
                        {
                            if (settings.agxColorLayer.enabled.value)
                            {
                                settings.colorGradingLayer.postExposure.overrideState = false;

                                GUILayout.Space(30);
                                Label("PRE-TRANSFORM (HDR)", "This settings should be preferred when doing corrections.", true);
                                GUILayout.Space(10);
                                SliderAlpha("Exposure", settings.agxColorLayer.exposure.value, -9, 9, "N2", false, exposure => settings.agxColorLayer.exposure.value = exposure,
                                    settings.agxColorLayer.exposure.overrideState, overrideState => settings.agxColorLayer.exposure.overrideState = overrideState);

                                GUILayout.Space(30);
                                Label("WHITE BALANCE", "", true);
                                GUILayout.Space(10);
                                SliderColorTemp("Temperature", settings.agxColorLayer.temperature.value, -100, 100, "N0", temperature => settings.agxColorLayer.temperature.value = temperature,
                                    settings.agxColorLayer.temperature.overrideState, overrideState => settings.agxColorLayer.temperature.overrideState = overrideState);
                                SliderColorTint("Tint", settings.agxColorLayer.tint.value, -100, 100, "N0", tint => settings.agxColorLayer.tint.value = tint,
                                    settings.agxColorLayer.tint.overrideState, overrideState => settings.agxColorLayer.tint.overrideState = overrideState);

                                GUILayout.Space(30);
                                Label("TONE", "", true);
                                GUILayout.Space(10);
                                SliderColorHue("Hue Shift", settings.agxColorLayer.hueShift.value, -180, 180, "N0", hueShift => settings.agxColorLayer.hueShift.value = hueShift,
                                    settings.agxColorLayer.hueShift.overrideState, overrideState => settings.agxColorLayer.hueShift.overrideState = overrideState);
                                SliderColorVib("Vibrance", settings.agxColorLayer.colorBoost.value, -1, 1, "N2", colorboost => settings.agxColorLayer.colorBoost.value = colorboost,
                                    settings.agxColorLayer.colorBoost.overrideState, overrideState => settings.agxColorLayer.colorBoost.overrideState = overrideState);
                                Slider("Perceptual", settings.agxColorLayer.perceptual.value, 0, 1, "N1", perceptual => settings.agxColorLayer.perceptual.value = perceptual,
                                    settings.agxColorLayer.perceptual.overrideState, overrideState => settings.agxColorLayer.perceptual.overrideState = overrideState);

                                GUILayout.Space(30);
                                Label("COLOR BALANCE (HDR)", "", true);
                                GUILayout.Space(10);
                                SliderColor("Offset (Highlights)", settings.agxColorLayer.offset.value, colour => settings.agxColorLayer.offset.value = colour, true,
                                    settings.agxColorLayer.offset.overrideState, overrideState => settings.agxColorLayer.offset.overrideState = overrideState, "Value", 0f, 1f);
                                SliderColor("Power (Midtones)", settings.agxColorLayer.power.value, colour => settings.agxColorLayer.power.value = colour, true,
                                    settings.agxColorLayer.power.overrideState, overrideSate => settings.agxColorLayer.power.overrideState = overrideSate, "Value", 0f, 2f);
                                SliderColor("Slope (Shadows)", settings.agxColorLayer.slope.value, colour => settings.agxColorLayer.slope.value = colour, true,
                                    settings.agxColorLayer.slope.overrideState, overrideSate => settings.agxColorLayer.slope.overrideState = overrideSate, "Value", 0f, 2f);

                                GUILayout.Space(30);
                                Label("CHANNEL MIXER", "", true);
                                GUILayout.Space(10);
                                Selection("Channel", settings.agxColorLayer.channelMixer, mode => settings.agxColorLayer.channelMixer = mode);
                                if (settings.agxColorLayer.channelMixer == AgXColor.ChannelMixer.Red)
                                {
                                    GUILayout.Space(10);
                                    Slider("Red", settings.agxColorLayer.mixerRedOutRedIn.value, -200, 200, "N0", red => settings.agxColorLayer.mixerRedOutRedIn.value = red,
                                        settings.agxColorLayer.mixerRedOutRedIn.overrideState, overrideState => settings.agxColorLayer.mixerRedOutRedIn.overrideState = overrideState);
                                    Slider("Green", settings.agxColorLayer.mixerRedOutGreenIn.value, -200, 200, "N0", green => settings.agxColorLayer.mixerRedOutGreenIn.value = green,
                                        settings.agxColorLayer.mixerRedOutGreenIn.overrideState, overrideState => settings.agxColorLayer.mixerRedOutGreenIn.overrideState = overrideState);
                                    Slider("Blue", settings.agxColorLayer.mixerRedOutBlueIn.value, -200, 200, "N0", blue => settings.agxColorLayer.mixerRedOutBlueIn.value = blue,
                                        settings.agxColorLayer.mixerRedOutBlueIn.overrideState, overrideState => settings.agxColorLayer.mixerRedOutBlueIn.overrideState = overrideState);
                                }
                                if (settings.agxColorLayer.channelMixer == AgXColor.ChannelMixer.Green)
                                {
                                    GUILayout.Space(10);
                                    Slider("Red", settings.agxColorLayer.mixerGreenOutRedIn.value, -200, 200, "N0", red => settings.agxColorLayer.mixerGreenOutRedIn.value = red,
                                        settings.agxColorLayer.mixerGreenOutRedIn.overrideState, overrideState => settings.agxColorLayer.mixerGreenOutRedIn.overrideState = overrideState);
                                    Slider("Green", settings.agxColorLayer.mixerGreenOutGreenIn.value, -200, 200, "N0", green => settings.agxColorLayer.mixerGreenOutGreenIn.value = green,
                                        settings.agxColorLayer.mixerGreenOutGreenIn.overrideState, overrideState => settings.agxColorLayer.mixerGreenOutGreenIn.overrideState = overrideState);
                                    Slider("Blue", settings.agxColorLayer.mixerGreenOutBlueIn.value, -200, 200, "N0", blue => settings.agxColorLayer.mixerGreenOutBlueIn.value = blue,
                                        settings.agxColorLayer.mixerGreenOutBlueIn.overrideState, overrideState => settings.agxColorLayer.mixerGreenOutBlueIn.overrideState = overrideState);
                                }
                                if (settings.agxColorLayer.channelMixer == AgXColor.ChannelMixer.Blue)
                                {
                                    GUILayout.Space(10);
                                    Slider("Red", settings.agxColorLayer.mixerBlueOutRedIn.value, -200, 200, "N0", red => settings.agxColorLayer.mixerBlueOutRedIn.value = red,
                                        settings.agxColorLayer.mixerBlueOutRedIn.overrideState, overrideState => settings.agxColorLayer.mixerBlueOutRedIn.overrideState = overrideState);
                                    Slider("Green", settings.agxColorLayer.mixerBlueOutGreenIn.value, -200, 200, "N0", green => settings.agxColorLayer.mixerBlueOutGreenIn.value = green,
                                        settings.agxColorLayer.mixerBlueOutGreenIn.overrideState, overrideState => settings.agxColorLayer.mixerBlueOutGreenIn.overrideState = overrideState);
                                    Slider("Blue", settings.agxColorLayer.mixerBlueOutBlueIn.value, -200, 200, "N0", blue => settings.agxColorLayer.mixerBlueOutBlueIn.value = blue,
                                        settings.agxColorLayer.mixerBlueOutBlueIn.overrideState, overrideState => settings.agxColorLayer.mixerBlueOutBlueIn.overrideState = overrideState);
                                }

                            }
                            GUILayout.Space(30);
                            Label("COLOR TRANSFORM", "", true);
                            GUILayout.Space(10);
                            Selection("Tonemapper", postprocessingManager.Current3DLUTName, postprocessingManager.LUT3DNames,
                                lut3d => { if (lut3d != postprocessingManager.Current3DLUTName) { settings.colorGradingLayer.externalLut.Override(postprocessingManager.Load3DLUT(lut3d)); } }, 2);

                            GUILayout.Space(30);
                            //Toggle("POST-TRANSFORM (LDR)", settings.agxColorPostLayer.enabled, true, enabled => { settings.agxColorPostLayer.active = enabled; settings.agxColorPostLayer.enabled.Override(enabled); settings.agxColorPostLayer.external.Override(enabled); });
                            if (settings.agxColorPostLayer.enabled.value)
                            {
                                GUILayout.Space(10);
                                Label("", "Warning! This settings may be destructive. Prefer to use Pre-Transform instead.", true);
                                GUILayout.Space(10);
                                SliderColorVib("Saturation", settings.agxColorPostLayer.saturation.value, -100, 100, "N0", saturation => settings.agxColorPostLayer.saturation.value = saturation,
                                    settings.agxColorPostLayer.saturation.overrideState, overrideState => settings.agxColorPostLayer.saturation.overrideState = overrideState);
                                SliderAlpha("Brightness", settings.agxColorPostLayer.brightness.value, -100, 100, "N0", false, brightness => settings.agxColorPostLayer.brightness.value = brightness,
                                    settings.agxColorPostLayer.brightness.overrideState, overrideState => settings.agxColorPostLayer.brightness.overrideState = overrideState);
                                SliderAlpha("Contrast", settings.agxColorPostLayer.contrast.value, -100, 100, "N0", true, contrast => settings.agxColorPostLayer.contrast.value = contrast,
                                    settings.agxColorPostLayer.contrast.overrideState, overrideState => settings.agxColorPostLayer.contrast.overrideState = overrideState);

                                GUILayout.Space(30);
                                Label("COLOR BALANCE (LDR)", "", true);
                                GUILayout.Space(10);
                                SliderColor("Lift", settings.agxColorPostLayer.lift.value, colour => settings.agxColorPostLayer.lift.value = colour, false,
                                    settings.agxColorPostLayer.lift.overrideState, overrideState => settings.agxColorPostLayer.lift.overrideState = overrideState, "Value", -1.5f, 3f);
                                SliderColor("Gamma", settings.agxColorPostLayer.gamma.value, colour => settings.agxColorPostLayer.gamma.value = colour, false,
                                    settings.agxColorPostLayer.gamma.overrideState, overrideSate => settings.agxColorPostLayer.gamma.overrideState = overrideSate, "Value", -1.5f, 3f);
                                SliderColor("Gain", settings.agxColorPostLayer.gain.value, colour => settings.agxColorPostLayer.gain.value = colour, false,
                                    settings.agxColorPostLayer.gain.overrideState, overrideSate => settings.agxColorPostLayer.gain.overrideState = overrideSate, "Value", -1.5f, 3f);

                                //GUILayout.Space(30);
                                if (settings.colorClippingLayer != null)
                                {
                                    ToggleAlt("Debug Clipping", settings.colorClippingLayer.enabled, false, enabled => { settings.colorClippingLayer.active = enabled; settings.colorClippingLayer.enabled.Override(enabled); });
                                    if (settings.colorClippingLayer.enabled.value)
                                    {
                                        GUILayout.Space(10);
                                        Slider("Shadow Threshold", settings.colorClippingLayer.shadowThreshold.value, 0.001f, 0.1f, "N3", shadowThreshold => settings.colorClippingLayer.shadowThreshold.value = shadowThreshold,
                                            settings.colorClippingLayer.shadowThreshold.overrideState, overrideState => settings.colorClippingLayer.shadowThreshold.overrideState = overrideState);
                                        Slider("Highlight Threshold", settings.colorClippingLayer.highlightThreshold.value, 0.9f, 1f, "N3", highlightThreshold => settings.colorClippingLayer.highlightThreshold.value = highlightThreshold,
                                            settings.colorClippingLayer.highlightThreshold.overrideState, overrideState => settings.colorClippingLayer.highlightThreshold.overrideState = overrideState);
                                        ToggleAlt("Show Shadows", settings.colorClippingLayer.showShadows, false, showShadows => settings.colorClippingLayer.showShadows.Override(showShadows));
                                        ToggleAlt("Show Highlights", settings.colorClippingLayer.showHighlights, false, showHighlights => settings.colorClippingLayer.showHighlights.Override(showHighlights));

                                    }
                                }
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
            }
        }

        private static void DrawScreenSpaceReflections(PostProcessingSettings settings, GlobalSettings renderSettings)
        {
            if (settings.screenSpaceReflectionsLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch("SSR", settings.screenSpaceReflectionsLayer.enabled.value, true, enabled => settings.screenSpaceReflectionsLayer.active = settings.screenSpaceReflectionsLayer.enabled.value = enabled);
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
                    Slider("Distance Fade", settings.screenSpaceReflectionsLayer.distanceFade, 0f, 1f, "N2", fade => settings.screenSpaceReflectionsLayer.distanceFade.value = fade,
                        settings.screenSpaceReflectionsLayer.distanceFade.overrideState, overrideState => settings.screenSpaceReflectionsLayer.distanceFade.overrideState = overrideState);
                    Slider("Vignette", settings.screenSpaceReflectionsLayer.vignette.value, 0f, 1f, "N2", vignette => settings.screenSpaceReflectionsLayer.vignette.value = vignette,
                        settings.screenSpaceReflectionsLayer.vignette.overrideState, overrideState => settings.screenSpaceReflectionsLayer.vignette.overrideState = overrideState);
                }
                GUILayout.EndVertical();
            }

        }

        private static void DrawAutoExposure(PostProcessingSettings settings)
        {
            GUILayout.BeginVertical(SmallTab);
            if (settings.autoExposureLayer != null)
            {
                Switch("AUTO EXPOSURE", settings.autoExposureLayer.enabled.value, true, enabled => settings.autoExposureLayer.active = settings.autoExposureLayer.enabled.value = enabled);
                if (settings.autoExposureLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    ToggleAlt("Histogram Filtering (%)", settings.autoExposureLayer.filtering.overrideState, false, overrideState => settings.autoExposureLayer.filtering.overrideState = overrideState);
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
                    ToggleAlt("Eye Adaptation", settings.autoExposureLayer.eyeAdaptation.overrideState, false, overrideState => settings.autoExposureLayer.eyeAdaptation.overrideState = overrideState);
                    Selection("Type", settings.autoExposureLayer.eyeAdaptation.value, type => settings.autoExposureLayer.eyeAdaptation.value = type, -1,
                        settings.autoExposureLayer.eyeAdaptation.overrideState);
                    Slider("Speed from light to dark", settings.autoExposureLayer.speedUp.value, 0f, 10f, "N1",
                        luminance => settings.autoExposureLayer.speedUp.value = luminance, settings.autoExposureLayer.speedUp.overrideState,
                        overrideState => settings.autoExposureLayer.speedUp.overrideState = overrideState);
                    Slider("Speed from dark to light", settings.autoExposureLayer.speedDown.value, 0f, 10f, "N1",
                        luminance => settings.autoExposureLayer.speedDown.value = luminance, settings.autoExposureLayer.speedDown.overrideState,
                        overrideState => settings.autoExposureLayer.speedDown.overrideState = overrideState);
                }
            }
            GUILayout.EndVertical();
        }

        private static void DrawChromaticAberrationLayer(PostProcessingSettings settings, PostProcessingManager postprocessingManager)
        {
            GUILayout.BeginVertical(SmallTab);

            if (settings.chromaticAberrationLayer)
            {
                Switch("CHROMATIC ABERRATION", settings.chromaticAberrationLayer.enabled.value, true, enabled => settings.chromaticAberrationLayer.active = settings.chromaticAberrationLayer.enabled.value = enabled);
                if (settings.chromaticAberrationLayer.enabled.value)
                {
                    GUILayout.Space(30);

                    Selection("Spectral Lut", postprocessingManager.CurrentSpecLUTName, postprocessingManager.LUTSpecNames,
                        speclut => { if (speclut != postprocessingManager.CurrentSpecLUTName) { settings.chromaticAberrationLayer.spectralLut.Override(postprocessingManager.LoadSpecLUT(speclut)); } }, 4);
                    GUILayout.Space(10);
                    Slider("Intensity", settings.chromaticAberrationLayer.intensity.value, 0f, 5f, "N2", intensity => settings.chromaticAberrationLayer.intensity.value = intensity,
                        settings.chromaticAberrationLayer.intensity.overrideState, overrideState => settings.chromaticAberrationLayer.intensity.overrideState = overrideState);
                    ToggleAlt("Fast Mode", settings.chromaticAberrationLayer.fastMode.value, false, fastMode => settings.chromaticAberrationLayer.fastMode.value = fastMode);
                }

            }

            GUILayout.EndVertical();
        }

        private static void DrawDepthOfFieldLayer(PostProcessingSettings settings)
        {
            if (settings.depthOfFieldLayer)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch("DEPTH OF FIELD", settings.depthOfFieldLayer.enabled.value, true, enabled => settings.depthOfFieldLayer.active = settings.depthOfFieldLayer.enabled.value = enabled);
                if (settings.depthOfFieldLayer.enabled.value)
                {
                    FocusSettings focusSettings = FocusManager.settings;
                    FocusPuller focusPuller = FocusManager.FocusInstance;

                    GUILayout.Space(30);
                    ToggleAlt("Focus On Object", focusSettings.Enabled, false, focus => { focusSettings.Enabled = focus; FocusManager.UpdateSettings(); });
                    if (focusSettings.Enabled)
                    {
                        GUILayout.Space(10);
                        if (KKAPI.Studio.StudioAPI.InsideStudio)
                        {
                            GUILayout.ExpandHeight(true);
                            GUILayout.BeginHorizontal();
                            if (Button("Focus on selected node", true))
                            {
                                var TreeNodeCtrl = Singleton<Studio.Studio>.Instance.treeNodeCtrl;
                                var p = TreeNodeCtrl.selectNodes[0];
                                if (!Studio.Studio.Instance.dicInfo.TryGetValue(p, out Studio.ObjectCtrlInfo objectCtrlInfo) || // If an object is selected...
                                    !(objectCtrlInfo is Studio.OCIFolder ociFolderOld && ociFolderOld.objectItem.name == "DOF Target")) // make sure that object isn't an existing DOF Target
                                {
                                    // Delete all other DOF Targets
                                    List<Studio.TreeNodeObject> deathNote = new List<Studio.TreeNodeObject>();
                                    foreach (var i in Studio.Studio.Instance.dicInfo)
                                        if (i.Value is Studio.OCIFolder ociFolder && ociFolder.objectItem.name == "DOF Target")
                                            deathNote.Add(i.Key);
                                    foreach (var i in deathNote)
                                        if (i != null)
                                            TreeNodeCtrl.DeleteNode(i);
                                    // Add DOF Target
                                    Singleton<Studio.Studio>.Instance.AddFolder();
                                    var newFolderIndex = TreeNodeCtrl.m_TreeNodeObject.Count - 1; // index of the newly created folder
                                    var n = TreeNodeCtrl.GetNode(newFolderIndex); // select the new folder created
                                    if (Studio.Studio.Instance.dicInfo.TryGetValue(n, out objectCtrlInfo))
                                        if (objectCtrlInfo is Studio.OCIFolder ociFolder)
                                        {
                                            TreeNodeCtrl.SetParent(n, p);
                                            ociFolder.name = "DOF Target";
                                            ociFolder.objectItem.name = "DOF Target";
                                            ociFolder.guideObject.changeAmount.m_Pos = Vector3.zero;
                                            ociFolder.guideObject.changeAmount.onChangePos();
                                        }
                                }
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(10);
                        }

                        GUI.enabled = false;
                        Dimension("Target Position", focusPuller.TargetPosition);
                        GUI.enabled = true;
                        Slider("Focus Speed", focusSettings.Speed.value, 1f, 12f, "N1", speed => { focusSettings.Speed.value = speed; FocusManager.UpdateSettings(); },
                            focusSettings.Speed.overrideState, overrideState => { focusSettings.Speed.overrideState = overrideState; FocusManager.UpdateSettings(); });
                    }
                    Slider("Focal Distance", settings.depthOfFieldLayer.focusDistance.value, 0.1f, 1000f, "N0", focusDistance => settings.depthOfFieldLayer.focusDistance.value = focusDistance,
                        settings.depthOfFieldLayer.focusDistance.overrideState && !focusPuller.enabled, overrideState => settings.depthOfFieldLayer.focusDistance.overrideState = overrideState);
                    GUILayout.Space(30);
                    Label("LENS SETTINGS", "", true);
                    GUILayout.Space(5);
                    Slider("Aperture", settings.depthOfFieldLayer.aperture.value, 1f, 22f, "N1", aperture => settings.depthOfFieldLayer.aperture.value = aperture,
                        settings.depthOfFieldLayer.aperture.overrideState, overrideState => settings.depthOfFieldLayer.aperture.overrideState = overrideState);
                    Slider("Focal Length", settings.depthOfFieldLayer.focalLength.value, 10f, 600f, "N0", focalLength => settings.depthOfFieldLayer.focalLength.value = focalLength,
                        settings.depthOfFieldLayer.focalLength.overrideState, overrideState => settings.depthOfFieldLayer.focalLength.overrideState = overrideState);
                    GUILayout.Space(30);
                    Label("BLUR SETTINGS", "", true);
                    GUILayout.Space(5);
                    Selection("Max Blur Size", settings.depthOfFieldLayer.kernelSize.value, kernelSize => settings.depthOfFieldLayer.kernelSize.value = kernelSize, -1,
                        settings.depthOfFieldLayer.kernelSize.overrideState, overrideState => settings.depthOfFieldLayer.kernelSize.overrideState = overrideState);
                }
                GUILayout.EndVertical();
            }
        }

        private static void DrawGrainLayer()
        {
            //if (settings.grainLayer != null)
            //{
            //    GUILayout.BeginVertical(SmallTab);
            //    Switch("GRAIN", settings.grainLayer.enabled.value, true, enabled => { settings.grainLayer.active = enabled; settings.grainLayer.enabled.Override(enabled); });
            //    if (settings.grainLayer.enabled.value)
            //    {
            //        GUILayout.Space(10);
            //        ToggleAlt("Colored", settings.grainLayer.colored.overrideState, false, overrideState => settings.grainLayer.colored.overrideState = overrideState);
            //        Slider("Intensity", settings.grainLayer.intensity.value, 0f, 1f, "N2", intensity => settings.grainLayer.intensity.value = intensity,
            //            settings.grainLayer.intensity.overrideState, overrideState => settings.grainLayer.intensity.overrideState = overrideState);
            //        Slider("Size", settings.grainLayer.size.value, 0.3f, 3f, "N2", focalLength => settings.grainLayer.size.value = focalLength,
            //            settings.grainLayer.size.overrideState, overrideState => settings.grainLayer.size.overrideState = overrideState);
            //        Slider("Luminance Contribution", settings.grainLayer.lumContrib.value, 0f, 1f, "N2", lumContrib => settings.grainLayer.lumContrib.value = lumContrib,
            //            settings.grainLayer.lumContrib.overrideState, overrideState => settings.grainLayer.lumContrib.overrideState = overrideState);
            //    }
            //    GUILayout.EndVertical();
            //}

            if (FilmGrainManager.settings != null)
            {
                FilmGrainSettings grainLayer = FilmGrainManager.settings;

                GUILayout.BeginVertical(SmallTab);
                Switch("GRAIN", grainLayer.enabled, true, enabled => { grainLayer.enabled = enabled; FilmGrainManager.UpdateSettings(); });

                if (grainLayer.enabled)
                {
                    GUILayout.Space(30);

                    ToggleAlt("Colored", grainLayer.colored.value, false, colored => { grainLayer.colored.value = colored; FilmGrainManager.UpdateSettings(); });

                    Slider("Intensity", grainLayer.intensity.value, 0f, 1f, "N2", intensity => { grainLayer.intensity.value = intensity; FilmGrainManager.UpdateSettings(); },
                        grainLayer.intensity.overrideState, overrideState => { grainLayer.intensity.overrideState = overrideState; FilmGrainManager.UpdateSettings(); });
                    Slider("Size", grainLayer.size.value, 0f, 1f, "N2", size => { grainLayer.size.value = size; FilmGrainManager.UpdateSettings(); },
                        grainLayer.size.overrideState, overrideState => { grainLayer.size.overrideState = overrideState; FilmGrainManager.UpdateSettings(); });
                    Slider("Luminance Contribution", grainLayer.lumContrib.value, 0f, 1f, "N2", lumContrib => { grainLayer.lumContrib.value = lumContrib; FilmGrainManager.UpdateSettings(); },
                        grainLayer.lumContrib.overrideState, overrideState => { grainLayer.lumContrib.overrideState = overrideState; FilmGrainManager.UpdateSettings(); });
                }
                GUILayout.EndVertical();
            }
        }

        private static void DrawVignetteLayer(PostProcessingSettings settings)
        {
            if (settings.vignetteLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch("VIGNETTE", settings.vignetteLayer.enabled.value, true, enabled => settings.vignetteLayer.active = settings.vignetteLayer.enabled.value = enabled);
                if (settings.vignetteLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Selection("Mode", settings.vignetteLayer.mode.value, mode => settings.vignetteLayer.mode.value = mode, -1,
                        settings.vignetteLayer.mode.overrideState, overrideState => settings.vignetteLayer.mode.overrideState = overrideState);
                    SliderColor("Colour", settings.vignetteLayer.color.value, colour => settings.vignetteLayer.color.value = colour, false,
                        settings.vignetteLayer.color.overrideState, overrideState => settings.vignetteLayer.color.overrideState = overrideState);
                    Slider("Intensity", settings.vignetteLayer.intensity, 0f, 1f, "N2", fade => settings.vignetteLayer.intensity.value = fade,
                        settings.vignetteLayer.intensity.overrideState, overrideState => settings.vignetteLayer.intensity.overrideState = overrideState);
                    Slider("Smoothness", settings.vignetteLayer.smoothness.value, 0.01f, 1f, "N2", vignette => settings.vignetteLayer.smoothness.value = vignette,
                        settings.vignetteLayer.smoothness.overrideState, overrideState => settings.vignetteLayer.smoothness.overrideState = overrideState);
                    Slider("Roundness", settings.vignetteLayer.roundness.value, 0f, 1f, "N2", vignette => settings.vignetteLayer.roundness.value = vignette,
                        settings.vignetteLayer.roundness.overrideState, overrideState => settings.vignetteLayer.roundness.overrideState = overrideState);
                    ToggleAlt("Rounded", settings.vignetteLayer.rounded, settings.vignetteLayer.rounded.overrideState, rounded => settings.vignetteLayer.rounded.value = rounded);
                }
                GUILayout.EndVertical();
            }
        }

        private static void DrawMotionBlurLayer(PostProcessingSettings settings)
        {
            if (settings.motionBlurLayer != null)
            {
                GUILayout.BeginVertical(SmallTab);
                Switch("MOTION BLUR", settings.motionBlurLayer.enabled.value, true, enabled => settings.motionBlurLayer.active = settings.motionBlurLayer.enabled.value = enabled);
                if (settings.motionBlurLayer.enabled.value)
                {
                    GUILayout.Space(30);
                    Slider("Shutter Angle", settings.motionBlurLayer.shutterAngle.value, 0f, 360f, "N0", intensity => settings.motionBlurLayer.shutterAngle.value = intensity,
                        settings.motionBlurLayer.shutterAngle.overrideState, overrideState => settings.motionBlurLayer.shutterAngle.overrideState = overrideState);
                    Slider("Sample Count", settings.motionBlurLayer.sampleCount.value, 4, 32, intensity => settings.motionBlurLayer.sampleCount.value = intensity,
                        settings.motionBlurLayer.sampleCount.overrideState, overrideState => settings.motionBlurLayer.sampleCount.overrideState = overrideState);
                }
                GUILayout.EndVertical();
            }
        }

        private static void DrawGlobalFogLayer()
        {
            if (GlobalFogManager.settings != null)
            {
                GlobalFogSettings globalfogSettings = GlobalFogManager.settings;
                GUILayout.BeginVertical(SmallTab);
                Switch("GLOBAL FOG", globalfogSettings.Enabled, true, enabled =>
                {
                    globalfogSettings.Enabled = enabled;
                    GlobalFogManager.UpdateSettings();

                });
                if (globalfogSettings.Enabled)
                {
                    GUILayout.Space(30);
                    ToggleAlt("Distance Fog", globalfogSettings.distanceFog.value, false, distanceFog => { globalfogSettings.distanceFog.value = distanceFog; GlobalFogManager.UpdateSettings(); });
                    ToggleAlt("Exclude Far Pixels", globalfogSettings.excludeFarPixels.value, false, excludeFarPixels => { globalfogSettings.excludeFarPixels.value = excludeFarPixels; GlobalFogManager.UpdateSettings(); });
                    ToggleAlt("Use Radial Distance", globalfogSettings.useRadialDistance.value, false, useRadialDistance => { globalfogSettings.useRadialDistance.value = useRadialDistance; GlobalFogManager.UpdateSettings(); });

                    Slider("Height", globalfogSettings.height.value, 1f, 100f, "N0", height => { globalfogSettings.height.value = height; GlobalFogManager.UpdateSettings(); });
                    Slider("Height Density", globalfogSettings.heightDensity.value, 0.001f, 10f, "N2", density => { globalfogSettings.heightDensity.value = density; GlobalFogManager.UpdateSettings(); });
                    Slider("Start Distance", globalfogSettings.startDistance.value, 1f, 100f, "N0", start => { globalfogSettings.startDistance.value = start; GlobalFogManager.UpdateSettings(); });

                    SliderColor("Fog Color", globalfogSettings.fogColor, colour => { globalfogSettings.fogColor = colour; GlobalFogManager.UpdateSettings(); });
                    Selection("Fog Mode", globalfogSettings.fogMode, mode => { globalfogSettings.fogMode = mode; GlobalFogManager.UpdateSettings(); });
                }

                GUILayout.EndVertical();
            }
        }

        private static void DrawLuxWaterLayer(LightManager lightManager)
        {
            if (LuxWater_UnderWaterRenderingManager.settings != null)
            {
                UnderWaterRenderingSettings underwaterSettings = LuxWater_UnderWaterRenderingManager.settings;
                WaterVolumeTriggerSettings triggerSettings = LuxWater_WaterVolumeTriggerManager.settings;
                ConnectSunToUnderwaterSettings connectSettings = ConnectSunToUnderwaterManager.settings;
                UnderWaterBlurSettings blurSettings = LuxWater_UnderwaterBlurManager.settings;
                GUILayout.BeginVertical(SmallTab);

                Switch("UNDERWATER RENDERING", underwaterSettings.Enabled, true, enabled =>
                {
                    underwaterSettings.Enabled = enabled;
                    triggerSettings.Enabled = enabled;
                    connectSettings.Enabled = enabled;
                    LuxWater_UnderWaterRenderingManager.UpdateSettings();
                    LuxWater_WaterVolumeTriggerManager.UpdateSettings();
                    ConnectSunToUnderwaterManager.UpdateSettings();
                });

                if (underwaterSettings.Enabled)
                {
                    GUILayout.Space(30);
                    Label("Info", "Requires Water Volume");
                    GUILayout.Space(5);
                    LightSelector(lightManager, "Sun Source", RenderSettings.sun, light =>
                    {
                        RenderSettings.sun = light;
                        ConnectSunToUnderwater.ConnectSun();
                    });

                    GUILayout.Space(30);
                    ToggleAlt("Underwater Blur", blurSettings.Enabled, false, underwaterBlur => { blurSettings.Enabled = underwaterBlur; LuxWater_UnderwaterBlurManager.UpdateSettings(); });
                    if (blurSettings.Enabled)
                    {
                        Slider("Blur Spread", blurSettings.BlurSpread.value, 0.1f, 1f, "N2", spread => { blurSettings.BlurSpread.value = spread; LuxWater_UnderwaterBlurManager.UpdateSettings(); });
                        Slider("Blur Down Sample", blurSettings.BlurDownSample.value, 1, 8, downSample => { blurSettings.BlurDownSample.value = downSample; LuxWater_UnderwaterBlurManager.UpdateSettings(); });
                        Slider("Blur Iterations", blurSettings.BlurIterations.value, 1, 8, iterations => { blurSettings.BlurIterations.value = iterations; LuxWater_UnderwaterBlurManager.UpdateSettings(); });
                    }
                }

                GUILayout.EndVertical();
            }
        }

        private static void DrawAuraLayer()
        {
            if (AuraManager.Available && AuraManager.settings != null)
            {
                AuraSettings auralayer = AuraManager.settings;

                GUILayout.BeginVertical(SmallTab);
                Switch("AURA 2 Volumetric Lighting & Fog", auralayer.Enabled, true, enabled => { auralayer.Enabled = enabled; AuraManager.UpdateSettings(); });

                if (auralayer.Enabled)
                {
                    GUILayout.Space(30);
                    Label("BASE SETTINGS", "", true);
                    GUILayout.Space(5);
                    ToggleAlt("Use Density", auralayer.useDensity.value, false, useDensity => { auralayer.useDensity.value = useDensity; AuraManager.UpdateSettings(); });
                    if (auralayer.useDensity.value)
                    {
                        Slider("Density", auralayer.density.value, 0.0f, 1.0f, "N2", density => { auralayer.density.value = density; AuraManager.UpdateSettings(); },
                            auralayer.density.overrideState, overrideState => { auralayer.density.overrideState = overrideState; AuraManager.UpdateSettings(); });
                    }

                    ToggleAlt("Use Scattering", auralayer.useScattering.value, false, useScattering => { auralayer.useScattering.value = useScattering; AuraManager.UpdateSettings(); });
                    if (auralayer.useScattering.value)
                    {
                        Slider("Scattering", auralayer.scattering.value, 0.0f, 1.0f, "N2", scattering => { auralayer.scattering.value = scattering; AuraManager.UpdateSettings(); },
                            auralayer.scattering.overrideState, overrideState => { auralayer.scattering.overrideState = overrideState; AuraManager.UpdateSettings(); });
                    }

                    ToggleAlt("Use Ambient Lighting", auralayer.useAmbientLighting.value, false, useAmbientLighting => { auralayer.useAmbientLighting.value = useAmbientLighting; AuraManager.UpdateSettings(); });
                    if (auralayer.useAmbientLighting.value)
                    {
                        Slider("Ambient Lighting Strength", auralayer.ambientLightingStrength.value, 0.0f, 1.0f, "N2", ambientLightingStrength => { auralayer.ambientLightingStrength.value = ambientLightingStrength; AuraManager.UpdateSettings(); },
                            auralayer.ambientLightingStrength.overrideState, overrideState => { auralayer.ambientLightingStrength.overrideState = overrideState; AuraManager.UpdateSettings(); });
                    }

                    ToggleAlt("Use Color", auralayer.useColor.value, false, useColor => { auralayer.useColor.value = useColor; AuraManager.UpdateSettings(); });
                    if (auralayer.useColor.value)
                    {
                        SliderColor("Color Tint", auralayer.color, colortint => { auralayer.color = colortint; AuraManager.UpdateSettings(); });
                        Slider("Color Strength", auralayer.colorStrength.value, 0.0f, 1.0f, "N2", colorStrength => { auralayer.colorStrength.value = colorStrength; AuraManager.UpdateSettings(); },
                            auralayer.colorStrength.overrideState, overrideState => { auralayer.colorStrength.overrideState = overrideState; AuraManager.UpdateSettings(); });
                    }

                    ToggleAlt("Use Tint", auralayer.useTint.value, false, useTint => { auralayer.useTint.value = useTint; AuraManager.UpdateSettings(); });
                    if (auralayer.useTint.value)
                    {
                        SliderColor("Tint", auralayer.tint, tint => { auralayer.tint = tint; AuraManager.UpdateSettings(); });
                        Slider("Tint Strength", auralayer.tintStrength.value, 0.0f, 1.0f, "N2", tintStrength => { auralayer.tintStrength.value = tintStrength; AuraManager.UpdateSettings(); },
                            auralayer.tintStrength.overrideState, overrideState => { auralayer.tintStrength.overrideState = overrideState; AuraManager.UpdateSettings(); });
                    }

                    ToggleAlt("Use Extinction", auralayer.useExtinction.value, false, useExtinction => { auralayer.useExtinction.value = useExtinction; AuraManager.UpdateSettings(); });
                    if (auralayer.useExtinction.value)
                    {
                        Slider("Extinction", auralayer.extinction.value, 0.0f, 1.0f, "N2", extinction => { auralayer.extinction.value = extinction; AuraManager.UpdateSettings(); },
                            auralayer.extinction.overrideState, overrideState => { auralayer.extinction.overrideState = overrideState; AuraManager.UpdateSettings(); });
                    }

                    GUILayout.Space(30);
                    Label("QUALITY SETTINGS", "", true);
                    GUILayout.Space(5);

                    Slider("Far Clip Plane Distance", auralayer.farClipPlaneDistance.value, 0.0f, 256.0f, "N2", farClipPlaneDistance => { auralayer.farClipPlaneDistance.value = farClipPlaneDistance; AuraManager.UpdateSettings(); },
                        auralayer.farClipPlaneDistance.overrideState, overrideState => { auralayer.farClipPlaneDistance.overrideState = overrideState; AuraManager.UpdateSettings(); });
                    Slider("Depth Bias Coefficient", auralayer.depthBiasCoefficient.value, 0.0f, 1.0f, "N2", depthBiasCoefficient => { auralayer.depthBiasCoefficient.value = depthBiasCoefficient; AuraManager.UpdateSettings(); },
                        auralayer.depthBiasCoefficient.overrideState, overrideState => { auralayer.depthBiasCoefficient.overrideState = overrideState; AuraManager.UpdateSettings(); });

                    ToggleAlt("Enable Denoising Filter", auralayer.EXPERIMENTAL_enableDenoisingFilter.value, false, enableDenoisingFilter => { auralayer.EXPERIMENTAL_enableDenoisingFilter.value = enableDenoisingFilter; AuraManager.UpdateSettings(); });

                    if (auralayer.EXPERIMENTAL_enableDenoisingFilter.value)
                    {
                        Selection("Denoising Filter Range", auralayer.EXPERIMENTAL_denoisingFilterRange, denoisingFilterRange => { auralayer.EXPERIMENTAL_denoisingFilterRange = denoisingFilterRange; AuraManager.UpdateSettings(); }, 3);
                    }
                    ToggleAlt("Enable Blur Filter", auralayer.EXPERIMENTAL_enableBlurFilter.value, false, enableBlurFilter => { auralayer.EXPERIMENTAL_enableBlurFilter.value = enableBlurFilter; AuraManager.UpdateSettings(); });
                    if (auralayer.EXPERIMENTAL_enableBlurFilter.value)
                    {
                        Selection("Blur Filter Range", auralayer.EXPERIMENTAL_blurFilterRange, blurFilterRange => { auralayer.EXPERIMENTAL_blurFilterRange = blurFilterRange; AuraManager.UpdateSettings(); }, 3);
                        Selection("Blur Filter Type", auralayer.EXPERIMENTAL_blurFilterType, blurFilterType => { auralayer.EXPERIMENTAL_blurFilterType = blurFilterType; AuraManager.UpdateSettings(); }, 3);
                        Slider("Blur Filter Gaussian Deviation", auralayer.EXPERIMENTAL_blurFilterGaussianDeviation.value, 0.0f, 0.1f, "N2", blurFilterGaussianDeviation => { auralayer.EXPERIMENTAL_blurFilterGaussianDeviation.value = blurFilterGaussianDeviation; AuraManager.UpdateSettings(); },
                            auralayer.EXPERIMENTAL_blurFilterGaussianDeviation.overrideState, overrideState => { auralayer.EXPERIMENTAL_blurFilterGaussianDeviation.overrideState = overrideState; AuraManager.UpdateSettings(); });
                    }

                    GUILayout.Space(30);
                    Label("DEBUG", "", true);
                    GUILayout.Space(5);
                    ToggleAlt("Display Volumetric Lighting Buffer", auralayer.displayVolumetricLightingBuffer.value, false, displayVolumetricLightingBuffer => { auralayer.displayVolumetricLightingBuffer.value = displayVolumetricLightingBuffer; AuraManager.UpdateSettings(); });
                }
                GUILayout.EndVertical();
            }
        }
    }
}
