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

            public override readonly bool Equals(object obj) => obj is CellPosition position && Equals(position);

            public readonly bool Equals(CellPosition other) => position == other.position && size == other.size;

            public override readonly int GetHashCode() => HashCode.Combine(position, size);
        }

        CellPosition[] _cellPositions;

        public override float GetScrollAmountForBottomOfLastItem() => orientaion switch {
            TableOrientaion.Horizontal => GetComponent<RectTransform>().rect.width,
            TableOrientaion.Vertical => GetComponent<RectTransform>().rect.height,
            _ => 0f
        };

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
            _cellPositions = new CellPosition[transform.childCount];
        }

        protected override void UpdateContent()
        {
            if(!ScrollRect) return;
            if(!EnsureCellCount(extraCells + 1)) return;
            if(_cellPositions.Length < cellPool.Length) Array.Resize(ref _cellPositions, cellPool.Length);

            var totalCount = DataSource != null ? DataSource.TotalCount : 0;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            var viewLower = orientaion switch {
                TableOrientaion.Vertical => ScrollRect.viewport.rect.height,
                TableOrientaion.Horizontal => ScrollRect.viewport.rect.width,
                _ => 0f
            };

            float contentSize = 0;
            int startIndex = 0;
            int endIndex = -1;

            for(int i = 0; i < totalCount; ++i) {
                var cellSize = DataSource.CellSize(i);
                var cellUpper = orientaion switch {
                    TableOrientaion.Vertical => contentSize - contentRectLocalPosition.y,
                    TableOrientaion.Horizontal => contentSize + contentRectLocalPosition.x,
                    _ => 0f
                };
                var cellPosition = new CellPosition { position = contentSize, size = cellSize };

                contentSize += cellSize;

                if(startIndex > endIndex) {
                    if(cellUpper >= -cellSize && cellUpper <= viewLower) {
                        startIndex = endIndex = i;
                        _cellPositions[Mathf.Min(startIndex, extraCells)] = cellPosition;
                    }
                    else if(extraCells > 0) {
                        // keep last extraCells positions before the viewport as a sliding window
                        if(i < extraCells) _cellPositions[i] = cellPosition;
                        else {
                            for(var n = 1; n < extraCells; ++n) _cellPositions[n - 1] = _cellPositions[n];
                            _cellPositions[extraCells - 1] = cellPosition;
                        }
                    }
                }
                else {
                    if(cellUpper >= -cellSize && cellUpper <= viewLower) endIndex = i;
                    if(i - Mathf.Max(0, startIndex - extraCells) < _cellPositions.Length) {
                        _cellPositions[i - Mathf.Max(0, startIndex - extraCells)] = cellPosition;
                    }
                }
            }

            var sizeDelta = contentRectTransform.sizeDelta;
            switch(orientaion) {
            case TableOrientaion.Vertical:
                sizeDelta.y = contentSize;
                break;
            case TableOrientaion.Horizontal:
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
                    if(_cellPositions.Length < transform.childCount) {
                        Array.Resize(ref _cellPositions, transform.childCount);
                        UpdateContent();
                        return;
                    }
                    if(cell && !(i - startIndex < 0 || i - startIndex >= _cellPositions.Length)) {
                        var rectTransform = cell.GetComponent<RectTransform>();
                        var localPosition = rectTransform.localPosition;
                        var size = rectTransform.sizeDelta;
                        var cellPosition = _cellPositions[i - startIndex];
                        switch(orientaion) {
                        case TableOrientaion.Vertical:
                            size.y = cellPosition.size;
                            localPosition.y = -cellPosition.position - size.y * rectTransform.pivot.y;
                            break;
                        case TableOrientaion.Horizontal:
                            size.x = cellPosition.size;
                            localPosition.x = cellPosition.position + size.x * rectTransform.pivot.x;
                            break;
                        }
                        rectTransform.sizeDelta = size;
                        rectTransform.localPosition = localPosition;
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
