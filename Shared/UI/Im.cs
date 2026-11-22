using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graphics.UI
{
    /// <summary>
    /// Immediate-mode drawing onto retained uGUI objects.
    ///
    /// The inspectors are written the IMGUI way: every frame they run from top to bottom, asking for a
    /// slider here and a toggle there, each with the value it has now and what to do when it changes.
    /// That code is kept as it was, and so is every rule in it about what is shown when. What changed is
    /// what a call draws: instead of an IMGUI control it finds (or makes) a uGUI row in the window's
    /// style, keyed by its kind and its label at its place in the page, shows the value on it, and
    /// hands back what the user did to it since the last frame. A row that is no longer asked for is
    /// taken away; one asked for anew is made where it is asked for.
    /// </summary>
    internal static class Im
    {
        /// <summary>GUI.enabled, for what is drawn next.</summary>
        public static bool Enabled = true;

        private static readonly List<ImNode> _stack = new List<ImNode>();

        public static bool Active => _stack.Count > 0;
        public static ImNode Top => _stack[_stack.Count - 1];
        public static bool InRow => Active && Top.IsRow;

        /// <summary>How many cards are open around what is drawn next.</summary>
        public static int CardDepth
        {
            get
            {
                int depth = 0;
                foreach (ImNode n in _stack)
                    if (n is CardNode)
                        ++depth;
                return depth;
            }
        }

        public static bool InScroll
        {
            get
            {
                foreach (ImNode n in _stack)
                    if (n is ScrollNode)
                        return true;
                return false;
            }
        }

        public static void Begin(ImNode root)
        {
            _stack.Clear();
            root.Cursor = 0;
            _stack.Add(root);
            Enabled = true;
        }

        public static void End()
        {
            while (_stack.Count > 1)
                EndContainer();
            if (_stack.Count == 1)
                Trim(_stack[0]);
            _stack.Clear();
            Enabled = true;
        }

        /// <summary>The node for the next call: the one made for it last frame, or a new one.</summary>
        public static T Next<T>(string key, Func<Transform, T> create) where T : ImNode
        {
            ImNode parent = Top;
            List<ImNode> list = parent.Children;
            int limit = Math.Min(list.Count, parent.Cursor + 64);
            for (int j = parent.Cursor; j < limit; ++j)
            {
                if (list[j].Key == key && list[j] is T)
                {
                    // What stood between was not asked for this time.
                    for (int d = parent.Cursor; d < j; ++d)
                        list[d].Dispose();
                    list.RemoveRange(parent.Cursor, j - parent.Cursor);
                    T found = (T)list[parent.Cursor];
                    parent.Cursor++;
                    found.SetEnabled(Enabled);
                    return found;
                }
            }

            T node = create(parent.Content);
            node.Key = key;
            if (parent.IsRow && node.FlexInRow)
            {
                LayoutElement le = node.Rect.GetComponent<LayoutElement>() ?? node.Rect.gameObject.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                le.minWidth = 0f;
                if (le.preferredWidth < 0f)
                    le.preferredWidth = 0f;
            }
            if (parent.Cursor < list.Count)
                node.Rect.SetSiblingIndex(list[parent.Cursor].Rect.GetSiblingIndex());
            else
                node.Rect.SetAsLastSibling();
            list.Insert(parent.Cursor, node);
            parent.Cursor++;
            node.SetEnabled(Enabled);
            return node;
        }

        public static T BeginContainer<T>(string key, Func<Transform, T> create) where T : ImNode
        {
            T node = Next(key, create);
            node.Cursor = 0;
            _stack.Add(node);
            return node;
        }

        public static void EndContainer()
        {
            if (_stack.Count <= 1)
                return;
            ImNode node = Top;
            Trim(node);
            _stack.RemoveAt(_stack.Count - 1);
            node.OnEnd();
        }

        private static void Trim(ImNode node)
        {
            List<ImNode> list = node.Children;
            if (node.Cursor >= list.Count)
                return;
            for (int i = node.Cursor; i < list.Count; ++i)
                list[i].Dispose();
            list.RemoveRange(node.Cursor, list.Count - node.Cursor);
        }
    }

    internal class ImNode
    {
        public string Key;
        public RectTransform Rect;
        public Transform Content;
        public readonly List<ImNode> Children = new List<ImNode>();
        public int Cursor;
        public bool IsRow;
        public bool IsContainer;
        public bool FlexInRow = true;
        private bool _enabled = true;
        private CanvasGroup _group;

        public void SetEnabled(bool on)
        {
            if (IsContainer || on == _enabled)
                return;
            _enabled = on;
            if (_group == null)
                _group = Rect.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = on ? 1f : 0.4f;
            _group.blocksRaycasts = on;
            _group.interactable = on;
        }

        public virtual void OnEnd() { }

        public virtual void Dispose()
        {
            if (Rect != null)
            {
                Rect.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(Rect.gameObject);
            }
        }

        /// <summary>Sets a text only when it changed: a text set again dirties its canvas.</summary>
        protected static void SetText(Text t, ref string shown, string value)
        {
            if (value == shown)
                return;
            shown = value;
            t.text = value;
        }
    }

    /// <summary>The part of a row that its override box switches off: everything but the box and the label.</summary>
    internal sealed class ControlGroup
    {
        public readonly RectTransform Rect;
        private CanvasGroup _group;
        private bool _on = true;

        public ControlGroup(Transform parent, float gap = 8f)
        {
            Rect = Kit.Node("Controls", parent);
            Kit.Row(Rect.gameObject, gap);
            Kit.Flex(Rect.gameObject, W.RowHeight);
        }

        public void Set(bool on)
        {
            if (on == _on)
                return;
            _on = on;
            if (_group == null)
                _group = Rect.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = on ? 1f : 0.4f;
            _group.blocksRaycasts = on;
            _group.interactable = on;
        }
    }

    #region Containers
    internal sealed class RootNode : ImNode
    {
        public RootNode(RectTransform rect)
        {
            Rect = rect;
            Content = rect;
            IsContainer = true;
        }
    }

    internal sealed class ColumnNode : ImNode
    {
        public ColumnNode(Transform parent, float gap)
        {
            Rect = Kit.Node("Col", parent);
            Kit.Col(Rect.gameObject, gap);
            Content = Rect;
            IsContainer = true;
        }
    }

    internal sealed class RowNode : ImNode
    {
        public RowNode(Transform parent)
        {
            Rect = Kit.Node("Row", parent);
            HorizontalLayoutGroup row = Kit.Row(Rect.gameObject, 6f);
            row.childAlignment = TextAnchor.UpperLeft;
            Content = Rect;
            IsContainer = true;
            IsRow = true;
        }
    }

    internal sealed class CardNode : ImNode
    {
        public CardNode(Transform parent, bool inner)
        {
            Rect = W.Card(parent, inner);
            Kit.Col(Rect.gameObject, 0f);
            RectTransform body = Kit.Node("Body", Rect);
            Kit.Col(body.gameObject, 4f, inner ? new RectOffset(10, 10, 8, 8) : new RectOffset(12, 12, 10, 12));
            Content = body;
            IsContainer = true;
        }
    }

    /// <summary>A list that scrolls inside the page, as tall as its content up to a height.</summary>
    internal sealed class ScrollNode : ImNode
    {
        private readonly LayoutElement _size;
        private readonly RectTransform _content;
        public float MaxHeight = 320f;

        public ScrollNode(Transform parent)
        {
            Rect = Kit.Node("Scroll", parent);
            Rect.gameObject.AddComponent<Image>().color = Color.clear;
            Rect.gameObject.AddComponent<RectMask2D>();
            _size = Rect.gameObject.AddComponent<LayoutElement>();
            _size.minHeight = 0f;
            _size.preferredHeight = 0f;

            _content = Kit.Node("Content", Rect);
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.sizeDelta = Vector2.zero;
            Kit.Col(_content.gameObject, 4f);
            _content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = Rect.gameObject.AddComponent<ScrollRect>();
            scroll.content = _content;
            scroll.viewport = Rect;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = false;
            scroll.scrollSensitivity = 24f;
            Content = _content;
            IsContainer = true;
        }

        public override void OnEnd()
        {
            float h = Mathf.Min(LayoutUtility.GetPreferredHeight(_content), MaxHeight);
            if (Mathf.Abs(_size.preferredHeight - h) > 0.5f)
                _size.preferredHeight = h;
        }
    }
    #endregion

    #region Spacing and text
    internal sealed class SpaceNode : ImNode
    {
        private readonly LayoutElement _le;
        private float _shown = -1f;

        public SpaceNode(Transform parent)
        {
            Rect = Kit.Node("Space", parent);
            _le = Rect.gameObject.AddComponent<LayoutElement>();
            FlexInRow = false;
        }

        public void Update(float size, bool row, bool flexible)
        {
            if (flexible)
            {
                _le.flexibleWidth = row ? 1f : 0f;
                _le.flexibleHeight = row ? 0f : 1f;
                return;
            }
            if (size == _shown)
                return;
            _shown = size;
            if (row)
                _le.minWidth = _le.preferredWidth = size;
            else
                _le.minHeight = _le.preferredHeight = size;
        }
    }

    /// <summary>A heading inside a card: small capitals in a faint colour, as the window's captions are.</summary>
    internal sealed class CaptionNode : ImNode
    {
        private readonly Text _text;
        private string _shown;

        public CaptionNode(Transform parent)
        {
            Rect = Kit.Node("Cap", parent);
            Kit.Size(Rect.gameObject, -1f, 22f);
            _text = Kit.Text("Text", Rect, "", 10, Pal.C(W.Faint), TextAnchor.LowerLeft, true);
            _text.rectTransform.Css(0f, float.NaN, 0f, 3f, float.NaN, 16f);
        }

        public void Update(string text)
        {
            SetText(_text, ref _shown, (text ?? "").ToUpperInvariant());
        }
    }

    /// <summary>A paragraph that wraps in its width.</summary>
    internal sealed class NoteNode : ImNode
    {
        private readonly Text _text;
        private string _shown;
        private Color _color;

        public NoteNode(Transform parent)
        {
            Rect = Kit.Node("Note", parent);
            Kit.Col(Rect.gameObject, 0f, new RectOffset(0, 0, 3, 3));
            _text = Kit.Paragraph("Text", Rect, "", W.FontSize, Pal.C(W.Dim));
            _text.lineSpacing = 1.2f;
        }

        public void Update(string text, Color color, bool bold)
        {
            SetText(_text, ref _shown, text ?? "");
            if (_color != color)
            {
                _color = color;
                _text.color = color;
            }
            FontStyle style = bold ? FontStyle.Bold : FontStyle.Normal;
            if (_text.fontStyle != style)
                _text.fontStyle = style;
        }
    }

    /// <summary>A label and a text beside it.</summary>
    internal sealed class LabelRowNode : ImNode
    {
        private readonly Text _label, _text;
        private string _shownLabel, _shownText;

        public LabelRowNode(Transform parent)
        {
            Rect = W.Line(parent);
            _label = W.Label(Rect, "");
            _text = Kit.Paragraph("Text", Rect, "", W.FontSize, Pal.C(W.TextC));
            _text.alignment = TextAnchor.MiddleLeft;
            Kit.Flex(_text.gameObject);
            _text.GetComponent<LayoutElement>().minHeight = W.RowHeight;
        }

        public void Update(string label, string text, bool boldLabel, bool boldText)
        {
            SetText(_label, ref _shownLabel, label);
            SetText(_text, ref _shownText, text);
            _label.fontStyle = boldLabel ? FontStyle.Bold : FontStyle.Normal;
            _label.color = boldLabel ? Pal.C(W.TextC) : Pal.C(W.Dim);
            _text.fontStyle = boldText ? FontStyle.Bold : FontStyle.Normal;
        }
    }

    internal sealed class WarningNode : ImNode
    {
        private readonly Text _text;
        private string _shown;

        public WarningNode(Transform parent)
        {
            Image box = Kit.Box("Warning", parent, Pal.Accent(0.10f), 5f);
            Rect = box.rectTransform;
            HorizontalLayoutGroup row = Kit.Row(box.gameObject, 8f, 10f, 10f, TextAnchor.UpperLeft);
            row.padding.top = 7;
            row.padding.bottom = 7;
            Kit.Ring("Ring", box.transform, Pal.Accent(0.45f), 5f).gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Text mark = Kit.Text("Mark", box.transform, "!", 13, Pal.accent, TextAnchor.UpperCenter, true);
            Kit.Size(mark.gameObject, 10f, -1f);
            _text = Kit.Paragraph("Text", box.transform, "", W.FontSize, Pal.C(W.TextC));
            Kit.Flex(_text.gameObject);
        }

        public void Update(string text)
        {
            SetText(_text, ref _shown, text ?? "");
        }
    }

    internal sealed class ImageNode : ImNode
    {
        private readonly RawImage _image;
        private readonly LayoutElement _le;
        private Texture _shown;
        private float _size = -1f;

        public ImageNode(Transform parent)
        {
            Image frame = Kit.Box("Preview", parent, Pal.C(W.Sunk), 4f);
            Rect = frame.rectTransform;
            _le = frame.gameObject.AddComponent<LayoutElement>();
            _image = Kit.Node("Image", Rect).Fill(2f, 2f, 2f, 2f).gameObject.AddComponent<RawImage>();
            _image.raycastTarget = false;
            FlexInRow = false;
        }

        public void Update(Texture texture, float size)
        {
            if (_size != size)
            {
                _size = size;
                _le.minWidth = _le.preferredWidth = size;
                _le.minHeight = _le.preferredHeight = size;
                _le.flexibleWidth = 0f;
            }
            if (_shown != texture)
            {
                _shown = texture;
                _image.texture = texture;
                _image.color = texture != null ? Color.white : Color.clear;
            }
        }
    }
    #endregion

    #region Controls
    /// <summary>A number: a thin track with a knob, and the value in a box beside it that can be typed into.</summary>
    internal sealed class SliderNode : ImNode
    {
        private readonly Text _label;
        private readonly CheckView _check;
        private readonly PaintArea _area;
        private readonly InputField _field;
        private readonly ControlGroup _controls;
        private string _shownLabel;
        private float _drawn = float.NaN;
        private bool _hover, _dragging;
        private float _min, _max = 1f, _value;
        private bool _int;
        private string _format = "N2";
        private Color[] _track;
        private bool _checkClicked;
        private bool _hasPending;
        private float _pending;
        public bool MinusClicked, PlusClicked;

        public SliderNode(Transform parent, bool check, bool steps)
        {
            Rect = W.Line(parent);
            if (check)
            {
                _check = new CheckView(Rect);
                _check.Hit.onClick = () => _checkClicked = true;
            }
            _label = W.Label(Rect, "");
            if (check)
                _label.GetComponent<LayoutElement>().minWidth = _label.GetComponent<LayoutElement>().preferredWidth = W.LabelWidth - 24f;

            _controls = new ControlGroup(Rect);
            Transform ctrl = _controls.Rect;
            Image hit = Kit.Box("Slider", ctrl, Color.clear);
            hit.raycastTarget = true;
            Kit.Flex(hit.gameObject, W.RowHeight);
            RectTransform rect = hit.rectTransform;
            RectTransform areaNode = Kit.Node("Track", rect).Fill();
            _area = areaNode.gameObject.AddComponent<PaintArea>();
            _area.raycastTarget = false;
            _area.draw = a =>
            {
                float zero = _min < 0f && _max > 0f ? Mathf.InverseLerp(_min, _max, 0f) : 0f;
                W.DrawSlider(a, T(_value), zero, _hover || _dragging, _dragging, 1f, _track);
            };

            Func<PointerEventData, float> at = e =>
            {
                Vector2 local;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out local);
                Rect r = rect.rect;
                return Mathf.Clamp01((local.x - r.xMin - W.KnobInset) / Mathf.Max(1f, r.width - W.KnobInset * 2f));
            };
            float startValue = 0f, startT = 0f;
            Clickable press = hit.gameObject.AddComponent<Clickable>();
            press.onDown = e =>
            {
                if (e.button != PointerEventData.InputButton.Left)
                    return;
                startValue = _value;
                startT = at(e);
                if (Shift == false)
                    Push(Mathf.Lerp(_min, _max, startT));
                _dragging = true;
                _area.Redraw();
            };
            DragHandler drag = hit.gameObject.AddComponent<DragHandler>();
            drag.onBeginDrag = e => startValue = _value;
            drag.onDrag = e =>
            {
                float t = at(e);
                if (Shift)
                    Push(Mathf.Clamp(startValue + (t - startT) * (_max - _min) * 0.1f, _min, _max), fine: true);
                else
                {
                    startT = t;
                    Push(Mathf.Lerp(_min, _max, t));
                }
            };
            hit.gameObject.AddComponent<PointerUpHandler>().onPointerUp = e =>
            {
                _dragging = false;
                _area.Redraw();
            };
            hit.gameObject.AddComponent<HoverHandler>().onHover = h =>
            {
                _hover = h;
                _area.Redraw();
            };

            if (steps)
            {
                Clickable minus = W.Pill(ctrl, "Minus", 22f, 0f, 0f, Color.clear, Pal.C(W.Hover), 4f);
                Kit.Size(minus.gameObject, 22f, 22f);
                minus.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
                Kit.Text("Text", minus.transform, "-", 13, Pal.C(W.Dim), TextAnchor.MiddleCenter);
                minus.onClick = () => MinusClicked = true;
            }

            _field = W.Field(ctrl, 56f, TextAnchor.MiddleRight, InputField.ContentType.Standard, s =>
            {
                float v;
                if (W.ParseFloat(s, out v))
                {
                    _hasPending = true;
                    _pending = Mathf.Clamp(_int ? Mathf.Round(v) : v, Mathf.Min(_min, _max), Mathf.Max(_min, _max));
                }
            });

            if (steps)
            {
                Clickable plus = W.Pill(ctrl, "Plus", 22f, 0f, 0f, Color.clear, Pal.C(W.Hover), 4f);
                Kit.Size(plus.gameObject, 22f, 22f);
                plus.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
                Kit.Text("Text", plus.transform, "+", 13, Pal.C(W.Dim), TextAnchor.MiddleCenter);
                plus.onClick = () => PlusClicked = true;
            }
        }

        private static bool Shift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        private float T(float v)
        {
            return _max > _min ? Mathf.InverseLerp(_min, _max, v) : 0f;
        }

        private void Push(float v, bool fine = false)
        {
            if (_int)
                v = Mathf.Round(v);
            else
            {
                int decimals = W.Decimals(_format);
                if (decimals >= 0)
                    v = (float)Math.Round(v, decimals + (fine ? 1 : 0));
            }
            _pending = v;
            _hasPending = true;
            _value = v;
            _area.Redraw();
        }

        /// <summary>Whether the override box was clicked since the last frame.</summary>
        public bool CheckToggled()
        {
            bool c = _checkClicked;
            _checkClicked = false;
            return c;
        }

        public float Update(string label, float value, float min, float max, string format, bool isInt, Color[] track, bool check, bool controlsOn)
        {
            SetText(_label, ref _shownLabel, label);
            _controls.Set(controlsOn);
            if (_check != null)
                _check.Set(check);
            float result = value;
            if (_hasPending)
            {
                result = _pending;
                _hasPending = false;
            }
            _min = min;
            _max = max;
            _value = result;
            _int = isInt;
            _format = format;
            if (_track != track)
            {
                _track = track;
                _drawn = float.NaN;
            }
            string text;
            try
            {
                text = isInt ? Mathf.RoundToInt(result).ToString() : result.ToString(format);
            }
            catch (FormatException)
            {
                text = result.ToString("0.00");
            }
            if (_field.isFocused == false && _field.text != text)
                _field.text = text;
            float t = T(result);
            if (t != _drawn)
            {
                _drawn = t;
                _area.Redraw();
            }
            return result;
        }
    }

    /// <summary>On or off: a label and a switch.</summary>
    internal sealed class ToggleNode : ImNode
    {
        private readonly Text _label, _right;
        private readonly SwitchView _switch;
        private string _shownLabel, _shownRight;
        private bool _clicked;

        public ToggleNode(Transform parent, bool switchFirst)
        {
            Rect = W.Line(parent);
            if (switchFirst)
                _switch = new SwitchView(Rect);
            _label = Kit.Text("L", Rect, "", W.FontSize, Pal.C(W.Dim));
            _label.horizontalOverflow = HorizontalWrapMode.Wrap;
            if (switchFirst)
                Kit.Flex(_label.gameObject, W.RowHeight);
            else
            {
                LayoutElement le = _label.gameObject.AddComponent<LayoutElement>();
                le.minWidth = le.preferredWidth = W.LabelWidth;
                le.minHeight = W.RowHeight;
            }
            if (switchFirst == false)
            {
                _right = Kit.Paragraph("Right", Rect, "", W.FontSize, Pal.C(W.Dim));
                _right.alignment = TextAnchor.MiddleLeft;
                Kit.Flex(_right.gameObject);
                _right.GetComponent<LayoutElement>().minHeight = W.RowHeight;
                _switch = new SwitchView(Rect);
            }
            _switch.Hit.onClick = () => _clicked = true;
            _label.raycastTarget = true;
            Clickable labelClick = _label.gameObject.AddComponent<Clickable>();
            labelClick.onClick = () => _clicked = true;
        }

        public bool Update(string label, bool on, bool bold, string right = null)
        {
            SetText(_label, ref _shownLabel, label);
            _label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            _label.color = bold || on ? Pal.C(W.TextC) : Pal.C(W.Dim);
            if (_right != null)
                SetText(_right, ref _shownRight, right ?? "");
            bool result = on;
            if (_clicked)
            {
                _clicked = false;
                result = !on;
            }
            _switch.Set(result);
            return result;
        }
    }

    /// <summary>A card's head: the switch in front of the effect's name.</summary>
    internal sealed class SwitchHeadNode : ImNode
    {
        private readonly Text _title;
        private readonly SwitchView _switch;
        private string _shown;
        private bool _clicked;

        public SwitchHeadNode(Transform parent)
        {
            Rect = Kit.Node("Head", parent);
            Kit.Row(Rect.gameObject, 8f);
            Kit.Size(Rect.gameObject, -1f, 28f);
            _switch = new SwitchView(Rect);
            _switch.Hit.onClick = () => _clicked = true;
            _title = Kit.Text("Name", Rect, "", 12, Pal.C(W.TextC), TextAnchor.MiddleLeft, true);
            Kit.Flex(_title.gameObject, 28f);
            _title.raycastTarget = true;
            _title.gameObject.AddComponent<Clickable>().onClick = () => _clicked = true;
        }

        public bool Update(string title, bool on)
        {
            SetText(_title, ref _shown, title);
            bool result = on;
            if (_clicked)
            {
                _clicked = false;
                result = !on;
            }
            _switch.Set(result);
            _title.color = result ? Pal.C(W.TextC) : Pal.C(W.Dim);
            return result;
        }
    }

    /// <summary>One of several: segments for a short list, a box that opens a menu for a long one.</summary>
    internal sealed class ChoiceNode : ImNode
    {
        private readonly Text _label;
        private readonly CheckView _check;
        private readonly RectTransform _holder;
        private readonly ControlGroup _controls;
        private string _shownLabel;
        private string _signature;
        private string[] _names = new string[0];
        private readonly List<Clickable> _segments = new List<Clickable>();
        private readonly List<Text> _texts = new List<Text>();
        private Clickable _dropdown;
        private Text _current;
        private string _shownCurrent;
        private int _shownIndex = -2;
        private string _shownDisabled;
        private Func<int, bool> _disabled;
        private int _pending = -1;
        private bool _checkClicked;

        public ChoiceNode(Transform parent, bool check, bool labelled)
        {
            Rect = W.Line(parent);
            if (check)
            {
                _check = new CheckView(Rect);
                _check.Hit.onClick = () => _checkClicked = true;
            }
            if (labelled)
            {
                _label = W.Label(Rect, "");
                if (check)
                    _label.GetComponent<LayoutElement>().minWidth = _label.GetComponent<LayoutElement>().preferredWidth = W.LabelWidth - 24f;
            }
            _controls = new ControlGroup(Rect, 0f);
            _holder = _controls.Rect;
        }

        public bool CheckToggled()
        {
            bool c = _checkClicked;
            _checkClicked = false;
            return c;
        }

        private void Build(string[] names)
        {
            for (int i = _holder.childCount - 1; i >= 0; --i)
                UnityEngine.Object.Destroy(_holder.GetChild(i).gameObject);
            _segments.Clear();
            _texts.Clear();
            _dropdown = null;
            _current = null;
            _shownCurrent = null;
            _shownIndex = -2;
            _shownDisabled = null;
            _names = names;

            int total = 0;
            foreach (string n in names)
                total += n.Length;
            bool segmented = names.Length > 0 && names.Length <= 5 && total <= 42;
            if (segmented)
            {
                Image box = Kit.Box("Seg", _holder, Pal.C(W.Sunk), 5f);
                Kit.Row(box.gameObject, 2f).padding = new RectOffset(2, 2, 2, 2);
                Kit.Flex(box.gameObject, W.RowHeight);
                for (int i = 0; i < names.Length; ++i)
                {
                    int index = i;
                    Clickable seg = W.Pill(box.transform, "Item", 20f, 4f, 0f, Color.clear, Pal.C(W.CardBg), 3f);
                    seg.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
                    Kit.Flex(seg.gameObject, 20f);
                    Text t = Kit.Text("Text", seg.transform, names[i], W.FontSize, Pal.C(W.Dim), TextAnchor.MiddleCenter);
                    _texts.Add(t);
                    seg.onClick = () =>
                    {
                        if (_disabled == null || _disabled(index) == false)
                            _pending = index;
                    };
                    _segments.Add(seg);
                }
                return;
            }

            _dropdown = W.Pill(_holder, "Dropdown", W.RowHeight, 8f, 6f, Pal.C(W.Sunk), Pal.C(0x1B1D21), 5f);
            Kit.Flex(_dropdown.gameObject, W.RowHeight);
            Kit.Ring("Ring", _dropdown.transform, Pal.C(W.SunkEdge), 5f).gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            _current = Kit.Text("Text", _dropdown.transform, "", W.FontSize, Pal.C(W.TextC));
            Kit.Flex(_current.gameObject, W.RowHeight);
            Kit.Icon("Chevron", _dropdown.transform, "down", Pal.C(W.Dim));
            _dropdown.onClick = () =>
            {
                LiteWindow window = LiteWindow.Instance;
                if (window != null)
                    window.OpenMenu((RectTransform)_dropdown.transform, _names, _shownIndex, i => _pending = i, _disabled);
            };
        }

        public int Update(string label, string[] names, int index, Func<int, bool> disabled, bool check, bool controlsOn)
        {
            _controls.Set(controlsOn);
            if (_label != null)
                SetText(_label, ref _shownLabel, label);
            if (_check != null)
                _check.Set(check);
            _disabled = disabled;
            string signature = string.Join("\u0001", names);
            if (signature != _signature)
            {
                _signature = signature;
                Build(names);
            }

            int result = index;
            if (_pending >= 0)
            {
                if (_pending < names.Length)
                    result = _pending;
                _pending = -1;
            }

            string disabledKey = null;
            if (disabled != null && _segments.Count > 0)
            {
                char[] flags = new char[names.Length];
                for (int i = 0; i < names.Length; ++i)
                    flags[i] = disabled(i) ? '1' : '0';
                disabledKey = new string(flags);
            }

            if (result != _shownIndex || disabledKey != _shownDisabled)
            {
                _shownIndex = result;
                _shownDisabled = disabledKey;
                for (int i = 0; i < _segments.Count; ++i)
                {
                    bool on = i == result;
                    bool off = disabledKey != null && disabledKey[i] == '1';
                    _segments[i].disabled = off;
                    _segments[i].normal = on ? Pal.C(W.Selected) : Color.clear;
                    _segments[i].hover = _segments[i].pressed = on ? Pal.C(W.Selected) : off ? Color.clear : Pal.C(W.CardBg);
                    Color tint = off ? Pal.C(W.Faint) : on ? Pal.C(W.TextC) : Pal.C(W.Dim);
                    _segments[i].Tint(tint, off ? tint : Pal.C(W.TextC), _texts[i]);
                }
            }
            if (_current != null)
                SetText(_current, ref _shownCurrent, result >= 0 && result < names.Length ? names[result] : "");
            return result;
        }
    }

    /// <summary>A grid of buttons that stay pressed: a selection grid, a set of flags or a layer mask.</summary>
    internal sealed class GridNode : ImNode
    {
        private readonly List<Clickable> _cells = new List<Clickable>();
        private readonly List<Text> _texts = new List<Text>();
        private string _signature;
        private string _shownState;
        private int _clicked = -1;

        public GridNode(Transform parent)
        {
            Rect = Kit.Node("Grid", parent);
            Kit.Col(Rect.gameObject, 3f);
        }

        private void Build(string[] names, int columns)
        {
            for (int i = Rect.childCount - 1; i >= 0; --i)
                UnityEngine.Object.Destroy(Rect.GetChild(i).gameObject);
            _cells.Clear();
            _texts.Clear();
            _shownState = null;
            columns = Mathf.Max(1, columns);
            RectTransform row = null;
            for (int i = 0; i < names.Length; ++i)
            {
                if (i % columns == 0)
                {
                    row = Kit.Node("Row", Rect);
                    Kit.Row(row.gameObject, 3f);
                }
                int index = i;
                Clickable cell = W.Pill(row, "Cell", 22f, 6f, 0f, Pal.C(W.Sunk), Pal.C(W.Hover), 4f);
                cell.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
                Kit.Flex(cell.gameObject, 22f);
                Text t = Kit.Text("Text", cell.transform, names[i], W.FontSize, Pal.C(W.Dim), TextAnchor.MiddleCenter);
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Truncate;
                Kit.Flex(t.gameObject, 22f);
                cell.onClick = () => _clicked = index;
                _cells.Add(cell);
                _texts.Add(t);
            }
            // The last row's empty places keep the cells of every row the same width.
            if (names.Length % columns != 0 && row != null)
                for (int i = names.Length % columns; i < columns; ++i)
                    Kit.Flex(Kit.Node("Pad", row).gameObject, 22f);
        }

        /// <summary>Shows which cells are on, and gives the one clicked since the last frame, or -1.</summary>
        public int Update(string[] names, Func<int, bool> on, int columns, Func<int, bool> disabled = null)
        {
            string signature = columns + "\u0002" + string.Join("\u0001", names);
            if (signature != _signature)
            {
                _signature = signature;
                Build(names, columns);
            }
            int clicked = _clicked;
            _clicked = -1;
            if (clicked >= 0 && disabled != null && disabled(clicked))
                clicked = -1;

            char[] state = new char[names.Length];
            for (int i = 0; i < names.Length; ++i)
            {
                bool off = disabled != null && disabled(i);
                state[i] = off ? 'x' : on(i) ? '1' : '0';
            }
            string key = new string(state);
            if (key != _shownState)
            {
                _shownState = key;
                for (int i = 0; i < _cells.Count; ++i)
                {
                    bool off = state[i] == 'x';
                    bool sel = state[i] == '1';
                    _cells[i].disabled = off;
                    _cells[i].normal = sel ? Pal.C(W.Selected) : Pal.C(W.Sunk);
                    _cells[i].hover = _cells[i].pressed = sel ? Pal.C(W.Selected) : off ? Pal.C(W.Sunk) : Pal.C(W.Hover);
                    Color tint = off ? Pal.C(W.Faint) : sel ? Pal.C(W.TextC) : Pal.C(W.Dim);
                    _cells[i].Tint(tint, off ? tint : Pal.C(W.TextC), _texts[i]);
                }
            }
            return clicked;
        }
    }

    /// <summary>A grid of pictures to pick from: lens dirt, cubemaps, cookies.</summary>
    internal sealed class ThumbGridNode : ImNode
    {
        private readonly List<Image> _rings = new List<Image>();
        private Texture[] _shown;
        private int _shownColumns = -1, _shownSelected = -2;
        private float _size;
        private int _clicked = -1;

        public ThumbGridNode(Transform parent)
        {
            Rect = Kit.Node("Thumbs", parent);
            Kit.Col(Rect.gameObject, 4f);
        }

        private void Build(Texture[] textures, int columns)
        {
            for (int i = Rect.childCount - 1; i >= 0; --i)
                UnityEngine.Object.Destroy(Rect.GetChild(i).gameObject);
            _rings.Clear();
            _shownSelected = -2;
            RectTransform row = null;
            for (int i = 0; i < textures.Length; ++i)
            {
                if (i % columns == 0)
                {
                    row = Kit.Node("Row", Rect);
                    Kit.Row(row.gameObject, 4f);
                }
                int index = i;
                Clickable cell = W.Pill(row, "Thumb", _size, 0f, 0f, Pal.C(W.Sunk), Pal.C(W.Hover), 4f);
                Kit.Size(cell.gameObject, _size, _size);
                RawImage image = Kit.Node("Image", cell.transform).Fill(3f, 3f, 3f, 3f).gameObject.AddComponent<RawImage>();
                image.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                image.raycastTarget = false;
                image.texture = textures[i];
                image.color = textures[i] != null ? Color.white : Color.clear;
                Image ring = Kit.Ring("Sel", cell.transform, Pal.accent, 4f, 2f);
                ring.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                ring.gameObject.SetActive(false);
                _rings.Add(ring);
                cell.onClick = () => _clicked = index;
            }
        }

        private bool Same(Texture[] textures)
        {
            if (_shown == null || _shown.Length != textures.Length)
                return false;
            for (int i = 0; i < textures.Length; ++i)
                if (_shown[i] != textures[i])
                    return false;
            return true;
        }

        public int Update(Texture[] textures, int selected, float size)
        {
            textures = textures ?? new Texture[0];
            float width = Rect.rect.width > 1f ? Rect.rect.width : LiteWindow.ContentWidth;
            int columns = Mathf.Max(1, Mathf.FloorToInt((width + 4f) / (size + 4f)));
            if (Same(textures) == false || columns != _shownColumns || _size != size)
            {
                _shown = (Texture[])textures.Clone();
                _shownColumns = columns;
                _size = size;
                Build(textures, columns);
            }
            if (selected != _shownSelected)
            {
                _shownSelected = selected;
                for (int i = 0; i < _rings.Count; ++i)
                    _rings[i].gameObject.SetActive(i == selected);
            }
            int clicked = _clicked;
            _clicked = -1;
            return clicked;
        }
    }

    internal sealed class ButtonNode : ImNode
    {
        private readonly Text _text;
        private string _shown;
        private bool _clicked;

        public ButtonNode(Transform parent)
        {
            Clickable b = W.Button(parent, "", out _text);
            Rect = (RectTransform)b.transform;
            b.onClick = () => _clicked = true;
        }

        public bool Update(string text)
        {
            SetText(_text, ref _shown, text);
            bool c = _clicked;
            _clicked = false;
            return c;
        }
    }

    /// <summary>A button that stays pressed while its value is on.</summary>
    internal sealed class ToggleButtonNode : ImNode
    {
        private readonly Clickable _pill;
        private readonly Text _text;
        private string _shown;
        private bool? _shownOn;
        private bool _clicked;

        public ToggleButtonNode(Transform parent)
        {
            _pill = W.Pill(parent, "Toggle", 22f, 8f, 0f, Pal.C(W.Sunk), Pal.C(W.Hover), 4f);
            _pill.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            Kit.Flex(_pill.gameObject, 22f);
            _text = Kit.Text("Text", _pill.transform, "", W.FontSize, Pal.C(W.Dim), TextAnchor.MiddleCenter);
            _text.horizontalOverflow = HorizontalWrapMode.Wrap;
            _text.verticalOverflow = VerticalWrapMode.Truncate;
            Kit.Flex(_text.gameObject, 22f);
            Rect = (RectTransform)_pill.transform;
            _pill.onClick = () => _clicked = true;
        }

        public bool Update(string text, bool on)
        {
            SetText(_text, ref _shown, text);
            bool result = on;
            if (_clicked)
            {
                _clicked = false;
                result = !on;
            }
            if (_shownOn != result)
            {
                _shownOn = result;
                _pill.normal = result ? Pal.C(W.Selected) : Pal.C(W.Sunk);
                _pill.hover = _pill.pressed = result ? Pal.C(W.Selected) : Pal.C(W.Hover);
                _pill.Tint(result ? Pal.C(W.TextC) : Pal.C(W.Dim), Pal.C(W.TextC), _text);
            }
            return result;
        }
    }

    /// <summary>A checkbox with its label after it.</summary>
    internal sealed class CheckToggleNode : ImNode
    {
        private readonly CheckView _check;
        private readonly Text _text;
        private string _shown;
        private bool _clicked;

        public CheckToggleNode(Transform parent)
        {
            Rect = W.Line(parent, 6f);
            _check = new CheckView(Rect);
            _check.Hit.onClick = () => _clicked = true;
            _text = Kit.Text("Text", Rect, "", W.FontSize, Pal.C(W.Dim));
            Kit.Flex(_text.gameObject, W.RowHeight);
            _text.raycastTarget = true;
            _text.gameObject.AddComponent<Clickable>().onClick = () => _clicked = true;
        }

        public bool Update(string text, bool on)
        {
            SetText(_text, ref _shown, text);
            bool result = on;
            if (_clicked)
            {
                _clicked = false;
                result = !on;
            }
            _check.Set(result);
            _text.color = result ? Pal.C(W.TextC) : Pal.C(W.Dim);
            return result;
        }
    }

    /// <summary>A text box: typed into, its value is handed back as typed, or when it is left for a number.</summary>
    internal sealed class FieldNode : ImNode
    {
        private readonly Text _label;
        private readonly CheckView _check;
        private readonly InputField _field;
        private readonly ControlGroup _controls;
        private string _shownLabel;
        private bool _hasPending;
        private string _pending;
        private bool _checkClicked;

        public FieldNode(Transform parent, bool labelled, bool check, bool live, int limit)
        {
            Rect = W.Line(parent);
            if (check)
            {
                _check = new CheckView(Rect);
                _check.Hit.onClick = () => _checkClicked = true;
            }
            if (labelled)
            {
                _label = W.Label(Rect, "");
                if (check)
                    _label.GetComponent<LayoutElement>().minWidth = _label.GetComponent<LayoutElement>().preferredWidth = W.LabelWidth - 24f;
            }
            _controls = new ControlGroup(Rect);
            _field = W.Field(_controls.Rect, -1f, TextAnchor.MiddleLeft, InputField.ContentType.Standard, live ? null : (Action<string>)(s =>
            {
                _pending = s;
                _hasPending = true;
            }));
            if (limit > 0)
                _field.characterLimit = limit;
            if (live)
                _field.onValueChanged.AddListener(s =>
                {
                    _pending = s;
                    _hasPending = true;
                });
        }

        public bool CheckToggled()
        {
            bool c = _checkClicked;
            _checkClicked = false;
            return c;
        }

        public string Update(string label, string text, string placeholder, bool check, bool controlsOn)
        {
            _controls.Set(controlsOn);
            if (_label != null)
                SetText(_label, ref _shownLabel, label);
            if (_check != null)
                _check.Set(check);
            Text hint = (Text)_field.placeholder;
            if (hint.text != (placeholder ?? ""))
                hint.text = placeholder ?? "";
            string result = text ?? "";
            if (_hasPending)
            {
                _hasPending = false;
                result = _pending;
            }
            if (_field.isFocused == false && _field.text != result)
                _field.text = result;
            return result;
        }
    }

    /// <summary>Two to four numbers side by side, X Y Z W.</summary>
    internal sealed class VectorNode : ImNode
    {
        private readonly Text _label;
        private readonly InputField[] _fields;
        private string _shownLabel;
        private readonly float?[] _pending;

        public VectorNode(Transform parent, int size)
        {
            Rect = W.Line(parent, 6f);
            _label = W.Label(Rect, "");
            _fields = new InputField[size];
            _pending = new float?[size];
            for (int i = 0; i < size; ++i)
            {
                int index = i;
                Text axis = Kit.Text("Axis", Rect, "XYZW".Substring(i, 1), 10, Pal.C(W.Faint), TextAnchor.MiddleCenter);
                Kit.Size(axis.gameObject, 10f, W.RowHeight);
                _fields[i] = W.Field(Rect, -1f, TextAnchor.MiddleRight, InputField.ContentType.Standard, s =>
                {
                    float v;
                    if (W.ParseFloat(s, out v))
                        _pending[index] = v;
                });
            }
        }

        public bool Update(string label, float[] values, string format, out float[] result)
        {
            SetText(_label, ref _shownLabel, label);
            result = (float[])values.Clone();
            bool changed = false;
            for (int i = 0; i < _fields.Length && i < values.Length; ++i)
            {
                if (_pending[i].HasValue)
                {
                    result[i] = _pending[i].Value;
                    _pending[i] = null;
                    changed |= result[i] != values[i];
                }
                string text = format == null ? result[i].ToString() : result[i].ToString(format);
                if (_fields[i].isFocused == false && _fields[i].text != text)
                    _fields[i].text = text;
            }
            return changed;
        }
    }

    /// <summary>A colour as a small swatch and its red, green and blue (0 to 255); click it for the picker.</summary>
    internal sealed class ColorNode : ImNode
    {
        private readonly Text _label, _rgb;
        private readonly CheckView _check;
        private readonly Image _swatch;
        private readonly ControlGroup _controls;
        private string _shownLabel, _shownRgb;
        private Color _current, _shownColor;
        private bool _hasPending;
        private Color _pending;
        private bool _checkClicked;

        public ColorNode(Transform parent, bool check)
        {
            Rect = W.Line(parent);
            if (check)
            {
                _check = new CheckView(Rect);
                _check.Hit.onClick = () => _checkClicked = true;
            }
            _label = W.Label(Rect, "");
            if (check)
                _label.GetComponent<LayoutElement>().minWidth = _label.GetComponent<LayoutElement>().preferredWidth = W.LabelWidth - 24f;
            _controls = new ControlGroup(Rect);
            Clickable pick = W.Pill(_controls.Rect, "Pick", W.RowHeight, 0f, 8f, Color.clear, Color.clear, 4f);
            Kit.Flex(pick.gameObject, W.RowHeight);
            _swatch = Kit.Box("Swatch", pick.transform, Color.black, 4f);
            Kit.Size(_swatch.gameObject, 34f, 20f);
            Kit.Ring("Ring", _swatch.transform, new Color(1f, 1f, 1f, 0.15f), 4f);
            _rgb = Kit.Text("Rgb", pick.transform, "", W.FontSize, Pal.C(W.Dim));
            pick.Tint(Pal.C(W.Dim), Pal.C(W.TextC), _rgb);
            pick.onClick = () =>
            {
                LiteWindow window = LiteWindow.Instance;
                if (window == null)
                    return;
                window.OpenPicker((RectTransform)_swatch.transform, new ColorParam(() => _current, c =>
                {
                    _current = c;
                    _pending = c;
                    _hasPending = true;
                }));
            };
        }

        public bool CheckToggled()
        {
            bool c = _checkClicked;
            _checkClicked = false;
            return c;
        }

        public Color Update(string label, Color value, bool check, bool controlsOn)
        {
            _controls.Set(controlsOn);
            SetText(_label, ref _shownLabel, label);
            if (_check != null)
                _check.Set(check);
            Color result = value;
            if (_hasPending)
            {
                _hasPending = false;
                result = _pending;
            }
            _current = result;
            if (result != _shownColor)
            {
                _shownColor = result;
                _swatch.color = new Color(result.r, result.g, result.b, 1f);
                Color32 c = new Color(Mathf.Clamp01(result.r), Mathf.Clamp01(result.g), Mathf.Clamp01(result.b), 1f);
                SetText(_rgb, ref _shownRgb, c.r + ", " + c.g + ", " + c.b);
            }
            return result;
        }
    }
    #endregion
}
