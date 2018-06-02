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
                cellPool[i].index = -1;
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
            var contentRectLocalPosition = contentRectTransform.localPosition;
            switch(orientaion) {
            case Orientaion.Vertical:
                contentSize = totalCount * cellSize.y;
                startIndex = Mathf.CeilToInt(contentRectLocalPosition.y % contentSize / cellSize.y);
                endIndex = Mathf.CeilToInt((contentRectLocalPosition.y + viewSize.y) % contentSize / cellSize.y);
                break;
            case Orientaion.Horizontal:
                contentSize = totalCount * cellSize.x;
                startIndex = Mathf.CeilToInt(contentRectLocalPosition.x % contentSize / cellSize.x);
                endIndex = Mathf.CeilToInt((contentRectLocalPosition.x + viewSize.x) % contentSize / cellSize.x);
                break;
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
                            localPosition.y = -(i * cellSize.y) - cellSize.y * rectTrans.pivot.y;
                            break;
                        case Orientaion.Horizontal:
                            localPosition.x =  (i * cellSize.x) + cellSize.x * rectTrans.pivot.x;
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
