﻿using KKAPI.Utilities;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Graphics.Settings;

namespace Graphics.Inspector
{
    internal static class GUIStyles
    {
        private static readonly Texture2D _boxNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _winNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _winOnNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _btnNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _btnOnNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _btnActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _btnOnActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _btnFocusedBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _btnNormalHoverBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _btnOnlHoverBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);

        private static readonly Texture2D _sliderHNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _sliderVNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _sliderThumbNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _sliderThumbActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _sliderThumbFocusedBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollHNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollHLNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollHLActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollHRNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollHRActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollVNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollVUNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollVUActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollVDNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollVDActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollHTNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _scrollVTNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toggleNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toggleOnNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toggleActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toggleOnActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toggleOnHoverBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toggleHoverBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _textNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _textFocusedBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toolbarbtnNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toolbarbtnOnNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toolbarbtnActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toolbarbtnOnActiveBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _toolbarbtnHoverBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _tabcontentNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);      
        private static readonly Texture2D _switchNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _switchNormalHoverBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _switchNormalActBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _switchOnBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _switchOnHoverBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _switchOnActBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _tabHeaderBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _tabSmallBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        private static readonly Texture2D _lightbutNormalBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);

        private static GUISkin _skin;
        private static int fontSize = 12;
        private static readonly string[] fonts = new string[] { "Lucida Grande", "Segoe UI", "Terminal" };
        //private static readonly string[] fonts = new string[] { "Times New Roman" };
        public static GUIStyle toolbarbutton;
        public static GUIStyle boldlabel;
        public static GUIStyle tabcontent;
        public static GUIStyle fswitch;
        public static GUIStyle switchlabel;
        public static GUIStyle label;
        public static GUIStyle tabheader;
        public static GUIStyle tabsmall;
        public static GUIStyle sliderfill;
        public static GUIStyle lightbutton;
        public static float labelWidth = 150f;



        public static GUISkin Skin
        {
            get
            {
                if (_skin == null)
                {
                    try
                    {
                        _skin = CreateSkin();
                    }
                    catch (Exception ex)
                    {
                        Graphics.Instance.Log.Log(BepInEx.Logging.LogLevel.Fatal, "Could not load custom GUISkin - " + ex.Message);
                        _skin = GUI.skin;
                    }
                }

                return _skin;
            }
        }

        public static int FontSize
        {
            get => fontSize;
            set
            {
                // Need to limit fontSize to prevent permantly breaking GUI from loading preset values.
                fontSize = Math.Max(Math.Min(24, value), 12);
                labelWidth = fontSize * 15f;
                Font font = Font.CreateDynamicFontFromOSFont(fonts, fontSize);
                if (_skin != null)
                {
                    _skin.font = font;
                }
            }
        }

        private static void LoadImage(Texture2D texture, byte[] tex)
        {
            ImageConversion.LoadImage(texture, tex);
            texture.anisoLevel = 1;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
        }

        private static GUISkin CreateSkin()
        {
            GUISkin newSkin = Object.Instantiate(GUI.skin);
            Object.DontDestroyOnLoad(newSkin);
            Font font = Font.CreateDynamicFontFromOSFont(fonts, fontSize);
            newSkin.font = font;

            // Load the custom skin from resources
            byte[] texData = ResourceUtils.GetEmbeddedResource("box.png");
            LoadImage(_boxNormalBackground, texData);
            Object.DontDestroyOnLoad(_boxNormalBackground);
            newSkin.box.normal.background = _boxNormalBackground;
            newSkin.box.normal.textColor = Color.black;
            newSkin.box.border = new RectOffset(3, 3, 2, 2);
            newSkin.box.margin = new RectOffset(0, 0, 0, 0);
            newSkin.box.padding = new RectOffset(Mathf.RoundToInt(fontSize * 0.2f), Mathf.RoundToInt(fontSize * 0.2f), Mathf.RoundToInt(fontSize * 0.2f), Mathf.RoundToInt(fontSize * 0.2f));
            newSkin.box.overflow = new RectOffset(0, 0, 0, 0);
            newSkin.box.font = null;
            newSkin.box.fontSize = 0;
            newSkin.box.fontStyle = FontStyle.Normal;
            newSkin.box.alignment = TextAnchor.UpperCenter;
            newSkin.box.wordWrap = true;
            newSkin.box.richText = false;
            newSkin.box.clipping = TextClipping.Clip;
            newSkin.box.imagePosition = ImagePosition.ImageLeft;
            newSkin.box.contentOffset = new Vector2(0, 0);
            newSkin.box.fixedWidth = 0;
            newSkin.box.fixedHeight = 0;
            newSkin.box.stretchWidth = false;
            newSkin.box.stretchHeight = false;


            texData = ResourceUtils.GetEmbeddedResource("PopupWindowOff.png");
            LoadImage(_winNormalBackground, texData);
            Object.DontDestroyOnLoad(_winNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("PopupWindowOn.png");
            LoadImage(_winOnNormalBackground, texData);
            Object.DontDestroyOnLoad(_winOnNormalBackground);
            newSkin.window.normal.background = _winNormalBackground;
            newSkin.window.normal.textColor = new Color32(180, 180, 180, 255);
            newSkin.window.onNormal.background = _winOnNormalBackground;
            newSkin.window.onNormal.textColor = new Color32(180, 180, 180, 255);
            newSkin.window.border = new RectOffset(10, 10, 10, 10);
            newSkin.window.margin = new RectOffset(0, 0, 0, 0);
            newSkin.window.padding = new RectOffset(0, 0, 20, 5);
            newSkin.window.overflow = new RectOffset(8, 8, 5, 12);
            newSkin.window.font = font;
            newSkin.window.fontSize = 0;
            newSkin.window.fontStyle = FontStyle.Normal;
            newSkin.window.alignment = TextAnchor.UpperCenter;
            newSkin.window.wordWrap = false;
            newSkin.window.richText = false;
            newSkin.window.clipping = TextClipping.Clip;
            newSkin.window.imagePosition = ImagePosition.ImageLeft;
            newSkin.window.contentOffset = new Vector2(0, -18);
            newSkin.window.fixedWidth = 0;
            newSkin.window.fixedHeight = 0;
            newSkin.window.stretchWidth = true;
            newSkin.window.stretchHeight = true;

            texData = ResourceUtils.GetEmbeddedResource("toggle.png");
            LoadImage(_toggleNormalBackground, texData);
            Object.DontDestroyOnLoad(_toggleNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("toggle on.png");
            LoadImage(_toggleOnNormalBackground, texData);
            Object.DontDestroyOnLoad(_toggleOnNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("toggle act.png");
            LoadImage(_toggleActiveBackground, texData);
            Object.DontDestroyOnLoad(_toggleActiveBackground);
            texData = ResourceUtils.GetEmbeddedResource("toggle on act.png");
            LoadImage(_toggleOnActiveBackground, texData);
            Object.DontDestroyOnLoad(_toggleOnActiveBackground);
            texData = ResourceUtils.GetEmbeddedResource("toggle on hover.png");
            LoadImage(_toggleOnHoverBackground, texData);
            Object.DontDestroyOnLoad(_toggleHoverBackground);
            texData = ResourceUtils.GetEmbeddedResource("toggle hover.png");
            LoadImage(_toggleHoverBackground, texData);
            Object.DontDestroyOnLoad(_toggleHoverBackground);
            newSkin.toggle.normal.background = _toggleNormalBackground;
            newSkin.toggle.normal.textColor = new Color32(180, 180, 180, 255);
            newSkin.toggle.onNormal.background = _toggleOnNormalBackground;
            newSkin.toggle.onNormal.textColor = new Color32(180, 180, 180, 255);
            newSkin.toggle.hover.background = _toggleHoverBackground;
            newSkin.toggle.hover.textColor = new Color32(180, 180, 180, 255);
            newSkin.toggle.onHover.background = _toggleOnHoverBackground;
            newSkin.toggle.onHover.textColor = new Color32(180, 180, 180, 255);
            newSkin.toggle.active.background = _toggleActiveBackground;
            newSkin.toggle.active.textColor = new Color32(180, 180, 180, 255);
            newSkin.toggle.onActive.background = _toggleOnActiveBackground;
            newSkin.toggle.onActive.textColor = new Color32(180, 180, 180, 255);
            newSkin.toggle.focused.background = null;
            newSkin.toggle.focused.textColor = new Color32(180, 180, 180, 255);
            newSkin.toggle.onFocused.background = null;
            newSkin.toggle.onFocused.textColor = new Color32(180, 180, 180, 255);
            newSkin.toggle.border = new RectOffset(24, 0, 24, 0);
            newSkin.toggle.margin = new RectOffset(4, 14, 2, 2);//new RectOffset(4, 4, 3, 2);
            newSkin.toggle.padding = new RectOffset(15, 3, 1, 2);
            newSkin.toggle.overflow = new RectOffset(0, 0, -3, 1);
            newSkin.toggle.alignment = TextAnchor.MiddleCenter;
            newSkin.toggle.wordWrap = false;
            newSkin.toggle.richText = false;
            newSkin.toggle.clipping = TextClipping.Clip;
            newSkin.toggle.imagePosition = ImagePosition.ImageLeft;
            newSkin.toggle.contentOffset = new Vector2(0, 0);
            newSkin.toggle.fixedWidth = 0;
            newSkin.toggle.fixedHeight = 0;
            newSkin.toggle.stretchWidth = true;
            newSkin.toggle.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("btn.png");
            LoadImage(_btnNormalBackground, texData);
            Object.DontDestroyOnLoad(_btnNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("btn on.png");
            LoadImage(_btnOnNormalBackground, texData);
            Object.DontDestroyOnLoad(_btnOnNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("btn act.png");
            LoadImage(_btnActiveBackground, texData);
            Object.DontDestroyOnLoad(_btnActiveBackground);
            texData = ResourceUtils.GetEmbeddedResource("btn onact.png");
            LoadImage(_btnOnActiveBackground, texData);
            Object.DontDestroyOnLoad(_btnOnActiveBackground);
            texData = ResourceUtils.GetEmbeddedResource("btn hover.png");
            LoadImage(_btnNormalHoverBackground, texData);
            Object.DontDestroyOnLoad(_btnNormalHoverBackground);
            texData = ResourceUtils.GetEmbeddedResource("btn on hover.png");
            LoadImage(_btnOnlHoverBackground, texData);
            Object.DontDestroyOnLoad(_btnOnlHoverBackground);
            newSkin.button.normal.background = _btnNormalBackground;
            newSkin.button.normal.textColor = new Color32(180, 180, 180, 255);
            newSkin.button.hover.background = _btnNormalHoverBackground;
            newSkin.button.hover.textColor = Color.white;
            newSkin.button.onHover.background = _btnOnlHoverBackground;
            newSkin.button.onHover.textColor = Color.white;
            newSkin.button.onNormal.background = _btnOnNormalBackground;
            newSkin.button.onNormal.textColor = new Color32(240, 240, 240, 255);
            newSkin.button.active.background = _btnActiveBackground;
            newSkin.button.active.textColor = new Color32(180, 180, 180, 255);
            newSkin.button.onActive.background = _btnOnActiveBackground;
            newSkin.button.onActive.textColor = new Color32(180, 180, 180, 255);
            newSkin.button.focused.background = null;
            newSkin.button.focused.textColor = new Color32(180, 180, 180, 255);
            newSkin.button.onFocused.background = null;
            newSkin.button.onFocused.textColor = new Color32(180, 180, 180, 255);
            newSkin.button.border = new RectOffset(6, 6, 4, 4);
            newSkin.button.margin = new RectOffset(3, 3, 3, 3);
            newSkin.button.padding = new RectOffset(6, 6, 7, 9);
            newSkin.button.overflow = new RectOffset(0, 0, -1, 2);
            newSkin.button.imagePosition = ImagePosition.ImageLeft;
            newSkin.button.alignment = TextAnchor.MiddleCenter;
            newSkin.button.contentOffset = new Vector2(0, 0);
            newSkin.button.stretchWidth = true;
            newSkin.button.stretchHeight = false;
            newSkin.button.fontStyle = FontStyle.Bold;

            newSkin.label.normal.textColor = new Color32(180, 180, 180, 255);
            newSkin.label.hover.textColor = Color.black;
            newSkin.label.active.textColor = Color.black;
            newSkin.label.focused.textColor = Color.black;
            newSkin.label.border = new RectOffset(0, 0, 0, 0);
            newSkin.label.margin = new RectOffset(4, 4, 2, 2); //new RectOffset(4, 4, 2, 2);
            newSkin.label.padding = new RectOffset(2, 2, 1, 2); //new RectOffset(2, 2, 1, 2);
            newSkin.label.overflow = new RectOffset(0, 0, 0, 0); //new RectOffset(2, 2, 1, 2);
            newSkin.label.fontStyle = FontStyle.Normal;
            newSkin.label.alignment = TextAnchor.UpperLeft;
            newSkin.label.wordWrap = false;
            newSkin.label.richText = false;
            newSkin.label.clipping = TextClipping.Clip;
            newSkin.label.imagePosition = ImagePosition.ImageLeft;
            newSkin.label.contentOffset = new Vector2(0, 0);
            newSkin.label.fixedWidth = 0;
            newSkin.label.fixedHeight = 0;
            newSkin.label.stretchWidth = true;
            newSkin.label.stretchHeight = true;

            texData = ResourceUtils.GetEmbeddedResource("TextField.png");
            LoadImage(_textNormalBackground, texData);
            Object.DontDestroyOnLoad(_textNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("TextField focused.png");
            LoadImage(_textFocusedBackground, texData);
            Object.DontDestroyOnLoad(_textFocusedBackground);
            newSkin.textField.normal.background = _textNormalBackground;
            newSkin.textField.normal.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.hover.background = null;
            newSkin.textField.hover.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.onNormal.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.focused.background = _textNormalBackground;
            newSkin.textField.focused.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.onFocused.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.hover.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.onHover.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.active.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.onActive.textColor = new Color32(180, 180, 180, 255);
            newSkin.textField.border = new RectOffset(7, 7, 7, 7);
            newSkin.textField.margin = new RectOffset(4, 4, 6, 6);
            newSkin.textField.padding = new RectOffset(3, 3, 5, 7);
            newSkin.textField.overflow = new RectOffset(0, 0, 0, 0);
            newSkin.textField.font = null;
            newSkin.textField.fontSize = 0;
            newSkin.textField.fontStyle = FontStyle.Normal;
            newSkin.textField.alignment = TextAnchor.MiddleLeft;
            newSkin.textField.wordWrap = false;
            newSkin.textField.richText = false;
            newSkin.textField.clipping = TextClipping.Clip;
            newSkin.textField.imagePosition = ImagePosition.TextOnly;
            newSkin.textField.contentOffset = new Vector2(0, 0);
            newSkin.textField.fixedWidth = 0;
            newSkin.textField.fixedHeight = 0;
            newSkin.textField.stretchWidth = true;
            newSkin.textField.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("slider horiz.png");
            LoadImage(_sliderHNormalBackground, texData);
            Object.DontDestroyOnLoad(_sliderHNormalBackground);
            newSkin.horizontalSlider.normal.background = _sliderHNormalBackground;
            newSkin.horizontalSlider.normal.textColor = Color.black;
            newSkin.horizontalSlider.border = new RectOffset(3, 3, 0, 0);
            newSkin.horizontalSlider.margin = new RectOffset(4, 20, 8, 0);
            newSkin.horizontalSlider.padding = new RectOffset(-1, -1, 0, 0);
            newSkin.horizontalSlider.overflow = new RectOffset(0, 0, -7, -6);
            newSkin.horizontalSlider.font = null;
            newSkin.horizontalSlider.fontSize = 0;
            newSkin.horizontalSlider.fontStyle = FontStyle.Normal;
            newSkin.horizontalSlider.alignment = TextAnchor.MiddleLeft;
            newSkin.horizontalSlider.wordWrap = false;
            newSkin.horizontalSlider.richText = false;
            newSkin.horizontalSlider.imagePosition = ImagePosition.ImageOnly;
            newSkin.horizontalSlider.clipping = TextClipping.Clip;
            newSkin.horizontalSlider.contentOffset = new Vector2(0, 0);
            newSkin.horizontalSlider.fixedWidth = 0f;
            newSkin.horizontalSlider.fixedHeight = 18f;
            newSkin.horizontalSlider.stretchWidth = true;
            newSkin.horizontalSlider.stretchHeight = false;



            texData = ResourceUtils.GetEmbeddedResource("slider thumb.png");
            LoadImage(_sliderThumbNormalBackground, texData);
            Object.DontDestroyOnLoad(_sliderThumbNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("slider thumb act.png");
            LoadImage(_sliderThumbActiveBackground, texData);
            Object.DontDestroyOnLoad(_sliderThumbActiveBackground);
            texData = ResourceUtils.GetEmbeddedResource("slider thumb focus.png");
            LoadImage(_sliderThumbFocusedBackground, texData);
            Object.DontDestroyOnLoad(_sliderThumbFocusedBackground);
            newSkin.horizontalSliderThumb.normal.background = _sliderThumbNormalBackground;
            newSkin.horizontalSliderThumb.normal.textColor = Color.black;
            newSkin.horizontalSliderThumb.onNormal.textColor = Color.black;
            newSkin.horizontalSliderThumb.hover.background = null;
            newSkin.horizontalSliderThumb.hover.textColor = Color.black;
            newSkin.horizontalSliderThumb.onHover.background = null;
            newSkin.horizontalSliderThumb.onHover.textColor = Color.black;
            newSkin.horizontalSliderThumb.active.background = _sliderThumbActiveBackground;
            newSkin.horizontalSliderThumb.active.textColor = Color.black;
            newSkin.horizontalSliderThumb.onActive.textColor = Color.black;
            newSkin.horizontalSliderThumb.focused.background = _sliderThumbFocusedBackground;
            newSkin.horizontalSliderThumb.focused.textColor = Color.black;
            newSkin.horizontalSliderThumb.onFocused.textColor = Color.black;
            newSkin.horizontalSliderThumb.border = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalSliderThumb.margin = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalSliderThumb.padding = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalSliderThumb.overflow = new RectOffset(1, 1, -4, 4);
            newSkin.horizontalSliderThumb.font = null;
            newSkin.horizontalSliderThumb.fontSize = 0;
            newSkin.horizontalSliderThumb.fontStyle = FontStyle.Normal;
            newSkin.horizontalSliderThumb.alignment = TextAnchor.MiddleLeft;
            newSkin.horizontalSliderThumb.wordWrap = false;
            newSkin.horizontalSliderThumb.richText = false;
            newSkin.horizontalSliderThumb.imagePosition = ImagePosition.ImageOnly;
            newSkin.horizontalSliderThumb.clipping = TextClipping.Clip;
            newSkin.horizontalSliderThumb.contentOffset = new Vector2(0, 0);
            newSkin.horizontalSliderThumb.fixedWidth = 10f;
            newSkin.horizontalSliderThumb.fixedHeight = 12f;
            newSkin.horizontalSliderThumb.stretchWidth = true;
            newSkin.horizontalSliderThumb.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("scroll horiz.png");
            LoadImage(_scrollHNormalBackground, texData);
            Object.DontDestroyOnLoad(_scrollHNormalBackground);
            newSkin.horizontalScrollbar.normal.background = _scrollHNormalBackground;
            newSkin.horizontalScrollbar.normal.textColor = Color.black;
            newSkin.horizontalScrollbar.border = new RectOffset(25, 25, 0, 0);
            newSkin.horizontalScrollbar.margin = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbar.padding = new RectOffset(-1, -1, 0, 0);
            newSkin.horizontalScrollbar.overflow = new RectOffset(1, 1, 0, 0);
            newSkin.horizontalScrollbar.font = null;
            newSkin.horizontalScrollbar.fontSize = 0;
            newSkin.horizontalScrollbar.fontStyle = FontStyle.Normal;
            newSkin.horizontalScrollbar.alignment = TextAnchor.UpperLeft;
            newSkin.horizontalScrollbar.wordWrap = false;
            newSkin.horizontalScrollbar.richText = false;
            newSkin.horizontalScrollbar.imagePosition = ImagePosition.ImageOnly;
            newSkin.horizontalScrollbar.clipping = TextClipping.Clip;
            newSkin.horizontalScrollbar.contentOffset = new Vector2(0, 0);
            newSkin.horizontalScrollbar.fixedWidth = 0f;
            newSkin.horizontalScrollbar.fixedHeight = 15f;
            newSkin.horizontalScrollbar.stretchWidth = true;
            newSkin.horizontalScrollbar.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("scroll vert.png");
            LoadImage(_scrollVNormalBackground, texData);
            Object.DontDestroyOnLoad(_scrollVNormalBackground);
            newSkin.verticalScrollbar.normal.background = _scrollVNormalBackground;
            newSkin.verticalScrollbar.normal.textColor = Color.black;
            newSkin.verticalScrollbar.border = new RectOffset(0, 0, 9, 9);
            newSkin.verticalScrollbar.margin = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbar.padding = new RectOffset(0, 0, -1, -1);
            newSkin.verticalScrollbar.overflow = new RectOffset(0, 0, 1, 1);
            newSkin.verticalScrollbar.font = null;
            newSkin.verticalScrollbar.fontSize = 0;
            newSkin.verticalScrollbar.fontStyle = FontStyle.Normal;
            newSkin.verticalScrollbar.alignment = TextAnchor.UpperLeft;
            newSkin.verticalScrollbar.wordWrap = false;
            newSkin.verticalScrollbar.richText = false;
            newSkin.verticalScrollbar.imagePosition = ImagePosition.ImageLeft;
            newSkin.verticalScrollbar.clipping = TextClipping.Clip;
            newSkin.verticalScrollbar.contentOffset = new Vector2(0, 0);
            newSkin.verticalScrollbar.fixedWidth = 15f;
            newSkin.verticalScrollbar.fixedHeight = 0f;
            newSkin.verticalScrollbar.stretchWidth = true;
            newSkin.verticalScrollbar.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("scroll horiz thumb.png");
            LoadImage(_scrollHTNormalBackground, texData);
            Object.DontDestroyOnLoad(_scrollHTNormalBackground);
            newSkin.horizontalScrollbarThumb.normal.background = _scrollHTNormalBackground;
            newSkin.horizontalScrollbarThumb.normal.textColor = Color.black;
            newSkin.horizontalScrollbarThumb.border = new RectOffset(8, 8, 0, 0);
            newSkin.horizontalScrollbarThumb.margin = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbarThumb.padding = new RectOffset(8, 8, 0, 0);
            newSkin.horizontalScrollbarThumb.overflow = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbarThumb.font = null;
            newSkin.horizontalScrollbarThumb.fontSize = 0;
            newSkin.horizontalScrollbarThumb.fontStyle = FontStyle.Normal;
            newSkin.horizontalScrollbarThumb.alignment = TextAnchor.UpperLeft;
            newSkin.horizontalScrollbarThumb.wordWrap = false;
            newSkin.horizontalScrollbarThumb.richText = false;
            newSkin.horizontalScrollbarThumb.imagePosition = ImagePosition.ImageLeft;
            newSkin.horizontalScrollbarThumb.clipping = TextClipping.Clip;
            newSkin.horizontalScrollbarThumb.contentOffset = new Vector2(0, 0);
            newSkin.horizontalScrollbarThumb.fixedWidth = 0f;
            newSkin.horizontalScrollbarThumb.fixedHeight = 15f;
            newSkin.horizontalScrollbarThumb.stretchWidth = true;
            newSkin.horizontalScrollbarThumb.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("scroll vert thumb.png");
            LoadImage(_scrollVTNormalBackground, texData);
            Object.DontDestroyOnLoad(_scrollVTNormalBackground);
            newSkin.verticalScrollbarThumb.normal.background = _scrollVTNormalBackground;
            newSkin.verticalScrollbarThumb.normal.textColor = Color.black;
            newSkin.verticalScrollbarThumb.border = new RectOffset(0, 0, 8, 8);
            newSkin.verticalScrollbarThumb.margin = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbarThumb.padding = new RectOffset(0, 0, 10, 10);
            newSkin.verticalScrollbarThumb.overflow = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbarThumb.font = null;
            newSkin.verticalScrollbarThumb.fontSize = 0;
            newSkin.verticalScrollbarThumb.fontStyle = FontStyle.Normal;
            newSkin.verticalScrollbarThumb.alignment = TextAnchor.UpperLeft;
            newSkin.verticalScrollbarThumb.wordWrap = false;
            newSkin.verticalScrollbarThumb.richText = false;
            newSkin.verticalScrollbarThumb.imagePosition = ImagePosition.ImageOnly;
            newSkin.verticalScrollbarThumb.clipping = TextClipping.Clip;
            newSkin.verticalScrollbarThumb.contentOffset = new Vector2(0, 0);
            newSkin.verticalScrollbarThumb.fixedWidth = 15f;
            newSkin.verticalScrollbarThumb.fixedHeight = 0f;
            newSkin.verticalScrollbarThumb.stretchWidth = false;
            newSkin.verticalScrollbarThumb.stretchHeight = true;

            texData = ResourceUtils.GetEmbeddedResource("scroll horiz left.png");
            LoadImage(_scrollHLNormalBackground, texData);
            Object.DontDestroyOnLoad(_scrollHLNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("scroll horiz left act.png");
            LoadImage(_scrollHLActiveBackground, texData);
            Object.DontDestroyOnLoad(_scrollHLActiveBackground);
            newSkin.horizontalScrollbarLeftButton.normal.background = _scrollHLNormalBackground;
            newSkin.horizontalScrollbarLeftButton.normal.textColor = Color.black;
            newSkin.horizontalScrollbarLeftButton.active.background = _scrollHLActiveBackground;
            newSkin.horizontalScrollbarLeftButton.border = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbarLeftButton.margin = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbarLeftButton.padding = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbarLeftButton.overflow = new RectOffset(0, 8, 0, 0);
            newSkin.horizontalScrollbarLeftButton.font = null;
            newSkin.horizontalScrollbarLeftButton.fontSize = 0;
            newSkin.horizontalScrollbarLeftButton.fontStyle = FontStyle.Normal;
            newSkin.horizontalScrollbarLeftButton.alignment = TextAnchor.UpperLeft;
            newSkin.horizontalScrollbarLeftButton.wordWrap = false;
            newSkin.horizontalScrollbarLeftButton.richText = false;
            newSkin.horizontalScrollbarLeftButton.imagePosition = ImagePosition.ImageLeft;
            newSkin.horizontalScrollbarLeftButton.clipping = TextClipping.Clip;
            newSkin.horizontalScrollbarLeftButton.contentOffset = new Vector2(0, 0);
            newSkin.horizontalScrollbarLeftButton.fixedWidth = 17f;
            newSkin.horizontalScrollbarLeftButton.fixedHeight = 15f;
            newSkin.horizontalScrollbarLeftButton.stretchWidth = true;
            newSkin.horizontalScrollbarLeftButton.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("scroll horiz right.png");
            LoadImage(_scrollHRNormalBackground, texData);
            Object.DontDestroyOnLoad(_scrollHRNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("scroll horiz right act.png");
            LoadImage(_scrollHRActiveBackground, texData);
            Object.DontDestroyOnLoad(_scrollHRActiveBackground);
            newSkin.horizontalScrollbarRightButton.normal.background = _scrollHRNormalBackground;
            newSkin.horizontalScrollbarRightButton.normal.textColor = Color.black;
            newSkin.horizontalScrollbarRightButton.active.background = _scrollHRActiveBackground;
            newSkin.horizontalScrollbarRightButton.border = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbarRightButton.margin = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbarRightButton.padding = new RectOffset(0, 0, 0, 0);
            newSkin.horizontalScrollbarRightButton.overflow = new RectOffset(8, 0, 0, 0);
            newSkin.horizontalScrollbarRightButton.font = null;
            newSkin.horizontalScrollbarRightButton.fontSize = 0;
            newSkin.horizontalScrollbarRightButton.fontStyle = FontStyle.Normal;
            newSkin.horizontalScrollbarRightButton.alignment = TextAnchor.UpperLeft;
            newSkin.horizontalScrollbarRightButton.wordWrap = false;
            newSkin.horizontalScrollbarRightButton.richText = false;
            newSkin.horizontalScrollbarRightButton.imagePosition = ImagePosition.ImageLeft;
            newSkin.horizontalScrollbarRightButton.clipping = TextClipping.Clip;
            newSkin.horizontalScrollbarRightButton.contentOffset = new Vector2(0, 0);
            newSkin.horizontalScrollbarRightButton.fixedWidth = 17.24739f;
            newSkin.horizontalScrollbarRightButton.fixedHeight = 15f;
            newSkin.horizontalScrollbarRightButton.stretchWidth = true;
            newSkin.horizontalScrollbarRightButton.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("scroll vert up.png");
            LoadImage(_scrollVUNormalBackground, texData);
            Object.DontDestroyOnLoad(_scrollVUNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("scroll vert up act.png");
            LoadImage(_scrollVUActiveBackground, texData);
            Object.DontDestroyOnLoad(_scrollVUActiveBackground);
            newSkin.verticalScrollbarUpButton.normal.background = _scrollVUNormalBackground;
            newSkin.verticalScrollbarUpButton.normal.textColor = Color.black;
            newSkin.verticalScrollbarUpButton.active.background = _scrollVUActiveBackground;
            newSkin.verticalScrollbarUpButton.border = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbarUpButton.margin = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbarUpButton.padding = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbarUpButton.overflow = new RectOffset(0, 0, 0, 8);
            newSkin.verticalScrollbarUpButton.font = null;
            newSkin.verticalScrollbarUpButton.fontSize = 0;
            newSkin.verticalScrollbarUpButton.fontStyle = FontStyle.Normal;
            newSkin.verticalScrollbarUpButton.alignment = TextAnchor.UpperLeft;
            newSkin.verticalScrollbarUpButton.wordWrap = false;
            newSkin.verticalScrollbarUpButton.richText = false;
            newSkin.verticalScrollbarUpButton.imagePosition = ImagePosition.ImageLeft;
            newSkin.verticalScrollbarUpButton.clipping = TextClipping.Clip;
            newSkin.verticalScrollbarUpButton.contentOffset = new Vector2(0, 0);
            newSkin.verticalScrollbarUpButton.fixedWidth = 15f;
            newSkin.verticalScrollbarUpButton.fixedHeight = 17f;
            newSkin.verticalScrollbarUpButton.stretchWidth = true;
            newSkin.verticalScrollbarUpButton.stretchHeight = false;

            texData = ResourceUtils.GetEmbeddedResource("scroll vert down.png");
            LoadImage(_scrollVDNormalBackground, texData);
            Object.DontDestroyOnLoad(_scrollVDNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("scroll vert down act.png");
            LoadImage(_scrollVDActiveBackground, texData);
            Object.DontDestroyOnLoad(_scrollVDActiveBackground);
            newSkin.verticalScrollbarDownButton.normal.background = _scrollVDNormalBackground;
            newSkin.verticalScrollbarDownButton.normal.textColor = Color.black;
            newSkin.verticalScrollbarDownButton.active.background = _scrollVDActiveBackground;
            newSkin.verticalScrollbarDownButton.border = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbarDownButton.margin = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbarDownButton.padding = new RectOffset(0, 0, 0, 0);
            newSkin.verticalScrollbarDownButton.overflow = new RectOffset(0, 0, 8, 0);
            newSkin.verticalScrollbarDownButton.font = null;
            newSkin.verticalScrollbarDownButton.fontSize = 0;
            newSkin.verticalScrollbarDownButton.fontStyle = FontStyle.Normal;
            newSkin.verticalScrollbarDownButton.alignment = TextAnchor.UpperLeft;
            newSkin.verticalScrollbarDownButton.wordWrap = false;
            newSkin.verticalScrollbarDownButton.richText = false;
            newSkin.verticalScrollbarDownButton.imagePosition = ImagePosition.ImageLeft;
            newSkin.verticalScrollbarDownButton.clipping = TextClipping.Clip;
            newSkin.verticalScrollbarDownButton.contentOffset = new Vector2(0, 0);
            newSkin.verticalScrollbarDownButton.fixedWidth = 15f;
            newSkin.verticalScrollbarDownButton.fixedHeight = 17f;
            newSkin.verticalScrollbarDownButton.stretchWidth = true;
            newSkin.verticalScrollbarDownButton.stretchHeight = false;

            boldlabel = new GUIStyle
            {
                name = "boldlabel"
            };

            boldlabel.normal.textColor = newSkin.label.normal.textColor;
            boldlabel.border = newSkin.label.border;
            boldlabel.margin = new RectOffset(4, 4, 6, 4); //newSkin.label.margin;
            boldlabel.padding = newSkin.label.padding;
            boldlabel.overflow = newSkin.label.overflow;
            boldlabel.stretchWidth = newSkin.label.stretchWidth;
            boldlabel.stretchHeight = newSkin.label.stretchHeight;
            boldlabel.fontStyle = FontStyle.Bold;
            boldlabel.alignment = TextAnchor.MiddleCenter;
            boldlabel.wordWrap = false;
            boldlabel.richText = false;
            boldlabel.clipping = TextClipping.Clip;
            boldlabel.imagePosition = ImagePosition.ImageLeft;
            boldlabel.contentOffset = new Vector2(0, 0);
            boldlabel.fixedWidth = 0;
            boldlabel.fixedHeight = 0;
            boldlabel.stretchWidth = true;
            boldlabel.stretchHeight = false;


            toolbarbutton = new GUIStyle
            {
                name = "toolbarbutton"
            };
            texData = ResourceUtils.GetEmbeddedResource("toolbar button.png");
            LoadImage(_toolbarbtnNormalBackground, texData);
            Object.DontDestroyOnLoad(_toolbarbtnNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("toolbar button on.png");
            LoadImage(_toolbarbtnOnNormalBackground, texData);
            Object.DontDestroyOnLoad(_toolbarbtnOnNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("toolbar button act.png");
            LoadImage(_toolbarbtnActiveBackground, texData);
            Object.DontDestroyOnLoad(_toolbarbtnActiveBackground);
            texData = ResourceUtils.GetEmbeddedResource("toolbar button act on.png");
            LoadImage(_toolbarbtnOnActiveBackground, texData);
            Object.DontDestroyOnLoad(_toolbarbtnOnActiveBackground);
            texData = ResourceUtils.GetEmbeddedResource("toolbar button hov.png");
            LoadImage(_toolbarbtnHoverBackground, texData);
            Object.DontDestroyOnLoad(_toolbarbtnHoverBackground);
            toolbarbutton.normal.background = null;
            toolbarbutton.normal.textColor = new Color32(180, 180, 180, 255);
            toolbarbutton.hover.background = _toolbarbtnHoverBackground;
            toolbarbutton.hover.textColor = new Color32(255, 255, 255, 255);
            toolbarbutton.onHover.background = null;
            toolbarbutton.onHover.textColor = new Color32(255, 255, 255, 255);
            toolbarbutton.onNormal.background = _toolbarbtnOnNormalBackground;
            toolbarbutton.onNormal.textColor = new Color32(180, 180, 180, 255);
            toolbarbutton.active.background = _toolbarbtnActiveBackground;
            toolbarbutton.active.textColor = new Color32(180, 180, 180, 255);
            toolbarbutton.onActive.background = _toolbarbtnOnActiveBackground;
            toolbarbutton.onActive.textColor = new Color32(180, 180, 180, 255);
            toolbarbutton.focused.background = null;
            toolbarbutton.focused.textColor = Color.black;
            toolbarbutton.onFocused.background = null;
            toolbarbutton.onFocused.textColor = new Color32(180, 180, 180, 255);
            toolbarbutton.border = new RectOffset(12, 12, 9, 4);
            toolbarbutton.margin = new RectOffset(0, 0, 0, 0);
            toolbarbutton.padding = new RectOffset(5, 5, 5, 12);
            toolbarbutton.overflow = new RectOffset(0, 1, 0, 0);
            toolbarbutton.fontStyle = FontStyle.Bold;
            toolbarbutton.alignment = TextAnchor.MiddleCenter;
            toolbarbutton.wordWrap = false;
            toolbarbutton.richText = false;
            toolbarbutton.clipping = TextClipping.Clip;
            toolbarbutton.imagePosition = ImagePosition.ImageLeft;
            toolbarbutton.contentOffset = new Vector2(0, 0);
            toolbarbutton.fixedWidth = 0f;
            toolbarbutton.fixedHeight = 0f;
            toolbarbutton.stretchWidth = true;
            toolbarbutton.stretchHeight = false;

            tabcontent = new GUIStyle
            {
                name = "tabcontent"
            };

            texData = ResourceUtils.GetEmbeddedResource("tabcontent.png");
            LoadImage(_tabcontentNormalBackground, texData);
            Object.DontDestroyOnLoad(_tabcontentNormalBackground);
            tabcontent.normal.background = _tabcontentNormalBackground;
            tabcontent.normal.textColor = Color.black;
            tabcontent.border = new RectOffset(3, 3, 2, 2);
            tabcontent.margin = new RectOffset(0, 0, 0, 0);
            tabcontent.padding = new RectOffset(25, 25, 35, 3);
            tabcontent.overflow = new RectOffset(0, 0, 0, 0);
            tabcontent.font = null;
            tabcontent.fontSize = 0;
            tabcontent.fontStyle = FontStyle.Normal;
            tabcontent.alignment = TextAnchor.UpperCenter;
            tabcontent.wordWrap = true;
            tabcontent.richText = false;
            tabcontent.clipping = TextClipping.Clip;
            tabcontent.imagePosition = ImagePosition.ImageLeft;
            tabcontent.contentOffset = new Vector2(0, 0);
            tabcontent.fixedWidth = 0;
            tabcontent.fixedHeight = 0;
            tabcontent.stretchWidth = false;
            tabcontent.stretchHeight = true;

            tabheader = new GUIStyle
            {
                name = "tabheader"
            };

            texData = ResourceUtils.GetEmbeddedResource("tabheader.png");
            LoadImage(_tabHeaderBackground, texData);
            Object.DontDestroyOnLoad(_tabHeaderBackground);
            tabheader.normal.background = _tabHeaderBackground;
            tabheader.normal.textColor = Color.black;
            tabheader.border = new RectOffset(14, 14, 1, 16);
            tabheader.margin = new RectOffset(0, 0, 0, 0);
            tabheader.padding = new RectOffset(25, 25, 10, 20);
            tabheader.overflow = new RectOffset(0, 0, 0, 0);
            tabheader.font = null;
            tabheader.fontSize = 0;
            tabheader.fontStyle = FontStyle.Normal;
            tabheader.alignment = TextAnchor.UpperCenter;
            tabheader.wordWrap = true;
            tabheader.richText = false;
            tabheader.clipping = TextClipping.Clip;
            tabheader.imagePosition = ImagePosition.ImageLeft;
            tabheader.contentOffset = new Vector2(0, 0);
            tabheader.fixedWidth = 0;
            tabheader.fixedHeight = 0;
            tabheader.stretchWidth = false;
            tabheader.stretchHeight = true;

            tabsmall = new GUIStyle
            {
                name = "tabsmall"
            };

            texData = ResourceUtils.GetEmbeddedResource("tabsmall.png");
            LoadImage(_tabSmallBackground, texData);
            Object.DontDestroyOnLoad(_tabSmallBackground);
            tabsmall.normal.background = _tabSmallBackground;
            tabsmall.normal.textColor = Color.black;
            tabsmall.border = new RectOffset(2, 2, 2, 2);
            tabsmall.margin = new RectOffset(0, 0, 4, 4);
            tabsmall.padding = new RectOffset(Mathf.RoundToInt(fontSize * 1.5f), Mathf.RoundToInt(fontSize * 1.5f), Mathf.RoundToInt(fontSize * 1.5f), Mathf.RoundToInt(fontSize * 1.5f));
            tabsmall.overflow = new RectOffset(0, 0, 0, 0);
            tabsmall.font = null;
            tabsmall.fontSize = 0;
            tabsmall.fontStyle = FontStyle.Normal;
            tabsmall.alignment = TextAnchor.UpperCenter;
            tabsmall.wordWrap = true;
            tabsmall.richText = false;
            tabsmall.clipping = TextClipping.Clip;
            tabsmall.imagePosition = ImagePosition.ImageLeft;
            tabsmall.contentOffset = new Vector2(0, 0);
            tabsmall.fixedWidth = 0;
            tabsmall.fixedHeight = 0;
            tabsmall.stretchWidth = false;
            tabsmall.stretchHeight = true;

            fswitch = new GUIStyle
            {
                name = "fswitch"
            };

            texData = ResourceUtils.GetEmbeddedResource("switch.png");
            LoadImage(_switchNormalBackground, texData);
            Object.DontDestroyOnLoad(_switchNormalBackground);
            texData = ResourceUtils.GetEmbeddedResource("switch on.png");
            LoadImage(_switchOnBackground, texData);
            Object.DontDestroyOnLoad(_switchOnBackground);
            texData = ResourceUtils.GetEmbeddedResource("switch hover.png");
            LoadImage(_switchNormalHoverBackground, texData);
            Object.DontDestroyOnLoad(_switchNormalHoverBackground);
            texData = ResourceUtils.GetEmbeddedResource("switch act.png");
            LoadImage(_switchNormalActBackground, texData);
            Object.DontDestroyOnLoad(_switchNormalActBackground);
            texData = ResourceUtils.GetEmbeddedResource("switch on hover.png");
            LoadImage(_switchOnHoverBackground, texData);
            Object.DontDestroyOnLoad(_switchOnHoverBackground);
            texData = ResourceUtils.GetEmbeddedResource("switch on act.png");
            LoadImage(_switchOnActBackground, texData);
            Object.DontDestroyOnLoad(_switchOnActBackground);

            fswitch.normal.background = _switchNormalBackground;
            fswitch.normal.textColor = new Color32(180, 180, 180, 255);
            fswitch.onNormal.background = _switchOnBackground;
            fswitch.onNormal.textColor = new Color32(180, 180, 180, 255);
            fswitch.hover.background = _switchNormalHoverBackground;
            fswitch.hover.textColor = new Color32(180, 180, 180, 255);
            fswitch.onHover.background = _switchOnHoverBackground;
            fswitch.onHover.textColor = new Color32(180, 180, 180, 255);
            fswitch.active.background = _switchNormalActBackground;
            fswitch.active.textColor = new Color32(180, 180, 180, 255);
            fswitch.onActive.background = _switchOnActBackground;
            fswitch.onActive.textColor = new Color32(180, 180, 180, 255);
            fswitch.focused.background = null;
            fswitch.focused.textColor = new Color32(180, 180, 180, 255);
            fswitch.onFocused.background = null;
            fswitch.onFocused.textColor = new Color32(180, 180, 180, 255);
            fswitch.border = new RectOffset(0, 0, 0, 0);
            fswitch.margin = new RectOffset(2, 25, 2, Mathf.RoundToInt(fontSize * 0.5f));//new RectOffset(4, 4, 3, 2);
            fswitch.padding = new RectOffset(0, 0, 1, 2);
            fswitch.overflow = new RectOffset(0, 0, 0, 1);
            fswitch.alignment = TextAnchor.MiddleCenter;
            fswitch.wordWrap = false;
            fswitch.richText = false;
            fswitch.clipping = TextClipping.Clip;
            fswitch.imagePosition = ImagePosition.ImageLeft;
            fswitch.contentOffset = new Vector2(0, 0);
            fswitch.fixedWidth = 78;
            fswitch.fixedHeight = 40;
            fswitch.stretchWidth = true;
            fswitch.stretchHeight = true;

            switchlabel = new GUIStyle
            {
                name = "switchlabel"
            };

            switchlabel.normal.textColor = newSkin.label.normal.textColor;
            switchlabel.border = newSkin.label.border;
            switchlabel.margin = new RectOffset(4, 4, 6, 4); //newSkin.label.margin;
            switchlabel.padding = newSkin.label.padding;
            switchlabel.overflow = newSkin.label.overflow;
            switchlabel.stretchWidth = newSkin.label.stretchWidth;
            switchlabel.stretchHeight = newSkin.label.stretchHeight;
            switchlabel.fontStyle = FontStyle.Bold;
            switchlabel.alignment = TextAnchor.MiddleCenter;
            switchlabel.wordWrap = false;
            switchlabel.richText = false;
            switchlabel.clipping = TextClipping.Clip;
            switchlabel.imagePosition = ImagePosition.ImageLeft;
            switchlabel.contentOffset = new Vector2(0, 0);
            switchlabel.fixedWidth = 0;
            switchlabel.fixedHeight = 0;
            switchlabel.stretchWidth = true;
            switchlabel.stretchHeight = false;

            sliderfill = new GUIStyle
            {
                name = "sliderfill"
            };

            texData = ResourceUtils.GetEmbeddedResource("slider horiz fill.png");
            LoadImage(_sliderVNormalBackground, texData);
            Object.DontDestroyOnLoad(_sliderVNormalBackground);
            sliderfill.normal.background = _sliderVNormalBackground;
            sliderfill.normal.textColor = Color.black;
            sliderfill.border = new RectOffset(3, 3, 0, 0);
            sliderfill.margin = new RectOffset(4, 4, 8, 0);
            sliderfill.padding = new RectOffset(-1, -1, 0, 0);
            sliderfill.overflow = new RectOffset(0, 0, -7, -6);
            sliderfill.font = null;
            sliderfill.fontSize = 0;
            sliderfill.fontStyle = FontStyle.Normal;
            sliderfill.alignment = TextAnchor.MiddleLeft;
            sliderfill.wordWrap = false;
            sliderfill.richText = false;
            sliderfill.imagePosition = ImagePosition.ImageOnly;
            sliderfill.clipping = TextClipping.Clip;
            sliderfill.contentOffset = new Vector2(0, 0);
            sliderfill.fixedWidth = 0f;
            sliderfill.fixedHeight = 18f;
            sliderfill.stretchWidth = true;
            sliderfill.stretchHeight = false;

            lightbutton = new GUIStyle
            {
                name = "lightbutton"
            };

            texData = ResourceUtils.GetEmbeddedResource("lightbut.png");
            LoadImage(_lightbutNormalBackground, texData);
            Object.DontDestroyOnLoad(_lightbutNormalBackground);
            lightbutton.normal.background = _lightbutNormalBackground;
            lightbutton.normal.textColor = new Color32(180, 180, 180, 255);
            lightbutton.hover.background = _lightbutNormalBackground;
            lightbutton.hover.textColor = Color.white;
            lightbutton.onHover.background = null;
            lightbutton.onHover.textColor = Color.white;
            lightbutton.onNormal.background = _lightbutNormalBackground;
            lightbutton.onNormal.textColor = new Color32(78, 160, 180, 255);
            lightbutton.active.background = null;
            lightbutton.active.textColor = new Color32(180, 180, 180, 255);
            lightbutton.onActive.background = null;
            lightbutton.onActive.textColor = new Color32(180, 180, 180, 255);
            lightbutton.focused.background = null;
            lightbutton.focused.textColor = new Color32(180, 180, 180, 255);
            lightbutton.onFocused.background = null;
            lightbutton.onFocused.textColor = new Color32(180, 180, 180, 255);
            lightbutton.border = new RectOffset(6, 6, 4, 4);
            lightbutton.margin = new RectOffset(4, 4, 3, 3);
            lightbutton.padding = new RectOffset(6, 6, 7, 9);
            lightbutton.overflow = new RectOffset(0, 0, -1, 2);
            lightbutton.imagePosition = ImagePosition.ImageLeft;
            lightbutton.alignment = TextAnchor.MiddleCenter;
            lightbutton.contentOffset = new Vector2(0, 0);
            lightbutton.stretchWidth = true;
            lightbutton.stretchHeight = false;
            lightbutton.fontStyle = FontStyle.Bold;

            newSkin.customStyles = new GUIStyle[] { toolbarbutton, boldlabel, tabcontent, tabheader, tabsmall, fswitch, switchlabel, sliderfill, lightbutton };

            newSkin.settings.doubleClickSelectsWord = true;
            newSkin.settings.tripleClickSelectsLine = true;
            newSkin.settings.cursorColor = new Color32(180, 180, 180, 255);
            newSkin.settings.cursorFlashSpeed = 0;
            newSkin.settings.selectionColor = new Color(61, 128, 223, 166);

            return newSkin;
        }
    }
}
