using System.Collections.Generic;
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

        public int TotalCount => (appendToFront ? dataSource2.Length : 0) + dataSource.Length + (appendToBack ? dataSource2.Length : 0);

        bool _prevAppendToFront;
        bool _prevAppendToBack;
        MultiColumnJaggedTableContent _tableContent;

        public void OnPreUpdate()
        {
            if(_prevAppendToFront != appendToFront || _prevAppendToBack != appendToBack) _tableContent.Refresh();

            if(_prevAppendToFront != appendToFront) {
                var viewSize = _tableContent.GetComponent<RectTransform>().sizeDelta;
                var pos = _tableContent.transform.localPosition;
                var moveSize = 0f;
                var curWidth = 0f;
                var curHeight = 0f;
                switch(_tableContent.orientaion) {
                case TableOrientaion.Vertical:
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
                case TableOrientaion.Horizontal:
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
        
        public Vector2 CellSize(int index)
        {
            var c = GetCellData(index);
            return new Vector2(c.width, c.height);
        }

        bool _needsUpdateNavigation;

        public void SetUpCell(int index, GameObject cell)
        {
            _needsUpdateNavigation = true;
            var data = GetCellData(index);
            cell.transform.Find("Image").GetComponent<Image>().color = data.color;
            cell.transform.Find("Button/Text").GetComponent<Text>().text = data.buttonTitle;
        }

        public void CellDeactivated(GameObject cell)
        {
            _needsUpdateNavigation = true;
        }

        struct NavCell
        {
            public Selectable selectable;
            // position along the scroll direction (top/left edge, increasing toward the end of the list)
            public float primary;
            // range across the scroll direction (increasing toward right/bottom)
            public float crossMin;
            public float crossMax;
        }

        const float RowTolerance = 0.5f;

        static readonly IComparer<NavCell> PrimaryComparer = Comparer<NavCell>.Create((a, b) => a.primary.CompareTo(b.primary));
        static readonly IComparer<NavCell> CrossComparer = Comparer<NavCell>.Create((a, b) => a.crossMin.CompareTo(b.crossMin));

        readonly List<NavCell> _navCells = new();
        readonly List<(int start, int count)> _navRows = new();

        /// <summary>
        /// Build explicit navigation for active cells laid out as jagged rows.
        /// Along a row: previous / next cell. Across rows: the cell of the adjacent row that overlaps most.
        /// </summary>
        void UpdateNavigation()
        {
            var vertical = _tableContent.orientaion == TableOrientaion.Vertical;

            _navCells.Clear();
            foreach(Transform child in transform) {
                if(!child.gameObject.activeInHierarchy) continue;
                var selectable = child.GetComponentInChildren<Selectable>(true);
                if(!selectable) continue;
                var rectTransform = (RectTransform)child;
                var rect = rectTransform.rect;
                var position = (Vector2)rectTransform.localPosition;
                var xMin = position.x + rect.xMin;
                var xMax = position.x + rect.xMax;
                var yMin = position.y + rect.yMin;
                var yMax = position.y + rect.yMax;
                _navCells.Add(vertical
                    ? new NavCell { selectable = selectable, primary = -yMax, crossMin = xMin, crossMax = xMax }
                    : new NavCell { selectable = selectable, primary = xMin, crossMin = -yMax, crossMax = -yMin });
            }

            // group into rows by primary position, then order each row across
            _navCells.Sort(PrimaryComparer);
            _navRows.Clear();
            for(int i = 0; i < _navCells.Count;) {
                int start = i;
                var rowPrimary = _navCells[i].primary;
                while(i < _navCells.Count && _navCells[i].primary - rowPrimary <= RowTolerance) ++i;
                _navCells.Sort(start, i - start, CrossComparer);
                _navRows.Add((start, i - start));
            }

            for(int r = 0; r < _navRows.Count; ++r) {
                var (start, count) = _navRows[r];
                for(int i = start; i < start + count; ++i) {
                    var current = _navCells[i];
                    var crossBack = i > start ? _navCells[i - 1].selectable : null;
                    var crossForward = i < start + count - 1 ? _navCells[i + 1].selectable : null;
                    var primaryBack = r > 0 ? FindClosestInRow(_navRows[r - 1], current) : null;
                    var primaryForward = r < _navRows.Count - 1 ? FindClosestInRow(_navRows[r + 1], current) : null;

                    var navigation = current.selectable.navigation;
                    navigation.mode = Navigation.Mode.Explicit;
                    if(vertical) {
                        navigation.selectOnUp = primaryBack;
                        navigation.selectOnDown = primaryForward;
                        navigation.selectOnLeft = crossBack;
                        navigation.selectOnRight = crossForward;
                    }
                    else {
                        navigation.selectOnLeft = primaryBack;
                        navigation.selectOnRight = primaryForward;
                        navigation.selectOnUp = crossBack;
                        navigation.selectOnDown = crossForward;
                    }
                    current.selectable.navigation = navigation;
                }
            }
        }

        Selectable FindClosestInRow((int start, int count) row, NavCell from)
        {
            Selectable best = null;
            // positive: overlap length, negative: -distance between centers
            var bestScore = float.NegativeInfinity;
            var fromCenter = (from.crossMin + from.crossMax) * 0.5f;
            for(int i = row.start; i < row.start + row.count; ++i) {
                var cell = _navCells[i];
                var overlap = Mathf.Min(from.crossMax, cell.crossMax) - Mathf.Max(from.crossMin, cell.crossMin);
                var score = overlap > 0 ? overlap : -Mathf.Abs((cell.crossMin + cell.crossMax) * 0.5f - fromCenter);
                if(score > bestScore) {
                    bestScore = score;
                    best = cell.selectable;
                }
            }
            return best;
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
            _tableContent = GetComponent<MultiColumnJaggedTableContent>();
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
                UpdateNavigation();
            }
            _prevAppendToFront = appendToFront;
            _prevAppendToBack = appendToBack;
        }
    }
}
