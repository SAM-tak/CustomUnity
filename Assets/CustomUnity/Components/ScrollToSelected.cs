using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollToSelected : ScrollToItemBase, IScrollHandler
    {
        public EventSystem eventSystem;

        // フォーカスが外れてもしばらくスクロールの対象にするため
        GameObject lastSelected;
        // フォーカスされているオブジェクトがいつまでもスクロールの対象にならないようにする時間の計測用
        float lastSelectedInterval;

        protected override void ScrollToTarget()
        {
            targetItem = lastSelected;
            if(eventSystem.currentSelectedGameObject
                && eventSystem.currentSelectedGameObject.transform.IsChildOf(ScrollRect.content.transform)
                && eventSystem.currentSelectedGameObject.GetComponentInParent<ScrollRect>() == ScrollRect) {
                lastSelected = targetItem = eventSystem.currentSelectedGameObject;
                lastSelectedInterval = 0;
            }
            else if(lastSelectedInterval < halfLife * 2) lastSelectedInterval += DeltaTime;
            else lastSelected = null;

            base.ScrollToTarget();
        }

        public void OnScroll(PointerEventData eventData)
        {
            eventSystem.SetSelectedGameObject(null);
            lastSelected = null;
        }
    }
}
