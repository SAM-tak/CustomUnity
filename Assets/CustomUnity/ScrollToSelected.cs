using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollToSelected : MonoBehaviour
    {
        public EventSystem eventSystem;
        public AnimatorUpdateMode updateMode;
        public float halfLife = 0.3f;

        ScrollRect scrollRect;
        
        void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        void Scroll()
        {
            if(scrollRect.velocity.magnitude < 0.001f && eventSystem.currentSelectedGameObject && eventSystem.currentSelectedGameObject.transform.IsChildOf(scrollRect.content.transform)) {
                var selected = eventSystem.currentSelectedGameObject.GetComponent<RectTransform>();

                var viewportRect = new Rect(scrollRect.viewport.TransformPoint(scrollRect.viewport.rect.position), scrollRect.viewport.TransformVector(scrollRect.viewport.rect.size));
                var selectedRect = new Rect(selected.TransformPoint(selected.rect.position), selected.TransformVector(selected.rect.size));

                var diff = Vector3.zero;
                if(viewportRect.x > selectedRect.x) diff.x += viewportRect.x - selectedRect.x;
                if(viewportRect.xMax < selectedRect.xMax) diff.x += viewportRect.xMax - selectedRect.xMax;
                if(viewportRect.y > selectedRect.y) diff.y += viewportRect.y - selectedRect.y;
                if(viewportRect.yMax < selectedRect.yMax) diff.y += viewportRect.yMax - selectedRect.yMax;

                if(diff.magnitude > 0.001f) {
                    scrollRect.content.localPosition = Math.RubberStep(
                        scrollRect.content.localPosition,
                        scrollRect.content.localPosition + scrollRect.content.InverseTransformVector(diff),
                        halfLife,
                        (updateMode == AnimatorUpdateMode.UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime)
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
    }
}