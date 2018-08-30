using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnity
{
    public static class UIGraphicExtension
    {
        public static Vector3 GetLocalDragAmount(this Graphic graphic, PointerEventData eventData)
        {
            if(graphic.canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                return graphic.rectTransform.InverseTransformDirection(eventData.delta);
            }
            var prev = Vector2.zero;
            var now = Vector2.zero;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, eventData.position - eventData.delta, eventData.pressEventCamera, out prev)) {
                if(RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, eventData.position, eventData.pressEventCamera, out now)) {
                    return now - prev;
                }
            }
            return Vector3.zero;
        }

        /// <summary>
        /// UnityEngine.UI.Graphic派生オブジェクト(Text,Image,Panel...)をマウスのドラッグイベントにしたがって移動する
        /// </summary>
        /// <param name="graphic">Graphic.</param>
        /// <param name="eventData">Event data.</param>
        public static void ApplyDrag(this Graphic graphic, PointerEventData eventData)
        {
            graphic.rectTransform.localPosition += graphic.GetLocalDragAmount(eventData);
        }

        public static Vector3 GetLocalHoverPoint(this Graphic graphic, PointerEventData eventData)
        {
            if(graphic.canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                return graphic.rectTransform.InverseTransformPoint(eventData.position);
            }
            var now = Vector2.zero;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(graphic.rectTransform, eventData.position, eventData.enterEventCamera, out now)) {
                return now;
            }
            return Vector3.zero;
        }

    }
}