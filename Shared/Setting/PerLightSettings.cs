using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using static Graphics.LightManager;

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
        public Color Filter { get; set; }
        public int ShadowType { get; set; }
        public float ShadowStrength { get; set; }
        public int ShadowResolutionType { get; set; }
        public int ShadowResolutionCustom { get; set; }
        public float ShadowBias { get; set; }
        public float ShadowNormalBias { get; set; }
        public float ShadowNearPlane { get; set; }
        public float LightIntensity { get; set; }
        public float IndirectMultiplier { get; set; }
        //public bool SegiSun { get; set; }
        public bool SunSource { get; set; }
        public Vector3 Rotation { get; set; }
        public float Range { get; set; }
        public float SpotAngle { get; set; }
        public float Specular { get; set; }
        public float Length { get; set; }
        public int RenderMode { get; set; }
        public int CullingMask { get; set; }
        public PathElement HierarchyPath { get; set; }
        public int LightId { get; set; }

        public int Type { get; set; }
        public bool AdditionalCamLight { get; set; }

        internal void ApplySettings(LightObject lightObject)
        {            
            lightObject.Enabled = !Disabled;            

            Graphics.Instance.LightManager.UseAlloyLight = UseAlloyLight;

            if (!string.IsNullOrEmpty(LightName))
            {
                SetAlias(lightObject.Light, LightName);
            }
            
            lightObject.Color = Color;
            lightObject.Light.colorTemperature = ColorTemperature;
            //lightObject.Filter = Filter;

            lightObject.Shadows = (LightShadows)ShadowType;
            lightObject.Light.shadowStrength = ShadowStrength;
            if (LightType.Directional == lightObject.Type && Graphics.Instance.Settings.UsePCSS)
                lightObject.ShadowCustomResolution = ShadowResolutionCustom;
            else
                lightObject.Light.shadowResolution = (LightShadowResolution)ShadowResolutionType;

            
            lightObject.Light.shadowBias = ShadowBias;
            lightObject.Light.shadowNormalBias = ShadowNormalBias;
            lightObject.Light.shadowNearPlane = ShadowNearPlane;
            
            lightObject.Intensity = LightIntensity;
            lightObject.Light.bounceIntensity = IndirectMultiplier;
            
            //if (SegiSun)
            //{
            //    if (null != Graphics.Instance.CameraSettings.MainCamera)
            //    {
            //        SEGI.SEGI segi = Graphics.Instance.CameraSettings.MainCamera.GetComponent<SEGI.SEGI>();
            //        if (null != segi && segi.enabled)
            //        {
            //            segi.sun = lightObject.Light;
            //        }
            //    }
            //}

            if (SunSource)
            {
                if (null != Graphics.Instance.CameraSettings.MainCamera)
                {
                    RenderSettings.sun = lightObject.Light;
                }
            }

            // Exclude Cam Light from rotation setting
            if (KKAPI.Studio.StudioAPI.InsideStudio && lightObject.Light.name != "Cam Light" && !lightObject.Light.transform.IsChildOf(GameObject.Find("StudioScene/Camera").transform))
                lightObject.Rotation = Rotation;
            else if (!KKAPI.Studio.StudioAPI.InsideStudio && lightObject.Light.name.StartsWith("(Graphics)"))
                lightObject.Rotation = Rotation;

            lightObject.Range = Range;
            lightObject.SpotAngle = SpotAngle;
            if (Graphics.Instance.LightManager.UseAlloyLight)
            {
                AlloyAreaLight alloyLight = lightObject.Light.GetComponent<AlloyAreaLight>();
                if (alloyLight != null)
                {
                    alloyLight.Radius = Specular;
                    alloyLight.Length = Length;
                }
            }
            lightObject.Light.renderMode = (LightRenderMode)RenderMode;
            lightObject.Light.cullingMask = CullingMask;
            lightObject.AdditionalCamLight = AdditionalCamLight;
        }

        internal void FillSettings(LightObject lightObject)
        {
            Disabled = !lightObject.Enabled;

            Type = (int)lightObject.Type;

            UseAlloyLight = Graphics.Instance.LightManager.UseAlloyLight;

            if (AliasedLight(lightObject.Light))
            {
                LightName = NameForLight(lightObject.Light);
                Graphics.Instance.Log.LogInfo($"Storing Light Alias {LightName}");
            }
            else if (GraphicsAddedLight(lightObject.Light))
            {
                LightName = lightObject.Light.gameObject.name;
            }

            Color = lightObject.Color;
            ColorTemperature = lightObject.Light.colorTemperature;
            //Filter = lightObject.Filter;
            ShadowType = (int)lightObject.Shadows;
            ShadowStrength = lightObject.Light.shadowStrength;
            ShadowResolutionType = (int)lightObject.Light.shadowResolution;
            ShadowResolutionCustom = lightObject.ShadowCustomResolution;
            ShadowBias = lightObject.Light.shadowBias;
            ShadowNormalBias = lightObject.Light.shadowNormalBias;
            ShadowNearPlane = lightObject.Light.shadowNearPlane;

            LightIntensity = lightObject.Intensity;
            IndirectMultiplier = lightObject.Light.bounceIntensity;
            if (null != Graphics.Instance.CameraSettings.MainCamera)
            {
                //SEGI.SEGI segi = Graphics.Instance.CameraSettings.MainCamera.GetComponent<SEGI.SEGI>();
                //if (null != segi && segi.enabled)
                //{
                //    SegiSun = ReferenceEquals(lightObject.Light, segi.sun);
                //}
                //else
                //    SegiSun = false;

                SunSource = ReferenceEquals(lightObject.Light, RenderSettings.sun);
            }
            else
            {
                //SegiSun = false;
                SunSource = false;
            }
                
                
            Rotation = lightObject.Rotation;

            Range = lightObject.Range;
            SpotAngle = lightObject.SpotAngle;
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

            RenderMode = (int)lightObject.Light.renderMode;
            CullingMask = lightObject.Light.cullingMask;

            HierarchyPath = PathElement.Build(lightObject.Light.gameObject.transform);

            if (lightObject.OciLight != null)
                LightId = lightObject.OciLight.lightInfo.dicKey;

            AdditionalCamLight = lightObject.AdditionalCamLight;
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