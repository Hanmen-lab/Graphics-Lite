// Adapted from Timeline 2.0 (HSPlugins, Timeline.Core/View/Icons.cs), MIT License,
// Copyright (c) 2021 DeathWeasel1337 and contributors. Namespace changed; otherwise as upstream.
using System.Collections.Generic;
using UnityEngine;

namespace Graphics.UI
{
    /// <summary>
    /// The playground's icons, from its own SVG markup: same paths, same stroke widths, same boxes.
    /// Each is drawn as vectors, so it is as sharp at 200 % as at 100 %.
    /// </summary>
    internal static class Icons
    {
        private delegate void Drawer(Paint p, Vector2 o, float k, Color c);

        private class Def
        {
            public float width, height;
            public Drawer draw;
        }

        private static readonly Dictionary<string, Def> _defs = new Dictionary<string, Def>
        {
            { "down", D(8, 8, 1f, (p, o, k, c) => S(p, o, k, c, "M1 2.5l3 3 3-3", 1.3f)) },
            { "right", D(8, 8, 1f, (p, o, k, c) => S(p, o, k, c, "M2.5 1l3 3-3 3", 1.3f)) },
            { "eye", D(14, 10, 1f, (p, o, k, c) =>
                {
                    S(p, o, k, c, "M1 5s2.4-4 6-4 6 4 6 4-2.4 4-6 4-6-4-6-4z", 1.3f);
                    Dot(p, o, k, c, 7f, 5f, 1.8f);
                }) },
            { "eyeoff", D(14, 10, 1f, (p, o, k, c) =>
                {
                    S(p, o, k, c, "M1 5s2.4-4 6-4 6 4 6 4-2.4 4-6 4-6-4-6-4z", 1.3f);
                    S(p, o, k, c, "M2 9L12 1", 1.4f);
                }) },
            { "lock", D(10, 12, 1f, (p, o, k, c) =>
                {
                    p.RoundRect(o.x + 1f * k, o.y + 5f * k, 8f * k, 6f * k, 1f * k, c);
                    S(p, o, k, c, "M3 5V3.5a2 2 0 014 0V5", 1.3f);
                }) },
            { "unlock", D(10, 12, 1f, (p, o, k, c) =>
                {
                    Box(p, o, k, c, 1f, 5f, 8f, 6f, 1.2f);
                    S(p, o, k, c, "M3 5V3.5a2 2 0 013.6-1.2", 1.2f);
                }) },
            { "magnet", D(11, 11, 1f, (p, o, k, c) => S(p, o, k, c, "M2 1v5a3.5 3.5 0 007 0V1", 2f)) },
            { "path", D(12, 10, 1f, (p, o, k, c) =>
                {
                    S(p, o, k, c, "M1 8C3 1 7 1 11 6", 1.3f);
                    Dot(p, o, k, c, 1.5f, 8f, 1.3f);
                    Dot(p, o, k, c, 10.5f, 6f, 1.3f);
                }) },
            { "undo", D(12, 12, 12f / 14f, (p, o, k, c) => S(p, o, k, c, "M5 3L2 6l3 3M2 6h6.5a3.5 3.5 0 010 7H6", 1.5f)) },
            { "redo", D(12, 12, 12f / 14f, (p, o, k, c) => S(p, o, k, c, "M9 3l3 3-3 3M12 6H5.5a3.5 3.5 0 000 7H8", 1.5f)) },
            { "more", D(12, 12, 1f, (p, o, k, c) =>
                {
                    Dot(p, o, k, c, 2.5f, 6f, 1.2f);
                    Dot(p, o, k, c, 6f, 6f, 1.2f);
                    Dot(p, o, k, c, 9.5f, 6f, 1.2f);
                }) },
            { "close", D(10, 10, 10f / 12f, (p, o, k, c) => S(p, o, k, c, "M2 2l8 8M10 2l-8 8", 1.6f)) },
            { "search", D(11, 11, 1f, (p, o, k, c) =>
                {
                    Ring(p, o, k, c, 4.5f, 4.5f, 3.3f, 1.3f);
                    S(p, o, k, c, "M7 7l3 3", 1.3f);
                }) },
            { "palette", D(13, 12, 1f, (p, o, k, c) =>
                {
                    S(p, o, k, c, "M6.5 1C3.4 1 1 3.2 1 6s2.3 5 5 5c.9 0 1.3-.6 1-1.3-.3-.8.2-1.5 1.1-1.5h1.4C11 8.2 12 7.3 12 5.8 12 3.1 9.6 1 6.5 1z", 1.2f);
                    Dot(p, o, k, c, 3.8f, 5.2f, 0.9f);
                    Dot(p, o, k, c, 6f, 3.4f, 0.9f);
                    Dot(p, o, k, c, 8.8f, 4f, 0.9f);
                    Ring(p, o, k, c, 4.4f, 8.3f, 1f, 1f);
                }) },
            { "popout", D(11, 11, 11f / 12f, (p, o, k, c) => S(p, o, k, c, "M7 1h4v4M11 1L6 6M9 7.5V11H1V3h3.5", 1.3f)) },
            { "min", D(10, 10, 10f / 12f, (p, o, k, c) => S(p, o, k, c, "M2 9h8", 1.6f)) },
            { "side", D(12, 10, 1f, (p, o, k, c) =>
                {
                    Box(p, o, k, c, 1f, 1f, 10f, 8f, 1.2f);
                    p.Rect(o.x + 7f * k, o.y + 1f * k, 4f * k, 8f * k, c);
                }) },
            { "play", D(12, 12, 1f, (p, o, k, c) => F(p, o, k, c, "M3 1.5l7 4.5-7 4.5z")) },
            { "pause", D(12, 12, 1f, (p, o, k, c) =>
                {
                    p.Rect(o.x + 2.5f * k, o.y + 1.5f * k, 2.6f * k, 9f * k, c);
                    p.Rect(o.x + 7f * k, o.y + 1.5f * k, 2.6f * k, 9f * k, c);
                }) },
            // Transport, drawn 10 wide in 12 unit boxes (.tb2 svg{width:10px;height:10px}).
            { "start", D(10, 10, 10f / 12f, (p, o, k, c) =>
                {
                    S(p, o, k, c, "M2 2v8", 1.4f);
                    F(p, o, k, c, "M10 2L4 6l6 4z");
                    S(p, o, k, c, "M10 2L4 6l6 4z", 1.4f);
                }) },
            { "end", D(10, 10, 10f / 12f, (p, o, k, c) =>
                {
                    S(p, o, k, c, "M10 2v8", 1.4f);
                    F(p, o, k, c, "M2 2l6 4-6 4z");
                    S(p, o, k, c, "M2 2l6 4-6 4z", 1.4f);
                }) },
            { "prevkey", D(10, 10, 10f / 12f, (p, o, k, c) =>
                {
                    p.Diamond(o.x + 6.75f * k, o.y + 5.25f * k, 4.5f * k, c);
                    S(p, o, k, c, "M1.5 6h2.5", 1.3f);
                }) },
            { "nextkey", D(10, 10, 10f / 12f, (p, o, k, c) =>
                {
                    p.Diamond(o.x + 5.25f * k, o.y + 5.25f * k, 4.5f * k, c);
                    S(p, o, k, c, "M8 6h2.5", 1.3f);
                }) },
            { "playT", D(10, 10, 10f / 12f, (p, o, k, c) => F(p, o, k, c, "M3 1.5l7 4.5-7 4.5z")) },
            { "pauseT", D(10, 10, 10f / 12f, (p, o, k, c) =>
                {
                    p.Rect(o.x + 2.5f * k, o.y + 1.5f * k, 2.6f * k, 9f * k, c);
                    p.Rect(o.x + 7f * k, o.y + 1.5f * k, 2.6f * k, 9f * k, c);
                }) },
            // Editors, as on the editor picker.
            { "dope", D(11, 11, 11f / 12f, (p, o, k, c) => p.Diamond(o.x + 6f * k, o.y + 6f * k, 6f * k, c)) },
            { "graph", D(13, 11, 13f / 14f, (p, o, k, c) => S(p, o, k, c, "M1 10C5 10 5 2 9 2s3 3 4 3", 1.6f)) },
            { "nla", D(13, 11, 13f / 14f, (p, o, k, c) =>
                {
                    p.RoundRect(o.x + 1f * k, o.y + 2f * k, 8f * k, 3f * k, 1f * k, c);
                    p.RoundRect(o.x + 5f * k, o.y + 7f * k, 8f * k, 3f * k, 1f * k, c);
                }) },
            { "target", D(12, 12, 1f, (p, o, k, c) =>
                {
                    Ring(p, o, k, c, 6f, 6f, 4.5f, 1.3f);
                    Dot(p, o, k, c, 6f, 6f, 1.6f);
                }) },
            { "plus", D(9, 9, 1f, (p, o, k, c) => S(p, o, k, c, "M4.5 1v7M1 4.5h7", 1.6f)) },
            { "sets", D(12, 12, 1f, (p, o, k, c) =>
                {
                    p.Diamond(o.x + 3.4f * k, o.y + 3.4f * k, 3.6f * k, c);
                    p.Diamond(o.x + 8.6f * k, o.y + 3.4f * k, 3.6f * k, c);
                    S(p, o, k, c, "M1 9.5h10", 1.6f);
                }) },
            { "picker", D(12, 12, 1f, (p, o, k, c) =>
                {
                    p.RoundRect(o.x + 4f * k, o.y + 1f * k, 4f * k, 3f * k, 1f * k, c);
                    p.RoundRect(o.x + 1f * k, o.y + 6f * k, 3f * k, 2.5f * k, 1f * k, c);
                    p.RoundRect(o.x + 8f * k, o.y + 6f * k, 3f * k, 2.5f * k, 1f * k, c);
                    p.RoundRect(o.x + 4.2f * k, o.y + 8f * k, 3.6f * k, 3f * k, 1f * k, c);
                }) },
            { "check", D(10, 10, 1f, (p, o, k, c) => S(p, o, k, c, "M1.5 5.2l2.4 2.4 4.6-5", 1.6f)) },

            // The rail's tabs, drawn on a 20 unit grid and shown at 15 pixels.
            { "tab-environment", D(15, 15, 0.75f, (p, o, k, c) => S(p, o, k, c, "M3 15h14M5 12a5 5 0 0 1 10 0M10 3v2M4 6l1.4 1.4M16 6l-1.4 1.4", 1.5f)) },
            { "tab-lights", D(15, 15, 0.75f, (p, o, k, c) => S(p, o, k, c, "M10 3a5 5 0 0 0-3 9v2h6v-2a5 5 0 0 0-3-9zM8 17h4", 1.5f)) },
            { "tab-gi", D(15, 15, 0.75f, (p, o, k, c) =>
                {
                    Ring(p, o, k, c, 10f, 10f, 6f, 1.5f);
                    S(p, o, k, c, "M4 10h12M10 4c2.5 2 2.5 10 0 12M10 4c-2.5 2-2.5 10 0 12", 1.5f);
                }) },
            { "tab-reflections", D(15, 15, 0.75f, (p, o, k, c) =>
                {
                    Box(p, o, k, c, 3.5f, 3.5f, 13f, 13f, 1.5f);
                    S(p, o, k, c, "M6.5 13.5l7-7M10 13.5l3.5-3.5", 1.5f);
                }) },
            { "tab-post", D(15, 15, 0.75f, (p, o, k, c) =>
                {
                    Ring(p, o, k, c, 10f, 10f, 6.5f, 1.5f);
                    S(p, o, k, c, "M10 3.5v13M4.4 6.75l11.2 6.5M4.4 13.25l11.2-6.5", 1.5f);
                }) },
            { "tab-aa", D(15, 15, 0.75f, (p, o, k, c) => S(p, o, k, c, "M3 16l4-12 4 12M4.5 11.5h5M13 16l3-8", 1.5f)) },
            { "tab-presets", D(15, 15, 0.75f, (p, o, k, c) => S(p, o, k, c, "M5 3h10v14l-5-3.5-5 3.5z", 1.5f)) },
            { "tab-skin", D(15, 15, 0.75f, (p, o, k, c) =>
                {
                    S(p, o, k, c, "M10 3c3 3.5 5 6 5 8.5a5 5 0 0 1-10 0C5 9 7 6.5 10 3z", 1.5f);
                    S(p, o, k, c, "M8 12.5a2 2 0 0 0 2 2", 1.5f);
                }) },
            { "tab-camera", D(15, 15, 0.75f, (p, o, k, c) =>
                {
                    S(p, o, k, c, "M3 6.5h3.5L8 4.5h4l1.5 2H17v9H3z", 1.5f);
                    Ring(p, o, k, c, 10f, 11f, 2.8f, 1.5f);
                }) },
            { "tab-settings", D(15, 15, 0.75f, (p, o, k, c) =>
                {
                    Ring(p, o, k, c, 10f, 10f, 2.6f, 1.5f);
                    Ring(p, o, k, c, 10f, 10f, 5.6f, 1.5f);
                    S(p, o, k, c, "M10 2v2.4M10 15.6V18M2 10h2.4M15.6 10H18M4.3 4.3l1.7 1.7M14 14l1.7 1.7M4.3 15.7L6 14M14 6l1.7-1.7", 1.5f);
                }) },
        };

