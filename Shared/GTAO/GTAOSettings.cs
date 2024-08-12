using Graphics.GTAO;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using static Graphics.GTAO.GroundTruthAmbientOcclusion;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class GTAOSettings
    {
        public enum OutPass
        {
            Combined = 4,
            AO = 5,
            RO = 6,
            BentNormal = 7
        };

        public bool Enabled = false;
        public IntValue DirSampler = new IntValue(4, false);
        public IntValue SliceSampler = new IntValue(8, false);
        public FloatValue Radius = new FloatValue(2.5f, false);
        public FloatValue Intensity = new FloatValue(1.0f, false);
        public FloatValue Power = new FloatValue(1.0f, false);
        public BoolValue MultiBounce = new BoolValue(true, false);
        public FloatValue Sharpeness = new FloatValue(0.25f, false);
        public FloatValue TemporalScale = new FloatValue(1, false);
        public FloatValue TemporalResponse = new FloatValue(1, false);
        public OutPass Debug = OutPass.Combined;

        public void Load(GroundTruthAmbientOcclusion gtao)
        {
            if (gtao == null)
                return;

            gtao.enabled = Enabled;
            if (DirSampler.overrideState)
                gtao.DirSampler = DirSampler.value;
            else
                gtao.DirSampler = 4;

            if (SliceSampler.overrideState)
                gtao.SliceSampler = SliceSampler.value;
            else
                gtao.SliceSampler = 8;

            if (Radius.overrideState)
                gtao.Radius = Radius.value;
            else
                gtao.Radius = 2.5f;

            if (Intensity.overrideState)
                gtao.Intensity = Intensity.value;
            else
                gtao.Intensity = 1.0f;

            if (Power.overrideState)
                gtao.Power = Power.value;
            else
                gtao.Power = 1.0f;

            if (MultiBounce.overrideState)
                gtao.MultiBounce = MultiBounce.value;
            else
                gtao.MultiBounce = true;

            if (Sharpeness.overrideState)
                gtao.Sharpeness = Sharpeness.value;
            else
                gtao.Sharpeness = 0.25f;

            if (TemporalScale.overrideState)
                gtao.TemporalScale = TemporalScale.value;
            else
                gtao.TemporalScale = 1;

            if (TemporalResponse.overrideState)
                gtao.TemporalResponse = TemporalResponse.value;
            else
                gtao.TemporalResponse = 1;

            if (Debug == 0)
                gtao.Debug = (GroundTruthAmbientOcclusion.OutPass)OutPass.Combined;
            else
                gtao.Debug = (GroundTruthAmbientOcclusion.OutPass)Debug;

        }

        public void Save(GroundTruthAmbientOcclusion gtao)
        {
            if (gtao == null)
                return;

            Enabled = gtao.enabled;
            DirSampler.value = gtao.DirSampler;
            SliceSampler.value = gtao.SliceSampler;
            Radius.value = gtao.Radius;
            Intensity.value = gtao.Intensity;
            Power.value = gtao.Power;
            MultiBounce.value = gtao.MultiBounce;
            Sharpeness.value = gtao.Sharpeness;
            TemporalScale.value = gtao.TemporalScale;
            TemporalResponse.value = gtao.TemporalResponse;
            Debug = (OutPass)gtao.Debug;
        }
    }
}
