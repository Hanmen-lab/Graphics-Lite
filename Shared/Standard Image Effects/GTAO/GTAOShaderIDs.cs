using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graphics.GTAO
{
    public static class GTAOShaderIDs
    {
        public static int _MainTex = Shader.PropertyToID("_MainTex");
        public static int _TempTex = Shader.PropertyToID("_TempTex");
        public static int _DepthTex = Shader.PropertyToID("_DepthTexture");
        public static int _MirrorNormal = Shader.PropertyToID("_MirrorNormal");
        public static int _MirrorPos = Shader.PropertyToID("_MirrorPos");
        public static int _BlurOffset = Shader.PropertyToID("_BlurOffset");
    }
}