        private static Def D(float w, float h, float k, Drawer draw)
        {
            return new Def { width = w, height = h, draw = (p, o, s, c) => draw(p, o, s * k, c) };
        }

        public static bool Has(string name)
        {
            return name != null && _defs.ContainsKey(name);
        }

        public static Vector2 Size(string name)
        {
            Def def;
            return _defs.TryGetValue(name, out def) ? new Vector2(def.width, def.height) : Vector2.zero;
        }

        /// <summary>Draws an icon with its top left corner at (x, y), scaled by k from its own size.</summary>
        public static void Draw(Paint p, string name, float x, float y, Color color, float k = 1f)
        {
            Def def;
            if (_defs.TryGetValue(name, out def))
                def.draw(p, new Vector2(x, y), k, color);
        }

        /// <summary>Draws an icon centred on (x, y).</summary>
        public static void DrawCentred(Paint p, string name, float x, float y, Color color, float k = 1f)
        {
            Def def;
            if (_defs.TryGetValue(name, out def))
                def.draw(p, new Vector2(x - def.width * k * 0.5f, y - def.height * k * 0.5f), k, color);
        }

        #region Primitives in icon units
        private static readonly List<Vector2> _points = new List<Vector2>();

        private static void S(Paint p, Vector2 o, float k, Color c, string d, float width)
        {
            Svg.Shape shape = Svg.Parse(d);
            for (int i = 0; i < shape.parts.Count; ++i)
            {
                Place(shape.parts[i], o, k);
                p.Stroke(_points, width * k, c, shape.closed[i]);
            }
        }

