using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(RectTransform))]
    public class TableContent : TableContentBase
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            void SetUpCell(int index, GameObject cell);
        }

        public IDataSource DataSource { get; set; }

        public Vector2 cellSize;

        public int columnCount = 1;

        public bool repeat;
        
        public Vector2 GetContentSize(IDataSource dataSource)
        {
            var n = dataSource.TotalCount;
            switch(orientaion) {
            default:
            case Orientaion.Vertical:
                return new Vector2(n < columnCount ? n * cellSize.x : cellSize.x * columnCount, n * cellSize.y / columnCount);
            case Orientaion.Horizontal:
                return new Vector2(n * cellSize.x / columnCount, n < columnCount ? n * cellSize.y : cellSize.y * columnCount);
            }
        }

        const int merginScaler = 2;
        const int minimumMergin = 400;

        void OnValidate()
        {
            if(columnCount < 1) columnCount = 1;
        }

        protected override void Start()
        {
            base.Start();
            if(repeat) {
                var contentRectLocalPosition = contentRectTransform.localPosition;
                var viewSize = ScrollRect.viewport.rect.size;
                switch(orientaion) {
                case Orientaion.Vertical:
                    var contentMargin = Mathf.Max(minimumMergin, viewSize.y * merginScaler);
                    if(contentRectLocalPosition.y < contentMargin) {
                        contentRectLocalPosition.y = contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                    break;
                case Orientaion.Horizontal:
                    contentMargin = Mathf.Max(minimumMergin, viewSize.x * merginScaler);
                    if(contentRectLocalPosition.x < contentMargin) {
                        contentRectLocalPosition.x = contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                    break;
                }
            }
        }

        protected override void UpdateContent()
        {
            if(!ScrollRect) return;
            
            var totalCount = (DataSource != null ? DataSource.TotalCount : 0);

            int startIndex = 0;
            int endIndex = 0;
            int leftRadix = 0;
            int rightRadix = columnCount;
            var viewSize = ScrollRect.viewport.rect.size;
            var contentMargin = 0f;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            var sizeDelta = contentRectTransform.sizeDelta;
            switch(orientaion) {
            case Orientaion.Vertical:
                var contentSize = Math.CeilDiv(totalCount, columnCount) * cellSize.y;
                if(repeat) {
                    contentMargin = Mathf.Max(minimumMergin, viewSize.y * merginScaler);
                    if(contentRectLocalPosition.y < contentMargin / 2 || contentRectLocalPosition.y + viewSize.y > (contentMargin + contentSize + contentMargin / 2)) {
                        contentRectLocalPosition.y = Math.Wrap(contentRectLocalPosition.y - contentMargin, contentSize) + contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                startIndex = Mathf.FloorToInt((contentRectLocalPosition.y - contentMargin) / cellSize.y) * columnCount;
                endIndex = Mathf.FloorToInt((contentRectLocalPosition.y - contentMargin + viewSize.y) / cellSize.y) * columnCount + (columnCount - 1);
                if(columnCount > 1 && cellSize.x > 0) {
                    leftRadix = Mathf.FloorToInt(-contentRectLocalPosition.x / cellSize.x);
                    rightRadix = Mathf.FloorToInt((viewSize.x - contentRectLocalPosition.x) / cellSize.x);
                }
                sizeDelta.x = cellSize.x * columnCount;
                sizeDelta.y = contentSize + contentMargin * 2;
                break;
            case Orientaion.Horizontal:
                contentSize = Math.CeilDiv(totalCount, columnCount) * cellSize.x;
                if(repeat) {
                    contentMargin = Mathf.Max(minimumMergin, viewSize.x * merginScaler);
                    if(contentRectLocalPosition.x > -contentMargin / 2 || contentRectLocalPosition.x - viewSize.x < -(contentMargin + contentSize + contentMargin / 2)) {
                        contentRectLocalPosition.x = -Math.Wrap(-contentRectLocalPosition.x - contentMargin, contentSize) - contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                startIndex = Mathf.FloorToInt((-contentRectLocalPosition.x + contentMargin) / cellSize.x) * columnCount;
                endIndex = Mathf.FloorToInt((-contentRectLocalPosition.x + contentMargin + viewSize.x) / cellSize.x) * columnCount + (columnCount - 1);
                if(columnCount > 1 && cellSize.y > 0) {
                    leftRadix = Mathf.FloorToInt(contentRectLocalPosition.y / cellSize.y);
                    rightRadix = Mathf.FloorToInt((contentRectLocalPosition.y + viewSize.y) / cellSize.y);
                }
                sizeDelta.x = contentSize + contentMargin * 2;
                sizeDelta.y = cellSize.y * columnCount;
                break;
            }
            contentRectTransform.sizeDelta = sizeDelta;
            if(!repeat) {
                if(startIndex < 0) startIndex = 0;
                if(endIndex >= totalCount) endIndex = totalCount - 1;
            }
            
            int ceillingTotalCount = Math.CeilDiv(totalCount, columnCount) * columnCount;
            foreach(var i in cellPool) {
                if(i.cell.activeSelf && (
                    totalCount == 0 || i.index < startIndex || i.index > endIndex
                    || Math.Wrap(i.index, columnCount) < leftRadix
                    || Math.Wrap(i.index, columnCount) > rightRadix
                    || Math.Wrap(i.index, ceillingTotalCount) > totalCount
                    || (repeat && totalCount < columnCount * 3 && IsCulled(i.cell))
                )) i.cell.SetActive(false);
            }

            if(totalCount > 0 && endIndex - startIndex + 1 > 0) {
                if(endIndex - startIndex + 1 > MaxCellsRequired) MaxCellsRequired = endIndex - startIndex + 1;
                for(int i = startIndex; i <= endIndex; ++i) {
                    int wrappedIndex = Math.Wrap(i, ceillingTotalCount);

                    if(wrappedIndex >= totalCount || Math.Wrap(i, columnCount) < leftRadix || Math.Wrap(i, columnCount) > rightRadix) continue;

                    var newCell = NewCell(i);
                    if(newCell) {
                        var rectTrans = newCell.GetComponent<RectTransform>();
                        var localPosition = rectTrans.localPosition;
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            localPosition.y = -contentMargin - Math.FloorDiv(i, columnCount) * cellSize.y - cellSize.y * rectTrans.pivot.y;
                            if(columnCount > 1) {
                                localPosition.x = Math.Wrap(i, columnCount) * cellSize.x + cellSize.x * rectTrans.pivot.x;
                            }
                            break;
                        case Orientaion.Horizontal:
                            localPosition.x = -contentMargin + Math.FloorDiv(i, columnCount) * cellSize.x + cellSize.x * rectTrans.pivot.x;
                            if(columnCount > 1) {
                                localPosition.y = -Math.Wrap(i, columnCount) * cellSize.y - cellSize.y * rectTrans.pivot.y;
                            }
                            break;
                        }
                        rectTrans.sizeDelta = cellSize;
                        rectTrans.localPosition = localPosition;
                        DataSource.SetUpCell(wrappedIndex, newCell);
                        newCell.SetActive(true);
                    }
                }
            }
        }
    }
}
