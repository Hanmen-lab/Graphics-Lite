using Graphics.Settings;
using Graphics.UI;
using KKAPI.Studio;
using System;
using UnityEngine;
using static Graphics.Inspector.Util;
using static Graphics.LightManager;

namespace Graphics.Inspector
{
    internal static class LightInspector
    {
        private static Vector2 dirLightScrollView;
        private static Vector2 pointLightScrollView;
        private static Vector2 spotLightScrollView;
        private static Vector2 inspectorScrollView;
        private static int customLightIndex = 0;
        private static int cachedFontSize = -1;
        private static int cachedWindowWidth = -1;
        private static int paddingL, paddingT, floorsize, labelspace;
        private static Lay.Style Tab, TabContent, LightList, LightButton, LightListBox;
        private static float lightbox, lightbuttonwidth;

        static void UpdateCachedValues(GlobalSettings renderSettings)
        {
            if (cachedFontSize == renderSettings.FontSize && cachedWindowWidth == Inspector.Width)
                return;

            cachedFontSize = renderSettings.FontSize;
            cachedWindowWidth = Inspector.Width;

            paddingL = Mathf.RoundToInt(renderSettings.FontSize * 2f);
            paddingT = Mathf.RoundToInt(renderSettings.FontSize * 1.5f);
            floorsize = Mathf.RoundToInt(renderSettings.FontSize);
            lightbox = Inspector.Height * 0.25f;
            lightbuttonwidth = Mathf.Clamp(LiteWindow.ContentWidth * 0.34f, 170f, 230f);

            Tab = Lay.Style.Card;
            LightList = Lay.Style.Plain;
            LightButton = Lay.Style.Card;
            LightListBox = Lay.Style.Fixed(lightbuttonwidth, false);
            TabContent = Lay.Style.Plain;

            labelspace = Mathf.RoundToInt(renderSettings.FontSize * 1.8f);
        }

