using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CustomUnity;

namespace YourProjectNamespace
{
    [RequireComponent(typeof(MultiColumnJaggedTableContent))]
    public class MultiColumnDataSource : MonoBehaviour, MultiColumnJaggedTableContent.IDataSource
    {
        [System.Serializable]
        public struct CellData
        {
            public Color color;
            public string buttonTitle;
            public int width;
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
        bool prevAppendToBack;
        MultiColumnJaggedTableContent tableContent;

        public void OnPreUpdate()
        {
            if(prevAppendToFront != appendToFront || prevAppendToBack != appendToBack) tableContent.InactivateAllCells();

            if(prevAppendToFront != appendToFront) {
                var viewSize = tableContent.GetComponent<RectTransform>().sizeDelta;
                var pos = tableContent.transform.localPosition;
                var moveSize = 0f;
                var curWidth = 0f;
                var curHeight = 0f;
                switch(tableContent.orientaion) {
                case Orientaion.Vertical:
                    foreach(var i in dataSource2) {
                        if(curWidth + i.width > viewSize.x) {
                            moveSize += curHeight;
                            curHeight = curWidth = 0;
                        }
                        else {
                            if(curHeight < i.height) curHeight = i.height;
                            curWidth += i.width;
                        }
                    }
                    break;
                case Orientaion.Horizontal:
                    foreach(var i in dataSource2) {
                        if(curWidth + i.height > viewSize.y) {
                            moveSize += curHeight;
                            curHeight = curWidth = 0;
                        }
                        else {
                            if(curHeight < i.width) curHeight = i.width;
                            curWidth += i.height;
                        }
                    }
                    break;
                }
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
        
        public Vector2 CellSize(int index)
        {
            var c = GetCellData(index);
            return new Vector2(c.width, c.height);
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
                dataSource[i].width = Mathf.FloorToInt(Random.Range(100, 120));
                dataSource[i].height = Mathf.FloorToInt(Random.Range(50, 80));
            }
            dataSource2 = new CellData[10];
            for(int i = 0; i < 10; ++i) {
                dataSource2[i].color = Color.Lerp(Color.magenta, Color.green, i / 10.0f);
                dataSource2[i].buttonTitle = $"Button2 {i}";
                dataSource2[i].width = Mathf.FloorToInt(Random.Range(100, 120));
                dataSource2[i].height = Mathf.FloorToInt(Random.Range(50, 80));
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
                dataSource[i].width = 100;
                dataSource[i].height = Mathf.FloorToInt(Random.Range(50, 80));
            }
            dataSource2 = new CellData[10];
            for(int i = 0; i < 10; ++i) {
                dataSource2[i].color = Color.Lerp(Color.magenta, Color.green, i / 10.0f);
                dataSource2[i].buttonTitle = $"Button2 {i}";
                dataSource2[i].width = 100;
                dataSource2[i].height = Mathf.FloorToInt(Random.Range(50, 80));
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        void Awake()
        {
            tableContent = GetComponent<MultiColumnJaggedTableContent>();
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
