using MessagePack;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class DitheredShadowsSettings
    {
        public bool Enabled = false;
        public FloatValue point_size = new FloatValue(0.03f, false);
        public FloatValue direction_size = new FloatValue(0.03f, false);
        public FloatValue spot_size = new FloatValue(0.03f, false);

        public void Load(DitheredShadows ditheredshadows)
        {
            if (ditheredshadows == null)
                return;

            ditheredshadows.enabled = Enabled;
            ditheredshadows.point_size = point_size.value;
            ditheredshadows.direction_size = direction_size.value;
            ditheredshadows.spot_size = point_size.value;
        }
        public void Save(DitheredShadows ditheredshadows)
        {
            if (ditheredshadows == null)
                return;

            Enabled = ditheredshadows.enabled;
            point_size.value = ditheredshadows.point_size;
            direction_size.value = ditheredshadows.direction_size;
            spot_size.value = ditheredshadows.spot_size;

        }
    }
}
