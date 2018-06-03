using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    public enum Orientaion
    {
        Vertical,
        Horizontal
    }

    [RequireComponent(typeof(RectTransform))]
    public abstract class TableContentBase : MonoBehaviour
    {
        [ReadOnlyWhenPlaying]
        public Orientaion orientaion;

        public Action OnPreUpdate { get; set; }

        public ScrollRect ScrollRect { get; protected set; }

        public int MaxCells {
            get {
                return cellPool != null ? cellPool.Length : 0;
            }
        }

        public int MaxCellsRequired { get; protected set; }

        protected RectTransform contentRectTransform;

        protected struct Cell
        {
            public GameObject cell;
            public int index;
        }

        protected Cell[] cellPool;

        protected bool needsUpdateContent = true;

        /// <summary>
        /// Inactivate All Active Cells
        /// 
        /// To use for forcing to reset cells up.
        /// </summary>
        public void Refresh()
        {
            foreach(var i in cellPool) i.cell.SetActive(false);
            needsUpdateContent = true;
        }

        protected virtual void Start()
        {
            ScrollRect = GetComponentInParent<ScrollRect>();
            Debug.Assert(ScrollRect);
            contentRectTransform = GetComponent<RectTransform>();
            cellPool = new Cell[transform.childCount];
            for(int i = 0; i < transform.childCount; i++) {
                var go = transform.GetChild(i).gameObject;
                go.SetActive(false);
                cellPool[i].cell = go;
            }
            ScrollRect.onValueChanged.AddListener((v) => UpdateContent());
        }

        protected virtual void Update()
        {
            OnPreUpdate?.Invoke();
            if(needsUpdateContent) {
                UpdateContent();
                needsUpdateContent = false;
            }
        }

        protected abstract void UpdateContent();
    }
}
