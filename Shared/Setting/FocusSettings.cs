using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class FocusSettings
    {
        public bool Enabled = false;
        public FloatValue Speed = new FloatValue(6f, false);

        public void Load(FocusPuller focus)
        {
            if (focus == null)
                return;

            focus.enabled = Enabled;

            if (Speed.overrideState)
                focus.Speed = Speed.value;
            else
                focus.Speed = 6f;
        }

        public void Save(FocusPuller focus)
        {
            if (focus == null)
                return;

            Enabled = focus.enabled;
            Speed.value = focus.Speed;

        }
    }
}
