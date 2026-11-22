using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graphics.UI
{
    /// <summary>
    /// The stylesheet of the window (the same palette, sizes and pieces as the HS2PostProcessing
    /// window it is taken from), shared by the window frame and the rows the inspectors draw.
    /// </summary>
    internal static class W
    {
        // The stylesheet's colours, in the reference palette Pal themes from.
        public const uint Chrome = 0x1F2126, PageBg = 0x24272C, CardBg = 0x2A2D33, CardOff = 0x26292E, CardEdge = 0x2F3238;
        public const uint Hover = 0x30333A, Sunk = 0x17191D, SunkEdge = 0x2C2F35, Rule = 0x34373E;
        public const uint TextC = 0xE4E7EC, Dim = 0x9A9DA2, Faint = 0x6B6E74, Selected = 0x464950;

        public const float RowHeight = 24f;
        public const float KnobInset = 6f;
        public const int FontSize = 11;

        /// <summary>The width of a row's label column, set by the window from its own width.</summary>
        public static float LabelWidth = 150f;

        /// <summary>A row of controls with a gap, at least one row high.</summary>
        public static RectTransform Line(Transform parent, float gap = 8f)
        {
            RectTransform line = Kit.Node("Line", parent);
            Kit.Row(line.gameObject, gap);
            LayoutElement le = line.gameObject.AddComponent<LayoutElement>();
            le.minHeight = RowHeight;
            return line;
        }

        public static Clickable Pill(Transform parent, string name, float height, float pad, float gap, Color normal, Color hover, float radius)
        {
            Image box = Kit.Box(name, parent, normal, radius);
            box.raycastTarget = true;
            Kit.Row(box.gameObject, gap, pad, pad);
            Kit.Size(box.gameObject, -1f, height);
            Clickable c = box.gameObject.AddComponent<Clickable>();
            c.background = box;
            c.normal = normal;
            c.hover = hover;
            c.pressed = hover;
            c.Refresh();
            return c;
        }

        /// <summary>A button one row high, filling the row unless sized afterwards.</summary>
        public static Clickable Button(Transform parent, string text, out Text label)
        {
            Clickable b = Pill(parent, "Btn", RowHeight, 10f, 5f, Pal.C(0x393C43), Pal.C(0x44474E), 5f);
            b.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            Kit.Flex(b.gameObject, RowHeight);
            label = Kit.Text("Text", b.transform, text, FontSize, Pal.C(0xD5D8DD), TextAnchor.MiddleCenter);
            return b;
        }

        /// <summary>A 24 by 22 icon button.</summary>
        public static Clickable IconButton(Transform parent, string icon, Action act)
        {
            Clickable b = Pill(parent, "IconBtn", 22f, 0f, 0f, Color.clear, Pal.C(Hover), 4f);
            Kit.Size(b.gameObject, 24f, 22f);
            b.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            IconView view = Kit.Icon("Icon", b.transform, icon, Pal.C(Dim));
            b.Tint(Pal.C(Dim), Pal.C(TextC), view);
            b.onClick = act;
            return b;
        }

        /// <summary>A sunken field with the accent outline while it is being typed into. A width below zero fills the row.</summary>
        public static InputField Field(Transform parent, float width, TextAnchor align, InputField.ContentType type, Action<string> commit, string placeholder = null)
        {
            Image box = Kit.Box("Fld", parent, Pal.C(Sunk), 4f);
            box.raycastTarget = true;
            if (width >= 0f)
                Kit.Size(box.gameObject, width, 22f);
            else
                Kit.Flex(box.gameObject, 22f);
            Kit.Ring("Edge", box.transform, Pal.C(SunkEdge), 4f);

            Text text = Kit.Text("Text", box.transform, "", FontSize, Pal.C(TextC), align);
            ((RectTransform)text.transform).Fill(7f, 0f, 7f, 0f);
            text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;

            InputField field = box.gameObject.AddComponent<InputField>();
            field.textComponent = text;
            Text hint = Kit.Text("Hint", box.transform, placeholder ?? "", FontSize, Pal.C(Faint), align);
            ((RectTransform)hint.transform).Fill(7f, 0f, 7f, 0f);
            field.placeholder = hint;
            field.targetGraphic = box;
            field.transition = Selectable.Transition.None;
            field.lineType = InputField.LineType.SingleLine;
            field.contentType = type;
            field.caretColor = Pal.C(TextC);
            field.selectionColor = Pal.Accent(0.35f);
            if (commit != null)
                field.onEndEdit.AddListener(s => commit(s));

            Image focus = Kit.Ring("Focus", box.transform, Pal.accent, 4f);
            focus.gameObject.SetActive(false);
            FocusRing ring = box.gameObject.AddComponent<FocusRing>();
            ring.field = field;
            ring.ring = focus.gameObject;
            return field;
        }

        /// <summary>A label in the row's label column, wrapping onto a second line when it is long.</summary>
        public static Text Label(Transform line, string text, bool bold = false)
        {
            Text label = Kit.Text("L", line, text, FontSize, Pal.C(Dim), TextAnchor.MiddleLeft, bold);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            LayoutElement le = label.gameObject.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = LabelWidth;
            le.flexibleWidth = 0f;
            le.minHeight = RowHeight;
            return label;
        }

        public static RectTransform Card(Transform parent, bool inner)
        {
            Image card = Kit.Box(inner ? "Panel" : "Card", parent, inner ? Pal.C(CardOff) : Pal.C(CardBg), 6f);
            Kit.Ring("Ring", card.transform, Pal.C(inner ? SunkEdge : CardEdge), 6f).gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            return card.rectTransform;
        }

        public static void DrawSlider(PaintArea a, float t, float zero, bool hot, bool dragging, float alpha, Color[] track = null)
        {
            Color under = Pal.C(CardOff);
            Func<Color, Color> fade = c => alpha >= 1f ? c : Color.Lerp(under, c, alpha);
            float x0 = KnobInset, len = Mathf.Max(1f, a.width - x0 * 2f), cy = Mathf.Round(a.height * 0.5f);
            Func<float, float> x = v => x0 + v * len;

            float outer = 5f, inner = 3f;
            a.RoundRect(x0 - 1f, cy - outer * 0.5f, len + 2f, outer, outer * 0.5f, fade(Pal.C(Rule)));
            a.RoundRect(x0, cy - inner * 0.5f, len, inner, inner * 0.5f, fade(Pal.C(0x15171B)));
            if (zero > 0f && zero < 1f)
                a.Rect(Mathf.Round(x(zero)) - 0.5f, cy - 4.5f, 1f, 9f, fade(Pal.C(0x5A5D63)));
            if (track != null && track.Length > 1)
            {
                // A track that shows what the value does, from one end's colour to the other's.
                float step = len / (track.Length - 1);
                for (int i = 0; i < track.Length - 1; ++i)
                {
                    Color left = fade(track[i]), right = fade(track[i + 1]);
                    a.Gradient(x0 + i * step, cy - inner * 0.5f, step, inner, left, right, right, left);
                }
            }
            else
            {
                float from = x(Mathf.Min(zero, t)), to = x(Mathf.Max(zero, t));
                if (to - from > 0.5f)
                    a.RoundRect(from, cy - inner * 0.5f, to - from, inner, inner * 0.5f, fade(Pal.accent));
            }

            float k = x(t);
            if (hot)
                a.Circle(k, cy, dragging ? 10f : 9f, Pal.Accent(0.28f * alpha));
            a.Circle(k, cy + 1f, 6.5f, new Color(0f, 0f, 0f, 0.45f * alpha));
            a.Circle(k, cy, 6f, fade(new Color(0.914f, 0.922f, 0.937f, 1f)));
        }

        /// <summary>Rounds to the precision a value is shown at, for formats such as N2, 0.00 or F1.</summary>
        public static float Snap(float v, string format)
        {
            int decimals = Decimals(format);
            return decimals < 0 ? v : (float)Math.Round(v, decimals);
        }

        public static int Decimals(string format)
        {
            if (string.IsNullOrEmpty(format))
                return -1;
            char first = char.ToUpperInvariant(format[0]);
            if ((first == 'N' || first == 'F') && format.Length > 1)
            {
                int d;
                return int.TryParse(format.Substring(1), out d) ? d : -1;
            }
            int dot = format.IndexOf('.');
            if (format.IndexOf('#') >= 0 || format.IndexOf('0') >= 0)
                return dot < 0 ? 0 : format.Length - dot - 1;
            return -1;
        }

        public static bool ParseFloat(string s, out float value)
        {
            if (float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out value))
                return true;
            return float.TryParse(s.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        public static string Localize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? "";
            return LocalizationManager.HasLocalization() ? LocalizationManager.Localized(text) : text;
        }

        public static readonly Color[] TempTrack = { new Color(0.32f, 0.55f, 1f), new Color(0.86f, 0.86f, 0.86f), new Color(1f, 0.68f, 0.24f) };
        public static readonly Color[] TintTrack = { new Color(0.35f, 0.82f, 0.38f), new Color(0.86f, 0.86f, 0.86f), new Color(0.88f, 0.36f, 0.9f) };
        public static readonly Color[] HueTrack =
        {
            new Color(1f, 0.2f, 0.2f), new Color(1f, 1f, 0.2f), new Color(0.2f, 1f, 0.2f), new Color(0.2f, 1f, 1f),
            new Color(0.2f, 0.2f, 1f), new Color(1f, 0.2f, 1f), new Color(1f, 0.2f, 0.2f)
        };
        public static readonly Color[] VibTrack = { new Color(0.5f, 0.5f, 0.5f), new Color(0.95f, 0.3f, 0.45f) };
        public static readonly Color[] AlphaTrack = { new Color(0.08f, 0.08f, 0.08f), new Color(0.92f, 0.92f, 0.92f) };
        public static readonly Color[] ReverseAlphaTrack = { new Color(0.6f, 0.6f, 0.6f), new Color(0.08f, 0.08f, 0.08f), new Color(0.95f, 0.95f, 0.95f) };
        public static readonly Color[] RedTrack = { new Color(0.08f, 0.08f, 0.08f), new Color(0.95f, 0.25f, 0.25f) };
        public static readonly Color[] GreenTrack = { new Color(0.08f, 0.08f, 0.08f), new Color(0.3f, 0.9f, 0.3f) };
        public static readonly Color[] BlueTrack = { new Color(0.08f, 0.08f, 0.08f), new Color(0.3f, 0.45f, 1f) };
    }

    /// <summary>The accent outline a field gets while it is being typed into.</summary>
    internal class FocusRing : MonoBehaviour
    {
        public InputField field;
        public GameObject ring;

        private void Update()
        {
            bool on = field != null && field.isFocused;
            if (ring != null && ring.activeSelf != on)
                ring.SetActive(on);
        }
    }

    /// <summary>An on/off switch: a 28 by 16 pill whose knob slides right and takes the accent when on.</summary>
    internal class SwitchView
    {
        public readonly Clickable Hit;
        private readonly Image _track, _ring, _knob;
        private bool? _shown;
        private float _slideStart = -1f;
        private bool _slideFrom;
        private const float SlideTime = 0.12f;

        public SwitchView(Transform parent)
        {
            Hit = W.Pill(parent, "Switch", W.RowHeight, 2f, 0f, Color.clear, Color.clear, 3f);
            Hit.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            Kit.Size(Hit.gameObject, 32f, W.RowHeight);
            _track = Kit.Box("Track", Hit.transform, Pal.C(0x15171B), 8f);
            Kit.Size(_track.gameObject, 28f, 16f);
            _ring = Kit.Ring("Ring", _track.transform, Pal.C(0x3D4047), 8f);
            _knob = Kit.Box("Knob", _track.transform, Pal.C(0x6D7077), 5f);
            RectTransform k = _knob.rectTransform;
            k.anchorMin = k.anchorMax = k.pivot = new Vector2(0f, 0.5f);
            k.sizeDelta = new Vector2(10f, 10f);
        }

        public void Set(bool on)
        {
            if (_shown == on)
            {
                Step();
                return;
            }
            if (_shown.HasValue)
            {
                _slideFrom = _shown.Value;
                _slideStart = Time.unscaledTime;
            }
            _shown = on;
            _track.color = on ? Pal.accent : Pal.C(0x15171B);
            _ring.gameObject.SetActive(on == false);
            _knob.color = on ? Pal.onAccent : Pal.C(0x6D7077);
            Step();
        }

        private void Step()
        {
            bool on = _shown ?? false;
            float target = on ? 15f : 3f;
            float x = target;
            if (_slideStart >= 0f)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - _slideStart) / SlideTime);
                x = Mathf.Lerp(_slideFrom ? 15f : 3f, target, t * (2f - t));
                if (t >= 1f)
                    _slideStart = -1f;
            }
            RectTransform k = _knob.rectTransform;
            if (Mathf.Abs(k.anchoredPosition.x - x) > 0.01f)
                k.anchoredPosition = new Vector2(x, 0f);
        }
    }

    /// <summary>A 14 pixel checkbox: the override box in front of a post-processing parameter.</summary>
    internal class CheckView
    {
        public readonly Clickable Hit;
        private readonly Image _box, _ring;
        private readonly IconView _tick;
        private bool? _shown;

        public CheckView(Transform parent)
        {
            Hit = W.Pill(parent, "Check", W.RowHeight, 0f, 0f, Color.clear, Color.clear, 3f);
            Hit.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            Kit.Size(Hit.gameObject, 16f, W.RowHeight);
            _box = Kit.Box("Box", Hit.transform, Pal.C(0x15171B), 3f);
            Kit.Size(_box.gameObject, 14f, 14f);
            _ring = Kit.Ring("Ring", _box.transform, Pal.C(0x4A4D54), 3f);
            RectTransform tickNode = Kit.Node("TickHolder", _box.transform).Fill();
            _tick = Kit.Icon("Tick", tickNode, "check", Pal.onAccent, 0.9f);
            RectTransform tr = (RectTransform)_tick.transform;
            tr.anchorMin = tr.anchorMax = tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = Vector2.zero;
        }

        public void Set(bool on)
        {
            if (_shown == on)
                return;
            _shown = on;
            _box.color = on ? Pal.accent : Pal.C(0x15171B);
            _ring.gameObject.SetActive(on == false);
            _tick.gameObject.SetActive(on);
        }
    }
}
