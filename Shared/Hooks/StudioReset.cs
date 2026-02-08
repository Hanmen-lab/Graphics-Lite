using HarmonyLib;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics.Hooks
{
    public class StudioReset
    {
        public static void InitializeStudioHooks()
        {
            Harmony harmony = new Harmony(Graphics.GUID);

            HarmonyMethod studioInit = new HarmonyMethod(typeof(StudioReset), "LoadPresetOnStudioReset");
            harmony.Patch(AccessTools.Method(typeof(Studio.Studio), "InitScene"), null, studioInit);

            // Disable/Enable Decal when checkbox toggled in Studio.
            HarmonyMethod studioVisible = new HarmonyMethod(typeof(StudioReset), "OCIItem_Visible_Decal_Hook");
            harmony.Patch(AccessTools.Method(typeof(Studio.OCIItem), "set_visible"), null, studioVisible);

            // Show Decal Gizmo when selected. Hook only runs when selection contains an IFormData.
            HarmonyMethod studioSelect = new HarmonyMethod(typeof(StudioReset), "MakeFormHook");
            harmony.Patch(AccessTools.Method(typeof(AdvancedStudioUI.StudioItemControl), "MakeForm"), null, studioSelect);

            // Hide old Decal Gizmos.
            HarmonyMethod studioSelect2 = new HarmonyMethod(typeof(StudioReset), "OnSelectStudioItem");
            harmony.Patch(AccessTools.Method(typeof(HooahComponents.Hooks.Hooks), "OnSelectStudioItem"), studioSelect2, null);
            harmony.Patch(AccessTools.Method(typeof(HooahComponents.Hooks.Hooks), "OnDeselectStudioItem"), studioSelect2, null);

            HarmonyMethod breakIt = new HarmonyMethod(typeof(StudioReset), "BreakIt");

            var HDSaveCard = Type.GetType("HDSaveCard.HS2_HDSaveCard, HS2_HDSaveCard");
            if (HDSaveCard != null)
            {
                // Since we can't hook early enough to prevent the hook, instead break the function it patches
                harmony.Patch(AccessTools.Method(HDSaveCard, "CharaCustom_CustomCapture_CreatePng"), breakIt);
            }

            var BetterAA = Type.GetType("AI_stuff.BetterAA, HS2_BetterAA");
            if (BetterAA != null)
            {
                // Since we can't hook early enough to prevent the hook, instead break the function it patches
                harmony.Patch(AccessTools.Method(BetterAA, "CameraCreateHook"), breakIt);
                // Also break all other functions in this plugin
                harmony.Patch(AccessTools.Method(BetterAA, "UpdateEnabledState"), breakIt);
                harmony.Patch(AccessTools.Method(BetterAA, "UpdateSettings"), breakIt);
                harmony.Patch(AccessTools.Method(BetterAA, "AddCamera"), breakIt);
            }

            //var DofToggle = Type.GetType("itsnt.doftoggle, DoF Toggle");
            //if (DofToggle != null)
            //{
            //    // Since we can't hook early enough to prevent the hook, instead break the function it patches
            //    harmony.Patch(AccessTools.Method(DofToggle, "RegisterToolbar"), breakIt);
            //}
        }

        public static bool BreakIt()
        {
            Console.WriteLine("Blocked");
            return false;
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
        public static void OCIItem_Visible_Decal_Hook(OCIItem __instance, bool __0)
        {
            Graphics.Instance.StartCoroutine(Do_OCIItem_Visible_Decal_Hook(__instance, __0));
        }
        public static IEnumerator Do_OCIItem_Visible_Decal_Hook(OCIItem __instance, bool __0)
        {
            for (var i = 0; i < 5; i++) // Need slight delay when loading scene
                yield return null;
            var decal = __instance.objectItem.GetComponent<Knife.DeferredDecals.DecalProxy>();
            if (decal && decal.SourceDecal)
                decal.SourceDecal.enabled = __0;
        }

        public static List<Knife.DeferredDecals.DecalProxy> SelectedDecals = new List<Knife.DeferredDecals.DecalProxy>();
        public static void MakeFormHook(GameObject[] gameObjects)
        {
            // Check the new gameObjects for decal components.
            if (gameObjects != null)
                foreach (var go in gameObjects)
                {
                    if (go == null) continue;

                    // If a component is found, add it directly to our cached list.
                    var decalProxy = go.GetComponent<Knife.DeferredDecals.DecalProxy>();
                    if (decalProxy != null)
                        SelectedDecals.Add(decalProxy);
                }

            // Finally, iterate through the newly populated list and enable the gizmos.
            foreach (var decal in SelectedDecals)
                if (decal != null && decal.SourceDecal != null)
                    decal.SourceDecal.NeedDrawGizmo = true;
        }
        public static void OnSelectStudioItem()
        {
            // Iterate over the current decals in the list and disable their gizmos.
            foreach (var decal in SelectedDecals)
                if (decal != null && decal.SourceDecal != null)
                    decal.SourceDecal.NeedDrawGizmo = false;

            // Clear the list to prepare for the new selection.
            SelectedDecals.Clear();
        }
    }
}
