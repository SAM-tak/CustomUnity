using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(RectTransform))]
    public class LargeTableContent : MonoBehaviour
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            void SetUpCell(int index, GameObject cell);
        }

        public Vector2 cellSize;

        [ReadOnlyWhenPlaying]
        public bool repeat;

        public int columnCount = 1;

        public Orientaion orientaion;
        
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
            var n = dataSource.TotalCount;
            switch(orientaion) {
            default:
            case Orientaion.Vertical:
                return new Vector2(n < columnCount ? n * cellSize.x : cellSize.x * columnCount, n * cellSize.y / columnCount);
            case Orientaion.Horizontal:
                return new Vector2(n * cellSize.x / columnCount, n < columnCount ? n * cellSize.y : cellSize.y * columnCount);
            }
        }

        const int merginScaler = 2;

        void OnValidate()
        {
            if(columnCount < 1) columnCount = 1;
        }

        void Start()
        {
            ScrollRect = GetComponentInParent<ScrollRect>();
            Debug.Assert(ScrollRect);
            contentRectTransform = GetComponent<RectTransform>();
            scrollRectTransform = GetComponentInParent<ScrollRect>().GetComponent<RectTransform>();
            cellPool = new Cell[transform.childCount];
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

            OnPreUpdate?.Invoke();

            var totalCount = (DataSource != null ? DataSource.TotalCount : 0);

            float contentSize = 0;
            int startIndex = -1;
            int endIndex = -1;
            var viewSize = scrollRectTransform.sizeDelta;
            var contentMargin = 0f;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            var sizeDelta = contentRectTransform.sizeDelta;
            switch(orientaion) {
            case Orientaion.Vertical:
                contentSize = totalCount * cellSize.y / columnCount;
                if(repeat) {
                    contentMargin = viewSize.y * merginScaler;
                    if(contentRectLocalPosition.y < contentMargin || contentRectLocalPosition.y > (contentMargin + contentSize)) {
                        contentRectLocalPosition.y = contentMargin + Math.Wrap(contentRectLocalPosition.y - contentMargin, contentSize);
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                startIndex = Mathf.FloorToInt((contentRectLocalPosition.y - contentMargin) / cellSize.y) * columnCount;
                endIndex = Mathf.FloorToInt(((contentRectLocalPosition.y - contentMargin) + viewSize.y) / cellSize.y) * columnCount + (columnCount - 1);
                sizeDelta.y = contentSize + contentMargin * merginScaler;
                break;
            case Orientaion.Horizontal:
                contentSize = totalCount * cellSize.x / columnCount;
                if(repeat) {
                    contentMargin = viewSize.x * merginScaler;
                    if(contentRectLocalPosition.x < contentMargin || contentRectLocalPosition.x > (contentMargin + contentSize)) {
                        contentRectLocalPosition.x = contentMargin + Math.Wrap(contentRectLocalPosition.x - contentMargin, contentSize);
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                startIndex = Mathf.FloorToInt((contentRectLocalPosition.x - contentMargin) / cellSize.x) * columnCount;
                endIndex = Mathf.FloorToInt(((contentRectLocalPosition.x - contentMargin) + viewSize.x) / cellSize.x) * columnCount + (columnCount - 1);
                sizeDelta.x = contentSize + contentMargin * merginScaler;
                break;
            }
            contentRectTransform.sizeDelta = sizeDelta;
            if(!repeat) {
                if(startIndex < 0) startIndex = 0;
                if(endIndex >= totalCount) endIndex = totalCount - 1;
            }
            
            foreach(var i in cellPool) {
                if(i.cell.activeSelf && (i.index < startIndex || i.index > endIndex)) i.cell.SetActive(false);
            }

            if(endIndex - startIndex + 1 > 0) {
                if(endIndex - startIndex + 1 > MaxCellsRequired) MaxCellsRequired = endIndex - startIndex + 1;
                for(int i = startIndex; i <= endIndex; ++i) {
                    int wrapedIndex = Math.Wrap(i, totalCount);
                    int firstinactive = -1;
                    for(int j = 0; j < cellPool.Length; j++) {
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
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            localPosition.y = -contentMargin - (i / columnCount * cellSize.y) - cellSize.y * rectTrans.pivot.y;
                            if(columnCount > 1) {
                                localPosition.x = (i % columnCount * cellSize.x) + cellSize.x * rectTrans.pivot.x;
                            }
                            break;
                        case Orientaion.Horizontal:
                            localPosition.x =  contentMargin + (i / columnCount * cellSize.x) + cellSize.x * rectTrans.pivot.x;
                            if(columnCount > 1) {
                                localPosition.y = -(i % columnCount * cellSize.y) - cellSize.y * rectTrans.pivot.y;
                            }
                            break;
                        }
                        rectTrans.sizeDelta = cellSize;
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
