using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollToSelected : UIBehaviour, IScrollHandler
    {
        public EventSystem eventSystem;
        public AnimatorUpdateMode updateMode;
        public float halfLife = 0.3f;
        public Mergin selectedBoxMergin;

        [Serializable]
        public struct Mergin
        {
            public float left;
            public float top;
            public float right;
            public float bottom;
        }

        ScrollRect scrollRect;
        // フォーカスが外れてもしばらくスクロールの対象にするため
        GameObject lastSelected;
        // フォーカスされているオブジェクトがいつまでもスクロールの対象にならないようにする時間の計測用
        float lastSelectedInterval;

        new void Awake()
        {
            base.Awake();
            scrollRect = GetComponent<ScrollRect>();
        }

        float DeltaTime => updateMode == AnimatorUpdateMode.UnscaledTime
            ? (Time.inFixedTimeStep ? Time.fixedUnscaledDeltaTime : Time.unscaledDeltaTime) : (Time.inFixedTimeStep ? Time.fixedDeltaTime : Time.deltaTime);

        void Scroll()
        {
            var selectedGO = lastSelected;

            if(eventSystem.currentSelectedGameObject
                && eventSystem.currentSelectedGameObject.transform.IsChildOf(scrollRect.content.transform)
                && eventSystem.currentSelectedGameObject.GetComponentInParent<ScrollRect>() == scrollRect) {
                lastSelected = selectedGO = eventSystem.currentSelectedGameObject;
                lastSelectedInterval = 0;
            }
            else if(lastSelectedInterval < halfLife * 2) lastSelectedInterval += DeltaTime;
            else lastSelected = null;

            if(scrollRect.velocity.magnitude < 0.001f && selectedGO) {
                var selected = selectedGO.GetComponent<RectTransform>();

                var viewportRect = new Rect(
                    scrollRect.content.InverseTransformPoint(scrollRect.viewport.TransformPoint(scrollRect.viewport.rect.position)),
                    scrollRect.content.InverseTransformVector(scrollRect.viewport.TransformVector(scrollRect.viewport.rect.size))
                );
                var selectedRect = new Rect(
                    scrollRect.content.InverseTransformPoint(selected.TransformPoint(selected.rect.position)),
                    scrollRect.content.InverseTransformVector(selected.TransformVector(selected.rect.size))
                );

                var lb = scrollRect.content.InverseTransformVector(selected.TransformVector(selectedBoxMergin.left, selectedBoxMergin.bottom, 0));
                selectedRect.xMin -= lb.x;
                selectedRect.yMin -= lb.y;
                var rt = scrollRect.content.InverseTransformVector(selected.TransformVector(selectedBoxMergin.right, selectedBoxMergin.top, 0));
                selectedRect.xMax += rt.x;
                selectedRect.yMax += rt.y;

                var diff = Vector3.zero;
                if(viewportRect.x > selectedRect.x) diff.x += viewportRect.x - selectedRect.x;
                if(viewportRect.xMax < selectedRect.xMax) diff.x += viewportRect.xMax - selectedRect.xMax;
                if(viewportRect.y > selectedRect.y) diff.y += viewportRect.y - selectedRect.y;
                if(viewportRect.yMax < selectedRect.yMax) diff.y += viewportRect.yMax - selectedRect.yMax;

                if(diff.magnitude > 0.001f) {
                    scrollRect.content.localPosition = Math.RubberStep(
                        scrollRect.content.localPosition,
                        scrollRect.content.localPosition + diff,
                        halfLife,
                        DeltaTime
                    );
                }
            }
        }

        void FixedUpdate()
        {
            if(updateMode == AnimatorUpdateMode.AnimatePhysics) Scroll();
        }

        void Update()
        {
            if(updateMode != AnimatorUpdateMode.AnimatePhysics) Scroll();
        }

        public void OnScroll(PointerEventData eventData)
        {
            eventSystem.SetSelectedGameObject(null);
            lastSelected = null;
        }
    }
}
