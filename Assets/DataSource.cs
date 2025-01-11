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

        public int TotalCount => (appendToFront ? dataSource2.Length : 0) + dataSource.Length + (appendToBack ? dataSource2.Length : 0);

        bool _prevAppendToFront;
        bool _prevAppendToBack;
        JaggedTableContent _tableContent;

        void JaggedTableContent.IDataSource.OnPreUpdate()
        {
            if(_prevAppendToFront != appendToFront || _prevAppendToBack != appendToBack) _tableContent.Refresh();

            if(_prevAppendToFront != appendToFront) {
                var moveSize = dataSource2.Sum(x => x.size);
                var pos = _tableContent.transform.localPosition;
                if(appendToFront) {
                    switch(_tableContent.orientaion) {
                    case TableOrientaion.Vertical:
                        pos.y += moveSize;
                        break;
                    case TableOrientaion.Horizontal:
                        pos.x -= moveSize;
                        break;
                    }
                }
                else {
                    switch(_tableContent.orientaion) {
                    case TableOrientaion.Vertical:
                        pos.y -= moveSize;
                        break;
                    case TableOrientaion.Horizontal:
                        pos.x += moveSize;
                        break;
                    }
                }
                _tableContent.transform.localPosition = pos;
            }
        }
        
        float JaggedTableContent.IDataSource.CellSize(int index) => GetCellData(index).size;

        bool _needsUpdateNavigation;

        void JaggedTableContent.IDataSource.SetUpCell(int index, GameObject cell)
        {
            _needsUpdateNavigation = true;
            var data = GetCellData(index);
            cell.transform.Find("Image").GetComponent<Image>().color = data.color;
            cell.transform.Find("Button/Text").GetComponent<Text>().text = data.buttonTitle;
        }

        void JaggedTableContent.IDataSource.CellDeactivated(GameObject cell)
        {
            _needsUpdateNavigation = true;
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
            _tableContent = GetComponent<JaggedTableContent>();
        }

        void Start()
        {
            _prevAppendToFront = appendToFront;
            _prevAppendToBack = appendToBack;
        }

        void LateUpdate()
        {
            if(_needsUpdateNavigation) {
                _needsUpdateNavigation = false;
                var children = transform.EnumChildren()
                    .Where(x => x.gameObject.activeInHierarchy)
                    .OrderByDescending(x => _tableContent.orientaion == TableOrientaion.Vertical ? x.position.y : x.position.x)
                    .ToArray();
                for(int i = 1; i < children.Length; i++) {
                    var prevButton = children[i - 1].GetComponentInChildren<Button>(true);
                    var button = children[i].GetComponentInChildren<Button>(true);

                    var navigation = prevButton.navigation;
                    if(_tableContent.orientaion == TableOrientaion.Vertical) {
                        if(i == 1) navigation.selectOnUp = null;
                        navigation.selectOnDown = button;
                    }
                    else {
                        if(i == 1) navigation.selectOnLeft = null;
                        navigation.selectOnRight = button;
                    }
                    prevButton.navigation = navigation;

                    navigation = button.navigation;
                    if(_tableContent.orientaion == TableOrientaion.Vertical) {
                        navigation.selectOnUp = prevButton;
                        if(i == children.Length - 1) navigation.selectOnDown = null;
                    }
                    else {
                        navigation.selectOnLeft = prevButton;
                        if(i == children.Length - 1) navigation.selectOnRight = null;
                    }
                    button.navigation = navigation;
                }
            }
            _prevAppendToFront = appendToFront;
            _prevAppendToBack = appendToBack;
        }
    }
}
