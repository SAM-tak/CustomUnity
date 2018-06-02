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
    public class LargeJaggedTableContent : MonoBehaviour
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            Vector2 CellSize(int index);
            void SetUpCell(int index, GameObject cell);
        }

        public Orientaion orientaion;

        public bool repeat;

        public bool multiColumn;

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
        Rect[] cellRects;

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
            cellRects = new Rect[transform.childCount];
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
            var viewSize = scrollRectTransform.sizeDelta;
            float rowWidth = 0f;
            float viewLower = 0f;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            switch(orientaion) {
            case Orientaion.Vertical:
                viewLower = viewSize.y;
                if(multiColumn) rowWidth = viewSize.x;
                break;
            case Orientaion.Horizontal:
                viewLower = viewSize.x;
                if(multiColumn) rowWidth = viewSize.y;
                break;
            }

            OnPreUpdate?.Invoke();

            var totalCount = (DataSource != null ? DataSource.TotalCount : 0);

            int curLineItemCount = 0;
            float curRowWidth = 0f;
            float curRowHeight = 0f;
            for(int i = 0; i < totalCount; ++i) {
                var cellSize = DataSource.CellSize(i);
                float rowHeight = 0;
                float columnWidth = 0;
                float cellUpper = 0;
                Rect rect = Rect.zero;
                switch(orientaion) {
                case Orientaion.Vertical:
                    rowHeight = cellSize.y;
                    columnWidth = cellSize.x;
                    break;
                case Orientaion.Horizontal:
                    rowHeight = cellSize.x;
                    columnWidth = cellSize.y;
                    break;
                }

                if(multiColumn) {
                    if(curRowWidth + columnWidth > rowWidth || i + 1 == totalCount) {
                        if(i > 0) {
                            if(endIndex >= 0) {
                                var cl = endIndex - startIndex + 1;
                                for(int j = cl - curLineItemCount; j < cl && j < cellRects.Length; ++j) {
                                    if(j < 0) continue;
                                    var c = cellRects[j];
                                    switch(orientaion) {
                                    case Orientaion.Vertical:
                                        c.height = curRowHeight;
                                        break;
                                    case Orientaion.Horizontal:
                                        c.width = curRowHeight;
                                        break;
                                    }
                                    cellRects[j] = c;
                                }
                            }
                            contentSize += curRowHeight;
                            curRowHeight = rowHeight;
                            curRowWidth = 0;
                            curLineItemCount = 0;
                        }
                    }
                    else {
                        if(curRowHeight < rowHeight) curRowHeight = rowHeight;
                        curLineItemCount++;
                    }
                }
                else curRowHeight = rowHeight;

                switch(orientaion) {
                case Orientaion.Vertical:
                    cellUpper = contentSize - contentRectLocalPosition.y;
                    rect = new Rect(curRowWidth, contentSize, columnWidth, rowHeight);
                    break;
                case Orientaion.Horizontal:
                    cellUpper = contentSize + contentRectLocalPosition.x;
                    rect = new Rect(contentSize, curRowWidth, rowHeight, columnWidth);
                    break;
                }

                if(multiColumn) {
                    curRowWidth += columnWidth;
                    if(i + 1 == totalCount) contentSize += curRowHeight;
                }
                else contentSize += rowHeight;

                if(startIndex < 0) {
                    if(cellUpper >= -curRowHeight && cellUpper <= viewLower) {
                        startIndex = endIndex = i;
                        cellRects[0] = rect;
                    }
                }
                else {
                    if(cellUpper >= -curRowHeight && cellUpper <= viewLower) {
                        endIndex = i;
                        if(i - startIndex < cellRects.Length) cellRects[i - startIndex] = rect;
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
                        var cellRect = cellRects[i - startIndex];
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            size.y = cellRect.height;
                            localPosition.y = -cellRect.y - size.y * rectTrans.pivot.y;
                            if(multiColumn) {
                                localPosition.x = cellRect.x + size.x * rectTrans.pivot.x;
                                size.x = cellRect.width;
                            }
                            break;
                        case Orientaion.Horizontal:
                            size.x = cellRect.width;
                            localPosition.x = cellRect.x + size.x * rectTrans.pivot.x;
                            if(multiColumn) {
                                size.y = cellRect.height;
                                localPosition.y = -cellRect.y - size.y * rectTrans.pivot.y;
                            }
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
