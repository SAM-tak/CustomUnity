using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(RectTransform))]
    public class ContentsFiller : MonoBehaviour
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            float CellSize(int index, ContentsFiller contentsFiller);
            void SetUpCell(int index, ContentsFiller contentsFiller, GameObject cell);
        }

        public enum Orientaion
        {
            Vertical,
            Horizontal
        }

        public Orientaion orientaion;
        
        public IDataSource dataSource;

        RectTransform contentRectTransform;
        RectTransform scrollRectTransform;
        
        struct Cell
        {
            public GameObject cell;
            public int index;
        }

        Cell[] cellPool;
        float[] cellPositions;
        
        void Start()
        {
            contentRectTransform = GetComponent<RectTransform>();
            scrollRectTransform = GetComponentInParent<ScrollRect>().GetComponent<RectTransform>();
            cellPool = new Cell[transform.childCount];
            cellPositions = new float[transform.childCount];
            for(int i = 0; i < transform.childCount; i++) {
                var go = transform.GetChild(i).gameObject;
                go.SetActive(false);
                cellPool[i].cell = go;
                cellPool[i].index = -1;
            }
        }

        void Update()
        {
            float contentSize = 0;
            int startIndex = -1;
            int endIndex = -1;
            var viewSize = scrollRectTransform.sizeDelta;
            float viewLower = 0;
            switch(orientaion) {
            case Orientaion.Vertical:
                viewLower = viewSize.y;
                break;
            case Orientaion.Horizontal:
                viewLower = viewSize.x;
                break;
            }
            for(int i = 0; i < dataSource.TotalCount; ++i) {
                var size = dataSource.CellSize(i, this);
                float cellUpper = 0;
                switch(orientaion) {
                case Orientaion.Vertical:
                    cellUpper = contentSize - contentRectTransform.localPosition.y;
                    break;
                case Orientaion.Horizontal:
                    cellUpper = contentSize - contentRectTransform.localPosition.x;
                    break;
                }
                if(startIndex < 0) {
                    if(cellUpper >= -size && cellUpper <= viewLower) {
                        startIndex = endIndex = i;
                        cellPositions[0] = contentSize;
                    }
                }
                else {
                    if(cellUpper >= -size && cellUpper <= viewLower) {
                        endIndex = i;
                        if(i - startIndex < cellPositions.Length) cellPositions[i - startIndex] = contentSize;
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
                if(endIndex - startIndex + 1 > cellPool.Length) LogWarning("Short of Cell object : current active count is {0}", endIndex - startIndex + 1);
                for(int i = startIndex; i <= endIndex; ++i) {
                    int celli = -1;
                    for(int j = 0; j < cellPool.Length; ++j) {
                        if(cellPool[j].index == i) {
                            celli = j;
                            break;
                        }
                    }
                    if(celli < 0) {
                        for(int j = 0; j < cellPool.Length; ++j) {
                            if(cellPool[j].index < 0 && endIndex - startIndex < cellPositions.Length) {
                                var x = cellPool[j];
                                dataSource.SetUpCell(i, this, x.cell);
                                var rectTrans = x.cell.GetComponent<RectTransform>();
                                var localPosition = rectTrans.localPosition;
                                var size = rectTrans.sizeDelta;
                                switch(orientaion) {
                                case Orientaion.Vertical:
                                    localPosition.y = -cellPositions[i - startIndex] - size.y * rectTrans.pivot.y;
                                    break;
                                case Orientaion.Horizontal:
                                    localPosition.x = -cellPositions[i - startIndex] - size.x * rectTrans.pivot.x;
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
