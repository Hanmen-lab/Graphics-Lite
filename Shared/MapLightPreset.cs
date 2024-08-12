using Graphics.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Graphics
{
    // TODO: Find better way to save the data... maybe builder? idk...
    [MessagePackObject(keyAsPropertyName: true)]
    public struct MapLightPreset
    {

        public ReflectionProbeSettings[] reflectionProbes;
        public PerLightSettings[] lights;

        public void Save(string targetPath, bool overwrite = true)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            UpdateParameters();
            byte[] bytes = Serialize();
            if (File.Exists(targetPath) && overwrite)
            {
                File.Delete(targetPath);
                File.WriteAllBytes(targetPath, bytes);
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(targetPath), "debug-light.json"), MessagePackSerializer.ToJson(this));
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
                    Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, string.Format("Couldn't open map light preset file '{0}' at {1}", name + ".light", targetPath));
                    Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Error, e.Message + "\n" + e.StackTrace);
                    return false;
                }
            }
            else
            {
                Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Debug, string.Format("No presaved map light preset file '{0}' at {1}", name + ".light", targetPath));
                return false;
            }
        }

        public byte[] Serialize()
        {
            return MessagePackSerializer.Serialize(this);
        }

        public void UpdateParameters()
        {
            reflectionProbes = SceneController.BuildReflectionProbeSettings();
            lights = SceneController.BuildLightSettings();
        }

        public void ApplyParameters()
        {
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Loading Reflection Probes.");
#endif
            if (reflectionProbes != null && reflectionProbes.Length > 0)
                SceneController.ApplyReflectionProbeSettings(reflectionProbes);
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Loaded Light Settings.");
#endif
            if (lights != null && lights.Length > 0)
                SceneController.ApplyLightSettings(lights);
        }       

        public void Load(byte[] bytes)
        {
            Deserialize(bytes);
#if DEBUG
            Graphics.Instance.Log.LogInfo($"Loaded map preset, applying.");
#endif
            ApplyParameters();
        }

        public void Deserialize(byte[] bytes)
        {
            this = MessagePackSerializer.Deserialize<MapLightPreset>(bytes);
        }

    }
}
