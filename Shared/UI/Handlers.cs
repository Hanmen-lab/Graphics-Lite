using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Graphics.UI
{
    /// <summary>Drag callbacks for a uGUI element (what Timeline gets from UILib's DragHandler).</summary>
    internal class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> onBeginDrag;
        public Action<PointerEventData> onDrag;
        public Action<PointerEventData> onEndDrag;

        public void OnBeginDrag(PointerEventData e) { onBeginDrag?.Invoke(e); }
        public void OnDrag(PointerEventData e) { onDrag?.Invoke(e); }
        public void OnEndDrag(PointerEventData e) { onEndDrag?.Invoke(e); }
    }

    internal class PointerDownHandler : MonoBehaviour, IPointerDownHandler
    {
        public Action<PointerEventData> onPointerDown;

        public void OnPointerDown(PointerEventData e) { onPointerDown?.Invoke(e); }
    }

    internal class PointerUpHandler : MonoBehaviour, IPointerUpHandler
    {
        public Action<PointerEventData> onPointerUp;

        public void OnPointerUp(PointerEventData e) { onPointerUp?.Invoke(e); }
    }

    internal class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<bool> onHover;

        public void OnPointerEnter(PointerEventData e) { onHover?.Invoke(true); }
        public void OnPointerExit(PointerEventData e) { onHover?.Invoke(false); }
    }
}
