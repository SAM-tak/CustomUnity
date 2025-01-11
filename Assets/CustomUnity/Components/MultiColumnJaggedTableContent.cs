using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    /// <summary>
    /// Various cell size multi column table view.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MultiColumnJaggedTableContent : TableContentBase
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            void SetUpCell(int index, GameObject cell);
            void CellDeactivated(GameObject cell);
            Vector2 CellSize(int index);
            void OnPreUpdate();
        }

        public IDataSource DataSource { get; private set; }

        Rect[] _cellRects;

        public override float GetScrollAmountForBottomOfLastItem() => orientaion switch {
            TableOrientaion.Horizontal => GetComponent<RectTransform>().rect.width,
            TableOrientaion.Vertical => GetComponent<RectTransform>().rect.height,
            _ => 0f
        };

        public override int DataSourceTotalCount => DataSource.TotalCount;

        protected override void PreUpdate() => DataSource?.OnPreUpdate();

        struct LookAheadedCellSize : IEquatable<LookAheadedCellSize>
        {
            public int index;
            public Rect rect;

            public override bool Equals(object obj) => obj is LookAheadedCellSize size && Equals(size);

            public bool Equals(LookAheadedCellSize other) => index == other.index && rect.Equals(other.rect);

            public override readonly int GetHashCode() => HashCode.Combine(index, rect);
        }

        readonly List<LookAheadedCellSize> _lookAheadedCellSizes = new(10);

        protected override void Awake()
        {
            base.Awake();
            DataSource = GetComponent<IDataSource>();
        }

        protected override void Start()
        {
            base.Start();
            _cellRects = new Rect[transform.childCount];
        }

        protected override void UpdateContent()
        {
            if(!ScrollRect) return;

            var viewSize = ScrollRect.viewport.rect.size;
            var rowWidth = orientaion switch {
                TableOrientaion.Vertical => viewSize.x,
                TableOrientaion.Horizontal => viewSize.y,
                _ => 0f
            };
            var viewLower = orientaion switch {
                TableOrientaion.Vertical => viewSize.y,
                TableOrientaion.Horizontal => viewSize.x,
                _ => 0f
            };

            PreUpdate();

            var totalCount = DataSource != null ? DataSource.TotalCount : 0;

            float contentSize = 0;
            int startIndex = 0;
            int endIndex = -1;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            _lookAheadedCellSizes.Clear();
            for(int i = 0; i < totalCount;) {
                int addCount = 0;
                float rowHeight = _lookAheadedCellSizes.Count > 0 ? _lookAheadedCellSizes[0].rect.height : 0;
                float columnOffset = _lookAheadedCellSizes.Count > 0 ? _lookAheadedCellSizes[0].rect.width : 0;
                for(int j = i; j < totalCount && columnOffset <= rowWidth; ++j) {
                    Rect rect = Rect.zero;
                    var cellSize = DataSource.CellSize(j);
                    switch(orientaion) {
                    case TableOrientaion.Vertical:
                        rect = new Rect(columnOffset, contentSize, cellSize.x, cellSize.y);
                        columnOffset += cellSize.x;
                        if(columnOffset > rowWidth) rect.position = new Vector2(0, contentSize + rowHeight);
                        else if(rowHeight < cellSize.y) rowHeight = cellSize.y;
                        break;
                    case TableOrientaion.Horizontal:
                        rect = new Rect(contentSize, columnOffset, cellSize.y, cellSize.x);
                        columnOffset += cellSize.y;
                        if(columnOffset > rowWidth) rect.position = new Vector2(contentSize + rowHeight, 0);
                        else if(rowHeight < cellSize.x) rowHeight = cellSize.x;
                        break;
                    }
                    _lookAheadedCellSizes.Add(new LookAheadedCellSize { index = j, rect = rect });
                    addCount++;
                }

                for(int j = 0; j < _lookAheadedCellSizes.Count; ++j) {
                    if(_lookAheadedCellSizes.Count > 1 && j == _lookAheadedCellSizes.Count - 1) {
                        i += addCount;
                        if(i < totalCount) {
                            _lookAheadedCellSizes.RemoveRange(0, _lookAheadedCellSizes.Count - 1);
                            break;
                        }
                        else {
                            contentSize += rowHeight;
                            rowHeight = _lookAheadedCellSizes.Last().rect.height;
                        }
                    }
                    float cellUpper = 0;
                    int index = _lookAheadedCellSizes[j].index;
                    var rect = _lookAheadedCellSizes[j].rect;
                    switch(orientaion) {
                    case TableOrientaion.Vertical:
                        cellUpper = contentSize - contentRectLocalPosition.y;
                        rect.height = rowHeight;
                        break;
                    case TableOrientaion.Horizontal:
                        cellUpper = contentSize + contentRectLocalPosition.x;
                        rect.width = rowHeight;
                        break;
                    }

                    if(startIndex > endIndex) {
                        if(cellUpper >= -rowHeight && cellUpper <= viewLower) {
                            startIndex = endIndex = index;
                            _cellRects[Mathf.Min(startIndex, extraCells)] = rect;
                        }
                        else if(extraCells > 0) {
                            for(var n = Mathf.Min(index, extraCells - 1); n > 0; --n) _cellRects[n - 1] = _cellRects[n];
                            _cellRects[Mathf.Min(index, extraCells - 1)] = rect;
                        }
                    }
                    else {
                        if(cellUpper >= -rowHeight && cellUpper <= viewLower) {
                            endIndex = index;
                        }
                        if(index - Mathf.Max(0, startIndex - extraCells) < _cellRects.Length) {
                            _cellRects[index - Mathf.Max(0, startIndex - extraCells)] = rect;
                        }
                    }

                    if(_lookAheadedCellSizes.Count == 1) {
                        _lookAheadedCellSizes.Clear();
                        i += addCount;
                    }
                }
                contentSize += rowHeight;
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
                    if(cell && !(i - startIndex < 0 || i - startIndex >= _cellRects.Length)) {
                        var rectTrans = cell.GetComponent<RectTransform>();
                        var localPosition = rectTrans.localPosition;
                        var size = rectTrans.sizeDelta;
                        var cellRect = _cellRects[i - startIndex];
                        switch(orientaion) {
                        case TableOrientaion.Vertical:
                            size.y = cellRect.height;
                            localPosition.y = -cellRect.y - size.y * rectTrans.pivot.y;
                            size.x = cellRect.width;
                            localPosition.x = cellRect.x + size.x * rectTrans.pivot.x;
                            break;
                        case TableOrientaion.Horizontal:
                            size.x = cellRect.width;
                            localPosition.x = cellRect.x + size.x * rectTrans.pivot.x;
                            size.y = cellRect.height;
                            localPosition.y = -cellRect.y - size.y * rectTrans.pivot.y;
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
