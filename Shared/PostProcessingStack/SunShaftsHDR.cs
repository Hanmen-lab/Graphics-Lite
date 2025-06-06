using KKAPI.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Graphics
{
    [Serializable]
    [PostProcess(typeof(SunShaftsHDRRenderer), PostProcessEvent.BeforeStack, "Custom/SunShaftsHDR")]
    public sealed class SunShaftsHDR : PostProcessEffectSettings
    {
        [Range(0f, 1f), Tooltip("SunShafts effect intensity.")]
        public FloatParameter blend = new FloatParameter { value = 0.5f };

        public enum SunShaftsResolution
        {
            Low = 0,
            Normal = 1,
            High = 2,
        }

        public enum ShaftsScreenBlendMode
        {
            Screen = 0,
            Add = 1,
        }

        public SunShaftsResolution resolution = SunShaftsResolution.High;
        public ShaftsScreenBlendMode screenBlendMode = ShaftsScreenBlendMode.Screen;

        public Vector3Parameter sunTransform = new Vector3Parameter { value = new Vector3(0f, 0f, 0f) }; // Transform sunTransform;

        public IntParameter radialBlurIterations = new IntParameter { value = 2 };

        [ColorUsage(false, true)]
        public ColorParameter sunColor = new ColorParameter { value = new Color(1f, 1f, 1f, 1f) };

        public ColorParameter sunThreshold = new ColorParameter { value = new Color(0.87f, 0.74f, 0.65f) };

        public FloatParameter sunShaftBlurRadius = new FloatParameter { value = 2.5f };

        public FloatParameter sunShaftIntensity = new FloatParameter { value = 1.15f };

        public FloatParameter maxRadius = new FloatParameter { value = 0.75f };

        public BoolParameter useDepthTexture = new BoolParameter { value = true };

        public BoolParameter connectSun = new BoolParameter { value = false };
        public IntParameter priority = new IntParameter { value = 100 };
    }

    public sealed class SunShaftsHDRRenderer : PostProcessEffectRenderer<SunShaftsHDR>
    {

        Shader shader;
        AssetBundle assetBundle;

        //v5.0
        public override void Init()
        {
            if (shader != null) return;
            //Load shaders from Assetbundle
            assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("sunshaftshdr.unity3d"));
            //if (assetBundle == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load asset bunle 'tiltshift.unity3d'");
            //DontDestroyOnLoad(assetBundle);
            shader = assetBundle.LoadAsset<Shader>("Assets/SunShaftsHDR/Shaders/SunShaftsHDR.shader");
            //if (shader == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load shader 'Assets/TiltShift.shader'");
            //DontDestroyOnLoad(tiltshader);
            assetBundle.Unload(false);
        }


        RenderTexture lrColorB;
        RenderTexture lrDepthBuffer;


        public override void Render(PostProcessRenderContext context)
        {
            var sheetSHAFTS = context.propertySheets.Get(shader);
            sheetSHAFTS.properties.SetFloat("_Blend", settings.blend);

            Camera camera = Camera.main;
            // we actually need to check this every frame
            if (settings.useDepthTexture)
            {
                camera.depthTextureMode |= DepthTextureMode.Depth;
            }

            Vector3 v = Vector3.one * 0.5f;
            if (settings.sunTransform != Vector3.zero)
                v = camera.WorldToViewportPoint(settings.sunTransform);
            else
                v = new Vector3(0.5f, 0.5f, 0.0f);

            if (settings.connectSun)
                v = camera.WorldToViewportPoint(camera.transform.position - UnityEngine.RenderSettings.sun.transform.forward * camera.nearClipPlane * 2);

            int rtW = context.width; //source.width / divider;
            int rtH = context.width; //source.height / divider;

            lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0);

            //mask out everything except the skybox
            //we have 2 methods, one of which requires depth buffer support, the other one is just comparing images
            sheetSHAFTS.properties.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * settings.sunShaftBlurRadius);
            sheetSHAFTS.properties.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, settings.maxRadius));
            sheetSHAFTS.properties.SetVector("_SunThreshold", settings.sunThreshold);

            if (!settings.useDepthTexture)
            {
                var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default; //v3.4.9
                RenderTexture tmpBuffer = RenderTexture.GetTemporary(context.width, context.height, 0, format);
                RenderTexture.active = tmpBuffer;
                GL.ClearWithSkybox(false, camera);

                sheetSHAFTS.properties.SetTexture("_Skybox", tmpBuffer);
                context.command.BlitFullscreenTriangle(context.source, lrDepthBuffer, sheetSHAFTS, 3);
                RenderTexture.ReleaseTemporary(tmpBuffer);
            }
            else
            {
                context.command.BlitFullscreenTriangle(context.source, lrDepthBuffer, sheetSHAFTS, 2);

            }

            int radialBlurIterations = Mathf.Clamp(settings.radialBlurIterations, 1, 4);

            float ofs = settings.sunShaftBlurRadius * (1.0f / 768.0f);

            sheetSHAFTS.properties.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            sheetSHAFTS.properties.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, settings.maxRadius));

            for (int it2 = 0; it2 < radialBlurIterations; it2++)
            {
                // each iteration takes 2 * 6 samples
                // we update _BlurRadius each time to cheaply get a very smooth look

                lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);

                context.command.BlitFullscreenTriangle(lrDepthBuffer, lrColorB, sheetSHAFTS, 1);
                RenderTexture.ReleaseTemporary(lrDepthBuffer);
                ofs = settings.sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;

                sheetSHAFTS.properties.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

                lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0);

                context.command.BlitFullscreenTriangle(lrColorB, lrDepthBuffer, sheetSHAFTS, 1);
                RenderTexture.ReleaseTemporary(lrColorB);
                ofs = settings.sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;

                sheetSHAFTS.properties.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
            }

            // put together:

            if (v.z >= 0.0f)
            {
                sheetSHAFTS.properties.SetVector("_SunColor", new Vector4(settings.sunColor.value.linear.r, settings.sunColor.value.linear.g, settings.sunColor.value.linear.b, settings.sunColor.value.linear.a) * settings.sunShaftIntensity);
            }
            else
            {
                sheetSHAFTS.properties.SetVector("_SunColor", Vector4.zero); // no backprojection !
            }

            sheetSHAFTS.properties.SetTexture("_ColorBuffer", lrDepthBuffer);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheetSHAFTS, (settings.screenBlendMode == SunShaftsHDR.ShaftsScreenBlendMode.Add) ? 0 : 4);

            RenderTexture.ReleaseTemporary(lrDepthBuffer);

        }
    }
}