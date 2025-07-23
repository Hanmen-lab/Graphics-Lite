using MessagePack;
using PlaceholderSoftware.WetStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics.Settings
{
    [MessagePackObject(true)]
    public class FilmGrainSettings
    {
        public bool enabled = false;
        public BoolValue colored = new BoolValue(true, false);
        public FloatValue intensity = new FloatValue(0f, false);
        public FloatValue size = new FloatValue(1f, false);
        public FloatValue lumContrib = new FloatValue(0.8f, false);

        public void Load(FilmGrain grainLayer)
        {
            if (grainLayer == null)
                return;

            grainLayer.enabled = enabled;

            grainLayer.colored = colored.value;

            if (intensity.overrideState)
                grainLayer.intensity = intensity.value;
            else
                grainLayer.intensity = 0.5f;

            if (size.overrideState)
                grainLayer.size = size.value;
            else
                grainLayer.size = 1f;

            if (lumContrib.overrideState)
                grainLayer.lumContrib = lumContrib.value;
            else
                grainLayer.lumContrib = 0.8f;
        }

        //public void Save(FilmGrain grainLayer)
        //{
        //    if (grainLayer == null)
        //        return;

        //    enabled = grainLayer.enabled;

        //}
    }
}
