using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Graphics.UI
{
    /// <summary>
    /// HSV colour wheel used by the window's colour popup. Angle selects hue, distance from the
    /// centre selects saturation, and the strip below selects value. The separate grading Level
    /// param remains the brightness control for Lift/Gamma/Gain and ASC-CDL.
    /// </summary>
    internal class ColorPicker : MonoBehaviour
    {
        private const float InlineDiscRatio = 0.88f;
        public Action onChanged;
        public Action onPainted;

        private ColorParam _param;
        private float _h, _s, _v, _a = 1f;
        private bool _inline;
        private PaintArea _wheel, _value, _alpha;
        private readonly List<Vector2> _innerRing = new List<Vector2>(96);

        public static ColorPicker Build(RectTransform parent, ColorParam param)
        {
            RectTransform root = Kit.Node("Picker", parent);
            Kit.Col(root.gameObject, 6f);
            ColorPicker picker = root.gameObject.AddComponent<ColorPicker>();
            picker._param = param;
            picker._wheel = picker.Area(root, 156f, picker.DrawWheel, picker.PickWheel);
            picker._value = picker.Area(root, 12f, picker.DrawValue, picker.PickValue);
            // A third strip, only for a param that carries an alpha.
            if (param.Alpha)
                picker._alpha = picker.Area(root, 12f, picker.DrawAlpha, picker.PickAlpha);
            return picker;
        }

        /// <summary>The same wheel without the HSV value strip, for an inline grading card.</summary>
        public static ColorPicker BuildInline(RectTransform parent, ColorParam param, float height)
        {
            RectTransform root = Kit.Node("InlineWheel", parent);
            Kit.Col(root.gameObject, 0f);
            ColorPicker picker = root.gameObject.AddComponent<ColorPicker>();
            picker._param = param;
            picker._inline = true;
            picker._wheel = picker.Area(root, height, picker.DrawWheel, picker.PickWheel);
            return picker;
        }

        private PaintArea Area(RectTransform parent, float height, Action<PaintArea> draw, Action<Vector2> pick)
        {
            RectTransform rect = Kit.Node("Area", parent);
            Kit.Size(rect.gameObject, -1f, height);
            PaintArea area = rect.gameObject.AddComponent<PaintArea>();
            area.raycastTarget = true;
            area.draw = draw;
            Action<PointerEventData> at = e =>
            {
                Vector2 local;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out local);
                Rect r = rect.rect;
                pick(new Vector2((local.x - r.xMin) / Mathf.Max(1f, r.width), (r.yMax - local.y) / Mathf.Max(1f, r.height)));
            };
            rect.gameObject.AddComponent<PointerDownHandler>().onPointerDown = at;
            rect.gameObject.AddComponent<DragHandler>().onDrag = at;
            return area;
        }

        public void Sync()
        {
            float h, s, v;
            Color.RGBToHSV(_param.Value, out h, out s, out v);
            if (s > 0.001f)
                _h = h;
            _s = s;
            _v = v;
            _a = _param.Value.a;
            Redraw();
        }

        private void PickWheel(Vector2 t)
        {
            // The area is taller than the circle to leave room for its edge and marker.
            float x = (t.x - 0.5f) * _wheel.width;
            float y = (t.y - 0.5f) * _wheel.height;
            float radius = Mathf.Min(_wheel.width, _wheel.height) * 0.46f;
            float distance = new Vector2(x, y).magnitude;
            if (_inline && distance > radius * InlineDiscRatio)
            {
                _h = HueAtAngle(Mathf.Atan2(y, x));
                _s = Mathf.Max(_s, 0.55f);
            }
            else
            {
                _s = Mathf.Clamp01(distance / (radius * (_inline ? InlineDiscRatio : 1f)));
                if (_s > 0.001f)
                    _h = _inline ? HueAtAngle(Mathf.Atan2(y, x)) :
                        Mathf.Repeat(Mathf.Atan2(y, x) / (2f * Mathf.PI), 1f);
            }
            Commit();
        }

        private void PickValue(Vector2 t)
        {
            _v = Mathf.Clamp01(t.x);
            Commit();
        }

        private void PickAlpha(Vector2 t)
        {
            _a = Mathf.Clamp01(t.x);
            Commit();
        }

        private void Commit()
        {
            // HSVToRGB returns alpha 1 - the strip's alpha rides on top of it.
            Color c = Color.HSVToRGB(_h, _s, _v);
            c.a = _a;
            _param.Value = c;
            Redraw();
            onChanged?.Invoke();
        }

        private void Redraw()
        {
            _wheel.Redraw();
            if (_value != null)
                _value.Redraw();
            if (_alpha != null)
                _alpha.Redraw();
            onPainted?.Invoke();
        }

        private void DrawWheel(PaintArea p)
        {
            if (_inline)
            {
                DrawInlineWheel(p);
                return;
            }
            const int sectors = 72, rings = 8;
            float cx = p.width * 0.5f, cy = p.height * 0.5f;
            float radius = Mathf.Min(p.width, p.height) * 0.46f;
            for (int i = 0; i < sectors; ++i)
            {
                float a0 = i * 2f * Mathf.PI / sectors, a1 = (i + 1) * 2f * Mathf.PI / sectors;
                for (int j = 0; j < rings; ++j)
                {
                    float r0 = radius * j / rings, r1 = radius * (j + 1) / rings;
                    float s0 = (float)j / rings, s1 = (float)(j + 1) / rings;
                    p.ColorQuad(
                        new Vector2(cx + Mathf.Cos(a0) * r0, cy + Mathf.Sin(a0) * r0),
                        new Vector2(cx + Mathf.Cos(a1) * r0, cy + Mathf.Sin(a1) * r0),
                        new Vector2(cx + Mathf.Cos(a1) * r1, cy + Mathf.Sin(a1) * r1),
                        new Vector2(cx + Mathf.Cos(a0) * r1, cy + Mathf.Sin(a0) * r1),
                        Color.HSVToRGB((float)i / sectors, s0, _v),
                        Color.HSVToRGB((float)(i + 1) / sectors, s0, _v),
                        Color.HSVToRGB((float)(i + 1) / sectors, s1, _v),
                        Color.HSVToRGB((float)i / sectors, s1, _v));
                }
            }
            float angle = _h * 2f * Mathf.PI;
            float x = cx + Mathf.Cos(angle) * radius * _s;
            float y = cy + Mathf.Sin(angle) * radius * _s;
            p.Circle(x, y, 6f, new Color(0f, 0f, 0f, 0.65f));
            p.Circle(x, y, 4.5f, Color.white);
            p.Circle(x, y, 2.8f, _param.Value);
        }

        private static float HueAtAngle(float angle) => Mathf.Repeat((angle - Mathf.PI) / (2f * Mathf.PI), 1f);

        private void DrawInlineWheel(PaintArea p)
        {
            const int sectors = 96, rings = 8;
            float cx = p.width * 0.5f, cy = p.height * 0.5f;
            float radius = Mathf.Min(p.width, p.height) * 0.46f, disc = radius * InlineDiscRatio;
            float halfPixel = p.screenPixel * 0.5f;
            float opaqueRadius = radius - halfPixel;
            Color neutral = new Color(0.72f * _v, 0.72f * _v, 0.72f * _v);
            _innerRing.Clear();
            for (int i = 0; i < sectors; ++i)
            {
                float a0 = i * 2f * Mathf.PI / sectors, a1 = (i + 1) * 2f * Mathf.PI / sectors;
                float h0 = HueAtAngle(a0), h1 = HueAtAngle(a1);
                Color soft0 = Color.HSVToRGB(h0, 0.38f, 0.85f * _v);
                Color soft1 = Color.HSVToRGB(h1, 0.38f, 0.85f * _v);
                Color rim0 = Color.HSVToRGB(h0, 0.88f, 0.94f);
                Color rim1 = Color.HSVToRGB(h1, 0.88f, 0.94f);
                Color clear0 = rim0, clear1 = rim1;
                clear0.a = clear1.a = 0f;
                _innerRing.Add(new Vector2(cx + Mathf.Cos(a0) * disc, cy + Mathf.Sin(a0) * disc));
                for (int j = 0; j < rings; ++j)
                {
                    float t0 = (float)j / rings, t1 = (float)(j + 1) / rings;
                    float r0 = disc * t0, r1 = disc * t1;
                    p.ColorQuad(
                        new Vector2(cx + Mathf.Cos(a0) * r0, cy + Mathf.Sin(a0) * r0),
                        new Vector2(cx + Mathf.Cos(a1) * r0, cy + Mathf.Sin(a1) * r0),
                        new Vector2(cx + Mathf.Cos(a1) * r1, cy + Mathf.Sin(a1) * r1),
                        new Vector2(cx + Mathf.Cos(a0) * r1, cy + Mathf.Sin(a0) * r1),
                        Color.Lerp(neutral, soft0, t0), Color.Lerp(neutral, soft1, t0),
                        Color.Lerp(neutral, soft1, t1), Color.Lerp(neutral, soft0, t1));
                }
                p.ColorQuad(
                    new Vector2(cx + Mathf.Cos(a0) * disc, cy + Mathf.Sin(a0) * disc),
                    new Vector2(cx + Mathf.Cos(a1) * disc, cy + Mathf.Sin(a1) * disc),
                    new Vector2(cx + Mathf.Cos(a1) * opaqueRadius, cy + Mathf.Sin(a1) * opaqueRadius),
                    new Vector2(cx + Mathf.Cos(a0) * opaqueRadius, cy + Mathf.Sin(a0) * opaqueRadius),
                    rim0, rim1, rim1, rim0);
                p.ColorQuad(
                    new Vector2(cx + Mathf.Cos(a0) * opaqueRadius, cy + Mathf.Sin(a0) * opaqueRadius),
                    new Vector2(cx + Mathf.Cos(a1) * opaqueRadius, cy + Mathf.Sin(a1) * opaqueRadius),
                    new Vector2(cx + Mathf.Cos(a1) * (radius + halfPixel), cy + Mathf.Sin(a1) * (radius + halfPixel)),
                    new Vector2(cx + Mathf.Cos(a0) * (radius + halfPixel), cy + Mathf.Sin(a0) * (radius + halfPixel)),
                    rim0, rim1, clear1, clear0);
            }
            p.Stroke(_innerRing, 1.1f * p.screenPixel, new Color(1f, 1f, 1f, 0.28f), true);
            float angle = Mathf.PI + _h * 2f * Mathf.PI;
            float x = cx + Mathf.Cos(angle) * disc * _s;
            float y = cy + Mathf.Sin(angle) * disc * _s;
            p.Circle(x, y + 1.3f, 7.2f, new Color(0f, 0f, 0f, 0.22f));
            p.Circle(x, y, 5.5f, Color.white);
            // A white grading value must not erase the white outline of the marker.
            p.Circle(x, y, 3.7f, Color.Lerp(new Color(0.62f, 0.62f, 0.62f), _param.Value, 0.35f));
        }

        private void DrawValue(PaintArea p)
        {
            Color color = Color.HSVToRGB(_h, _s, 1f);
            p.Gradient(0f, 0f, p.width, p.height, Color.black, color, color, Color.black);
            float x = Mathf.Round(_v * p.width);
            p.Rect(x - 2f, -1f, 4f, p.height + 2f, new Color(0f, 0f, 0f, 0.5f));
            p.Rect(x - 1f, -1f, 2f, p.height + 2f, Color.white);
        }

        private void DrawAlpha(PaintArea p)
        {
            Color rgb = Color.HSVToRGB(_h, _s, _v);
            Color clear = rgb;
            clear.a = 0f;
            p.Gradient(0f, 0f, p.width, p.height, clear, rgb, rgb, clear);
            float x = Mathf.Round(_a * p.width);
            p.Rect(x - 2f, -1f, 4f, p.height + 2f, new Color(0f, 0f, 0f, 0.5f));
            p.Rect(x - 1f, -1f, 2f, p.height + 2f, Color.white);
        }
    }

    /// <summary>A Paint that redraws itself through a callback whenever its size changes.</summary>
    internal class PaintArea : Paint
    {
        public Action<PaintArea> draw;
        private Vector2 _drawnSize;
        private float _drawnScale;

        public override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (_drawnSize != rectTransform.rect.size || _drawnScale != (canvas != null ? canvas.scaleFactor : 1f))
                Redraw();
        }

        public void Redraw()
        {
            if (isActiveAndEnabled == false || draw == null)
                return;
            _drawnSize = rectTransform.rect.size;
            _drawnScale = canvas != null ? canvas.scaleFactor : 1f;
            Begin();
            draw(this);
            End();
        }
    }
}
