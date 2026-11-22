using Graphics.Inspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graphics.UI
{
    /// <summary>
    /// The plugin's window, drawn as the HS2PostProcessing window is: a head with the title, a rail of
    /// tabs on the left with their icons (a tab's sub-tabs listed under it while it is open) and the
    /// page beside it, one card per box the inspector draws.
    ///
    /// The page is the inspectors' own drawing code, run every frame through the immediate-mode layer
    /// (see Im), so what is on it and when is exactly what the inspectors decide.
    ///
    /// Sharp at any resolution: the canvas scales by a factor rather than stretching a fixed-size
    /// texture, so text is rasterised at its final pixel size.
    /// </summary>
    internal class LiteWindow : MonoBehaviour
    {
        public static LiteWindow Instance { get; private set; }

        /// <summary>The width a card's rows have, for the inspectors' column counts.</summary>
        public static float ContentWidth = 480f;

        private const float RailWidth = 156f;
        private const float HeadHeight = 32f;
        private const float MinWidth = 560f;
        private const float MinHeight = 360f;
        private const float PopupTime = 0.1f;

        private Graphics _plugin;
        private Inspector.Inspector _inspector;

        private Canvas _canvas;
        private CanvasScaler _scaler;
        private RectTransform _win, _railList, _viewport, _popups;
        private ScrollRect _scroll;
        private Text _title;

        private readonly Dictionary<string, RootNode> _pages = new Dictionary<string, RootNode>();
        private readonly Dictionary<string, float> _scrollY = new Dictionary<string, float>();
        private string _pageKey;
        private RootNode _page;

        private string _railSignature;
        private readonly List<Action> _railRefreshers = new List<Action>();
        private bool _shown;
        private float _lastError;

        public void Init(Graphics plugin, Inspector.Inspector inspector)
        {
            Instance = this;
            _plugin = plugin;
            _inspector = inspector;
        }

        public bool Visible
        {
            get { return _shown; }
            set
            {
                if (value && _canvas == null)
                    Build();
                _shown = value;
                if (_canvas != null)
                    _canvas.gameObject.SetActive(value);
                if (value == false)
                    ClosePopup();
            }
        }

        private float Width => Mathf.Max(MinWidth, Inspector.Inspector.Width);

        #region Frame
        private void Build()
        {
            EnsureEventSystem();

            GameObject root = new GameObject("Graphics Lite UI");
            DontDestroyOnLoad(root);
            root.layer = LayerMask.NameToLayer("UI") >= 0 ? LayerMask.NameToLayer("UI") : 5;
            _canvas = root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 1010;
            _canvas.pixelPerfect = true;
            _scaler = root.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            root.AddComponent<GraphicRaycaster>();
            ApplyScale();

            Image win = Kit.Box("Window", _canvas.transform, Pal.C(W.PageBg), 8f);
            win.raycastTarget = true;
            _win = win.rectTransform;
            _win.anchorMin = _win.anchorMax = new Vector2(0f, 1f);
            _win.pivot = new Vector2(0f, 1f);
            _win.anchoredPosition = Clamp(new Vector2(Inspector.Inspector.StartOffsetX, -Inspector.Inspector.StartOffsetY));
            win.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            Resize();

            BuildHead();
            BuildRail();
            BuildMain();
            Kit.Ring("Border", _win, Pal.C(0x0D0E10), 8f);
            BuildResizeGrip();

            _popups = Kit.Node("Popups", _canvas.transform).Fill();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;
            GameObject es = new GameObject("EventSystem");
            DontDestroyOnLoad(es);
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private void BuildHead()
        {
            Image head = Kit.Box("Head", _win, Pal.C(W.Chrome));
            head.raycastTarget = true;
            head.rectTransform.Css(0f, 0f, 0f, float.NaN, float.NaN, HeadHeight);
            Kit.Box("Border", head.transform, Pal.C(0x111215)).rectTransform.Css(0f, float.NaN, 0f, 0f, float.NaN, 1f);

            RectTransform row = Kit.Node("Row", head.transform).Fill(12f, 0f, 6f, 1f);
            Kit.Row(row.gameObject, 6f);
            _title = Kit.Text("Title", row, "Graphics Lite", 12, Pal.C(W.TextC), TextAnchor.MiddleLeft, true);
            Text version = Kit.Text("Version", row, Graphics.Version, 11, Pal.C(W.Faint));
            Kit.Spacer(row);
            W.IconButton(row, "close", () => _plugin.ToggleGUI());

            DragHandler drag = head.gameObject.AddComponent<DragHandler>();
            Vector2 start = Vector2.zero, mouse = Vector2.zero;
            drag.onBeginDrag = e =>
            {
                start = _win.anchoredPosition;
                mouse = e.position / _canvas.scaleFactor;
            };
            drag.onDrag = e =>
            {
                Vector2 at = Clamp(start + e.position / _canvas.scaleFactor - mouse);
                float scale = _canvas.scaleFactor;
                _win.anchoredPosition = new Vector2(Mathf.Round(at.x * scale), Mathf.Round(at.y * scale)) / scale;
            };
            drag.onEndDrag = e => SavePosition();
        }

        private void SavePosition()
        {
            Inspector.Inspector.StartOffsetX = Mathf.RoundToInt(_win.anchoredPosition.x);
            Inspector.Inspector.StartOffsetY = Mathf.RoundToInt(-_win.anchoredPosition.y);
        }

        private void BuildRail()
        {
            Image rail = Kit.Box("Rail", _win, Pal.C(W.Chrome));
            rail.rectTransform.Css(0f, HeadHeight, float.NaN, 0f, RailWidth);
            Kit.Box("Edge", rail.transform, Pal.C(0x1A1C20)).rectTransform.Css(float.NaN, 0f, 0f, 0f, 1f);

            RectTransform viewport = Kit.Node("Viewport", rail.transform).Fill(0f, 0f, 1f, 0f);
            viewport.gameObject.AddComponent<Image>().color = Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            _railList = Kit.Node("List", viewport);
            _railList.anchorMin = new Vector2(0f, 1f);
            _railList.anchorMax = new Vector2(1f, 1f);
            _railList.pivot = new Vector2(0.5f, 1f);
            _railList.sizeDelta = Vector2.zero;
            Kit.Col(_railList.gameObject, 1f, new RectOffset(6, 7, 8, 8));
            _railList.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = _railList;
            scroll.viewport = viewport;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = false;
            scroll.scrollSensitivity = 24f;
            _railSignature = null;
        }

        private static readonly Inspector.Inspector.Tab[] FootTabs =
        {
            Inspector.Inspector.Tab.Presets, Inspector.Inspector.Tab.Screenshot, Inspector.Inspector.Tab.Settings
        };

        private static string IconOf(Inspector.Inspector.Tab tab)
        {
            switch (tab)
            {
                case Inspector.Inspector.Tab.Environmental: return "tab-environment";
                case Inspector.Inspector.Tab.Probes: return "tab-reflections";
                case Inspector.Inspector.Tab.Lights: return "tab-lights";
                case Inspector.Inspector.Tab.GI: return "tab-gi";
                case Inspector.Inspector.Tab.PostProcessing: return "tab-post";
                case Inspector.Inspector.Tab.AntiAliasing: return "tab-aa";
                case Inspector.Inspector.Tab.SSS: return "tab-skin";
                case Inspector.Inspector.Tab.Presets: return "tab-presets";
                case Inspector.Inspector.Tab.Screenshot: return "tab-camera";
                default: return "tab-settings";
            }
        }

        /// <summary>The rail is built again only when what it lists changes: the tab, a sub-tab, the probes.</summary>
        private void RefreshRail()
        {
            string[] probes = _inspector.ProbeNames();
            string signature = _inspector.SelectedTab + "|" + _inspector.SelectedPostProcessingSubTab + "|" + _inspector.SelectedProbeIndex + "|" +
                               string.Join("\u0001", probes) + "|" + LocalizationManager.CurrentLanguage;
            if (signature != _railSignature)
            {
                _railSignature = signature;
                for (int i = _railList.childCount - 1; i >= 0; --i)
                    Destroy(_railList.GetChild(i).gameObject);
                _railRefreshers.Clear();

                foreach (Inspector.Inspector.Tab tab in Enum.GetValues(typeof(Inspector.Inspector.Tab)))
                {
                    if (Array.IndexOf(FootTabs, tab) >= 0)
                        continue;
                    RailTab(tab);
                }
                RectTransform sep = Kit.Node("Sep", _railList);
                Kit.Size(sep.gameObject, -1f, 13f);
                Kit.Box("Line", sep, Pal.C(W.Rule)).rectTransform.Css(4f, 6f, 4f, float.NaN, float.NaN, 1f);
                foreach (Inspector.Inspector.Tab tab in FootTabs)
                    RailTab(tab);
            }
            foreach (Action r in _railRefreshers)
                r();
        }

        private void RailTab(Inspector.Inspector.Tab tab)
        {
            bool on = _inspector.SelectedTab == tab;
            string label = W.Localize(Inspector.Inspector.DisplayNames[tab]);
            Clickable item = W.Pill(_railList, "Tab", 28f, 8f, 8f, on ? Pal.C(W.Hover) : Color.clear, on ? Pal.C(W.Hover) : Pal.C(W.CardBg), 5f);
            item.GetComponent<HorizontalLayoutGroup>().padding.right = 7;
            IconView view = Kit.Icon("Icon", item.transform, IconOf(tab), on ? Pal.accent : Pal.C(W.Dim));
            Text text = Kit.Text("Text", item.transform, label, W.FontSize, on ? Pal.C(W.TextC) : Pal.C(W.Dim), TextAnchor.MiddleLeft, on);
            Kit.Flex(text.gameObject, 28f);
            if (on)
                item.Tint(Pal.C(W.TextC), Pal.C(W.TextC), text);
            else
                item.Tint(Pal.C(W.Dim), Pal.C(W.TextC), text, view);
            item.onClick = () => _inspector.SelectedTab = tab;

            if (on == false)
                return;
            if (tab == Inspector.Inspector.Tab.PostProcessing)
            {
                foreach (KeyValuePair<Inspector.Inspector.PostProcessingSubTab, string> sub in Inspector.Inspector.PostProcessingDisplayNames)
                {
                    Inspector.Inspector.PostProcessingSubTab key = sub.Key;
                    RailSub(W.Localize(sub.Value), _inspector.SelectedPostProcessingSubTab == key,
                        () => _inspector.SelectedPostProcessingSubTab = key,
                        () => _inspector.IsPostProcessingEffectEnabled(key));
                }
            }
            else if (tab == Inspector.Inspector.Tab.Probes)
            {
                string[] probes = _inspector.ProbeNames();
                if (probes.Length == 0)
                {
                    Text none = Kit.Text("None", _railList, W.Localize("No probes found"), W.FontSize, Pal.C(W.Faint));
                    Kit.Size(none.gameObject, -1f, 24f);
                }
                for (int i = 0; i < probes.Length; ++i)
                {
                    int index = i;
                    RailSub(probes[i], _inspector.SelectedProbeIndex == i, () => _inspector.SelectedProbeIndex = index, null);
                }
            }
        }

        private void RailSub(string label, bool on, Action pick, Func<bool> active)
        {
            Clickable item = W.Pill(_railList, "Sub", 24f, 8f, 8f, on ? Pal.C(W.CardBg) : Color.clear, Pal.C(W.CardBg), 5f);
            item.GetComponent<HorizontalLayoutGroup>().padding.left = 30;
            Text text = Kit.Text("Text", item.transform, label, W.FontSize, on ? Pal.C(W.TextC) : Pal.C(W.Dim), TextAnchor.MiddleLeft, on);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            Kit.Flex(text.gameObject, 24f);
            item.Tint(on ? Pal.C(W.TextC) : Pal.C(W.Dim), Pal.C(W.TextC), text);
            item.onClick = pick;
            if (active == null)
                return;
            RectTransform dotNode = Kit.Node("Dot", item.transform);
            Kit.Size(dotNode.gameObject, 6f, 6f);
            PaintArea dot = dotNode.gameObject.AddComponent<PaintArea>();
            dot.raycastTarget = false;
            bool shown = false;
            dot.draw = p =>
            {
                if (shown)
                    p.Circle(p.width * 0.5f, p.height * 0.5f, 2.5f, Pal.accent);
            };
            _railRefreshers.Add(() =>
            {
                bool now = active();
                if (now == shown)
                    return;
                shown = now;
                dot.Redraw();
            });
        }

        private void BuildMain()
        {
            RectTransform main = Kit.Node("Main", _win).Css(RailWidth, HeadHeight, 0f, 0f);
            _viewport = Kit.Node("Viewport", main).Fill();
            _viewport.gameObject.AddComponent<Image>().color = Color.clear;
            _viewport.gameObject.AddComponent<RectMask2D>();
            _scroll = _viewport.gameObject.AddComponent<ScrollRect>();
            _scroll.viewport = _viewport;
            _scroll.horizontal = false;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.inertia = false;
            _scroll.scrollSensitivity = 24f;
            _pages.Clear();
            _page = null;
            _pageKey = null;
        }

        private RootNode Page(string key)
        {
            RootNode page;
            if (_pages.TryGetValue(key, out page) && page.Rect != null)
                return page;
            RectTransform rect = Kit.Node("Page", _viewport);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = Vector2.zero;
            Kit.Col(rect.gameObject, 8f, new RectOffset(10, 10, 8, 14));
            rect.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            page = new RootNode(rect);
            _pages[key] = page;
            return page;
        }

        /// <summary>Shows the page for the open tab, kept from the last time it was shown.</summary>
        private void ShowPage(string key)
        {
            if (key == _pageKey && _page != null)
                return;
            if (_page != null && _pageKey != null)
            {
                _scrollY[_pageKey] = _page.Rect.anchoredPosition.y;
                _page.Rect.gameObject.SetActive(false);
            }
            _pageKey = key;
            _page = Page(key);
            _page.Rect.gameObject.SetActive(true);
            _scroll.content = _page.Rect;
            float y;
            _page.Rect.anchoredPosition = new Vector2(0f, _scrollY.TryGetValue(key, out y) ? y : 0f);
        }

        /// <summary>The window's bottom right corner: drag it to change its size.</summary>
        private void BuildResizeGrip()
        {
            Image grip = Kit.Box("Grip", _win, Color.clear);
            grip.raycastTarget = true;
            grip.rectTransform.Css(float.NaN, float.NaN, 0f, 0f, 16f, 16f);
            RectTransform markNode = Kit.Node("Mark", grip.transform).Fill();
            PaintArea mark = markNode.gameObject.AddComponent<PaintArea>();
            mark.raycastTarget = false;
            bool hover = false;
            mark.draw = p =>
            {
                Color c = hover ? Pal.C(W.Dim) : Pal.C(W.Rule);
                p.Line(p.width - 4f, p.height - 12f, p.width - 12f, p.height - 4f, 1.2f, c);
                p.Line(p.width - 4f, p.height - 7f, p.width - 7f, p.height - 4f, 1.2f, c);
            };
            grip.gameObject.AddComponent<HoverHandler>().onHover = on =>
            {
                hover = on;
                mark.Redraw();
            };
            DragHandler drag = grip.gameObject.AddComponent<DragHandler>();
            Vector2 startSize = Vector2.zero, startMouse = Vector2.zero;
            drag.onBeginDrag = e =>
            {
                startSize = _win.sizeDelta;
                startMouse = e.position / _canvas.scaleFactor;
            };
            drag.onDrag = e =>
            {
                Vector2 d = e.position / _canvas.scaleFactor - startMouse;
                float room = Screen.height / _canvas.scaleFactor - 20f;
                float w = Mathf.Max(MinWidth, startSize.x + d.x);
                float h = Mathf.Clamp(startSize.y - d.y, MinHeight, Mathf.Max(MinHeight, room));
                _win.sizeDelta = new Vector2(w, h);
                Inspector.Inspector.Width = Mathf.RoundToInt(w);
                Inspector.Inspector.Height = Mathf.RoundToInt(h);
                Measure();
            };
        }

        private void Resize()
        {
            float room = Screen.height / _scaler.scaleFactor - 20f;
            _win.sizeDelta = new Vector2(Width, Mathf.Clamp(Inspector.Inspector.Height, MinHeight, Mathf.Max(MinHeight, room)));
            Measure();
        }

        /// <summary>The widths the rows and the inspectors' grids are laid out for.</summary>
        private void Measure()
        {
            ContentWidth = _win.sizeDelta.x - RailWidth - 20f - 24f;
            W.LabelWidth = Mathf.Clamp(Mathf.Round(ContentWidth * 0.32f), 110f, 190f);
            Inspector.Inspector.adaptiveColumns = Mathf.Max(1, Mathf.FloorToInt(ContentWidth / 150f));
            Inspector.Inspector.adaptivePreviewColumns = Mathf.Max(1, Mathf.FloorToInt(ContentWidth / 74f));
            Inspector.Inspector.maxTextWidth = Mathf.RoundToInt(ContentWidth);
        }

        private Vector2 Clamp(Vector2 pos)
        {
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            Vector2 screen = new Vector2(Screen.width, Screen.height) / scale;
            pos.x = Mathf.Clamp(pos.x, 40f - Width, screen.x - 40f);
            pos.y = Mathf.Clamp(pos.y, -(screen.y - HeadHeight), 0f);
            return pos;
        }

        /// <summary>
        /// The UI scale: the Font Size setting against its default of 12, grown with the screen above
        /// 1080p so the window keeps its share of it, and rounded to an eighth.
        /// </summary>
        private float WantedScale
        {
            get
            {
                float font = Graphics.ConfigFontSize != null ? Graphics.ConfigFontSize.Value : 12;
                float s = Mathf.Clamp(font / 12f, 0.75f, 2f) * Mathf.Max(1f, Screen.height / 1080f);
                return Mathf.Max(0.5f, Mathf.Round(s * 8f) / 8f);
            }
        }

        private void ApplyScale()
        {
            _scaler.scaleFactor = WantedScale;
            _canvas.scaleFactor = WantedScale;
        }
        #endregion

        #region Popups
        private RectTransform _popupRect;
        private Vector2 _popupBelow, _popupAbove;

        private RectTransform OpenPopup(RectTransform anchor, float width)
        {
            ClosePopup();
            Image backdrop = Kit.Box("Backdrop", _popups, Color.clear);
            backdrop.raycastTarget = true;
            backdrop.rectTransform.Fill();
            Clickable close = backdrop.gameObject.AddComponent<Clickable>();
            close.onDown = e => ClosePopup();

            Image panel = Kit.Box("Popup", _popups, Pal.C(W.Chrome), 6f);
            panel.raycastTarget = true;
            RectTransform rect = panel.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            Kit.Col(panel.gameObject, 1f, new RectOffset(4, 4, 4, 4));
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            rect.sizeDelta = new Vector2(width, 0f);
            Kit.Ring("Border", rect, Pal.C(0x0D0E10), 6f).gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            Vector3[] corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector2 bottomLeft, topLeft;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_popups, corners[0], null, out bottomLeft);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_popups, corners[1], null, out topLeft);
            Rect area = _popups.rect;
            _popupBelow = new Vector2(bottomLeft.x - area.xMin, bottomLeft.y - area.yMax - 3f);
            _popupAbove = new Vector2(topLeft.x - area.xMin, topLeft.y - area.yMax + 3f);
            _popupRect = rect;
            rect.anchoredPosition = _popupBelow;
            return rect;
        }

        /// <summary>Once the popup knows its height: flips it above its anchor and keeps it on screen.</summary>
        private void PlacePopup()
        {
            if (_popupRect == null)
                return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_popupRect);
            float h = _popupRect.rect.height, w = _popupRect.rect.width;
            Rect area = _popups.rect;
            Vector2 pos = _popupBelow;
            if (-pos.y + h > area.height)
                pos = new Vector2(_popupAbove.x, _popupAbove.y + h);
            pos.x = Mathf.Clamp(pos.x, 0f, Mathf.Max(0f, area.width - w));
            pos.y = Mathf.Clamp(pos.y, -(area.height - h), 0f);
            _popupRect.anchoredPosition = pos;
        }

        public void ClosePopup()
        {
            _popupRect = null;
            if (_popups == null)
                return;
            for (int i = _popups.childCount - 1; i >= 0; --i)
                Destroy(_popups.GetChild(i).gameObject);
        }

        public void OpenMenu(RectTransform anchor, string[] names, int current, Action<int> pick, Func<int, bool> disabled = null)
        {
            RectTransform menu = OpenPopup(anchor, Mathf.Max(140f, anchor.rect.width));
            if (names.Length > 16)
            {
                RectTransform viewport = Kit.Node("Scroll", menu);
                viewport.gameObject.AddComponent<Image>().color = Color.clear;
                viewport.gameObject.AddComponent<RectMask2D>();
                Kit.Size(viewport.gameObject, -1f, 16 * 23f);
                RectTransform content = Kit.Node("Content", viewport);
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = new Vector2(1f, 1f);
                content.pivot = new Vector2(0.5f, 1f);
                content.sizeDelta = Vector2.zero;
                content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                Kit.Col(content.gameObject, 1f);
                ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
                scroll.content = content;
                scroll.viewport = viewport;
                scroll.horizontal = false;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                scroll.inertia = false;
                scroll.scrollSensitivity = 24f;
                float y = Mathf.Max(0, current - 7) * 23f;
                content.anchoredPosition = new Vector2(0f, Mathf.Min(y, Mathf.Max(0f, names.Length * 23f - 16 * 23f)));
                menu = content;
            }
            for (int i = 0; i < names.Length; ++i)
            {
                int index = i;
                bool off = disabled != null && disabled(i);
                Clickable item = W.Pill(menu, "Item", 22f, 8f, 6f, Color.clear, off ? Color.clear : Pal.C(W.Hover), 4f);
                Color color = off ? Pal.C(W.Faint) : i == current ? Pal.accent : Pal.C(0xD5D8DD);
                Text t = Kit.Text("Text", item.transform, names[i], W.FontSize, color, TextAnchor.MiddleLeft, i == current);
                Kit.Flex(t.gameObject, 22f);
                item.Tint(color, off ? color : Pal.C(W.TextC), t);
                item.disabled = off;
                item.onClick = () =>
                {
                    ClosePopup();
                    pick(index);
                };
            }
            PlacePopup();
        }

        public void OpenPicker(RectTransform anchor, ColorParam p)
        {
            RectTransform panel = OpenPopup(anchor, 216f);
            VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            ColorPicker picker = ColorPicker.Build(panel, p);

            RectTransform line = W.Line(panel, 6f);
            Image preview = Kit.Box("Preview", line, p.Value, 4f);
            Kit.Size(preview.gameObject, 26f, 20f);
            InputField[] channels = new InputField[3];
            for (int i = 0; i < channels.Length; ++i)
            {
                int channel = i;
                channels[i] = W.Field(line, -1f, TextAnchor.MiddleCenter, InputField.ContentType.IntegerNumber, s =>
                {
                    int typed;
                    if (int.TryParse(s, out typed) == false)
                        return;
                    Color32 c = p.Value;
                    byte value = (byte)Mathf.Clamp(typed, 0, 255);
                    if (channel == 0) c.r = value;
                    else if (channel == 1) c.g = value;
                    else c.b = value;
                    Color keep = p.Value;
                    Color next = c;
                    next.a = keep.a;
                    p.Value = next;
                    picker.Sync();
                }, "RGB".Substring(i, 1));
            }

            RectTransform hexLine = W.Line(panel, 6f);
            Kit.Text("Hash", hexLine, "#", W.FontSize, Pal.C(W.Dim));
            InputField hex = W.Field(hexLine, -1f, TextAnchor.MiddleLeft, InputField.ContentType.Standard, s =>
            {
                string code = s.Trim().TrimStart('#');
                uint packed;
                if (code.Length != 6 || uint.TryParse(code, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out packed) == false)
                    return;
                Color keep = p.Value;
                Color next = new Color32((byte)(packed >> 16), (byte)(packed >> 8), (byte)packed, 255);
                next.a = keep.a;
                p.Value = next;
                picker.Sync();
            }, "RRGGBB");
            hex.characterLimit = 7;

            picker.onPainted = () =>
            {
                Color v = p.Value;
                preview.color = new Color(v.r, v.g, v.b, 1f);
                Color32 c = new Color(Mathf.Clamp01(v.r), Mathf.Clamp01(v.g), Mathf.Clamp01(v.b), 1f);
                byte[] now = { c.r, c.g, c.b };
                for (int i = 0; i < channels.Length; ++i)
                    if (channels[i].isFocused == false)
                        channels[i].text = now[i].ToString();
                if (hex.isFocused == false)
                    hex.text = string.Format("#{0:X2}{1:X2}{2:X2}", c.r, c.g, c.b);
            };
            picker.Sync();
            PlacePopup();
        }
        #endregion

        #region Frame loop
        private void Update()
        {
            if (_shown == false || _canvas == null)
                return;
            // Studio and the H scenes lock and hide the cursor while the camera turns; the window
            // needs it, as Graphics' did.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// The page is drawn after the event system has handed the clicks and drags of this frame to
        /// the rows, so a row hands them on to the inspector in the same frame.
        /// </summary>
        private void LateUpdate()
        {
            if (_shown == false || _canvas == null || _inspector == null)
                return;

            if (Mathf.Approximately(_scaler.scaleFactor, WantedScale) == false)
            {
                ApplyScale();
                Resize();
                _win.anchoredPosition = Clamp(_win.anchoredPosition);
            }

            // The size is a setting too (Settings > Window Width and Height).
            float room = Screen.height / _canvas.scaleFactor - 20f;
            Vector2 wanted = new Vector2(Width, Mathf.Clamp(Inspector.Inspector.Height, MinHeight, Mathf.Max(MinHeight, room)));
            if (Mathf.Abs(wanted.x - _win.sizeDelta.x) > 0.5f || Mathf.Abs(wanted.y - _win.sizeDelta.y) > 0.5f)
                Resize();

            RefreshRail();
            ShowPage(_inspector.PageKey);

            Lay.Reset();
            Im.Begin(_page);
            try
            {
                _inspector.DrawTabs();
            }
            catch (Exception e)
            {
                if (Time.unscaledTime - _lastError > 5f)
                {
                    _lastError = Time.unscaledTime;
                    _plugin.Log.LogError(e);
                }
            }
            finally
            {
                Im.End();
            }
        }

        /// <summary>
        /// While the pointer is over the window, the game does not see the mouse: the camera does not
        /// turn under a slider being dragged. Done where Graphics did it, in OnGUI.
        /// </summary>
        private void OnGUI()
        {
            if (_shown == false || _win == null)
                return;
            Vector2 mouse = Input.mousePosition;
            if (RectTransformUtility.RectangleContainsScreenPoint(_win, mouse, null) ||
                (_popups != null && _popups.childCount > 0))
                Input.ResetInputAxes();
        }

        private void OnDestroy()
        {
            if (_canvas != null)
                Destroy(_canvas.gameObject);
            if (Instance == this)
                Instance = null;
        }
        #endregion
    }
}
