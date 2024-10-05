using Graphics.CTAA;
using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class CTAASettings
    {
        public bool Enabled = false;
        public IntValue TemporalStability = new IntValue(6, false);
        public FloatValue HdrResponse = new FloatValue(1.2f, false);
        public FloatValue EdgeResponse = new FloatValue(0.5f, false);
        public FloatValue AdaptiveSharpness = new FloatValue(0.2f, false);
        public FloatValue TemporalJitterScale = new FloatValue(0.475f, false);
        public CTAA_MODE Mode = CTAA_MODE.STANDARD;

        public enum CTAA_MODE
        {
            STANDARD = 0,
            CINA_SOFT = 1,
            CINA_ULTRA = 2
        }

        public void Load(CTAA_PC ctaa)
        {
            ctaa.enabled = Enabled;
            ctaa.CTAA_Enabled = Enabled;

            ctaa.SupersampleMode = 0;

            if (TemporalStability.overrideState)
                ctaa.TemporalStability = TemporalStability.value;
            else
                ctaa.TemporalStability = 6;

            if (HdrResponse.overrideState)
                ctaa.HdrResponse = HdrResponse.value;
            else
                ctaa.HdrResponse = 1.2f;

            if (EdgeResponse.overrideState)
                ctaa.EdgeResponse = EdgeResponse.value;
            else
                ctaa.EdgeResponse = 0.5f;

            if (AdaptiveSharpness.overrideState)
                ctaa.AdaptiveSharpness = AdaptiveSharpness.value;
            else
                ctaa.AdaptiveSharpness = 0.2f;

            if (TemporalJitterScale.overrideState)
                ctaa.TemporalJitterScale = TemporalJitterScale.value;
            else
                ctaa.TemporalJitterScale = 0.475f;
        }
    }
}
