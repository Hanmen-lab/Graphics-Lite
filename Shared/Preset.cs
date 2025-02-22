using Graphics.CTAA;
using Graphics.AmplifyOcclusion;
using Graphics.GTAO;
using Graphics.SEGI;
using Graphics.GlobalFog;
using Graphics.VAO;
using Graphics.Settings;
using Graphics.Textures;
using MessagePack;
using System;
using System.IO;
using UnityEngine;

namespace Graphics
{
    // TODO: Find better way to save the data... maybe builder? idk...
    [MessagePackObject(keyAsPropertyName: true)]
    public struct Preset
    {
        public GlobalSettings global;
        public CameraSettings camera;
        public LightingSettings lights;
        public SkyboxParams skybox;
        public SkyboxSettings skyboxSetting;
        public PostProcessingSettings pp;
        public CTAASettings ctaa;
        public SSSSettings sss;
        public SEGISettings segi;
        public VAOSettings vao;
        public GTAOSettings gtao;
        public AmplifyOccSettings amplifyocc;
        public GlobalFogSettings fog;
        public DitheredShadowsSettings ditheredShadows;
        public UnderWaterRenderingSettings underwater;
        public WaterVolumeTriggerSettings trigger;
        public ConnectSunToUnderwaterSettings connectSun;
        public FocusSettings focus;
        public TwoPointColorSkyboxSettings twopointsky;
        public FourPointGradientSkyboxSetting fourpointsky;
        public HemisphereGradientSkyboxSetting hemispheresky;
        public AIOSkySettings aiosky;
        public ProceduralSkyboxSettings proceduralsky;
        public AuraSettings aura;

        public Preset(GlobalSettings global, CameraSettings camera, LightingSettings lights, PostProcessingSettings pp, SkyboxParams skybox)
        {
            this.camera = camera;
            this.global = global;
            this.lights = lights;
            this.pp = pp;
            this.skybox = skybox;
            this.ctaa = CTAAManager.settings;
            this.sss = SSSManager.settings;
            this.segi = SEGIManager.settings;
            this.gtao = GTAOManager.settings;
            this.vao = VAOManager.settings;
            this.ctaa = CTAAManager.settings;
            this.fog = GlobalFogManager.settings;
            this.vao = VAOManager.settings;
            this.amplifyocc = AmplifyOccManager.settings;
            this.ditheredShadows = new DitheredShadowsSettings();
            this.underwater = LuxWater_UnderWaterRenderingManager.settings;
            this.trigger = new WaterVolumeTriggerSettings();
            this.connectSun = new ConnectSunToUnderwaterSettings();
            this.focus = FocusManager.settings;

            // Skybox setting is generated when preset is being saved.
            skyboxSetting = null;
            this.aiosky = SkyboxManager.dynAIOSkySetting;
            this.hemispheresky = SkyboxManager.dynHemisphereGradientSettings;
            this.fourpointsky = SkyboxManager.dynFourPointGradientSettings;
            this.twopointsky = SkyboxManager.dynTwoPointGradientSettings;
            this.proceduralsky = SkyboxManager.dynProceduralSkySettings;
            this.aura = AuraManager.settings;
        }

