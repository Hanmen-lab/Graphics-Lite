using Graphics.CTAA;
using Graphics.FSR3;
using Graphics.Settings;
using UnityEngine;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class AntiAliasingInspector
    {
        private static Vector2 aaScrollView;
        private static int cachedFontSize = -1;
        private static int paddingL;
        private static GUIStyle TabContent, TabContent2;

        static void UpdateCachedValues(GlobalSettings renderSettings)
        {
            if (cachedFontSize == renderSettings.FontSize) return;

            cachedFontSize = renderSettings.FontSize;

            paddingL = Mathf.RoundToInt(renderSettings.FontSize * 2f);

            TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(paddingL, paddingL, paddingL, paddingL);


            TabContent2 = new GUIStyle(GUIStyles.tabcontent);
            TabContent2.padding = new RectOffset(paddingL, paddingL, 0, paddingL);
        }

        internal static void Draw(GlobalSettings renderSettings, CameraSettings cameraSettings, PostProcessingSettings postProcessingSettings, PostProcessingManager postprocessingManager, bool showAdvanced)
        {

            UpdateCachedValues(renderSettings);

            GUILayout.BeginVertical(TabContent);
            {
                Label("POST PROCESS AA", "", true);
                GUILayout.Space(2);
                Selection("Mode", postProcessingSettings.AntialiasingMode, mode => { postProcessingSettings.AntialiasingMode = mode; postProcessingSettings.UpdateFilterDithering(); CTAAManager.UpdateSettings(); FSR3Manager.UpdateSettings(); });
                GUILayout.Space(20);

                GUILayout.EndVertical();
                aaScrollView = GUILayout.BeginScrollView(aaScrollView);
                GUILayout.BeginVertical(TabContent2);
                if (PostProcessingSettings.Antialiasing.None == postProcessingSettings.AntialiasingMode)
                {
                    Label("", "", true);
                }
                if (PostProcessingSettings.Antialiasing.SMAA == postProcessingSettings.AntialiasingMode)
                {
                    Label("SETTINGS", "", true);
                    GUILayout.Space(2);
                    Selection("SMAA Quality", postProcessingSettings.SMAAQuality, quality => postProcessingSettings.SMAAQuality = quality);
                    Shader.DisableKeyword("_TEMPORALFILTER_ON");
                }
                else if (PostProcessingSettings.Antialiasing.TAA == postProcessingSettings.AntialiasingMode)
                {
                    Label("SETTINGS", "", true);
                    GUILayout.Space(2);
                    Slider("Jitter Spread", postProcessingSettings.JitterSpread, 0.1f, 1f, "N2", spread => { postProcessingSettings.JitterSpread = spread; });
                    Slider("Stationary Blending", postProcessingSettings.StationaryBlending, 0f, 1f, "N2", sblending => { postProcessingSettings.StationaryBlending = sblending; });
                    Slider("Motion Blending", postProcessingSettings.MotionBlending, 0f, 1f, "N2", mblending => { postProcessingSettings.MotionBlending = mblending; });
                    Slider("Sharpness", postProcessingSettings.Sharpness, 0f, 3f, "N2", sharpness => { postProcessingSettings.Sharpness = sharpness; });
                    GUILayout.Space(10);
                    ToggleAlt("FILTER DITHERING", postProcessingSettings.FilterDithering, true, filter => { postProcessingSettings.FilterDithering = filter; postProcessingSettings.UpdateFilterDithering(); });
                    GUILayout.Space(10);
                    Label("Tips:", "Decrease 'Motion Blending' to around 0.1-0.3 if you have ghosting.", false);
                    Label("", "Decrease 'Jitter Spread' if you have problem with pantyhose/tight clothing flickering.", false);
                }
                else if (PostProcessingSettings.Antialiasing.FXAA == postProcessingSettings.AntialiasingMode)
                {
                    Label("SETTINGS", "", true);
                    GUILayout.Space(2);
                    Toggle("Fast Mode", postProcessingSettings.FXAAMode, false, fxaa => postProcessingSettings.FXAAMode = fxaa);
                    Toggle("Keep Alpha", postProcessingSettings.FXAAAlpha, false, alpha => postProcessingSettings.FXAAAlpha = alpha);
                    Shader.DisableKeyword("_TEMPORALFILTER_ON");
                }
                else if (PostProcessingSettings.Antialiasing.FSR3 == postProcessingSettings.AntialiasingMode)
                {
                    if (FSR3Manager.Settings != null)
                    {
                        FSR3Settings fsr3Settings = FSR3Manager.Settings;

                        if (fsr3Settings.Enabled)
                        {
                            if (!Graphics.ScreenshotOverride.Value)
                            {
                                //GUILayout.Space(15);
                                Warning("Screenshot Enghine Not Enabled! Rendered screenshot will be incorrect! Toggle 'Enable New Screenshot Engine' it in the F1 - Graphics Plugin settings.", true);
                                GUILayout.Space(5);
                            }
                            Label("SETTINGS", "", true);
                            GUILayout.Space(10);
                            //Selection("Quality Mode", fsr3Settings.QualityMode, value => { fsr3Settings.QualityMode = value; FSR3Manager.UpdateSettings(); FSR3HelperManager.UpdateSettings(); }, 3);
                            Selection("Quality Mode", fsr3Settings.QualityMode, value => { fsr3Settings.QualityMode = value; FSR3Manager.UpdateSettings(); }, 3);
                            GUILayout.Space(10);
                            Label("", "Resolution: NativveAA: 100%, UltraQuality: 83%, Quality: 66%, Balanced: 58%, Performance: 50%, UltraPerformance: 33%", false);
                            GUILayout.Space(10);
                            ToggleAlt("Perform Sharpen Pass", fsr3Settings.PerformSharpenPass.value, false, value => { fsr3Settings.PerformSharpenPass.value = value; FSR3Manager.UpdateSettings(); });
                            if (fsr3Settings.PerformSharpenPass.value)
                            {
                                GUILayout.Space(10);
                                Slider("Sharpness", fsr3Settings.Sharpness.value, 0f, 1f, "N2", value => { fsr3Settings.Sharpness.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.Sharpness.overrideState, overrideState => { fsr3Settings.Sharpness.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                                Slider("Velocity Factor", fsr3Settings.VelocityFactor.value, 0f, 1f, "N2", value => { fsr3Settings.VelocityFactor.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.VelocityFactor.overrideState, overrideState => { fsr3Settings.VelocityFactor.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                            }

                            GUILayout.Space(30);
                            Label("EXPOSURE", "", true);
                            GUILayout.Space(10);
                            ToggleAlt("Enable Auto Exposure", fsr3Settings.EnableAutoExposure.value, false, value => { fsr3Settings.EnableAutoExposure.value = value; FSR3Manager.UpdateSettings(); });
                            Text("Pre Exposure", fsr3Settings.PreExposure.value, "N2", value => { fsr3Settings.PreExposure.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.PreExposure.overrideState, overrideState => { fsr3Settings.PreExposure.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                            //Label("Exposure", "dummy for Texture", false);

                            GUILayout.Space(30);
                            Label("TRANSPARENCY & COMPOSITION", "", true);
                            GUILayout.Space(10);
                            //Label("Reactive Mask", "dummy for Texture", false);
                            //Label("Transparency And Composition Mask", "dummy for Texture", false);
                            ToggleAlt("Auto Reactive Mask", fsr3Settings.AutoGenerateReactiveMask.value, false, value => { fsr3Settings.AutoGenerateReactiveMask.value = value; FSR3Manager.UpdateSettings(); });
                            if (fsr3Settings.AutoGenerateReactiveMask.value)
                            {
                                GUILayout.Space(10);
                                Slider("Scale", fsr3Settings.Scale.value, 0f, 2f, "N2", value => { fsr3Settings.Scale.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.Scale.overrideState, overrideState => { fsr3Settings.Scale.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                                Slider("Cutoff Threshold", fsr3Settings.CutoffThreshold.value, 0f, 1f, "N2", value => { fsr3Settings.CutoffThreshold.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.CutoffThreshold.overrideState, overrideState => { fsr3Settings.CutoffThreshold.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                                Slider("Binary Value", fsr3Settings.BinaryValue.value, 0f, 1f, "N2", value => { fsr3Settings.BinaryValue.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.BinaryValue.overrideState, overrideState => { fsr3Settings.BinaryValue.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                                GUILayout.Space(5);
                                MultiSelection("Flags", fsr3Settings.Flags, value => { fsr3Settings.Flags = value; FSR3Manager.UpdateSettings(); }, 2);
                            }
                            GUILayout.Space(30);
                            Label("EXPERIMENTAL", "Automatically generate and use Reactive mask and Transparency & composition mask internally.", true);
                            GUILayout.Space(10);
                            ToggleAlt("Auto Transparency And Composition", fsr3Settings.AutoGenerateTransparencyAndComposition.value, false, value => { fsr3Settings.AutoGenerateTransparencyAndComposition.value = value; FSR3Manager.UpdateSettings(); });
                            if (fsr3Settings.AutoGenerateTransparencyAndComposition.value)
                            {
                                GUILayout.Space(10);
                                Slider("Auto Tc Threshold", fsr3Settings.AutoTcThreshold.value, 0f, 1f, "N2", value => { fsr3Settings.AutoTcThreshold.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.AutoTcThreshold.overrideState, overrideState => { fsr3Settings.AutoTcThreshold.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                                Slider("Auto Tc Scale", fsr3Settings.AutoTcScale.value, 0f, 2f, "N2", value => { fsr3Settings.AutoTcScale.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.AutoTcScale.overrideState, overrideState => { fsr3Settings.AutoTcScale.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                                Slider("Auto Reactive Scale", fsr3Settings.AutoReactiveScale.value, 0f, 10f, "N2", value => { fsr3Settings.AutoReactiveScale.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.AutoReactiveScale.overrideState, overrideState => { fsr3Settings.AutoReactiveScale.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                                Slider("Auto Reactive Max", fsr3Settings.AutoReactiveMax.value, 0f, 1f, "N2", value => { fsr3Settings.AutoReactiveMax.value = value; FSR3Manager.UpdateSettings(); }, fsr3Settings.AutoReactiveMax.overrideState, overrideState => { fsr3Settings.AutoReactiveMax.overrideState = overrideState; FSR3Manager.UpdateSettings(); });
                            }
                            GUILayout.Space(30);
                            Label("DEBUG", "", true);
                            GUILayout.Space(10);
                            ToggleAlt("Enable Debug View", fsr3Settings.EnableDebugView.value, false, value => { fsr3Settings.EnableDebugView.value = value; FSR3Manager.UpdateSettings(); });
                            //GUILayout.Space(10);
                            //Label("Assets", "dummy for Assets", false);
                            GUILayout.Space(10);
                            ToggleAlt("FILTER DITHERING", postProcessingSettings.FilterDithering, true, value => { postProcessingSettings.FilterDithering = value; postProcessingSettings.UpdateFilterDithering(); });
                        }
                    }
                }
                else if (PostProcessingSettings.Antialiasing.CTAA == postProcessingSettings.AntialiasingMode)
                {

                    if (CTAAManager.settings != null)
                    {
                        CTAASettings ctaaSettings = CTAAManager.settings;
                        TemporalScreenshotTool screenshot = Graphics.Instance.CameraSettings.MainCamera.GetComponent<TemporalScreenshotTool>();

                        if (ctaaSettings.Enabled)
                        {
                            if (!Graphics.ScreenshotOverride.Value)
                            {
                                //GUILayout.Space(15);
                                Warning("Screenshot Enghine Not Enabled! Rendered screenshot will be incorrect! Toggle 'Enable New Screenshot Engine' it in the F1 - Graphics Plugin settings.", true);
                                GUILayout.Space(5);
                            }
                            Label("SETTINGS", "", true);
                            GUILayout.Space(10);
                            Slider("Temporal Stability", ctaaSettings.TemporalStability.value, 3, 16,
                                stability => { ctaaSettings.TemporalStability.value = stability; CTAAManager.UpdateSettings(); },
                                ctaaSettings.TemporalStability.overrideState,
                                overrideState => { ctaaSettings.TemporalStability.overrideState = overrideState; CTAAManager.UpdateSettings(); });
                            Slider("HDR Response", ctaaSettings.HdrResponse.value, 0.001f, 4f, "N2",
                                hdrResponse => { ctaaSettings.HdrResponse.value = hdrResponse; CTAAManager.UpdateSettings(); },
                                ctaaSettings.HdrResponse.overrideState,
                                overrideState => { ctaaSettings.HdrResponse.overrideState = overrideState; CTAAManager.UpdateSettings(); });
                            Slider("Edge Response", ctaaSettings.EdgeResponse.value, 0f, 2f, "N1",
                                edgeResponse => { ctaaSettings.EdgeResponse.value = edgeResponse; CTAAManager.UpdateSettings(); },
                                ctaaSettings.EdgeResponse.overrideState,
                                overrideState => { ctaaSettings.EdgeResponse.overrideState = overrideState; CTAAManager.UpdateSettings(); });
                            Slider("Adaptive Sharpness", ctaaSettings.AdaptiveSharpness.value, 0f, 1.5f, "N1",
                                adaptiveSharpness => { ctaaSettings.AdaptiveSharpness.value = adaptiveSharpness; CTAAManager.UpdateSettings(); },
                                ctaaSettings.AdaptiveSharpness.overrideState,
                                overrideState => { ctaaSettings.AdaptiveSharpness.overrideState = overrideState; CTAAManager.UpdateSettings(); });
                            Slider("Temporal Jitter Scale", ctaaSettings.TemporalJitterScale.value, 0f, 0.5f, "N2",
                                temporalJitterScale => { ctaaSettings.TemporalJitterScale.value = temporalJitterScale; CTAAManager.UpdateSettings(); },
                                ctaaSettings.TemporalJitterScale.overrideState,
                                overrideState => { ctaaSettings.TemporalJitterScale.overrideState = overrideState; CTAAManager.UpdateSettings(); });
                            //GUILayout.Space(10);
                            ToggleAlt("Filter Dithering", postProcessingSettings.FilterDithering, false, filter => { postProcessingSettings.FilterDithering = filter; postProcessingSettings.UpdateFilterDithering(); });
                            GUILayout.Space(20);
                            Label("SUPER SAMPLING", "", true);
                            GUILayout.Space(5);
                            Selection("Super Sampling Mode", ctaaSettings.SupersampleMode, mode => { ctaaSettings.SupersampleMode = mode; CTAAManager.UpdateSettings(); });
                            GUILayout.Space(20);
                            Label("SCREENSHOT", "", true);
                            GUILayout.Space(5);
                            ToggleWithText("Enable Supersampling", screenshot.enableCTAASuperSampling, "Auto set Supersampling mode to CINA_ULTRA while render screenshot (F11). Screenshot Manager supersampling value ignored for CTAA.", false, value => { screenshot.enableCTAASuperSampling = value; });
                            GUILayout.Space(5);

                            //if (ctaaSettings.SupersampleMode != CTAASettings.CTAA_MODE.STANDARD)
                            //{
                            //    GUILayout.Space(10);
                            //    Label("Warning!", "CINA SOFT & CINA ULTRA not working with rendered screenshots F11, use normal screenshots F9 instead.", false);
                            //}
                            //else
                            //{
                            //    GUILayout.Space(10);
                            //    Label("Warning!", "Rendered Screenshots F11 with Upsampling or Custom res may be blurry than usual. Use F9 instead if possible.", false);
                            //}
                            //GUILayout.Space(20);
                            //Label("EXPERIMENTAL FEATURE", "", true);
                            //GUILayout.Space(2);
                            //Toggle("Anti Shimmer", ctaaSettings.AntiShimmerMode.value, false, antiShimmer => { ctaaSettings.AntiShimmerMode.value = antiShimmer; CTAAManager.UpdateSettings(); });
                            ////GUILayout.Space(10);
                            //Label("Warning!", "Suitable only for static visualisation, CAD or non-animated objects. Camera can be moved.", false);
                            //Label("", "Will reduce micro shimmer, but cause heavy ghosting if used with animated objects.", false);
                            //Label("", "Can compeletely reduce flickering with Advanced Depth of Field.", false);
                        }
                    }
                }

                //GUILayout.Space(30);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.Space(3);
            GUILayout.BeginVertical(TabContent);
            {
                if (Graphics.Instance.CameraSettings.RenderingPath == CameraSettings.AIRenderingPath.Deferred)
                {
                    GUI.enabled = false;
                }
                Switch("MSAA", cameraSettings.MSAA, true, msaa => cameraSettings.MSAA = msaa);
                GUILayout.Space(30);
                Slider("MSAA Multiplier", renderSettings.AntiAliasing, 0, 8, aa => renderSettings.AntiAliasing = aa);
                Label("Forward Only", "This is a separate type of anti-aliasing which can be combined with the post-processing AA above.", false);
                Label("", "Performance intensive! Good for capturing not for actual gameplay.", false);
                GUILayout.Space(30);
                GUI.enabled = true;
            }
            GUILayout.EndVertical();
        }
    }
}
