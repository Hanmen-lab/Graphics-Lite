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
using static Graphics.DebugUtils;


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
        public GroundProjectionSkyboxSettings groundProjectionSkybox;
        public AIOSkySettings aiosky;
        public ProceduralSkyboxSettings proceduralsky;
        public AuraSettings aura;
        public FilmGrainSettings filmGrain;
        public DeferredDecalsSettings deferredDecals;

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
            this.filmGrain = FilmGrainManager.settings;
            this.aura = AuraManager.settings;
            this.deferredDecals = DecalsSystemManager.settings;

            // Skybox setting is generated when preset is being saved.
            skyboxSetting = null;
            this.aiosky = SkyboxManager.dynAIOSkySetting;
            this.hemispheresky = SkyboxManager.dynHemisphereGradientSettings;
            this.fourpointsky = SkyboxManager.dynFourPointGradientSettings;
            this.twopointsky = SkyboxManager.dynTwoPointGradientSettings;
            this.proceduralsky = SkyboxManager.dynProceduralSkySettings;
            this.groundProjectionSkybox = SkyboxManager.groundProjectionSkyboxSettings;
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
            filmGrain = FilmGrainManager.settings;
            SkyboxManager manager = Graphics.Instance.SkyboxManager;
            hemispheresky = SkyboxManager.dynHemisphereGradientSettings;
            aiosky = SkyboxManager.dynAIOSkySetting;
            fourpointsky = SkyboxManager.dynFourPointGradientSettings;
            twopointsky = SkyboxManager.dynTwoPointGradientSettings;
            proceduralsky = SkyboxManager.dynProceduralSkySettings;
            groundProjectionSkybox = SkyboxManager.groundProjectionSkyboxSettings;
            aura = AuraManager.settings;
            deferredDecals = DecalsSystemManager.settings;


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
                    LoadPreset(bytes, name);
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

        public bool LoadDefaultPreset(string targetPath, string name)
        {
            if (File.Exists(targetPath))
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(targetPath);
                    LoadScenePreset(bytes);
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

        public void LoadPreset(byte[] bytes, string name)
        {
            LogWithDotsWarning("LOADING USER PRESET", name);
            PresetManager presetManager = Graphics.Instance.PresetManager;
            Deserialize(bytes);
            ApplyParameters(presetManager.loadSkybox, presetManager.loadSEGI, presetManager.loadSSS, presetManager.loadShadows,
                presetManager.loadLoadAura, presetManager.loadVolumetrics, presetManager.loadLuxwater, presetManager.loadHeightFog, presetManager.loadDoF, presetManager.loadRain, name);
            LogWithDotsMessage("USER PRESET", name);
            //Graphics.Instance.Log.LogMessage($"Preset: {name}");
        }

        public void LoadScenePreset(byte[] bytes)
        {
            LogWithDotsWarning("LOADING PRESET", "SCENE");
            Deserialize(bytes);
            ApplyParameters(true, true, true, true, true, true, true, true, true, true, "Scene Preset");

        }

        public void Deserialize(byte[] bytes)
        {
            this = MessagePackSerializer.Deserialize<Preset>(bytes);
        }

        public void ApplyParameters(bool loadskybox, bool loadsegi, bool loadsss, bool loadshadows,
            bool loadAura, bool LoadVolumetrics, bool loadluxwater, bool loadfog, bool loaddof, bool loadrain, string name)
        {
            pp.LoadParameters(loaddof);

            //ContactShadowsManager.settings = contactshadows;
            //ContactShadowsManager.UpdateSettings();
            //LogWithDots("Contact Shadows", "OK");

            if (loadsss)
            {
                SSSManager.settings = sss;
                SSSManager.UpdateSettings();
                LogWithDots("SSS", "OK");
            }
            else
            {
                LogWithDots("SSS", "SKIP");
            }

            //SSSManager.skinSettings = skin;
            //SSSManager.UpdateGlobalSettings();
            //LogWithDots("Global Skin Settings", "OK");

            if (loadsegi)
            {
                SEGIManager.settings = segi;
                SEGIManager.UpdateSettings();
                LogWithDots("SEGI", "OK");

            }
            else
                LogWithDots("SEGI", "SKIP");

            GTAOManager.settings = gtao;
            GTAOManager.UpdateSettings();
            LogWithDots("GTAO", "OK");

            //if (loadshadows)
            //{
            //    LightManager.ngsssettings = ngss;
            //    LightManager.UpdateNGSSSettings();
            //    LogWithDots("Next-Gen Soft Shadows", "OK");
            //    //FrustumShadowsManager.settings = frustumShadows;
            //    //FrustumShadowsManager.UpdateSettings();
            //    //LogWithDots("Frustum Shadows", "OK");
            //}
            //else
            //{
            //    LogWithDots("Next-Gen Soft Shadows", "SKIP");
            //    LogWithDots("Frustum Shadows", "SKIP");
            //}

            if (loadfog)
            {
                GlobalFogManager.settings = fog;
                GlobalFogManager.UpdateSettings();
                LogWithDots("Global Fog", "OK");
            }
            else
            {
                LogWithDots("Global Fog", "SKIP");
            }

            if (loadluxwater)
            {
                LuxWater_UnderWaterRenderingManager.settings = underwater;
                LuxWater_UnderWaterRenderingManager.UpdateSettings();
                LuxWater_WaterVolumeTriggerManager.settings = trigger;
                LuxWater_WaterVolumeTriggerManager.UpdateSettings();
                ConnectSunToUnderwaterManager.settings = connectSun;
                ConnectSunToUnderwaterManager.UpdateSettings();
                LogWithDots("LuxWater", "OK");
            }
            else
            {
                LogWithDots("LuxWater", "SKIP");
            }

            VAOManager.settings = vao;
            VAOManager.UpdateSettings();
            LogWithDots("Volumetic Ambient Occlusion", "OK");

            AmplifyOccManager.settings = amplifyocc;
            AmplifyOccManager.UpdateSettings();
            LogWithDots("Amplify Occlusion", "OK");

            //ShinySSRRManager.settings = shinyssrr;
            //ShinySSRRManager.UpdateSettings();
            //LogWithDots("Shiny SSRR", "OK");

            if (ctaa == null)
                ctaa = new CTAASettings();
            CTAAManager.settings = ctaa;
            CTAAManager.UpdateSettings();
            LogWithDots("CTAA", "OK");

            if (loaddof)
            {
                FocusManager.settings = focus;
                FocusManager.UpdateSettings();
                LogWithDots("DoF Focus", "OK");
            }
            else
                LogWithDots("DoF Focus", "SKIP");

            //FrostFXManager.settings = frostfx;
            //FrostFXManager.UpdateSettings();
            //LogWithDots("FrostFX", "OK");

            if (loadAura)
            {
                AuraManager.settings = aura;
                AuraManager.UpdateSettings();
                LogWithDots("Aura 2 Volumetrics", "OK");
            }
            else
            {
                LogWithDots("Aura 2 Volumetrics", "SKIP");
            }

            //if (LoadVolumetrics)
            //{
            //    VolumetricLightManager.settings = volumetric;
            //    VolumetricLightManager.UpdateSettings();
            //    LogWithDots("Volumetric Lights", "OK");
            //}
            //else
            //{
            //    LogWithDots("Volumetric Lights", "SKIP");
            //}

            DecalsSystemManager.settings = deferredDecals;
            DecalsSystemManager.UpdateSettings();
            LogWithDots("Deferred Decals", "OK");

            FilmGrainManager.settings = filmGrain;
            FilmGrainManager.UpdateSettings();
            LogWithDots("Film Grain", "OK");

            if (loadskybox)
            {
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
                        else if (skyboxSetting is GroundProjectionSkyboxSettings && groundProjectionSkybox == null) SkyboxManager.groundProjectionSkyboxSettings = skyboxSetting as GroundProjectionSkyboxSettings;
                    }

                    SkyboxManager.dynAIOSkySetting = aiosky;
                    SkyboxManager.dynHemisphereGradientSettings = hemispheresky;
                    SkyboxManager.dynFourPointGradientSettings = fourpointsky;
                    SkyboxManager.dynTwoPointGradientSettings = twopointsky;
                    SkyboxManager.dynProceduralSkySettings = proceduralsky;
                    SkyboxManager.groundProjectionSkyboxSettings = groundProjectionSkybox;

                    manager.skyboxParams = skybox;
                    manager.PresetUpdate = true;
                    manager.LoadSkyboxParams();
                    manager.SetupDefaultReflectionProbe(Graphics.Instance.LightingSettings);
                }

                //LogWithDots("Skybox");
            }
            else
            {
                //camera = new CameraSettings();
                Graphics.Instance.CameraSettings.ClearFlag = CameraSettings.AICameraClearFlags.Skybox;
                LogWithDots("Skybox", "SKIP");
            }

            Graphics.Instance.LightingSettings.DefaultReflectionProbeSettings = lights.DefaultReflectionProbeSettings;
            LogWithDots("Reflection Probes", "OK");

        }


        //void LogAndUpdateManager<T>(Manager<T> manager, T settings, string managerName)
        //{
        //    manager.settings = settings;
        //    manager.UpdateSettings();

        //    Graphics.Instance.Log.LogInfo($"Done with {managerName}");

        //}

    }
}