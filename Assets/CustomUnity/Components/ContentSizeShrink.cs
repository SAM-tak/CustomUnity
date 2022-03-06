using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    /// <summary>
    /// limit rect size into parent size.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class ContentSizeShrink : UIBehaviour, ILayoutSelfController
    {
        public Orientaion orientaion;
        public float insetSizeFromParent = 6f;

        RectTransform _rectTransform;
        RectTransform _parentRectTransform;

        RectTransform rectTransform {
            get {
                if(_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        RectTransform parentRectTransform {
            get {
                if(_parentRectTransform == null) _parentRectTransform = transform.parent.GetComponent<RectTransform>();
                return _parentRectTransform;
            }
        }
        
        DrivenRectTransformTracker tracker;

        #region Unity Lifetime calls

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        #endregion

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        public void SetLayoutHorizontal()
        {
            if(orientaion == Orientaion.Horizontal) {
                tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
                rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    Mathf.Min(parentRectTransform.rect.width - insetSizeFromParent, LayoutUtility.GetPreferredWidth(rectTransform))
                );
            }
        }

        public void SetLayoutVertical()
        {
            if(orientaion == Orientaion.Vertical) {
                tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
                rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    Mathf.Min(parentRectTransform.rect.height - insetSizeFromParent, LayoutUtility.GetPreferredHeight(rectTransform))
                );
            }
        }

        protected void SetDirty()
        {
            if(!IsActive()) return;
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif
    }
}
