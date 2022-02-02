using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    public enum Orientaion
    {
        Vertical,
        Horizontal
    }
    
    public abstract class TableContentBase : MonoBehaviour
    {
        [ReadOnlyWhenPlaying]
        public Orientaion orientaion;

        public Action OnPreUpdate { get; set; }

        public ScrollRect ScrollRect { get; protected set; }

        public int MaxCells { get => cellPool != null ? cellPool.Length : 0; }

        public int MaxCellsRequired { get; protected set; }

        protected RectTransform contentRectTransform;

        protected struct Cell : IEquatable<Cell>
        {
            public GameObject cell;
            public int index;

            public override bool Equals(object obj) => obj is Cell cell && Equals(cell);

            public bool Equals(Cell other) => EqualityComparer<GameObject>.Default.Equals(cell, other.cell) && index == other.index;

            public override int GetHashCode() => HashCode.Combine(cell, index);
        }

        protected Cell[] cellPool;

        protected GameObject GetCell(int index, out bool @new)
        {
            @new = false;
            int candidate = -1;
            for(int j = 0; j < cellPool.Length; ++j) {
                if(cellPool[j].cell.activeSelf) {
                    if(cellPool[j].index == index) return cellPool[j].cell;
                }
                else if(candidate < 0) candidate = j;
            }
            if(candidate >= 0) {
                @new = true;
                cellPool[candidate].index = index;
                return cellPool[candidate].cell;
            }
            return null;
        }
        
        protected bool IsCulled(GameObject cell)
        {
            var cellRectTransform = cell.GetComponent<RectTransform>();
            var rect = cellRectTransform.rect;
            rect.position = cellRectTransform.TransformPoint(rect.position);
            rect.size = cellRectTransform.TransformVector(rect.size);
            rect.position = ScrollRect.viewport.InverseTransformPoint(rect.position);
            rect.size = cellRectTransform.InverseTransformVector(rect.size);
            return !ScrollRect.viewport.rect.Overlaps(rect);
        }

        public bool NeedsUpdateContent { get; private set; }  = true;

        /// <summary>
        /// To use for forcing to reset cells up.
        /// </summary>
        public void Refresh()
        {
            foreach(var i in cellPool) i.cell.SetActive(false);
            NeedsUpdateContent = true;
        }

        LayoutGroup layoutGroup;

        protected virtual void Start()
        {
            layoutGroup = GetComponentInParent<LayoutGroup>();
            if(layoutGroup != null) layoutGroup.enabled = false;
            ScrollRect = GetComponentInParent<ScrollRect>();
            Debug.Assert(ScrollRect);
            contentRectTransform = GetComponent<RectTransform>();
            cellPool = new Cell[transform.childCount];
            for(int i = 0; i < transform.childCount; i++) {
                var go = transform.GetChild(i).gameObject;
                go.SetActive(false);
                cellPool[i].cell = go;
            }
            ScrollRect.onValueChanged.AddListener(_ => {
                if(isUpdatingContent) NeedsUpdateContent = true;
                else {
                    isUpdatingContent = true;
                    UpdateContent();
                    isUpdatingContent = false;
                }
            });
            FrameCount = 0;
        }

        // workaround for 2019.1 higher
        protected int FrameCount { get; private set; } = 0;

        bool isUpdatingContent;

        protected virtual void Update()
        {
            OnPreUpdate?.Invoke();
            if(NeedsUpdateContent || FrameCount < 2) {
                isUpdatingContent = true;
                UpdateContent();
                isUpdatingContent = false;
                NeedsUpdateContent = false;
            }
            if(FrameCount < int.MaxValue) FrameCount++;
        }

        protected abstract void UpdateContent();
    }
}
