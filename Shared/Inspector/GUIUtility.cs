using Graphics.Settings;
using Graphics.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graphics.Inspector
{
    /// <summary>
    /// The controls the inspectors are written with. Each keeps the signature and the behaviour it had
    /// as an IMGUI control - the value in, the change out through its callback, the override box of a
    /// post-processing parameter in front of it - and is drawn as a row of the new window.
    /// </summary>
    internal class Util
    {
        private static string L(string text)
        {
            return W.Localize(text);
        }

        private static string[] L(string[] texts)
        {
            return LocalizationManager.HasLocalization() ? texts.Select(t => LocalizationManager.Localized(t)).ToArray() : texts;
        }

        /// <summary>
        /// The override box: shown when there is a callback for it. Its click is handed on at once, and the
        /// control after it follows the new state in the same frame, as it did.
        /// </summary>
        private static bool Override(bool toggled, ref bool enable, Action<bool> onChangedEnable)
        {
            if (onChangedEnable != null && toggled)
            {
                enable = !enable;
                onChangedEnable(enable);
                return true;
            }
            return false;
        }

        #region Sliders
        private static float SliderRow(string kind, string label, float value, float min, float max, string format, bool isInt, Color[] track,
            bool enable, Action<bool> onChangedEnable, bool steps = false, SliderNode[] node = null)
        {
            bool check = onChangedEnable != null;
            SliderNode n = Im.Next(kind + (check ? "+" : "") + ":" + label, p => new SliderNode(p, check, steps));
            if (node != null)
                node[0] = n;
            Override(n.CheckToggled(), ref enable, onChangedEnable);
            return n.Update(L(label), value, min, max, format, isInt, track, enable, enable);
        }

        internal static void Slider(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null, int spacing = 0)
        {
            float newValue = SliderRow("slider", label, value, min, max, format, false, null, enable, onChangedEnable);
            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        internal static void LightIntensity(string label, float value, float min, ref float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null, int spacing = 0)
        {
            SliderNode[] node = new SliderNode[1];
            float newValue = SliderRow("intensity", label, value, min, max, format, false, null, enable, onChangedEnable, true, node);
            SliderNode n = node[0];
            if (n.MinusClicked)
            {
                n.MinusClicked = false;
                float newMax = Mathf.Max(min, max - 100f);
                if (newMax < max)
                {
                    max = newMax;
                    newValue = Mathf.Clamp(newValue, min, max);
                }
            }
            if (n.PlusClicked)
            {
                n.PlusClicked = false;
                max += 100f;
                newValue = Mathf.Clamp(newValue, min, max);
            }
            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        internal static void SliderAlpha(string label, float value, float min, float max, string format, bool reverse = false, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            float newValue = SliderRow("salpha", label, value, min, max, format, false, reverse ? W.ReverseAlphaTrack : W.AlphaTrack, enable, onChangedEnable);
            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        internal static void SliderColorTemp(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            float newValue = SliderRow("stemp", label, value, min, max, format, false, W.TempTrack, enable, onChangedEnable);
            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        internal static void SliderColorTint(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            float newValue = SliderRow("stint", label, value, min, max, format, false, W.TintTrack, enable, onChangedEnable);
            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        internal static void SliderColorVib(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            float newValue = SliderRow("svib", label, value, min, max, format, false, W.VibTrack, enable, onChangedEnable);
            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        internal static void SliderColorHue(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            float newValue = SliderRow("shue", label, value, min, max, format, false, W.HueTrack, enable, onChangedEnable);
            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        internal static void SliderTemp(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            Slider(label, value, min, max, format, onChanged, enable, onChangedEnable);
        }

        internal static void Slider(string label, int value, int min, int max, Action<int> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            int newValue = Mathf.RoundToInt(SliderRow("islider", label, value, min, max, "N0", true, null, enable, onChangedEnable));
            if (onChanged != null && value != newValue)
                onChanged(newValue);
        }

        internal static void SliderCompact(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            Slider(label, value, min, max, format, onChanged, enable, onChangedEnable);
        }

        internal static void SliderCompact(string label, int value, int min, int max, Action<int> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            Slider(label, value, min, max, onChanged, enable, onChangedEnable);
        }
        #endregion

        #region Colours
        // useColorDisplayColor32 is for setting colour on skybox tint, Color<->Color32 conversion loses precision
        internal static void SliderColor(string label, Color value, Action<Color> onChanged = null,
            bool useColorDisplayColor32 = false, bool enable = true, Action<bool> onChangedEnable = null, string colourGradingName = "", float mincolourGrading = 0, float maxcolourGrading = 1)
        {
            bool check = onChangedEnable != null;
            ColorNode n = Im.Next("color" + (check ? "+" : "") + ":" + label, p => new ColorNode(p, check));
            Override(n.CheckToggled(), ref enable, onChangedEnable);
            Color picked = n.Update(L(label), value, enable, enable);

            Color newValue = value;
            if (picked != value)
            {
                if (useColorDisplayColor32)
                {
                    newValue.r = picked.r;
                    newValue.g = picked.g;
                    newValue.b = picked.b;
                }
                else
                {
                    // The channels go through 0 to 255, as they did with the RGB sliders.
                    Color32 c32 = picked;
                    Color c = c32;
                    newValue.r = c.r;
                    newValue.g = c.g;
                    newValue.b = c.b;
                }
            }

            if (!colourGradingName.IsNullOrEmpty())
            {
                bool wasEnabled = Im.Enabled;
                Im.Enabled = wasEnabled && enable;
                float a = SliderRow("cgrade", colourGradingName + "#" + label, value.a, mincolourGrading, maxcolourGrading, "N2", false, W.AlphaTrack, true, null);
                Im.Enabled = wasEnabled;
                newValue.a = a;
            }
            else if (useColorDisplayColor32 == false)
                newValue.a = ((Color)(Color32)value).a;

            if (onChanged != null && value != newValue)
                onChanged(newValue);
        }

        internal static void SliderColorVertical(string label, Color value, Action<Color> onChanged = null,
            bool useColorDisplayColor32 = false, bool enable = true, Action<bool> onChangedEnable = null, string colourGradingName = "", float mincolourGrading = 0, float maxcolourGrading = 1)
        {
            SliderColor(label, value, onChanged, useColorDisplayColor32, enable, onChangedEnable, colourGradingName, mincolourGrading, maxcolourGrading);
        }

        private static float Channel(string kind, string label, float value, float min, float max, Color[] track)
        {
            return SliderRow(kind, label, value, min, max, "N2", false, track, true, null);
        }

        internal static float SliderColorR(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true) { return Channel("cr", label, value, min, max, W.RedTrack); }
        internal static float SliderColorG(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true) { return Channel("cg", label, value, min, max, W.GreenTrack); }
        internal static float SliderColorB(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true) { return Channel("cb", label, value, min, max, W.BlueTrack); }
        internal static float SliderColorA(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true) { return Channel("ca", label, value, min, max, W.AlphaTrack); }
        internal static byte SliderColorR(string label, byte value, int spacing) { return (byte)Mathf.RoundToInt(SliderRow("cr8", label, value, 0, 255, "N0", true, W.RedTrack, true, null)); }
        internal static byte SliderColorG(string label, byte value, int spacing) { return (byte)Mathf.RoundToInt(SliderRow("cg8", label, value, 0, 255, "N0", true, W.GreenTrack, true, null)); }
        internal static byte SliderColorB(string label, byte value, int spacing) { return (byte)Mathf.RoundToInt(SliderRow("cb8", label, value, 0, 255, "N0", true, W.BlueTrack, true, null)); }
        internal static float SliderColorVerticalR(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true) { return SliderColorR(label, value, spacing, min, max, RGB); }
        internal static float SliderColorVerticalG(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true) { return SliderColorG(label, value, spacing, min, max, RGB); }
        internal static float SliderColorVerticalB(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true) { return SliderColorB(label, value, spacing, min, max, RGB); }
        internal static byte SliderColorVerticalR(string label, byte value, int spacing) { return SliderColorR(label, value, spacing); }
        internal static byte SliderColorVerticalG(string label, byte value, int spacing) { return SliderColorG(label, value, spacing); }
        internal static byte SliderColorVerticalB(string label, byte value, int spacing) { return SliderColorB(label, value, spacing); }
        #endregion

        #region Choices
        /// <summary>One of several names: the index picked, or the one given.</summary>
        private static int ChoiceRow(string label, string[] names, int index, bool enable, Action<bool> onChangedEnable, Func<int, bool> disabled = null, bool labelled = true)
        {
            bool check = onChangedEnable != null;
            ChoiceNode n = Im.Next("choice" + (check ? "+" : "") + (labelled ? "" : "-") + ":" + label, p => new ChoiceNode(p, check, labelled));
            Override(n.CheckToggled(), ref enable, onChangedEnable);
            return n.Update(L(label), L(names), index, disabled, enable, enable);
        }

        internal static T Selection<T>(string label, T selected, T[] selection, Action<T> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null)
        {
            string[] names = selection.Select(entry => entry.ToString()).ToArray();
            int currentIndex = Array.IndexOf(selection, selected);
            int selectedIndex = ChoiceRow(label, names, currentIndex, enable, onChangedEnable);
            if (selectedIndex == currentIndex || selectedIndex < 0)
                return selected;
            selected = (T)selection.GetValue(selectedIndex);
            onChanged?.Invoke(selected);
            return selected;
        }

        internal static T SelectionVertical<T>(string label, T selected, T[] selection, Action<T> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null)
        {
            return Selection(label, selected, selection, onChanged, columns, enable, onChangedEnable);
        }

        private static TEnum EnumRow<TEnum>(string label, TEnum selected, Action<TEnum> onChanged, bool enable, Action<bool> onChangedEnable, Func<string, bool> disabledName = null)
        {
            string[] selection = Enum.GetNames(typeof(TEnum));
            int currentIndex = Array.IndexOf(selection, selected.ToString());
            Func<int, bool> disabled = disabledName == null ? (Func<int, bool>)null : i => disabledName(selection[i]);
            int selectedIndex = ChoiceRow(label, selection, currentIndex, enable, onChangedEnable, disabled);
            if (selectedIndex == currentIndex || selectedIndex < 0)
                return selected;
            selected = (TEnum)Enum.Parse(typeof(TEnum), selection[selectedIndex]);
            onChanged?.Invoke(selected);
            return selected;
        }

        internal static TEnum Selection<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null)
        {
            return EnumRow(label, selected, onChanged, enable, onChangedEnable);
        }

        internal static TEnum SelectionVertical<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null)
        {
            return EnumRow(label, selected, onChanged, enable, null);
        }

        internal static TEnum Selection2<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null)
        {
            return EnumRow(label, selected, onChanged, enable, onChangedEnable);
        }

        internal static TEnum SelectionNormals<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null) where TEnum : Enum
        {
            bool forward = Graphics.Instance.CameraSettings.RenderingPath != CameraSettings.AIRenderingPath.Deferred;
            return EnumRow(label, selected, onChanged, enable, onChangedEnable,
                name => forward ? (name == "GBuffer" || name == "OctaEncoded") : (name == "Camera" || name == "None"));
        }

        internal static TEnum SelectionApply<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null) where TEnum : Enum
        {
            bool deferred = Graphics.Instance.CameraSettings.RenderingPath == CameraSettings.AIRenderingPath.Deferred;
            return EnumRow(label, selected, onChanged, enable, onChangedEnable, name => deferred == false && name == "Deferred");
        }

        internal static TEnum SelectionSSR<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null) where TEnum : Enum
        {
            bool deferred = Graphics.Instance.CameraSettings.RenderingPath == CameraSettings.AIRenderingPath.Deferred;
            return EnumRow(label, selected, onChanged, enable, null, name => deferred == false && name == "StochasticSSR");
        }

        internal static TEnum SelectionAO<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null) where TEnum : Enum
        {
            bool deferred = Graphics.Instance.CameraSettings.RenderingPath == CameraSettings.AIRenderingPath.Deferred;
            return EnumRow(label, selected, onChanged, enable, null, name => deferred == false && name == "GTAO");
        }

        /// <summary>A label, then the buttons of a grid under it.</summary>
        private static void GridCaption(string label)
        {
            LabelRowNode n = Im.Next("gridlabel:" + label, p => new LabelRowNode(p));
            n.Update(L(label), "", false, false);
        }

        public static TEnum MultiSelection<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1) where TEnum : Enum
        {
            if (-1 == columns)
                columns = Inspector.adaptiveColumns;
            List<TEnum> flags = new List<TEnum>();
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
                if (Convert.ToInt32(value) != 0)
                    flags.Add(value);
            int currentValue = Convert.ToInt32(selected);

            GridCaption(label);
            GridNode n = Im.Next("multi:" + label, p => new GridNode(p));
            int clicked = n.Update(flags.Select(f => f.ToString()).ToArray(), i => (currentValue & Convert.ToInt32(flags[i])) != 0, Mathf.Max(1, columns));
            if (clicked < 0)
                return selected;
            int flag = Convert.ToInt32(flags[clicked]);
            currentValue = (currentValue & flag) != 0 ? currentValue & ~flag : currentValue | flag;
            selected = (TEnum)Enum.ToObject(typeof(TEnum), currentValue);
            onChanged?.Invoke(selected);
            return selected;
        }

        internal static int SelectionTexture(string label, int currentIndex, Texture[] selection, int space = 30, bool enable = true, Action<bool> onChangedEnable = null, object style = null)
        {
            bool check = onChangedEnable != null;
            if (check)
            {
                CheckToggleNode box = Im.Next("texcheck:" + label, p => new CheckToggleNode(p));
                bool now = box.Update(L(label), enable);
                if (now != enable)
                {
                    enable = now;
                    onChangedEnable(enable);
                }
            }
            else
                GridCaption(label);

            bool wasEnabled = Im.Enabled;
            Im.Enabled = wasEnabled && enable;
            ThumbGridNode n = Im.Next("textures:" + label, p => new ThumbGridNode(p));
            int clicked = n.Update(selection, currentIndex, 64f);
            Im.Enabled = wasEnabled;
            return clicked >= 0 ? clicked : currentIndex;
        }

        internal static void SelectionMask(string label, int cullingMask, Action<int> onChanged = null, int columns = 4)
        {
            GridCaption(label);
            GridNode n = Im.Next("mask:" + label, p => new GridNode(p));
            List<string> layers = CullingMaskExtensions.Layers;
            int clicked = n.Update(layers.ToArray(), i => CullingMaskExtensions.LayerCullingIncludes(cullingMask, layers[i]), Mathf.Max(1, columns));
            if (clicked < 0)
                return;
            string layer = layers[clicked];
            int newMask = CullingMaskExtensions.LayerCullingToggle(cullingMask, layer, !CullingMaskExtensions.LayerCullingIncludes(cullingMask, layer));
            if (newMask != cullingMask)
                onChanged(newMask);
        }

        internal static void SelectionMaskVertical(string label, int cullingMask, Action<int> onChanged = null, int columns = 4)
        {
            SelectionMask(label, cullingMask, onChanged, columns);
        }

        internal static TEnum Toolbar<TEnum>(TEnum selected) where TEnum : Enum
        {
            string[] names = Enum.GetNames(typeof(TEnum));
            int current = Array.IndexOf(names, selected.ToString());
            GridNode n = Im.Next("toolbar:" + typeof(TEnum).Name, p => new GridNode(p));
            int clicked = n.Update(L(names), i => i == current, names.Length);
            return clicked >= 0 ? (TEnum)Enum.Parse(typeof(TEnum), names[clicked]) : selected;
        }
        #endregion

        #region Toggles and buttons
        internal static void Toggle(string label, bool toggle, bool bold = false, Action<bool> onChanged = null)
        {
            ToggleNode n = Im.Next("toggle:" + label, p => new ToggleNode(p, false));
            bool newToggle = n.Update(L(label), toggle, bold);
            if (onChanged != null && toggle != newToggle)
                onChanged(newToggle);
        }

        internal static void ToggleAlt(string label, bool toggle, bool bold = false, Action<bool> onChanged = null)
        {
            ToggleNode n = Im.Next("togglealt:" + label, p => new ToggleNode(p, false));
            bool newToggle = n.Update(L(label), toggle, bold);
            if (onChanged != null && toggle != newToggle)
                onChanged(newToggle);
        }

        internal static void ToggleWithText(string label, bool toggle, string rightText = "", bool bold = false, Action<bool> onChanged = null)
        {
            ToggleNode n = Im.Next("toggletext:" + label, p => new ToggleNode(p, false));
            bool newToggle = n.Update(L(label), toggle, bold, string.IsNullOrEmpty(rightText) ? "" : L(rightText));
            if (onChanged != null && toggle != newToggle)
                onChanged(newToggle);
        }

        /// <summary>An effect's own switch: the head of its card.</summary>
        internal static void Switch(string label, bool toggle, bool bold = true, Action<bool> onChanged = null)
        {
            SwitchHeadNode n = Im.Next("switch:" + label, p => new SwitchHeadNode(p));
            bool newToggle = n.Update(L(label), toggle);
            if (onChanged != null && toggle != newToggle)
                onChanged(newToggle);
        }

        internal static bool ToggleButton(string label, bool toggle, bool UseButton = false)
        {
            label = L(label);
            return UseButton ? Lay.Toggle(toggle, label, "Button") : Lay.Toggle(toggle, label);
        }

        internal static bool LightButton(string label, bool toggle, bool UseButton = false, int maxLength = 24)
        {
            label = L(label);
            if (label.Length > maxLength)
                label = label.Substring(0, maxLength) + "...";
            return UseButton ? Lay.Toggle(toggle, label, "Button") : Lay.Toggle(toggle, label);
        }

        internal static bool Button(string label, bool ExpandWidth = false)
        {
            return Lay.Button(L(label));
        }
        #endregion

        #region Text
        internal static void Label(string label, string text, bool bold = false, bool boldtext = false)
        {
            label = L(label);
            if (0 < text.Length)
                text = L(text);

            if (label.Length == 0 && text.Length == 0)
            {
                SpaceNode gap = Im.Next("space", p => new SpaceNode(p));
                gap.Update(4f, Im.InRow, false);
                return;
            }
            if (text.Length == 0)
            {
                if (bold)
                {
                    CaptionNode cap = Im.Next("caption", p => new CaptionNode(p));
                    cap.Update(label);
                }
                else
                {
                    NoteNode note = Im.Next("note", p => new NoteNode(p));
                    note.Update(label, Pal.C(W.Dim), false);
                }
                return;
            }
            if (label.Length == 0)
            {
                NoteNode note = Im.Next("note", p => new NoteNode(p));
                note.Update(text, Pal.C(W.Dim), boldtext);
                return;
            }
            LabelRowNode n = Im.Next("label", p => new LabelRowNode(p));
            n.Update(label, text, bold, boldtext);
        }

        internal static void Warning(string text, bool bold = false)
        {
            WarningNode n = Im.Next("warning", p => new WarningNode(p));
            n.Update(L(text));
        }

        internal static void WarningCompact(string text, bool bold = false)
        {
            Warning(text, bold);
        }

        internal static void LabelColorRed(string label, string text, bool bold = false)
        {
            label = L(label);
            if (0 < text.Length)
                text = L(text);
            NoteNode n = Im.Next("red", p => new NoteNode(p));
            n.Update(text.Length > 0 ? label + "  " + text : label, Pal.Hex(0xF08A7E), bold);
        }

        private static string FieldRow(string kind, string label, string text, bool enable, Action<bool> onChangedEnable, bool live, int limit = 0, string placeholder = null, bool labelled = true)
        {
            bool check = onChangedEnable != null;
            FieldNode n = Im.Next(kind + (check ? "+" : "") + ":" + label, p => new FieldNode(p, labelled, check, live, limit));
            Override(n.CheckToggled(), ref enable, onChangedEnable);
            return n.Update(L(label), text, placeholder, enable, enable);
        }

        internal static void Text(string label, int Integer, Action<int> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            string typed = FieldRow("int", label, Integer.ToString(), enable, onChangedEnable, false);
            int count;
            if (int.TryParse(typed, out count) && onChanged != null && Integer != count)
                onChanged(count);
        }

        internal static void Text(string label, float Float, string format = "N0", Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            string typed = FieldRow("float", label, Float.ToString(format), enable, onChangedEnable, false);
            float count;
            if (W.ParseFloat(typed, out count) && onChanged != null && Float.ToString(format) != typed)
                onChanged(count);
        }

        internal static void Text(string label, string text, Action<string> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            bool wasEnabled = Im.Enabled;
            string newText = FieldRow("text", label, text, enable, null, true, 32);
            if (onChanged != null && text != newText)
                onChanged(newText);
        }

        internal static void TextSearch(string label, string text, Action<string> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            string newText = FieldRow("search", label, text, enable, null, true, 32, L("Search Presets..."));
            if (onChanged != null && text != newText)
                onChanged(newText);
        }

        internal static void TextSave(string label, string text, Action<string> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            string newText = FieldRow("save", label, text, enable, null, true, 32, L("Preset Name..."));
            if (onChanged != null && text != newText)
                onChanged(newText);
        }

        private static float[] VectorRow(string label, float[] values, string format)
        {
            VectorNode n = Im.Next("vec" + values.Length + ":" + label, p => new VectorNode(p, values.Length));
            float[] result;
            n.Update(L(label), values, format, out result);
            return result;
        }

        internal static Vector2 Dimension(string label, Vector2 size, Action<Vector2> onChanged = null)
        {
            float[] v = VectorRow(label, new[] { size.x, size.y }, "N2");
            Vector2 newSize = new Vector2(v[0], v[1]);
            if (newSize != size)
                onChanged?.Invoke(newSize);
            return newSize;
        }

        internal static Vector3 Dimension(string label, Vector3 size, Action<Vector3> onChanged = null)
        {
            float[] v = VectorRow(label, new[] { size.x, size.y, size.z }, "N2");
            Vector3 newSize = new Vector3(v[0], v[1], v[2]);
            if (newSize != size)
                onChanged?.Invoke(newSize);
            return newSize;
        }

        internal static Vector4 Dimension(string label, Vector4 size, Action<Vector4> onChanged = null)
        {
            float[] v = VectorRow(label, new[] { size.x, size.y, size.z, size.w }, null);
            Vector4 newSize = new Vector4(v[0], v[1], v[2], v[3]);
            if (newSize != size)
                onChanged?.Invoke(newSize);
            return newSize;
        }
        #endregion

        #region Pickers
        public static Light LightSelector(LightManager lightManager, string label, Light selected, Action<Light> onChanged = null, int columns = 3, bool enable = true, Action<bool> onChangedEnable = null, int minButtonWidth = 170)
        {
            List<Light> selectionList = new List<Light>();
            lightManager.DirectionalLights.ForEach(dirLight => selectionList.Add(dirLight.Light));
            Light[] selectionLights = selectionList.ToArray();
            int currentIndex = Array.IndexOf(selectionLights, selected);

            string[] names = selectionLights.Select(light => light != null ? light.name : "").ToArray();
            int selectedIndex = ChoiceRow(label, names, currentIndex, enable, onChangedEnable);
            if (selectedIndex == currentIndex || selectedIndex < 0 || selectedIndex >= selectionLights.Length)
                return null;

            Light selectedLight = selectionLights[selectedIndex];
            onChanged?.Invoke(selectedLight);
            return selectedLight;
        }

        public static void DrawTextureGrid(string[] names, Texture[] textures, string currentName, Action<string> onSelect, float previewSize = 64f)
        {
            if (names == null || textures == null || names.Length == 0)
                return;
            int currentIndex = Array.IndexOf(names, currentName);
            ThumbGridNode n = Im.Next("texgrid", p => new ThumbGridNode(p));
            int clicked = n.Update(textures, currentIndex, previewSize);
            if (clicked >= 0 && clicked < names.Length)
                onSelect?.Invoke(names[clicked]);
        }
        #endregion
    }
}
