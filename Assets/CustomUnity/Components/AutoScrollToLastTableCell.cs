using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    [RequireComponent(typeof(ScrollRect))]
    public class AutoScrollToLastTableCell : ScrollToItemBase
    {
        TableContentBase tableContent;

        int lastTotalCount;
        float lastScrollAmount;
        int frameCountAfterChange;
        int frameCountManualScroll;
        const int maxFrameCountManualScroll = 2;
        bool IsAutoScrolling => frameCountManualScroll < maxFrameCountManualScroll;

        protected override void Awake()
        {
            base.Awake();
            tableContent = GetComponentInChildren<TableContentBase>();
        }

        protected override void ScrollToTarget()
        {
            var totalCount = tableContent.DataSourceTotalCount;
            // maxFrameCountManualScroll間、身に覚えのない座標変更があったのなら、UIによる干渉だと判断して自動スクロールをやめる
            if(totalCount > 0 && lastTotalCount == totalCount && MayMoveByOther) {
                if(frameCountManualScroll < maxFrameCountManualScroll) ++frameCountManualScroll;
            }
            else if(totalCount == 0 || targetItem) {
                // 前回フレームの時点で最終セルが画面外なら自動スクロールを再開させないよう、targetItemが非nullかチェックする
                frameCountManualScroll = 0;
            }

            //if(totalCount - lastTotalCount >= 100) {
            //    LogDataSource.StopLogging();
            //    LogInfo($"frameCountManualScroll = {frameCountManualScroll} targetItem = {(targetItem ? true : false)}");
            //    LogDataSource.StartLogging();
            //}

            targetItem = totalCount > 0 ? tableContent.GetActiveCell(totalCount - 1) : null;

            if(IsAutoScrolling) {
                if(targetItem) base.ScrollToTarget();
                else {
                    //LogDataSource.StopLogging();
                    //Log.Info($"frameCountManualScroll = {frameCountManualScroll}");
                    //LogDataSource.StartLogging();
                    if(lastTotalCount != totalCount) frameCountAfterChange = 0;
                    if(frameCountAfterChange % 5 == 0) lastScrollAmount = tableContent.GetScrollAmountForBottomOfLastItem();
                    if(frameCountAfterChange < int.MaxValue) ++frameCountAfterChange;
                    prevScrollPosition = ScrollRect.content.localPosition = Math.RubberStep(
                        ScrollRect.content.localPosition,
                        tableContent.GetPositionFromScrollAmount(lastScrollAmount),
                        halfLife,
                        DeltaTime
                    );
                }
            }
            lastTotalCount = totalCount;
        }
    }
}
