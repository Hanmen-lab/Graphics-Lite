// Copyright (c) 2016-2018 Jakub Boksansky - All Rights Reserved
// Volumetric Ambient Occlusion Unity Plugin 2.0

using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Graphics.Settings.CameraSettings;


namespace Graphics.VAO
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [HelpURL("https://projectwilberforce.github.io/vaomanual/")]
    public class VAOEffect : VAOEffectCommandBuffer
    {
        [ImageEffectOpaque]
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            this.PerformOnRenderImage(source, destination);
        }

    }

}
