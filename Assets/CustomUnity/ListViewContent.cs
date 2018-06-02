﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    public enum Orientaion
    {
        Vertical,
        Horizontal
    }

    [RequireComponent(typeof(RectTransform))]
    public class ListViewContent : MonoBehaviour
    {
        public interface IDataSource
        {
            int TotalCount { get; }
            Vector2 CellSize(int index);
            void SetUpCell(int index, GameObject cell);
            void UpdateCell(int index, GameObject cell);
        }

        public Orientaion orientaion;
        
        public IDataSource DataSource { get; set; }
        
        public Action OnPreUpdate { get; set; }

        public ScrollRect ScrollRect { get; protected set; }

        public int MaxCells {
            get {
                return cellPool != null ? cellPool.Length : 0;
            }
        }

        public int MaxCellsRequired { get; protected set; }

        RectTransform contentRectTransform;
        RectTransform scrollRectTransform;
        
        struct Cell
        {
            public GameObject cell;
            public int index;
        }

        Cell[] cellPool;
        Vector2[] cellPositions;
        
        void Start()
        {
            ScrollRect = GetComponentInParent<ScrollRect>();
            Debug.Assert(ScrollRect);
            contentRectTransform = GetComponent<RectTransform>();
            scrollRectTransform = GetComponentInParent<ScrollRect>().GetComponent<RectTransform>();
            cellPool = new Cell[transform.childCount];
            cellPositions = new Vector2[transform.childCount];
            for(int i = 0; i < transform.childCount; i++) {
                var go = transform.GetChild(i).gameObject;
                go.SetActive(false);
                cellPool[i].cell = go;
                cellPool[i].index = -1;
            }
        }

        void Update()
        {
            if(!ScrollRect) return;

            float contentSize = 0;
            int startIndex = -1;
            int endIndex = -1;
            var viewSize = scrollRectTransform.sizeDelta;
            float viewLower = 0;
            var contentRectLocalPosition = contentRectTransform.localPosition;
            switch(orientaion) {
            case Orientaion.Vertical:
                viewLower = viewSize.y;
                break;
            case Orientaion.Horizontal:
                viewLower = viewSize.x;
                break;
            }

            OnPreUpdate?.Invoke();

            var totalCount = (DataSource != null ? DataSource.TotalCount : 0);

            for(int i = 0; i < totalCount; ++i) {
                float size = 0;
                float cellUpper = 0;
                Vector2 position = Vector2.zero;
                switch(orientaion) {
                case Orientaion.Vertical:
                    cellUpper = contentSize - contentRectLocalPosition.y;
                    size = DataSource.CellSize(i).y;
                    position = new Vector2(0, contentSize);
                    break;
                case Orientaion.Horizontal:
                    cellUpper = contentSize + contentRectLocalPosition.x;
                    size = DataSource.CellSize(i).x;
                    position = new Vector2(contentSize, 0);
                    break;
                }
                if(startIndex < 0) {
                    if(cellUpper >= -size && cellUpper <= viewLower) {
                        startIndex = endIndex = i;
                        cellPositions[0] = position;
                    }
                }
                else {
                    if(cellUpper >= -size && cellUpper <= viewLower) {
                        endIndex = i;
                        if(i - startIndex < cellPositions.Length) cellPositions[i - startIndex] = position;
                    }
                }
                contentSize += size;
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

            foreach(var i in cellPool) {
                if(i.cell.activeSelf && (i.index < startIndex || i.index > endIndex)) i.cell.SetActive(false);
            }

            if(endIndex - startIndex + 1 > 0) {
                if(endIndex - startIndex + 1 > MaxCellsRequired) MaxCellsRequired = endIndex - startIndex + 1;
                for(int i = startIndex; i <= endIndex; ++i) {
                    int wrapedIndex = Math.Wrap(i, totalCount);
                    bool found = false;
                    int firstinactive = -1;
                    for(int j = 0; j < cellPool.Length; ++j) {
                        if(cellPool[j].cell.activeSelf) {
                            if(cellPool[j].index == i) {
                                found = true;
                                DataSource.UpdateCell(wrapedIndex, cellPool[j].cell);
                                break;
                            }
                        }
                        else if(firstinactive < 0) firstinactive = j;
                    }
                    if(!found && firstinactive >= 0) {
                        var x = cellPool[firstinactive];
                        var rectTrans = x.cell.GetComponent<RectTransform>();
                        var localPosition = rectTrans.localPosition;
                        var size = rectTrans.sizeDelta;
                        var cellSize = DataSource.CellSize(wrapedIndex);
                        switch(orientaion) {
                        case Orientaion.Vertical:
                            size.y = cellSize.y;
                            localPosition.y = -cellPositions[i - startIndex].y - size.y * rectTrans.pivot.y;
                            break;
                        case Orientaion.Horizontal:
                            size.x = cellSize.x;
                            localPosition.x =  cellPositions[i - startIndex].x + size.x * rectTrans.pivot.x;
                            break;
                        }
                        rectTrans.sizeDelta = size;
                        rectTrans.localPosition = localPosition;
                        DataSource.SetUpCell(wrapedIndex, x.cell);
                        x.cell.SetActive(true);
                        x.index = i;
                        cellPool[firstinactive] = x;
                    }
                }
            }
        }
    }
}
