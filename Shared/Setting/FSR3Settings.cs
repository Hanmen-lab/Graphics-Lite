using FidelityFX;
using FidelityFX.FSR3;
using Graphics.CTAA;
using Graphics.FSR3;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class FSR3Settings
    {
        public bool Enabled = false;
        public Fsr3Upscaler.QualityMode QualityMode = Fsr3Upscaler.QualityMode.NativeAA;
        public BoolValue PerformSharpenPass = new BoolValue(true, false);
        public FloatValue Sharpness = new FloatValue(0.8f, false);
        public FloatValue VelocityFactor = new FloatValue(1.0f, false);

        // EXPOSURE
        public BoolValue EnableAutoExposure = new BoolValue(true, false);
        public FloatValue PreExposure = new FloatValue(1.0f, false);
        //public Texture Exposure = null;

        // DEBUG
        public BoolValue EnableDebugView = new BoolValue(false, false);

        // REACTIVITY, TRANSPARENCY & COMPOSITION
        //public Texture ReactiveMask = null;
        //public Texture TransparencyAndCompositionMask = null;

        // GENERATE REACTIVE PARAMETERS
        public BoolValue AutoGenerateReactiveMask = new BoolValue(true, false);
        public FloatValue Scale = new FloatValue(0.5f, false);
        public FloatValue CutoffThreshold = new FloatValue(0.2f, false);
        public FloatValue BinaryValue = new FloatValue(0.9f, false);
        public Fsr3Upscaler.GenerateReactiveFlags Flags = Fsr3Upscaler.GenerateReactiveFlags.ApplyTonemap
            | Fsr3Upscaler.GenerateReactiveFlags.ApplyThreshold
            | Fsr3Upscaler.GenerateReactiveFlags.UseComponentsMax;

        // EXPERIMENTAL
        public BoolValue AutoGenerateTransparencyAndComposition = new BoolValue(false, false);
        public FloatValue AutoTcThreshold = new FloatValue(0.05f, false);
        public FloatValue AutoTcScale = new FloatValue(1.0f, false);
        public FloatValue AutoReactiveScale = new FloatValue(5.0f, false);
        public FloatValue AutoReactiveMax = new FloatValue(0.9f, false);

        public void Load(Fsr3UpscalerImageEffect fsr3)
        {
            Fsr3UpscalerImageEffectHelper helper = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<Fsr3UpscalerImageEffectHelper>();
            PostProcessingSettings postProcessingSettings = Graphics.Instance.PostProcessingSettings;
            
            fsr3.enabled = Enabled;
            helper.enabled = Enabled;

            ////Disable Sharpening in PostProcessing when FSR3 is enabled
            //if (Enabled)
            //    postProcessingSettings.sharpen3in1Layer.active = false;

            fsr3.qualityMode = QualityMode;

            fsr3.performSharpenPass = PerformSharpenPass.value;

            if (Sharpness.overrideState)
                fsr3.sharpness = Sharpness.value;
            else
                fsr3.sharpness = 0.8f;

            if (VelocityFactor.overrideState)
                fsr3.velocityFactor = VelocityFactor.value;
            else
                fsr3.velocityFactor = 1.0f;

            fsr3.enableAutoExposure = EnableAutoExposure.value;

            if (PreExposure.overrideState)
                fsr3.preExposure = PreExposure.value;
            else
                fsr3.preExposure = 0.475f;

            //fsr3.exposure = Exposure;

            fsr3.enableDebugView = EnableDebugView.value;
            
            //fsr3.reactiveMask = ReactiveMask;

            //fsr3.transparencyAndCompositionMask = TransparencyAndCompositionMask;

            fsr3.autoGenerateReactiveMask = AutoGenerateReactiveMask.value;
            
            if (Scale.overrideState)
                fsr3.GenerateReactiveParams.scale = Scale.value;
            else
                fsr3.GenerateReactiveParams.scale = 0.5f;

            if (CutoffThreshold.overrideState)
                fsr3.GenerateReactiveParams.cutoffThreshold = CutoffThreshold.value;
            else
                fsr3.GenerateReactiveParams.cutoffThreshold = 0.2f;

            if (BinaryValue.overrideState)
                fsr3.GenerateReactiveParams.binaryValue = BinaryValue.value;
            else
                fsr3.GenerateReactiveParams.binaryValue = 0.9f;

            fsr3.GenerateReactiveParams.flags = Flags;

            fsr3.autoGenerateTransparencyAndComposition = AutoGenerateTransparencyAndComposition.value;
            
            if (AutoTcThreshold.overrideState)
                fsr3.GenerateTcrParams.autoTcThreshold = AutoTcThreshold.value;
            else
                fsr3.GenerateTcrParams.autoTcThreshold = 0.05f;

            if (AutoTcScale.overrideState)
                fsr3.GenerateTcrParams.autoTcScale = AutoTcScale.value;
            else
                fsr3.GenerateTcrParams.autoTcScale = 1.0f;

            if (AutoReactiveScale.overrideState)
                fsr3.GenerateTcrParams.autoReactiveScale = AutoReactiveScale.value;
            else
                fsr3.GenerateTcrParams.autoReactiveScale = 5.0f;

            if (AutoReactiveMax.overrideState)
                fsr3.GenerateTcrParams.autoReactiveMax = AutoReactiveMax.value;
            else
                fsr3.GenerateTcrParams.autoReactiveMax = 0.9f;
        }
    }
}
