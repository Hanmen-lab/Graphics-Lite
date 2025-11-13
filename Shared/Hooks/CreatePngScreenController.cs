using Graphics.CTAA;
//using Graphics.FSR3;
using Graphics.Settings;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace Graphics
{
    public class CreatePngScreenController
    {
        public static class Hooks
        {
            public static void PatchCreatePngScreenController()
            {
                Harmony harmony = new Harmony(Graphics.GUID);

                var createPngScreenControllerType = AccessTools.TypeByName("Studio.GameScreenShot");
                if (createPngScreenControllerType == null) return;

                var createPngScreenControllerMethod = AccessTools.Method(createPngScreenControllerType, "CreatePngScreen");
                if (createPngScreenControllerMethod == null) return;

                harmony.Patch(createPngScreenControllerMethod, postfix: new HarmonyMethod(typeof(Hooks), nameof(CustomCreatePngScreen)));
                Graphics.Instance.Log.LogInfo("Hooked New CreatePngScreen()");

            }

            public static void PatchCreatePngController()
            {
                Harmony harmony = new Harmony(Graphics.GUID);

                var createPngControllerType = AccessTools.TypeByName("CharaCustom.CustomCapture");
                if (createPngControllerType == null) return;

                var createPngControllerMethod = AccessTools.Method(createPngControllerType, "CreatePng");
                if (createPngControllerMethod == null) return;

                harmony.Patch(createPngControllerMethod, postfix: new HarmonyMethod(typeof(Hooks), nameof(CustomCreatePng)));
                Graphics.Instance.Log.LogInfo("Hooked New CreatePng()");
            }

            /// <summary>
            /// This is used to override the thumbnail image in the scene in the studio.
            /// </summary>
            //[HarmonyPostfix, HarmonyPatch(typeof(Studio.GameScreenShot), "CreatePngScreen", typeof(int), typeof(int), typeof(bool), typeof(bool))]
            public static void CustomCreatePngScreen(ref byte[] __result, int _width, int _height, bool _ARGB = false, bool _cap = false)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;

                // Create Upscaled Texture
                Texture2D bigTexture = new Texture2D(_width * 4, _height * 4, _ARGB ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);

                // Create Final Texture
                Texture2D finalTexture = new Texture2D(_width * 2, _height * 2, _ARGB ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);

                int num = ((QualitySettings.antiAliasing == 0) ? 1 : QualitySettings.antiAliasing);
                RenderTexture bigRT = RenderTexture.GetTemporary(bigTexture.width, bigTexture.height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, num);
                RenderTexture smallRT = RenderTexture.GetTemporary(_width * 2, _height * 2, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);

                PostProcessingSettings postProcessingSettings = Graphics.Instance.PostProcessingSettings;
                PostProcessingSettings.Antialiasing origAAMode = postProcessingSettings.AntialiasingMode;
                postProcessingSettings.AntialiasingMode = PostProcessingSettings.Antialiasing.None;

                RenderTexture origTargetTexture = camera.targetTexture;
                camera.targetTexture = bigRT;

                for (int i = 0; i < 16; i++)
                {
                    camera.Render();
                }

                camera.targetTexture = origTargetTexture;

                // Downsample: copy bigRT to smallRT
                UnityEngine.Graphics.Blit(bigRT, smallRT);

                postProcessingSettings.AntialiasingMode = origAAMode;
                switch (origAAMode)
                {
                    //case PostProcessingSettings.Antialiasing.FSR3:
                    //    //postProcessingSettings.AntialiasingMode = origAAMode;
                    //    postProcessingSettings.UpdateFilterDithering();
                    //    FSR3Manager.UpdateSettings();
                    //    break;
                    case PostProcessingSettings.Antialiasing.CTAA:
                        //postProcessingSettings.AntialiasingMode = origAAMode;
                        CTAAManager.UpdateSettings();
                        break;
                }

                // ReadPixels from smallRT to finalTexture
                RenderTexture.active = smallRT;
                finalTexture.ReadPixels(new Rect(0f, 0f, _width * 2, _height * 2), 0, 0);
                finalTexture.Apply();
                RenderTexture.active = null;

                __result = finalTexture.EncodeToPNG();

                // Cleanup
                RenderTexture.ReleaseTemporary(bigRT);
                RenderTexture.ReleaseTemporary(smallRT);
                Object.Destroy(bigTexture);
                Object.Destroy(finalTexture);
                Resources.UnloadUnusedAssets();
            }

            public static void CustomCreatePng(ref byte[] pngData, Camera _camBG = null, Camera _camBackFrame = null, Camera _camMain = null, Camera _camFrontFrame = null)
            {
                int num = 1280;
                int num2 = 720;
                int num3 = 504;
                int num4 = 704;

                PostProcessingSettings postProcessingSettings = Graphics.Instance.PostProcessingSettings;
                PostProcessingSettings.Antialiasing origAAMode = postProcessingSettings.AntialiasingMode;
                postProcessingSettings.AntialiasingMode = PostProcessingSettings.Antialiasing.None;

                RenderTexture renderTexture;
                if (QualitySettings.antiAliasing == 0)
                {
                    renderTexture = RenderTexture.GetTemporary(num, num2, 24);
                }
                else
                {
                    renderTexture = RenderTexture.GetTemporary(num, num2, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);
                }

                RenderTexture bigRT = RenderTexture.GetTemporary(num * 2, num2 * 2, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1);

                bool sRGBWrite = GL.sRGBWrite;
                GL.sRGBWrite = true;
                if (null != _camMain)
                {
                    //RenderTexture targetTexture = _camMain.targetTexture;
                    RenderTexture origTargetTexture = _camMain.targetTexture;
                    bool allowHDR = _camMain.allowHDR;
                    _camMain.allowHDR = false;
                    _camMain.targetTexture = bigRT;
                    for (int i = 0; i < 4; i++)
                    {
                        _camMain.Render();
                    }
                    UnityEngine.Graphics.Blit(bigRT, renderTexture);
                    _camMain.targetTexture = origTargetTexture;
                    _camMain.allowHDR = allowHDR;

                }
                if (null != _camBG)
                {
                    bool allowHDR2 = _camBG.allowHDR;
                    _camBG.allowHDR = false;
                    _camBG.targetTexture = renderTexture;
                    _camBG.Render();
                    _camBG.targetTexture = null;
                    _camBG.allowHDR = allowHDR2;
                }
                if (null != _camBackFrame)
                {
                    _camBackFrame.targetTexture = renderTexture;
                    _camBackFrame.Render();
                    _camBackFrame.targetTexture = null;
                }
                if (null != _camFrontFrame)
                {
                    _camFrontFrame.targetTexture = renderTexture;
                    _camFrontFrame.Render();
                    _camFrontFrame.targetTexture = null;
                }
                GL.sRGBWrite = sRGBWrite;
                Texture2D texture2D = new Texture2D(num3, num4, TextureFormat.RGB24, false, true);
                RenderTexture.active = renderTexture;
                texture2D.ReadPixels(new Rect((float)(num - num3) / 2f, (float)(num2 - num4) / 2f, (float)num3, (float)num4), 0, 0);
                texture2D.Apply();
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(renderTexture);
                RenderTexture.ReleaseTemporary(bigRT);
                //TextureScale.Bilinear(texture2D, num3 / 2, num4 / 2);

                pngData = texture2D.EncodeToPNG();

                postProcessingSettings.AntialiasingMode = origAAMode;
                switch (origAAMode)
                {
                    //case PostProcessingSettings.Antialiasing.FSR3:
                    //    //postProcessingSettings.AntialiasingMode = origAAMode;
                    //    postProcessingSettings.UpdateFilterDithering();
                    //    FSR3Manager.UpdateSettings();
                    //    break;
                    case PostProcessingSettings.Antialiasing.CTAA:
                        //postProcessingSettings.AntialiasingMode = origAAMode;
                        CTAAManager.UpdateSettings();
                        break;
                }

                global::UnityEngine.Object.Destroy(texture2D);
            }
        }
    }
}