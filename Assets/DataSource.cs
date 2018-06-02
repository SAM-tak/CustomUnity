using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CustomUnity;

namespace YourProjectNamespace
{
    [RequireComponent(typeof(LargeJaggedTableContent))]
    public class DataSource : MonoBehaviour, LargeJaggedTableContent.IDataSource
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
        LargeJaggedTableContent listViewContent;

        public void OnPreUpdate()
        {
            if(prevAppendToFront != appendToFront) {
                var moveSize = dataSource2.Sum(x => x.height);
                var pos = listViewContent.transform.localPosition;
                if(appendToFront) {
                    switch(listViewContent.orientaion) {
                    case Orientaion.Vertical:
                        pos.y += moveSize;
                        break;
                    case Orientaion.Horizontal:
                        pos.x -= moveSize;
                        break;
                    }
                }
                else {
                    switch(listViewContent.orientaion) {
                    case Orientaion.Vertical:
                        pos.y -= moveSize;
                        break;
                    case Orientaion.Horizontal:
                        pos.x += moveSize;
                        break;
                    }
                }
                listViewContent.transform.localPosition = pos;
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
        }

        public void UpdateCell(int index, GameObject cell)
        {
            if(prevAppendToFront != appendToFront) SetUpCell(index, cell);
        }

        [ContextMenu("Make Test Data")]
        void MakeTestData()
        {
            dataSource = new CellData[100];
            for(int i = 0; i < 100; ++i) {
                dataSource[i].color = Color.Lerp(Color.magenta, Color.green, i / 100.0f);
                dataSource[i].buttonTitle = $"Button {i}";
                dataSource[i].height = 10 <= i && i < 15 ? 30 : Mathf.FloorToInt(Random.Range(50, 80));
            }
            dataSource2 = new CellData[10];
            for(int i = 0; i < 10; ++i) {
                dataSource2[i].color = Color.Lerp(Color.magenta, Color.green, i / 10.0f);
                dataSource2[i].buttonTitle = $"Button2 {i}";
                dataSource2[i].height = Mathf.FloorToInt(Random.Range(50, 80));
            }
        }
        
        [ContextMenu("Make Test Data 2")]
        void MakeTestData2()
        {
            dataSource = new CellData[100];
            for(int i = 0; i < 100; ++i) {
                dataSource[i].color = Color.Lerp(Color.magenta, Color.green, i / 100.0f);
                dataSource[i].buttonTitle = $"Button {i}";
                dataSource[i].height = Mathf.FloorToInt(Random.Range(80, 150));
            }
            dataSource2 = new CellData[10];
            for(int i = 0; i < 10; ++i) {
                dataSource2[i].color = Color.Lerp(Color.magenta, Color.green, i / 10.0f);
                dataSource2[i].buttonTitle = $"Button2 {i}";
                dataSource2[i].height = Mathf.FloorToInt(Random.Range(80, 120));
            }
        }

        void Awake()
        {
            listViewContent = GetComponent<LargeJaggedTableContent>();
            listViewContent.DataSource = this;
            listViewContent.OnPreUpdate += OnPreUpdate;
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
