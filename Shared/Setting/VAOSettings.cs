using Graphics.VAO;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static Graphics.VAO.VAOEffectCommandBuffer;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class VAOSettings
    {
        public bool Enabled = false;
        /// <summary>
        /// NEW
        /// </summary>
        public FloatValue Radius = new FloatValue(0.5f, false);
        public FloatValue Power = new FloatValue(1.0f, false);
        public FloatValue Presence = new FloatValue(0.1f, false);
        public FloatValue Thickness = new FloatValue(0.25f, false);
        public FloatValue BordersIntensity = new FloatValue(0.3f, false);
        public FloatValue DetailAmountVAO = new FloatValue(0.0f, false);
        public DetailQualityType DetailQuality = DetailQualityType.Medium;
        public FloatValue DetailAmountRaycast = new FloatValue(0.0f, false);
        public IntValue Quality = new IntValue(16, false);
        public FloatValue SSAOBias = new FloatValue(0.005f, false);
        public BoolValue MaxRadiusEnabled = new BoolValue(true, false);
        public FloatValue MaxRadius = new FloatValue(0.5f, false);
        public AlgorithmType Algorithm = AlgorithmType.StandardVAO;
        public DistanceFalloffModeType DistanceFalloffMode = DistanceFalloffModeType.Off;
        public FloatValue DistanceFalloffStartAbsolute = new FloatValue(100.0f, false);
        public FloatValue DistanceFalloffStartRelative = new FloatValue(0.1f, false);
        public FloatValue DistanceFalloffSpeedAbsolute = new FloatValue(30.0f, false);
        public FloatValue DistanceFalloffSpeedRelative = new FloatValue(0.1f, false);
        public AdaptiveSamplingType AdaptiveType = AdaptiveSamplingType.EnabledAutomatic;
        public FloatValue AdaptiveQualityCoefficient = new FloatValue(1.0f, false);
        public CullingPrepassModeType CullingPrepassMode = CullingPrepassModeType.Careful;
        public IntValue Downsampling = new IntValue(1, false);
        public HierarchicalBufferStateType HierarchicalBufferState = HierarchicalBufferStateType.Auto;
        public BoolValue CommandBufferEnabled = new BoolValue(true, false);
        public BoolValue UseGBuffer = new BoolValue(true, false);
        public BoolValue UsePreciseDepthBuffer = new BoolValue(true, false);
        public BoolValue EnableTemporalFiltering = new BoolValue(true, false);
        public VAOCameraEventType VaoCameraEvent = VAOCameraEventType.AfterLighting;
        public FarPlaneSourceType FarPlaneSource = FarPlaneSourceType.Camera;
        public BoolValue IsLumaSensitive = new BoolValue(false, false);
        public LuminanceModeType LuminanceMode = LuminanceModeType.Luma;
        public FloatValue LumaThreshold = new FloatValue(0.7f, false);
        public FloatValue LumaKneeWidth = new FloatValue(0.3f, false);
        public FloatValue LumaKneeLinearity = new FloatValue(3.0f, false);
        public EffectMode Mode = EffectMode.ColorTint;

        public Color ColorTint = new Color( 0, 0, 0 ); 

        public FloatValue ColorBleedPower = new FloatValue(5.0f, false);
        public FloatValue ColorBleedPresence = new FloatValue(1.0f, false);
        public ScreenTextureFormat IntermediateScreenTextureFormat = ScreenTextureFormat.Auto;
        public BoolValue ColorbleedHueSuppresionEnabled = new BoolValue(true, true);
        public FloatValue ColorBleedHueSuppresionThreshold = new FloatValue(7.0f, false);
        public FloatValue ColorBleedHueSuppresionWidth = new FloatValue(2.0f, false);
        public FloatValue ColorBleedHueSuppresionSaturationThreshold = new FloatValue(0.5f, false);
        public FloatValue ColorBleedHueSuppresionSaturationWidth = new FloatValue(0.2f, false);
        public FloatValue ColorBleedHueSuppresionBrightness = new FloatValue(0.0f, false);
        //Int as enum
        public IntValue ColorBleedQuality = new IntValue(2, false);
        public ColorBleedSelfOcclusionFixLevelType ColorBleedSelfOcclusionFixLevel = ColorBleedSelfOcclusionFixLevelType.Hard;
        public BoolValue GiBackfaces = new BoolValue(false, false);
        public BlurQualityType BlurQuality = BlurQualityType.Precise;
        public BlurModeType BlurMode = BlurModeType.Basic;
        public IntValue EnhancedBlurSize = new IntValue(5, false);
        public FloatValue EnhancedBlurDeviation = new FloatValue(1.3f, false);
        public BoolValue OutputAOOnly = new BoolValue(false, false);
        public NormalsSourceType NormalsSource = NormalsSourceType.GBuffer;

        public void Load(VAOEffectCommandBuffer vao)
        {
            if (vao == null)
                return;

            vao.enabled = Enabled;
            if (Radius.overrideState)
                vao.Radius = Radius.value;
            else
                vao.Radius = 0.5f;

            if (Power.overrideState)
                vao.Power = Power.value;
            else
                vao.Power = 1.0f;

            if (Presence.overrideState)
                vao.Presence = Presence.value;
            else
                vao.Presence = 0.1f;

            if (Thickness.overrideState)
                vao.Thickness = Thickness.value;
            else
                vao.Thickness = 0.25f;

            if (BordersIntensity.overrideState)
                vao.BordersIntensity = BordersIntensity.value;
            else
                vao.BordersIntensity = 0.3f;

            vao.DetailQuality = DetailQuality;

            if (DetailAmountVAO.overrideState)
                vao.DetailAmountVAO = DetailAmountVAO.value;
            else
                vao.DetailAmountVAO = 0.0f;

            if (DetailAmountRaycast.overrideState)
                vao.DetailAmountRaycast = DetailAmountRaycast.value;
            else
                vao.DetailAmountRaycast = 0.0f;            

            if (Quality.overrideState)
                vao.Quality = Quality.value;
            else
                vao.Quality = 16;

            if (SSAOBias.overrideState)
                vao.SSAOBias = SSAOBias.value;
            else
                vao.SSAOBias = 0.005f;

            vao.MaxRadiusEnabled = MaxRadiusEnabled.value;

            if (MaxRadius.overrideState)
                vao.MaxRadius = MaxRadius.value;
            else
                vao.MaxRadius = 0.5f;

            vao.Algorithm = Algorithm;

            vao.DistanceFalloffMode = DistanceFalloffMode;

            if (DistanceFalloffStartAbsolute.overrideState)
                vao.DistanceFalloffStartAbsolute = DistanceFalloffStartAbsolute.value;
            else
                vao.DistanceFalloffStartAbsolute = 100.0f;

            if (DistanceFalloffStartRelative.overrideState)
                vao.DistanceFalloffStartRelative = DistanceFalloffStartRelative.value;
            else
                vao.DistanceFalloffStartRelative = 0.1f;

            if (DistanceFalloffSpeedAbsolute.overrideState)
                vao.DistanceFalloffSpeedAbsolute = DistanceFalloffSpeedAbsolute.value;
            else
                vao.DistanceFalloffSpeedAbsolute = 30.0f;

            if (DistanceFalloffSpeedRelative.overrideState)
                vao.DistanceFalloffSpeedRelative = DistanceFalloffSpeedRelative.value;
            else
                vao.DistanceFalloffSpeedRelative = 0.1f;

            vao.AdaptiveType = AdaptiveType;

            if (AdaptiveQualityCoefficient.overrideState)
                vao.AdaptiveQualityCoefficient = AdaptiveQualityCoefficient.value;
            else
                vao.AdaptiveQualityCoefficient = 1.0f;

            vao.CullingPrepassMode = CullingPrepassMode;

            if (Downsampling.overrideState)
                vao.Downsampling = Downsampling.value;
            else
                vao.Downsampling = 1;

            vao.HierarchicalBufferState = HierarchicalBufferState;

            vao.CommandBufferEnabled = CommandBufferEnabled.value;

            vao.UseGBuffer = UseGBuffer.value;

            vao.UsePreciseDepthBuffer = UsePreciseDepthBuffer.value;

            vao.EnableTemporalFiltering = EnableTemporalFiltering.value;

            vao.IsLumaSensitive = IsLumaSensitive.value;

            if (LumaThreshold.overrideState)
                vao.LumaThreshold = LumaThreshold.value;
            else
                vao.LumaThreshold = 0.7f;

            if (LumaKneeWidth.overrideState)
                vao.LumaKneeWidth = LumaKneeWidth.value;
            else
                vao.LumaKneeWidth = 0.3f;

            if (LumaKneeLinearity.overrideState)
                vao.LumaKneeLinearity = LumaKneeLinearity.value;
            else
                vao.LumaKneeLinearity = 3.0f;

            if (ColorBleedPower.overrideState)
                vao.ColorBleedPower = ColorBleedPower.value;
            else
                vao.ColorBleedPower = 5.0f;

            vao.ColorTint = ColorTint;

            if (ColorBleedPresence.overrideState)
                vao.ColorBleedPresence = ColorBleedPresence.value;
            else
                vao.ColorBleedPresence = 1.0f;

            vao.ColorbleedHueSuppresionEnabled = ColorbleedHueSuppresionEnabled.value;

            if (ColorBleedHueSuppresionThreshold.overrideState)
                vao.ColorBleedHueSuppresionThreshold = ColorBleedHueSuppresionThreshold.value;
            else
                vao.ColorBleedHueSuppresionThreshold = 7.0f;

            if (ColorBleedHueSuppresionWidth.overrideState)
                vao.ColorBleedHueSuppresionWidth = ColorBleedHueSuppresionWidth.value;
            else
                vao.ColorBleedHueSuppresionWidth = 2.0f;

            if (ColorBleedHueSuppresionSaturationThreshold.overrideState)
                vao.ColorBleedHueSuppresionSaturationThreshold = ColorBleedHueSuppresionSaturationThreshold.value;
            else
                vao.ColorBleedHueSuppresionSaturationThreshold = 0.5f;

            if (ColorBleedHueSuppresionSaturationWidth.overrideState)
                vao.ColorBleedHueSuppresionSaturationWidth = ColorBleedHueSuppresionSaturationWidth.value;
            else
                vao.ColorBleedHueSuppresionSaturationWidth = 0.2f;

            if (ColorBleedHueSuppresionBrightness.overrideState)
                vao.ColorBleedHueSuppresionBrightness = ColorBleedHueSuppresionBrightness.value;
            else
                vao.ColorBleedHueSuppresionBrightness = 0.0f;

            if (ColorBleedQuality.overrideState)
                vao.ColorBleedQuality = ColorBleedQuality.value;
            else
                vao.ColorBleedQuality = 2;

            vao.ColorBleedSelfOcclusionFixLevel = ColorBleedSelfOcclusionFixLevel;

            vao.VaoCameraEvent = VaoCameraEvent;
            vao.FarPlaneSource = FarPlaneSource;
            vao.IntermediateScreenTextureFormat = IntermediateScreenTextureFormat;

            vao.GiBackfaces = GiBackfaces.value;

            vao.LuminanceMode = LuminanceMode;

            vao.BlurMode = BlurMode;
            vao.BlurQuality = BlurQuality;

            vao.Mode = Mode;

            if (EnhancedBlurSize.overrideState)
                vao.EnhancedBlurSize = EnhancedBlurSize.value;
            else
                vao.EnhancedBlurSize = 5;

            if (EnhancedBlurDeviation.overrideState)
                vao.EnhancedBlurDeviation = EnhancedBlurDeviation.value;
            else
                vao.EnhancedBlurDeviation = 1.3f;

            vao.OutputAOOnly = OutputAOOnly.value;
            vao.NormalsSource = NormalsSource;
        }

        public void Save(VAOEffect vao)
        {
            if (vao == null)
                return;

            Enabled = vao.enabled;
            Radius.value = vao.Radius;
            Power.value = vao.Power;
            Presence.value = vao.Presence;
            Thickness.value = vao.Thickness;
            BordersIntensity.value = vao.BordersIntensity;
            DetailAmountVAO.value = vao.DetailAmountVAO;
            DetailQuality = vao.DetailQuality;
            DetailAmountRaycast.value = vao.DetailAmountRaycast;
            Quality.value = vao.Quality;
            SSAOBias.value = vao.SSAOBias;
            MaxRadiusEnabled.value = vao.MaxRadiusEnabled;
            MaxRadius.value = vao.MaxRadius;
            Algorithm = vao.Algorithm;
            DistanceFalloffMode = vao.DistanceFalloffMode;
            DistanceFalloffStartAbsolute.value = vao.DistanceFalloffStartAbsolute;
            DistanceFalloffStartRelative.value = vao.DistanceFalloffStartRelative;
            DistanceFalloffSpeedAbsolute.value = vao.DistanceFalloffSpeedAbsolute;
            DistanceFalloffSpeedRelative.value = vao.DistanceFalloffSpeedRelative;
            AdaptiveType = vao.AdaptiveType;
            AdaptiveQualityCoefficient.value = vao.AdaptiveQualityCoefficient;
            CullingPrepassMode = vao.CullingPrepassMode;
            Downsampling.value = vao.Downsampling;
            HierarchicalBufferState = vao.HierarchicalBufferState;
            CommandBufferEnabled.value = vao.CommandBufferEnabled;
            UseGBuffer.value = vao.UseGBuffer;
            UsePreciseDepthBuffer.value = vao.UsePreciseDepthBuffer;
            EnableTemporalFiltering.value = vao.EnableTemporalFiltering;
            VaoCameraEvent = vao.VaoCameraEvent;
            FarPlaneSource = vao.FarPlaneSource;
            IsLumaSensitive.value = vao.IsLumaSensitive;
            LumaThreshold.value = vao.LumaThreshold;
            LuminanceMode = vao.LuminanceMode;    
            LumaThreshold.value = vao.LumaThreshold;
            LumaKneeWidth.value = vao.LumaKneeWidth;
            LumaKneeLinearity.value = vao.LumaKneeLinearity;
            Mode = vao.Mode;
            ColorTint = vao.ColorTint;
            ColorBleedPower.value = vao.ColorBleedPower;
            ColorBleedPresence.value = vao.ColorBleedPresence;
            IntermediateScreenTextureFormat = vao.IntermediateScreenTextureFormat;
            ColorbleedHueSuppresionEnabled.value = vao.ColorbleedHueSuppresionEnabled;
            ColorBleedHueSuppresionThreshold.value = vao.ColorBleedHueSuppresionThreshold;
            ColorBleedHueSuppresionWidth.value = vao.ColorBleedHueSuppresionWidth;
            ColorBleedHueSuppresionSaturationThreshold.value = vao.ColorBleedHueSuppresionSaturationThreshold;
            ColorBleedHueSuppresionSaturationWidth.value = vao.ColorBleedHueSuppresionSaturationWidth;
            ColorBleedHueSuppresionBrightness.value = vao.ColorBleedHueSuppresionBrightness;
            ColorBleedQuality.value = vao.ColorBleedQuality;
            ColorBleedSelfOcclusionFixLevel = vao.ColorBleedSelfOcclusionFixLevel;
            GiBackfaces.value = vao.GiBackfaces;
            BlurQuality = vao.BlurQuality;
            BlurMode = vao.BlurMode;
            EnhancedBlurSize.value = vao.EnhancedBlurSize;
            EnhancedBlurDeviation.value = vao.EnhancedBlurDeviation;
            OutputAOOnly.value = vao.OutputAOOnly;
            NormalsSource = vao.NormalsSource;

        }
    }
}
