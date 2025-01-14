using Aura2API;
using MessagePack;


namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class AuraSettings
    {
        public bool Enabled = true;

        public void Load(AuraCamera settings)
        {
            if (settings == null)
                return;

            settings.enabled = Enabled;

            if (!Enabled)
                return;
        }

        public void Save(AuraCamera settings)
        {
            if (settings == null)
                return;

            Enabled = settings.enabled;

        }

    }
}
