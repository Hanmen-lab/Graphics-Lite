using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Graphics.DebugUtils;



namespace Graphics
{
    public class TemporalScreenshotManager
    {
        public static TemporalScreenshotTool TemporalScreenshotInstance;
        public const string videoExportGUID = "com.joan6694.illusionplugins.videoexport";
        public const string videoExportName = "VideoExport";

        // Initialize Components
        internal void Initialize()
        {
            TemporalScreenshotInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<TemporalScreenshotTool>();
            TemporalScreenshotInstance.enabled = true;
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }

        public void CheckInstance()
        {
            if (TemporalScreenshotInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                TemporalScreenshotInstance = camera.GetOrAddComponent<TemporalScreenshotTool>();
            }
        }

        public static class Hooks
        {
            public static Traverse PluginInstance;
            public static bool IsNewPlugin = false;
            public static Dictionary<Light, int> originalShadowResolutions = new Dictionary<Light, int>();
            public static int customShadowRes;
            public static int shadowCascadeValue;
            public static int sc;
            public static int limitFrames;

            public static void GetDimensions(out Vector2Int _screenshotResolution, out int _supersampling)
            {
                if (PluginInstance == null)
                {
                    _screenshotResolution = new Vector2Int(1920, 1080);
                    _supersampling = 1;
                }
                int w, h, d;
                if (IsNewPlugin)
                {
                    w = PluginInstance.Property("ResolutionX").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
                    h = PluginInstance.Property("ResolutionY").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
                    d = PluginInstance.Property("DownscalingRate").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
                }
                else
                {
                    w = PluginInstance.Property("CaptureWidth").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
                    h = PluginInstance.Property("CaptureHeight").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
                    d = PluginInstance.Property("Downscaling").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
                }
                _screenshotResolution = new Vector2Int(w, h);
                _supersampling = d;
            }

            public static string GetUniqueFilename(out string extension, out int _jpegQuality)
            {
                _jpegQuality = 100;
                if (PluginInstance == null)
                {
                    var now = DateTime.Now;
                    extension = "png";
                    return $"{BepInEx.Paths.GameRootPath}/UserData/cap/{Application.productName}_{now.Day}_{now.Month}_{now.Year}_{now.Hour}_{now.Second}.{extension}";
                }
                if (IsNewPlugin)
                {
                    bool UseJpg = PluginInstance.Property("UseJpg").GetValue<BepInEx.Configuration.ConfigEntry<bool>>().Value;
                    if (UseJpg)
                    {
                        extension = "jpg";
                        _jpegQuality = PluginInstance.Property("JpgQuality").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
                    }
                    else
                        extension = "png";
                    return PluginInstance.Method("GetUniqueFilename", "Render", UseJpg).GetValue<string>();
                }
                else
                {
                    extension = "png";
                    return PluginInstance.Method("GetCaptureFilename").GetValue<string>();
                }
            }

            public static bool AlphaEnabled()
            {
                if (PluginInstance == null)
                    return false;
                if (IsNewPlugin)
                    return 0 != (int)PluginInstance.Property("CaptureAlphaMode").GetValue<BepInEx.Configuration.ConfigEntryBase>().BoxedValue;
                else
                    return PluginInstance.Property("Alpha").GetValue<BepInEx.Configuration.ConfigEntry<bool>>().Value;
            }

