using Graphics.Settings;
using Graphics.Textures;
using System.Linq;
using UnityEngine;
using static Graphics.Inspector.Util;
using static Graphics.PathUtils;

namespace Graphics.Inspector
{
    internal static class LightingInspector
    {
        private const float ExposureMin = 0f;
        private const float ExposureMax = 8f;

        private const float RotationMin = 0f;
        private const float RotationMax = 360f;

        private static Vector2 cubeMapScrollView;

        private static Vector2 dynSkyScrollView;

        private static int cachedFontSize = -1;
        private static int paddingL, paddingR;
        private static GUIStyle BoxPadding, SelBox, EmptyBox;

        static void UpdateCachedValues(GlobalSettings renderSettings)
        {
            if (cachedFontSize == renderSettings.FontSize) return;

            cachedFontSize = renderSettings.FontSize;

            paddingL = Mathf.RoundToInt(renderSettings.FontSize * 2f);
            paddingR = Mathf.RoundToInt(renderSettings.FontSize * 2.9f);

            BoxPadding = new GUIStyle(GUIStyles.tabcontent);
            BoxPadding.padding = new RectOffset(paddingL, paddingR, paddingL, 0);
            BoxPadding.margin = new RectOffset(0, 0, 0, 5);
            //BoxPadding.normal.background = null;

            EmptyBox = new GUIStyle(GUIStyles.tabcontent);
            EmptyBox.padding = new RectOffset(0, 0, 0, 0);
            EmptyBox.margin = new RectOffset(0, 0, 0, 0);
            EmptyBox.normal.background = null;

            SelBox = new GUIStyle(GUI.skin.box);
            SelBox.padding = new RectOffset(0, 0, 0, 0);
            SelBox.margin = new RectOffset(2, 2, 2, 2);
            SelBox.normal.background = null;
            //SelBox.fixedHeight = 350;

        }

