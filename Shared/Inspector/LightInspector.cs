using Graphics.Settings;
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
        private static GUIStyle Tab, TabContent, LightList, LightButton, LightListBox;
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
            lightbuttonwidth = Inspector.Width * 0.32f;

            Tab = new GUIStyle(GUIStyles.tabcontent);
            Tab.padding = new RectOffset(paddingL, paddingL, paddingT, paddingT);
            Tab.margin = new RectOffset(0, 0, 0, 5);
            Tab.normal.background = null;

            LightList = new GUIStyle(GUIStyles.tabsmall);
            LightList.margin = new RectOffset(0, 0, 0, floorsize);
            LightList.padding = new RectOffset(floorsize, floorsize, floorsize, 4);

            LightButton = new GUIStyle(GUIStyles.tabsmall);
            LightButton.margin = new RectOffset(0, 0, 0, 0);
            LightButton.padding = new RectOffset(0, 0, 0, 0);

            LightButton.fixedWidth = lightbuttonwidth;
            LightButton.fixedHeight = lightbox;

            LightListBox = new GUIStyle(GUIStyles.tabcontent);
            LightListBox.margin = new RectOffset(0, 0, 0, 0);
            LightListBox.padding = new RectOffset(0, 0, paddingT, 0);
            LightListBox.normal.background = null;
            LightListBox.fixedWidth = lightbuttonwidth;
            //LightListBox.fixedHeight = 400;

            TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(0, 0, 0, 0);
            TabContent.margin = new RectOffset(0, 0, 0, 0);

            labelspace = Mathf.RoundToInt(renderSettings.FontSize * 1.8f);
        }

        internal static void Draw(GlobalSettings renderingSettings, LightManager lightManager, LightingSettings lightingSettings, bool showAdvanced/*, VolumetricLight volumetricLight*/)
        {

            UpdateCachedValues(renderingSettings);

            lightToDestroy = null;

            GUILayout.BeginVertical(TabContent);
            {

                GUILayout.BeginHorizontal();
                {
                    if (KKAPI.KoikatuAPI.GetCurrentGameMode() != KKAPI.GameMode.Studio)
                    {
                        if (GUILayout.Button("Save Map Lights to Preset"))
                        {
                            Graphics.Instance.PresetManager.SaveMapLights(false);
                        }
                        if (GUILayout.Button("Load Map Lights Preset"))
                        {
                            Graphics.Instance.PresetManager.LoadMapLights(false);
                        }
                        if (GUILayout.Button("Reset Map Lights"))
                        {
                            Graphics.Instance.PresetManager.LoadMapLightsDefault(false);
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical(LightListBox);
                    {
                        GUILayout.Space(15);
                        Label("  DIRECTIONAL", "", true);
                        GUILayout.Space(15);
                        if (0 < lightManager.DirectionalLights.Count)
                        {
                            GUILayout.BeginVertical(LightButton);
                            {
                                GUILayout.BeginVertical(LightList);
                                {
                                    dirLightScrollView = GUILayout.BeginScrollView(dirLightScrollView);
                                    lightManager.DirectionalLights.ForEach(l => LightOverviewModule(lightManager, l));
                                    //GUILayout.Space(200);
                                    GUILayout.EndScrollView();
                                }
                                GUILayout.EndVertical();
                                if (3 < lightManager.DirectionalLights.Count)
                                {
                                    GUILayout.BeginHorizontal();
                                    if (GUILayout.Button("All ON"))
                                    {
                                        switch (LightSettings.LightType.Directional)
                                        {
                                            case LightSettings.LightType.Directional:
                                                lightManager.DirectionalLights.ForEach(l => l.Enabled = true);
                                                break;

                                        }
                                    }
                                    if (GUILayout.Button("All OFF"))
                                    {
                                        switch (LightSettings.LightType.Directional)
                                        {
                                            case LightSettings.LightType.Directional:
                                                lightManager.DirectionalLights.ForEach(l => l.Enabled = false);
                                                break;

                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }

                                if (Graphics.Instance.IsStudio())
                                {
                                    if (GUILayout.Button(" + "))
                                    {
                                        Singleton<Studio.Studio>.Instance.AddLight((int)LightSettings.LightType.Directional);
                                        lightManager.Light();
                                    }
                                }
                                //add custom directional lights in maker
                                else/* if (LightSettings.LightType.Directional == LightSettings.LightType.Directional)*/
                                {
                                    if (GUILayout.Button(" + "))
                                    {
                                        customLightIndex += 1;
                                        GameObject lightGameObject = new GameObject("(Graphics) Directional Light " + customLightIndex);
                                        Light lightComp = lightGameObject.AddComponent<Light>();
                                        lightGameObject.GetComponent<Light>().type = LightType.Directional;
                                        lightManager.Light();
                                    }
                                }
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.Space(15);
                        Label("  POINT LIGHTS", "", true);
                        GUILayout.Space(15);
                        GUILayout.BeginVertical(LightButton);
                        {
                            GUILayout.BeginVertical(LightList);
                            {
                                pointLightScrollView = GUILayout.BeginScrollView(pointLightScrollView);
                                lightManager.PointLights.ForEach(l => LightOverviewModule(lightManager, l));
                                //GUILayout.Space(200);
                                GUILayout.EndScrollView();
                            }
                            GUILayout.EndVertical();

                            if (1 < lightManager.PointLights.Count)
                            {
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("All ON"))
                                {
                                    switch (LightSettings.LightType.Point)
                                    {
                                        case LightSettings.LightType.Point:
                                            lightManager.PointLights.ForEach(l => l.Enabled = true);
                                            break;

                                    }
                                }
                                if (GUILayout.Button("All OFF"))
                                {
                                    switch (LightSettings.LightType.Point)
                                    {
                                        case LightSettings.LightType.Point:
                                            lightManager.PointLights.ForEach(l => l.Enabled = false);
                                            break;

                                    }
                                }
                                GUILayout.EndHorizontal();
                            }

                            if (Graphics.Instance.IsStudio())
                            {
                                if (GUILayout.Button(" + "))
                                {
                                    Singleton<Studio.Studio>.Instance.AddLight((int)LightSettings.LightType.Point);
                                    lightManager.Light();
                                }
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(15);
                        Label("  SPOT LIGHTS", "", true);
                        GUILayout.Space(15);
                        GUILayout.BeginVertical(LightButton);
                        {
                            GUILayout.BeginVertical(LightList);
                            {
                                spotLightScrollView = GUILayout.BeginScrollView(spotLightScrollView);
                                lightManager.SpotLights.ForEach(l => LightOverviewModule(lightManager, l));
                                //GUILayout.Space(200);
                                GUILayout.EndScrollView();
                            }
                            GUILayout.EndVertical();

                            if (1 < lightManager.SpotLights.Count)
                            {
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("All ON"))
                                {
                                    switch (LightSettings.LightType.Spot)
                                    {
                                        case LightSettings.LightType.Spot:
                                            lightManager.SpotLights.ForEach(l => l.Enabled = true);
                                            break;

                                    }
                                }
                                if (GUILayout.Button("All OFF"))
                                {
                                    switch (LightSettings.LightType.Spot)
                                    {
                                        case LightSettings.LightType.Spot:
                                            lightManager.SpotLights.ForEach(l => l.Enabled = false);
                                            break;

                                    }
                                }
                                GUILayout.EndHorizontal();
                            }

                            if (Graphics.Instance.IsStudio())
                            {
                                if (GUILayout.Button(" + "))
                                {
                                    Singleton<Studio.Studio>.Instance.AddLight((int)LightSettings.LightType.Spot);
                                    lightManager.Light();
                                }
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndVertical();
                    inspectorScrollView = GUILayout.BeginScrollView(inspectorScrollView);
                    GUILayout.BeginVertical();
                    {

                        if (null != lightManager.SelectedLight)
                        {
                            DrawLightSettings(lightManager, renderingSettings/*, volumetricLight*/);
                        }
                        else
                        {
                            GUILayout.BeginVertical(Tab);
                            {
                                Label("Select a light source on the left panel.", "");
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

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
            GUIStyle TabHeader = new GUIStyle(GUIStyles.tabcontent);
            TabHeader.padding = new RectOffset(Mathf.RoundToInt(renderingSettings.FontSize * 2f), Mathf.RoundToInt(renderingSettings.FontSize * 2f), 0, 0);
            TabHeader.margin = new RectOffset(0, 0, Mathf.RoundToInt(renderingSettings.FontSize * 2f), 0);
            TabHeader.normal.background = null;
            TabHeader.fixedHeight = 61;
            //TabHeader.fixedWidth = 200;


            GUIStyle Tab = new GUIStyle(GUIStyles.tabcontent);
            Tab.padding = new RectOffset(Mathf.RoundToInt(renderingSettings.FontSize * 2f), Mathf.RoundToInt(renderingSettings.FontSize * 2f), Mathf.RoundToInt(renderingSettings.FontSize * 1.5f), Mathf.RoundToInt(renderingSettings.FontSize * 1.5f));
            Tab.margin = new RectOffset(0, 0, 0, 5);
            Tab.normal.background = null;

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
                    //GUILayout.Space(10);

                    GUILayout.BeginHorizontal(TabHeader, GUILayout.ExpandWidth(false));
                    {
                        Text("LIGHT NAME", currentAliasedName, lightName =>
                        {
                            HandleAlias(lightName, lightManager, currentAliasedName);
                            UpdateLightName(lightName, lightManager, currentAliasedName);
                        },
                        !lightManager.SelectedLight.Light.transform.IsChildOf(GameObject.Find("StudioScene/Camera").transform));
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal(Tab);
                    Label(lightManager.SelectedLight.Light.name, "", true);

                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginVertical(Tab);
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
                        GUILayout.Label("Unknown light type");
                        break;
                }
                GUILayout.EndVertical();
            }
            else
            {

                GUILayout.BeginVertical(Tab);
                {
                    Label("Selected light is disabled.", "");
                }
                GUILayout.EndVertical();
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
            //Layers
            DrawLayersSettings(lightManager);
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
            //Alloy
            //DrawAlloySettings(lightManager, alloyLight);
        }

        private static void DrawBasicSettings(LightManager lightManager, GlobalSettings renderingSettings)
        {
            //Label("COLOR", "", true);
            //GUILayout.Space(10);
            SliderColorVertical("Light Color", lightManager.SelectedLight.Color, c => lightManager.SelectedLight.Color = c);
            //GUILayout.Space(20);
            //Label("INTENSITY", "", true);
            GUILayout.Space(10);
            SliderCompact("Intensity", lightManager.SelectedLight.Intensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N2", i => lightManager.SelectedLight.Intensity = i);
            //SliderCompact("Indirect Multiplier", lightManager.SelectedLight.Light.bounceIntensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N0", bi => lightManager.SelectedLight.Light.bounceIntensity = bi);
            ToggleAlt("Linear Intensity", renderingSettings.LightsUseLinearIntensity, false, useLinear => renderingSettings.LightsUseLinearIntensity = useLinear);
            GUILayout.Space(10);
            SliderCompact("Indirect Multiplier", lightManager.SelectedLight.Light.bounceIntensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N0", bi => lightManager.SelectedLight.Light.bounceIntensity = bi);
        }
        private static void DrawSunSettings(LightManager lightManager)
        {
            bool isSun = ReferenceEquals(lightManager.SelectedLight.Light, RenderSettings.sun);
            GUILayout.Space(labelspace);
            Label("SUN", "", true);
            GUILayout.Space(10);
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
                GUILayout.Space(labelspace);
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
                    GUILayout.Space(labelspace);
                    Label("POSITION", "", true);
                    GUILayout.Space(10);
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
                        GUILayout.Space(labelspace);
                        Label("POSITION", "", true);
                        GUILayout.Space(10);
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
                        GUILayout.Space(labelspace);
                        Label("POSITION", "", true);
                        GUILayout.Space(10);
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
            GUILayout.Space(labelspace);
            SliderCompact("Light Range", lightManager.SelectedLight.Range, 0.1f, 500f, "N1", range => { lightManager.SelectedLight.Range = range; });
        }
        private static void DrawSpotControls(LightManager lightManager)
        {
            GUILayout.Space(labelspace);
            SliderCompact("Light Range", lightManager.SelectedLight.Range, 0.1f, 500f, "N1", range => { lightManager.SelectedLight.Range = range; });
            SliderCompact("Spot Angle", lightManager.SelectedLight.SpotAngle, 1f, 179f, "N1", angle => { lightManager.SelectedLight.SpotAngle = angle; });
            if (lightManager.SelectedLight.SpotAngle > 90f)
                WarningCompact("Spot Angle values above 90 degrees will cause shadow quality degradation!");
        }

        //private static void DrawAlloySettings(LightManager lightManager, AlloyAreaLight alloyLight)
        //{
        //    //GUI.enabled = false;

        //    GUILayout.Space(20);
        //    Label("SPECULARITY", "", true);
        //    GUILayout.Space(10);
        //    ToggleAlt("Use Alloy Light", lightManager.UseAlloyLight, false, useAlloy => lightManager.UseAlloyLight = useAlloy);
        //    GUILayout.Space(10);
        //    if (lightManager.UseAlloyLight && null != alloyLight)
        //    {
        //        ToggleAlt("Specular Highlight", alloyLight.HasSpecularHighlight, false, highlight => alloyLight.HasSpecularHighlight = highlight);
        //        if (alloyLight.HasSpecularHighlight)
        //        {
        //            GUILayout.Space(10);
        //            SliderCompact("Specular Size", alloyLight.Radius, 0f, 1f, "N2", i => alloyLight.Radius = i);

        //            if (lightManager.SelectedLight.Type == LightType.Point)
        //            {
        //                GUILayout.Space(10);
        //                SliderCompact("Length", alloyLight.Length, 0f, 1f, "N2", i => alloyLight.Length = i);
        //            }
        //        }
        //    }
        //    //GUI.enabled = true;
        //}

        private static void DrawDirShadowsSettings(LightManager lightManager, CustomShadowResolutionHandler customShadowResolutionHandler)
        {

                GUILayout.Space(labelspace);
                Label("SHADOWS", "", true);
                GUILayout.Space(10);

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
                GUILayout.Space(labelspace);
                Label("SHADOWS", "", true);
                GUILayout.Space(10);

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

        private static void DrawLayersSettings(LightManager lightManager)
        {
            GUILayout.Space(labelspace);
            Label("LAYERS", "", true);
            GUILayout.Space(10);
            SelectionMaskVertical("Culling Mask", lightManager.SelectedLight.Light.cullingMask, mask => lightManager.SelectedLight.Light.cullingMask = mask, 2);
            GUILayout.Space(10);
            SelectionVertical("Render Mode", lightManager.SelectedLight.Light.renderMode, mode => lightManager.SelectedLight.Light.renderMode = mode, 3);
        }

        //private static void DrawCookieSettings(LightManager lightManager)
        //{
        //    GUILayout.Space(labelspace);
        //    Label("COOKIE", "", true);
        //    GUILayout.Space(10);



        //    CookieTextureManager cookieTextureManager = lightManager.SelectedLight.Light.GetComponent<CookieTextureManager>();
        //    ToggleAlt("Enable Cookie Texture", cookieTextureManager.enabled, false, isCookieEnabled => cookieTextureManager.enabled = isCookieEnabled);

        //    GUILayout.Space(5);
        //    if (cookieTextureManager.enabled)
        //    {
        //        Label("Cookie Texture: " + cookieTextureManager.cookiename, "", false);
        //        GUILayout.Space(5);
        //        DrawTextureGrid(
        //        cookieTextureManager.SPOTCookieNames,
        //        cookieTextureManager._spotCookieManager.Textures.ToArray(),
        //        cookieTextureManager.CurrentSpotCookieName,
        //        name => {
        //            if (name != cookieTextureManager.CurrentSpotCookieName)
        //            {
        //                cookieTextureManager.ApplyCookieTexture(name);
        //                //cookieTextureManager.Saved = name;
        //            }
        //        }, previewSize: 64f);
        //    }
        //}

        private static GameObject lightToDestroy = null;
        private static void LightOverviewModule(LightManager lightManager, LightObject l)
        {

            if (null == l || null == l.Light)
            {
                lightManager.Light();
                return;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);

            if (LightButton(PerLightSettings.NameForLight(l.Light), ReferenceEquals(l, lightManager.SelectedLight), true))
            {
                lightManager.SelectedLight = l;
            }
            GUILayout.FlexibleSpace();
            l.Enabled = ToggleButton(l.Enabled ? " ON" : "OFF", l.Enabled, true);
            if (!KKAPI.Studio.StudioAPI.InsideStudio && l.Light.name.StartsWith("(Graphics)"))
            {
                if (GUILayout.Button("-"))
                {
                    lightToDestroy = l.Light.gameObject;
                }
            }

            GUILayout.EndHorizontal();
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
            if (texture != null)
            {
                Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
            else
            {
                GUILayout.Box(GUIContent.none, GUILayout.Width(size), GUILayout.Height(size));
            }
        }
    }
}