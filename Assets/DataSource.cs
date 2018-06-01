using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CustomUnity;

namespace YourProjectNamespace
{
    [RequireComponent(typeof(ContentFiller))]
    public class DataSource : MonoBehaviour, ContentFiller.IDataSource
    {
        [System.Serializable]
        public struct CellData
        {
            public Color color;
            public string buttonTitle;
            public int height;
        }

        public CellData[] dataSource;

        public CellData[] dataSource2;

        CellData GetCellData(int index)
        {
            if(appendToFront) {
                if(index < dataSource2.Length) return dataSource2[index];
                else if(index < dataSource2.Length + dataSource.Length) return dataSource[index - dataSource2.Length];
                else return dataSource2[index - dataSource.Length - dataSource2.Length];
            }
            if(index < dataSource.Length) return dataSource[index];
            else return dataSource2[index - dataSource.Length];
        }

        public bool appendToFront;
        public bool appendToBack;

        public int TotalCount {
            get {
                return (appendToFront ? dataSource2.Length : 0) + dataSource.Length + (appendToBack ? dataSource2.Length : 0);
            }
        }

        bool prevAppendToFront;
        ContentFiller contentFiller;

        public void OnPreUpdate()
        {
            if(prevAppendToFront != appendToFront) {
                var moveSize = dataSource2.Sum(x => x.height);
                var pos = contentFiller.transform.localPosition;
                if(appendToFront) {
                    switch(contentFiller.orientaion) {
                    case ContentFiller.Orientaion.Vertical:
                        pos.y += moveSize;
                        break;
                    case ContentFiller.Orientaion.Horizontal:
                        pos.x -= moveSize;
                        break;
                    }
                }
                else {
                    switch(contentFiller.orientaion) {
                    case ContentFiller.Orientaion.Vertical:
                        pos.y -= moveSize;
                        break;
                    case ContentFiller.Orientaion.Horizontal:
                        pos.x += moveSize;
                        break;
                    }
                }
                contentFiller.transform.localPosition = pos;
            }
        }
        
        public Vector2 CellSize(int index)
        {
            var s = GetCellData(index).height;
            return new Vector2(s, s);
        }

        public void SetUpCell(int index, GameObject cell)
        {
            var data = GetCellData(index);
            cell.transform.Find("Image").GetComponent<Image>().color = data.color;
            cell.transform.Find("Button/Text").GetComponent<Text>().text = data.buttonTitle;
            var rectTransform = cell.GetComponent<RectTransform>();
            var sizeDelta = rectTransform.sizeDelta;
            switch(contentFiller.orientaion) {
            case ContentFiller.Orientaion.Vertical:
                sizeDelta.y = data.height;
                break;
            case ContentFiller.Orientaion.Horizontal:
                sizeDelta.x = data.height;
                break;
            }
            rectTransform.sizeDelta = sizeDelta;
        }

        public void UpdateCell(int index, GameObject cell)
        {
            if(prevAppendToFront != appendToFront) SetUpCell(index, cell);
        }

        void Awake()
        {
            contentFiller = GetComponent<ContentFiller>();
            contentFiller.DataSource = this;
            contentFiller.OnPreUpdate += OnPreUpdate;
        }

        void Start()
        {
            prevAppendToFront = appendToFront;
        }

        void LateUpdate()
        {
            prevAppendToFront = appendToFront;
        }
    }
}