        internal static void Draw(GlobalSettings renderingSettings, LightManager lightManager, LightingSettings lightingSettings, bool showAdvanced/*, VolumetricLight volumetricLight*/)
        {

            UpdateCachedValues(renderingSettings);

            lightToDestroy = null;

            Lay.BeginVertical(TabContent);
            {

                Lay.BeginHorizontal();
                {
                    if (KKAPI.KoikatuAPI.GetCurrentGameMode() != KKAPI.GameMode.Studio)
                    {
                        if (Lay.Button("Save Map Lights to Preset"))
                        {
                            Graphics.Instance.PresetManager.SaveMapLights(false);
                        }
                        if (Lay.Button("Load Map Lights Preset"))
                        {
                            Graphics.Instance.PresetManager.LoadMapLights(false);
                        }
                        if (Lay.Button("Reset Map Lights"))
                        {
                            Graphics.Instance.PresetManager.LoadMapLightsDefault(false);
                        }
                    }
                }
                Lay.EndHorizontal();
                Lay.BeginHorizontal();
                {
                    Lay.BeginVertical(LightListBox);
                    {
                        Lay.Space(15);
                        Label("  DIRECTIONAL", "", true);
                        Lay.Space(15);
                        if (0 < lightManager.DirectionalLights.Count)
                        {
                            Lay.BeginVertical(LightButton);
                            {
                                Lay.BeginVertical(LightList);
                                {
                                    dirLightScrollView = Lay.BeginScrollView(dirLightScrollView, Lay.MaxHeight(lightbox));
                                    lightManager.DirectionalLights.ForEach(l => LightOverviewModule(lightManager, l));
                                    //Lay.Space(200);
                                    Lay.EndScrollView();
                                }
                                Lay.EndVertical();
                                if (3 < lightManager.DirectionalLights.Count)
                                {
                                    Lay.BeginHorizontal();
                                    if (Lay.Button("All ON"))
                                    {
                                        switch (LightSettings.LightType.Directional)
                                        {
                                            case LightSettings.LightType.Directional:
                                                lightManager.DirectionalLights.ForEach(l => l.Enabled = true);
                                                break;

                                        }
                                    }
                                    if (Lay.Button("All OFF"))
                                    {
                                        switch (LightSettings.LightType.Directional)
                                        {
                                            case LightSettings.LightType.Directional:
                                                lightManager.DirectionalLights.ForEach(l => l.Enabled = false);
                                                break;

                                        }
                                    }
                                    Lay.EndHorizontal();
                                }

                                if (Graphics.Instance.IsStudio())
                                {
                                    if (Lay.Button(" + "))
                                    {
                                        Singleton<Studio.Studio>.Instance.AddLight((int)LightSettings.LightType.Directional);
                                        lightManager.Light();
                                    }
                                }
                                //add custom directional lights in maker
                                else/* if (LightSettings.LightType.Directional == LightSettings.LightType.Directional)*/
                                {
                                    if (Lay.Button(" + "))
                                    {
                                        customLightIndex += 1;
                                        GameObject lightGameObject = new GameObject("(Graphics) Directional Light " + customLightIndex);
                                        Light lightComp = lightGameObject.AddComponent<Light>();
                                        lightGameObject.GetComponent<Light>().type = LightType.Directional;
                                        lightManager.Light();
                                    }
                                }
                            }
                            Lay.EndVertical();
                        }
                        Lay.Space(15);
                        Label("  POINT LIGHTS", "", true);
                        Lay.Space(15);
                        Lay.BeginVertical(LightButton);
                        {
                            Lay.BeginVertical(LightList);
                            {
                                pointLightScrollView = Lay.BeginScrollView(pointLightScrollView, Lay.MaxHeight(lightbox));
                                lightManager.PointLights.ForEach(l => LightOverviewModule(lightManager, l));
                                //Lay.Space(200);
                                Lay.EndScrollView();
                            }
                            Lay.EndVertical();

                            if (1 < lightManager.PointLights.Count)
                            {
                                Lay.BeginHorizontal();
                                if (Lay.Button("All ON"))
                                {
                                    switch (LightSettings.LightType.Point)
                                    {
                                        case LightSettings.LightType.Point:
                                            lightManager.PointLights.ForEach(l => l.Enabled = true);
                                            break;

                                    }
                                }
                                if (Lay.Button("All OFF"))
                                {
                                    switch (LightSettings.LightType.Point)
                                    {
                                        case LightSettings.LightType.Point:
                                            lightManager.PointLights.ForEach(l => l.Enabled = false);
                                            break;

                                    }
                                }
                                Lay.EndHorizontal();
                            }

                            if (Graphics.Instance.IsStudio())
                            {
                                if (Lay.Button(" + "))
                                {
                                    Singleton<Studio.Studio>.Instance.AddLight((int)LightSettings.LightType.Point);
                                    lightManager.Light();
                                }
                            }
                        }
                        Lay.EndVertical();
                        Lay.Space(15);
                        Label("  SPOT LIGHTS", "", true);
                        Lay.Space(15);
                        Lay.BeginVertical(LightButton);
                        {
                            Lay.BeginVertical(LightList);
                            {
                                spotLightScrollView = Lay.BeginScrollView(spotLightScrollView, Lay.MaxHeight(lightbox));
                                lightManager.SpotLights.ForEach(l => LightOverviewModule(lightManager, l));
                                //Lay.Space(200);
                                Lay.EndScrollView();
                            }
                            Lay.EndVertical();

                            if (1 < lightManager.SpotLights.Count)
                            {
                                Lay.BeginHorizontal();
                                if (Lay.Button("All ON"))
                                {
                                    switch (LightSettings.LightType.Spot)
                                    {
                                        case LightSettings.LightType.Spot:
                                            lightManager.SpotLights.ForEach(l => l.Enabled = true);
                                            break;

                                    }
                                }
                                if (Lay.Button("All OFF"))
                                {
                                    switch (LightSettings.LightType.Spot)
                                    {
                                        case LightSettings.LightType.Spot:
                                            lightManager.SpotLights.ForEach(l => l.Enabled = false);
                                            break;

                                    }
                                }
                                Lay.EndHorizontal();
                            }

                            if (Graphics.Instance.IsStudio())
                            {
                                if (Lay.Button(" + "))
                                {
                                    Singleton<Studio.Studio>.Instance.AddLight((int)LightSettings.LightType.Spot);
                                    lightManager.Light();
                                }
                            }
                        }
                        Lay.EndVertical();
                        Lay.FlexibleSpace();
                    }
                    Lay.EndVertical();
                    inspectorScrollView = Lay.BeginScrollView(inspectorScrollView);
                    Lay.BeginVertical();
                    {

                        if (null != lightManager.SelectedLight)
                        {
                            DrawLightSettings(lightManager, renderingSettings/*, volumetricLight*/);
                        }
                        else
                        {
                            Lay.BeginVertical(Tab);
                            {
                                Label("Select a light source on the left panel.", "");
                            }
                            Lay.EndVertical();
                        }
                        Lay.FlexibleSpace();
                    }
                    Lay.EndVertical();
                    Lay.EndScrollView();
                }
                Lay.EndHorizontal();
            }
            Lay.EndVertical();

            if (lightToDestroy != null)
            {
                GameObject.DestroyImmediate(lightToDestroy.gameObject);
                lightManager.Light();
            }

        }
        private static bool IsLightNameValid(string lightName, string currentAliasedName)
        {
            return !string.IsNullOrEmpty(lightName) && lightName != currentAliasedName;
        }

