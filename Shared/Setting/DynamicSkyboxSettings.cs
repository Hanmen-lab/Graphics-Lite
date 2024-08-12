using Graphics.Textures;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Graphics.Settings
{
    [Union(0, typeof(ProceduralSkyboxSettings))]
    [Union(1, typeof(TwoPointColorSkyboxSettings))]
    [Union(2, typeof(FourPointGradientSkyboxSetting))]
    [Union(3, typeof(HemisphereGradientSkyboxSetting))]
    [Union(4, typeof(AIOSkySettings))]
    [Union(5, typeof(GroundProjectionSkyboxSettings))]

    public abstract class SkyboxSettings
    {
        virtual public void Save() { }
        virtual public void Load() { }
    }
    [MessagePackObject(keyAsPropertyName: true)]
    public static class SkyboxID
    {
        public static readonly int _Horizon = Shader.PropertyToID("_Horizon");
        public static readonly int _Scale = Shader.PropertyToID("_Scale");
        public static readonly int _Projection = Shader.PropertyToID("_Projectione");


        // Procedural Skybox
        public static readonly int _SunDisk = Shader.PropertyToID("_SunDisk");
        public static readonly int _SunSize = Shader.PropertyToID("_SunSize");
        public static readonly int _SunSizeConvergence = Shader.PropertyToID("_SunSizeConvergence");
        public static readonly int _AtmosphereThickness = Shader.PropertyToID("_AtmosphereThickness");
        public static readonly int _SkyTint = Shader.PropertyToID("_SkyTint");
        public static readonly int _GroundColor = Shader.PropertyToID("_GroundColor");
        public static readonly int _Exposure = Shader.PropertyToID("_Exposure");
        //Two Point Color Skybox

        public static readonly int _IntensityA = Shader.PropertyToID("_IntensityA");
        public static readonly int _IntensityB = Shader.PropertyToID("_IntensityB");
        public static readonly int _ColorA = Shader.PropertyToID("_ColorA");
        public static readonly int _ColorB = Shader.PropertyToID("_ColorB");
        public static readonly int _DirA = Shader.PropertyToID("_DirA");
        public static readonly int _DirB = Shader.PropertyToID("_DirB");
        //Four Point Gradient Skybox

        public static readonly int _Color1 = Shader.PropertyToID("_Color1");
        public static readonly int _Color2 = Shader.PropertyToID("_Color2");
        public static readonly int _Color3 = Shader.PropertyToID("_Color3");
        public static readonly int _Color4 = Shader.PropertyToID("_Color4");
        public static readonly int _Direction1 = Shader.PropertyToID("_Direction1");
        public static readonly int _Direction2 = Shader.PropertyToID("_Direction2");
        public static readonly int _Direction3 = Shader.PropertyToID("_Direction3");
        public static readonly int _Direction4 = Shader.PropertyToID("_Direction4");
        public static readonly int _Exponent1 = Shader.PropertyToID("_Exponent1");
        public static readonly int _Exponent2 = Shader.PropertyToID("_Exponent2");
        public static readonly int _Exponent3 = Shader.PropertyToID("_Exponent3");
        public static readonly int _Exponent4 = Shader.PropertyToID("_Exponent4");

        //Hemisphere Gradient Skybox
        public static readonly int _TopColor = Shader.PropertyToID("_TopColor");
        public static readonly int _MiddleColor = Shader.PropertyToID("_MiddleColor");
        public static readonly int _BottomColor = Shader.PropertyToID("_BottomColor");

        //AIO Skybox
        public static readonly int _sunColor = Shader.PropertyToID("_sunColor");
        public static readonly int _sunMin = Shader.PropertyToID("_sunMin");
        public static readonly int _sunMax = Shader.PropertyToID("_sunMax");
        public static readonly int _SunGlow = Shader.PropertyToID("_SunGlow");
        public static readonly int _AtmosphereStart = Shader.PropertyToID("_AtmosphereStart");
        public static readonly int _AtmosphereEnd = Shader.PropertyToID("_AtmosphereEnd");
        public static readonly int _DaySky = Shader.PropertyToID("_DaySky");
        public static readonly int _DayRange = Shader.PropertyToID("_DayRange");
        public static readonly int _DayAtmosphere = Shader.PropertyToID("_DayAtmosphere");
        public static readonly int _SunSetSky = Shader.PropertyToID("_SunSetSky");
        public static readonly int _setRange = Shader.PropertyToID("_setRange");
        public static readonly int _NightSky = Shader.PropertyToID("_NightSky");
        public static readonly int _nightRange = Shader.PropertyToID("_nightRange");
        public static readonly int _NightAtmosphere = Shader.PropertyToID("_NightAtmosphere");
        public static readonly int _CloudsDensity = Shader.PropertyToID("_CloudsDensity");
        public static readonly int _CloudsDensitySkyEdge = Shader.PropertyToID("_CloudsDensitySkyEdge");
        public static readonly int _CloudsThickness = Shader.PropertyToID("_CloudsThickness");
        public static readonly int _DomeCurved = Shader.PropertyToID("_DomeCurved");
        public static readonly int _CloudsScale = Shader.PropertyToID("_CloudsScale");
        public static readonly int _DetailScale01 = Shader.PropertyToID("_DetailScale01");
        public static readonly int _DetailScale02 = Shader.PropertyToID("_DetailScale02");
        public static readonly int _DetailScale03 = Shader.PropertyToID("_DetailScale03");
        public static readonly int _SunLightPower = Shader.PropertyToID("_SunLightPower");
        public static readonly int _DayTransmissionColor = Shader.PropertyToID("_DayTransmissionColor");
        public static readonly int _DayCloudsTransmission = Shader.PropertyToID("_DayCloudsTransmission");
        public static readonly int _AmbientLight = Shader.PropertyToID("_AmbientLight");
        public static readonly int _CloudsBrightness = Shader.PropertyToID("_CloudsBrightness");
        public static readonly int _CloudsContract = Shader.PropertyToID("_CloudsContract");
        public static readonly int _MoonLight = Shader.PropertyToID("_MoonLight");
        public static readonly int _MoonLightPower = Shader.PropertyToID("_MoonLightPower");
        public static readonly int _NightTransmissionColor = Shader.PropertyToID("_NightTransmissionColor");
        public static readonly int _NightCloudsTransmission = Shader.PropertyToID("_NightCloudsTransmission");
        public static readonly int _NightAmbientLight = Shader.PropertyToID("_NightAmbientLight");
        public static readonly int _NightCloudsBrightness = Shader.PropertyToID("_NightCloudsBrightness");
        public static readonly int _NightCloudsContract = Shader.PropertyToID("_NightCloudsContract");
        public static readonly int _CloudsShadowWeight = Shader.PropertyToID("_CloudsShadowWeight");
        public static readonly int _SunZoffset = Shader.PropertyToID("_SunZoffset");
        public static readonly int _CloudsPan = Shader.PropertyToID("_CloudsPan");
        public static readonly int _CloudsRotation = Shader.PropertyToID("_CloudsRotation");
        public static readonly int _timeScale = Shader.PropertyToID("_timeScale");
        public static readonly int _DistortionTime = Shader.PropertyToID("_DistortionTime");
        public static readonly int _BGscale = Shader.PropertyToID("_BGscale");
        public static readonly int _BGRotate = Shader.PropertyToID("_BGRotate");
        public static readonly int _moonScale = Shader.PropertyToID("_moonScale");
        public static readonly int _MoonPosition = Shader.PropertyToID("_MoonPosition");
        public static readonly int _FogColor = Shader.PropertyToID("_FogColor");
        public static readonly int _FogLevel = Shader.PropertyToID("_FogLevel");
        public static readonly int _GroundLevel = Shader.PropertyToID("_GroundLevel");

    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class GroundProjectionSkyboxSettings : SkyboxSettings
    {
        [IgnoreMember]
        public static readonly string shaderName = "Skybox/Cubemap Ground Projection";

        //public bool projection;
        public float horizon;
        public float scale;

        public override void Save()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                //projection = mat.IsKeywordEnabled("_GROUNDPROJECTION_ON");
                horizon = mat.GetFloat(SkyboxID._Horizon);
                scale = mat.GetFloat(SkyboxID._Scale);

            }
        }
        public override void Load()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                //if (projection)
                //{
                //    mat.EnableKeyword("_GROUNDPROJECTION_ON");
                //}
                //else
                //{
                //    mat.DisableKeyword("_GROUNDPROJECTION_ON");
                //}
                mat.SetFloat(SkyboxID._Horizon, horizon);
                mat.SetFloat(SkyboxID._Scale, scale);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class ProceduralSkyboxSettings : SkyboxSettings
    {
        [IgnoreMember]
        public static readonly string shaderName = "Skybox/Procedural";

        public enum SunDisk
        {
            None,
            Simple,
            HighQuality,
        }

        public SunDisk sunDisk;
        public float sunSize;
        public float sunsizeConvergence;
        public float atmosphereThickness;
        public Color skyTint;
        public Color groundTint;
        public float exposure;

        public override void Save()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                sunDisk = (ProceduralSkyboxSettings.SunDisk)mat.GetInt(SkyboxID._SunDisk);
                sunSize = mat.GetFloat(SkyboxID._SunSize);
                sunsizeConvergence = mat.GetFloat(SkyboxID._SunSizeConvergence);
                atmosphereThickness = mat.GetFloat(SkyboxID._AtmosphereThickness);
                skyTint = mat.GetColor(SkyboxID._SkyTint);
                groundTint = mat.GetColor(SkyboxID._GroundColor);
                exposure = mat.GetFloat(SkyboxID._Exposure);
            }
        }
        public override void Load()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                mat.SetInt(SkyboxID._SunDisk, (int)sunDisk);
                mat.SetFloat(SkyboxID._SunSize, sunSize);
                mat.SetFloat(SkyboxID._SunSizeConvergence, sunsizeConvergence);
                mat.SetFloat(SkyboxID._AtmosphereThickness, atmosphereThickness);
                mat.SetColor(SkyboxID._SkyTint, skyTint);
                mat.SetColor(SkyboxID._GroundColor, groundTint);
                mat.SetFloat(SkyboxID._Exposure, exposure);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class TwoPointColorSkyboxSettings : SkyboxSettings
    {
        public float intensityA;
        public float intensityB;
        public Color colorA = new Color();
        public Color colorB = new Color();
        public Vector4 directionA = new Vector4();
        public Vector4 directionB = new Vector4();

        [IgnoreMember]
        public static readonly string shaderName = "SkyBox/Simple Two Colors";
        public override void Save()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                intensityA = mat.GetFloat(SkyboxID._IntensityA);
                intensityB = mat.GetFloat(SkyboxID._IntensityB);
                colorA = mat.GetColor(SkyboxID._ColorA);
                colorB = mat.GetColor(SkyboxID._ColorB);
                directionA = mat.GetVector(SkyboxID._DirA);
                directionB = mat.GetVector(SkyboxID._DirB);
            }
        }
        public override void Load()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                mat.SetFloat(SkyboxID._IntensityA, intensityA);
                mat.SetFloat(SkyboxID._IntensityB, intensityB);
                mat.SetColor(SkyboxID._ColorA, colorA);
                mat.SetColor(SkyboxID._ColorB, colorB);
                mat.SetVector(SkyboxID._DirA, directionA);
                mat.SetVector(SkyboxID._DirB, directionB);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class FourPointGradientSkyboxSetting : SkyboxSettings
    {
        public Color colorA = new Color();
        public Color colorB = new Color();
        public Color colorC = new Color();
        public Color colorD = new Color();
        public Vector3 directionA = new Vector3();
        public Vector3 directionB = new Vector3();
        public Vector3 directionC = new Vector3();
        public Vector3 directionD = new Vector3();
        public float exponentA;
        public float exponentB;
        public float exponentC;
        public float exponentD;

        [IgnoreMember]
        public static readonly string shaderName = "SkyboxPlus/Gradients";
        public override void Save()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                colorA = mat.GetColor(SkyboxID._Color1);
                colorB = mat.GetColor(SkyboxID._Color2);
                colorC = mat.GetColor(SkyboxID._Color3);
                colorD = mat.GetColor(SkyboxID._Color4);
                directionA = mat.GetVector(SkyboxID._Direction1);
                directionB = mat.GetVector(SkyboxID._Direction2);
                directionC = mat.GetVector(SkyboxID._Direction3);
                directionD = mat.GetVector(SkyboxID._Direction4);
                exponentA = mat.GetFloat(SkyboxID._Exponent1);
                exponentB = mat.GetFloat(SkyboxID._Exponent2);
                exponentC = mat.GetFloat(SkyboxID._Exponent3);
                exponentD = mat.GetFloat(SkyboxID._Exponent4);
            }
        }
        public override void Load()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                mat.SetColor(SkyboxID._Color1, colorA);
                mat.SetColor(SkyboxID._Color2, colorB);
                mat.SetColor(SkyboxID._Color3, colorC);
                mat.SetColor(SkyboxID._Color4, colorD);
                mat.SetVector(SkyboxID._Direction1, directionA);
                mat.SetVector(SkyboxID._Direction2, directionB);
                mat.SetVector(SkyboxID._Direction3, directionC);
                mat.SetVector(SkyboxID._Direction4, directionD);
                mat.SetFloat(SkyboxID._Exponent1, exponentA);
                mat.SetFloat(SkyboxID._Exponent2, exponentB);
                mat.SetFloat(SkyboxID._Exponent3, exponentC);
                mat.SetFloat(SkyboxID._Exponent4, exponentD);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class HemisphereGradientSkyboxSetting : SkyboxSettings
    {
        public Color colorA = new Color();
        public Color colorB = new Color();
        public Color colorC = new Color();

        [IgnoreMember]
        public static readonly string shaderName = "SkyboxPlus/Hemisphere";

        public override void Save()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                colorA = mat.GetColor(SkyboxID._TopColor);
                colorB = mat.GetColor(SkyboxID._MiddleColor);
                colorC = mat.GetColor(SkyboxID._BottomColor);
            }
        }
        public override void Load()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                mat.SetColor(SkyboxID._TopColor, colorA);
                mat.SetColor(SkyboxID._MiddleColor, colorB);
                mat.SetColor(SkyboxID._BottomColor, colorC);
            }
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class AIOSkySettings : SkyboxSettings
    {
        [IgnoreMember]
        public static readonly string shaderName = "AIOsky/V2";

        public Color sunColor = new Color();
        public float sunMin;
        public float sunMax;
        public float SunGlow;
        public float AtmosphereStart;
        public float AtmosphereEnd;
        public Color DaySky = new Color();
        public float DayRange;
        public Color DayAtmosphere;
        public Color SunSetSky;
        public float setRange;
        public Color NightSky;
        public float nightRange;
        public Color NightAtmosphere;
        public float CloudsDensity;
        public float CloudsDensitySkyEdge;
        public float CloudsThickness;
        public float DomeCurved;
        public float CloudsScale;
        public float DetailScale01;
        public float DetailScale02;
        public float DetailScale03;
        public float SunLightPower;
        public Color DayTransmissionColor;
        public float DayCloudsTransmission;
        public float AmbientLight;
        public float CloudsBrightness;
        public float CloudsContract;
        public Color MoonLight;
        public float MoonLightPower;
        public Color NightTransmissionColor;
        public float NightCloudsTransmission;
        public float NightAmbientLight;
        public float NightCloudsBrightness;
        public float NightCloudsContract;
        public float CloudsShadowWeight;
        public float SunZoffset;
        public float CloudsRotation;
        public Vector2 CloudsPan;
        public float timeScale;
        public float DistortionTime;
        public float BGscale;
        public float BGRotate;
        public float moonScale;
        public Vector3 MoonPosition;
        public Color FogColor;
        public float FogLevel;
        public Color GroundColor;
        public float GroundLevel;

        public override void Save()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                sunColor = mat.GetColor(SkyboxID._sunColor);
                sunMin = mat.GetFloat(SkyboxID._sunMin);
                sunMax = mat.GetFloat(SkyboxID._sunMax);
                SunGlow = mat.GetFloat(SkyboxID._SunGlow);
                AtmosphereStart = mat.GetFloat(SkyboxID._AtmosphereStart);
                AtmosphereEnd = mat.GetFloat(SkyboxID._AtmosphereEnd);
                DaySky = mat.GetColor(SkyboxID._DaySky);
                DayRange = mat.GetFloat(SkyboxID._DayRange);
                DayAtmosphere = mat.GetColor(SkyboxID._DayAtmosphere);
                SunSetSky = mat.GetColor(SkyboxID._SunSetSky);
                setRange = mat.GetFloat(SkyboxID._setRange);
                NightSky = mat.GetColor(SkyboxID._NightSky);
                nightRange = mat.GetFloat(SkyboxID._nightRange);
                NightAtmosphere = mat.GetColor(SkyboxID._NightAtmosphere);
                CloudsDensity = mat.GetFloat(SkyboxID._CloudsDensity);
                CloudsDensitySkyEdge = mat.GetFloat(SkyboxID._CloudsDensitySkyEdge);
                CloudsThickness = mat.GetFloat(SkyboxID._CloudsThickness);
                DomeCurved = mat.GetFloat(SkyboxID._DomeCurved);
                CloudsScale = mat.GetFloat(SkyboxID._CloudsScale);
                DetailScale01 = mat.GetFloat(SkyboxID._DetailScale01);
                DetailScale02 = mat.GetFloat(SkyboxID._DetailScale02);
                DetailScale03 = mat.GetFloat(SkyboxID._DetailScale03);
                SunLightPower = mat.GetFloat(SkyboxID._SunLightPower);
                DayTransmissionColor = mat.GetColor(SkyboxID._DayTransmissionColor);
                DayCloudsTransmission = mat.GetFloat(SkyboxID._DayCloudsTransmission);
                AmbientLight = mat.GetFloat(SkyboxID._AmbientLight);
                CloudsBrightness = mat.GetFloat(SkyboxID._CloudsBrightness);
                CloudsContract = mat.GetFloat(SkyboxID._CloudsContract);
                MoonLight = mat.GetColor(SkyboxID._MoonLight);
                MoonLightPower = mat.GetFloat(SkyboxID._MoonLightPower);
                NightTransmissionColor = mat.GetColor(SkyboxID._NightTransmissionColor);
                NightCloudsTransmission = mat.GetFloat(SkyboxID._NightCloudsTransmission);
                NightAmbientLight = mat.GetFloat(SkyboxID._NightAmbientLight);
                NightCloudsBrightness = mat.GetFloat(SkyboxID._NightCloudsBrightness);
                NightCloudsContract = mat.GetFloat(SkyboxID._NightCloudsContract);
                CloudsShadowWeight = mat.GetFloat(SkyboxID._CloudsShadowWeight);
                SunZoffset = mat.GetFloat(SkyboxID._SunZoffset);
                CloudsRotation = mat.GetFloat(SkyboxID._CloudsRotation);
                timeScale = mat.GetFloat(SkyboxID._timeScale);
                DistortionTime = mat.GetFloat(SkyboxID._DistortionTime);
                BGscale = mat.GetFloat(SkyboxID._BGscale);
                BGRotate = mat.GetFloat(SkyboxID._BGRotate);
                moonScale = mat.GetFloat(SkyboxID._moonScale);
                MoonPosition = mat.GetVector(SkyboxID._MoonPosition);
                FogColor = mat.GetColor(SkyboxID._FogColor);
                FogLevel = mat.GetFloat(SkyboxID._FogLevel);
                GroundColor = mat.GetColor(SkyboxID._GroundColor);
                GroundLevel = mat.GetFloat(SkyboxID._GroundLevel);
                CloudsPan = mat.GetVector(SkyboxID._CloudsPan);
            }
        }
        public override void Load()
        {
            Material mat = Graphics.Instance?.SkyboxManager?.Skybox;
            if (mat != null && mat.shader.name == shaderName)
            {
                mat.SetColor(SkyboxID._sunColor, sunColor);
                mat.SetFloat(SkyboxID._sunMin, sunMin);
                mat.SetFloat(SkyboxID._sunMax, sunMax);
                mat.SetFloat(SkyboxID._SunGlow, SunGlow);
                mat.SetFloat(SkyboxID._AtmosphereStart, AtmosphereStart);
                mat.SetFloat(SkyboxID._AtmosphereEnd, AtmosphereEnd);
                mat.SetColor(SkyboxID._DaySky, DaySky);
                mat.SetFloat(SkyboxID._DayRange, DayRange);
                mat.SetColor(SkyboxID._DayAtmosphere, DayAtmosphere);
                mat.SetColor(SkyboxID._SunSetSky, SunSetSky);
                mat.SetFloat(SkyboxID._setRange, setRange);
                mat.SetColor(SkyboxID._NightSky, NightSky);
                mat.SetFloat(SkyboxID._nightRange, nightRange);
                mat.SetColor(SkyboxID._NightAtmosphere, NightAtmosphere);
                mat.SetFloat(SkyboxID._CloudsDensity, CloudsDensity);
                mat.SetFloat(SkyboxID._CloudsDensitySkyEdge, CloudsDensitySkyEdge);
                mat.SetFloat(SkyboxID._CloudsThickness, CloudsThickness);
                mat.SetFloat(SkyboxID._DomeCurved, DomeCurved);
                mat.SetFloat(SkyboxID._CloudsScale, CloudsScale);
                mat.SetFloat(SkyboxID._DetailScale01, DetailScale01);
                mat.SetFloat(SkyboxID._DetailScale02, DetailScale02);
                mat.SetFloat(SkyboxID._DetailScale03, DetailScale03);
                mat.SetFloat(SkyboxID._SunLightPower, SunLightPower);
                mat.SetColor(SkyboxID._DayTransmissionColor, DayTransmissionColor);
                mat.SetFloat(SkyboxID._DayCloudsTransmission, DayCloudsTransmission);
                mat.SetFloat(SkyboxID._AmbientLight, AmbientLight);
                mat.SetFloat(SkyboxID._CloudsBrightness, CloudsBrightness);
                mat.SetFloat(SkyboxID._CloudsContract, CloudsContract);
                mat.SetColor(SkyboxID._MoonLight, MoonLight);
                mat.SetFloat(SkyboxID._MoonLightPower, MoonLightPower);
                mat.SetColor(SkyboxID._NightTransmissionColor, NightTransmissionColor);
                mat.SetFloat(SkyboxID._NightCloudsTransmission, NightCloudsTransmission);
                mat.SetFloat(SkyboxID._NightAmbientLight, NightAmbientLight);
                mat.SetFloat(SkyboxID._NightCloudsBrightness, NightCloudsBrightness);
                mat.SetFloat(SkyboxID._NightCloudsContract, NightCloudsContract);
                mat.SetFloat(SkyboxID._CloudsShadowWeight, CloudsShadowWeight);
                mat.SetFloat(SkyboxID._SunZoffset, SunZoffset);
                mat.SetFloat(SkyboxID._CloudsRotation, CloudsRotation);
                mat.SetFloat(SkyboxID._timeScale, timeScale);
                mat.SetFloat(SkyboxID._DistortionTime, DistortionTime);
                mat.SetFloat(SkyboxID._BGscale, BGscale);
                mat.SetFloat(SkyboxID._BGRotate, BGRotate);
                mat.SetFloat(SkyboxID._moonScale, moonScale);
                mat.SetVector(SkyboxID._MoonPosition, MoonPosition);
                mat.SetColor(SkyboxID._FogColor, FogColor);
                mat.SetFloat(SkyboxID._FogLevel, FogLevel);
                mat.SetColor(SkyboxID._GroundColor, GroundColor);
                mat.SetFloat(SkyboxID._GroundLevel, GroundLevel);
                mat.SetVector(SkyboxID._CloudsPan, CloudsPan);
            }
        }
    }
}