            public static void PatchScreenshotManager()
            {
                Harmony harmony = new Harmony(Graphics.GUID);

                var screenshotManagerType = AccessTools.TypeByName("Screencap.ScreenshotManager");
                if (screenshotManagerType == null) return;

                PluginInstance = Traverse.Create(screenshotManagerType);

                var takeScreenshotMethodNew = AccessTools.Method(screenshotManagerType, "TakeRenderScreenshot");
                var takeScreenshotMethodOld = AccessTools.Method(screenshotManagerType, "WaitForEndOfFrameThen");

                if (takeScreenshotMethodNew != null)
                {
                    Graphics.Instance.Log.LogInfo("Hooking A New Screencap...");
                    /*harmony.Patch(
                        takeScreenshotMethodNew,
                        prefix: new HarmonyMethod(typeof(Hooks), nameof(TakeRenderScreenshot_Prefix))
                    );*/
                    HarmonyMethod prefixTakeScreenshotMethodNew = new HarmonyMethod(
                        typeof(Hooks),
                        nameof(TakeRenderScreenshot_Prefix)
                    );
                    //prefixTakeScreenshotMethodNew.priority = Priority.First;
                    //prefixTakeScreenshotMethodNew.before = new string[] { videoExportGUID };
                    harmony.Patch(
                        takeScreenshotMethodNew,
                        prefix: prefixTakeScreenshotMethodNew
                    );
                    /* Lower level hook just in case above stops working again.
                    var t = screenshotManagerType.GetNestedTypes(AccessTools.all);
                    for (var i = 0; i < t.Length; i++)
                    {
                        if (t[i].Name.StartsWith("<TakeRenderScreenshot>"))
                        {
                            Graphics.Instance.Log.LogInfo("Hooked MoveNext.");
                            harmony.Patch(
                                AccessTools.Method(t[i], "MoveNext"),
                                prefix: new HarmonyMethod(typeof(Hooks), nameof(fuckCoroutines))
                            );
                        }
                    }*/
                    IsNewPlugin = true;
                }

                if (takeScreenshotMethodOld != null)
                {
                    Graphics.Instance.Log.LogInfo("Hooking An Old Screencap...");
                    /*harmony.Patch(
                        takeScreenshotMethodOld,
                        prefix: new HarmonyMethod(typeof(Hooks), nameof(WaitForEndOfFrameThen_Prefix))
                    );*/
                    HarmonyMethod prefixTakeScreenshotMethodOld = new HarmonyMethod(
                        typeof(Hooks),
                        nameof(WaitForEndOfFrameThen_Prefix)
                    );
                    //prefixTakeScreenshotMethodOld.priority = Priority.First;
                    //prefixTakeScreenshotMethodOld.before = new string[] { videoExportGUID };
                    harmony.Patch(
                        takeScreenshotMethodOld,
                        prefix: prefixTakeScreenshotMethodOld
                    );
                }

                // patch ScreenshotManager's CaptureScreen()
                var captureScreenMethod = AccessTools.Method(screenshotManagerType, "CaptureScreen");

                if (captureScreenMethod != null)
                {
                    Graphics.Instance.Log.LogInfo("Hooking A CaptureScreen...");
                    HarmonyMethod prefixCaptureScreenMethod = new HarmonyMethod(
                        typeof(Hooks),
                        nameof(CaptureScreen_Prefix)
                    );
                    harmony.Patch(
                        captureScreenMethod,
                        prefix: prefixCaptureScreenMethod
                    );
                }

                // patch VideoExport's RecordVideo() and StopRecording()
                var videoExportType = AccessTools.TypeByName(videoExportName);
                if (videoExportType == null) return;

                var RecordVideoMethod = AccessTools.Method(videoExportType, "RecordVideo");
                var StopRecordingMethod = AccessTools.Method(videoExportType, "StopRecording");

                if (RecordVideoMethod != null)
                {
                    Graphics.Instance.Log.LogInfo("Hooking A RecordVideo...");
                    HarmonyMethod postfixRecordVideoMethod = new HarmonyMethod(
                        typeof(Hooks),
                        nameof(RecordVideo_Postfix)
                    );
                    harmony.Patch(
                        RecordVideoMethod,
                        postfix: postfixRecordVideoMethod
                    );
                }

                if (StopRecordingMethod != null)
                {
                    Graphics.Instance.Log.LogInfo("Hooking A StopRecording...");
                    HarmonyMethod postfixStopRecordingMethod = new HarmonyMethod(
                        typeof(Hooks),
                        nameof(StopRecording_Postfix)
                    );
                    harmony.Patch(
                        StopRecordingMethod,
                        postfix: postfixStopRecordingMethod
                    );
                }
            }
            public static System.Collections.IEnumerator TakeRenderScreenshot_Hook(BaseUnityPlugin __instance)
            {
                Graphics.Instance.Log.LogWarning("Screenshot Engine Triggered.");

                GetDimensions(out Vector2Int r, out int Downscaling);

                List<AmbientOcclusion> aos = new List<AmbientOcclusion>();
                var DisableAO = (int)PluginInstance.Property("DisableAO").GetValue<BepInEx.Configuration.ConfigEntryBase>().BoxedValue;

                if (DisableAO == 0 || (DisableAO == 1 && Downscaling > 1))
                    foreach (PostProcessVolume vol in UnityEngine.Object.FindObjectsOfType<PostProcessVolume>())
                        if (vol.profile.TryGetSettings<AmbientOcclusion>(out AmbientOcclusion ao) && ao.enabled.value)
                        {
                            ao.enabled.value = false;
                            aos.Add(ao);
                        }

                sc = QualitySettings.shadowCascades;

                // Use value from TemporalScreenshotInstance
                int shadowCascadeValue = (int)Graphics.customShadowCascadesOverride.Value;

                if (shadowCascadeValue != 5)
                {
                    QualitySettings.shadowCascades = shadowCascadeValue;
                    Graphics.Instance.Log.LogInfo("Shadow cascades override set: " + shadowCascadeValue.ToString());
                }

                var lights = GameObject.FindObjectsOfType<Light>();

                int customShadowRes = (int)Graphics.customShadowResolutionOverride.Value;


                if (customShadowRes > 0)
                {
                    Graphics.Instance.Log.LogInfo("Shadow resolution override set: " + customShadowRes);
                    foreach (var l in lights)
                    {
                        if (l.enabled)
                        {
                            originalShadowResolutions[l] = l.shadowCustomResolution;
                            var handler = l.GetComponent<CustomShadowResolutionHandler>();
                            if (handler != null)
                                handler.SetResolution(customShadowRes);
                            else
                                l.shadowCustomResolution = customShadowRes;
                        }
                    }
                }

                TemporalScreenshotInstance.MakeScreenshot();

                while (TemporalScreenshotInstance.cameraRenderTarget != null)
                {
                    yield return null;
                }

                yield return new WaitForEndOfFrame();

                // Restore settings
                foreach (AmbientOcclusion ao2 in aos)
                    ao2.enabled.value = true;

                if (shadowCascadeValue != 5)
                {
                    QualitySettings.shadowCascades = sc;
                    Graphics.Instance.Log.LogInfo("Restored shadow cascades to: " + sc.ToString());
                }

                if (customShadowRes > 0)
                {
                    Graphics.Instance.Log.LogInfo("Restoring shadow resolutions...");
                    foreach (var kvp in originalShadowResolutions)
                    {
                        var l = kvp.Key;
                        var originalResolution = kvp.Value;
                        if (l != null)
                        {
                            var handler = l.GetComponent<CustomShadowResolutionHandler>();
                            if (handler != null)
                                handler.SetResolution(originalResolution);
                            else
                                l.shadowCustomResolution = originalResolution;
                        }
                    }
                }

                yield break;
            }

