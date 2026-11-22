using MessagePack;

namespace Graphics.Settings
{
    /// <summary>The volumetric light renderer's settings; which lights scatter, and how, is kept per light.</summary>
    [MessagePackObject(true)]
    public class VolumetricLightSettings
    {
        public bool Enabled = false;
        /// <summary>0 Full, 1 Half, 2 Quarter.</summary>
        public int Resolution = 1;
        public bool BilateralBlur = true;
        public bool Visualize = false;
    }
}
