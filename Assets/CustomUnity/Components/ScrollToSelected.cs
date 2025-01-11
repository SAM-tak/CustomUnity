using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnity
{
    /// <summary>
    /// Scroll to ui item focused by event system.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollToSelected : ScrollToItemBase, IScrollHandler
    {
        public EventSystem eventSystem;

        // フォーカスが外れてもしばらくスクロールの対象にするため
        GameObject _lastSelected;
        // フォーカスされているオブジェクトがいつまでもスクロールの対象にならないようにする時間の計測用
        float _lastSelectedInterval;

        protected override void ScrollToTarget()
        {
            targetItem = _lastSelected;
            if(eventSystem.currentSelectedGameObject
                && eventSystem.currentSelectedGameObject.transform.IsChildOf(ScrollRect.content.transform)
                && eventSystem.currentSelectedGameObject.GetComponentInParent<ScrollRect>() == ScrollRect) {
                _lastSelected = targetItem = eventSystem.currentSelectedGameObject;
                _lastSelectedInterval = 0;
            }
            else if(_lastSelectedInterval < halfLife * 2) _lastSelectedInterval += DeltaTime;
            else _lastSelected = null;

            base.ScrollToTarget();
        }

        public void OnScroll(PointerEventData eventData)
        {
            eventSystem.SetSelectedGameObject(null);
            _lastSelected = null;
        }
    }
}