            public static bool TakeRenderScreenshot_Prefix(BaseUnityPlugin __instance, ref System.Collections.IEnumerator __result)
            {
                //Graphics.Instance.Log.LogInfo("TakeRenderScreenshot_Prefix.");
                if (PluginInstance == null)
                    PluginInstance = Traverse.Create(__instance);
                if (!Graphics.ScreenshotOverride.Value || AlphaEnabled())
                    return true; // Don't override
                __result = TakeRenderScreenshot_Hook(__instance);
                return false;
            }

            public static bool WaitForEndOfFrameThen_Prefix(Action a, BaseUnityPlugin __instance, ref System.Collections.IEnumerator __result)
            {
                //Graphics.Instance.Log.LogInfo("WaitForEndOfFrameThen_Prefix.");
                if (PluginInstance == null)
                    PluginInstance = Traverse.Create(__instance);
                if (!Graphics.ScreenshotOverride.Value || AlphaEnabled())
                    return true; // Don't override
                if (a.Method.Name == "<Update>b__60_1" || a.Method.Name == "<Update>b__60_2")
                {
                    __instance.StartCoroutine(TakeRenderScreenshot_Hook(__instance));
                    return false;
                }
                return true;
            }

            public static bool CaptureScreen_Prefix(BaseUnityPlugin __instance, ref RenderTexture __result, int width, int height, bool alpha)
            {
                if (PluginInstance == null)
                    PluginInstance = Traverse.Create(__instance);

                // Проверяем, включен ли override
                if (!Graphics.ScreenshotOverride.Value || AlphaEnabled())
                    return true; // Используем оригинальный метод

                var fmt = alpha ? RenderTextureFormat.ARGB32 : RenderTextureFormat.Default;
                var rt = RenderTexture.GetTemporary(width, height, 32, fmt, RenderTextureReadWrite.Default);

                TemporalScreenshotInstance.MakeScreenRender(rt);

                __result = rt;
                return false;
            }

