using Graphics.Settings;
using KKAPI.Studio;
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
        //private static SEGI.SEGI segi;
        private static float lightbox = Inspector.Height * 0.25f;
        internal static void Draw(GlobalSettings renderingSettings, LightManager lightManager, LightingSettings lightingSettings, bool showAdvanced)
        {
            GUIStyle TabHeader = new GUIStyle(GUIStyles.tabcontent);
            TabHeader.padding = new RectOffset(25, 25, 0, 0);
            TabHeader.margin = new RectOffset(0, 0, 15, 0);
            TabHeader.normal.background = null;
            TabHeader.fixedHeight = 61;
            //TabHeader.fixedWidth = 200;

            GUIStyle Tab = new GUIStyle(GUIStyles.tabcontent);
            Tab.padding = new RectOffset(25, 25, 15, 15);
            Tab.margin = new RectOffset(0, 0, 0, 5);
            Tab.normal.background = null;
            //Tab.fixedHeight = 56;
            //Tab.fixedWidth = 200;

            //GUIStyle LightSetting = new GUIStyle(GUI.skin.box);
            //LightSetting.padding = new RectOffset(25, 25, 0, 25);
            //LightSetting.normal.background = null;
            ////LightSetting.fixedHeight = 56;

            GUIStyle LightList = new GUIStyle(GUIStyles.tabsmall);
            LightList.margin = new RectOffset(0, 0, 0, 10);
            LightList.padding = new RectOffset(10, 10, 10, 4);
            //LightList.normal.background = null;
            //LightList.fixedWidth = 400;
            //LightList.fixedHeight = 400;

            GUIStyle LightButton = new GUIStyle(GUIStyles.tabsmall);
            LightButton.margin = new RectOffset(0, 0, 0, 0);
            LightButton.padding = new RectOffset(0, 0, 0, 0);
            //LightButton.normal.background = null;
            LightButton.fixedWidth = 350;
            LightButton.fixedHeight = lightbox;

            GUIStyle LightListBox = new GUIStyle(GUIStyles.tabcontent);
            LightListBox.margin = new RectOffset(0, 0, 0, 0);
            LightListBox.padding = new RectOffset(0, 0, 0, 0);
            LightListBox.normal.background = null;
            LightListBox.fixedWidth = 350;
            //LightListBox.fixedHeight = 400;

            GUIStyle TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(0, 0, 0, 0);
            TabContent.margin = new RectOffset(0, 0, 0, 0);
            //TabContent.normal.background = null;

            //GUIStyle TabContent = new GUIStyle(GUIStyles.tabcontent);
            //TabContent.padding = new RectOffset(0, 0, 0, 0);
            //TabContent.margin = new RectOffset(0, 0, 0, 0);
            //EmptyBoxR.normal.background = null;

            int width = Inspector.Width;

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
                        if (GUILayout.Button("Load Default Map Lights"))
                        {
                            Graphics.Instance.PresetManager.LoadMapLights(true);
                            Graphics.Instance.PresetManager.SaveMapLights(false);
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
                                else if (LightSettings.LightType.Directional == LightSettings.LightType.Directional)
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
                            if (lightManager.SelectedLight.Enabled)
                            {
                                AlloyAreaLight alloyLight = null;
                                if (lightManager.UseAlloyLight)
                                {
                                    alloyLight = lightManager.SelectedLight.Light.GetComponent<AlloyAreaLight>();
                                }
                                if (Graphics.Instance.IsStudio())
                                {
                                    string currentAliasedName = PerLightSettings.NameForLight(lightManager.SelectedLight.Light);
                                    //GUILayout.Space(10);
                                    GUILayout.BeginHorizontal(TabHeader, GUILayout.ExpandWidth(false));
                                    {

                                        Text("LIGHT NAME", currentAliasedName, lightName =>
                                        {


                                            if (lightName != currentAliasedName && lightName != lightManager.SelectedLight.Light.name)
                                            {
                                                PerLightSettings.SetAlias(lightManager.SelectedLight.Light, lightName);
                                            }
                                            else if (lightName == lightManager.SelectedLight.Light.name && PerLightSettings.AliasedLight(lightManager.SelectedLight.Light))
                                            {
                                                PerLightSettings.ClearAlias(lightManager.SelectedLight.Light);
                                            }
                                            if (lightManager.SelectedLight != null && lightManager.SelectedLight.Light != null)
                                            {
                                                string currentLightName = lightManager.SelectedLight.Light.name;

                                                if (lightName != currentLightName && !string.IsNullOrEmpty(lightName))
                                                {
                                                    lightManager.SelectedLight.Light.name = lightName;
                                                }
                                                else if (lightName == currentLightName && PerLightSettings.AliasedLight(lightManager.SelectedLight.Light))
                                                {
                                                    PerLightSettings.ClearAlias(lightManager.SelectedLight.Light);
                                                }
                                            }
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
                                {
                                    GUILayout.BeginVertical(Tab);
                                    Label("COLOR", "", true);
                                    SliderColor("Color", lightManager.SelectedLight.Color, c => lightManager.SelectedLight.Color = c);

                                    //if (renderingSettings.LightsUseColorTemperature)
                                    //{
                                    //    SliderTemp("Temperature (K)", lightManager.SelectedLight.Light.colorTemperature, 1f, 30000f, "N0", t =>
                                    //        lightManager.SelectedLight.Light.colorTemperature = t);
                                    //}

                                    GUILayout.Space(30);
                                    Label("SHADOWS", "", true);
                                    Selection("Shadow Type", lightManager.SelectedLight.Shadows, type => lightManager.SelectedLight.Shadows = type);
                                    Slider("Strength", lightManager.SelectedLight.Light.shadowStrength, 0f, 1f, "N2", strength => lightManager.SelectedLight.Light.shadowStrength = strength);
                                    if (LightType.Directional == lightManager.SelectedLight.Type && renderingSettings.UsePCSS)
                                        Slider("Resolution", lightManager.SelectedLight.ShadowCustomResolution, 0, PCSSLight.MaxShadowCustomResolution, resolution => lightManager.SelectedLight.ShadowCustomResolution = resolution);
                                    else
                                        Selection("Resolution", lightManager.SelectedLight.Light.shadowResolution, resolution => lightManager.SelectedLight.Light.shadowResolution = resolution, 2);
                                    Slider("Bias", lightManager.SelectedLight.Light.shadowBias, 0f, 2f, "N3", bias => lightManager.SelectedLight.Light.shadowBias = bias);
                                    Slider("Normal Bias", lightManager.SelectedLight.Light.shadowNormalBias, 0f, 3f, "N2", nbias => lightManager.SelectedLight.Light.shadowNormalBias = nbias);
                                    Slider("Near Plane", lightManager.SelectedLight.Light.shadowNearPlane, 0f, 10f, "N2", np => lightManager.SelectedLight.Light.shadowNearPlane = np);
                                    GUILayout.Space(30);
                                    Label("INTENSITY", "", true);
                                    Slider("Intensity", lightManager.SelectedLight.Intensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N2", i => lightManager.SelectedLight.Intensity = i);
                                    Slider("Indirect Multiplier", lightManager.SelectedLight.Light.bounceIntensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N0", bi => lightManager.SelectedLight.Light.bounceIntensity = bi);
                                    GUILayout.Space(10);
                                    if (lightManager.SelectedLight.Type == LightType.Directional)
                                    {
                                        bool isSun = ReferenceEquals(lightManager.SelectedLight.Light, RenderSettings.sun);
                                        //if (null != RenderSettings.sun && !isSun)
                                        Toggle("Sun Source", isSun, false, suns =>
                                        {
                                            if (suns)
                                            {
                                                RenderSettings.sun = lightManager.SelectedLight.Light;
                                                //ConnectSunToUnderwater.ConnectSun();
                                            }
                                            else
                                            {
                                                RenderSettings.sun = null;
                                            }
                                        });

                                        //if (null != Graphics.Instance.CameraSettings.MainCamera && null == segi)
                                        //segi = Graphics.Instance.CameraSettings.MainCamera.GetComponent<SEGI.SEGI>();

                                        //if (null != segi && segi.enabled)
                                        //{
                                        //    bool isSEGISun = ReferenceEquals(lightManager.SelectedLight.Light, segi.sun);
                                        //    if (null != segi.sun && !isSEGISun)
                                        //    GUI.enabled = false;
                                        //    Toggle("SEGI Sun source", isSEGISun, false, sun =>
                                        //    {
                                        //        if (sun)
                                        //        {
                                        //            segi.sun = lightManager.SelectedLight.Light;
                                        //        }
                                        //        else
                                        //        {
                                        //            segi.sun = null;
                                        //        }
                                        //    });
                                        //    GUI.enabled = true;
                                        //}

                                        if (lightManager.SelectedLight.Light.name != "Cam Light" || !lightManager.SelectedLight.Light.transform.IsChildOf(GameObject.Find("StudioScene/Camera").transform))
                                        {
                                            if (!lightManager.SelectedLight.AdditionalCamLight)
                                            {
                                                Vector3 rot = lightManager.SelectedLight.Rotation;
                                                GUILayout.Space(30);
                                                Label("POSITION", "", true);
                                                Slider("Vertical Rotation", rot.x, LightSettings.RotationXMin, LightSettings.RotationXMax, "N1", x => { rot.x = x; });
                                                Slider("Horizontal Rotation", rot.y, LightSettings.RotationYMin, LightSettings.RotationYMax, "N1", y => { rot.y = y; });

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
                                                    GUILayout.Space(30);
                                                    Label("POSITION", "", true);
                                                    Slider("Vertical Rotation", rot.x, LightSettings.RotationXMin, LightSettings.RotationXMax, "N1", x => { rot.x = x; });
                                                    Slider("Horizontal Rotation", rot.y, LightSettings.RotationYMin, LightSettings.RotationYMax, "N1", y => { rot.y = y; });

                                                    if (rot != graphicsAdditionalCamLight.OriginalRotation.eulerAngles)
                                                    {
                                                        graphicsAdditionalCamLight.OriginalRotation = Quaternion.Euler(rot);
                                                    }
                                                }
                                                else
                                                {
                                                    Vector3 rot = lightManager.SelectedLight.OciLight.guideObject.changeAmount.rot;
                                                    GUILayout.Space(30);
                                                    Label("POSITION", "", true);
                                                    Slider("Vertical Rotation", rot.x, LightSettings.RotationXMin, LightSettings.RotationXMax, "N1", x => { rot.x = x; });
                                                    Slider("Horizontal Rotation", rot.y, LightSettings.RotationYMin, LightSettings.RotationYMax, "N1", y => { rot.y = y; });

                                                    if (rot != lightManager.SelectedLight.OciLight.guideObject.changeAmount.rot)
                                                    {
                                                        lightManager.SelectedLight.OciLight.guideObject.changeAmount.rot = rot;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Slider("Light Range", lightManager.SelectedLight.Range, 0.1f, 500f, "N1", range => { lightManager.SelectedLight.Range = range; });
                                        if (lightManager.SelectedLight.Type == LightType.Spot)
                                        {
                                            Slider("Spot Angle", lightManager.SelectedLight.SpotAngle, 1f, 179f, "N1", angle => { lightManager.SelectedLight.SpotAngle = angle; });
                                        }
                                    }
                                    GUILayout.Space(10);
                                    if (lightManager.UseAlloyLight && alloyLight.HasSpecularHighlight && null != alloyLight)
                                    {
                                        Slider("Specular Highlight", alloyLight.Radius, 0f, 1f, "N2", i => alloyLight.Radius = i);

                                        if (lightManager.SelectedLight.Type == LightType.Point)
                                        {
                                            Slider("Length", alloyLight.Length, 0f, 1f, "N2", i => alloyLight.Length = i);
                                        }
                                    }
                                    if (!lightManager.SelectedLight.IsNotAdditionalCamAvailable && (lightManager.SelectedLight.Type == LightType.Directional || lightManager.SelectedLight.Type == LightType.Spot))
                                    {
                                        Toggle("Additional Cam Light", lightManager.SelectedLight.AdditionalCamLight, false, addCamLight => lightManager.SelectedLight.AdditionalCamLight = addCamLight);
                                    }
                                    SelectionMask("Culling Mask", lightManager.SelectedLight.Light.cullingMask, mask => lightManager.SelectedLight.Light.cullingMask = mask, 2);
                                    if (showAdvanced)
                                    {
                                        Selection("Render Mode", lightManager.SelectedLight.Light.renderMode, mode => lightManager.SelectedLight.Light.renderMode = mode);
                                        
                                    }

                                    GUILayout.EndVertical();
                                }
                                

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
                        else
                        {
                            GUILayout.BeginVertical(Tab);
                            {
                                Label("Select a light source on the left panel.", "");
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical(Tab);
                        {
                            Label("GLOBAL SETTINGS", "", true);
                            Toggle("Use Alloy Light", lightManager.UseAlloyLight, false, useAlloy => lightManager.UseAlloyLight = useAlloy);
                            Toggle("Lights Use Linear Intensity", renderingSettings.LightsUseLinearIntensity, false, useLinear => renderingSettings.LightsUseLinearIntensity = useLinear);
                            //Toggle("Lights Use Color Temperature", renderingSettings.LightsUseColorTemperature, false, useTemperature => renderingSettings.LightsUseColorTemperature = useTemperature);
                            GUILayout.Space(15);
                        }
                        GUILayout.EndVertical();
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

        //private static void LightGroup(LightManager lightManager, string typeName, LightSettings.LightType type)
        //{

        //    GUIStyle EmptyBoxR = new GUIStyle(GUI.skin.box);
        //    EmptyBoxR.padding = new RectOffset(25, 25, 0, 0);
        //    EmptyBoxR.normal.background = null;

        //    EmptyBoxR.fixedHeight = 56;
        //    EmptyBoxR.fixedWidth = 400;

        //    GUILayout.BeginHorizontal(EmptyBoxR);
        //    {
        //        Label(typeName, "", true);
        //        //GUILayout.FlexibleSpace();

        //    }
        //    GUILayout.EndHorizontal();
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
    }
}