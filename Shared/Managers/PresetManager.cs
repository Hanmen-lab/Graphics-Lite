using ExtensibleSaveFormat;
using KKAPI.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using static Graphics.DebugUtils;

namespace Graphics
{
    internal class PresetManager
    {
        private readonly string _defaultsPath;
        private readonly string _path;
        private string _currentPresetName;
        private Dictionary<string, (string Path, string Folder)> _presetNameToPath;
        private readonly Graphics _parent;

        internal PresetManager(string presetPath, Graphics parent)
        {
            _path = presetPath;
            _parent = parent;
            _defaultsPath = Path.Combine(Path.GetDirectoryName(Graphics.Instance.Info.Location), "Resources", "defaults");

            LoadPresets();
        }

        private void LoadPresets()
        {
            _presetNameToPath = Util.GetRelativeFileNameToFullPathMap(_path, "*.preset")
                                 .ToDictionary(name => name.Key, name => (name.Value, Path.GetDirectoryName(name.Value)));
        }

        internal void RefreshPresetListManually() => LoadPresets();

        internal (string Path, string Folder) GetPresetInfo(string presetName)
        {
            return _presetNameToPath.TryGetValue(presetName, out var presetInfo) ? presetInfo : (null, null);
        }

        internal string[] PresetNames => _presetNameToPath.Keys.ToArray();

        internal string CurrentPreset
        {
            get => _currentPresetName;
            set
            {
                Load(value);
                _currentPresetName = value;
            }
        }

        internal void Save(string presetName, string folder = "")
        {
            Preset preset = new Preset(_parent.Settings, _parent.CameraSettings, _parent.LightingSettings, _parent.PostProcessingSettings, _parent.SkyboxManager.skyboxParams);
            string path = Graphics.ConfigPresetPath.Value; // Runtime Config Preset Path.

            string targetPath = !string.IsNullOrEmpty(folder)
                ? Path.Combine(path, folder, presetName + ".preset")
                : Path.Combine(path, presetName + ".preset");

            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            preset.Save(targetPath);

            LogWithDotsWarning("PRESET SAVED", presetName);
            RefreshPresetListManually();
        }

        internal void SaveDefault(PresetDefaultType defaultType)
        {
            Preset preset = new Preset(_parent.Settings, _parent.CameraSettings, _parent.LightingSettings, _parent.PostProcessingSettings, _parent.SkyboxManager.skyboxParams/*, _parent.SSSSettings*/);
            string targetPath = Path.Combine(_defaultsPath, defaultType.ToString() + ".default");
            preset.Save(targetPath);
            Graphics.Instance.Log.LogInfo(string.Format("Saved New Default: {0}", defaultType));
        }

        internal PluginData GetExtendedData()
        {
            Preset preset = new Preset(_parent.Settings, _parent.CameraSettings, _parent.LightingSettings, _parent.PostProcessingSettings, _parent.SkyboxManager.skyboxParams/*, _parent.SSSSettings*/);
            PluginData saveData = new PluginData();
            preset.UpdateParameters();
            byte[] presetData = preset.Serialize();
            saveData.data.Add("bytes", presetData);
            saveData.version = 1;
            return saveData;
        }

        internal bool Load(string presetName)
        {
            Preset preset = new Preset(_parent.Settings, _parent.CameraSettings, _parent.LightingSettings, _parent.PostProcessingSettings, _parent.SkyboxManager.skyboxParams);
            presetName = presetName == null ? "default" : presetName;

            var (path, folder) = GetPresetInfo(presetName);

            string targetPath = string.IsNullOrEmpty(folder)
                ? path
                : Path.Combine(folder, path);

            return preset.Load(targetPath, presetName);
        }

        internal class PresetFolder
        {
            public string Name { get; set; }
            public List<string> Presets { get; set; }

            public PresetFolder(string name)
            {
                Name = name;
                Presets = new List<string>();
            }
        }

        internal PresetFolder[] GetPresetStructure()
        {
            var presetFolders = new Dictionary<string, PresetFolder>();

            foreach (var kvp in _presetNameToPath)
            {
                string presetName = kvp.Key;
                string folderPath = kvp.Value.Folder;

                if (!presetFolders.ContainsKey(folderPath))
                {
                    string folderName = Path.GetFileName(folderPath);
                    presetFolders[folderPath] = new PresetFolder(folderName);
                }

                presetFolders[folderPath].Presets.Add(presetName);
            }

            return presetFolders.Values.ToArray();
        }

