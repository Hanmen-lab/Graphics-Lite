using Graphics.Settings;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Graphics.CTAA
{
    public class CTAAManager
    {
        public static CTAASettings settings { get; set; } = new CTAASettings();
        public static CTAA_PC Instance;

        internal void Initialize()
        {
            var ctaaInstances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<CTAA_PC>(true);

            if (ctaaInstances.Length > 0)
            {
                foreach (var instance in ctaaInstances)
                {
                    settings.Load(instance);
                }
            }
            else
            {
                Instance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<CTAA_PC>();
                settings.Load(Instance);
            }
        }

        public static void UpdateSettings()
        {
            if (settings == null)
                settings = new CTAASettings();

            var ctaaInstances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<CTAA_PC>(true);

            if (ctaaInstances.Length > 0)
            {
                foreach (var instance in ctaaInstances)
                {
                    settings.Load(instance);
                }
            }
            else
            {
                Instance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<CTAA_PC>();
                settings = new CTAASettings();
                settings.Load(Instance);
            }
        }

        public void CheckInstance()
        {
            var ctaaInstances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<CTAA_PC>(true);

            if (ctaaInstances.Length > 0)
            {
                Instance = ctaaInstances[0];
            }
            else
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                Instance = camera.GetOrAddComponent<CTAA_PC>();
            }
        }

        //public static class Hooks
        //{
        //    public static void PatchScreenshotManager()
        //    {
        //        Harmony harmony = new Harmony(Graphics.GUID);

        //        //var screenshotManagerType = Type.GetType("Screencap.ScreenshotManager, HS2_Screencap");
        //        var screenshotManagerType = AccessTools.TypeByName("Screencap.ScreenshotManager");
        //        if (screenshotManagerType == null) return;

        //        var waitMethod = AccessTools.Method(screenshotManagerType, "WaitForEndOfFrameThen");
        //        var takeScreenshotMethod = AccessTools.Method(screenshotManagerType, "TakeCharScreenshot");

        //        if (waitMethod != null)
        //        {
        //            harmony.Patch(
        //                waitMethod,
        //                postfix: new HarmonyMethod(typeof(Hooks), nameof(WaitForEndOfFrameThen_Postfix))
        //            );
        //        }

        //        if (takeScreenshotMethod != null)
        //        {
        //            harmony.Patch(
        //                takeScreenshotMethod,
        //                postfix: new HarmonyMethod(typeof(Hooks), nameof(TakeCharScreenshot_Postfix))
        //            );
        //        }
        //    }
        //    static void StabilizeForScreenshot(int width, int height, int renderCount)
        //    {
        //        Camera mainCam = Camera.main;
        //        RenderTexture tempRT = new RenderTexture(width, height, 24);
        //        RenderTexture originalRT = mainCam.targetTexture;

        //        mainCam.targetTexture = tempRT;

        //        for (int i = 0; i < renderCount; i++)
        //        {
        //            mainCam.Render();
        //            UnityEngine.Graphics.Blit(tempRT, (RenderTexture)null);
        //            Console.WriteLine($"Render {i + 1}/{renderCount}");
        //        }

        //        mainCam.targetTexture = originalRT;

        //        if (tempRT != null)
        //        {
        //            tempRT.Release();
        //            GameObject.Destroy(tempRT);
        //        }

        //        Console.WriteLine("Stabilization complete");
        //    }
        //    // Target really old Screenshot Manager
        //    public static System.Collections.IEnumerator WaitForEndOfFrameThen_Postfix(
        //        System.Collections.IEnumerator result,
        //        Action a,
        //        object __instance)
        //    {
        //        Dictionary<CTAA_PC, CTAASettings.CTAA_MODE> cache = null;
        //        if (settings.Enabled && a.Method.Name == "<Update>b__60_2")
        //        {
        //            var traverse = new HarmonyLib.Traverse(__instance);
        //            var w = traverse.Property("CaptureWidth").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
        //            var h = traverse.Property("CaptureHeight").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
        //            var d = traverse.Property("Downscaling").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
        //            var fw = w * d;
        //            var fh = h * d;
        //            int renderCount = settings.TemporalStability.value + 1; //Number of frames to blend
        //            // Only stabilize if target resolution is different
        //            if (fw != Screen.width || fh != Screen.height)
        //            {
        //                // Set CTAA to standard
        //                var ctaaInstances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<CTAA_PC>(true);
        //                cache = new Dictionary<CTAA_PC, CTAASettings.CTAA_MODE>();
        //                foreach (var instance in ctaaInstances)
        //                {
        //                    cache[instance] = instance.SupersampleMode;
        //                    if (instance.SupersampleMode != CTAASettings.CTAA_MODE.STANDARD)
        //                        instance.SupersampleMode = CTAASettings.CTAA_MODE.STANDARD;
        //                }

        //                result.MoveNext(); // Skip first WaitForEndOfFrame
        //                StabilizeForScreenshot(w * d, h * d, renderCount);
        //            }
        //        }
        //        // Allow rest or coroutine to run
        //        while (result.MoveNext())
        //            yield return result.Current;
        //        if (cache != null)
        //            foreach (var entry in cache)
        //                entry.Key.SupersampleMode = entry.Value;
        //    }
        //    // Target new Screenshot Manager
        //    public static System.Collections.IEnumerator TakeCharScreenshot_Postfix(
        //        System.Collections.IEnumerator result,
        //        object __instance)
        //    {
        //        Dictionary<CTAA_PC, CTAASettings.CTAA_MODE> cache = null;
        //        if (settings.Enabled)
        //        {
        //            var traverse = new HarmonyLib.Traverse(__instance);
        //            var w = traverse.Property("ResolutionX").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
        //            var h = traverse.Property("ResolutionY").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
        //            var d = traverse.Property("DownscalingRate").GetValue<BepInEx.Configuration.ConfigEntry<int>>().Value;
        //            var fw = w * d;
        //            var fh = h * d;
        //            int renderCount = settings.TemporalStability.value + 1; //Number of frames to blend
        //            // Only stabilize if target resolution is different
        //            if (fw != Screen.width || fh != Screen.height)
        //            {
        //                // Set CTAA to standard
        //                var ctaaInstances = Graphics.Instance.CameraSettings.MainCamera.GetComponentsInChildren<CTAA_PC>(true);
        //                cache = new Dictionary<CTAA_PC, CTAASettings.CTAA_MODE>();
        //                foreach (var instance in ctaaInstances)
        //                {
        //                    cache[instance] = instance.SupersampleMode;
        //                    if (instance.SupersampleMode != CTAASettings.CTAA_MODE.STANDARD)
        //                        instance.SupersampleMode = CTAASettings.CTAA_MODE.STANDARD;
        //                }

        //                result.MoveNext(); // Skip first WaitForEndOfFrame
        //                StabilizeForScreenshot(w * d, h * d, renderCount);
        //            }
        //        }
        //        // Allow rest or coroutine to run
        //        while (result.MoveNext())
        //            yield return result.Current;
        //        if (cache != null)
        //            foreach (var entry in cache)
        //                entry.Key.SupersampleMode = entry.Value;
        //    }
        //}
    }
}
