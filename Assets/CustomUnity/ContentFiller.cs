using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(RectTransform))]
    public class ContentFiller : MonoBehaviour
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            Vector2 CellSize(int index);
            void SetUpCell(int index, GameObject cell);
            void UpdateCell(int index, GameObject cell);
        }

        public enum Orientaion
        {
            Vertical,
            Horizontal
        }

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
        Vector2[] cellPositions;
        
        void Start()
        {
            ScrollRect = GetComponentInParent<ScrollRect>();
            Debug.Assert(ScrollRect);
            contentRectTransform = GetComponent<RectTransform>();
            scrollRectTransform = GetComponentInParent<ScrollRect>().GetComponent<RectTransform>();
            cellPool = new Cell[transform.childCount];
            cellPositions = new Vector2[transform.childCount];
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

            float contentSize = 0;
            int startIndex = -1;
            int endIndex = -1;
            var viewSize = scrollRectTransform.sizeDelta;
            float viewLower = 0;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            switch(orientaion) {
            case Orientaion.Vertical:
                viewLower = viewSize.y;
                break;
            case Orientaion.Horizontal:
                viewLower = viewSize.x;
                break;
            }

            OnPreUpdate?.Invoke();

            for(int i = 0; i < (DataSource != null ? DataSource.TotalCount : 0); ++i) {
                float size = 0;
                float cellUpper = 0;
                Vector2 position = Vector2.zero;
                switch(orientaion) {
                case Orientaion.Vertical:
                    cellUpper = contentSize - contentRectLocalPosition.y;
                    size = DataSource.CellSize(i).y;
                    position = new Vector2(0, contentSize);
                    break;
                case Orientaion.Horizontal:
                    cellUpper = contentSize + contentRectLocalPosition.x;
                    size = DataSource.CellSize(i).x;
                    position = new Vector2(contentSize, 0);
                    break;
                }
                if(startIndex < 0) {
                    if(cellUpper >= -size && cellUpper <= viewLower) {
                        startIndex = endIndex = i;
                        cellPositions[0] = position;
                    }
                }
                else {
                    if(cellUpper >= -size && cellUpper <= viewLower) {
                        endIndex = i;
                        if(i - startIndex < cellPositions.Length) cellPositions[i - startIndex] = position;
                    }
                }
                contentSize += size;
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

            for(int i = 0; i < cellPool.Length; ++i) {
                if(cellPool[i].index >= 0 && (cellPool[i].index < startIndex || cellPool[i].index > endIndex)) {
                    var x = cellPool[i];
                    x.cell.SetActive(false);
                    x.index = -1;
                    cellPool[i] = x;
                }
            }

            if(startIndex >= 0) {
                //if(endIndex - startIndex + 1 > cellPool.Length) LogWarning("Short of Cell Object : current active count is {0}", endIndex - startIndex + 1);
                if(endIndex - startIndex + 1 > MaxCellsRequired) MaxCellsRequired = endIndex - startIndex + 1;
                for(int i = startIndex; i <= endIndex; ++i) {
                    int celli = -1;
                    for(int j = 0; j < cellPool.Length; ++j) {
                        if(cellPool[j].index == i) {
                            celli = j;
                            DataSource.UpdateCell(i, cellPool[j].cell);
                            break;
                        }
                    }
                    if(celli < 0) {
                        for(int j = 0; j < cellPool.Length; ++j) {
                            if(cellPool[j].index < 0 && endIndex - startIndex < cellPositions.Length) {
                                var x = cellPool[j];
                                DataSource.SetUpCell(i, x.cell);
                                var rectTrans = x.cell.GetComponent<RectTransform>();
                                var localPosition = rectTrans.localPosition;
                                var size = rectTrans.sizeDelta;
                                switch(orientaion) {
                                case Orientaion.Vertical:
                                    localPosition.y = -cellPositions[i - startIndex].y - size.y * rectTrans.pivot.y;
                                    break;
                                case Orientaion.Horizontal:
                                    localPosition.x =  cellPositions[i - startIndex].x + size.x * rectTrans.pivot.x;
                                    break;
                                }
                                rectTrans.localPosition = localPosition;
                                x.cell.SetActive(true);
                                x.index = i;
                                cellPool[j] = x;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
