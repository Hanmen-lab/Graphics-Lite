using UnityEngine;
using Graphics.UI;
using UnityEngine.UI;
using Graphics.Settings;
using static Graphics.Inspector.Util;
using Graphics.CTAA;

namespace Graphics.Inspector
{
    internal static class SSSInspector
    {

        private static Vector2 sssScrollView;


        private static int cachedFontSize = -1;
        private static int paddingL, paddingR;
        private static Lay.Style TabContent;

        static void UpdateCachedValues(GlobalSettings renderSettings)
        {
            if (cachedFontSize == renderSettings.FontSize)
                return;
            cachedFontSize = renderSettings.FontSize;
            TabContent = Lay.Style.Card;
        }

        internal static void Draw(GlobalSettings renderSettings)
        {
            UpdateCachedValues(renderSettings);

            sssScrollView = Lay.BeginScrollView(sssScrollView);
            if (Graphics.Instance.CameraSettings.MainCamera is null)
            {
                return;
            }

            DrawHSSSS();

            if (SSSManager.settings != null)
            {
                SSSSettings sss = SSSManager.settings;
                SSS sssinstance = SSSManager.SSSInstance;
                Lay.BeginVertical(TabContent);
                {
                    Switch("SCREEN SPACE SSS", sss.Enabled, true, enabled =>
                    {
                        sss.Enabled = enabled;
                        SSSManager.UpdateSettings();
                        // One engine at a time, as in Graphics.
                        if (enabled && HSSSSManager.settings != null && HSSSSManager.settings.Enabled)
                        {
                            HSSSSManager.settings.Enabled = false;
                            HSSSSManager.UpdateSettings();
                        }
                    });

                    if (sss.Enabled)
                    {
                        Lay.Space(20);
                        Label("COLOR", "", true);
                        Lay.Space(5);
                        Slider("Blur size", sss.BlurSize, 0f, 10f, "N1", radius => { sss.BlurSize = radius; SSSManager.UpdateSettings(); });
                        Toggle("Profile per object", sss.ProfilePerObject, false, perObj => { sss.ProfilePerObject = perObj; SSSManager.UpdateSettings(); });
                        if (!sss.ProfilePerObject)
                        {
                            Lay.Space(10);
                            SliderColor("Scattering colour", sss.sssColor, colour => { sss.sssColor = colour; SSSManager.UpdateSettings(); });
                        }

                        Lay.Space(20);
                        Label("RENDERING SETTINGS", "", true);
                        Lay.Space(5);
                        Slider("Postprocess iterations", sss.ProcessIterations, 0, 10, iterations => { sss.ProcessIterations = iterations; SSSManager.UpdateSettings(); });
                        Slider("Shader iterations per pass", sss.ShaderIterations, 1, 20, iterations => { sss.ShaderIterations = iterations; SSSManager.UpdateSettings(); });
                        Slider("Downscale factor", sss.DownscaleFactor, 1f, 4f, "N1", sampling => { sss.DownscaleFactor = sampling; SSSManager.UpdateSettings(); });
                        Slider("Max Distance", sss.MaxDistance, 0, 9999, distance => { sss.MaxDistance = distance; SSSManager.UpdateSettings(); });
                        SelectionMaskVertical("Layers", sss.LayerBitMask, layer => { sss.LayerBitMask = layer; SSSManager.UpdateSettings(); }, 4);

                        Lay.Space(20);
                        Label("DITHERING", "", true);
                        Lay.Space(5);
                        Toggle("Dither", sss.Dither, false, dither => { sss.Dither = dither; SSSManager.UpdateSettings(); });
                        if (sss.Dither)
                        {
                            Slider("Dither intensity", sss.DitherIntensity, 0f, 5f, "N2", intensity => { sss.DitherIntensity = intensity; SSSManager.UpdateSettings(); });
                            Slider("Dither scale", sss.DitherScale, 1f, 100f, "N1", scale => { sss.DitherScale = scale; SSSManager.UpdateSettings(); });
                        }

                        Lay.Space(20);
                        Label("DEBUG", "", true);
                        Lay.Space(5);
                        Label("Camera Name", sssinstance.transform.parent.name);
                        if (sssinstance.LightingTex)
                            Label("Buffer size", sssinstance.LightingTex.width + " x " + sssinstance.LightingTex.height);
                        Label("Light pass shader", sssinstance.LightingPassShader is null ? "NULL" : sssinstance.LightingPassShader.name);
                        Selection("View Buffer", sss.ViewBuffer, texture => { sss.ViewBuffer = texture; SSSManager.UpdateSettings(); }, 2);
                        Toggle("Debug distance", sss.DebugDistance, false, debug => { sss.DebugDistance = debug; SSSManager.UpdateSettings(); });

                        Lay.Space(20);
                        Label("EDGE TEST", "", true);
                        Lay.Space(5);
                        Slider("Depth test", sss.DepthTest, 0f, 1f, "N2", depth => { sss.DepthTest = depth; SSSManager.UpdateSettings(); });
                        Slider("Normal test", sss.NormalTest, 0f, 1f, "N2", normal => { sss.NormalTest = normal; SSSManager.UpdateSettings(); });
                        Toggle("Apply edge test to dither noise", sss.EdgeDitherNoise, false, dither => { sss.EdgeDitherNoise = dither; SSSManager.UpdateSettings(); });
                        Toggle("Fix pixel leaks", sss.FixPixelLeaks, false, fix => { sss.FixPixelLeaks = fix; SSSManager.UpdateSettings(); });
                        if (sss.FixPixelLeaks)
                        {
                            Slider("Normal test", sss.FixPixelLeaksNormal, 1f, 1.2f, "N2", offset => { sss.FixPixelLeaksNormal = offset; SSSManager.UpdateSettings(); });
                        }

                        Toggle("Profile Test (per obj)", sss.ProfileTest, false, profileTest => { sss.ProfileTest = profileTest; SSSManager.UpdateSettings(); });
                        if (sss.ProfilePerObject && sss.ProfileTest)
                        {
                            Slider("Profile Colour Test", sss.ProfileColorTest, 0f, 1f, "N2", test => { sss.ProfileColorTest = test; SSSManager.UpdateSettings(); });
                            Slider("Profile Radius Test", sss.ProfileRadiusTest, 0f, 1f, "N2", test => { sss.ProfileRadiusTest = test; SSSManager.UpdateSettings(); });
                        }


                    }
                    //Graphics.Instance.SSSManager.CopySettingsToOtherInstances();

                    Lay.ExpandHeight(true);
                    Lay.FlexibleSpace();
                    Lay.FlexibleSpace();
                    Lay.FlexibleSpace();

                }
                Lay.EndVertical();
            }
            Lay.EndScrollView();
        }