        private static void UpdateLightName(string lightName, LightManager lightManager, string currentAliasedName)
        {
            var selectedLight = lightManager.SelectedLight?.Light;
            if (selectedLight == null) return;

            string currentLightName = selectedLight.name;

            if (IsLightNameValid(lightName, currentAliasedName) && lightName != currentLightName)
            {
                selectedLight.name = lightName;
            }
            else if (lightName == currentLightName && PerLightSettings.AliasedLight(selectedLight))
            {
                PerLightSettings.ClearAlias(selectedLight);
            }
        }

        private static void HandleAlias(string lightName, LightManager lightManager, string currentAliasedName)
        {
            if (IsLightNameValid(lightName, currentAliasedName))
            {
                PerLightSettings.SetAlias(lightManager.SelectedLight.Light, lightName);
            }
            else if (lightName == lightManager.SelectedLight.Light.name && PerLightSettings.AliasedLight(lightManager.SelectedLight.Light))
            {
                PerLightSettings.ClearAlias(lightManager.SelectedLight.Light);
            }
        }
        private static void DrawLightSettings(LightManager lightManager, GlobalSettings renderingSettings)
        {
            Lay.Style TabHeader = Lay.Style.Card;
            Lay.Style Tab = Lay.Style.Card;

            if (lightManager.SelectedLight.Enabled)
            {
                //AlloyAreaLight alloyLight = null;
                //if (lightManager.UseAlloyLight)
                //{
                //    alloyLight = lightManager.SelectedLight.Light.GetComponent<AlloyAreaLight>();
                //}

                CustomShadowResolutionHandler customShadowResolutionHandler = lightManager.SelectedLight.Light.GetComponent<CustomShadowResolutionHandler>();


                if (Graphics.Instance.IsStudio())
                {
                    string currentAliasedName = PerLightSettings.NameForLight(lightManager.SelectedLight.Light);
                    //Lay.Space(10);

                    Lay.BeginHorizontal(TabHeader, Lay.ExpandWidth(false));
                    {
                        Text("LIGHT NAME", currentAliasedName, lightName =>
                        {
                            HandleAlias(lightName, lightManager, currentAliasedName);
                            UpdateLightName(lightName, lightManager, currentAliasedName);
                        },
                        !lightManager.SelectedLight.Light.transform.IsChildOf(GameObject.Find("StudioScene/Camera").transform));
                    }
                    Lay.EndHorizontal();
                }
                else
                {
                    Lay.BeginHorizontal(Tab);
                    Label(lightManager.SelectedLight.Light.name, "", true);

                    Lay.EndHorizontal();
                }

                Lay.BeginVertical(Tab);
                //------------------------------PutlogicHere
                switch (lightManager.SelectedLight.Type)
                {
                    case LightType.Point:
                        DrawPointLightSettings(lightManager, /*alloyLight,*/ renderingSettings, customShadowResolutionHandler);
                        break;
                    case LightType.Directional:
                        DrawDirectionalLightSettings(lightManager, renderingSettings, customShadowResolutionHandler);
                        break;
                    case LightType.Spot:
                        DrawSpotLightSettings(lightManager, /*alloyLight,*/ renderingSettings, customShadowResolutionHandler);
                        break;
                    default:
                        Lay.Label("Unknown light type");
                        break;
                }
                Lay.EndVertical();
            }
            else
            {

                Lay.BeginVertical(Tab);
                {
                    Label("Selected light is disabled.", "");
                }
                Lay.EndVertical();
            }
        }

