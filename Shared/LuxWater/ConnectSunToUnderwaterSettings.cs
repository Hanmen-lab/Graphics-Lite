using LuxWater;
using MessagePack;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class ConnectSunToUnderwaterSettings
    {
        public bool Enabled = false;


        public void Load(ConnectSunToUnderwater connector)
        {
            if (connector == null)
                return;

            connector.enabled = Enabled;
        }
        public void Save(ConnectSunToUnderwater connector)
        {
            if (connector == null)
                return;

            Enabled = connector.enabled;

        }
    }
}
