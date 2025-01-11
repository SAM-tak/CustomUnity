using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    /// <summary>
    /// Track last item of table view.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class AutoScrollToLastTableCell : ScrollToItemBase
    {
        TableContentBase _tableContent;

        int _lastTotalCount;
        float _lastScrollAmount;
        int _frameCountAfterChange;
        int _frameCountManualScroll;
        const int MaxFrameCountManualScroll = 2;
        bool IsAutoScrolling => _frameCountManualScroll < MaxFrameCountManualScroll;

        protected override void Awake()
        {
            base.Awake();
            _tableContent = GetComponentInChildren<TableContentBase>();
        }

        protected override void ScrollToTarget()
        {
            var totalCount = _tableContent.DataSourceTotalCount;
            // maxFrameCountManualScroll間、身に覚えのない座標変更があったのなら、UIによる干渉だと判断して自動スクロールをやめる
            if(totalCount > 0 && _lastTotalCount == totalCount && MayMoveByOther) {
                if(_frameCountManualScroll < MaxFrameCountManualScroll) ++_frameCountManualScroll;
            }
            else if(totalCount == 0 || targetItem) {
                // 前回フレームの時点で最終セルが画面外なら自動スクロールを再開させないよう、targetItemが非nullかチェックする
                _frameCountManualScroll = 0;
            }

            //if(totalCount - lastTotalCount >= 100) {
            //    LogDataSource.StopLogging();
            //    LogInfo($"frameCountManualScroll = {frameCountManualScroll} targetItem = {(targetItem ? true : false)}");
            //    LogDataSource.StartLogging();
            //}

            targetItem = totalCount > 0 ? _tableContent.GetActiveCell(totalCount - 1) : null;

            if(IsAutoScrolling) {
                if(targetItem) base.ScrollToTarget();
                else {
                    //LogDataSource.StopLogging();
                    //Log.Info($"frameCountManualScroll = {frameCountManualScroll}");
                    //LogDataSource.StartLogging();
                    if(_lastTotalCount != totalCount) _frameCountAfterChange = 0;
                    if(_frameCountAfterChange % 5 == 0) _lastScrollAmount = _tableContent.GetScrollAmountForBottomOfLastItem();
                    if(_frameCountAfterChange < int.MaxValue) ++_frameCountAfterChange;
                    prevScrollPosition = ScrollRect.content.localPosition = Math.RubberStep(
                        ScrollRect.content.localPosition,
                        _tableContent.GetPositionFromScrollAmount(_lastScrollAmount),
                        halfLife,
                        DeltaTime
                    );
                }
            }
            _lastTotalCount = totalCount;
        }
    }
}
