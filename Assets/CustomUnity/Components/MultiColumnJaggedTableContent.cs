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
            void OnPreUpdate();
            Vector2 CellSize(int index);
        }

        public IDataSource DataSource { get; private set; }

        Rect[] cellRects;

        public override float GetScrollAmountForBottomOfLastItem()
        {
            return orientaion switch {
                Orientaion.Horizontal => GetComponent<RectTransform>().rect.width,
                _ => GetComponent<RectTransform>().rect.height,
            };
        }

        public override int DataSourceTotalCount => DataSource.TotalCount;

        protected override void PreUpdate() => DataSource?.OnPreUpdate();

        struct LookAheadedCellSize : IEquatable<LookAheadedCellSize>
        {
            public int index;
            public Rect rect;

            public override bool Equals(object obj) => obj is LookAheadedCellSize size && Equals(size);

            public bool Equals(LookAheadedCellSize other) => index == other.index && rect.Equals(other.rect);

            public override int GetHashCode() => HashCode.Combine(index, rect);
        }

        List<LookAheadedCellSize> LookAheadedCellSizes { get; } = new List<LookAheadedCellSize>(10);

        protected override void Awake()
        {
            base.Awake();
            DataSource = GetComponent<IDataSource>();
        }

        protected override void Start()
        {
            base.Start();
            cellRects = new Rect[transform.childCount];
        }

        protected override void UpdateContent()
        {
            if(!ScrollRect) return;

            var viewSize = ScrollRect.viewport.rect.size;
            var rowWidth = orientaion switch {
                Orientaion.Vertical => viewSize.x,
                Orientaion.Horizontal => viewSize.y,
                _ => 0f
            };
            var viewLower = orientaion switch {
                Orientaion.Vertical => viewSize.y,
                Orientaion.Horizontal => viewSize.x,
                _ => 0f
            };

            PreUpdate();

            var totalCount = DataSource != null ? DataSource.TotalCount : 0;

            float contentSize = 0;
            int startIndex = 0;
            int endIndex = -1;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            LookAheadedCellSizes.Clear();
            for(int i = 0; i < totalCount;) {
                int addCount = 0;
                float rowHeight = LookAheadedCellSizes.Count > 0 ? LookAheadedCellSizes[0].rect.height : 0;
                float columnOffset = LookAheadedCellSizes.Count > 0 ? LookAheadedCellSizes[0].rect.width : 0;
                for(int j = i; j < totalCount && columnOffset <= rowWidth; ++j) {
                    Rect rect = Rect.zero;
                    var cellSize = DataSource.CellSize(j);
                    switch(orientaion) {
                    case Orientaion.Vertical:
                        rect = new Rect(columnOffset, contentSize, cellSize.x, cellSize.y);
                        columnOffset += cellSize.x;
                        if(columnOffset > rowWidth) rect.position = new Vector2(0, contentSize + rowHeight);
                        else if(rowHeight < cellSize.y) rowHeight = cellSize.y;
                        break;
                    case Orientaion.Horizontal:
                        rect = new Rect(contentSize, columnOffset, cellSize.y, cellSize.x);
                        columnOffset += cellSize.y;
                        if(columnOffset > rowWidth) rect.position = new Vector2(contentSize + rowHeight, 0);
                        else if(rowHeight < cellSize.x) rowHeight = cellSize.x;
                        break;
                    }
                    LookAheadedCellSizes.Add(new LookAheadedCellSize { index = j, rect = rect });
                    addCount++;
                }

                for(int j = 0; j < LookAheadedCellSizes.Count; ++j) {
                    if(LookAheadedCellSizes.Count > 1 && j == LookAheadedCellSizes.Count - 1) {
                        i += addCount;
                        if(i < totalCount) {
                            LookAheadedCellSizes.RemoveRange(0, LookAheadedCellSizes.Count - 1);
                            break;
                        }
                        else {
                            contentSize += rowHeight;
                            rowHeight = LookAheadedCellSizes.Last().rect.height;
                        }
                    }
                    float cellUpper = 0;
                    int index = LookAheadedCellSizes[j].index;
                    var rect = LookAheadedCellSizes[j].rect;
                    switch(orientaion) {
                    case Orientaion.Vertical:
                        cellUpper = contentSize - contentRectLocalPosition.y;
                        rect.height = rowHeight;
                        break;
                    case Orientaion.Horizontal:
                        cellUpper = contentSize + contentRectLocalPosition.x;
                        rect.width = rowHeight;
                        break;
                    }

                    if(startIndex > endIndex) {
                        if(cellUpper >= -rowHeight && cellUpper <= viewLower) {
                            startIndex = endIndex = index;
                            cellRects[0] = rect;
                        }
                    }
                    else {
                        if(cellUpper >= -rowHeight && cellUpper <= viewLower) {
                            endIndex = index;
                            if(index - startIndex < cellRects.Length) cellRects[index - startIndex] = rect;
                        }
                    }

                    if(LookAheadedCellSizes.Count == 1) {
                        LookAheadedCellSizes.Clear();
                        i += addCount;
                    }
                }
                contentSize += rowHeight;
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
                    if(cell && !(i - startIndex < 0 || i - startIndex >= cellRects.Length)) {
                        var rectTrans = cell.GetComponent<RectTransform>();
                        var localPosition = rectTrans.localPosition;
                        var size = rectTrans.sizeDelta;
                        var cellRect = cellRects[i - startIndex];
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            size.y = cellRect.height;
                            localPosition.y = -cellRect.y - size.y * rectTrans.pivot.y;
                            size.x = cellRect.width;
                            localPosition.x = cellRect.x + size.x * rectTrans.pivot.x;
                            break;
                        case Orientaion.Horizontal:
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