        private static readonly string[] LutNames = { "None", "NVIDIA 1", "NVIDIA 2" };

        private static void DrawHSSSS()
        {
            if (HSSSSManager.settings == null)
                HSSSSManager.settings = new HSSSSSettings();
            HSSSSSettings h = HSSSSManager.settings;
            Lay.BeginVertical(TabContent);
            {
                Switch("HSSSS", h.Enabled, true, enabled =>
                {
                    h.Enabled = enabled;
                    HSSSSManager.UpdateSettings();
                    if (enabled && SSSManager.settings != null && SSSManager.settings.Enabled)
                    {
                        SSSManager.settings.Enabled = false;
                        SSSManager.UpdateSettings();
                    }
                });

                if (h.Enabled)
                {
                    if (!HSSSSManager.Deferred)
                        Warning("HSSSS needs the Deferred rendering path (Settings tab).");

                    Lay.Space(20);
                    Label("SCATTERING", "", true);
                    Lay.Space(5);
                    Toggle("Screen Space Blur", h.ScreenSpaceSSS, false, on => { h.ScreenSpaceSSS = on; HSSSSManager.UpdateSettings(); });
                    if (h.ScreenSpaceSSS)
                    {
                        Slider("Blur Radius", h.BlurRadius, 0f, 200f, "N1", v => { h.BlurRadius = v; HSSSSManager.UpdateSettings(); });
                        Slider("Blur Depth Range", h.BlurDepthRange, 0f, 5f, "N2", v => { h.BlurDepthRange = v; HSSSSManager.UpdateSettings(); });
                        Slider("Blur Iterations", h.BlurIterations, 1, 10, v => { h.BlurIterations = v; HSSSSManager.UpdateSettings(); });
                        Slider("Depth Test", h.DepthTest, 0f, 1f, "N2", v => { h.DepthTest = v; HSSSSManager.UpdateSettings(); });
                        Slider("Normal Test", h.NormalTest, 0f, 1f, "N2", v => { h.NormalTest = v; HSSSSManager.UpdateSettings(); });
                        Toggle("Blur Albedo", h.BlurAlbedo, false, on => { h.BlurAlbedo = on; HSSSSManager.UpdateSettings(); });
                    }

                    Lay.Space(20);
                    Label("TRANSLUCENCY", "", true);
                    Lay.Space(5);
                    Toggle("Baked Thickness", h.BakedThickness, false, on => { h.BakedThickness = on; HSSSSManager.UpdateSettings(); });
                    Slider("Weight", h.TransWeight, 0f, 25f, "N2", v => { h.TransWeight = v; HSSSSManager.UpdateSettings(); });
                    Slider("Shadow Weight", h.TransShadowWeight, 0f, 1f, "N2", v => { h.TransShadowWeight = v; HSSSSManager.UpdateSettings(); });
                    Slider("Distortion", h.TransDistortion, 0f, 1f, "N2", v => { h.TransDistortion = v; HSSSSManager.UpdateSettings(); });
                    Slider("Falloff", h.TransFalloff, 0f, 10f, "N2", v => { h.TransFalloff = v; HSSSSManager.UpdateSettings(); });
                    if (!h.BakedThickness)
                        Slider("Thickness Bias", h.ThicknessBias, 0f, 1f, "N2", v => { h.ThicknessBias = v; HSSSSManager.UpdateSettings(); });
                    Dimension("Absorption", h.TransAbsorption, v => { h.TransAbsorption = v; HSSSSManager.UpdateSettings(); });

                    Lay.Space(20);
                    Label("SKIN LUT", "", true);
                    Lay.Space(5);
                    Selection("LUT Profile", LutNames[Mathf.Clamp(h.LutProfile, 0, 2)], LutNames, v => { h.LutProfile = System.Array.IndexOf(LutNames, v); HSSSSManager.UpdateSettings(); });
                    Slider("Intensity", h.LutIntensity, 0f, 1f, "N2", v => { h.LutIntensity = v; HSSSSManager.UpdateSettings(); });
                    Slider("Bias", h.LutBias, 0f, 1f, "N2", v => { h.LutBias = v; HSSSSManager.UpdateSettings(); });
                    Slider("Scale", h.LutScale, 0f, 1f, "N2", v => { h.LutScale = v; HSSSSManager.UpdateSettings(); });
                    Slider("Shadow Bias", h.ShadowLutBias, -1f, 1f, "N2", v => { h.ShadowLutBias = v; HSSSSManager.UpdateSettings(); });
                    Slider("Shadow Scale", h.ShadowLutScale, 0f, 2f, "N2", v => { h.ShadowLutScale = v; HSSSSManager.UpdateSettings(); });
                    Slider("Blur Weight", h.BlurWeight, 0f, 5f, "N2", v => { h.BlurWeight = v; HSSSSManager.UpdateSettings(); });
                    Dimension("Colour Bleed", h.ColorBleed, v => { h.ColorBleed = v; HSSSSManager.UpdateSettings(); });
                }
            }
            Lay.EndVertical();
        }
    }
}