using Graphics.GTAO;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Graphics
{
    public class SSSMirrorHooks
    {

        public static void InitializeMirrorHooks()
        {
            Harmony harmony = new Harmony(Graphics.GUID);

            try
            {
                niceMirrorReflectionType = AccessTools.TypeByName("NiceMirrorReflection");
            }
            catch { }
            try
            {
                monPlaneType = AccessTools.TypeByName("RockyScript.MonPlane");
            }
            catch { }

            HarmonyMethod sssComponentAdd = new HarmonyMethod(typeof(SSSMirrorHooks), "AddSSSComponentToMirrorCamera");
            harmony.Patch(AccessTools.Method(typeof(MirrorReflection), "CreateMirrorObjects"), null, sssComponentAdd);
            if (monPlaneType != null)
            {
                harmony.Patch(AccessTools.Method(monPlaneType, "Start"), null, new HarmonyMethod(typeof(SSSMirrorHooks), "AddMonPlaneSSSComponent"));
                planeCamField = AccessTools.Field(monPlaneType, "planeCam");
                refreshCamMethod = AccessTools.Method(monPlaneType, "RefreshCam");
            }
            if (niceMirrorReflectionType != null)
                harmony.Patch(AccessTools.Method(niceMirrorReflectionType, "CreateMirrorObjects"), null, sssComponentAdd);
        }

        private static Type niceMirrorReflectionType;
        private static Type monPlaneType;
        private static FieldInfo planeCamField;
        private static MethodInfo refreshCamMethod;

        private static void AddMonPlaneSSSComponent(object __instance)
        {            
            Camera planeCam = (Camera)planeCamField.GetValue(__instance);
            if (planeCam.gameObject.GetComponent<SSS>() == null)
            {
        //        Graphics.Instance.Log.LogInfo($"Adding SSS Component to Camera: {planeCam.name} GO: {planeCam.gameObject.name}");
                SSS mirrorSSS = planeCam.gameObject.AddComponent<SSS>();
                mirrorSSS.enabled = true;
                mirrorSSS.Enabled = true;
                SSSManager.RegisterAdditionalInstance(mirrorSSS);

                if (Graphics.Instance.CameraSettings.MainCamera != null && Graphics.Instance.CameraSettings.MainCamera.stereoEnabled)
                {
                    float prevFov = planeCam.fieldOfView;
                    planeCam.CopyFrom(Graphics.Instance.CameraSettings.MainCamera);
                    planeCam.fieldOfView = prevFov;
                    refreshCamMethod.Invoke(__instance, new object[] { });
                }
            }
            if (planeCam.gameObject.GetComponent<GroundTruthAmbientOcclusion>() == null)
            {
                GroundTruthAmbientOcclusion gtao = planeCam.gameObject.AddComponent<GroundTruthAmbientOcclusion>();
                GTAOManager.RegisterAdditionalInstance(gtao);
            }
            if (planeCam.gameObject.GetComponent<VAO.VAOEffectCommandBuffer>() == null && planeCam.gameObject.GetComponent<VAO.VAOEffect>() == null)
            {
                VAO.VAOEffect vao = planeCam.gameObject.AddComponent<VAO.VAOEffect>();
                VAO.VAOManager.RegisterAdditionalInstance(vao);
            }
        }

        private static void AddSSSComponentToMirrorCamera(Camera currentCamera, Camera reflectionCamera)
        {
            if (reflectionCamera.gameObject.GetComponent<SSS>() == null)
            {
                Graphics.Instance.Log.LogInfo($"Adding SSS Component to Camera: {reflectionCamera.name} GO: {reflectionCamera.gameObject.name}");
                SSS mirrorSSS = reflectionCamera.gameObject.AddComponent<SSS>();
                mirrorSSS.enabled = true;
                mirrorSSS.Enabled = true;
                mirrorSSS.MirrorSSS = true;
                SSSManager.RegisterAdditionalInstance(mirrorSSS);
            }
            if (reflectionCamera.gameObject.GetComponent<GroundTruthAmbientOcclusion>() == null)
            {
                GroundTruthAmbientOcclusion gtao = reflectionCamera.gameObject.AddComponent<GroundTruthAmbientOcclusion>();
                GTAOManager.RegisterAdditionalInstance(gtao);
            }
            if (reflectionCamera.gameObject.GetComponent<VAO.VAOEffectCommandBuffer>() == null && reflectionCamera.gameObject.GetComponent<VAO.VAOEffect>() == null)
            {
                VAO.VAOEffect vao = reflectionCamera.gameObject.AddComponent<VAO.VAOEffect>();
                VAO.VAOManager.RegisterAdditionalInstance(vao);
            }
        }

    }
}