        private static void DrawDirectionalLightSettings(LightManager lightManager, GlobalSettings renderingSettings, CustomShadowResolutionHandler customShadowResolutionHandler)
        {
            DrawBasicSettings(lightManager, renderingSettings);
            //Sun
            DrawSunSettings(lightManager);
            //Position
            DrawPositionSetting(lightManager);
            //Additional Cam
            DrawAdditionalCamLight(lightManager);
            //Shadows
            DrawDirShadowsSettings(lightManager, customShadowResolutionHandler);
            //Layers
            DrawLayersSettings(lightManager);
            //Volumetric
            DrawVolumetricSettings(lightManager);
            //Alloy
            //DrawAlloySettings(lightManager, alloyLight);
        }
        private static void DrawSpotLightSettings(LightManager lightManager, GlobalSettings renderingSettings, CustomShadowResolutionHandler customShadowResolutionHandler)
        {
            DrawBasicSettings(lightManager, renderingSettings);
            //Position
            DrawSpotControls(lightManager);
            //Additional Cam
            DrawAdditionalCamLight(lightManager);
            //Shadows
            DrawLocalShadowsSettings(lightManager, customShadowResolutionHandler);
            //Cookies
            DrawCookieSettings(lightManager);
            //Layers
            DrawLayersSettings(lightManager);
            //Volumetric
            DrawVolumetricSettings(lightManager);
            //Alloy
            //DrawAlloySettings(lightManager, alloyLight);

        }
        private static void DrawPointLightSettings(LightManager lightManager, GlobalSettings renderingSettings, CustomShadowResolutionHandler customShadowResolutionHandler)
        {
            DrawBasicSettings(lightManager, renderingSettings);
            //Position
            DrawPointControls(lightManager);
            //Additional Cam
            //DrawAdditionalCamLight(lightManager);
            //Shadows
            DrawLocalShadowsSettings(lightManager, customShadowResolutionHandler);
            //Layers
            DrawLayersSettings(lightManager);
            //Volumetric
            DrawVolumetricSettings(lightManager);
            //Alloy
            //DrawAlloySettings(lightManager, alloyLight);
        }

