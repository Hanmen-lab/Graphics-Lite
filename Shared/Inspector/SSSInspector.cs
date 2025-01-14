using UnityEngine;
using UnityEngine.UI;
using Graphics.Settings;
using static Graphics.Inspector.Util;
using Graphics.CTAA;

namespace Graphics.Inspector
{
    internal static class SSSInspector
    {
        //private static GlobalSettings renderSettings;
        private static Vector2 sssScrollView;
        internal static void Draw(GlobalSettings renderSettings)
        {
            sssScrollView = GUILayout.BeginScrollView(sssScrollView);
            if (Graphics.Instance.CameraSettings.MainCamera is null)
            {
                return;
            }

            GUIStyle TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2.9f), Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f));

            if (SSSManager.settings != null)
            {
                SSSSettings sss = SSSManager.settings;
                SSS sssinstance = SSSManager.SSSInstance;
                GUILayout.BeginVertical(TabContent);
                {
                    Switch(renderSettings.FontSize, "SCREEN SPACE SSS", sss.Enabled, true, enabled =>
                    {
                        sss.Enabled = enabled;
                        SSSManager.UpdateSettings();
                    });

                    if (sss.Enabled)
                    {
                        GUILayout.Space(20);
                        Label("BLUR", "", true);
                        GUILayout.Space(5);
                        Toggle("Profile per object", sss.ProfilePerObject, false, perObj => { sss.ProfilePerObject = perObj; SSSManager.UpdateSettings(); });
                        if (!sss.ProfilePerObject)
                        {
                            GUILayout.Space(10);
                            SliderColor("Scattering colour", sss.sssColor, colour => { sss.sssColor = colour; SSSManager.UpdateSettings(); });
                        }

                        Slider("Blur size", sss.BlurSize, 0f, 10f, "N1", radius => { sss.BlurSize = radius; SSSManager.UpdateSettings(); });
                        Slider("Postprocess iterations", sss.ProcessIterations, 0, 10, iterations => { sss.ProcessIterations = iterations; SSSManager.UpdateSettings(); });
                        Slider("Shader iterations per pass", sss.ShaderIterations, 1, 20, iterations => { sss.ShaderIterations = iterations; SSSManager.UpdateSettings(); });
                        Slider("Downscale factor", sss.DownscaleFactor, 1f, 4f, "N1", sampling => { sss.DownscaleFactor = sampling; SSSManager.UpdateSettings(); });
                        Slider("Max Distance", sss.MaxDistance, 0, 10000, distance => { sss.MaxDistance = distance; SSSManager.UpdateSettings(); });
                        SelectionMask("Layers", sss.LayerBitMask, layer => { sss.LayerBitMask = layer; SSSManager.UpdateSettings(); });
                        GUILayout.Space(10);
                        Label("DITHERING", "", true);
                        GUILayout.Space(5);
                        Toggle("Dither", sss.Dither, false, dither => { sss.Dither = dither; SSSManager.UpdateSettings(); });
                        if (sss.Dither)
                        {
                            Slider("Dither intensity", sss.DitherIntensity, 0f, 5f, "N1", intensity => { sss.DitherIntensity = intensity; SSSManager.UpdateSettings(); });
                            Slider("Dither scale", sss.DitherScale, 1f, 100f, "N1", scale => { sss.DitherScale = scale; SSSManager.UpdateSettings(); });
                        }
                        GUILayout.Space(20);
                        Label("DEBUG", "", true);
                        GUILayout.Space(5);
                        Label("Camera Name", sssinstance.transform.parent.name);
                        if (sssinstance.LightingTex)
                            Label("Buffer size", sssinstance.LightingTex.width + " x " + sssinstance.LightingTex.height);
                        Label("Light pass shader", sssinstance.LightingPassShader is null ? "NULL" : sssinstance.LightingPassShader.name);
                        Selection("View Buffer", sss.ViewBuffer, texture => { sss.ViewBuffer = texture; SSSManager.UpdateSettings(); });
                        Toggle("Debug distance", sss.DebugDistance, false, debug => { sss.DebugDistance = debug; SSSManager.UpdateSettings(); });

                        GUILayout.Space(20);
                        Label("EDGE TEST", "", true);
                        GUILayout.Space(5);
                        Slider("Depth test", sss.DepthTest, 0f, 1f, "N3", depth => { sss.DepthTest = depth; SSSManager.UpdateSettings(); });
                        Slider("Normal test", sss.NormalTest, 0f, 1f, "N3", normal => { sss.NormalTest = normal; SSSManager.UpdateSettings(); });
                        Toggle("Apply edge test to dither noise", sss.EdgeDitherNoise, false, dither => { sss.EdgeDitherNoise = dither; SSSManager.UpdateSettings(); });
                        Toggle("Fix pixel leaks", sss.FixPixelLeaks, false, fix => { sss.FixPixelLeaks = fix; SSSManager.UpdateSettings(); });
                        if (sss.FixPixelLeaks)
                        {
                            Slider("Normal test", sss.FixPixelLeaksNormal, 1f, 1.2f, "N3", offset => { sss.FixPixelLeaksNormal = offset; SSSManager.UpdateSettings(); });
                        }
                        Toggle("Profile Test (per obj)", sss.ProfileTest, false, profileTest => { sss.ProfileTest = profileTest; SSSManager.UpdateSettings(); });
                        if (sss.ProfilePerObject && sss.ProfileTest)
                        {
                            Slider("Profile Colour Test", sss.ProfileColorTest, 0f, 1f, "N3", test => { sss.ProfileColorTest = test; SSSManager.UpdateSettings(); });
                            Slider("Profile Radius Test", sss.ProfileRadiusTest, 0f, 1f, "N3", test => { sss.ProfileRadiusTest = test; SSSManager.UpdateSettings(); });
                        }
                    }
                    //Graphics.Instance.SSSManager.CopySettingsToOtherInstances();

                    GUILayout.ExpandHeight(true);
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();
                    GUILayout.FlexibleSpace();

                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }
    }
}