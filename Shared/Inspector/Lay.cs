using Graphics.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Graphics.Inspector
{
    /// <summary>
    /// What the inspectors called on GUILayout, drawn by the window's immediate-mode layer instead.
    /// The calls keep their shape, so the inspectors' code reads as it did.
    /// </summary>
    internal static class Lay
    {
        /// <summary>The look of a box the inspectors open: a card, or a plain column.</summary>
        internal sealed class Style
        {
            public readonly bool Panel;
            public readonly float Width;

            private Style(bool panel, float width = -1f)
            {
                Panel = panel;
                Width = width;
            }

            /// <summary>A card on the page; inside another card, a sunken panel.</summary>
            public static readonly Style Card = new Style(true);
            /// <summary>No box of its own: a column.</summary>
            public static readonly Style Plain = new Style(false);

            public static Style Fixed(float width, bool panel = true)
            {
                return new Style(panel, width);
            }
        }

        internal struct Opt
        {
            public enum Kind { Width, Height, MaxHeight, ExpandWidth, ExpandHeight, MinWidth }
            public Kind Type;
            public float Value;
        }

        public static Opt Width(float w) { return new Opt { Type = Opt.Kind.Width, Value = w }; }
        public static Opt MinWidth(float w) { return new Opt { Type = Opt.Kind.MinWidth, Value = w }; }
        public static Opt Height(float h) { return new Opt { Type = Opt.Kind.Height, Value = h }; }
        public static Opt MaxHeight(float h) { return new Opt { Type = Opt.Kind.MaxHeight, Value = h }; }
        public static Opt ExpandWidth(bool on) { return new Opt { Type = Opt.Kind.ExpandWidth, Value = on ? 1f : 0f }; }
        public static Opt ExpandHeight(bool on) { return new Opt { Type = Opt.Kind.ExpandHeight, Value = on ? 1f : 0f }; }

        private static void Apply(ImNode node, Opt[] options, float styleWidth = -1f)
        {
            if ((options == null || options.Length == 0) && styleWidth < 0f)
                return;
            LayoutElement le = node.Rect.GetComponent<LayoutElement>() ?? node.Rect.gameObject.AddComponent<LayoutElement>();
            if (styleWidth >= 0f)
            {
                le.minWidth = le.preferredWidth = styleWidth;
                le.flexibleWidth = 0f;
            }
            if (options == null)
                return;
            foreach (Opt o in options)
            {
                switch (o.Type)
                {
                    case Opt.Kind.Width:
                        le.minWidth = le.preferredWidth = o.Value;
                        le.flexibleWidth = 0f;
                        break;
                    case Opt.Kind.MinWidth:
                        le.minWidth = o.Value;
                        break;
                    case Opt.Kind.Height:
                        le.minHeight = le.preferredHeight = o.Value;
                        break;
                    case Opt.Kind.ExpandWidth:
                        if (o.Value > 0f && le.preferredWidth < 0f)
                            le.flexibleWidth = 1f;
                        break;
                }
            }
        }

        private static float MaxHeightOf(Opt[] options, float fallback)
        {
            if (options != null)
                foreach (Opt o in options)
                    if (o.Type == Opt.Kind.MaxHeight || o.Type == Opt.Kind.Height)
                        return o.Value;
            return fallback;
        }

        #region Boxes
        public static void BeginVertical(params Opt[] options)
        {
            ImNode n = Im.BeginContainer("col", p => new ColumnNode(p, 4f));
            Apply(n, options);
        }

        public static void BeginVertical(Style style, params Opt[] options)
        {
            if (style == null || style.Panel == false)
            {
                ImNode col = Im.BeginContainer("col", p => new ColumnNode(p, 4f));
                Apply(col, options, style != null ? style.Width : -1f);
                return;
            }
            bool inner = Im.CardDepth > 0;
            ImNode n = inner ? Im.BeginContainer("inner", p => new CardNode(p, true)) : Im.BeginContainer("card", p => new CardNode(p, false));
            Apply(n, options, style.Width);
        }

        public static void EndVertical()
        {
            Im.EndContainer();
        }

        private static readonly System.Collections.Generic.List<bool> _rows = new System.Collections.Generic.List<bool>();

        /// <summary>Forgets rows left open by a page that threw halfway: called before each frame's drawing.</summary>
        public static void Reset()
        {
            _rows.Clear();
        }

        public static void BeginHorizontal(params Opt[] options)
        {
            ImNode n = Im.BeginContainer("row", p => new RowNode(p));
            Apply(n, options);
            _rows.Add(false);
        }

        /// <summary>A row in a box: the box is a card's or a panel's, its content laid out in a row.</summary>
        public static void BeginHorizontal(Style style, params Opt[] options)
        {
            BeginVertical(style, options);
            Im.BeginContainer("row", p => new RowNode(p));
            _rows.Add(true);
        }

        public static void EndHorizontal()
        {
            bool styled = false;
            if (_rows.Count > 0)
            {
                styled = _rows[_rows.Count - 1];
                _rows.RemoveAt(_rows.Count - 1);
            }
            Im.EndContainer();
            if (styled)
                Im.EndContainer();
        }

        /// <summary>
        /// A scroll view. The page scrolls on its own, so a scroll view is only a list of its own when it
        /// is given a height; without one, its content is simply part of the page.
        /// </summary>
        public static Vector2 BeginScrollView(Vector2 position, params Opt[] options)
        {
            float max = MaxHeightOf(options, -1f);
            if (max > 0f)
            {
                ScrollNode s = Im.BeginContainer("scroll", p => new ScrollNode(p));
                s.MaxHeight = max;
            }
            else
                Im.BeginContainer("col", p => new ColumnNode(p, 4f));
            return position;
        }

        public static Vector2 BeginScrollView(Vector2 position, bool alwaysHorizontal, bool alwaysVertical, params Opt[] options)
        {
            return BeginScrollView(position, options);
        }

        public static void EndScrollView()
        {
            Im.EndContainer();
        }
        #endregion

        #region Spacing
        public static void Space(float pixels)
        {
            bool row = Im.InRow;
            float size;
            if (row)
                size = Mathf.Min(pixels, 16f);
            else
                size = pixels >= 20f ? 8f : pixels >= 10f ? 4f : pixels > 0f ? 2f : 0f;
            if (size <= 0f)
                return;
            SpaceNode n = Im.Next("space", p => new SpaceNode(p));
            n.Update(size, row, false);
        }

        public static void FlexibleSpace()
        {
            bool row = Im.InRow;
            SpaceNode n = Im.Next("flex", p => new SpaceNode(p));
            n.Update(0f, row, true);
        }
        #endregion

        #region Controls
        public static bool Button(string text, params Opt[] options)
        {
            ButtonNode n = Im.Next("button", p => new ButtonNode(p));
            Apply(n, options);
            return n.Update(text);
        }

        public static void Label(string text, params Opt[] options)
        {
            NoteNode n = Im.Next("note", p => new NoteNode(p));
            n.Update(text, Pal.C(W.Dim), false);
        }

        /// <summary>An empty box of a size, as a picture's place before there is a picture.</summary>
        public static void Box(GUIContent content, params Opt[] options)
        {
            float size = 64f;
            if (options != null)
                foreach (Opt o in options)
                    if (o.Type == Opt.Kind.Width)
                        size = o.Value;
            ImageNode n = Im.Next("image", p => new ImageNode(p));
            n.Update(null, size);
        }

        /// <summary>A picture of a size.</summary>
        public static void Image(Texture texture, float size)
        {
            ImageNode n = Im.Next("image", p => new ImageNode(p));
            n.Update(texture, size);
        }

        /// <summary>A toggle drawn as a button, GUILayout.Toggle(value, text, "Button").</summary>
        public static bool Toggle(bool value, string text, string buttonStyle, params Opt[] options)
        {
            ToggleButtonNode n = Im.Next("tbutton", p => new ToggleButtonNode(p));
            Apply(n, options);
            return n.Update(text, value);
        }

        /// <summary>A checkbox, GUILayout.Toggle(value, text).</summary>
        public static bool Toggle(bool value, string text, params Opt[] options)
        {
            CheckToggleNode n = Im.Next("check", p => new CheckToggleNode(p));
            return n.Update(text, value);
        }

        /// <summary>A grid of buttons, one of which is selected: the one clicked, or the selection as it was.</summary>
        public static int SelectionGrid(int selected, string[] names, int columns, params Opt[] options)
        {
            GridNode n = Im.Next("grid", p => new GridNode(p));
            int clicked = n.Update(names, i => i == selected, Mathf.Max(1, columns));
            return clicked >= 0 ? clicked : selected;
        }

        /// <summary>A grid of pictures, one of which is selected.</summary>
        public static int SelectionGrid(int selected, Texture[] textures, int columns, float size = 96f)
        {
            ThumbGridNode n = Im.Next("thumbs", p => new ThumbGridNode(p));
            int clicked = n.Update(textures, selected, size);
            return clicked >= 0 ? clicked : selected;
        }
        #endregion
    }
}
