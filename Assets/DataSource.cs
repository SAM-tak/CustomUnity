using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CustomUnity;

namespace YourProjectNamespace
{
    [RequireComponent(typeof(JaggedTableContent))]
    public class DataSource : MonoBehaviour, JaggedTableContent.IDataSource
    {
        [System.Serializable]
        public struct CellData
        {
            public Color color;
            public string buttonTitle;
            public int size;
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
        bool prevAppendToBack;
        JaggedTableContent tableContent;

        public void OnPreUpdate()
        {
            if(prevAppendToFront != appendToFront || prevAppendToBack != appendToBack) tableContent.InactivateAllCells();

            if(prevAppendToFront != appendToFront) {
                var moveSize = dataSource2.Sum(x => x.size);
                var pos = tableContent.transform.localPosition;
                if(appendToFront) {
                    switch(tableContent.orientaion) {
                    case Orientaion.Vertical:
                        pos.y += moveSize;
                        break;
                    case Orientaion.Horizontal:
                        pos.x -= moveSize;
                        break;
                    }
                }
                else {
                    switch(tableContent.orientaion) {
                    case Orientaion.Vertical:
                        pos.y -= moveSize;
                        break;
                    case Orientaion.Horizontal:
                        pos.x += moveSize;
                        break;
                    }
                }
                tableContent.transform.localPosition = pos;
            }
        }
        
        public float CellSize(int index)
        {
            return GetCellData(index).size;
        }

        public void SetUpCell(int index, GameObject cell)
        {
            var data = GetCellData(index);
            cell.transform.Find("Image").GetComponent<Image>().color = data.color;
            cell.transform.Find("Button/Text").GetComponent<Text>().text = data.buttonTitle;
        }
        
#if UNITY_EDITOR
        [ContextMenu("Make Test Data")]
        void MakeTestData()
        {
            UnityEditor.Undo.RecordObject(this, "Make Test Data");
            dataSource = new CellData[100];
            for(int i = 0; i < 100; ++i) {
                dataSource[i].color = Color.Lerp(Color.magenta, Color.green, i / 100.0f);
                dataSource[i].buttonTitle = $"Button {i}";
                dataSource[i].size = 10 <= i && i < 15 ? 30 : Mathf.FloorToInt(Random.Range(50, 80));
            }
            dataSource2 = new CellData[10];
            for(int i = 0; i < 10; ++i) {
                dataSource2[i].color = Color.Lerp(Color.magenta, Color.green, i / 10.0f);
                dataSource2[i].buttonTitle = $"Button2 {i}";
                dataSource2[i].size = Mathf.FloorToInt(Random.Range(50, 80));
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        [ContextMenu("Make Test Data 2")]
        void MakeTestData2()
        {
            UnityEditor.Undo.RecordObject(this, "Make Test Data 2");
            dataSource = new CellData[100];
            for(int i = 0; i < 100; ++i) {
                dataSource[i].color = Color.Lerp(Color.magenta, Color.green, i / 100.0f);
                dataSource[i].buttonTitle = $"Button {i}";
                dataSource[i].size = Mathf.FloorToInt(Random.Range(80, 150));
            }
            dataSource2 = new CellData[10];
            for(int i = 0; i < 10; ++i) {
                dataSource2[i].color = Color.Lerp(Color.magenta, Color.green, i / 10.0f);
                dataSource2[i].buttonTitle = $"Button2 {i}";
                dataSource2[i].size = Mathf.FloorToInt(Random.Range(80, 120));
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        void Awake()
        {
            tableContent = GetComponent<JaggedTableContent>();
            tableContent.DataSource = this;
            tableContent.OnPreUpdate += OnPreUpdate;
        }

        void Start()
        {
            prevAppendToFront = appendToFront;
            prevAppendToBack = appendToBack;
        }

        void LateUpdate()
        {
            prevAppendToFront = appendToFront;
            prevAppendToBack = appendToBack;
        }
    }
}
