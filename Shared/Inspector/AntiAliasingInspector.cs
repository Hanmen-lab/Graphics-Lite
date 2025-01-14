using Graphics.CTAA;
using Graphics.Settings;
using UnityEngine;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class AntiAliasingInspector
    {
        internal static void Draw(GlobalSettings renderSettings, CameraSettings cameraSettings, PostProcessingSettings postProcessingSettings, PostProcessingManager postprocessingManager, bool showAdvanced)
        {
            GUIStyle TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2.9f), Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f));

            GUILayout.BeginVertical(TabContent);
            {
                Label("POST PROCESS AA", "", true);
                GUILayout.Space(2);
                Selection("Mode", postProcessingSettings.AntialiasingMode, mode => { postProcessingSettings.AntialiasingMode = mode; postProcessingSettings.UpdateFilterDithering(); CTAAManager.UpdateSettings(); });
                GUILayout.Space(20);

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
                else if (PostProcessingSettings.Antialiasing.CTAA == postProcessingSettings.AntialiasingMode)
                {
                    if (CTAAManager.settings != null)
                    {
                        CTAASettings ctaaSettings = CTAAManager.settings;

                        if (ctaaSettings.Enabled)
                        {
                            Label("SETTINGS", "", true);
                            GUILayout.Space(10);
                            Slider("Temporal Stability", ctaaSettings.TemporalStability.value, 3, 16,
                                stability => { ctaaSettings.TemporalStability.value = stability; CTAAManager.UpdateSettings(); },
                                ctaaSettings.TemporalStability.overrideState,
                                overrideState => { ctaaSettings.TemporalStability.overrideState = overrideState; CTAAManager.UpdateSettings(); });
                            Slider("HDR Response", ctaaSettings.HdrResponse.value, 0.001f, 4f, "N3",
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
                            Slider("Temporal Jitter Scale", ctaaSettings.TemporalJitterScale.value, 0f, 0.5f, "N3",
                                temporalJitterScale => { ctaaSettings.TemporalJitterScale.value = temporalJitterScale; CTAAManager.UpdateSettings(); },
                                ctaaSettings.TemporalJitterScale.overrideState,
                                overrideState => { ctaaSettings.TemporalJitterScale.overrideState = overrideState; CTAAManager.UpdateSettings(); });
                            //GUILayout.Space(10);
                            ToggleAlt("Filter Dithering", postProcessingSettings.FilterDithering, false, filter => { postProcessingSettings.FilterDithering = filter; postProcessingSettings.UpdateFilterDithering(); });
                            GUILayout.Space(20);
                            Label("UPSCALING", "", true);
                            GUILayout.Space(2);
                            Selection("Upscale Mode", ctaaSettings.SupersampleMode, mode => { ctaaSettings.SupersampleMode = mode; CTAAManager.UpdateSettings(); });
                            if (ctaaSettings.SupersampleMode != CTAASettings.CTAA_MODE.STANDARD)
                            {
                                GUILayout.Space(10);
                                Label("Warning!", "CINA SOFT & CINA ULTRA not working with rendered screenshots F11, use normal screenshots F9 instead.", false);
                            }
                            else
                            {
                                GUILayout.Space(10);
                                Label("Warning!", "Rendered Screenshots F11 with Upsampling or Custom res may be blurry than usual. Use F9 instead if possible.", false);
                            }
                            GUILayout.Space(20);
                            Label("EXPERIMENTAL FEATURE", "", true);
                            GUILayout.Space(2);
                            Toggle("Anti Shimmer", ctaaSettings.AntiShimmerMode.value, false, antiShimmer => { ctaaSettings.AntiShimmerMode.value = antiShimmer; CTAAManager.UpdateSettings(); });
                            //GUILayout.Space(10);
                            Label("Warning!", "Suitable only for static visualisation, CAD or non-animated objects. Camera can be moved.", false);
                            Label("", "Will reduce micro shimmer, but cause heavy ghosting if used with animated objects.", false);
                            Label("", "Can compeletely reduce flickering with Advanced Depth of Field.", false);
                        }
                    }
                }

                GUILayout.Space(30);
            }
            GUILayout.EndVertical();
            GUILayout.Space(3);
            GUILayout.BeginVertical(TabContent);
            {
                if (Graphics.Instance.CameraSettings.RenderingPath == CameraSettings.AIRenderingPath.Deferred)
                {
                    GUI.enabled = false;
                }
                Switch(renderSettings.FontSize, "MSAA", cameraSettings.MSAA, true, msaa => cameraSettings.MSAA = msaa);
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
