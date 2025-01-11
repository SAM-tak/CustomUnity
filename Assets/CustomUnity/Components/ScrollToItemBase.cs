using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    public abstract class ScrollToItemBase : UIBehaviour
    {
        public AnimatorUpdateMode updateMode;
        public float halfLife = 0.3f;
        public float activateDelayTimeFromEnabled = 0.15f;
        public Mergin selectedBoxMergin;

        public ScrollRect ScrollRect { get; private set; }

        [Serializable]
        public struct Mergin
        {
            public float left;
            public float top;
            public float right;
            public float bottom;
        }

        protected override void Awake()
        {
            base.Awake();
            ScrollRect = GetComponent<ScrollRect>();
        }

        protected float DeltaTime => updateMode == AnimatorUpdateMode.UnscaledTime
            ? (Time.inFixedTimeStep ? Time.fixedUnscaledDeltaTime : Time.unscaledDeltaTime)
            : (Time.inFixedTimeStep ? Time.fixedDeltaTime : Time.deltaTime);

        protected GameObject targetItem;

        protected Vector3 prevScrollPosition;

        protected virtual void ScrollToTarget()
        {
            if(targetItem && ScrollRect.velocity.magnitude < 0.001f) {
                var rectTransform = targetItem.GetComponent<RectTransform>();

                var viewportRect = new Rect(
                    ScrollRect.content.InverseTransformPoint(ScrollRect.viewport.TransformPoint(ScrollRect.viewport.rect.position)),
                    ScrollRect.content.InverseTransformVector(ScrollRect.viewport.TransformVector(ScrollRect.viewport.rect.size))
                );
                var selectedRect = new Rect(
                    ScrollRect.content.InverseTransformPoint(rectTransform.TransformPoint(rectTransform.rect.position)),
                    ScrollRect.content.InverseTransformVector(rectTransform.TransformVector(rectTransform.rect.size))
                );

                var lb = ScrollRect.content.InverseTransformVector(rectTransform.TransformVector(selectedBoxMergin.left, selectedBoxMergin.bottom, 0));
                selectedRect.xMin -= lb.x;
                selectedRect.yMin -= lb.y;
                var rt = ScrollRect.content.InverseTransformVector(rectTransform.TransformVector(selectedBoxMergin.right, selectedBoxMergin.top, 0));
                selectedRect.xMax += rt.x;
                selectedRect.yMax += rt.y;

                var diff = Vector3.zero;
                if(viewportRect.x > selectedRect.x) diff.x += viewportRect.x - selectedRect.x;
                if(viewportRect.xMax < selectedRect.xMax) diff.x -= selectedRect.xMax - viewportRect.xMax;
                if(viewportRect.y > selectedRect.y) diff.y += viewportRect.y - selectedRect.y;
                if(viewportRect.yMax < selectedRect.yMax) diff.y -= selectedRect.yMax - viewportRect.yMax;

                if(diff.magnitude > 0.001f) {
                    prevScrollPosition = ScrollRect.content.localPosition = Math.RubberStep(
                        ScrollRect.content.localPosition,
                        ScrollRect.content.localPosition + diff,
                        halfLife,
                        DeltaTime
                    );
                }
            }
        }

        protected bool MayMoveByOther => Vector3.Distance(prevScrollPosition, ScrollRect.content.localPosition) > 0.001f;

        float _delayTime;

        protected override void OnEnable()
        {
            base.OnEnable();
            _delayTime = activateDelayTimeFromEnabled;
        }

        void FixedUpdate()
        {
            if(_delayTime <= 0 && updateMode == AnimatorUpdateMode.Fixed) ScrollToTarget();
        }

        void Update()
        {
            if(_delayTime <= 0 && updateMode != AnimatorUpdateMode.Fixed) ScrollToTarget();
            if(_delayTime > 0f) _delayTime -= Time.unscaledDeltaTime;
        }
    }
}
