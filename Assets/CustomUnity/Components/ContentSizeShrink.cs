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
        public enum Orientaion
        {
            Vertical,
            Horizontal
        }

        public Orientaion orientaion;
        public float insetSizeFromParent = 6f;

        RectTransform _rectTransform;
        RectTransform _parentRectTransform;

        RectTransform RectTransform {
            get {
                if(_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        RectTransform ParentRectTransform {
            get {
                if(_parentRectTransform == null) _parentRectTransform = transform.parent.GetComponent<RectTransform>();
                return _parentRectTransform;
            }
        }
        
        DrivenRectTransformTracker _tracker;

        #region Unity Lifetime calls

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            _tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
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
                _tracker.Add(this, RectTransform, DrivenTransformProperties.SizeDeltaX);
                RectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    Mathf.Min(ParentRectTransform.rect.width - insetSizeFromParent, LayoutUtility.GetPreferredWidth(RectTransform))
                );
            }
        }

        public void SetLayoutVertical()
        {
            if(orientaion == Orientaion.Vertical) {
                _tracker.Add(this, RectTransform, DrivenTransformProperties.SizeDeltaY);
                RectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    Mathf.Min(ParentRectTransform.rect.height - insetSizeFromParent, LayoutUtility.GetPreferredHeight(RectTransform))
                );
            }
        }

        protected void SetDirty()
        {
            if(!IsActive()) return;
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif
    }
}