        #region MapLightPresets
        internal void SaveMapLights(bool original = false)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string mapName = null;

#if AI
#else
            if (Manager.HSceneManager.isHScene)
                mapName = Manager.BaseMap.infoTable[Singleton<Manager.HSceneManager>.Instance.mapID].MapNames[0];
#endif

            MapLightPreset mapLightPreset = new MapLightPreset();
            string path = Graphics.ConfigPresetPath.Value;
            string targetPath = Path.Combine(path, "mapLightPresets", sceneName + (mapName == null ? "" : "-" + mapName) + (original ? "-original" : "") + ".light");
            mapLightPreset.Save(targetPath);
        }

        internal bool MapLightOriginalExists()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string mapName = null;

#if AI
#else
            if (Manager.HSceneManager.isHScene)
                mapName = Manager.BaseMap.infoTable[Singleton<Manager.HSceneManager>.Instance.mapID].MapNames[0];
#endif

            string presetName = sceneName + (mapName == null ? "" : "-" + mapName) + "-original";
            string targetPath = Path.Combine(_path, "mapLightPresets", presetName + ".light");
            return File.Exists(targetPath);
        }

        internal bool LoadMapLights(bool original = false)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string mapName = null;

#if AI
#else
            if (Manager.HSceneManager.isHScene)
                mapName = Manager.BaseMap.infoTable[Singleton<Manager.HSceneManager>.Instance.mapID].MapNames[0];
#endif

            MapLightPreset mapLightPreset = new MapLightPreset();
            string presetName = sceneName + (mapName == null ? "" : "-" + mapName) + (original ? "-original" : "");
            string targetPath = Path.Combine(_path, "mapLightPresets", presetName + ".light");
            return mapLightPreset.Load(targetPath, presetName);
        }

        internal bool LoadMapLightsDefault(bool original = false)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            string mapName = null;

#if AI
#else
            if (Manager.HSceneManager.isHScene)
                mapName = Manager.BaseMap.infoTable[Singleton<Manager.HSceneManager>.Instance.mapID].MapNames[0];
#endif

            MapLightPreset mapLightPreset = new MapLightPreset();
            string presetName = sceneName + (mapName == null ? "" : "-" + mapName) + (original ? "-original" : "");
            string targetPath = Path.Combine(_path, "mapLightPresets", presetName + ".light");

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            Graphics.Instance.Log.LogMessage(string.Format("Deleted Map Light Preset: {0}. Restart the game.", presetName));

            return true;
        }

        #endregion
        internal bool LoadDefault(PresetDefaultType defaultType)
        {
            Preset preset = new Preset(_parent.Settings, _parent.CameraSettings, _parent.LightingSettings, _parent.PostProcessingSettings, _parent.SkyboxManager.skyboxParams/*, _parent.SSSSettings*/);
            string presetName = defaultType.ToString();
            string targetPath = Path.Combine(_defaultsPath, presetName + ".default");
            if (!preset.LoadDefaultPreset(targetPath, presetName))
            {
                // Failed to load the default...try falling back to defaults
                return RestoreDefault(defaultType);
            }
            else
            {
                //Graphics.Instance.Log.LogInfo(string.Format("Loaded Default: {0}", defaultType));
                return true;
            }
        }

        internal bool LoadDefaultForCurrentGameMode()
        {
            return LoadDefault(PresetDefaultTypeUtils.ForGameMode(KKAPI.KoikatuAPI.GetCurrentGameMode()));
        }

        internal bool RestoreDefault(PresetDefaultType defaultType)
        {
            Preset preset = new Preset(_parent.Settings, _parent.CameraSettings, _parent.LightingSettings, _parent.PostProcessingSettings, _parent.SkyboxManager.skyboxParams/*, _parent.SSSSettings*/);
            string presetName = defaultType.ToString();
            //string targetPath = Path.Combine(_defaultsPath, presetName + ".factory");
            byte[] presetBytes = ResourceUtils.GetEmbeddedResource(presetName + ".factory");
            preset.LoadScenePreset(presetBytes);
            SaveDefault(defaultType);
            Graphics.Instance.Log.LogMessage(string.Format("Restored {0} to factory defaults.", defaultType.ToString()));
            return true;
        }

        internal void Load(PluginData data)
        {
            if (data != null && data.data.TryGetValue("bytes", out object val))
            {
                byte[] presetData = (byte[])val;
                if (!presetData.IsNullOrEmpty())
                {
                    Preset preset = new Preset(_parent.Settings, _parent.CameraSettings, _parent.LightingSettings, _parent.PostProcessingSettings, _parent.SkyboxManager.skyboxParams/*, _parent.SSSSettings*/);
                    preset.LoadScenePreset(presetData);
                }
            }
        }

    }

}