        public void UpdateParameters()
        {
            pp.SaveParameters();
            sss = SSSManager.settings;
            segi = SEGIManager.settings;
            gtao = GTAOManager.settings;
            ctaa = CTAAManager.settings;
            fog = GlobalFogManager.settings;
            vao = VAOManager.settings;
            amplifyocc = AmplifyOccManager.settings;
            ditheredShadows = DitheredShadowsManager.settings;
            underwater = LuxWater_UnderWaterRenderingManager.settings;
            trigger = LuxWater_WaterVolumeTriggerManager.settings;
            connectSun = ConnectSunToUnderwaterManager.settings;
            focus = FocusManager.settings;
            SkyboxManager manager = Graphics.Instance.SkyboxManager;
            hemispheresky = SkyboxManager.dynHemisphereGradientSettings;
            aiosky = SkyboxManager.dynAIOSkySetting;
            fourpointsky = SkyboxManager.dynFourPointGradientSettings;
            twopointsky = SkyboxManager.dynTwoPointGradientSettings;
            proceduralsky = SkyboxManager.dynProceduralSkySettings;
            aura = AuraManager.settings;

            Material mat = manager.Skybox;
            if (mat)
            {
                SkyboxSettings setting = null;

                // Generate Setting Class
                // TODO: Find better way...
                // TODO: Add EnviroSky Support (AI)
                // TODO: Add AIOSky Support (HS2)
                // TODO: Stronger exception handling for different games.
                //if (mat.shader.name == ProceduralSkyboxSettings.shaderName) setting = new ProceduralSkyboxSettings();
                //else if (mat.shader.name == TwoPointColorSkyboxSettings.shaderName) setting = new TwoPointColorSkyboxSettings();
                //else if (mat.shader.name == FourPointGradientSkyboxSetting.shaderName) setting = new FourPointGradientSkyboxSetting();
                //else if (mat.shader.name == HemisphereGradientSkyboxSetting.shaderName) setting = new HemisphereGradientSkyboxSetting();
                //else if (mat.shader.name == AIOSkySettings.shaderName) setting = new AIOSkySettings();

                if (setting != null)
                {
                    setting.Save();
                    skyboxSetting = setting;
                }

            }
            ReflectionProbe defaultProbe = manager.DefaultReflectionProbe();
            if (defaultProbe != null && defaultProbe.intensity > 0)
            {
                lights.DefaultReflectionProbeSettings = new ReflectionProbeSettings();
                lights.DefaultReflectionProbeSettings.FillSettings(manager.DefaultReflectionProbe());
            } 
            else
            {
                lights.DefaultReflectionProbeSettings = null;
            }
            skybox = manager.skyboxParams;

        }
        public byte[] Serialize()
        {
            return MessagePackSerializer.Serialize(this);
        }
        public void Save(string targetPath, bool overwrite = true)
        {          
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            UpdateParameters();
            byte[] bytes = Serialize();
            if (File.Exists(targetPath) && overwrite)
            {
                File.Delete(targetPath);
                File.WriteAllBytes(targetPath, bytes);
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(targetPath), "debug.json"), MessagePackSerializer.ToJson(this));
            }
            else
            {
                File.WriteAllBytes(targetPath, bytes);
            }
        }
        public bool Load(string targetPath, string name)
        {
           if (File.Exists(targetPath))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(targetPath);
                    Load(bytes);
                    return true;
                }
                catch (Exception e)
                {
                    Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, string.Format("Couldn't open preset file '{0}' at {1}", name + ".preset", targetPath));
                    Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, e.Message + "\n" + e.StackTrace);
                    return false;
                }
            }
            else
            {
                Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, string.Format("Couldn't find preset file '{0}' at {1}", name + ".preset", targetPath));
                return false;
            }
        }

        public void Load(byte[] bytes)
        {
            Deserialize(bytes);
            ApplyParameters();
        }

        public void Deserialize(byte[] bytes)
        {
            this = MessagePackSerializer.Deserialize<Preset>(bytes);
        }

        public void ApplyParameters()
        {
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Applying Parameters");
#endif
            pp.LoadParameters();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with Post Processing Stack");
#endif
            SSSManager.settings = sss;
            SSSManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with SSS");
#endif
            SEGIManager.settings = segi;
            SEGIManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with SEGI");
#endif
            GTAOManager.settings = gtao;
            GTAOManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with GTAO");
#endif
            DitheredShadowsManager.settings = ditheredShadows;
            DitheredShadowsManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with Dithered Shadows");
#endif

            GlobalFogManager.settings = fog;
            GlobalFogManager.UpdateSettings();

#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with Global Fog");
#endif
            LuxWater_UnderWaterRenderingManager.settings = underwater;
            LuxWater_UnderWaterRenderingManager.UpdateSettings();

            LuxWater_WaterVolumeTriggerManager.settings = trigger;
            LuxWater_WaterVolumeTriggerManager.UpdateSettings();

            ConnectSunToUnderwaterManager.settings = connectSun;
            ConnectSunToUnderwaterManager.UpdateSettings();

#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with LuxWater");
#endif
            VAOManager.settings = vao;
            VAOManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with VAO");
#endif

            AmplifyOccManager.settings = amplifyocc;
            AmplifyOccManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with Amplify Occlusion");
#endif
            if (ctaa == null)
                ctaa = new CTAASettings();
            CTAAManager.settings = ctaa;
            CTAAManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with CTAA");
#endif
            FocusManager.settings = focus;
            FocusManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with FocusPuller");
#endif
            AuraManager.settings = aura;
            AuraManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with Aura 2");
#endif
            SkyboxManager manager = Graphics.Instance.SkyboxManager;
            if (manager)
            {
                // Foward-port skybox settings
                if (skyboxSetting != null)
                {
                    if (skyboxSetting is AIOSkySettings && aiosky == null) SkyboxManager.dynAIOSkySetting = skyboxSetting as AIOSkySettings;
                    else if (skyboxSetting is HemisphereGradientSkyboxSetting && hemispheresky == null) SkyboxManager.dynHemisphereGradientSettings = skyboxSetting as HemisphereGradientSkyboxSetting;
                    else if (skyboxSetting is FourPointGradientSkyboxSetting && fourpointsky == null) SkyboxManager.dynFourPointGradientSettings = skyboxSetting as FourPointGradientSkyboxSetting;
                    else if (skyboxSetting is TwoPointColorSkyboxSettings && twopointsky == null) SkyboxManager.dynTwoPointGradientSettings = skyboxSetting as TwoPointColorSkyboxSettings;
                    else if (skyboxSetting is ProceduralSkyboxSettings && proceduralsky == null) SkyboxManager.dynProceduralSkySettings = skyboxSetting as ProceduralSkyboxSettings;
                }

                SkyboxManager.dynAIOSkySetting = aiosky;
                SkyboxManager.dynHemisphereGradientSettings = hemispheresky;
                SkyboxManager.dynFourPointGradientSettings = fourpointsky;
                SkyboxManager.dynTwoPointGradientSettings = twopointsky;
                SkyboxManager.dynProceduralSkySettings = proceduralsky;

                manager.skyboxParams = skybox;
                manager.PresetUpdate = true;
                manager.LoadSkyboxParams();
                manager.SetupDefaultReflectionProbe(Graphics.Instance.LightingSettings);
            }

#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with skybox");
#endif
            Graphics.Instance.LightingSettings.DefaultReflectionProbeSettings = lights.DefaultReflectionProbeSettings;
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with Default RP");
#endif
        }
    }
}
