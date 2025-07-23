using LuxWater;
using MessagePack;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class UnderWaterBlurSettings
    {
        //public float blurSpread = 0.6f;
        //public int blurDownSample = 4;
        //public int blurIterations = 4;
        public bool Enabled = false;
        public FloatValue BlurSpread = new FloatValue(0.6f, false);
        public IntValue BlurDownSample = new IntValue(4, false);
        public IntValue BlurIterations = new IntValue(4, false);

        public void Load(LuxWater_UnderWaterBlur underwaterblur)
        {
            if (underwaterblur == null)
                return;

            underwaterblur.enabled = Enabled;
            underwaterblur.blurSpread = BlurSpread.value;
            underwaterblur.blurDownSample = BlurDownSample.value;
            underwaterblur.blurIterations = BlurIterations.value;

        }
        public void Save(LuxWater_UnderWaterBlur underwaterblur)
        {
            if (underwaterblur == null)
                return;

            Enabled = underwaterblur.enabled;
            BlurSpread.value = underwaterblur.blurSpread;
            BlurDownSample.value = underwaterblur.blurDownSample;
            BlurIterations.value = underwaterblur.blurIterations;
        }
    }
}
