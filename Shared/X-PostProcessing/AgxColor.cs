
//----------------------------------------------------------------------------------------------------------
// X-PostProcessing Library
// https://github.com/QianMo/X-PostProcessing-Library
// Copyright (C) 2020 QianMo. All rights reserved.
// Licensed under the MIT License 
// You may not use this file except in compliance with the License.You may obtain a copy of the License at
// http://opensource.org/licenses/MIT
//----------------------------------------------------------------------------------------------------------

using KKAPI.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;


namespace Graphics
{

    [Serializable]
    [PostProcess(typeof(AgxColorRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/Color/AgXColor")]
    public class AgxColor : PostProcessEffectSettings
    {
        [Range(-1f, 1f)]
        public FloatParameter temperature = new FloatParameter { value = 0f };

        [Range(-1f, 1f)]
        public FloatParameter tint = new FloatParameter { value = 0f };

        [Range(0.0f, 2.0f)]
        public FloatParameter saturation = new FloatParameter { value = 1f };

        [Range(-0.9f, 1f)]
        public FloatParameter brightness = new FloatParameter { value = 0f };
    }

    public sealed class AgxColorRenderer : PostProcessEffectRenderer<AgxColor>
    {
        private const string PROFILER_TAG = "X-GlitchScanLineJitter";
        private Shader shader;
        AssetBundle assetbundle;

        public override void Init()
        {
            if (shader != null) return;

            assetbundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource("xcolor.unity3d"));
            if (assetbundle == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load asset bunle 'xcolor.unity3d'");

            shader = assetbundle.LoadAsset<Shader>("Assets/X-PostProcessing/Effects/XColorAdjustment/XColorAdjustment.shader");
            if (shader == null) Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, "failed to load shader 'ColorAdjustment.shader'");

            assetbundle.Unload(false);
        }

        public override void Release()
        {
            base.Release();
        }

        static class ShaderIDs
        {
            internal static readonly int Params = Shader.PropertyToID("_Params");
            internal static readonly int JitterIndensity = Shader.PropertyToID("_ScanLineJitter");
        }

        public override void Render(PostProcessRenderContext context)
        {
            CommandBuffer cmd = context.command;
            PropertySheet sheet = context.propertySheets.Get(shader);
            cmd.BeginSample(PROFILER_TAG);

            sheet.properties.SetFloat("_Temperature", settings.temperature * 0.1f);
            sheet.properties.SetFloat("_Tint", settings.tint);
            sheet.properties.SetFloat("_Saturation", settings.saturation);
            sheet.properties.SetFloat("_Brightness", settings.brightness + 1f);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            cmd.EndSample(PROFILER_TAG);
        }
    }
}
        