        private static void F(Paint p, Vector2 o, float k, Color c, string d)
        {
            Svg.Shape shape = Svg.Parse(d);
            foreach (List<Vector2> part in shape.parts)
            {
                Place(part, o, k);
                p.Convex(_points, c);
            }
        }

        private static void Place(List<Vector2> part, Vector2 o, float k)
        {
            _points.Clear();
            foreach (Vector2 v in part)
                _points.Add(o + v * k);
        }

        private static void Dot(Paint p, Vector2 o, float k, Color c, float cx, float cy, float r)
        {
            p.Circle(o.x + cx * k, o.y + cy * k, r * k, c);
        }

        private static void Ring(Paint p, Vector2 o, float k, Color c, float cx, float cy, float r, float width)
        {
            _points.Clear();
            const int n = 24;
            for (int i = 0; i < n; ++i)
            {
                float a = i * Mathf.PI * 2f / n;
                _points.Add(new Vector2(o.x + (cx + Mathf.Cos(a) * r) * k, o.y + (cy + Mathf.Sin(a) * r) * k));
            }
            p.Stroke(_points, width * k, c, true);
        }

        private static void Box(Paint p, Vector2 o, float k, Color c, float x, float y, float w, float h, float width)
        {
            _points.Clear();
            _points.Add(new Vector2(o.x + x * k, o.y + y * k));
            _points.Add(new Vector2(o.x + (x + w) * k, o.y + y * k));
            _points.Add(new Vector2(o.x + (x + w) * k, o.y + (y + h) * k));
            _points.Add(new Vector2(o.x + x * k, o.y + (y + h) * k));
            p.Stroke(_points, width * k, c, true);
        }
        #endregion
    }
}
