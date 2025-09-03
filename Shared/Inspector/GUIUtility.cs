using System;
using System.Linq;
using UnityEngine;
using static Graphics.Inspector.Util;
using Graphics.Settings;
using static Graphics.LightManager;
using System.Collections.Generic;

namespace Graphics.Inspector
{
    internal class Util
    {
        //private static readonly float textFieldSize = GUIStyles.fontSize * 3f;
        private static readonly Texture2D colourIndicator = new Texture2D(32, 16, TextureFormat.RGB24, false, true);
        private static readonly int enableSpacing = 18;

        static public float HorizontalTempSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle tempslider = GUIStyles.tempslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, tempslider, thumb, options);
        }

        static public float HorizontalTintSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle tintslider = GUIStyles.tintslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, tintslider, thumb, options);
        }

        static public float HorizontalVibSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle vibslider = GUIStyles.vibslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, vibslider, thumb, options);
        }

        static public float HorizontalHueSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle hueslider = GUIStyles.hueslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, hueslider, thumb, options);
        }

        static public float HorizontalRedSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle rslider = GUIStyles.rslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, rslider, thumb, options);
        }

        static public float HorizontalGreenSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle gslider = GUIStyles.gslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, gslider, thumb, options);
        }

        static public float HorizontalBlueSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle bslider = GUIStyles.bslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, bslider, thumb, options);
        }

        static public float HorizontalAlphaSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle aslider = GUIStyles.aslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, aslider, thumb, options);
        }

        static public float HorizontalReverseAlphaSlider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
        {
            GUIStyle arslider = GUIStyles.arslider;
            GUIStyle thumb = GUI.skin.horizontalSliderThumb;
            return DoHorizontalSlider(value, leftValue, rightValue, arslider, thumb, options);
        }

        static float DoHorizontalSlider(float value, float leftValue, float rightValue, GUIStyle slider, GUIStyle thumb, GUILayoutOption[] options)
        {
            return GUI.HorizontalSlider(GUILayoutUtility.GetRect(GUIContent.Temp("mmmm"), slider, options), value, leftValue, rightValue, slider, thumb);
        }


        internal static void Slider(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            bool previousEnabledState = GUI.enabled;
            if (!enable)
            {
                GUI.enabled = false;
            }

            float newValue = GUILayout.HorizontalSlider(value, min, max);
            string valueString = newValue.ToString(format);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));

            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = Mathf.Clamp(parseResult, min, max);
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (onChanged != null && !Mathf.Approximately(value, newValue))
            {
                onChanged(newValue);
            }
        }

        internal static void SliderAlpha(string label, float value, float min, float max, string format, bool reverse = false, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            bool previousEnabledState = GUI.enabled;
            float newValue = 0;

            if (!enable)
            {
                GUI.enabled = false;
            }
            if (reverse)
            {
                newValue = HorizontalReverseAlphaSlider(value, min, max);
            }
            else
            {
                newValue = HorizontalAlphaSlider(value, min, max);
            }
            //float newValue = HorizontalAlphaSlider(value, min, max);
            string valueString = newValue.ToString(format);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));

            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = Mathf.Clamp(parseResult, min, max);
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (onChanged != null && !Mathf.Approximately(value, newValue))
            {
                onChanged(newValue);
            }
        }

        internal static void SliderColorTemp(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            bool previousEnabledState = GUI.enabled;
            if (!enable)
            {
                GUI.enabled = false;
            }

            float newValue = HorizontalTempSlider(value, min, max);
            string valueString = newValue.ToString(format);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));

            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = Mathf.Clamp(parseResult, min, max);
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (onChanged != null && !Mathf.Approximately(value, newValue))
            {
                onChanged(newValue);
            }
        }

        internal static void SliderColorTint(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            bool previousEnabledState = GUI.enabled;
            if (!enable)
            {
                GUI.enabled = false;
            }

            float newValue = HorizontalTintSlider(value, min, max);
            string valueString = newValue.ToString(format);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));

            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = Mathf.Clamp(parseResult, min, max);
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (onChanged != null && !Mathf.Approximately(value, newValue))
            {
                onChanged(newValue);
            }
        }

        internal static void SliderColorVib(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            bool previousEnabledState = GUI.enabled;
            if (!enable)
            {
                GUI.enabled = false;
            }

            float newValue = HorizontalVibSlider(value, min, max);
            string valueString = newValue.ToString(format);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));

            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = Mathf.Clamp(parseResult, min, max);
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (onChanged != null && !Mathf.Approximately(value, newValue))
            {
                onChanged(newValue);
            }
        }

        internal static void SliderColorHue(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            bool previousEnabledState = GUI.enabled;
            if (!enable)
            {
                GUI.enabled = false;
            }

            float newValue = HorizontalHueSlider(value, min, max);
            string valueString = newValue.ToString(format);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));

            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = Mathf.Clamp(parseResult, min, max);
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (onChanged != null && !Mathf.Approximately(value, newValue))
            {
                onChanged(newValue);
            }
        }

        internal static void SliderTemp(string label, float value, float min, float max, string format, Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            if (!enable)
            {
                GUI.enabled = false;
            }

            float newValue = GUILayout.HorizontalSlider(value, min, max);
            string valueString = newValue.ToString(format);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));

            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = Mathf.Clamp(parseResult, min, max);
                }
            }
            GUILayout.EndHorizontal();

            if (onChanged != null && !Mathf.Approximately(value, newValue))
            {
                onChanged(newValue);
            }

            if (!enable)
            {
                GUI.enabled = true;
            }
        }

        internal static void Slider(string label, int value, int min, int max, Action<int> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);

            bool previousEnabledState = GUI.enabled;

            if (!enable)
            {
                GUI.enabled = false;
            }

            int newValue = (int)GUILayout.HorizontalSlider(value, min, max);
            string newValueString = GUILayout.TextField(newValue.ToString(), GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));

            if (newValueString != newValue.ToString())
            {
                if (int.TryParse(newValueString, out int parseResult))
                {
                    newValue = Mathf.Clamp(parseResult, min, max);
                }
            }
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (onChanged != null && !Mathf.Approximately(value, newValue))
            {
                onChanged(newValue);
            }

            //if (!enable)
            //{
            //    GUI.enabled = true;
            //}
        }

        // useColorDisplayColor32 is for setting colour on skybox tint, Color<->Color32 conversion loses precision
        internal static void SliderColor(string label, Color value, Action<Color> onChanged = null,
            bool useColorDisplayColor32 = false, bool enable = true, Action<bool> onChangedEnable = null, string colourGradingName = "", float mincolourGrading = 0, float maxcolourGrading = 1)
        {
            GUIStyle colorind = new GUIStyle(GUI.skin.label);
            colorind.contentOffset = new Vector2(5, 10);

            GUILayout.BeginHorizontal();
            int spacing = 0;

            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            if (!enable)
            {
                GUI.enabled = false;
            }

            GUI.color = new Color(value.r, value.g, value.b, 1f);
            GUILayout.Label(colourIndicator, colorind);
            //GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x - spacing));
            GUI.color = Color.white;

            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            if (useColorDisplayColor32)
            {
                Color color = value;
                color.r = SliderColorR("Red", color.r, spacing);
                color.g = SliderColorG("Green", color.g, spacing);
                color.b = SliderColorB("Blue", color.b, spacing);
                Color newValue = color;
                if (!colourGradingName.IsNullOrEmpty())
                {
                    float colourGradingValue = SliderColorA(colourGradingName, color.a, spacing, mincolourGrading, maxcolourGrading, false);
                    newValue.a = colourGradingValue;
                }
                if (onChanged != null && value != newValue)
                {
                    onChanged(newValue);
                }
            }
            else
            {
                Color32 color = value;
                color.r = SliderColorR("Red", color.r, spacing);
                color.g = SliderColorG("Green", color.g, spacing);
                color.b = SliderColorB("Blue", color.b, spacing);
                Color newValue = color;
                if (!colourGradingName.IsNullOrEmpty())
                {
                    float colourGradingValue = SliderColorA(colourGradingName, value.a, spacing, mincolourGrading, maxcolourGrading, false);
                    newValue.a = colourGradingValue;
                }
                if (onChanged != null && value != newValue)
                {
                    onChanged(newValue);
                }
            }
            GUILayout.EndVertical();
            if (!enable)
            {
                GUI.enabled = true;
            }
        }

        internal static float SliderColorR(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true)
        {
            float labelmar = (GUIStyles.labelWidth) - 60;
            GUIStyle Colorlabel = new GUIStyle(GUI.skin.label);
            Colorlabel.padding = new RectOffset(0, 0, 0, 0);
            Colorlabel.margin = new RectOffset(0, 0, 0, 0);
            Colorlabel.normal.background = null;
            Colorlabel.alignment = TextAnchor.MiddleLeft;
            Colorlabel.fixedWidth = 60;


            GUILayout.BeginHorizontal();
            if (0 != spacing)
            {
                GUILayout.Label("", GUILayout.Width(spacing));
            }
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label("", GUILayout.Width(labelmar));
            GUILayout.Label(label, Colorlabel, GUILayout.ExpandWidth(false));

            float newValue = HorizontalRedSlider(value, min, max);
            string valueString = value.ToString();
            string newValueString = RGB ? newValue.ToString("N2") : newValue.ToString();
            newValueString = GUILayout.TextField(newValueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));
            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = RGB ? parseResult : parseResult;
                }
            }
            GUILayout.EndHorizontal();
            return newValue;
        }

        internal static float SliderColorG(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true)
        {
            float labelmar = (GUIStyles.labelWidth) - 60;
            GUIStyle Colorlabel = new GUIStyle(GUI.skin.label);
            Colorlabel.padding = new RectOffset(0, 0, 0, 0);
            Colorlabel.margin = new RectOffset(0, 0, 0, 0);
            Colorlabel.normal.background = null;
            Colorlabel.alignment = TextAnchor.MiddleLeft;
            Colorlabel.fixedWidth = 60;


            GUILayout.BeginHorizontal();
            if (0 != spacing)
            {
                GUILayout.Label("", GUILayout.Width(spacing));
            }
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label("", GUILayout.Width(labelmar));
            GUILayout.Label(label, Colorlabel, GUILayout.ExpandWidth(false));

            float newValue = HorizontalGreenSlider(value, min, max);
            string valueString = value.ToString();
            string newValueString = RGB ? newValue.ToString("N2") : newValue.ToString();
            newValueString = GUILayout.TextField(newValueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));
            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = RGB ? parseResult : parseResult;
                }
            }
            GUILayout.EndHorizontal();
            return newValue;
        }

        internal static float SliderColorB(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true)
        {
            float labelmar = (GUIStyles.labelWidth) - 60;
            GUIStyle Colorlabel = new GUIStyle(GUI.skin.label);
            Colorlabel.padding = new RectOffset(0, 0, 0, 0);
            Colorlabel.margin = new RectOffset(0, 0, 0, 0);
            Colorlabel.normal.background = null;
            Colorlabel.alignment = TextAnchor.MiddleLeft;
            Colorlabel.fixedWidth = 60;


            GUILayout.BeginHorizontal();
            if (0 != spacing)
            {
                GUILayout.Label("", GUILayout.Width(spacing));
            }
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label("", GUILayout.Width(labelmar));
            GUILayout.Label(label, Colorlabel, GUILayout.ExpandWidth(false));

            float newValue = HorizontalBlueSlider(value, min, max);
            string valueString = value.ToString();
            string newValueString = RGB ? newValue.ToString("N2") : newValue.ToString();
            newValueString = GUILayout.TextField(newValueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));
            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = RGB ? parseResult : parseResult;
                }
            }
            GUILayout.EndHorizontal();
            return newValue;
        }

        internal static float SliderColorA(string label, float value, int spacing, float min = 0.0f, float max = 1.0f, bool RGB = true)
        {
            float labelmar = (GUIStyles.labelWidth) - 60;
            GUIStyle Colorlabel = new GUIStyle(GUI.skin.label);
            Colorlabel.padding = new RectOffset(0, 0, 0, 0);
            Colorlabel.margin = new RectOffset(0, 0, 0, 0);
            Colorlabel.normal.background = null;
            Colorlabel.alignment = TextAnchor.MiddleLeft;
            Colorlabel.fixedWidth = 60;


            GUILayout.BeginHorizontal();
            if (0 != spacing)
            {
                GUILayout.Label("", GUILayout.Width(spacing));
            }
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label("", GUILayout.Width(labelmar));
            GUILayout.Label(label, Colorlabel, GUILayout.ExpandWidth(false));

            float newValue = HorizontalAlphaSlider(value, min, max);
            string valueString = value.ToString("N2");
            string newValueString = RGB ? newValue.ToString("N2") : newValue.ToString("N2");
            newValueString = GUILayout.TextField(newValueString, GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));
            if (newValueString != valueString)
            {
                if (float.TryParse(newValueString, out float parseResult))
                {
                    newValue = RGB ? parseResult : parseResult;
                }
            }
            GUILayout.EndHorizontal();
            return newValue;
        }

        internal static byte SliderColorR(string label, byte value, int spacing)
        {
            float labelmar = (GUIStyles.labelWidth) - 60;
            GUIStyle Colorlabel = new GUIStyle(GUI.skin.label);
            Colorlabel.padding = new RectOffset(0, 0, 0, 0);
            Colorlabel.margin = new RectOffset(0, 0, 0, 0);
            Colorlabel.normal.background = null;
            Colorlabel.alignment = TextAnchor.MiddleLeft;
            Colorlabel.fixedWidth = 60;

            GUILayout.BeginHorizontal();
            if (0 != spacing)
            {
                GUILayout.Label("", GUILayout.Width(spacing));
            }
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label("", GUILayout.Width(labelmar));
            GUILayout.Label(label, Colorlabel, GUILayout.ExpandWidth(false));

            byte newValue = (byte)HorizontalRedSlider(value, 0, 255);
            string valueString = value.ToString();
            string newValueString = GUILayout.TextField(newValue.ToString(), GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            if (newValueString != valueString)
            {
                if (byte.TryParse(newValueString, out byte parseResult))
                {
                    newValue = parseResult;
                }
            }
            return newValue;
        }

        internal static byte SliderColorG(string label, byte value, int spacing)
        {
            float labelmar = (GUIStyles.labelWidth) - 60;
            GUIStyle Colorlabel = new GUIStyle(GUI.skin.label);
            Colorlabel.padding = new RectOffset(0, 0, 0, 0);
            Colorlabel.margin = new RectOffset(0, 0, 0, 0);
            Colorlabel.normal.background = null;
            Colorlabel.alignment = TextAnchor.MiddleLeft;
            Colorlabel.fixedWidth = 60;

            GUILayout.BeginHorizontal();
            if (0 != spacing)
            {
                GUILayout.Label("", GUILayout.Width(spacing));
            }
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label("", GUILayout.Width(labelmar));
            GUILayout.Label(label, Colorlabel, GUILayout.ExpandWidth(false));

            byte newValue = (byte)HorizontalGreenSlider(value, 0, 255);
            string valueString = value.ToString();
            string newValueString = GUILayout.TextField(newValue.ToString(), GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            if (newValueString != valueString)
            {
                if (byte.TryParse(newValueString, out byte parseResult))
                {
                    newValue = parseResult;
                }
            }
            return newValue;
        }

        internal static byte SliderColorB(string label, byte value, int spacing)
        {
            float labelmar = (GUIStyles.labelWidth) - 60;
            GUIStyle Colorlabel = new GUIStyle(GUI.skin.label);
            Colorlabel.padding = new RectOffset(0, 0, 0, 0);
            Colorlabel.margin = new RectOffset(0, 0, 0, 0);
            Colorlabel.normal.background = null;
            Colorlabel.alignment = TextAnchor.MiddleLeft;
            Colorlabel.fixedWidth = 60;

            GUILayout.BeginHorizontal();
            if (0 != spacing)
            {
                GUILayout.Label("", GUILayout.Width(spacing));
            }
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label("", GUILayout.Width(labelmar));
            GUILayout.Label(label, Colorlabel, GUILayout.ExpandWidth(false));

            byte newValue = (byte)HorizontalBlueSlider(value, 0, 255);
            string valueString = value.ToString();
            string newValueString = GUILayout.TextField(newValue.ToString(), GUILayout.Width(GUIStyles.fontSize * 3f), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            if (newValueString != valueString)
            {
                if (byte.TryParse(newValueString, out byte parseResult))
                {
                    newValue = parseResult;
                }
            }
            return newValue;
        }

        internal static T Selection<T>(string label, T selected, T[] selection, Action<T> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            if (!enable)
            {
                GUI.enabled = false;
            }

            string[] selectionString = selection.Select(entry => entry.ToString()).ToArray();
            string[] localizedSelection = LocalizationManager.HasLocalization() ? selectionString.Select(text => LocalizationManager.Localized(text)).ToArray() : selectionString;
            int currentIndex = Array.IndexOf(selection, selected);
            if (-1 == columns)
            {
                columns = selection.Length;
            }

            int selectedIndex = GUILayout.SelectionGrid(currentIndex, localizedSelection, columns);
            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            if (selectedIndex == currentIndex)
            {
                return selected;
            }

            selected = (T)selection.GetValue(selectedIndex);
            onChanged?.Invoke(selected);
            return selected;
        }



        internal static TEnum Selection<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);

            bool previousEnabledState = GUI.enabled;
            if (!enable)
            {
                GUI.enabled = false;
            }

            string[] selection = Enum.GetNames(typeof(TEnum));
            string[] localizedSelection = LocalizationManager.HasLocalization() ? selection.Select(text => LocalizationManager.Localized(text)).ToArray() : selection;

            int currentIndex = Array.IndexOf(selection, selected.ToString());
            if (-1 == columns)
            {
                columns = selection.Length;
            }

            int selectedIndex = GUILayout.SelectionGrid(currentIndex, localizedSelection, columns);
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (selectedIndex == currentIndex)
            {
                return selected;
            }

            string selectedName = selection.GetValue(selectedIndex).ToString();
            selected = (TEnum)Enum.Parse(typeof(TEnum), selectedName);
            onChanged?.Invoke(selected);

            return selected;
        }

        internal static TEnum Selection2<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            bool newEnable = enable;
            if (onChangedEnable != null)
            {
                spacing = enableSpacing;
                newEnable = GUILayout.Toggle(enable, "", GUILayout.ExpandWidth(false));
            }

            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x - spacing));
            if (onChangedEnable != null && newEnable != enable)
            {
                onChangedEnable(newEnable);
                enable = newEnable;
            }
            bool previousEnabledState = GUI.enabled;
            if (!enable)
            {
                GUI.enabled = false;
            }

            string[] selection = Enum.GetNames(typeof(TEnum));
            string[] localizedSelection = LocalizationManager.HasLocalization() ? selection.Select(text => LocalizationManager.Localized(text)).ToArray() : selection;

            int currentIndex = Array.IndexOf(selection, selected.ToString());
            if (-1 == columns)
            {
                columns = selection.Length;
            }

            int selectedIndex = GUILayout.SelectionGrid(currentIndex, localizedSelection, columns);
            GUILayout.EndHorizontal();

            GUI.enabled = previousEnabledState;

            if (selectedIndex == currentIndex)
            {
                return selected;
            }

            string selectedName = selection.GetValue(selectedIndex).ToString();
            selected = (TEnum)Enum.Parse(typeof(TEnum), selectedName);
            onChanged?.Invoke(selected);

            return selected;
        }

        internal static TEnum SelectionNormals<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null) where TEnum : Enum
        {
            GUILayout.BeginHorizontal();

            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);

            GUI.enabled = enable;
            string[] selection = Enum.GetNames(typeof(TEnum));
            string[] localizedSelection = LocalizationManager.HasLocalization() ? selection.Select(text => LocalizationManager.Localized(text)).ToArray() : selection;

            int currentIndex = Array.IndexOf(selection, selected.ToString());
            if (columns == -1)
            {
                columns = localizedSelection.Length;
            }

            bool isDeferredRendering = Graphics.Instance.CameraSettings.RenderingPath != CameraSettings.AIRenderingPath.Deferred;

            GUILayout.BeginVertical();
            for (int i = 0; i < localizedSelection.Length; i += columns)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < columns && i + j < localizedSelection.Length; j++)
                {
                    int index = i + j;
                    bool disableButton;
                    if (isDeferredRendering)
                    {
                        disableButton = (selection[index] == "GBuffer" || selection[index] == "OctaEncoded");
                    }
                    else
                    {
                        disableButton = (selection[index] == "Camera" || selection[index] == "None");
                    }

                    GUI.enabled = !disableButton && enable;

                    bool isSelected = currentIndex == index;
                    bool newSelected = GUILayout.Toggle(isSelected, localizedSelection[index], "Button");

                    if (newSelected && !isSelected)
                    {
                        selected = (TEnum)Enum.Parse(typeof(TEnum), selection[index]);
                        onChanged?.Invoke(selected);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUI.enabled = true;
            GUILayout.EndHorizontal();

            return selected;
        }

        internal static TEnum SelectionApply<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null) where TEnum : Enum
        {
            GUILayout.BeginHorizontal();

            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);

            GUI.enabled = enable;
            string[] selection = Enum.GetNames(typeof(TEnum));
            string[] localizedSelection = LocalizationManager.HasLocalization() ? selection.Select(text => LocalizationManager.Localized(text)).ToArray() : selection;

            int currentIndex = Array.IndexOf(selection, selected.ToString());
            if (columns == -1)
            {
                columns = localizedSelection.Length;
            }

            bool isDeferredRendering = Graphics.Instance.CameraSettings.RenderingPath == CameraSettings.AIRenderingPath.Deferred;

            GUILayout.BeginVertical();
            for (int i = 0; i < localizedSelection.Length; i += columns)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < columns && i + j < localizedSelection.Length; j++)
                {
                    int index = i + j;
                    bool disableButton;

                    if (isDeferredRendering)
                    {
                        disableButton = false;
                    }
                    else
                    {
                        disableButton = (selection[index] == "Deferred");
                    }

                    GUI.enabled = !disableButton && enable;

                    bool isSelected = currentIndex == index;
                    bool newSelected = GUILayout.Toggle(isSelected, localizedSelection[index], "Button");

                    if (newSelected && !isSelected)
                    {
                        selected = (TEnum)Enum.Parse(typeof(TEnum), selection[index]);
                        onChanged?.Invoke(selected);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            return selected;
        }

        internal static TEnum SelectionSSR<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null) where TEnum : Enum
        {
            GUILayout.BeginHorizontal();

            int spacing = 0;
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x - spacing));
            //EnableToggle(label, ref spacing, ref enable, onChangedEnable);

            GUI.enabled = enable;
            string[] selection = Enum.GetNames(typeof(TEnum));
            string[] localizedSelection = LocalizationManager.HasLocalization() ? selection.Select(text => LocalizationManager.Localized(text)).ToArray() : selection;

            int currentIndex = Array.IndexOf(selection, selected.ToString());
            if (columns == -1)
            {
                columns = localizedSelection.Length;
            }

            bool isDeferredRendering = Graphics.Instance.CameraSettings.RenderingPath == CameraSettings.AIRenderingPath.Deferred;

            GUILayout.BeginVertical();
            for (int i = 0; i < localizedSelection.Length; i += columns)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < columns && i + j < localizedSelection.Length; j++)
                {
                    int index = i + j;
                    bool disableButton;

                    if (isDeferredRendering)
                    {
                        disableButton = false;
                    }
                    else
                    {
                        disableButton = (selection[index] == "StochasticSSR");
                    }

                    GUI.enabled = !disableButton && enable;

                    bool isSelected = currentIndex == index;
                    bool newSelected = GUILayout.Toggle(isSelected, localizedSelection[index], "Button");

                    if (newSelected && !isSelected)
                    {
                        selected = (TEnum)Enum.Parse(typeof(TEnum), selection[index]);
                        onChanged?.Invoke(selected);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            return selected;
        }

        internal static TEnum SelectionAO<TEnum>(string label, TEnum selected, Action<TEnum> onChanged = null, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null) where TEnum : Enum
        {
            GUILayout.BeginHorizontal();

            int spacing = 0;
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x - spacing));
            //EnableToggle(label, ref spacing, ref enable, onChangedEnable);

            GUI.enabled = enable;
            string[] selection = Enum.GetNames(typeof(TEnum));
            string[] localizedSelection = LocalizationManager.HasLocalization() ? selection.Select(text => LocalizationManager.Localized(text)).ToArray() : selection;

            int currentIndex = Array.IndexOf(selection, selected.ToString());
            if (columns == -1)
            {
                columns = localizedSelection.Length;
            }

            bool isDeferredRendering = Graphics.Instance.CameraSettings.RenderingPath == CameraSettings.AIRenderingPath.Deferred;

            GUILayout.BeginVertical();
            for (int i = 0; i < localizedSelection.Length; i += columns)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < columns && i + j < localizedSelection.Length; j++)
                {
                    int index = i + j;
                    bool disableButton;

                    if (isDeferredRendering)
                    {
                        disableButton = false;
                    }
                    else
                    {
                        disableButton = (selection[index] == "GTAO");
                    }

                    GUI.enabled = !disableButton && enable;

                    bool isSelected = currentIndex == index;
                    bool newSelected = GUILayout.Toggle(isSelected, localizedSelection[index], "Button");

                    if (newSelected && !isSelected)
                    {
                        selected = (TEnum)Enum.Parse(typeof(TEnum), selection[index]);
                        onChanged?.Invoke(selected);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            return selected;
        }

        internal static int SelectionTexture(string label, int currentIndex, Texture[] selection, int columns = -1, bool enable = true, Action<bool> onChangedEnable = null, GUIStyle style = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            if (!enable)
            {
                GUI.enabled = false;
            }

            if (-1 == columns)
            {
                columns = selection.Length;
            }

            int selectedIndex = null == style ? GUILayout.SelectionGrid(currentIndex, selection, columns) : GUILayout.SelectionGrid(currentIndex, selection, columns, style);
            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            return selectedIndex;
        }

        internal static void SelectionMask(string label, int cullingMask, Action<int> onChanged = null, int columns = 4)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            int newMask = cullingMask;
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));
            int included = 0;
            GUILayout.BeginVertical();
            for (int i = 0; i < CullingMaskExtensions.Layers.Count; i++)
            {
                string layer = CullingMaskExtensions.Layers[i];
                bool include = CullingMaskExtensions.LayerCullingIncludes(newMask, layer);
                if (0 == (i % columns))
                    GUILayout.BeginHorizontal();
                include = GUILayout.Toggle(include, layer, GUIStyles.Skin.button);
                included++;
                newMask = CullingMaskExtensions.LayerCullingToggle(newMask, layer, include);
                if (included == columns)
                {
                    GUILayout.EndHorizontal();
                    included = 0;
                }
            }
            if (0 != included)
                GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (newMask != cullingMask)
                onChanged(newMask);
        }

        internal static TEnum Toolbar<TEnum>(TEnum selected) where TEnum : Enum
        {
            GUILayout.BeginHorizontal();

            string[] enumNames = Enum.GetNames(typeof(TEnum));
            TEnum[] enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
            string[] displayNames;

            if (typeof(TEnum) == typeof(Inspector.Tab))
            {
                displayNames = enumValues.Cast<Inspector.Tab>()
                         .Select(value =>
                         {
                             string key = Inspector.DisplayNames[value];
                             return LocalizationManager.HasLocalization() ? LocalizationManager.Localized(key) : key;
                         })
                .ToArray();
            }
            else
            {
                displayNames = enumNames;
            }

            int currentIndex = Array.IndexOf(enumNames, selected.ToString());

            GUIStyle buttonStyle = GUIStyles.toolbarbutton;
            GUIStyle activeButtonStyle = new GUIStyle(buttonStyle)
            {
                normal = buttonStyle.onNormal,
                hover = buttonStyle.onHover,
                active = buttonStyle.onActive,
                focused = buttonStyle.onFocused
            };

            float[] buttonWidths = new float[displayNames.Length];

            float totalTextLength = 0f;
            Vector2[] textSizes = new Vector2[displayNames.Length];

            for (int i = 0; i < displayNames.Length; i++)
            {
                Vector2 size = buttonStyle.CalcSize(new GUIContent(displayNames[i]));
                totalTextLength += size.x;
                textSizes[i] = size;
            }

            float availableWidth = Graphics.ConfigWindowWidth.Value;

            for (int i = 0; i < displayNames.Length; i++)
            {
                float proportion = textSizes[i].x / totalTextLength;
                buttonWidths[i] = Mathf.Max(proportion * availableWidth, textSizes[i].x + 20);
            }

            for (int i = 0; i < displayNames.Length; i++)
            {
                GUIStyle style = (i == currentIndex) ? activeButtonStyle : buttonStyle;
                if (GUILayout.Button(displayNames[i], style, GUILayout.Width(buttonWidths[i])))
                {
                    currentIndex = i;
                }
            }

            GUILayout.EndHorizontal();

            if (currentIndex != Array.IndexOf(enumNames, selected.ToString()))
            {
                string selectedName = enumNames[currentIndex];
                selected = (TEnum)Enum.Parse(typeof(TEnum), selectedName);
            }

            return selected;
        }

        internal static void Toggle(string label, bool toggle, bool bold = false, Action<bool> onChanged = null)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.BeginHorizontal();
            if (bold)
            {
                GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
                GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUIStyles.boldlabel.CalcSize(new GUIContent(label)).x));
            }
            else
            {
                GUILayout.Label(label, GUILayout.ExpandWidth(false));
                GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));
            }
            bool newToggle = GUILayout.Toggle(toggle, "");
            GUILayout.EndHorizontal();
            if (onChanged != null && toggle != newToggle)
            {
                onChanged(newToggle);
            }
        }

        internal static void ToggleAlt(string label, bool toggle, bool bold = false, Action<bool> onChanged = null)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.BeginHorizontal();

            //Draw checkbox
            bool newToggle = GUILayout.Toggle(toggle, "", GUILayout.Width(20)); // Checkbox width

            //Draw label
            if (bold)
            {
                GUIStyle boldStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(7, 7, 7, 7),
                    //margin = new RectOffset(4, 4, 6, 6),
                    padding = new RectOffset(0, 0, 5, 7),
                    overflow = new RectOffset(0, 0, 0, 0)
                };
                GUILayout.Label(label, boldStyle, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUIStyle normalStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Normal,
                    border = new RectOffset(7, 7, 7, 7),
                    //margin = new RectOffset(4, 4, 6, 6),
                    padding = new RectOffset(0, 0, 5, 7),
                    overflow = new RectOffset(0, 0, 0, 0)
                };
                GUILayout.Label(label, normalStyle, GUILayout.ExpandWidth(true));
            }

            GUILayout.EndHorizontal();

            if (onChanged != null && toggle != newToggle)
            {
                onChanged(newToggle);
            }
        }

        internal static void Switch(int fontsize, string label, bool toggle, bool bold = true, Action<bool> onChanged = null)
        {
            GUIStyle featureSwitch = GUIStyles.fswitch;
            featureSwitch.fixedWidth = fontsize * 4.91f;
            featureSwitch.fixedHeight = fontsize * 2.5f;

            GUIStyle switchlabel = GUIStyles.switchlabel;
            switchlabel.margin = new RectOffset(0, 0, 0, 0); //newSkin.label.margin;
            switchlabel.alignment = TextAnchor.MiddleCenter;
            switchlabel.fixedWidth = 0;
            switchlabel.fixedHeight = fontsize * 2.5f;
            switchlabel.stretchWidth = true;
            switchlabel.stretchHeight = true;

            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;

            GUILayout.BeginHorizontal();
            bool newToggle = GUILayout.Toggle(toggle, "", GUIStyles.fswitch);
            if (onChanged != null && toggle != newToggle)
            {
                onChanged(newToggle);
            }
            if (bold)
            {
                GUILayout.Label(label, switchlabel, GUILayout.ExpandWidth(false));

            }
            else
            {
                GUILayout.Label(label, GUILayout.ExpandWidth(false));

            }

            GUILayout.EndHorizontal();

        }

        internal static bool ToggleButton(string label, bool toggle, bool UseButton = false)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            return UseButton ? GUILayout.Toggle(toggle, label, GUIStyles.Skin.button) : GUILayout.Toggle(toggle, label);
        }

        internal static bool LightButton(string label, bool toggle, bool UseButton = false, int maxLength = 24)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;

            // Обрезаем label, если его длина превышает maxLength
            if (label.Length > maxLength)
            {
                label = label.Substring(0, maxLength) + "..."; // Добавляем многоточие в конце
            }

            return UseButton ? GUILayout.Toggle(toggle, label, GUIStyles.lightbutton) : GUILayout.Toggle(toggle, label);
        }

        internal static bool Button(string label, bool ExpandWidth = false)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            return GUILayout.Button(label, GUILayout.ExpandWidth(ExpandWidth));
        }

        internal static void Label(string label, string text, bool bold = false)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            if (0 < text.Length) text = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(text) : text;
            GUILayout.BeginHorizontal();
            if (bold)
            {
                GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
            }
            else
            {
                GUILayout.Label(label, GUILayout.ExpandWidth(false));
            }
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));
            GUILayout.Label(text);
            GUILayout.EndHorizontal();
        }

        internal static void LabelColorRed(string label, string text, bool bold = false)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            if (0 < text.Length) text = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(text) : text;
            GUILayout.BeginHorizontal();

            GUIStyle labelStyle = bold ? new GUIStyle(GUIStyles.boldlabel) : new GUIStyle(GUI.skin.label);

            labelStyle.normal.textColor = new Color(0.75f, 0.4f, 0.4f);


            GUILayout.Label(label, labelStyle, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));
            GUILayout.Label(text);
            GUILayout.EndHorizontal();
        }

        internal static void Text(string label, int Integer, Action<int> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            if (!enable)
            {
                GUI.enabled = false;
            }

            int.TryParse(GUILayout.TextField(Integer.ToString()), out int count);
            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            if (onChanged != null && Integer != count)
            {
                onChanged(count);
            }
        }

        internal static void Text(string label, float Float, string format = "N0", Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            if (!enable)
            {
                GUI.enabled = false;
            }

            float.TryParse(GUILayout.TextField(Float.ToString(format)), out float count);
            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            if (onChanged != null && Float != count)
            {
                onChanged(count);
            }
        }

        internal static void Text(string label, string text, Action<string> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));
            if (!enable)
            {
                GUI.enabled = false;
            }
            string newText = GUILayout.TextField(text, 32);
            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            if (onChanged != null && text != newText)
            {
                onChanged(newText);
            }
        }

        internal static void TextSearch(string label, string text, Action<string> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            string placeholder = LocalizationManager.HasLocalization() ? LocalizationManager.Localized("Search Presets...") : "Search Presets...";
            GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));

            if (!enable)
            {
                GUI.enabled = false;
            }

            Rect textFieldRect = GUILayoutUtility.GetRect(new GUIContent(placeholder), GUIStyles.Skin.textField, GUILayout.ExpandWidth(true));
            bool isMouseOver = textFieldRect.Contains(Event.current.mousePosition);
            bool hasFocus = GUI.GetNameOfFocusedControl() == "TextField";

            string displayText = !string.IsNullOrEmpty(text) || isMouseOver || hasFocus ? text : placeholder;

            string newText = GUI.TextField(textFieldRect, displayText, 32, GUIStyles.Skin.textField);

            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();

            if (onChanged != null && text != newText)
            {
                onChanged(newText == placeholder ? "" : newText);
            }

            if (hasFocus)
            {
                GUI.FocusControl("TextField");
            }
        }

        internal static void TextSave(string label, string text, Action<string> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            string placeholder = LocalizationManager.HasLocalization() ? LocalizationManager.Localized("Preset Name...") : "Preset Name...";
            GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));

            if (!enable)
            {
                GUI.enabled = false;
            }

            Rect textFieldRect = GUILayoutUtility.GetRect(new GUIContent(placeholder), GUIStyles.Skin.textField, GUILayout.ExpandWidth(true));
            bool isMouseOver = textFieldRect.Contains(Event.current.mousePosition);
            bool hasFocus = GUI.GetNameOfFocusedControl() == "TextField";

            string displayText = !string.IsNullOrEmpty(text) || isMouseOver || hasFocus ? text : placeholder;

            string newText = GUI.TextField(textFieldRect, displayText, 32, GUIStyles.Skin.textField);

            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();

            if (onChanged != null && text != newText)
            {
                onChanged(newText == placeholder ? "" : newText);
            }

            if (hasFocus)
            {
                GUI.FocusControl("TextField");
            }
        }

        internal static Vector2 Dimension(string label, Vector2 size, Action<Vector2> onChanged = null)
        {
            GUILayout.BeginHorizontal();
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));
            GUILayout.Label("X", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.x.ToString("N2")), out float x);
            GUILayout.Label("Y", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.y.ToString("N2")), out float y);
            Vector2 newSize = size;
            if (x != size.x || y != size.y)
            {
                newSize = new Vector2(x, y);
                onChanged?.Invoke(newSize);
            }
            GUILayout.EndHorizontal();
            return newSize;
        }

        internal static Vector3 Dimension(string label, Vector3 size, Action<Vector3> onChanged = null)
        {
            GUILayout.BeginHorizontal();
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));
            GUILayout.Label("X", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.x.ToString("N2")), out float x);
            GUILayout.Label("Y", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.y.ToString("N2")), out float y);
            GUILayout.Label("Z", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.z.ToString("N2")), out float z);
            Vector3 newSize = size;
            if (x != size.x || y != size.y || z != size.z)
            {
                newSize = new Vector3(x, y, z);
                onChanged?.Invoke(newSize);
            }
            GUILayout.EndHorizontal();
            return newSize;
        }

        internal static Vector4 Dimension(string label, Vector4 size, Action<Vector4> onChanged = null)
        {
            GUILayout.BeginHorizontal();
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x));
            GUILayout.Label("X", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.x.ToString()), out float x);
            GUILayout.Label("Y", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.y.ToString()), out float y);
            GUILayout.Label("Z", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.z.ToString()), out float z);
            GUILayout.Label("W", GUILayout.ExpandWidth(false));
            float.TryParse(GUILayout.TextField(size.w.ToString()), out float w);
            Vector4 newSize = size;
            if (x != size.x || y != size.y || z != size.z || w != size.w)
            {
                newSize = new Vector4(x, y, z, w);
                onChanged?.Invoke(newSize);
            }
            GUILayout.EndHorizontal();
            return newSize;
        }

        private static void EnableToggle(string label, ref int spacing, ref bool enable, Action<bool> onChangedEnable = null)
        {
            bool newEnable = enable;
            if (onChangedEnable != null)
            {
                spacing = enableSpacing;
                newEnable = GUILayout.Toggle(enable, "", GUILayout.ExpandWidth(false));
            }

            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x - spacing));
            if (onChangedEnable != null && newEnable != enable)
            {
                onChangedEnable(newEnable);
                enable = newEnable;
            }
        }
        internal static void TextInt(string label, int Integer, Action<int> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            //int spacing = 0;

            int.TryParse(GUILayout.TextField(Integer.ToString()), out int count);
            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            if (onChanged != null && Integer != count)
            {
                onChanged(count);
            }
        }

        internal static void TextFloat(string label, float Float, string format = "N0", Action<float> onChanged = null, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            //int spacing = 0;

            float.TryParse(GUILayout.TextField(Float.ToString(format)), out float count);
            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            if (onChanged != null && Float != count)
            {
                onChanged(count);
            }
        }

        internal static void Label1(string label, string text, bool bold = false)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            if (0 < text.Length) text = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(text) : text;
            GUILayout.BeginHorizontal();
            if (bold)
            {
                GUILayout.Label(label, GUIStyles.boldlabel);
            }
            else
            {
                GUILayout.Label(label);
            }
            GUILayout.Label("");
            GUILayout.Label(text);
            GUILayout.EndHorizontal();
        }
        internal static void Label2(string label, string text, bool bold = false)
        {
            label = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(label) : label;
            if (0 < text.Length) text = LocalizationManager.HasLocalization() ? LocalizationManager.Localized(text) : text;
            GUILayout.BeginHorizontal();
            if (bold)
            {
                GUILayout.Label(label, GUIStyles.boldlabel, GUILayout.ExpandWidth(false));
            }
            else
            {
                GUILayout.Label(label, GUILayout.ExpandWidth(false));
            }
            GUILayout.Label("", GUILayout.Width(GUIStyles.labelWidth - GUI.skin.label.CalcSize(new GUIContent(label)).x - 50));
            GUILayout.Label(text);
            GUILayout.EndHorizontal();
        }

        public static Light LightSelector(LightManager lightManager, string label, Light selected, Action<Light> onChanged = null, int columns = 3, bool enable = true, Action<bool> onChangedEnable = null)
        {
            GUILayout.BeginHorizontal();
            int spacing = 0;
            EnableToggle(label, ref spacing, ref enable, onChangedEnable);
            if (!enable)
            {
                GUI.enabled = false;
            }

            List<Light> selectionList = new List<Light>();

            lightManager.DirectionalLights.ForEach(dirLight =>
            {
                selectionList.Add(dirLight.Light);
            });


            Light[] selectionLights = selectionList.ToArray();
            int currentIndex = Array.IndexOf(selectionLights, selected);

            if (-1 == columns)
            {
                columns = selectionLights.Length;
            }

            int selectedIndex = GUILayout.SelectionGrid(currentIndex, selectionLights.Select(light => light.name).ToArray(), columns);
            if (!enable)
            {
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();
            if (selectedIndex == currentIndex)
            {
                return null;
            }

            UnityEngine.Light selectedLight = selectionLights[selectedIndex];
            onChanged?.Invoke(selectedLight);
            return selectedLight;
        }
    }
}
