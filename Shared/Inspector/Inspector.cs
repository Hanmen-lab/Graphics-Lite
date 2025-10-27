using ADV.Commands.Base;
using Graphics.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Graphics.Inspector.Util;

namespace Graphics.Inspector
{
    internal class Inspector
    {
        private static Rect _windowRect;
        private readonly int _windowID = 53157126;
        public static int adaptiveColumns;
        public static int adaptivePreviewColumns;
        private static int cachedWindowWidth = -1;
        public static int maxTextWidth;
        public enum Tab { Environmental, Probes, Lights, GI, PostProcessing, AntiAliasing, SSS, Presets, Settings };
        public static readonly Dictionary<Inspector.Tab, string> DisplayNames = new Dictionary<Inspector.Tab, string>
        {
            { Inspector.Tab.Environmental, "Environment" },
            { Inspector.Tab.Probes, "Probes" },
            { Inspector.Tab.Lights, "Lights" },
            { Inspector.Tab.GI, "  GI  " },
            { Inspector.Tab.PostProcessing, "Post Processing" },
            { Inspector.Tab.AntiAliasing, "Anti-Aliasing" },
            { Inspector.Tab.SSS, " SSS " },
            { Inspector.Tab.Presets, "Presets" },
            { Inspector.Tab.Settings, "Settings" }
        };


        private Tab SelectedTab { get; set; }
        internal Graphics Parent { get; set; }

        internal Inspector(Graphics parent)
        {
            Parent = parent;

            // Require at least 20 pixels of grab
            if (StartOffsetX + 20 > Screen.width || StartOffsetY + 20 > Screen.height)
            {
                // offscreen protection - reset offsets
                StartOffsetX = (Screen.width - Graphics.ConfigWindowWidth.Value) / 2;
                StartOffsetY = (Screen.height - Graphics.ConfigWindowHeight.Value) / 2;
            }

            if (StartOffsetX < 0)
                StartOffsetX = 0;

            if (StartOffsetY < 0)
                StartOffsetY = 0;

            _windowRect = new Rect(StartOffsetX, StartOffsetY, Width, Height);
        }

        internal static int Width
        {
            get => Graphics.ConfigWindowWidth.Value;
            set
            {
                Graphics.ConfigWindowWidth.Value = value;
                _windowRect.width = value;
            }
        }

        internal static int Height
        {
            get => Graphics.ConfigWindowHeight.Value;
            set
            {
                Graphics.ConfigWindowHeight.Value = value;
                _windowRect.height = value;
            }
        }

        internal static int StartOffsetX
        {
            get => Graphics.ConfigWindowOffsetX.Value;
            set => Graphics.ConfigWindowOffsetX.Value = value;
        }

        internal static int StartOffsetY
        {
            get => Graphics.ConfigWindowOffsetY.Value;
            set => Graphics.ConfigWindowOffsetY.Value = value;
        }
        static void UpdateColumns(GlobalSettings renderSettings)
        {
            if (cachedWindowWidth == Inspector.Width)
                return;
            maxTextWidth = Inspector.Width - 200;
            cachedWindowWidth = Inspector.Width;
            int availableWidth = Inspector.Width - 330; // Padding
            adaptiveColumns = Mathf.Max(1, Mathf.FloorToInt(availableWidth / 150f));
            adaptivePreviewColumns = Mathf.Max(1, Mathf.FloorToInt(availableWidth / 74f));
        }

        internal void DrawWindow()
        {
            _windowRect = GUILayout.Window(_windowID, _windowRect, WindowFunction, "");
            EatInputInRect(_windowRect);
            StartOffsetX = (int)_windowRect.x;
            StartOffsetY = (int)_windowRect.y;

            GlobalSettings renderSettings = Parent.Settings;
            UpdateColumns(renderSettings);
        }

        private void WindowFunction(int thisWindowID)
        {
            GUIStyle BoxPadding = new GUIStyle(GUI.skin.box);
            BoxPadding.margin = new RectOffset(0, 0, 0, 0);
            BoxPadding.padding = new RectOffset(0, 0, 0, 0);
            BoxPadding.normal.background = null;

            GUILayout.BeginVertical(BoxPadding);
            SelectedTab = Toolbar(SelectedTab);
            DrawTabs(SelectedTab);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawTabs(Tab tabSelected)
        {
            //GUILayout.Space(10);
            switch (tabSelected)
            {
                case Tab.Environmental:
                    LightingInspector.Draw(Parent.LightingSettings, Parent.SkyboxManager, Parent.LightManager, Parent.Settings, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.Lights:
                    LightInspector.Draw(Parent.Settings, Parent.LightManager, Parent.LightingSettings, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.Probes:
                    ReflectionProbeInspector.Draw(Parent.LightingSettings, Parent.SkyboxManager, Parent.Settings, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.GI:
                    SEGIInspector.Draw(Parent.LightManager, Parent.Settings);
                    break;
                case Tab.PostProcessing:
                    PostProcessingInspector.Draw(Parent.LightManager, Parent.PostProcessingSettings, Parent.Settings, Parent.PostProcessingManager, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.AntiAliasing:
                    AntiAliasingInspector.Draw(Parent.Settings, Parent.CameraSettings, Parent.PostProcessingSettings, Parent.PostProcessingManager, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.SSS:
                    SSSInspector.Draw(Parent.Settings);
                    break;
                case Tab.Presets:
                    PresetInspector.Draw(Parent.PresetManager, Parent.Settings, Parent.Settings.ShowAdvancedSettings);
                    break;
                case Tab.Settings:
                    SettingsInspector.Draw(Parent.CameraSettings, Parent.Settings, Parent.LightManager, Parent.Settings.ShowAdvancedSettings);
                    break;
            }
        }

        private static void EatInputInRect(Rect eatRect)
        {
            if (eatRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
            {
                Input.ResetInputAxes();
            }
        }
    }
}
