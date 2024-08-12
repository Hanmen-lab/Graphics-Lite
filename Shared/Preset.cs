using Graphics.CTAA;
using Graphics.AmplifyOcclusion;
using Graphics.GTAO;
using Graphics.SEGI;
using Graphics.GlobalFog;
using Graphics.VAO;
//using Graphics.AmplifyBloom;
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
        public PostProcessingSettings pp;
        public SSSSettings sss;
        public SkyboxParams skybox;
        public SkyboxSettings skyboxSetting;
        public GTAOSettings gtao;
        public CTAASettings ctaa;
        public GlobalFogSettings fog;
        //public ShinySSRRSettings shinyssrr;
        public VAOSettings vao;
        //public AmplifyBloomSettings amplifybloom;
        public AmplifyOccSettings amplifyocc;
        public SEGISettings segi;
        //public UnderWaterRenderingSettings underwater;
        //public WaterVolumeTriggerSettings trigger;
        //public ConnectSunToUnderwaterSettings connectSun;
        public DitheredShadowsSettings ditheredShadows;

        public Preset(GlobalSettings global, CameraSettings camera, LightingSettings lights, PostProcessingSettings pp, SkyboxParams skybox, SSSSettings sss)
        {
            this.camera = camera;
            this.global = global;
            this.lights = lights;
            this.pp = pp;
            this.skybox = skybox;
            this.sss = sss;
            this.segi = SEGIManager.settings;
            this.gtao = GTAOManager.settings;
            this.vao = VAOManager.settings;
            this.ctaa = CTAAManager.settings;
            this.fog = GlobalFogManager.settings;
            //this.shinyssrr = ShinySSRRManager.settings;
            this.vao = VAOManager.settings;

            this.amplifyocc = AmplifyOccManager.settings;
            //this.underwater = LuxWater_UnderWaterRenderingManager.settings;
            //this.trigger = new WaterVolumeTriggerSettings();
            //this.connectSun = new ConnectSunToUnderwaterSettings();
            this.ditheredShadows = new DitheredShadowsSettings();

            // Skybox setting is generated when preset is being saved.
            skyboxSetting = null;
        }

        public void UpdateParameters()
        {
            pp.SaveParameters();
            sss?.SaveParameters();
            segi = SEGIManager.settings;
            gtao = GTAOManager.settings;
            ctaa = CTAAManager.settings;
            fog = GlobalFogManager.settings;
            //shinyssrr = ShinySSRRManager.settings;
            vao = VAOManager.settings;
            //amplifybloom = AmplifyBloomManager.settings;
            amplifyocc = AmplifyOccManager.settings;
            //underwater = LuxWater_UnderWaterRenderingManager.settings;
            //trigger = LuxWater_WaterVolumeTriggerManager.settings;
            //connectSun = ConnectSunToUnderwaterManager.settings;
            ditheredShadows = DitheredShadowsManager.settings;
            SkyboxManager manager = Graphics.Instance.SkyboxManager;

            Material mat = manager.Skybox;
            if (mat)
            {
                SkyboxSettings setting = null;

                // Generate Setting Class
                // TODO: Find better way...
                // TODO: Add EnviroSky Support (AI)
                // TODO: Add AIOSky Support (HS2)
                // TODO: Stronger exception handling for different games.
                if (mat.shader.name == ProceduralSkyboxSettings.shaderName) setting = new ProceduralSkyboxSettings();
                else if (mat.shader.name == TwoPointColorSkyboxSettings.shaderName) setting = new TwoPointColorSkyboxSettings();
                else if (mat.shader.name == FourPointGradientSkyboxSetting.shaderName) setting = new FourPointGradientSkyboxSetting();
                else if (mat.shader.name == HemisphereGradientSkyboxSetting.shaderName) setting = new HemisphereGradientSkyboxSetting();
                else if (mat.shader.name == AIOSkySettings.shaderName) setting = new AIOSkySettings();

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
            Graphics.Instance.Log.LogInfo($"Done with PP");
#endif
            sss?.LoadParameters();
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
            Graphics.Instance.Log.LogInfo($"Done with Unity Standard Effects");
#endif

            //LuxWater_UnderWaterRenderingManager.settings = underwater;
            //LuxWater_UnderWaterRenderingManager.UpdateSettings();

            //LuxWater_WaterVolumeTriggerManager.settings = trigger;
            //LuxWater_WaterVolumeTriggerManager.UpdateSettings();

            //ConnectSunToUnderwaterManager.settings = connectSun;
            //ConnectSunToUnderwaterManager.UpdateSettings();

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
//            ShinySSRRManager.settings = shinyssrr;
//            ShinySSRRManager.UpdateSettings();
//#if DEBUG
//            Graphics.Instance.Log.LogInfo($"Done with ShinySSRR");
//#endif

            if (ctaa == null)
                ctaa = new CTAASettings();
            CTAAManager.settings = ctaa;
            CTAAManager.UpdateSettings();
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Done with CTAA");
#endif           

            SkyboxManager manager = Graphics.Instance.SkyboxManager;
            if (manager)
            {
                if (skyboxSetting != null)
                    SkyboxManager.dynSkyboxSetting = skyboxSetting;
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