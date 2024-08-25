using Graphics.Settings;
using Graphics.Textures;
using System.Linq;
using UnityEngine;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class LightingInspector
    {
        private const float ExposureMin = 0f;
        private const float ExposureMax = 8f;
        //private const float ExposureDefault = 1f;

        private const float RotationMin = 0f;
        private const float RotationMax = 360f;
        //private const float RotationDefault = 0f;

        private static Vector2 cubeMapScrollView;
        //private static int selectedCubeMapIdx = -1;

        private static Vector2 dynSkyScrollView;
        //private static float scrollViewHeight = Inspector.Height * 0.6f;

        internal static void Draw(LightingSettings lightingSettings, SkyboxManager skyboxManager, LightManager lightmanager, bool showAdvanced)
        {
            GUIStyle BoxPadding = new GUIStyle(GUIStyles.tabcontent);
            BoxPadding.padding = new RectOffset(25, 25, 0, 0);
            BoxPadding.margin = new RectOffset(0, 0, 0, 5);
            //BoxPadding.normal.background = null;

            GUIStyle EmptyBox = new GUIStyle(GUIStyles.tabcontent);
            EmptyBox.padding = new RectOffset(0, 0, 0, 0);
            EmptyBox.margin = new RectOffset(0, 0, 0, 0);
            EmptyBox.normal.background = null;

            GUIStyle EmptyBox2 = new GUIStyle(GUIStyles.tabcontent);
            EmptyBox2.padding = new RectOffset(0, 0, 0, 0);
            EmptyBox2.margin = new RectOffset(0, 0, 0, 0);
            //EmptyBox.normal.background = null;

            GUIStyle EmptyBox3 = new GUIStyle(GUIStyles.tabcontent);
            EmptyBox3.padding = new RectOffset(25, 25, 0, 0);
            EmptyBox3.margin = new RectOffset(0, 0, 0, 0);
            EmptyBox3.normal.background = null;

            GUIStyle SelBox = new GUIStyle(GUI.skin.box);
            SelBox.padding = new RectOffset(0, 0, 0, 0);
            SelBox.margin = new RectOffset(2, 2, 2, 2);
            SelBox.normal.background = null;
            //SelBox.fixedHeight = 350;

            GUIStyle SmallBox = new GUIStyle(GUIStyles.tabcontent);
            SmallBox.padding = new RectOffset(25, 25, 0, 0);
            SmallBox.margin = new RectOffset(0, 0, 0, 0);
            SmallBox.normal.background = null;


            GUILayout.BeginVertical(EmptyBox);
            {
                GUILayout.BeginVertical(BoxPadding);
                {
                    GUILayout.Space(35);
                    Label("CUBEMAP SKYBOX", "", true);
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

                GUILayout.Space(10);
                if (showAdvanced)
                {
                    //DrawDynSkyboxOptions(lightingSettings, skyboxManager, showAdvanced);
                    Label("Skybox Material", lightingSettings?.SkyboxSetting?.name ?? "");
                    //Label("Sun Source", lightingSettings?.SunSetting?.name ?? "");
                    GUILayout.Space(10);
                }
                GUILayout.Space(15);
                Label("ENVIRONMENT LIGHTING", "", true);
                GUILayout.Space(1);
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
                        if (skybox.shader.name == "Skybox/Cubemap Ground Projection")
                            Toggle("Ground Projection", skyboxManager.Projection, false, proj => { skyboxManager.Projection = proj; skyboxManager.Update = true; });

                        if (skyboxManager.Projection)
                        {
                            Slider("Horizon", skyboxManager.Horizon, -1f, 1f, "N2", horizon => { skyboxManager.Horizon = horizon; skyboxManager.Update = true; });
                            Slider("Scale", skyboxManager.Scale, -50f, 50f, "N2", scale => { skyboxManager.Scale = scale; skyboxManager.Update = true; });
                        }

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
                bool isSkyboxHemisphere = shaderName.Contains("SkyboxPlus/Gradients");
                bool isSkyboxFourColors = shaderName.Contains("SkyboxPlus/Hemisphere");
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
                else if (isSkyboxHemisphere)
                {
                    DrawHemisphereSkyboxSettings(mat, skyboxManager);
                }
                else if (isSkyboxFourColors)
                {
                    DrawFourColorsSkyboxSettings(mat, skyboxManager);
                }
                else if (isSkyboxAIOSky)
                {
                    DrawAIOSkyboxSettings(lightManager, mat, skyboxManager);
                }
                //else if (isSkyboxGroundProjection)
                //{
                //    DrawGroundProjectionSkyboxSettings(mat, skyboxManager);
                //}
            }
        }
        private static void DrawProceduralSkyboxSettings(LightManager lightManager, Material mat, SkyboxManager skyboxManager)
        {
            Label("Unity Skybox Settings", "", true);
            LightSelector(lightManager, "Sun Source", RenderSettings.sun, source => RenderSettings.sun = source);
            Selection("Sun", (ProceduralSkyboxSettings.SunDisk)mat.GetInt(SkyboxID._SunDisk), quality => { mat.SetInt(SkyboxID._SunDisk, (int)quality); skyboxManager.Update = true; });
            Slider("Sun Size", mat.GetFloat(SkyboxID._SunSize), 0, 1, "N2", value => { mat.SetFloat(SkyboxID._SunSize, value); skyboxManager.Update = true; });
            Slider("Sun Size Convergence", mat.GetFloat(SkyboxID._SunSizeConvergence), 1, 10, "N2", value => { mat.SetFloat(SkyboxID._SunSizeConvergence, value); skyboxManager.Update = true; });
            Slider("Atmosphere Thickness", mat.GetFloat(SkyboxID._AtmosphereThickness), 0, 5, "N2", value => { mat.SetFloat(SkyboxID._AtmosphereThickness, value); skyboxManager.Update = true; });
            SliderColor("Sky Tint", mat.GetColor(SkyboxID._SkyTint), c => { mat.SetColor(SkyboxID._SkyTint, c); skyboxManager.Update = true; }, true);
            SliderColor("Ground Color", mat.GetColor(SkyboxID._GroundColor), c => { mat.SetColor(SkyboxID._GroundColor, c); skyboxManager.Update = true; }, true);
            Slider("Exposure", mat.GetFloat(SkyboxID._Exposure), 0, 8, "N2", value => { mat.SetFloat(SkyboxID._Exposure, value); skyboxManager.Update = true; });
        }
        private static void DrawTwoColorsSkyboxSettings(Material mat, SkyboxManager skyboxManager)
        {
            Label("Two Point Color Skybox Settings", "", true);
            SliderColor("Colour A", mat.GetColor(SkyboxID._ColorA), c => { mat.SetColor(SkyboxID._ColorA, c); skyboxManager.Update = true; }, true);
            Slider("Intensity A", mat.GetFloat(SkyboxID._IntensityA), 0, 2, "N2", intensity => { mat.SetFloat(SkyboxID._IntensityA, intensity); skyboxManager.Update = true; });
            Dimension("Box Size", mat.GetVector(SkyboxID._DirA), direction => { mat.SetVector(SkyboxID._DirA, direction); skyboxManager.Update = true; });
            SliderColor("Colour B", mat.GetColor(SkyboxID._ColorB), c => { mat.SetColor(SkyboxID._ColorB, c); skyboxManager.Update = true; }, true);
            Slider("Intensity B", mat.GetFloat(SkyboxID._IntensityB), 0, 2, "N2", intensity => { mat.SetFloat(SkyboxID._IntensityB, intensity); skyboxManager.Update = true; });
            Dimension("Box Size", mat.GetVector(SkyboxID._DirB), direction => { mat.SetVector(SkyboxID._DirB, direction); skyboxManager.Update = true; });
        }
        private static void DrawHemisphereSkyboxSettings(Material mat, SkyboxManager skyboxManager)
        {
            Label("Hemisphere Skybox Settings", "", true);
            SliderColor("North Pole", mat.GetColor(SkyboxID._TopColor), c => { mat.SetColor(SkyboxID._TopColor, c); skyboxManager.Update = true; }, true);
            SliderColor("Equator", mat.GetColor(SkyboxID._MiddleColor), c => { mat.SetColor(SkyboxID._MiddleColor, c); skyboxManager.Update = true; }, true);
            SliderColor("South Pole", mat.GetColor(SkyboxID._BottomColor), c => { mat.SetColor(SkyboxID._BottomColor, c); skyboxManager.Update = true; }, true);
        }
        private static void DrawFourColorsSkyboxSettings(Material mat, SkyboxManager skyboxManager)
        {
            Label("Four Point Gradient Skybox Settings", "", true);
            SliderColor("Color1", mat.GetColor(SkyboxID._Color1), c => { mat.SetColor(SkyboxID._Color1, c); skyboxManager.Update = true; }, true);
            SliderColor("Color2", mat.GetColor(SkyboxID._Color2), c => { mat.SetColor(SkyboxID._Color2, c); skyboxManager.Update = true; }, true);
            SliderColor("Color3", mat.GetColor(SkyboxID._Color3), c => { mat.SetColor(SkyboxID._Color3, c); skyboxManager.Update = true; }, true);
            SliderColor("Color4", mat.GetColor(SkyboxID._Color4), c => { mat.SetColor(SkyboxID._Color4, c); skyboxManager.Update = true; }, true);
            Dimension("Direction1", mat.GetVector(SkyboxID._Direction1), direction => { mat.SetVector(SkyboxID._Direction1, direction); skyboxManager.Update = true; });
            Dimension("Direction2", mat.GetVector(SkyboxID._Direction2), direction => { mat.SetVector(SkyboxID._Direction2, direction); skyboxManager.Update = true; });
            Dimension("Direction3", mat.GetVector(SkyboxID._Direction3), direction => { mat.SetVector(SkyboxID._Direction3, direction); skyboxManager.Update = true; });
            Dimension("Direction4", mat.GetVector(SkyboxID._Direction4), direction => { mat.SetVector(SkyboxID._Direction4, direction); skyboxManager.Update = true; });
            Slider("Exponent1", mat.GetFloat(SkyboxID._Exponent1), 0, 2, "N1", intensity => { mat.SetFloat(SkyboxID._Exponent1, intensity); skyboxManager.Update = true; });
            Slider("Exponent2", mat.GetFloat(SkyboxID._Exponent2), 0, 2, "N1", intensity => { mat.SetFloat(SkyboxID._Exponent2, intensity); skyboxManager.Update = true; });
            Slider("Exponent3", mat.GetFloat(SkyboxID._Exponent3), 0, 2, "N1", intensity => { mat.SetFloat(SkyboxID._Exponent3, intensity); skyboxManager.Update = true; });
            Slider("Exponent4", mat.GetFloat(SkyboxID._Exponent4), 0, 2, "N1", intensity => { mat.SetFloat(SkyboxID._Exponent4, intensity); skyboxManager.Update = true; });
        }
        private static void DrawAIOSkyboxSettings(LightManager lightManager, Material mat, SkyboxManager skyboxManager)
        {
            GUILayout.Space(10);
            Label("SUN", "", true);
            GUILayout.Space(2);
            LightSelector(lightManager, "Sun Source", RenderSettings.sun, source => RenderSettings.sun = source);
            GUILayout.Space(15);
            SliderColor("Sun Color", mat.GetColor(SkyboxID._sunColor), sun => { mat.SetColor(SkyboxID._sunColor, sun); skyboxManager.Update = true; }, true);
            Slider("Sun Min", mat.GetFloat(SkyboxID._sunMin), 0f, 0.02f, "N5", sunmin => { mat.SetFloat(SkyboxID._sunMin, sunmin); skyboxManager.Update = true; });
            Slider("Sun Max", mat.GetFloat(SkyboxID._sunMax), 0.9f, 1.0f, "N5", sunmax => { mat.SetFloat(SkyboxID._sunMax, sunmax); skyboxManager.Update = true; });
            Slider("Sun Glow", mat.GetFloat(SkyboxID._SunGlow), 0f, 0.2f, "N2", sunglow => { mat.SetFloat(SkyboxID._SunGlow, sunglow); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("SKY", "", true);
            GUILayout.Space(2);
            Slider("Atmosphere Start", mat.GetFloat(SkyboxID._AtmosphereStart), -0.2f, 0.2f, "N2", atmospherestart => { mat.SetFloat(SkyboxID._AtmosphereStart, atmospherestart); skyboxManager.Update = true; });
            Slider("Atmosphere End", mat.GetFloat(SkyboxID._AtmosphereEnd), 0f, 1f, "N2", atmosphereend => { mat.SetFloat(SkyboxID._AtmosphereEnd, atmosphereend); skyboxManager.Update = true; });
            GUILayout.Space(10);
            SliderColor("Day Sky", mat.GetColor(SkyboxID._DaySky), daysky => { mat.SetColor(SkyboxID._DaySky, daysky); skyboxManager.Update = true; }, true);
            GUILayout.Space(10);
            SliderColor("Day Atmosphere", mat.GetColor(SkyboxID._DayAtmosphere), dayatmosphere => { mat.SetColor(SkyboxID._DayAtmosphere, dayatmosphere); skyboxManager.Update = true; }, true);
            Slider("Day Range", mat.GetFloat(SkyboxID._DayRange), -1f, 1f, "N2", dayrange => { mat.SetFloat(SkyboxID._DayRange, dayrange); skyboxManager.Update = true; });
            GUILayout.Space(10);
            SliderColor("Sunset Sky", mat.GetColor(SkyboxID._SunSetSky), sunsetsky => { mat.SetColor(SkyboxID._SunSetSky, sunsetsky); skyboxManager.Update = true; }, true);
            GUILayout.Space(10);
            Slider("Sunset Range", mat.GetFloat(SkyboxID._setRange), -1f, 1f, "N2", sunsetrange => { mat.SetFloat(SkyboxID._setRange, sunsetrange); skyboxManager.Update = true; });
            GUILayout.Space(10);
            SliderColor("Night Sky", mat.GetColor(SkyboxID._NightSky), nightsky => { mat.SetColor(SkyboxID._NightSky, nightsky); skyboxManager.Update = true; }, true);
            GUILayout.Space(10);
            SliderColor("Night Atmosphere", mat.GetColor(SkyboxID._NightAtmosphere), nightatmosphere => { mat.SetColor(SkyboxID._NightAtmosphere, nightatmosphere); skyboxManager.Update = true; }, true);
            Slider("Night Range", mat.GetFloat(SkyboxID._nightRange), -1f, 1f, "N2", nightrange => { mat.SetFloat(SkyboxID._nightRange, nightrange); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("MAIN CLOUDS", "", true);
            GUILayout.Space(2);
            Slider("Clouds Density Sky Edge", mat.GetFloat(SkyboxID._CloudsDensitySkyEdge), 0f, 1.5f, "N2", cloudsdensityskyedge => { mat.SetFloat(SkyboxID._CloudsDensitySkyEdge, cloudsdensityskyedge); skyboxManager.Update = true; });
            Slider("Clouds Density", mat.GetFloat(SkyboxID._CloudsDensity), -1f, 1f, "N2", cloudsDensity => { mat.SetFloat(SkyboxID._CloudsDensity, cloudsDensity); skyboxManager.Update = true; });
            Slider("Clouds Thickness", mat.GetFloat(SkyboxID._CloudsThickness), 0.0001f, 0.3f, "N2", cloudsThickness => { mat.SetFloat(SkyboxID._CloudsThickness, cloudsThickness); skyboxManager.Update = true; });
            Slider("Dome Curved", mat.GetFloat(SkyboxID._DomeCurved), -2f, 2f, "N2", domeCurved => { mat.SetFloat(SkyboxID._DomeCurved, domeCurved); skyboxManager.Update = true; });
            Slider("Clouds Scale", mat.GetFloat(SkyboxID._CloudsScale), 0f, 5f, "N2", cloudsScale => { mat.SetFloat(SkyboxID._CloudsScale, cloudsScale); skyboxManager.Update = true; });
            Slider("Detail Scale 01", mat.GetFloat(SkyboxID._DetailScale01), 0f, 2f, "N2", detailScale01 => { mat.SetFloat(SkyboxID._DetailScale01, detailScale01); skyboxManager.Update = true; });
            Slider("Detail Scale 02", mat.GetFloat(SkyboxID._DetailScale02), 0f, 2f, "N2", detailScale02 => { mat.SetFloat(SkyboxID._DetailScale02, detailScale02); skyboxManager.Update = true; });
            Slider("Detail Scale 03", mat.GetFloat(SkyboxID._DetailScale03), 0f, 2f, "N2", detailScale03 => { mat.SetFloat(SkyboxID._DetailScale03, detailScale03); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("DAY CLOUDS", "", true);
            GUILayout.Space(2);
            Slider("Sun Light Power", mat.GetFloat(SkyboxID._SunLightPower), 0f, 10f, "N2", sunLightPower => { mat.SetFloat(SkyboxID._SunLightPower, sunLightPower); skyboxManager.Update = true; });
            GUILayout.Space(10);
            SliderColor("Day Transmission Color", mat.GetColor(SkyboxID._DayTransmissionColor), dayTransmissionColor => { mat.SetColor(SkyboxID._DayTransmissionColor, dayTransmissionColor); skyboxManager.Update = true; }, true);
            Slider("Day Clouds Transmission", mat.GetFloat(SkyboxID._DayCloudsTransmission), 0f, 5f, "N2", dayCloudsTransmission => { mat.SetFloat(SkyboxID._DayCloudsTransmission, dayCloudsTransmission); skyboxManager.Update = true; });
            Slider("AmbientLight", mat.GetFloat(SkyboxID._AmbientLight), 0f, 1f, "N2", ambientl => { mat.SetFloat(SkyboxID._AmbientLight, ambientl); skyboxManager.Update = true; });
            Slider("Clouds Brightness", mat.GetFloat(SkyboxID._CloudsBrightness), -1f, 1f, "N2", cloudsBrightness => { mat.SetFloat(SkyboxID._CloudsBrightness, cloudsBrightness); skyboxManager.Update = true; });
            Slider("Clouds Contract", mat.GetFloat(SkyboxID._CloudsContract), 0f, 1f, "N2", cloudsContract => { mat.SetFloat(SkyboxID._CloudsContract, cloudsContract); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("NIGHT CLOUDS", "", true);
            GUILayout.Space(2);
            SliderColor("Moon Light", mat.GetColor(SkyboxID._MoonLight), moonLight => { mat.SetColor(SkyboxID._MoonLight, moonLight); skyboxManager.Update = true; }, true);
            Slider("Moon Light Power", mat.GetFloat(SkyboxID._MoonLightPower), 0f, 10f, "N2", moonLightPower => { mat.SetFloat(SkyboxID._MoonLightPower, moonLightPower); skyboxManager.Update = true; });
            GUILayout.Space(10);
            SliderColor("Night Transmission Color", mat.GetColor(SkyboxID._NightTransmissionColor), nightTransmissionColor => { mat.SetColor(SkyboxID._NightTransmissionColor, nightTransmissionColor); skyboxManager.Update = true; }, true);
            Slider("Night Clouds Transmission", mat.GetFloat(SkyboxID._NightCloudsTransmission), 0f, 5f, "N2", nightCloudsTransmission => { mat.SetFloat(SkyboxID._NightCloudsTransmission, nightCloudsTransmission); skyboxManager.Update = true; });
            Slider("Night Ambient Light", mat.GetFloat(SkyboxID._NightAmbientLight), 0f, 1f, "N2", nightAmbientLight => { mat.SetFloat(SkyboxID._NightAmbientLight, nightAmbientLight); skyboxManager.Update = true; });
            Slider("Night Clouds Brightness", mat.GetFloat(SkyboxID._NightCloudsBrightness), -1f, 1f, "N2", nightCloudsBrightness => { mat.SetFloat(SkyboxID._NightCloudsBrightness, nightCloudsBrightness); skyboxManager.Update = true; });
            Slider("Night Clouds Contrast", mat.GetFloat(SkyboxID._NightCloudsContract), 0f, 1f, "N2", nightCloudsContract => { mat.SetFloat(SkyboxID._NightCloudsContract, nightCloudsContract); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("Additional Clouds Color", "", true);
            GUILayout.Space(2);
            Slider("Clouds Shadow Weight", mat.GetFloat(SkyboxID._CloudsShadowWeight), 0f, 1f, "N2", cloudsShadowWeight => { mat.SetFloat(SkyboxID._CloudsShadowWeight, cloudsShadowWeight); skyboxManager.Update = true; });
            Slider("Sun Z Offset", mat.GetFloat(SkyboxID._SunZoffset), -0.5f, 0.5f, "N2", sunZoffset => { mat.SetFloat(SkyboxID._SunZoffset, sunZoffset); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("CLOUDS ANIMATION", "", true);
            GUILayout.Space(2);
            Dimension("Clouds Pan", (Vector2)mat.GetVector(SkyboxID._CloudsPan), cloudpan => { mat.SetVector(SkyboxID._CloudsPan, cloudpan); skyboxManager.Update = true; });
            Slider("Clouds Rotation", mat.GetFloat(SkyboxID._CloudsRotation), 0f, 7f, "N2", cloudsRotation => { mat.SetFloat(SkyboxID._CloudsRotation, cloudsRotation); skyboxManager.Update = true; });
            Slider("Time Scale", mat.GetFloat(SkyboxID._timeScale), 0f, 0.05f, "N2", timeScale => { mat.SetFloat(SkyboxID._timeScale, timeScale); skyboxManager.Update = true; });
            Slider("Distortion Time", mat.GetFloat(SkyboxID._DistortionTime), 0f, 0.05f, "N2", distortionTime => { mat.SetFloat(SkyboxID._DistortionTime, distortionTime); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("STAR FIELD", "", true);
            GUILayout.Space(2);
            Slider("BG Scale", mat.GetFloat(SkyboxID._BGscale), 0f, 0.5f, "N2", bgScale => { mat.SetFloat(SkyboxID._BGscale, bgScale); skyboxManager.Update = true; });
            Slider("BG Rotate", mat.GetFloat(SkyboxID._BGRotate), -7f, 7f, "N2", bgRotate => { mat.SetFloat(SkyboxID._BGRotate, bgRotate); skyboxManager.Update = true; });
            Slider("Moon Scale", mat.GetFloat(SkyboxID._moonScale), 1f, 40f, "N2", moonScale => { mat.SetFloat(SkyboxID._moonScale, moonScale); skyboxManager.Update = true; });
            Dimension("Moon Position", (Vector3)mat.GetVector(SkyboxID._MoonPosition), moonpos => { mat.SetVector(SkyboxID._MoonPosition, moonpos); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("FOG", "", true);
            GUILayout.Space(2);
            SliderColor("Fog Color", mat.GetColor(SkyboxID._FogColor), fogColor => { mat.SetColor(SkyboxID._FogColor, fogColor); skyboxManager.Update = true; }, true);
            Slider("Fog Level", mat.GetFloat(SkyboxID._FogLevel), 0f, 1f, "N2", fogLevel => { mat.SetFloat(SkyboxID._FogLevel, fogLevel); skyboxManager.Update = true; });
            GUILayout.Space(10);
            Label("GROUND", "", true);
            GUILayout.Space(2);
            SliderColor("Ground Color", mat.GetColor(SkyboxID._GroundColor), groundColor => { mat.SetColor(SkyboxID._GroundColor, groundColor); skyboxManager.Update = true; }, true);
            Slider("Ground Level", mat.GetFloat(SkyboxID._GroundLevel), -0.5f, 0.85f, "N2", groundLevel => { mat.SetFloat(SkyboxID._GroundLevel, groundLevel); skyboxManager.Update = true; });
        }

        private static void DrawGroundProjectionSkyboxSettings(Material mat, SkyboxManager skyboxManager)
        {

            Toggle("Ground Projection", mat.IsKeywordEnabled("_GROUNDPROJECTION"), false, isOn =>
            {
                if (isOn)
                {
                    mat.EnableKeyword("_GROUNDPROJECTION");
                }
                else
                {
                    mat.DisableKeyword("_GROUNDPROJECTION");
                }
                skyboxManager.Update = true;
            });
            Slider("Horizon", mat.GetFloat(SkyboxID._Horizon), -1, 1, "N2", horizon => { mat.SetFloat(SkyboxID._Horizon, horizon); skyboxManager.Update = true; });
            Slider("Scale", mat.GetFloat(SkyboxID._Scale), -50f, 50f, "N2", scale => { mat.SetFloat(SkyboxID._Scale, scale); skyboxManager.Update = true; });

        }
    }
}