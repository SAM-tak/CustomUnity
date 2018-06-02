using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(RectTransform))]
    public class FixedSizeListViewContent : MonoBehaviour
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            void SetUpCell(int index, GameObject cell);
            void UpdateCell(int index, GameObject cell);
        }

        public Vector2 cellSize;

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

        const int merginScaler = 2;

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
            var contentMargin = 0f;
            switch(orientaion) {
            case Orientaion.Vertical:
                if(repeat) {
                    contentMargin = viewSize.y * merginScaler;
                    if(contentRectLocalPosition.y < contentMargin) {
                        contentRectLocalPosition.y = contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                break;
            case Orientaion.Horizontal:
                if(repeat) {
                    contentMargin = viewSize.x * merginScaler;
                    if(contentRectLocalPosition.x < contentMargin) {
                        contentRectLocalPosition.x = contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                break;
            }
        }

        public int StartIndex { get; protected set; }
        public int EndIndex { get; protected set; }

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
                contentSize = totalCount * cellSize.y;
                if(repeat) {
                    contentMargin = viewSize.y * merginScaler;
                    if(contentRectLocalPosition.y < contentMargin || contentRectLocalPosition.y > (contentMargin + contentSize)) {
                        contentRectLocalPosition.y = contentMargin + Math.Wrap(contentRectLocalPosition.y - contentMargin, contentSize);
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                startIndex = Mathf.FloorToInt((contentRectLocalPosition.y - contentMargin) / cellSize.y);
                endIndex = Mathf.FloorToInt(((contentRectLocalPosition.y - contentMargin) + viewSize.y) / cellSize.y);
                sizeDelta.y = contentSize + contentMargin * merginScaler;
                break;
            case Orientaion.Horizontal:
                contentSize = totalCount * cellSize.x;
                if(repeat) {
                    contentMargin = viewSize.x * merginScaler;
                    if(contentRectLocalPosition.x < contentMargin || contentRectLocalPosition.x > (contentMargin + contentSize)) {
                        contentRectLocalPosition.x = contentMargin + Math.Wrap(contentRectLocalPosition.x - contentMargin, contentSize);
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                startIndex = Mathf.FloorToInt((contentRectLocalPosition.x - contentMargin) / cellSize.x);
                endIndex = Mathf.FloorToInt(((contentRectLocalPosition.x - contentMargin) + viewSize.x) / cellSize.x);
                sizeDelta.x = contentSize + contentMargin * merginScaler;
                break;
            }
            contentRectTransform.sizeDelta = sizeDelta;
            if(!repeat) {
                if(startIndex < 0) startIndex = 0;
                if(endIndex >= totalCount) endIndex = totalCount - 1;
            }

            StartIndex = startIndex;
            EndIndex = endIndex;

            foreach(var i in cellPool) {
                if(i.cell.activeSelf && (i.index < startIndex || i.index > endIndex)) i.cell.SetActive(false);
            }

            if(endIndex - startIndex + 1 > 0) {
                if(endIndex - startIndex + 1 > MaxCellsRequired) MaxCellsRequired = endIndex - startIndex + 1;
                for(int i = startIndex; i <= endIndex; ++i) {
                    int wrapedIndex = Math.Wrap(i, totalCount);
                    bool found = false;
                    int firstinactive = -1;
                    for(int j = 0; j < cellPool.Length; j++) {
                        if(cellPool[j].cell.activeSelf) {
                            if(cellPool[j].index == i) {
                                found = true;
                                DataSource.UpdateCell(wrapedIndex, cellPool[j].cell);
                                break;
                            }
                        }
                        else if(firstinactive < 0) firstinactive = j;
                    }
                    if(!found && firstinactive >= 0) {
                        var x = cellPool[firstinactive];
                        var rectTrans = x.cell.GetComponent<RectTransform>();
                        var localPosition = rectTrans.localPosition;
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            localPosition.y = -contentMargin - (i * cellSize.y) - cellSize.y * rectTrans.pivot.y;
                            break;
                        case Orientaion.Horizontal:
                            localPosition.x =  contentMargin + (i * cellSize.x) + cellSize.x * rectTrans.pivot.x;
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