            public static void RecordVideo_Postfix(BaseUnityPlugin __instance)
            {
                GetDimensions(out Vector2Int r, out int Downscaling);

                List<AmbientOcclusion> aos = new List<AmbientOcclusion>();
                var DisableAO = (int)PluginInstance.Property("DisableAO").GetValue<BepInEx.Configuration.ConfigEntryBase>().BoxedValue;

                if (DisableAO == 0 || (DisableAO == 1 && Downscaling > 1))
                    foreach (PostProcessVolume vol in UnityEngine.Object.FindObjectsOfType<PostProcessVolume>())
                        if (vol.profile.TryGetSettings<AmbientOcclusion>(out AmbientOcclusion ao) && ao.enabled.value)
                        {
                            ao.enabled.value = false;
                            aos.Add(ao);
                        }

                // Use value from TemporalScreenshotInstance
                shadowCascadeValue = (int)Graphics.customShadowCascadesOverride.Value;

                if (shadowCascadeValue != 5)
                {
                    QualitySettings.shadowCascades = shadowCascadeValue;
                    Graphics.Instance.Log.LogInfo("Shadow cascades override set: " + shadowCascadeValue.ToString());
                }

                var lights = GameObject.FindObjectsOfType<Light>();

                customShadowRes = (int)Graphics.customShadowResolutionOverride.Value;

                if (customShadowRes > 0)
                {
                    Graphics.Instance.Log.LogInfo("Shadow resolution override set: " + customShadowRes);
                    foreach (var l in lights)
                    {
                        if (l.enabled)
                        {
                            originalShadowResolutions[l] = l.shadowCustomResolution;
                            var handler = l.GetComponent<CustomShadowResolutionHandler>();
                            if (handler != null)
                                handler.SetResolution(customShadowRes);
                            else
                                l.shadowCustomResolution = customShadowRes;
                        }
                    }
                }
                //get recordingFrameLimit
                limitFrames = Traverse.Create(__instance).Field("_recordingFrameLimit").GetValue<int>();
                Graphics.Instance.Log.LogInfo("Limit Frames:" + limitFrames);

                TemporalScreenshotInstance.MakeScreenRenderPre(limitFrames);
            }

            public static void StopRecording_Postfix(BaseUnityPlugin __instance)
            {
                TemporalScreenshotInstance.MakeScreenRenderPost();

                List<AmbientOcclusion> aos = new List<AmbientOcclusion>();
                var DisableAO = (int)PluginInstance.Property("DisableAO").GetValue<BepInEx.Configuration.ConfigEntryBase>().BoxedValue;

                foreach (AmbientOcclusion ao2 in aos)
                    ao2.enabled.value = true;

                if (shadowCascadeValue != 5)
                {
                    QualitySettings.shadowCascades = sc;
                    Graphics.Instance.Log.LogInfo("Restored shadow cascades to: " + sc.ToString());
                }

                if (customShadowRes > 0)
                {
                    Graphics.Instance.Log.LogInfo("Restoring shadow resolutions...");
                    foreach (var kvp in originalShadowResolutions)
                    {
                        var l = kvp.Key;
                        var originalResolution = kvp.Value;
                        if (l != null)
                        {
                            var handler = l.GetComponent<CustomShadowResolutionHandler>();
                            if (handler != null)
                                handler.SetResolution(originalResolution);
                            else
                                l.shadowCustomResolution = originalResolution;
                        }
                    }
                }
            }
        }
    }
}
