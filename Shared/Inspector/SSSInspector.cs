using UnityEngine;
using UnityEngine.UI;
using Graphics.Settings;
using static Graphics.Inspector.Util;

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

            if (SSSManager.SSSInstance != null)
            {
                SSS sss = SSSManager.SSSInstance;
                GUILayout.BeginVertical(GUIStyles.tabcontent);
                {
                    Switch(renderSettings.FontSize, "SCREEN SPACE SSS", sss.Enabled, true, enabled =>
                    {
                        sss.Enabled = enabled;
                        if (!enabled) Graphics.Instance.SSSManager.Destroy();
                        else Graphics.Instance.SSSManager.Start();
                    });
                    if (sss.Enabled)
                    {
                        GUILayout.Space(20);
                        Label("BLUR", "", true);
                        GUILayout.Space(5);
                        Toggle("Profile per object", sss.ProfilePerObject, false, perObj => sss.ProfilePerObject = perObj);
                        if (!sss.ProfilePerObject)
                        {
                            GUILayout.Space(10);
                            SliderColor("Scattering colour", sss.sssColor, colour => sss.sssColor = colour);
                        }    
                        
                        Slider("Blur size", sss.ScatteringRadius, 0f, 10f, "N1", radius => sss.ScatteringRadius = radius);
                        Slider("Postprocess iterations", sss.ScatteringIterations, 0, 10, iterations => sss.ScatteringIterations = iterations);
                        Slider("Shader iterations per pass", sss.ShaderIterations, 1, 20, iterations => sss.ShaderIterations = iterations);
                        Slider("Downscale factor", sss.Downsampling, 1f, 4f, "N1", sampling => sss.Downsampling = sampling);
                        Slider("Max Distance", sss.maxDistance, 0, 10000, distance => sss.maxDistance = distance);                       
                        SelectionMask("Layers", sss.SSS_Layer, layer => sss.SSS_Layer = layer);
                        GUILayout.Space(10);
                        Label("DITHERING", "", true);
                        GUILayout.Space(5);
                        Toggle("Dither", sss.Dither, false, dither => sss.Dither = dither);
                        if (sss.Dither)
                        {
                            Slider("Dither intensity", sss.DitherIntensity, 0f, 5f, "N1", intensity => sss.DitherIntensity = intensity);
                            Slider("Dither scale", sss.DitherScale, 1f, 100f, "N1", scale => sss.DitherScale = scale);
                        }
                        GUILayout.Space(20);
                        Label("DEBUG", "", true);
                        GUILayout.Space(5);
                        Label("Camera Name", sss.transform.parent.name);
                        if (sss.LightingTex)
                        Label("Buffer size", sss.LightingTex.width + " x " + sss.LightingTex.height);
                        Label("Light pass shader", sss.LightingPassShader is null ? "NULL" : sss.LightingPassShader.name);
                        Selection("View Buffer", sss.toggleTexture, texture => sss.toggleTexture = texture);
                        Toggle("Debug distance", sss.DebugDistance, false, debug => sss.DebugDistance = debug);

                        GUILayout.Space(20);
                        Label("EDGE TEST", "", true);
                        GUILayout.Space(5);
                        Slider("Depth test", sss.DepthTest, 0f, 1f, "N3", depth => sss.DepthTest = depth);
                        Slider("Normal test", sss.NormalTest, 0f, 1f, "N3", normal => sss.NormalTest = normal);
                        Toggle("Apply edge test to dither noise", sss.DitherEdgeTest, false, dither => sss.DitherEdgeTest = dither);
                        Toggle("Fix pixel leaks", sss.FixPixelLeaks, false, fix => sss.FixPixelLeaks = fix);
                        if (sss.FixPixelLeaks)
                        {
                            Slider("Normal test", sss.EdgeOffset, 1f, 1.2f, "N3", offset => sss.EdgeOffset = offset);
                        }
                        Toggle("Profile Test (per obj)", sss.UseProfileTest, false, profileTest => sss.UseProfileTest = profileTest);
                        if (sss.ProfilePerObject && sss.UseProfileTest)
                        {
                            Slider("Profile Colour Test", sss.ProfileColorTest, 0f, 1f, "N3", test => sss.ProfileColorTest = test);
                            Slider("Profile Radius Test", sss.ProfileRadiusTest, 0f, 1f, "N3", test => sss.ProfileRadiusTest = test);
                        }
                    }
                    Graphics.Instance.SSSManager.CopySettingsToOtherInstances();

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