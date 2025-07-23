using Graphics;
using MessagePack;
using UnityEngine;
using UnityEngine.Rendering;

namespace Graphics.Settings
{

    [MessagePackObject(true)]
    public struct SkyboxParams
    {
        public float exposure;
        public float rotation;
        public Color tint;
        public string selectedCubeMap;
        //public bool projection;
        //public float horizon;
        //public float scale;

        public SkyboxParams(float exposure, float rotation, Color tint, string selectedCubeMap/*, bool projection, float horizon, float scale*/)
        {
            this.exposure = exposure;
            this.rotation = rotation;
            this.tint = tint;
            this.selectedCubeMap = selectedCubeMap;
            //this.projection = projection;
            //this.horizon = horizon;
            //this.scale = scale;
        }
    };

}