using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    /// <summary>
    /// Various size cell table view.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class JaggedTableContent : TableContentBase
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            void SetUpCell(int index, GameObject cell);
            void CellDeactivated(GameObject cell);
            float CellSize(int index);
            void OnPreUpdate();
        }

        public IDataSource DataSource { get; private set; }
        
        struct CellPosition : IEquatable<CellPosition>
        {
            public float position;
            public float size;

            public override bool Equals(object obj) => obj is CellPosition position && Equals(position);

            public bool Equals(CellPosition other) => position == other.position && size == other.size;

            public override int GetHashCode() => HashCode.Combine(position, size);
        }

        CellPosition[] cellPositions;

        public override float GetScrollAmountForBottomOfLastItem()
        {
            return orientaion switch {
                Orientaion.Horizontal => GetComponent<RectTransform>().rect.width,
                _ => GetComponent<RectTransform>().rect.height,
            };
        }

        public override int DataSourceTotalCount => DataSource.TotalCount;

        protected override void PreUpdate() => DataSource?.OnPreUpdate();

        protected override void Awake()
        {
            base.Awake();
            DataSource = GetComponent<IDataSource>();
        }

        protected override void Start()
        {
            base.Start();
            cellPositions = new CellPosition[transform.childCount];
        }

        protected override void UpdateContent()
        {
            if(!ScrollRect) return;

            var totalCount = DataSource != null ? DataSource.TotalCount : 0;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            var viewLower = orientaion switch {
                Orientaion.Vertical => ScrollRect.viewport.rect.height,
                Orientaion.Horizontal => ScrollRect.viewport.rect.width,
                _ => 0f
            };

            float contentSize = 0;
            int startIndex = 0;
            int endIndex = -1;

            for(int i = 0; i < totalCount; ++i) {
                var cellSize = DataSource.CellSize(i);
                var cellUpper = orientaion switch {
                    Orientaion.Vertical => contentSize - contentRectLocalPosition.y,
                    Orientaion.Horizontal => contentSize + contentRectLocalPosition.x,
                    _ => 0f
                };
                var cellPosition = new CellPosition { position = contentSize, size = cellSize };

                contentSize += cellSize;

                if(startIndex > endIndex) {
                    if(cellUpper >= -cellSize && cellUpper <= viewLower) {
                        startIndex = endIndex = i;
                        cellPositions[Mathf.Min(startIndex, extraCells)] = cellPosition;
                    }
                    else if(extraCells > 0) {
                        for(var n = Mathf.Min(i, extraCells - 1); n > 0; --n) cellPositions[n - 1] = cellPositions[n];
                        cellPositions[Mathf.Min(i, extraCells - 1)] = cellPosition;
                    }
                }
                else {
                    if(cellUpper >= -cellSize && cellUpper <= viewLower) endIndex = i;
                    if(i - Mathf.Max(0, startIndex - extraCells) < cellPositions.Length) {
                        cellPositions[i - Mathf.Max(0, startIndex - extraCells)] = cellPosition;
                    }
                }
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

            startIndex -= extraCells;
            endIndex += extraCells;

            if(startIndex < 0) startIndex = 0;
            if(endIndex >= totalCount) endIndex = totalCount - 1;

            foreach(var i in cellPool) {
                if(i.cell.activeSelf && (i.index < startIndex || i.index > endIndex)) i.cell.SetActive(false);
            }

            if(endIndex - startIndex + 1 > 0) {
                if(endIndex - startIndex + 1 > MaxCellsRequired) MaxCellsRequired = endIndex - startIndex + 1;
                for(int i = startIndex; i <= endIndex; ++i) {
                    int wrapedIndex = Math.Wrap(i, totalCount);
                    var cell = GetCell(i, out var @new);
                    if(cell && !(i - startIndex < 0 || i - startIndex >= cellPositions.Length)) {
                        var rectTrans = cell.GetComponent<RectTransform>();
                        var localPosition = rectTrans.localPosition;
                        var size = rectTrans.sizeDelta;
                        var cellPosition = cellPositions[i - startIndex];
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            size.y = cellPosition.size;
                            localPosition.y = -cellPosition.position - size.y * rectTrans.pivot.y;
                            break;
                        case Orientaion.Horizontal:
                            size.x = cellPosition.size;
                            localPosition.x = cellPosition.position + size.x * rectTrans.pivot.x;
                            break;
                        }
                        rectTrans.sizeDelta = size;
                        rectTrans.localPosition = localPosition;
                        if(@new) {
                            DataSource.SetUpCell(wrapedIndex, cell);
                            cell.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}
