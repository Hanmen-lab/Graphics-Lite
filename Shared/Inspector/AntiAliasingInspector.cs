using Graphics.CTAA;
using Graphics.Settings;
using UnityEngine;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class AntiAliasingInspector
    {
        //private static bool FilterDithering;
        internal static void Draw(GlobalSettings renderSettings, CameraSettings cameraSettings, PostProcessingSettings postProcessingSettings, PostProcessingManager postprocessingManager, bool showAdvanced)
        {

            GUILayout.BeginVertical(GUIStyles.tabcontent);
            {
                Label("POST PROCESS AA", "", true);
                GUILayout.Space(2);
                Selection("Mode", postProcessingSettings.AntialiasingMode, mode => postProcessingSettings.AntialiasingMode = mode);
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
                    ToggleAlt("FILTER DITHERING", postProcessingSettings.FilterDithering, true, filter => postProcessingSettings.FilterDithering = filter);
                    if (postProcessingSettings.FilterDithering)
                    {
                        Shader.EnableKeyword("_TEMPORALFILTER_ON");
                    }
                    else
                    {
                        Shader.DisableKeyword("_TEMPORALFILTER_ON");
                    }
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
                    Label("SETTINGS", "", true);
                    GUILayout.Space(10);
                    Slider("Temporal Stability", CTAAManager.settings.TemporalStability.value, 3, 16,
                        stability => CTAAManager.settings.TemporalStability.value = stability,
                        CTAAManager.settings.TemporalStability.overrideState,
                        overrideState => CTAAManager.settings.TemporalStability.overrideState = overrideState);
                    Slider("HDR Response", CTAAManager.settings.HdrResponse.value, 0.001f, 4f, "N3",
                        hdrResponse => CTAAManager.settings.HdrResponse.value = hdrResponse,
                        CTAAManager.settings.HdrResponse.overrideState,
                        overrideState => CTAAManager.settings.HdrResponse.overrideState = overrideState);
                    Slider("Edge Response", CTAAManager.settings.EdgeResponse.value, 0f, 2f, "N1",
                        edgeResponse => CTAAManager.settings.EdgeResponse.value = edgeResponse,
                        CTAAManager.settings.EdgeResponse.overrideState,
                        overrideState => CTAAManager.settings.EdgeResponse.overrideState = overrideState);
                    Slider("Adaptive Sharpness", CTAAManager.settings.AdaptiveSharpness.value, 0f, 1.5f, "N1",
                        adaptiveSharpness => CTAAManager.settings.AdaptiveSharpness.value = adaptiveSharpness,
                        CTAAManager.settings.AdaptiveSharpness.overrideState,
                        overrideState => CTAAManager.settings.AdaptiveSharpness.overrideState = overrideState);
                    Slider("Temporal Jitter Scale", CTAAManager.settings.TemporalJitterScale.value, 0f, 0.5f, "N3",
                        temporalJitterScale => CTAAManager.settings.TemporalJitterScale.value = temporalJitterScale,
                        CTAAManager.settings.TemporalJitterScale.overrideState,
                        overrideState => CTAAManager.settings.TemporalJitterScale.overrideState = overrideState);
                    GUILayout.Space(10);
                    Label("Warning!", "Don't use with Rendered Screenshot (F11)! with 1.0+ upsampling. Will cause blurry artifacts.", false);
                    Label("", "Decrease 'Temporal Jitter Scale' if you have problem with pantyhose/tight clothing flickering.", false);
                    Selection("Mode", CTAAManager.settings.Mode, mode => CTAAManager.settings.Mode = mode);
                    if (CTAAManager.settings.Mode > 0)
                        GUILayout.Space(10);
                    ToggleAlt("FILTER DITHERING", postProcessingSettings.FilterDithering, true, filter => postProcessingSettings.FilterDithering = filter);
                    if (postProcessingSettings.FilterDithering)
                    {
                        Shader.EnableKeyword("_TEMPORALFILTER_ON");
                    }
                    else
                    {
                        Shader.DisableKeyword("_TEMPORALFILTER_ON");
                    }

                    CTAAManager.settings.Load(Graphics.Instance.CameraSettings.MainCamera.GetComponent<CTAA_PC>());
                }
                else if (PostProcessingSettings.Antialiasing.None == postProcessingSettings.AntialiasingMode)
                {
                    Shader.DisableKeyword("_TEMPORALFILTER_ON");
                }

                GUILayout.Space(30);
            }
            GUILayout.EndVertical();
            GUILayout.Space(3);
            GUILayout.BeginVertical(GUIStyles.tabcontent);
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
