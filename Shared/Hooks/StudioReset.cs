using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Graphics.Hooks
{
    public class StudioReset
    {
        public static void InitializeStudioHooks()
        {
            Harmony harmony = new Harmony(Graphics.GUID);

            HarmonyMethod studioInit = new HarmonyMethod(typeof(StudioReset), "LoadPresetOnStudioReset");
            harmony.Patch(AccessTools.Method(typeof(Studio.Studio), "InitScene"), null, studioInit);
        }

        public static void LoadPresetOnStudioReset()
        {
            Graphics.Instance.StartCoroutine(DoLoadPresetOnStudioReset());
        }

        private static IEnumerator DoLoadPresetOnStudioReset()
        {
            yield return null;
            Graphics.Instance.PresetManager?.LoadDefaultForCurrentGameMode();
            Graphics.Instance.SkyboxManager.SetupDefaultReflectionProbe(Graphics.Instance.LightingSettings, false);
        }
    }
}
