using UnityEngine;
using UnityEngine.UI;

namespace Graphics.UI
{
    /// <summary>Keep text measurement independent of screen-pixel rounding at fractional UI scales.</summary>
    internal sealed class StableText : Text
    {
        public override float preferredHeight
        {
            get
            {
                // Match the extents used by Text.OnPopulateMesh. Pixel-adjusted width depends on
                // position and can cross a wrap boundary during a layout/scroll correction.
                var settings = GetGenerationSettings(new Vector2(rectTransform.rect.width, 0f));
                return cachedTextGeneratorForLayout.GetPreferredHeight(text, settings) / pixelsPerUnit;
            }
        }

        public override void OnPopulateMesh(VertexHelper mesh)
        {
            base.OnPopulateMesh(mesh);
            Canvas owner = canvas;
            if (font == null || owner == null || !owner.pixelPerfect ||
                owner.renderMode != RenderMode.ScreenSpaceOverlay || mesh.currentVertCount == 0 ||
                cachedTextGenerator.verts.Count == 0)
                return;

            // Unity's PixelAdjustPoint can choose opposite sides of a half-pixel boundary when
            // layout supplies numerically equivalent float positions. Derive one translation
            // from the unrounded glyph origin, with a tiny tie tolerance, for the entire mesh.
            Vector3 origin = cachedTextGenerator.verts[0].position / pixelsPerUnit;
            Vector3 screen = rectTransform.TransformPoint(origin);
            screen.x = SnapPixel(screen.x);
            screen.y = SnapPixel(screen.y);
            Vector3 target = rectTransform.InverseTransformPoint(screen);
            UIVertex vertex = default;
            mesh.PopulateUIVertex(ref vertex, 0);
            Vector3 offset = target - vertex.position;
            for (int i = 0; i < mesh.currentVertCount; i++)
            {
                mesh.PopulateUIVertex(ref vertex, i);
                vertex.position += offset;
                mesh.SetUIVertex(vertex, i);
            }
        }

        internal static float SnapPixel(float value) => Mathf.Floor(value + 0.5f + 0.001f);
    }
}
