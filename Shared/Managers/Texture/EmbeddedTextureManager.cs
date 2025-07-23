using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphics.Textures
{
    internal class EmbeddedTextureManager : InternalTextureManager
    {
        private string _resourcePath;
        internal string ResourcePath
        {
            get => _resourcePath;
            set
            {
                _resourcePath = value;
                StartCoroutine(LoadAssetsEmbedded());
            }
        }
        internal System.Collections.IEnumerator LoadAssetsEmbedded()
        {
            CurrentTexturePath = "";
            Textures = new List<Texture>();
            var assetBundle = AssetBundle.LoadFromMemory(ResourceUtils.GetEmbeddedResource(_resourcePath));
            foreach (string asset in TexturePaths)
            {
                Texture texture = assetBundle.LoadAsset<Texture>(asset);
                yield return texture;
                if (texture != null)
                {
                    Textures.Add(texture);
                }
                _assetsToLoad--;
            }
            yield return new WaitUntil(() => HasAssetsLoaded);
            assetBundle.Unload(false);
            TextureNames = TexturePaths.Select(path => GetName(path)).ToArray();
        }
    }
}