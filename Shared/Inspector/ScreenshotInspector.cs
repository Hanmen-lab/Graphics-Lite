using Graphics.UI;
using Graphics.Settings;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class ScreenshotInspector
    {
        private const float FOVMin = 10f;
        private const float FOVMax = 120f;
        private const float FOVDefault = 23.5f;
        private static Vector2 settingstScrollView;
        public delegate void RenderingPathChangedHandler();

        private static int cachedFontSize = -1;
        private static int paddingL, paddingR;
        private static Lay.Style TabContent;

        private static void UpdateCachedValues(GlobalSettings renderSettings)
        {
            if (cachedFontSize == renderSettings.FontSize)
                return;
            cachedFontSize = renderSettings.FontSize;
            TabContent = Lay.Style.Card;
        }

        internal static void Draw(GlobalSettings renderingSettings)
        {
            UpdateCachedValues(renderingSettings);
            settingstScrollView = Lay.BeginScrollView(settingstScrollView, Lay.ExpandHeight(true), Lay.ExpandWidth(true));
            Lay.BeginVertical(TabContent);
            {
                TemporalScreenshotTool screenshotTool = TemporalScreenshotManager.TemporalScreenshotInstance
                    ?? Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<TemporalScreenshotTool>();

                Switch("ENABLE SCREENSHOT ENGINE", Graphics.ScreenshotOverride.Value, true, enabled => Graphics.ScreenshotOverride.Value = enabled);

                Im.Enabled = Graphics.ScreenshotOverride.Value;

                Lay.Space(30);
                Label("CAPTURE SETTINGS", "", true);
                Lay.Space(10);
                if (Lay.Button("Open Screenshot Manager"))
                {
                    var screenshotManagerType = AccessTools.TypeByName("Screencap.ScreenshotManager");
                    var instance = screenshotManagerType != null ? GameObject.FindObjectOfType(screenshotManagerType) : null;
                    if (instance != null)
                    {
                        var plugin = Traverse.Create(instance);
                        bool currentShow = plugin.Field("_uiShow").GetValue<bool>();
                        Rect currentRect = plugin.Field("_uiRect").GetValue<Rect>();
                        plugin.Field("_uiShow").SetValue(!currentShow);
                        plugin.Field("_uiRect").SetValue(currentRect);
                    }
                }
                Lay.Space(10);
                Slider("Warmup Frames", screenshotTool.warmupFrames, 1, 60, frames => screenshotTool.warmupFrames = frames);

                Lay.Space(30);
                Label("QUALITY SETTINGS", "", true);
                Lay.Space(10);
                Selection("Shadow Cascades Override", Graphics.CustomShadowCascadesOverride.Value, cascades => Graphics.CustomShadowCascadesOverride.Value = cascades, 2);
                Selection("Shadow Resolution Override", Graphics.CustomShadowResolutionOverride.Value, resolution => Graphics.CustomShadowResolutionOverride.Value = resolution);
                if (Graphics.CustomShadowResolutionOverride.Value > Graphics.ShadowResolutionOverride._4096)
                {
                    Warning("High shadow resolutions override can cause crash or slow rendering, especially in scenes with many realtime lights. Per-light shadow tuning is still recommended.");
                    Lay.Space(10);
                }
                Lay.Space(5);
                ToggleWithText("Fullscreen SEGI", Graphics.ScreenshotFullScreenSEGI.Value, "Disable SEGI 'half resolution' while rendering screenshot.", false, value => Graphics.ScreenshotFullScreenSEGI.Value = value);
                ToggleWithText("Fullscreen SSS", Graphics.ScreenshotFullScreenSSS.Value, "Set SSS 'Downscale factor' to 1.0 while rendering a screenshot.", false, value => Graphics.ScreenshotFullScreenSSS.Value = value);
                ToggleWithText("Hi-Res Reflection Probes", Graphics.ScreenshotHiResReflectionProbes.Value, "Set max (2048) resolution for all realtime reflection probes while rendering screenshot.", false, value => Graphics.ScreenshotHiResReflectionProbes.Value = value);
                ToggleWithText("Hi-Res SSR", Graphics.ScreenshotHiResSSR.Value, "Set highest resolution for legacy SSR while rendering screenshot.", false, value => Graphics.ScreenshotHiResSSR.Value = value);
                Im.Enabled = Graphics.Instance.PostProcessingSettings.AntialiasingMode == PostProcessingSettings.Antialiasing.CTAA;
                ToggleWithText("CTAA Supersampling", Graphics.ScreenshotCTAASupersampling.Value, "Auto set supersampling mode to CINA_ULTRA while rendering screenshot (F11).", false, value => Graphics.ScreenshotCTAASupersampling.Value = value);
                Im.Enabled = true;
                Im.Enabled = TemporalScreenshotManager.Hooks.AlphaEnabled();
                ToggleWithText("Enable alpha MSAA", Graphics.ScreenshotAlphaMaskMSAA.Value, "Enables MSAA x2 on alpha channel, generating better transparent edges.", false, value => Graphics.ScreenshotAlphaMaskMSAA.Value = value);
                Im.Enabled = true;

                Lay.Space(30);
                Label("INFO", "New Screenshot Engine aims to create highest quality renders and allows modern anti-aliasing techniques like CTAA and FSR3 in custom resolutions.", true);
                Lay.Space(10);
                Label("Transparent Screenshot works!", "You can enable it in Screenshot Manager. For a correct mask, use compatible shaders like Hanmen/Clothing, Next-Gen Body/Face, and Hair shaders.", false);
                Im.Enabled = true;
            }
            Lay.EndVertical();
            Lay.EndScrollView();
        }
    }
}
