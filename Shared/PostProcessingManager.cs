﻿using Graphics.Textures;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{

    internal class PostProcessingManager : MonoBehaviour
    {
        private Graphics _parent;
        private ExternalTextureManager _lensDirtManager;
        private InternalTextureManager _lutManager;
        private EmbeddedTextureManager _exlutManager;
        private readonly string _lutAssetPath = "studio/lut/00.unity3d";
        private readonly List<string> _lutTexturePaths = new List<string>()
        {
                "Lut_Default", "Lut_TimeDay", "Lut_TimeSundown", "Lut_TimeNight", "Lut_Warm", "Lut_Cold",
                "Lut_Dull", "Lut_Pale", "Lut_Soft", "Lut_Strong", "Lut_Deep", "Lut_Bright", "Lut_Sepia", "Lut_Monochrome",
                "Lut_MonoRed", "Lut_MonoBlue", "Lut_MonoGreen", "Lut_LimitRed", "Lut_LimitBlue", "Lut_LimitGreen",
                "Lut_InvertMono", "Lut_Invert", "Lut_Sarmo", "Lut_Posterize",
                "Lut_OldPoster1", "Lut_OldPoster2", "Lut_RobotSalvation", "Lut_Greyish", "Lut_DarkGreyish",
                "Lut_Art", "Lut_Comic", "Lut_Aliens", "Lut_Fog", "Lut_Desert", "Lut_Vibrance_D"
        };

        private readonly List<string> _3dlutTexturePaths = new List<string>()
        {
            "AgX_Base Contrast",
            "AgX_Punchy",
            "AgX_Medium High Contrast",
            "AgX_Medium Low Contrast",
            "AgX_High Contrast",
            "AgX_Low Contrast",
            "AgX_Very High Contrast",
            "AgX_Very Low Contrast",
            "AgX_Greyscale"
        };

        internal string LensDirtTexturesPath
        {
            get => _lensDirtManager.AssetPath;
            set => _lensDirtManager.AssetPath = value;
        }

        internal Texture CurrentLensDirtTexture => _lensDirtManager.CurrentTexture;

        internal string CurrentLensDirtTexturePath => _lensDirtManager.CurrentTexturePath;

        internal int CurrentLensDirtTextureIndex => _lensDirtManager.CurrentTextureIndex;

        internal Texture[] LensDirtPreviews => _lensDirtManager.PreviewArray;

        internal void LoadLensDirtTexture(int index, Action<Texture> onChanged = null)
        {
            _lensDirtManager.LoadTexture(index, onChanged);
        }
        internal void LoadLensDirtTexture(string path, Action<Texture> onChanged = null)
        {
            StartCoroutine(_lensDirtManager.LoadTexture(path, onChanged));
        }

        internal Texture CurrentLUTTexture => _lutManager.CurrentTexture;

        internal string CurrentLUTName => _lutManager.CurrentTextureName.IsNullOrEmpty() ? LUTNames[0] : _lutManager.CurrentTextureName;

        internal int CurrentLUTIndex => _lutManager.CurrentTextureIndex >= 0 ? _lutManager.CurrentTextureIndex : 0;

        internal string[] LUTNames => _lutManager.TextureNames;

        internal Texture LoadLUT(int index)
        {
            return _lutManager.GetTexture(index);
        }

        internal Texture LoadLUT(string name)
        {
            return _lutManager.GetTexture(name);
        }
        
        //3dLuts
        internal Texture Current3DLUTTexture => _exlutManager.CurrentTexture;

        internal string Current3DLUTName => _exlutManager.CurrentTextureName.IsNullOrEmpty() ? LUT3DNames[0] : _exlutManager.CurrentTextureName;

        internal int Current3DLUTIndex => _exlutManager.CurrentTextureIndex >= 0 ? _exlutManager.CurrentTextureIndex : 0;

        internal string[] LUT3DNames => _exlutManager.TextureNames;

        internal Texture Load3DLUT(int index)
        {
            return _exlutManager.GetTexture(index);
        }

        internal Texture Load3DLUT(string name)
        {
            return _exlutManager.GetTexture(name);
        }
        internal bool LUTReady()
        {
            return _exlutManager.TextureNames != null && _lutManager.TextureNames != null;
        }

        internal Graphics Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                _lensDirtManager = _parent.gameObject.AddComponent<ExternalTextureManager>();
                _lensDirtManager.SearchPattern = "*.png";
                _lutManager = _parent.gameObject.AddComponent<InternalTextureManager>();
                _lutManager.TexturePaths = _lutTexturePaths;
                _lutManager.SearchPattern = "Lut_";
                _lutManager.AssetPath = _lutAssetPath;
                _exlutManager = _parent.gameObject.AddComponent<EmbeddedTextureManager>();
                _exlutManager.TexturePaths = _3dlutTexturePaths;
                _exlutManager.SearchPattern = "AgX_";
                _exlutManager.ResourcePath = "agx.unity3d";
            }
        }
    }
}