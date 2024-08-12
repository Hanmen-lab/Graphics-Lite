using KKAPI.Utilities;
using System;
using UnityEngine;
using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal static class PresetInspector
    {
        private const string _nameCue = "(preset name)";
        private static string _nameToSave = _nameCue;
        private static int _presetIndexOld = -1;
        private static int _presetIndexCurrent = -1;

        private static bool ShouldUpdate => _presetIndexCurrent != -1 && _presetIndexCurrent != _presetIndexOld;

        private static Vector2 presetScrollView;
        internal static void Draw(PresetManager presetManager)
        {
            GUIStyle BoxPadding = new GUIStyle(GUI.skin.box);
            BoxPadding.padding = new RectOffset(20, 20, 3, 13);
            BoxPadding.normal.background = null;

            GUIStyle PresetBox = new GUIStyle(GUI.skin.textField);
            PresetBox.padding = new RectOffset(5, 5, 10, 10);
            PresetBox.normal.background = null;

            GUIStyle EmptyBoxR = new GUIStyle(GUI.skin.box);
            EmptyBoxR.padding = new RectOffset(0, 0, 3, 3);
            EmptyBoxR.normal.background = null;

            GUIStyle SmallBox = new GUIStyle(GUI.skin.box);
            SmallBox.normal.background = null;
            SmallBox.fixedWidth = 60;

            GUILayout.BeginVertical(GUIStyles.tabcontent);
            {
                GUILayout.Space(10);
                Label("LOAD PRESET", "", true);
                GUILayout.Space(20);
                if (presetManager.PresetNames.IsNullOrEmpty())
                {
                    LabelColorRed("No presets found. Go to F1 Menu and reset the default folder paths.", "");
                }
                else
                {
                    GUILayout.BeginVertical(PresetBox);
                    {
                        presetScrollView = GUILayout.BeginScrollView(presetScrollView);
                        _presetIndexCurrent = Array.IndexOf(presetManager.PresetNames, presetManager.CurrentPreset);
                        _presetIndexCurrent = GUILayout.SelectionGrid(_presetIndexCurrent, presetManager.PresetNames, Inspector.Width / 300);
                        if (ShouldUpdate)
                        {
                            presetManager.CurrentPreset = presetManager.PresetNames[_presetIndexCurrent];
                            _presetIndexOld = _presetIndexCurrent; // to prevent continous update;
                        }
                        GUILayout.EndScrollView();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (Button("Refresh Preset List", true))
                {
                    presetManager.RefreshPresetListManually();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                Label("SAVE PRESET", "", true);
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                _nameToSave = GUILayout.TextField(_nameToSave);
                bool isValidFileName = (0 != _nameToSave.Length && 256 >= _nameToSave.Length);
                bool isCue = (_nameCue == _nameToSave);
                if (Button("Save") && isValidFileName && !isCue)
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
                }
                GUILayout.Space(20);
                Label("DEFAULT PRESETS", "", true);
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
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
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
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
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
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
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
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
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
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
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
        }
    }
}
