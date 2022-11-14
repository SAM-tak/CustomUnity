using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    /// <summary>
    /// Fixed cell size table view.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TableContent : TableContentBase
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            void SetUpCell(int index, GameObject cell);
            void OnPreUpdate();
        }

        public IDataSource DataSource { get; private set; }

        public int columnCount = 1;

        public bool repeat;

        public Vector2 CellSize { get; protected set; }

        public override float GetScrollAmountForBottomOfLastItem()
        {
            var n = DataSource.TotalCount;
            return orientaion switch {
                Orientaion.Horizontal => Math.CeilDiv(n, columnCount) * CellSize.x,
                _ => Math.CeilDiv(n, columnCount) * CellSize.y,
            };
        }

        public override int DataSourceTotalCount => DataSource.TotalCount;

        protected override void PreUpdate() => DataSource?.OnPreUpdate();

        const int merginScaler = 2;
        const int minimumMergin = 400;

        void OnValidate()
        {
            if(columnCount < 1) columnCount = 1;
        }

        protected override void Awake()
        {
            base.Awake();
            DataSource = GetComponent<IDataSource>();
            var firstChild = transform.GetChild(0);
            if(firstChild) {
                CellSize = columnCount == 1
                    ? orientaion switch {
                        Orientaion.Horizontal => new Vector2(firstChild.GetComponent<RectTransform>().rect.width, 0),
                        Orientaion.Vertical => new Vector2(0, firstChild.GetComponent<RectTransform>().rect.height),
                        _ => CellSize
                    }
                    : firstChild.GetComponent<RectTransform>().rect.size;
            }
        }

        protected override void Start()
        {
            base.Start();
            if(repeat) SetPositionToFirst();
        }

        void SetPositionToFirst()
        {
            var contentRectLocalPosition = contentRectTransform.localPosition;
            var viewSize = ScrollRect.viewport.rect.size;
            switch(orientaion) {
            case Orientaion.Vertical:
                {
                    var contentMargin = Mathf.Max(minimumMergin, viewSize.y * merginScaler);
                    if(contentRectLocalPosition.y < contentMargin) {
                        contentRectLocalPosition.y = contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                break;
            case Orientaion.Horizontal:
                {
                    var contentMargin = Mathf.Max(minimumMergin, viewSize.x * merginScaler);
                    if(contentRectLocalPosition.x < contentMargin) {
                        contentRectLocalPosition.x = contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                break;
            }
        }

        protected override void UpdateContent()
        {
            if(!ScrollRect) return;

            if(!NeedsUpdateContent && FrameCount < 2 && repeat) SetPositionToFirst();

            var totalCount = DataSource?.TotalCount ?? 0;

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
                var contentSize = Math.CeilDiv(totalCount, columnCount) * CellSize.y;
                if(repeat) {
                    contentMargin = Mathf.Max(minimumMergin, viewSize.y * merginScaler);
                    if(contentRectLocalPosition.y < contentMargin / 2 || contentRectLocalPosition.y + viewSize.y > (contentMargin + contentSize + contentMargin / 2)) {
                        contentRectLocalPosition.y = Math.Wrap(contentRectLocalPosition.y - contentMargin, contentSize) + contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                startIndex = Mathf.FloorToInt((contentRectLocalPosition.y - contentMargin) / CellSize.y) * columnCount;
                endIndex = Mathf.FloorToInt((contentRectLocalPosition.y - contentMargin + viewSize.y) / CellSize.y) * columnCount + (columnCount - 1);
                if(columnCount > 1 && CellSize.x > 0) {
                    leftRadix = Mathf.FloorToInt(-contentRectLocalPosition.x / CellSize.x);
                    rightRadix = Mathf.FloorToInt((viewSize.x - contentRectLocalPosition.x) / CellSize.x);
                }
                sizeDelta.x = CellSize.x * columnCount;
                sizeDelta.y = contentSize + contentMargin * 2;
                break;
            case Orientaion.Horizontal:
                contentSize = Math.CeilDiv(totalCount, columnCount) * CellSize.x;
                if(repeat) {
                    contentMargin = Mathf.Max(minimumMergin, viewSize.x * merginScaler);
                    if(contentRectLocalPosition.x > -contentMargin / 2 || contentRectLocalPosition.x - viewSize.x < -(contentMargin + contentSize + contentMargin / 2)) {
                        contentRectLocalPosition.x = -Math.Wrap(-contentRectLocalPosition.x - contentMargin, contentSize) - contentMargin;
                        contentRectTransform.localPosition = contentRectLocalPosition;
                    }
                }
                startIndex = Mathf.FloorToInt((-contentRectLocalPosition.x + contentMargin) / CellSize.x) * columnCount;
                endIndex = Mathf.FloorToInt((-contentRectLocalPosition.x + contentMargin + viewSize.x) / CellSize.x) * columnCount + (columnCount - 1);
                if(columnCount > 1 && CellSize.y > 0) {
                    leftRadix = Mathf.FloorToInt(contentRectLocalPosition.y / CellSize.y);
                    rightRadix = Mathf.FloorToInt((contentRectLocalPosition.y + viewSize.y) / CellSize.y);
                }
                sizeDelta.x = contentSize + contentMargin * 2;
                sizeDelta.y = CellSize.y * columnCount;
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

                    var cell = GetCell(i, out var @new);
                    if(cell) {
                        var rectTrans = cell.GetComponent<RectTransform>();
                        var localPosition = rectTrans.localPosition;
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            localPosition.y = -contentMargin - Math.FloorDiv(i, columnCount) * CellSize.y - CellSize.y * rectTrans.pivot.y;
                            if(columnCount > 1) {
                                localPosition.x = Math.Wrap(i, columnCount) * CellSize.x + CellSize.x * rectTrans.pivot.x;
                            }
                            break;
                        case Orientaion.Horizontal:
                            localPosition.x = -contentMargin + Math.FloorDiv(i, columnCount) * CellSize.x + CellSize.x * rectTrans.pivot.x;
                            if(columnCount > 1) {
                                localPosition.y = -Math.Wrap(i, columnCount) * CellSize.y - CellSize.y * rectTrans.pivot.y;
                            }
                            break;
                        }
                        rectTrans.sizeDelta = CellSize;
                        rectTrans.localPosition = localPosition;
                        if(@new) {
                            DataSource.SetUpCell(wrappedIndex, cell);
                            cell.SetActive(true);
                        }
                    }
                }
            }
        }
    }
}
