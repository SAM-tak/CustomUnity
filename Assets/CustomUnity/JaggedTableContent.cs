using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(RectTransform))]
    public class JaggedTableContent : MonoBehaviour
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            float CellSize(int index);
            void SetUpCell(int index, GameObject cell);
        }

        public Orientaion orientaion;

        public bool repeat;
        
        public IDataSource DataSource { get; set; }
        
        public Action OnPreUpdate { get; set; }

        public ScrollRect ScrollRect { get; protected set; }

        public int MaxCells {
            get {
                return cellPool != null ? cellPool.Length : 0;
            }
        }

        public int MaxCellsRequired { get; protected set; }

        RectTransform contentRectTransform;
        RectTransform scrollRectTransform;
        
        struct Cell
        {
            public GameObject cell;
            public int index;
        }

        Cell[] cellPool;

        struct CellPosition
        {
            public float position;
            public float size;
        }
        CellPosition[] cellPositions;

        /// <summary>
        /// Inactivate All Active Cells
        /// 
        /// To use for forcing to reset cells up.
        /// </summary>
        public void InactivateAllCells()
        {
            foreach(var i in cellPool) i.cell.SetActive(false);
        }
        
        const int merginScaler = 2;
        
        void Start()
        {
            ScrollRect = GetComponentInParent<ScrollRect>();
            Debug.Assert(ScrollRect);
            contentRectTransform = GetComponent<RectTransform>();
            scrollRectTransform = GetComponentInParent<ScrollRect>().GetComponent<RectTransform>();
            cellPool = new Cell[transform.childCount];
            cellPositions = new CellPosition[transform.childCount];
            for(int i = 0; i < transform.childCount; i++) {
                var go = transform.GetChild(i).gameObject;
                go.SetActive(false);
                cellPool[i].cell = go;
            }

            var contentRectLocalPosition = contentRectTransform.localPosition;
            var viewSize = scrollRectTransform.sizeDelta;
            switch(orientaion) {
            case Orientaion.Vertical:
                if(repeat) {
                    var contentMargin = viewSize.y * merginScaler;
                    if(contentRectLocalPosition.y < contentMargin) {
                        contentRectLocalPosition.y = contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                break;
            case Orientaion.Horizontal:
                if(repeat) {
                    var contentMargin = viewSize.x * merginScaler;
                    if(contentRectLocalPosition.x < contentMargin) {
                        contentRectLocalPosition.x = contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                break;
            }
        }

        void Update()
        {
            if(!ScrollRect) return;

            float contentSize = 0;
            int startIndex = -1;
            int endIndex = -1;
            float viewLower = 0f;
            switch(orientaion) {
            case Orientaion.Vertical:
                viewLower = ScrollRect.viewport.rect.height;
                break;
            case Orientaion.Horizontal:
                viewLower = ScrollRect.viewport.rect.width;
                break;
            }

            OnPreUpdate?.Invoke();

            var totalCount = (DataSource != null ? DataSource.TotalCount : 0);

            var contentRectLocalPosition = contentRectTransform.localPosition;

            for(int i = 0; i < totalCount; ++i) {
                var cellSize = DataSource.CellSize(i);
                float cellUpper = 0;
                switch(orientaion) {
                case Orientaion.Vertical:
                    cellUpper = contentSize - contentRectLocalPosition.y;
                    break;
                case Orientaion.Horizontal:
                    cellUpper = contentSize + contentRectLocalPosition.x;
                    break;
                }
                var cellPosition = new CellPosition { position = contentSize, size = cellSize };

                contentSize += cellSize;

                if(startIndex < 0) {
                    if(cellUpper >= -cellSize && cellUpper <= viewLower) {
                        startIndex = endIndex = i;
                        cellPositions[0] = cellPosition;
                    }
                }
                else {
                    if(cellUpper >= -cellSize && cellUpper <= viewLower) {
                        endIndex = i;
                        if(i - startIndex < cellPositions.Length) cellPositions[i - startIndex] = cellPosition;
                    }
                }
            }

            var sizeDelta = contentRectTransform.sizeDelta;
            switch(orientaion) {
            case Orientaion.Vertical:
                sizeDelta.y = contentSize;
                break;
            case Orientaion.Horizontal:
                sizeDelta.x = contentSize;
                break;
            }
            contentRectTransform.sizeDelta = sizeDelta;

            foreach(var i in cellPool) {
                if(i.cell.activeSelf && (i.index < startIndex || i.index > endIndex)) i.cell.SetActive(false);
            }

            if(endIndex - startIndex + 1 > 0) {
                if(endIndex - startIndex + 1 > MaxCellsRequired) MaxCellsRequired = endIndex - startIndex + 1;
                for(int i = startIndex; i <= endIndex; ++i) {
                    int wrapedIndex = Math.Wrap(i, totalCount);
                    int firstinactive = -1;
                    for(int j = 0; j < cellPool.Length; ++j) {
                        if(cellPool[j].cell.activeSelf) {
                            if(cellPool[j].index == i) {
                                firstinactive = -1;
                                break;
                            }
                        }
                        else if(firstinactive < 0) firstinactive = j;
                    }
                    if(firstinactive >= 0) {
                        var x = cellPool[firstinactive];
                        var rectTrans = x.cell.GetComponent<RectTransform>();
                        var localPosition = rectTrans.localPosition;
                        var size = rectTrans.sizeDelta;
                        var cellPosition = cellPositions[i - startIndex];
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            size.y = cellPosition.size;
                            localPosition.y = -cellPosition.position - size.y * rectTrans.pivot.y;
                            break;
                        case Orientaion.Horizontal:
                            size.x = cellPosition.size;
                            localPosition.x = cellPosition.position + size.x * rectTrans.pivot.x;
                            break;
                        }
                        rectTrans.sizeDelta = size;
                        rectTrans.localPosition = localPosition;
                        DataSource.SetUpCell(wrapedIndex, x.cell);
                        x.cell.SetActive(true);
                        x.index = i;
                        cellPool[firstinactive] = x;
                    }
                }
            }
        }
    }
}
