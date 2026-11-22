using UnityEngine;
using UnityEngine.UI;

namespace Graphics.UI
{
    /// <summary>Stable screen-pixel bounds for the UI's rectangular backgrounds and borders.</summary>
    internal sealed class StableImage : Image
    {
        public override void OnPopulateMesh(VertexHelper mesh)
        {
            base.OnPopulateMesh(mesh);
            Canvas owner = canvas;
            if (owner == null || !owner.pixelPerfect || owner.renderMode != RenderMode.ScreenSpaceOverlay ||
                mesh.currentVertCount == 0 || (type != Type.Sliced && sprite != null))
                return;

            Rect adjusted = GetPixelAdjustedRect();
            if (adjusted.width <= 0f || adjusted.height <= 0f)
                return;
            Rect raw = rectTransform.rect;
            Vector3 min = rectTransform.TransformPoint(new Vector3(raw.xMin, raw.yMin));
            Vector3 max = rectTransform.TransformPoint(new Vector3(raw.xMax, raw.yMax));
            min.x = StableText.SnapPixel(min.x);
            min.y = StableText.SnapPixel(min.y);
            max.x = StableText.SnapPixel(max.x);
            max.y = StableText.SnapPixel(max.y);
            min = rectTransform.InverseTransformPoint(min);
            max = rectTransform.InverseTransformPoint(max);
            // Undo Image's pixel-adjusted bounds and map its generated geometry onto stable bounds.
            // No layout/RectTransform writes: the correction cannot feed back into layout.
            UIVertex vertex = default;
            for (int i = 0; i < mesh.currentVertCount; i++)
            {
                mesh.PopulateUIVertex(ref vertex, i);
                Vector3 pos = vertex.position;
                pos.x = min.x + (pos.x - adjusted.xMin) / adjusted.width * (max.x - min.x);
                pos.y = min.y + (pos.y - adjusted.yMin) / adjusted.height * (max.y - min.y);
                vertex.position = pos;
                mesh.SetUIVertex(vertex, i);
            }
        }
    }
}
