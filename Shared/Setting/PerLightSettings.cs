using Graphics.Textures;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using static Graphics.LightManager;
using static Graphics.DebugUtils;

namespace Graphics
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class PerLightSettings
    {
        public bool Disabled { get; set; } = true;
        public bool UseAlloyLight { get; set; }
        public string LightName { get; set; }
        public Color Color { get; set; }
        public float ColorTemperature { get; set; }
        public int ShadowType { get; set; }
        public float ShadowStrength { get; set; }
        public int ShadowResolutionType { get; set; }
        public int ShadowResolutionCustom { get; set; }
        public float ShadowBias { get; set; } = 0.05f; // Default value
        public float ShadowNormalBias { get; set; }
        public float ShadowNearPlane { get; set; }
        public float LightIntensity { get; set; }
        public float IndirectMultiplier { get; set; }
        public bool SunSource { get; set; }
        public Vector3 Rotation { get; set; }
        public float Range { get; set; }
        public float SpotAngle { get; set; }
        public int RenderMode { get; set; }
        public int CullingMask { get; set; }
        public PathElement HierarchyPath { get; set; }
        public int LightId { get; set; }
        public int Type { get; set; }
        public bool AdditionalCamLight { get; set; }

        //Volumetric Lights
        public bool Enabled { get; set; }
        public bool UseVolumetricLight { get; set; }
        public int SampleCount { get; set; }
        public float ScatteringCoef { get; set; }
        public float ExtinctionCoef { get; set; }
        public float SkyboxExtinctionCoef { get; set; }
        public float MieG { get; set; }
        public bool HeightFog { get; set; }
        public float HeightScale { get; set; }
        public float GroundLevel { get; set; }
        public bool Noise { get; set; }
        public float NoiseScale { get; set; }
        public float NoiseIntensity { get; set; }
        public float NoiseIntensityOffset { get; set; }
        public Vector2 NoiseVelocity { get; set; }
        public float MaxRayLength { get; set; }
        public string CookieTextureName { get; set; }
        public bool IsCookieEnabled { get; set; }

        public float Specular { get; set; }
        public float Length { get; set; }

        //CustomShadowResolution
        public enum CustomShadowResolution
        {
            Default = -1,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192,
            _16K = 16384
        }

        public CustomShadowResolution ShadowResolutionCustomSelector { get; set; } = CustomShadowResolution.Default;

        internal void ApplySettings(LightObject lightObject)
        {
            //Base Settings
            Graphics.Instance.Log.LogInfo("===============================================================");

            lightObject.Enabled = !Disabled;
            lightObject.Color = Color;
            lightObject.Light.colorTemperature = ColorTemperature;
            lightObject.Shadows = (LightShadows)ShadowType;
            lightObject.Light.shadowStrength = ShadowStrength;
            lightObject.Light.shadowResolution = (LightShadowResolution)ShadowResolutionType;
            lightObject.Light.shadowCustomResolution = ShadowResolutionCustom;
            lightObject.Light.shadowBias = ShadowBias;
            lightObject.Light.shadowNormalBias = ShadowNormalBias;
            lightObject.Light.shadowNearPlane = ShadowNearPlane;
            lightObject.Intensity = LightIntensity;
            lightObject.Light.bounceIntensity = IndirectMultiplier;
            lightObject.Range = Range;
            lightObject.SpotAngle = SpotAngle;
            lightObject.Light.renderMode = (LightRenderMode)RenderMode;
            lightObject.Light.cullingMask = CullingMask;
            lightObject.AdditionalCamLight = AdditionalCamLight;

            // Exclude Cam Light from rotation setting
            if (KKAPI.Studio.StudioAPI.InsideStudio && lightObject.Light.name != "Cam Light" && !lightObject.Light.transform.IsChildOf(GameObject.Find("StudioScene/Camera").transform))
                lightObject.Rotation = Rotation;
            else if (!KKAPI.Studio.StudioAPI.InsideStudio && lightObject.Light.name.StartsWith("(Graphics)"))
                lightObject.Rotation = Rotation;

            //Name
            if (!string.IsNullOrEmpty(LightName))
            {
                SetAlias(lightObject.Light, LightName);
                if (lightObject.Light.name != LightName)
                    LogWithDotsLight($"{lightObject.Light.name} set name", LightName);
            }

            //Sun Settings
            if (SunSource)
            {
                if (null != Graphics.Instance.CameraSettings.MainCamera)
                {
                    RenderSettings.sun = lightObject.Light;
                }
            }

            LogWithDotsLight($"{lightObject.Light.name} basic settings", "SET");

            Graphics.Instance.LightManager.UseAlloyLight = UseAlloyLight;

            if (Graphics.Instance.LightManager.UseAlloyLight)
            {
                AlloyAreaLight alloyLight = lightObject.Light.GetComponent<AlloyAreaLight>();
                if (alloyLight != null)
                {
                    alloyLight.Radius = Specular;
                    alloyLight.Length = Length;
                }
            }

            //CustomShadowResolution
            CustomShadowResolutionHandler customShadowRes = lightObject.Light.GetComponent<CustomShadowResolutionHandler>();
            if (customShadowRes != null)
            {
                customShadowRes.shadowResolutionCustomSelector = (CustomShadowResolutionHandler.CustomShadowResolution)ShadowResolutionCustomSelector;
                customShadowRes.ApplyShadowResolution(lightObject.Light.name);
            }

            ////Volumetric Lights
            //VolumetricLight volumetricLight = lightObject.Light.GetComponent<VolumetricLight>();
            //if (volumetricLight != null)
            //{
            //    volumetricLight.enabled = UseVolumetricLight;
            //    if (SampleCount == 0)
            //        SampleCount = 16; // Default value
            //    volumetricLight.SampleCount = SampleCount;
            //    if (ScatteringCoef == 0)
            //        ScatteringCoef = 0.1f; // Default value
            //    volumetricLight.ScatteringCoef = ScatteringCoef;
            //    volumetricLight.ExtinctionCoef = ExtinctionCoef;
            //    volumetricLight.SkyboxExtinctionCoef = SkyboxExtinctionCoef;
            //    if (MieG == 0)
            //        MieG = 0.1f; // Default value
            //    volumetricLight.MieG = MieG;
            //    volumetricLight.HeightFog = HeightFog;
            //    if (HeightScale == 0)
            //        HeightScale = 1.0f; // Default value
            //    volumetricLight.HeightScale = HeightScale;
            //    volumetricLight.GroundLevel = GroundLevel;
            //    volumetricLight.Noise = Noise;
            //    if (NoiseScale == 0)
            //        NoiseScale = 0.015f; // Default value
            //    volumetricLight.NoiseScale = NoiseScale;
            //    if (NoiseIntensity == 0)
            //        NoiseIntensity = 1.0f; // Default value
            //    volumetricLight.NoiseIntensity = NoiseIntensity;
            //    if (NoiseIntensityOffset == 0)
            //        NoiseIntensityOffset = 0.3f; // Default value
            //    volumetricLight.NoiseIntensityOffset = NoiseIntensityOffset;
            //    volumetricLight.NoiseVelocity = NoiseVelocity;
            //    if (MaxRayLength == 0)
            //        MaxRayLength = 400f; // Default value
            //    volumetricLight.MaxRayLength = MaxRayLength;

            //    LogWithDotsLight($"{lightObject.Light.name} volumetric settings", "SET");
            //}

            //// Apply cookie texture when manager ready
            //if (!string.IsNullOrEmpty(CookieTextureName))
            //{
            //    if (CookieTextureManager.IsManagerReady())
            //    {
            //        CookieTextureManager cookieTextureManager = lightObject.Light.GetComponent<CookieTextureManager>();
            //        if (cookieTextureManager != null)
            //        {
            //            cookieTextureManager.enabled = IsCookieEnabled;
            //            cookieTextureManager.cookiename = CookieTextureName;
            //            if (cookieTextureManager.enabled)
            //                cookieTextureManager.ApplyCookieTexture(CookieTextureName);
            //        }
            //    }
            //    else
            //    {
            //        Graphics.Instance.Log.LogWarning($"Cookie manager not ready when applying settings to {lightObject.Light.name}");
            //    }
            //}
        }

        internal void FillSettings(LightObject lightObject)
        {
            //Base Settings
            Disabled = !lightObject.Enabled;
            Type = (int)lightObject.Type;
            Color = lightObject.Color;
            ColorTemperature = lightObject.Light.colorTemperature;
            ShadowType = (int)lightObject.Shadows;
            ShadowStrength = lightObject.Light.shadowStrength;
            ShadowResolutionType = (int)lightObject.Light.shadowResolution;
            ShadowResolutionCustom = lightObject.Light.shadowCustomResolution;
            ShadowBias = lightObject.Light.shadowBias;
            ShadowNormalBias = lightObject.Light.shadowNormalBias;
            ShadowNearPlane = lightObject.Light.shadowNearPlane;
            LightIntensity = lightObject.Intensity;
            IndirectMultiplier = lightObject.Light.bounceIntensity;
            Rotation = lightObject.Rotation;
            Range = lightObject.Range;
            SpotAngle = lightObject.SpotAngle;
            RenderMode = (int)lightObject.Light.renderMode;
            CullingMask = lightObject.Light.cullingMask;
            HierarchyPath = PathElement.Build(lightObject.Light.gameObject.transform);
            if (lightObject.OciLight != null)
                LightId = lightObject.OciLight.lightInfo.dicKey;
            AdditionalCamLight = lightObject.AdditionalCamLight;

            //Name
            if (AliasedLight(lightObject.Light))
            {
                LightName = NameForLight(lightObject.Light);
                Graphics.Instance.Log.LogInfo($"Storing Light Alias {LightName}");
            }
            else if (GraphicsAddedLight(lightObject.Light))
            {
                LightName = lightObject.Light.gameObject.name;
            }

            //Sun Settings
            if (null != Graphics.Instance.CameraSettings.MainCamera)
            {
                SunSource = ReferenceEquals(lightObject.Light, RenderSettings.sun);
            }
            else
            {
                SunSource = false;
            }

            //CustomShadowResolution
            CustomShadowResolutionHandler customShadowRes = lightObject.Light.GetComponent<CustomShadowResolutionHandler>();
            if (customShadowRes != null)
            {
                //ShadowResolutionCustomSetting = customShadowRes.ShadowResolutionCustomSetting;
                ShadowResolutionCustomSelector = (CustomShadowResolution)customShadowRes.shadowResolutionCustomSelector;
            }

            UseAlloyLight = Graphics.Instance.LightManager.UseAlloyLight;
            if (Graphics.Instance.LightManager.UseAlloyLight)
            {
                AlloyAreaLight alloyLight = lightObject.Light.GetComponent<AlloyAreaLight>();
                if (alloyLight != null)
                {
                    Specular = alloyLight.Radius;
                    Length = alloyLight.Length;
                }
                else
                {
                    Specular = 1.0f;
                    Length = 1.0f;
                }
            }
            else
            {
                Specular = 1.0f;
                Length = 1.0f;
            }

            ////Volumetric Lights
            //VolumetricLight volumetricLight = lightObject.Light.GetComponent<VolumetricLight>();
            //if (volumetricLight != null)
            //{
            //    UseVolumetricLight = volumetricLight.enabled;
            //    SampleCount = volumetricLight.SampleCount;
            //    ScatteringCoef = volumetricLight.ScatteringCoef;
            //    ExtinctionCoef = volumetricLight.ExtinctionCoef;
            //    SkyboxExtinctionCoef = volumetricLight.SkyboxExtinctionCoef;
            //    MieG = volumetricLight.MieG;
            //    HeightFog = volumetricLight.HeightFog;
            //    HeightScale = volumetricLight.HeightScale;
            //    GroundLevel = volumetricLight.GroundLevel;
            //    Noise = volumetricLight.Noise;
            //    NoiseScale = volumetricLight.NoiseScale;
            //    NoiseIntensity = volumetricLight.NoiseIntensity;
            //    NoiseIntensityOffset = volumetricLight.NoiseIntensityOffset;
            //    NoiseVelocity = volumetricLight.NoiseVelocity;
            //    MaxRayLength = volumetricLight.MaxRayLength;
            //}

            ////Cookie Settings
            //CookieTextureManager cookieTextureManager = lightObject.Light.GetComponent<CookieTextureManager>();
            //if (cookieTextureManager != null)
            //{
            //    IsCookieEnabled = cookieTextureManager.enabled;
            //    CookieTextureName = cookieTextureManager.cookiename;
            //}
        }

        internal static bool GraphicsAddedLight(Light light)
        {
            return light.gameObject.name.StartsWith("(Graphics)");
        }

        internal static Dictionary<WeakReference<Light>, string> LightNameAliases = new Dictionary<WeakReference<Light>, string>();
        internal static bool AliasedLight(Light light)
        {
            if (!KKAPI.Studio.StudioAPI.InsideStudio)
                return false;

            foreach (WeakReference<Light> lightRef in LightNameAliases.Keys)
            {
                if (lightRef.TryGetTarget(out Light storedRef))
                {
                    if (ReferenceEquals(storedRef, light))
                        return true;
                }
            }

            return false;
        }
        internal static string NameForLight(Light light)
        {
            if (!KKAPI.Studio.StudioAPI.InsideStudio)
                return light.name;

            foreach (WeakReference<Light> lightRef in LightNameAliases.Keys)
            {
                if (lightRef.TryGetTarget(out Light storedRef))
                {
                    if (ReferenceEquals(storedRef, light))
                        return LightNameAliases[lightRef];
                }
            }

            return light.name;
        }

        internal static void ClearAlias(Light light)
        {
            if (!KKAPI.Studio.StudioAPI.InsideStudio)
                return;

            foreach (Studio.TreeNodeObject node in Studio.Studio.Instance.dicInfo.Keys)
            {
                if (Studio.Studio.Instance.dicInfo[node] != null && Studio.Studio.Instance.dicInfo[node] is Studio.OCILight && ReferenceEquals(light, ((Studio.OCILight)Studio.Studio.Instance.dicInfo[node]).light))
                {
                    node.textName = light.name;
                    break;
                }
            }
            foreach (WeakReference<Light> lightRef in LightNameAliases.Keys)
            {
                if (lightRef.TryGetTarget(out Light storedRef))
                {
                    if (ReferenceEquals(storedRef, light))
                    {
                        LightNameAliases.Remove(lightRef);
                        return;
                    }
                }
            }
        }

        internal static void SetAlias(Light light, string name)
        {
            if (!KKAPI.Studio.StudioAPI.InsideStudio)
                return;

            foreach (Studio.TreeNodeObject node in Studio.Studio.Instance.dicInfo.Keys)
            {
                if (Studio.Studio.Instance.dicInfo[node] != null && Studio.Studio.Instance.dicInfo[node] is Studio.OCILight && ReferenceEquals(light, ((Studio.OCILight)Studio.Studio.Instance.dicInfo[node]).light))
                {
                    node.textName = name;
                    break;
                }
            }

            foreach (WeakReference<Light> lightRef in LightNameAliases.Keys)
            {
                if (lightRef.TryGetTarget(out Light storedRef))
                {
                    if (ReferenceEquals(storedRef, light))
                    {
                        LightNameAliases[lightRef] = name;
                        return;
                    }
                }
            }

            LightNameAliases.Add(new WeakReference<Light>(light), name);

        }

        internal static void FlushAliases()
        {
            if (!KKAPI.Studio.StudioAPI.InsideStudio)
                return;

            List<WeakReference<Light>> keysToPurge = new List<WeakReference<Light>>();
            foreach (WeakReference<Light> lightRef in LightNameAliases.Keys)
            {
                if (!lightRef.TryGetTarget(out Light target))
                {
                    keysToPurge.Add(lightRef);
                }
            }
            foreach (WeakReference<Light> key in keysToPurge)
            {
                LightNameAliases.Remove(key);
            }
        }
    }
}