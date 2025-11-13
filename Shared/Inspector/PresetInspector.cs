using System;
using System.Linq;
using UnityEngine;
using Graphics.Settings;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class PresetInspector
    {
        private const string _nameCue = "";
        private static string _nameToSave = string.Empty;
        private static int _presetIndexOld = -1;
        private static int _presetIndexCurrent = -1;
        private static string searchQuery = string.Empty;
        private static Vector2 presetScrollView;

        private static int cachedFontSize = -1;
        private static int paddingL, paddingR, paddingT;
        private static GUIStyle PresetBox, TabContent;

        static void UpdateCachedValues(GlobalSettings renderSettings)
        {
            if (cachedFontSize == renderSettings.FontSize) return;

            cachedFontSize = renderSettings.FontSize;

            paddingL = Mathf.RoundToInt(renderSettings.FontSize * 2f);
            paddingR = Mathf.RoundToInt(renderSettings.FontSize * 0.3f);
            paddingT = Mathf.RoundToInt(renderSettings.FontSize * 0.5f);

            PresetBox = new GUIStyle(GUI.skin.textField);
            PresetBox.padding = new RectOffset(paddingR, paddingR, paddingT, paddingT);
            PresetBox.normal.background = null;

            TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(paddingL, paddingL, paddingL, paddingL);

        }

        internal static void Draw(PresetManager presetManager, GlobalSettings renderSettings, bool showAdvanced)
        {

            UpdateCachedValues(renderSettings);

            GUILayout.BeginVertical(GUIStyles.tabcontent);
            {
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                TextSearch("LOAD PRESET", searchQuery, search => searchQuery = search);
                if (Button("Refresh Preset List", true))
                {
                    presetManager.RefreshPresetListManually();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                Label("Filter settings:", "", false);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                ToggleAlt("Skybox", Graphics.loadSkybox.Value, false, loadskybox => Graphics.loadSkybox.Value = loadskybox);
                //ToggleAlt("RainDrop", Graphics.loadRain.Value, false, loadrain => Graphics.loadRain.Value = loadrain);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                ToggleAlt("Aura 2", Graphics.loadAura.Value, false, loadAura => Graphics.loadAura.Value = loadAura);
                //ToggleAlt("Volumetrics", Graphics.loadVolumetrics.Value, false, loadVolumetrics => Graphics.loadVolumetrics.Value = loadVolumetrics);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                //ToggleAlt("NGS-Shadows", Graphics.loadShadows.Value, false, loadShadows => Graphics.loadShadows.Value = loadShadows);
                ToggleAlt("LuxWater", Graphics.loadLuxwater.Value, false, loadLuxwater => Graphics.loadLuxwater.Value = loadLuxwater);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                ToggleAlt("Fog", Graphics.loadHeightFog.Value, false, fog => Graphics.loadHeightFog.Value = fog);
                ToggleAlt("DoF", Graphics.loadDoF.Value, false, dof => Graphics.loadDoF.Value = dof);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                ToggleAlt("SSS", Graphics.loadSSS.Value, false, sss => Graphics.loadSSS.Value = sss);
                ToggleAlt("SEGI", Graphics.loadSEGI.Value, false, loadSEGI => Graphics.loadSEGI.Value = loadSEGI);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                if (presetManager.PresetNames.IsNullOrEmpty())
                {
                    LabelColorRed("No presets found. Go to F1 Menu and reset the default folder paths.", "");
                }
                else
                {
                    GUILayout.BeginVertical(PresetBox);
                    {
                        presetScrollView = GUILayout.BeginScrollView(presetScrollView);

                        var presetStructure = presetManager.GetPresetStructure();
                        _presetIndexCurrent = Array.IndexOf(presetManager.PresetNames, presetManager.CurrentPreset);

                        foreach (var folder in presetStructure)
                        {
                            Label(folder.Name, "", true);

                            //Filter preset
                            var filteredPresets = folder.Presets
                                .Where(p => p.ToLower().Contains(searchQuery.ToLower()))
                                .ToArray();

                            int presetCount = filteredPresets.Length;

                            if (presetCount > 0)
                            {
                                int selectedIndex = GUILayout.SelectionGrid(-1, filteredPresets, Inspector.Width / 300);

                                if (selectedIndex >= 0)
                                {
                                    presetManager.CurrentPreset = filteredPresets[selectedIndex];
                                    _presetIndexCurrent = Array.IndexOf(filteredPresets, presetManager.CurrentPreset);
                                    _presetIndexOld = _presetIndexCurrent;
                                }
                            }
                            else
                            {
                                Label("No matching presets found.", "", false);
                            }
                        }

                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.Space(5);
                GUILayout.FlexibleSpace(); // Add a flexible space here to push everything above upwards

                GUILayout.Space(10);
                GUILayout.ExpandHeight(true);

                GUILayout.Space(20);
                GUILayout.BeginHorizontal();

                TextSave("SAVE PRESET", _nameToSave, save =>
                {
                    _nameToSave = save;
                });
                bool isValidFileName = (0 != _nameToSave.Length && 256 >= _nameToSave.Length);
                bool isCue = (_nameCue == _nameToSave);
                if (Button("Save", true) && isValidFileName && !isCue)
                {
                    presetManager.Save(_nameToSave);
                    presetManager.CurrentPreset = _nameToSave;
                    _presetIndexOld = Array.IndexOf(presetManager.PresetNames, presetManager.CurrentPreset);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(1);
                if (!isCue && !isValidFileName)
                {
                    GUILayout.Label("Please specify a valid file name.");
                    Graphics.Instance.Log.LogMessage("Please specify a valid file name.");
                }

                GUILayout.Space(20);
                Label("DEFAULT PRESETS", "", true);
                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                if (KKAPI.KoikatuAPI.GetCurrentGameMode() != KKAPI.GameMode.MainGame && KKAPI.KoikatuAPI.GetCurrentGameMode() != KKAPI.GameMode.Unknown && (!showAdvanced))
                {
                    GUI.enabled = false;
                }
                if (Button("Load Main Game DEFAULT", true))
                {
                    presetManager.LoadDefault(PresetDefaultType.MAIN_GAME);
                }
                GUILayout.Space(10);
                if (Button("Save Current as Main Game DEFAULT", true))
                {
                    presetManager.SaveDefault(PresetDefaultType.MAIN_GAME);
                }
                GUILayout.Space(10);
                if (Button("Reset Main Game DEFAULT", true))
                {
                    presetManager.RestoreDefault(PresetDefaultType.MAIN_GAME);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (KKAPI.KoikatuAPI.GetCurrentGameMode() != KKAPI.GameMode.Unknown && (!showAdvanced))
                {
                    GUI.enabled = false;
                }
                if (Button("Load Title DEFAULT", true))
                {
                    presetManager.LoadDefault(PresetDefaultType.TITLE);
                }
                GUILayout.Space(10);
                if (Button("Save Current as Title DEFAULT", true))
                {
                    presetManager.SaveDefault(PresetDefaultType.TITLE);
                }
                GUILayout.Space(10);
                if (Button("Reset Title DEFAULT", true))
                {
                    presetManager.RestoreDefault(PresetDefaultType.TITLE);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (KKAPI.KoikatuAPI.GetCurrentGameMode() != KKAPI.GameMode.Maker && (!showAdvanced))
                {
                    GUI.enabled = false;
                }
                if (Button("Load Maker DEFAULT", true))
                {
                    presetManager.LoadDefault(PresetDefaultType.MAKER);
                }
                GUILayout.Space(10);
                if (Button("Save Current as Maker DEFAULT", true))
                {
                    presetManager.SaveDefault(PresetDefaultType.MAKER);
                }
                GUILayout.Space(10);
                if (Button("Reset Maker DEFAULT", true))
                {
                    presetManager.RestoreDefault(PresetDefaultType.MAKER);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (KKAPI.KoikatuAPI.GetCurrentGameMode() != KKAPI.GameMode.Unknown && (!showAdvanced))
                {
                    GUI.enabled = false;
                }
                if (Button("Load VR DEFAULT", true))
                {
                    presetManager.LoadDefault(PresetDefaultType.VR_GAME);
                }
                GUILayout.Space(10);
                if (Button("Save Current as VR DEFAULT", true))
                {
                    presetManager.SaveDefault(PresetDefaultType.VR_GAME);
                }
                GUILayout.Space(10);
                if (Button("Reset VR DEFAULT", true))
                {
                    presetManager.RestoreDefault(PresetDefaultType.VR_GAME);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (KKAPI.KoikatuAPI.GetCurrentGameMode() != KKAPI.GameMode.Studio && (!showAdvanced))
                {
                    GUI.enabled = false;
                }
                if (Button("Load Studio DEFAULT", true))
                {
                    presetManager.LoadDefault(PresetDefaultType.STUDIO);
                }
                GUILayout.Space(10);
                if (Button("Save Current as Studio DEFAULT", true))
                {
                    presetManager.SaveDefault(PresetDefaultType.STUDIO);
                }
                GUILayout.Space(10);
                if (Button("Reset Studio DEFAULT", true))
                {
                    presetManager.RestoreDefault(PresetDefaultType.STUDIO);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
            }
            GUILayout.EndVertical();
        }
    }
}