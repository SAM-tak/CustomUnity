using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(RectTransform))]
    public class MultiColumnJaggedTableContent : MonoBehaviour
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            Vector2 CellSize(int index);
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
        
        public Vector2 GetContentSize(IDataSource dataSource)
        {
            var totalCount = dataSource.TotalCount;

            var contentSize = 0f;
            float rowWidth = 0f;
            switch(orientaion) {
            case Orientaion.Vertical:
                rowWidth = scrollRectTransform.sizeDelta.x;
                break;
            case Orientaion.Horizontal:
                rowWidth = scrollRectTransform.sizeDelta.y;
                break;
            }
            float curRowWidth = 0f;
            float curRowHeight = 0f;
            for(int i = 0; i < totalCount; ++i) {
                var cellSize = dataSource.CellSize(i);
                float rowHeight = 0;
                float columnWidth = 0;

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

                if(curRowWidth + columnWidth > rowWidth || i + 1 == totalCount) {
                    if(i > 0) {
                        contentSize += curRowHeight;
                        curRowHeight = rowHeight;
                        curRowWidth = 0;
                    }
                }
                else {
                    if(curRowHeight < rowHeight) curRowHeight = rowHeight;
                }

                curRowWidth += columnWidth;
                if(i + 1 == totalCount) contentSize += curRowHeight;
            }
            switch(orientaion) {
            default:
            case Orientaion.Vertical:
                return new Vector2(rowWidth, contentSize);
            case Orientaion.Horizontal:
                return new Vector2(contentSize, rowWidth);
            }
        }

        const int merginScaler = 2;

        struct LookAheadedCellSize
        {
            public int index;
            public Rect rect;
        }
        List<LookAheadedCellSize> LookAheadedCellSizes { get; } = new List<LookAheadedCellSize>(10);

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
                rowWidth = viewSize.x;
                break;
            case Orientaion.Horizontal:
                viewLower = viewSize.x;
                rowWidth = viewSize.y;
                break;
            }

            OnPreUpdate?.Invoke();

            var totalCount = (DataSource != null ? DataSource.TotalCount : 0);

            LookAheadedCellSizes.Clear();
            for(int i = 0; i < totalCount;) {
                int addCount = 0;
                float columnOffset = 0;
                for(int j = i; j < totalCount && columnOffset <= rowWidth; ++j) {
                    Rect rect = Rect.zero;
                    var cellSize = DataSource.CellSize(j);
                    switch(orientaion) {
                    case Orientaion.Vertical:
                        rect = new Rect(columnOffset, contentSize, cellSize.x, cellSize.y);
                        columnOffset += cellSize.x;
                        if(columnOffset > rowWidth) rect.x = 0;
                        break;
                    case Orientaion.Horizontal:
                        rect = new Rect(contentSize, columnOffset, cellSize.y, cellSize.x);
                        columnOffset += cellSize.y;
                        if(columnOffset > rowWidth) rect.y = 0;
                        break;
                    }
                    LookAheadedCellSizes.Add(new LookAheadedCellSize { index = j, rect = rect });
                    addCount++;
                }

                float rowHeight = LookAheadedCellSizes.Count > 1 ?
                    LookAheadedCellSizes.Take(LookAheadedCellSizes.Count - 1).Max(x => x.rect.height) :
                    LookAheadedCellSizes.Last().rect.height;
                for(int j = 0; j < LookAheadedCellSizes.Count; ++j) {
                    if(LookAheadedCellSizes.Count > 1 && j == LookAheadedCellSizes.Count - 1) {
                        i += addCount;
                        if(i < totalCount) {
                            LookAheadedCellSizes.RemoveRange(0, LookAheadedCellSizes.Count - 1);
                            break;
                        }
                        else {
                            rowHeight = LookAheadedCellSizes.Last().rect.height;
                        }
                    }
                    float cellUpper = 0;
                    int index = LookAheadedCellSizes[j].index;
                    var rect = LookAheadedCellSizes[j].rect;
                    switch(orientaion) {
                    case Orientaion.Vertical:
                        cellUpper = contentSize - contentRectLocalPosition.y;
                        rect.height = rowHeight;
                        break;
                    case Orientaion.Horizontal:
                        cellUpper = contentSize + contentRectLocalPosition.x;
                        rect.width = rowHeight;
                        break;
                    }

                    if(startIndex < 0) {
                        if(cellUpper >= -rowHeight && cellUpper <= viewLower) {
                            startIndex = endIndex = index;
                            cellRects[0] = rect;
                        }
                    }
                    else {
                        if(cellUpper >= -rowHeight && cellUpper <= viewLower) {
                            endIndex = index;
                            if(index - startIndex < cellRects.Length) cellRects[index - startIndex] = rect;
                        }
                    }

                    if(LookAheadedCellSizes.Count == 1) {
                        LookAheadedCellSizes.Clear();
                        i++;
                    }
                }
                contentSize += rowHeight;
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
                            size.x = cellRect.width;
                            localPosition.x = cellRect.x + size.x * rectTrans.pivot.x;
                            break;
                        case Orientaion.Horizontal:
                            size.x = cellRect.width;
                            localPosition.x = cellRect.x + size.x * rectTrans.pivot.x;
                            size.y = cellRect.height;
                            localPosition.y = -cellRect.y - size.y * rectTrans.pivot.y;
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
