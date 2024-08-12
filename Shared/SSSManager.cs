using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphics
{
    public class SSSManager
    {
        internal static SSS SSSInstance;
        private static readonly List<SSS> otherSSSInstances = new List<SSS>();

        // Initialize Components
        internal void Initialize()
        {
            SSSInstance = Graphics.Instance.CameraSettings.MainCamera.GetOrAddComponent<SSS>();           
        }

        // When user enabled the option
        internal void Start()
        {
            // Apparently after release, it's going to be generated again automatically.
            // https://docs.unity3d.com/ScriptReference/RenderTexture.Release.html
        }

        public static void RegisterAdditionalInstance(SSS otherInstance)
        {
            if (!otherSSSInstances.Contains(otherInstance))
            {
                otherSSSInstances.Add(otherInstance);
                Graphics.Instance.SSSManager.CopySettingsToOtherInstances();
            }
        }

        internal void CopySettingsToOtherInstances()
        {
            foreach (SSS otherInstance in otherSSSInstances)
            {
                otherInstance.Enabled = SSSInstance.Enabled;
                otherInstance.ProfilePerObject = SSSInstance.ProfilePerObject;

                if (otherInstance.MirrorSSS)
                    otherInstance.sssColor = Color.black;
                else
                    otherInstance.sssColor = SSSInstance.sssColor;

                otherInstance.ScatteringRadius = SSSInstance.ScatteringRadius;
                
                if (otherInstance.MirrorSSS)
                    otherInstance.ScatteringIterations = 0;
                else
                    otherInstance.ScatteringIterations = SSSInstance.ScatteringIterations;

                otherInstance.ShaderIterations = SSSInstance.ShaderIterations;
                otherInstance.Downsampling = SSSInstance.Downsampling;
                otherInstance.maxDistance = SSSInstance.maxDistance;
                otherInstance.SSS_Layer = SSSInstance.SSS_Layer;

                if (otherInstance.MirrorSSS)
                    otherInstance.Dither = false;
                else
                    otherInstance.Dither = SSSInstance.Dither;

                otherInstance.DitherIntensity = SSSInstance.DitherIntensity;
                otherInstance.DitherScale = SSSInstance.DitherScale;
                otherInstance.DepthTest = SSSInstance.DepthTest;
                otherInstance.NormalTest = SSSInstance.NormalTest;
                otherInstance.DitherEdgeTest = SSSInstance.DitherEdgeTest;
                otherInstance.FixPixelLeaks = SSSInstance.FixPixelLeaks;
                otherInstance.EdgeOffset = SSSInstance.EdgeOffset;
                    
            }
        }

        // When user disabled the option
        internal void Destroy()
        {
            DestroySSSInstance(SSSInstance);
            for (int i = otherSSSInstances.Count - 1; i >= 0; i--)
            {
                SSS otherInstance = otherSSSInstances[i];
                if (otherInstance == null)
                {
                }
                else
                {
                    otherInstance.Enabled = SSSInstance.Enabled;
                    DestroySSSInstance(otherInstance);

                }
            }
        }

        internal void DestroySSSInstance(SSS SSSInstance)
        {
            // cleanup render texture. 
            if (SSSInstance is null)
            {
                return;
            }

            if (SSSInstance.LightingTex != null)  // I believe textures have an overridden ReferenceEquals that doesn't do what we expect here...
            {
                Clear(ref SSSInstance.LightingTex);
                SSSInstance.LightingTex?.Release();
            }
            if (SSSInstance.LightingTexR != null)
            {
                Clear(ref SSSInstance.LightingTexR);
                SSSInstance.LightingTexR?.Release();
            }
            if (SSSInstance.LightingTexBlurred != null)
            {
                Clear(ref SSSInstance.LightingTexBlurred);
                SSSInstance.LightingTexBlurred?.Release();
            }
            if (SSSInstance.LightingTexBlurredR != null)
            {
                Clear(ref SSSInstance.LightingTexBlurredR);
                SSSInstance.LightingTexBlurredR?.Release();
            }
            if (SSSInstance.SSS_ProfileTex != null)
            {
                Clear(ref SSSInstance.SSS_ProfileTex);
                SSSInstance.SSS_ProfileTex?.Release();
            }
            if (SSSInstance.SSS_ProfileTexR != null)
            {
                Clear(ref SSSInstance.SSS_ProfileTexR);
                SSSInstance.SSS_ProfileTexR?.Release();
            }
        }

        private static void Clear(ref RenderTexture texture)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

        IEnumerator WaitForCamera()
        {
            Camera camera = Graphics.Instance.CameraSettings.MainCamera;
            yield return new WaitUntil(() => camera != null);
            CheckInstance();
        }
        public void CheckInstance()
        {
            if (SSSInstance == null)
            {
                Camera camera = Graphics.Instance.CameraSettings.MainCamera;
                SSSInstance = camera.GetOrAddComponent<SSS>();
            }
        }
    }
}