        private static void DrawBasicSettings(LightManager lightManager, GlobalSettings renderingSettings)
        {
            //Label("COLOR", "", true);
            //Lay.Space(10);
            SliderColorVertical("Light Color", lightManager.SelectedLight.Color, c => lightManager.SelectedLight.Color = c);
            //Lay.Space(20);
            //Label("INTENSITY", "", true);
            Lay.Space(10);
            SliderCompact("Intensity", lightManager.SelectedLight.Intensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N2", i => lightManager.SelectedLight.Intensity = i);
            //SliderCompact("Indirect Multiplier", lightManager.SelectedLight.Light.bounceIntensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N0", bi => lightManager.SelectedLight.Light.bounceIntensity = bi);
            ToggleAlt("Linear Intensity", renderingSettings.LightsUseLinearIntensity, false, useLinear => renderingSettings.LightsUseLinearIntensity = useLinear);
            Lay.Space(10);
            SliderCompact("Indirect Multiplier", lightManager.SelectedLight.Light.bounceIntensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N0", bi => lightManager.SelectedLight.Light.bounceIntensity = bi);
        }
        private static void DrawSunSettings(LightManager lightManager)
        {
            bool isSun = ReferenceEquals(lightManager.SelectedLight.Light, RenderSettings.sun);
            Lay.Space(labelspace);
            Label("SUN", "", true);
            Lay.Space(10);
            ToggleAlt("Sun Source", isSun, false, suns =>
            {
                if (suns)
                {
                    RenderSettings.sun = lightManager.SelectedLight.Light;
                    ConnectSunToUnderwater.ConnectSun();
                }
                else
                {
                    RenderSettings.sun = null;
                }
            });
        }
        private static void DrawAdditionalCamLight(LightManager lightManager)
        {
            if (!lightManager.SelectedLight.IsNotAdditionalCamAvailable)
            {
                Lay.Space(labelspace);
                Toggle("Additional Cam Light", lightManager.SelectedLight.AdditionalCamLight, false, addCamLight => lightManager.SelectedLight.AdditionalCamLight = addCamLight);
            }
        }
        private static void DrawPositionSetting(LightManager lightManager)
        {
            if (lightManager.SelectedLight.Light.name != "Cam Light" || !lightManager.SelectedLight.Light.transform.IsChildOf(GameObject.Find("StudioScene/Camera").transform))
            {
                if (!lightManager.SelectedLight.AdditionalCamLight)
                {
                    Vector3 rot = lightManager.SelectedLight.Rotation;
                    Lay.Space(labelspace);
                    Label("POSITION", "", true);
                    Lay.Space(10);
                    SliderCompact("Vertical Rotation", rot.x, LightSettings.RotationXMin, LightSettings.RotationXMax, "N1", x => { rot.x = x; });
                    SliderCompact("Horizontal Rotation", rot.y, LightSettings.RotationYMin, LightSettings.RotationYMax, "N1", y => { rot.y = y; });

                    if (rot != lightManager.SelectedLight.Rotation)
                    {
                        lightManager.SelectedLight.Rotation = rot;
                    }
                }
                else
                {
                    if (!StudioAPI.InsideStudio)
                    {
                        GraphicsAdditionalCamLight graphicsAdditionalCamLight = lightManager.SelectedLight.Light.gameObject.GetComponent<GraphicsAdditionalCamLight>();
                        Vector3 rot = graphicsAdditionalCamLight.OriginalRotation.eulerAngles;
                        Lay.Space(labelspace);
                        Label("POSITION", "", true);
                        Lay.Space(10);
                        SliderCompact("Vertical Rotation", rot.x, LightSettings.RotationXMin, LightSettings.RotationXMax, "N1", x => { rot.x = x; });
                        SliderCompact("Horizontal Rotation", rot.y, LightSettings.RotationYMin, LightSettings.RotationYMax, "N1", y => { rot.y = y; });

                        if (rot != graphicsAdditionalCamLight.OriginalRotation.eulerAngles)
                        {
                            graphicsAdditionalCamLight.OriginalRotation = Quaternion.Euler(rot);
                        }
                    }
                    else
                    {
                        Vector3 rot = lightManager.SelectedLight.OciLight.guideObject.changeAmount.rot;
                        Lay.Space(labelspace);
                        Label("POSITION", "", true);
                        Lay.Space(10);
                        SliderCompact("Vertical Rotation", rot.x, LightSettings.RotationXMin, LightSettings.RotationXMax, "N1", x => { rot.x = x; });
                        SliderCompact("Horizontal Rotation", rot.y, LightSettings.RotationYMin, LightSettings.RotationYMax, "N1", y => { rot.y = y; });

                        if (rot != lightManager.SelectedLight.OciLight.guideObject.changeAmount.rot)
                        {
                            lightManager.SelectedLight.OciLight.guideObject.changeAmount.rot = rot;
                        }
                    }
                }
            }
        }
        private static void DrawPointControls(LightManager lightManager)
        {
            Lay.Space(labelspace);
            SliderCompact("Light Range", lightManager.SelectedLight.Range, 0.1f, 500f, "N1", range => { lightManager.SelectedLight.Range = range; });
        }
        private static void DrawSpotControls(LightManager lightManager)
        {
            Lay.Space(labelspace);
            SliderCompact("Light Range", lightManager.SelectedLight.Range, 0.1f, 500f, "N1", range => { lightManager.SelectedLight.Range = range; });
            SliderCompact("Spot Angle", lightManager.SelectedLight.SpotAngle, 1f, 179f, "N1", angle => { lightManager.SelectedLight.SpotAngle = angle; });
            if (lightManager.SelectedLight.SpotAngle > 90f)
                WarningCompact("Spot Angle values above 90 degrees will cause shadow quality degradation!");
        }

        //private static void DrawAlloySettings(LightManager lightManager, AlloyAreaLight alloyLight)
        //{
        //    //Im.Enabled = false;

        //    Lay.Space(20);
        //    Label("SPECULARITY", "", true);
        //    Lay.Space(10);
        //    ToggleAlt("Use Alloy Light", lightManager.UseAlloyLight, false, useAlloy => lightManager.UseAlloyLight = useAlloy);
        //    Lay.Space(10);
        //    if (lightManager.UseAlloyLight && null != alloyLight)
        //    {
        //        ToggleAlt("Specular Highlight", alloyLight.HasSpecularHighlight, false, highlight => alloyLight.HasSpecularHighlight = highlight);
        //        if (alloyLight.HasSpecularHighlight)
        //        {
        //            Lay.Space(10);
        //            SliderCompact("Specular Size", alloyLight.Radius, 0f, 1f, "N2", i => alloyLight.Radius = i);

        //            if (lightManager.SelectedLight.Type == LightType.Point)
        //            {
        //                Lay.Space(10);
        //                SliderCompact("Length", alloyLight.Length, 0f, 1f, "N2", i => alloyLight.Length = i);
        //            }
        //        }
        //    }
        //    //Im.Enabled = true;
        //}

        private static void DrawDirShadowsSettings(LightManager lightManager, CustomShadowResolutionHandler customShadowResolutionHandler)
        {

                Lay.Space(labelspace);
                Label("SHADOWS", "", true);
                Lay.Space(10);

            SelectionVertical("Shadow Type", lightManager.SelectedLight.Shadows, type => lightManager.SelectedLight.Shadows = type);
            if (lightManager.SelectedLight.Shadows == LightShadows.Soft)
            {
                SliderCompact("Strength", lightManager.SelectedLight.Light.shadowStrength, 0f, 1f, "N2", strength => lightManager.SelectedLight.Light.shadowStrength = strength);
                SliderCompact("Bias", lightManager.SelectedLight.Light.shadowBias, 0f, 2f, "N2", bias => lightManager.SelectedLight.Light.shadowBias = bias);
                SliderCompact("Normal Bias", lightManager.SelectedLight.Light.shadowNormalBias, 0f, 3f, "N2", nbias => lightManager.SelectedLight.Light.shadowNormalBias = nbias);
                SliderCompact("Near Plane", lightManager.SelectedLight.Light.shadowNearPlane, 0f, 10f, "N2", np => lightManager.SelectedLight.Light.shadowNearPlane = np);
                if (customShadowResolutionHandler != null)
                {
                    SelectionVertical("Custom Resolution", customShadowResolutionHandler.shadowResolutionCustomSelector, resolution =>
                    { customShadowResolutionHandler.shadowResolutionCustomSelector = resolution; customShadowResolutionHandler.ApplyShadowResolution(lightManager.SelectedLight.Light.name); }, 3);
                    //SliderCompact("Custom Resolution", customShadowResolutionHandler.ShadowResolutionCustomSetting, 256, 4096, resolution => { customShadowResolutionHandler.ShadowResolutionCustomSetting = resolution; customShadowResolutionHandler.ApplyShadowResolution(); });
                }

            }
        }
        private static void DrawLocalShadowsSettings(LightManager lightManager, CustomShadowResolutionHandler customShadowResolutionHandler)
        {
                Lay.Space(labelspace);
                Label("SHADOWS", "", true);
                Lay.Space(10);

            SelectionVertical("Shadow Type", lightManager.SelectedLight.Shadows, type => lightManager.SelectedLight.Shadows = type);
            if (lightManager.SelectedLight.Shadows == LightShadows.Soft)
            {

                SliderCompact("Strength", lightManager.SelectedLight.Light.shadowStrength, 0f, 1f, "N2", strength => lightManager.SelectedLight.Light.shadowStrength = strength);

                SliderCompact("Bias", lightManager.SelectedLight.Light.shadowBias, 0f, 2f, "N2", bias => lightManager.SelectedLight.Light.shadowBias = bias);
                SliderCompact("Normal Bias", lightManager.SelectedLight.Light.shadowNormalBias, 0f, 3f, "N2", nbias => lightManager.SelectedLight.Light.shadowNormalBias = nbias);
                SliderCompact("Near Plane", lightManager.SelectedLight.Light.shadowNearPlane, 0f, 10f, "N2", np => lightManager.SelectedLight.Light.shadowNearPlane = np);
                if (customShadowResolutionHandler != null)
                {
                    SelectionVertical("Custom Resolution", customShadowResolutionHandler.shadowResolutionCustomSelector, resolution =>
                    { customShadowResolutionHandler.shadowResolutionCustomSelector = resolution; customShadowResolutionHandler.ApplyShadowResolution(lightManager.SelectedLight.Light.name); }, 3);
                    //SliderCompact("Custom Resolution", customShadowResolutionHandler.ShadowResolutionCustomSetting, 256, 4096, resolution => { customShadowResolutionHandler.ShadowResolutionCustomSetting = resolution; customShadowResolutionHandler.ApplyShadowResolution(); });
                }
            }
        }

        /// <summary>This light's light in the air: the volumetric lights renderer draws the ones that scatter.</summary>
        private static void DrawVolumetricSettings(LightManager lightManager)
        {
            Light light = lightManager.SelectedLight.Light;
            VolumetricLights.VolumetricLight v = VolumetricLightManager.For(light, false);
            bool on = v != null && v.enabled;

            Lay.Space(labelspace);
            Label("VOLUMETRIC", "", true);
            ToggleAlt("Volumetric Light", on, false, enabled =>
            {
                VolumetricLights.VolumetricLight vl = VolumetricLightManager.For(light, enabled);
                if (vl != null)
                    vl.enabled = enabled;
                VolumetricLightManager.Rearm();
            });
            if (!on)
                return;
            if (VolumetricLightManager.settings == null || !VolumetricLightManager.settings.Enabled)
                WarningCompact("Volumetric Lights is off: switch it on in Post Processing > Volumetric Lights.");

            SliderCompact("Samples", v.SampleCount, 1, 32, x => v.SampleCount = x);
            SliderCompact("Scattering", v.ScatteringCoef, 0f, 3f, "N2", x => v.ScatteringCoef = x);
            SliderCompact("Extinction", v.ExtinctionCoef, 0f, 0.1f, "N3", x => v.ExtinctionCoef = x);
            SliderCompact("Anisotropy", v.MieG, 0f, 0.999f, "N3", x => v.MieG = x);
            ToggleAlt("Height Fog", v.HeightFog, false, x => v.HeightFog = x);
            if (v.HeightFog)
            {
                SliderCompact("Height Scale", v.HeightScale, 0f, 0.5f, "N2", x => v.HeightScale = x);
                SliderCompact("Ground Level", v.GroundLevel, -100f, 100f, "N1", x => v.GroundLevel = x);
            }
            ToggleAlt("Noise", v.Noise, false, x => v.Noise = x);
            if (v.Noise)
            {
                SliderCompact("Noise Scale", v.NoiseScale, 0f, 0.2f, "N3", x => v.NoiseScale = x);
                SliderCompact("Noise Intensity", v.NoiseIntensity, 0f, 1f, "N2", x => v.NoiseIntensity = x);
                SliderCompact("Noise Offset", v.NoiseIntensityOffset, 0f, 1f, "N2", x => v.NoiseIntensityOffset = x);
                Dimension("Noise Velocity", v.NoiseVelocity, x => v.NoiseVelocity = x);
            }
            SliderCompact("Sky Extinction", v.SkyboxExtinctionCoef, 0f, 1f, "N2", x => v.SkyboxExtinctionCoef = x);
            SliderCompact("Max Ray Length", v.MaxRayLength, 0f, 1000f, "N0", x => v.MaxRayLength = x);
        }

        private static void DrawLayersSettings(LightManager lightManager)
        {
            Lay.Space(labelspace);
            Label("LAYERS", "", true);
            Lay.Space(10);
            SelectionMaskVertical("Culling Mask", lightManager.SelectedLight.Light.cullingMask, mask => lightManager.SelectedLight.Light.cullingMask = mask, 2);
            Lay.Space(10);
            SelectionVertical("Render Mode", lightManager.SelectedLight.Light.renderMode, mode => lightManager.SelectedLight.Light.renderMode = mode, 3);
        }

        private static void DrawCookieSettings(LightManager lightManager)
        {
            Lay.Space(labelspace);
            Label("COOKIE", "", true);
            Lay.Space(10);

            CookieTextureManager cookieTextureManager = lightManager.SelectedLight.Light.GetComponent<CookieTextureManager>();
            if (cookieTextureManager == null)
            {
                Label("Cookie texture manager is unavailable for this light.", "", false);
                return;
            }

            ToggleAlt("Enable Cookie Texture", cookieTextureManager.enabled, false, isCookieEnabled => cookieTextureManager.enabled = isCookieEnabled);

            Lay.Space(5);
            if (cookieTextureManager.enabled)
            {
                if (!CookieTextureManager.IsManagerReady())
                {
                    Label("Loading cookie textures...", "", false);
                    return;
                }

                Label("Cookie Texture: " + cookieTextureManager.cookiename, "", false);
                Lay.Space(5);
                DrawTextureGrid(
                    cookieTextureManager.SPOTCookieNames,
                    cookieTextureManager._spotCookieManager.Textures.ToArray(),
                    cookieTextureManager.CurrentSpotCookieName,
                    name =>
                    {
                        if (name != cookieTextureManager.CurrentSpotCookieName)
                        {
                            cookieTextureManager.ApplyCookieTexture(name);
                        }
                    },
                    previewSize: 64f);
            }
        }

        private static GameObject lightToDestroy = null;
        private static void LightOverviewModule(LightManager lightManager, LightObject l)
        {

            if (null == l || null == l.Light)
            {
                lightManager.Light();
                return;
            }
            Lay.BeginHorizontal();

            if (LightButton(PerLightSettings.NameForLight(l.Light), ReferenceEquals(l, lightManager.SelectedLight), true))
            {
                lightManager.SelectedLight = l;
            }
            l.Enabled = Lay.Toggle(l.Enabled, l.Enabled ? "ON" : "OFF", "Button", Lay.Width(38));
            if (!KKAPI.Studio.StudioAPI.InsideStudio && l.Light.name.StartsWith("(Graphics)"))
            {
                if (Lay.Button("-", Lay.Width(24)))
                {
                    lightToDestroy = l.Light.gameObject;
                }
            }

            Lay.EndHorizontal();
        }
        public static void OnLightNameChanged(string newLightName, LightManager lightManager)
        {
            if (lightManager.SelectedLight != null && lightManager.SelectedLight.Light != null)
            {
                string currentLightName = lightManager.SelectedLight.Light.name;

                if (newLightName != currentLightName && !string.IsNullOrEmpty(newLightName))
                {
                    lightManager.SelectedLight.Light.name = newLightName;
                }
                else if (newLightName == currentLightName && PerLightSettings.AliasedLight(lightManager.SelectedLight.Light))
                {
                    PerLightSettings.ClearAlias(lightManager.SelectedLight.Light);
                }
            }
        }

        private static void DrawTexturePreview(Texture texture, float size = 64f)
        {
            Lay.Image(texture, size);
        }
    }
}
