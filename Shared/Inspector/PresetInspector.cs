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

        //public static PresetSettings presetSettings;

        //private static bool ShouldUpdate => _presetIndexCurrent != -1 && _presetIndexCurrent != _presetIndexOld;

        private static Vector2 presetScrollView;


        internal static void Draw(PresetManager presetManager, GlobalSettings renderSettings, bool showAdvanced)
        {
            GUIStyle PresetBox = new GUIStyle(GUI.skin.textField);
            PresetBox.padding = new RectOffset(Mathf.RoundToInt(renderSettings.FontSize * 0.3f), Mathf.RoundToInt(renderSettings.FontSize * 0.3f), Mathf.RoundToInt(renderSettings.FontSize * 0.5f), Mathf.RoundToInt(renderSettings.FontSize * 0.5f));
            PresetBox.normal.background = null;

            GUIStyle TabContent = new GUIStyle(GUIStyles.tabcontent);
            TabContent.padding = new RectOffset(Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f), Mathf.RoundToInt(renderSettings.FontSize * 2f));


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
                ToggleAlt("Skybox", presetManager.loadSkybox, false, loadskybox => presetManager.loadSkybox = loadskybox);
                //ToggleAlt("RainDrop", presetManager.loadRain, false, loadrain => presetManager.loadRain = loadrain);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                ToggleAlt("Aura 2", presetManager.loadLoadAura, false, loadAura => presetManager.loadLoadAura = loadAura);
                //ToggleAlt("Volumetrics", presetManager.loadVolumetrics, false, loadVolumetrics => presetManager.loadVolumetrics = loadVolumetrics);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                //ToggleAlt("NGS-Shadows", presetManager.loadShadows, false, loadShadows => presetManager.loadShadows = loadShadows);
                ToggleAlt("LuxWater", presetManager.loadLuxwater, false, loadLuxwater => presetManager.loadLuxwater = loadLuxwater);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                ToggleAlt("Fog", presetManager.loadHeightFog, false, fog => presetManager.loadHeightFog = fog);
                ToggleAlt("DoF", presetManager.loadDoF, false, dof => presetManager.loadDoF = dof);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                ToggleAlt("SSS", presetManager.loadSSS, false, sss => presetManager.loadSSS = sss);
                ToggleAlt("SEGI", presetManager.loadSEGI, false, loadSEGI => presetManager.loadSEGI = loadSEGI);
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