        internal static void Draw(LightingSettings lightingSettings, SkyboxManager skyboxManager, LightManager lightmanager, GlobalSettings renderSettings, bool showAdvanced)
        {
            UpdateCachedValues(renderSettings);

            string assetPath = skyboxManager.AssetPath;
            string cubemapname = "";
            cubemapname = skyboxManager.CurrentTexturePath != SkyboxManager.noCubemap ?
                GetRelativePathWithoutExtension(assetPath, skyboxManager.CurrentTexturePath) : "(None)";

            GUILayout.BeginVertical(EmptyBox);
            {
                GUILayout.BeginVertical(BoxPadding);
                {
                    Label("CUBEMAP SKYBOX:", cubemapname, true);
                    GUILayout.Space(10);
                }
                GUILayout.EndVertical();
                //inactivate controls if no cubemap
                if (skyboxManager.TexturePaths.IsNullOrEmpty())
                {
                    Label("No custom cubemaps found", "");
                    GUILayout.Space(10);
                }
                else
                {
                    cubeMapScrollView = GUILayout.BeginScrollView(cubeMapScrollView, GUILayout.MaxHeight(400));
                    int selectedCubeMapIdx = GUILayout.SelectionGrid(skyboxManager.CurrentTextureIndex, skyboxManager.Previews.ToArray(), Inspector.Width / 130, SelBox);
                    if (-1 != selectedCubeMapIdx)
                    {
                        skyboxManager.CurrentTexturePath = skyboxManager.TexturePaths[selectedCubeMapIdx];
                    }

                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
            dynSkyScrollView = GUILayout.BeginScrollView(dynSkyScrollView);
            GUILayout.BeginVertical(BoxPadding);
            {
                //GUILayout.Space(10);
                //Label("CUBEMAP:", text, true);
                //GUILayout.Space(30);
                Label("ENVIRONMENT LIGHTING", "", true);
                GUILayout.Space(10);
                Selection("Source", lightingSettings.AmbientModeSetting, mode =>
                {
                    lightingSettings.AmbientModeSetting = mode;
                    if (mode != LightingSettings.AIAmbientMode.Skybox)
                    {
                        skyboxManager.CurrentTexturePath = SkyboxManager.noCubemap;
                    }
                });
                GUILayout.Space(10);
                if (lightingSettings.AmbientModeSetting == LightingSettings.AIAmbientMode.Skybox)
                {
                    Slider("Intensity", lightingSettings.AmbientIntensity, LightSettings.IntensityMin, LightSettings.IntensityMax, "N1", intensity => { lightingSettings.AmbientIntensity = intensity; });

                    if (null != skyboxManager.Skybox && null != skyboxManager.Skyboxbg)
                    {
                        Material skybox = skyboxManager.Skybox;
                        if (skybox.HasProperty("_Exposure"))
                            Slider("Exposure", skyboxManager.Exposure, ExposureMin, ExposureMax, "N1", exp => { skyboxManager.Exposure = exp; skyboxManager.Update = true; });
                        if (skybox.HasProperty("_Rotation"))
                            Slider("Rotation", skyboxManager.Rotation, RotationMin, RotationMax, "N1", rot => { skyboxManager.Rotation = rot; skyboxManager.Update = true; });
                        GUILayout.Space(10);
                        if (skybox.HasProperty("_Tint"))
                            SliderColor("Skybox Tint", skyboxManager.Tint, c => { skyboxManager.Tint = c; skyboxManager.Update = true; }, true);
                        GUILayout.Space(10);
                        //if (skybox.shader.name == "Skybox/Cubemap Ground Projection")
                        //    Toggle("Ground Projection", skyboxManager.Projection, false, proj => { skyboxManager.Projection = proj; skyboxManager.Update = true; });

                        //if (skyboxManager.Projection)
                        //{
                        //    Slider("Horizon", skyboxManager.Horizon, -1f, 1f, "N2", horizon => { skyboxManager.Horizon = horizon; skyboxManager.Update = true; });
                        //    Slider("Scale", skyboxManager.Scale, -50f, 50f, "N2", scale => { skyboxManager.Scale = scale; skyboxManager.Update = true; });
                        //}

                        DrawDynSkyboxOptions(lightingSettings, skyboxManager, lightmanager, showAdvanced);
                    }
                }
                else if (lightingSettings.AmbientModeSetting == LightingSettings.AIAmbientMode.Trilight)
                {
                    SliderColor("Sky Color", lightingSettings.SkyColor, c => { RenderSettings.ambientSkyColor = c; });
                    SliderColor("Horizon Color", lightingSettings.EquatorColor, c => { RenderSettings.ambientEquatorColor = c; });
                    SliderColor("Ground Color", lightingSettings.GroundColor, c => { RenderSettings.ambientGroundColor = c; });
                }
                else
                {
                    SliderColor("Ambient Light", lightingSettings.AmbientLight, c => { RenderSettings.ambientLight = c; });
                }
                GUILayout.Space(10);
                //Label("ENVIRONMENT REFLECTIONS", "", true);
                //GUILayout.Space(1);
                //Selection("Resolution", lightingSettings.ReflectionResolution, LightingSettings.ReflectionResolutions, resolution => lightingSettings.ReflectionResolution = resolution);
                //Slider("Intensity", lightingSettings.ReflectionIntensity, 0f, 1f, "N1", intensity => { lightingSettings.ReflectionIntensity = intensity; });
                //Slider("Bounces", lightingSettings.ReflectionBounces, 1, 5, bounces => { lightingSettings.ReflectionBounces = bounces; });
                //GUILayout.Space(25);

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        internal static void DrawDynSkyboxOptions(LightingSettings lightingSettings, SkyboxManager skyboxManager, LightManager lightManager, bool showAdvanced)
        {
            if (skyboxManager != null && skyboxManager.Skybox != null)
            {
                Material mat = skyboxManager.Skybox;
                string shaderName = mat.shader.name;
                bool isSkyboxProcedural = shaderName.Contains("Skybox/Procedural");
                bool isSkyboxTwoColors = shaderName.Contains("SkyBox/Simple Two Colors");
                bool isSkyboxFourColors = shaderName.Contains("SkyboxPlus/Gradients");
                bool isSkyboxHemisphere = shaderName.Contains("SkyboxPlus/Hemisphere");
                bool isSkyboxAIOSky = shaderName.Contains("AIOsky/V2");
                bool isSkyboxGroundProjection = shaderName.Contains("Skybox/Cubemap Ground Projection");

                if (isSkyboxProcedural)
                {
                    DrawProceduralSkyboxSettings(lightManager, mat, skyboxManager);
                }
                else if (isSkyboxTwoColors)
                {
                    DrawTwoColorsSkyboxSettings(mat, skyboxManager);
                }
                else if (isSkyboxFourColors)
                {
                    DrawFourColorsSkyboxSettings(mat, skyboxManager);
                }
                else if (isSkyboxHemisphere)
                {
                    DrawHemisphereSkyboxSettings(mat, skyboxManager);
                }
                else if (isSkyboxAIOSky)
                {
                    DrawAIOSkyboxSettings(lightManager, mat, skyboxManager);
                }
                else if (isSkyboxGroundProjection)
                {
                    DrawGroundProjectionSkyboxSettings(mat, skyboxManager);
                }
            }
        }
        private static void DrawProceduralSkyboxSettings(LightManager lightManager, Material mat, SkyboxManager skyboxManager)
        {
            ProceduralSkyboxSettings settings = SkyboxManager.dynProceduralSkySettings;
            if (settings != null)
            {
                Label("Unity Skybox Settings", "", true);
                GUILayout.Space(10);
                LightSelector(lightManager, "Sun Source", RenderSettings.sun, source => RenderSettings.sun = source);
                Selection("Sun", (ProceduralSkyboxSettings.SunDisk)settings.sunDisk, quality => { settings.sunDisk = quality; SkyboxManager.UpdateProceduralSkySettings(); });
                Slider("Sun Size", settings.sunSize, 0, 1, "N2", value => { settings.sunSize = value; SkyboxManager.UpdateProceduralSkySettings(); });
                Slider("Sun Size Convergence", settings.sunsizeConvergence, 1, 10, "N2", value => { settings.sunsizeConvergence = value; SkyboxManager.UpdateProceduralSkySettings(); });
                Slider("Atmosphere Thickness", settings.atmosphereThickness, 0, 5, "N2", value => { settings.atmosphereThickness = value; SkyboxManager.UpdateProceduralSkySettings(); });
                SliderColor("Sky Tint", settings.skyTint, c => { settings.skyTint = c; SkyboxManager.UpdateProceduralSkySettings(); }, true);
                SliderColor("Ground Color", settings.groundTint, c => { settings.groundTint = c; SkyboxManager.UpdateProceduralSkySettings(); }, true);
                Slider("Exposure", settings.exposure, 0, 8, "N2", value => { settings.exposure = value; SkyboxManager.UpdateProceduralSkySettings(); });
            }
        }
        private static void DrawTwoColorsSkyboxSettings(Material mat, SkyboxManager skyboxManager)
        {
            TwoPointColorSkyboxSettings settings = SkyboxManager.dynTwoPointGradientSettings;

            if (settings != null)
            {
                Label("Two Point Color Skybox Settings", "", true);

                GUILayout.Space(10);
                SliderColor("Colour A", settings.colorA, c => { settings.colorA = c; SkyboxManager.UpdateTwoPointGradientSettings(); }, true);
                Slider("Intensity A", settings.intensityA, 0, 2, "N2", intensity => { settings.intensityA = intensity; SkyboxManager.UpdateTwoPointGradientSettings(); });
                Dimension("Box Size", settings.directionA, direction => { settings.directionA = direction; SkyboxManager.UpdateTwoPointGradientSettings(); });

                GUILayout.Space(10);
                SliderColor("Colour B", settings.colorB, c => { settings.colorB = c; SkyboxManager.UpdateTwoPointGradientSettings(); }, true);
                Slider("Intensity B", settings.intensityB, 0, 2, "N2", intensity => { settings.intensityB = intensity; SkyboxManager.UpdateTwoPointGradientSettings(); });
                Dimension("Box Size", settings.directionB, direction => { settings.directionB = direction; SkyboxManager.UpdateTwoPointGradientSettings(); });
            }
        }
        private static void DrawFourColorsSkyboxSettings(Material mat, SkyboxManager skyboxManager)
        {
            FourPointGradientSkyboxSetting settings = SkyboxManager.dynFourPointGradientSettings;

            if (settings != null)
            {
                GUILayout.Space(10);
                Label("Four Point Gradient Skybox Settings", "", true);
                GUILayout.Space(10);
                SliderColor("Color1", settings.colorA, c => { settings.colorA = c; SkyboxManager.UpdateFourPointGradientSettings(); }, true);
                SliderColor("Color2", settings.colorB, c => { settings.colorB = c; SkyboxManager.UpdateFourPointGradientSettings(); }, true);
                SliderColor("Color3", settings.colorC, c => { settings.colorC = c; SkyboxManager.UpdateFourPointGradientSettings(); }, true);
                SliderColor("Color4", settings.colorD, c => { settings.colorD = c; SkyboxManager.UpdateFourPointGradientSettings(); }, true);
                Dimension("Direction1", settings.directionA, direction => { settings.directionA = direction; SkyboxManager.UpdateFourPointGradientSettings(); });
                Dimension("Direction2", settings.directionB, direction => { settings.directionB = direction; SkyboxManager.UpdateFourPointGradientSettings(); });
                Dimension("Direction3", settings.directionC, direction => { settings.directionC = direction; SkyboxManager.UpdateFourPointGradientSettings(); });
                Dimension("Direction4", settings.directionD, direction => { settings.directionD = direction; SkyboxManager.UpdateFourPointGradientSettings(); });
                Slider("Exponent1", settings.exponentA, 0, 40, "N1", intensity => { settings.exponentA = intensity; SkyboxManager.UpdateFourPointGradientSettings(); });
                Slider("Exponent2", settings.exponentB, 0, 40, "N1", intensity => { settings.exponentB = intensity; SkyboxManager.UpdateFourPointGradientSettings(); });
                Slider("Exponent3", settings.exponentC, 0, 40, "N1", intensity => { settings.exponentC = intensity; SkyboxManager.UpdateFourPointGradientSettings(); });
                Slider("Exponent4", settings.exponentD, 0, 40, "N1", intensity => { settings.exponentD = intensity; SkyboxManager.UpdateFourPointGradientSettings(); });
            }
        }
        private static void DrawHemisphereSkyboxSettings(Material mat, SkyboxManager skyboxManager)
        {
            HemisphereGradientSkyboxSetting settings = SkyboxManager.dynHemisphereGradientSettings;

            if (settings != null)
            {
                Label("Hemisphere Skybox Settings", "", true);
                GUILayout.Space(10);
                SliderColor("North Pole", settings.Top, c => { settings.Top = c; SkyboxManager.UpdateHemisphereGradientSettings(); }, true);
                SliderColor("Equator", settings.Middle, c => { settings.Middle = c; SkyboxManager.UpdateHemisphereGradientSettings(); }, true);
                SliderColor("South Pole", settings.Bottom, c => { settings.Bottom = c; SkyboxManager.UpdateHemisphereGradientSettings(); }, true);
            }
        }
        private static void DrawAIOSkyboxSettings(LightManager lightManager, Material mat, SkyboxManager skyboxManager)
        {
            AIOSkySettings aioskysettings = SkyboxManager.dynAIOSkySetting;

            if (aioskysettings != null)
            {
                GUILayout.Space(10);
                Label("SUN", "", true);
                GUILayout.Space(2);
                LightSelector(lightManager, "Sun Source", RenderSettings.sun, source => RenderSettings.sun = source);
                GUILayout.Space(15);
                SliderColor("Sun Color", aioskysettings.sunColor, sun => { aioskysettings.sunColor = sun; SkyboxManager.UpdateAIOSkySettings(); }, true);
                Slider("Sun Min", aioskysettings.sunMin, 0f, 0.02f, "N2", sunmin => { aioskysettings.sunMin = sunmin; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Sun Max", aioskysettings.sunMax, 0.9f, 1.0f, "N2", sunmax => { aioskysettings.sunMax = sunmax; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Sun Glow", aioskysettings.SunGlow, 0f, 0.2f, "N2", sunglow => { aioskysettings.SunGlow = sunglow; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("SKY", "", true);
                GUILayout.Space(2);
                Slider("Atmosphere Start", aioskysettings.AtmosphereStart, -0.2f, 0.2f, "N2", atmospherestart => { aioskysettings.AtmosphereStart = atmospherestart; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Atmosphere End", aioskysettings.AtmosphereEnd, 0f, 1f, "N2", atmosphereend => { aioskysettings.AtmosphereEnd = atmosphereend; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                SliderColor("Day Sky", aioskysettings.DaySky, daysky => { aioskysettings.DaySky = daysky; SkyboxManager.UpdateAIOSkySettings(); }, true);
                GUILayout.Space(10);
                SliderColor("Day Atmosphere", aioskysettings.DayAtmosphere, dayatmosphere => { aioskysettings.DayAtmosphere = dayatmosphere; SkyboxManager.UpdateAIOSkySettings(); }, true);
                Slider("Day Range", aioskysettings.DayRange, -1f, 1f, "N2", dayrange => { aioskysettings.DayRange = dayrange; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                SliderColor("Sunset Sky", aioskysettings.SunSetSky, sunsetsky => { aioskysettings.SunSetSky = sunsetsky; SkyboxManager.UpdateAIOSkySettings(); }, true);
                GUILayout.Space(10);
                Slider("Sunset Range", aioskysettings.setRange, -1f, 1f, "N2", sunsetrange => { aioskysettings.setRange = sunsetrange; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                SliderColor("Night Sky", aioskysettings.NightSky, nightsky => { aioskysettings.NightSky = nightsky; SkyboxManager.UpdateAIOSkySettings(); }, true);
                GUILayout.Space(10);
                SliderColor("Night Atmosphere", aioskysettings.NightAtmosphere, nightatmosphere => { aioskysettings.NightAtmosphere = nightatmosphere; SkyboxManager.UpdateAIOSkySettings(); }, true);
                Slider("Night Range", aioskysettings.nightRange, -1f, 1f, "N2", nightrange => { aioskysettings.nightRange = nightrange; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("MAIN CLOUDS", "", true);
                GUILayout.Space(2);
                Slider("Clouds Density Sky Edge", aioskysettings.CloudsDensitySkyEdge, 0f, 1.5f, "N2", cloudsdensityskyedge => { aioskysettings.CloudsDensitySkyEdge = cloudsdensityskyedge; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Clouds Density", aioskysettings.CloudsDensity, -1f, 1f, "N2", cloudsDensity => { aioskysettings.CloudsDensity = cloudsDensity; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Clouds Thickness", aioskysettings.CloudsThickness, 0.0001f, 0.3f, "N2", cloudsThickness => { aioskysettings.CloudsThickness = cloudsThickness; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Dome Curved", aioskysettings.DomeCurved, -2f, 2f, "N2", domeCurved => { aioskysettings.DomeCurved = domeCurved; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Clouds Scale", aioskysettings.CloudsScale, 0f, 5f, "N2", cloudsScale => { aioskysettings.CloudsScale = cloudsScale; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Detail Scale 01", aioskysettings.DetailScale01, 0f, 2f, "N2", detailScale01 => { aioskysettings.DetailScale01 = detailScale01; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Detail Scale 02", aioskysettings.DetailScale02, 0f, 2f, "N2", detailScale02 => { aioskysettings.DetailScale02 = detailScale02; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Detail Scale 03", aioskysettings.DetailScale03, 0f, 2f, "N2", detailScale03 => { aioskysettings.DetailScale03 = detailScale03; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("DAY CLOUDS", "", true);
                GUILayout.Space(2);
                Slider("Sun Light Power", aioskysettings.SunLightPower, 0f, 10f, "N2", sunLightPower => { aioskysettings.SunLightPower = sunLightPower; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                SliderColor("Day Transmission Color", aioskysettings.DayTransmissionColor, dayTransmissionColor => { aioskysettings.DayTransmissionColor = dayTransmissionColor; SkyboxManager.UpdateAIOSkySettings(); }, true);
                Slider("Day Clouds Transmission", aioskysettings.DayCloudsTransmission, 0f, 5f, "N2", dayCloudsTransmission => { aioskysettings.DayCloudsTransmission = dayCloudsTransmission; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("AmbientLight", aioskysettings.AmbientLight, 0f, 1f, "N2", ambientl => { aioskysettings.AmbientLight = ambientl; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Clouds Brightness", aioskysettings.CloudsBrightness, -1f, 1f, "N2", cloudsBrightness => { aioskysettings.CloudsBrightness = cloudsBrightness; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Clouds Contract", aioskysettings.CloudsContract, 0f, 1f, "N2", cloudsContract => { aioskysettings.CloudsContract = cloudsContract; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("NIGHT CLOUDS", "", true);
                GUILayout.Space(2);
                SliderColor("Moon Light", aioskysettings.MoonLight, moonLight => { aioskysettings.MoonLight = moonLight; SkyboxManager.UpdateAIOSkySettings(); }, true);
                Slider("Moon Light Power", aioskysettings.MoonLightPower, 0f, 10f, "N2", moonLightPower => { aioskysettings.MoonLightPower = moonLightPower; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                SliderColor("Night Transmission Color", aioskysettings.NightTransmissionColor, nightTransmissionColor => { aioskysettings.NightTransmissionColor = nightTransmissionColor; SkyboxManager.UpdateAIOSkySettings(); }, true);
                Slider("Night Clouds Transmission", aioskysettings.NightCloudsTransmission, 0f, 5f, "N2", nightCloudsTransmission => { aioskysettings.NightCloudsTransmission = nightCloudsTransmission; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Night Ambient Light", aioskysettings.NightAmbientLight, 0f, 1f, "N2", nightAmbientLight => { aioskysettings.NightAmbientLight = nightAmbientLight; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Night Clouds Brightness", aioskysettings.NightCloudsBrightness, -1f, 1f, "N2", nightCloudsBrightness => { aioskysettings.NightCloudsBrightness = nightCloudsBrightness; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Night Clouds Contrast", aioskysettings.NightCloudsContract, 0f, 1f, "N2", nightCloudsContract => { aioskysettings.NightCloudsContract = nightCloudsContract; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("Additional Clouds Color", "", true);
                GUILayout.Space(2);
                Slider("Clouds Shadow Weight", aioskysettings.CloudsShadowWeight, 0f, 1f, "N2", cloudsShadowWeight => { aioskysettings.CloudsShadowWeight = cloudsShadowWeight; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Sun Z Offset", aioskysettings.SunZoffset, -0.5f, 0.5f, "N2", sunZoffset => { aioskysettings.SunZoffset = sunZoffset; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("CLOUDS ANIMATION", "", true);
                GUILayout.Space(2);
                Dimension("Clouds Pan", aioskysettings.CloudsPan, cloudpan => { aioskysettings.CloudsPan = cloudpan; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Clouds Rotation", aioskysettings.CloudsRotation, 0f, 7f, "N2", cloudsRotation => { aioskysettings.CloudsRotation = cloudsRotation; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Time Scale", aioskysettings.timeScale, 0f, 0.05f, "N2", timeScale => { aioskysettings.timeScale = timeScale; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Distortion Time", aioskysettings.DistortionTime, 0f, 0.05f, "N2", distortionTime => { aioskysettings.DistortionTime = distortionTime; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("STAR FIELD", "", true);
                GUILayout.Space(2);
                Slider("BG Scale", aioskysettings.BGscale, 0f, 0.5f, "N2", bgScale => { aioskysettings.BGscale = bgScale; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("BG Rotate", aioskysettings.BGRotate, -7f, 7f, "N2", bgRotate => { aioskysettings.BGRotate = bgRotate; SkyboxManager.UpdateAIOSkySettings(); });
                Slider("Moon Scale", aioskysettings.moonScale, 1f, 40f, "N2", moonScale => { aioskysettings.moonScale = moonScale; SkyboxManager.UpdateAIOSkySettings(); });
                Dimension("Moon Position", aioskysettings.MoonPosition, moonpos => { aioskysettings.MoonPosition = moonpos; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("FOG", "", true);
                GUILayout.Space(2);
                SliderColor("Fog Color", aioskysettings.FogColor, fogColor => { aioskysettings.FogColor = fogColor; SkyboxManager.UpdateAIOSkySettings(); }, true);
                Slider("Fog Level", aioskysettings.FogLevel, 0f, 1f, "N2", fogLevel => { aioskysettings.FogLevel = fogLevel; SkyboxManager.UpdateAIOSkySettings(); });
                GUILayout.Space(10);
                Label("GROUND", "", true);
                GUILayout.Space(2);
                SliderColor("Ground Color", aioskysettings.GroundColor, groundColor => { aioskysettings.GroundColor = groundColor; SkyboxManager.UpdateAIOSkySettings(); }, true);
                Slider("Ground Level", aioskysettings.GroundLevel, -0.5f, 0.85f, "N2", groundLevel => { aioskysettings.GroundLevel = groundLevel; SkyboxManager.UpdateAIOSkySettings(); });
            }

        }
        private static void DrawGroundProjectionSkyboxSettings(Material mat, SkyboxManager skyboxManager)
        {
            GroundProjectionSkyboxSettings groundProjectionSkyboxSettings = SkyboxManager.groundProjectionSkyboxSettings;

            if (groundProjectionSkyboxSettings != null)
            {

                Slider("Rotation Z", groundProjectionSkyboxSettings.rotationZ, -180f, 180f, "N0", rotationZ => { groundProjectionSkyboxSettings.rotationZ = rotationZ; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                GUILayout.Space(10);
                Label("WHITE BALANCE", "", true);
                SliderColorTemp("Temperature", groundProjectionSkyboxSettings.temperature, -100f, 100f, "N0", temperature => { groundProjectionSkyboxSettings.temperature = temperature; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                SliderColorTint("Tint", groundProjectionSkyboxSettings.tint, -100f, 100f, "N0", tint => { groundProjectionSkyboxSettings.tint = tint; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                GUILayout.Space(10);
                Label("COLOR", "", true);
                SliderColorVib("Vibrance", groundProjectionSkyboxSettings.vibrance, -1f, 1f, "N2", vibrance => { groundProjectionSkyboxSettings.vibrance = vibrance; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                Slider("Perceptual", groundProjectionSkyboxSettings.perceptual, 0f, 1f, "N2", perceptual => { groundProjectionSkyboxSettings.perceptual = perceptual; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                Slider("Hue", groundProjectionSkyboxSettings.hue, -180f, 180f, "N0", hue => { groundProjectionSkyboxSettings.hue = hue; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                GUILayout.Space(10);
                Label("COLOR BALANCE", "", true);
                Slider("Offset", groundProjectionSkyboxSettings.offset, 0f, 1f, "N2", offset => { groundProjectionSkyboxSettings.offset = offset; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                Slider("Power", groundProjectionSkyboxSettings.power, 0f, 2f, "N2", power => { groundProjectionSkyboxSettings.power = power; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                Slider("Slope", groundProjectionSkyboxSettings.slope, 0f, 2f, "N2", slope => { groundProjectionSkyboxSettings.slope = slope; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                GUILayout.Space(10);
                Toggle("GROUND PROJECTION", groundProjectionSkyboxSettings.projection, true, proj => { groundProjectionSkyboxSettings.projection = proj; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                GUI.enabled = false;
                if (groundProjectionSkyboxSettings.projection)
                {
                    GUI.enabled = true;
                }
                Slider("Horizon", groundProjectionSkyboxSettings.horizon, -1, 1, "N2", horizon => { groundProjectionSkyboxSettings.horizon = horizon; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                Slider("Scale", groundProjectionSkyboxSettings.scale, -50f, 50f, "N0", scale => { groundProjectionSkyboxSettings.scale = scale; SkyboxManager.UpdateGroundProjectionSkySettings(); });
                GUI.enabled = true;
            }
        }

    }

}