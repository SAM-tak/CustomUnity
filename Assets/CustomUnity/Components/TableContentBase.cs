using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUnity
{
    public enum Orientaion
    {
        Vertical,
        Horizontal
    }

    /// <summary>
    /// Base class of TabelContent / JuggedTabelContent / MultiColumnJuggedTabelContent
    /// </summary>
    public abstract class TableContentBase : MonoBehaviour
    {
        [ReadOnlyWhenPlaying]
        public Orientaion orientaion;

        [Tooltip("Number of cells to be active even outside the viewport for navigation.")]
        public int extraCells;

        public int MaxCells => cellPool != null ? cellPool.Length : 0;

        public int MaxCellsRequired { get; protected set; }

        protected RectTransform contentRectTransform;

        protected abstract void PreUpdate();

        protected struct Cell : IEquatable<Cell>
        {
            public GameObject cell;
            public int index;

            public override bool Equals(object obj) => obj is Cell cell && Equals(cell);

            public bool Equals(Cell other) => EqualityComparer<GameObject>.Default.Equals(cell, other.cell) && index == other.index;

            public override int GetHashCode() => HashCode.Combine(cell, index);
        }

        protected Cell[] cellPool;

        public GameObject GetCell(int index, out bool @new)
        {
            @new = false;
            int candidate = -1;
            for(int j = 0; j < cellPool.Length; ++j) {
                if(cellPool[j].cell.activeSelf) {
                    if(cellPool[j].index == index) return cellPool[j].cell;
                }
                else if(candidate < 0) candidate = j;
            }
            if(candidate >= 0) {
                @new = true;
                cellPool[candidate].index = index;
                return cellPool[candidate].cell;
            }
            return null;
        }

        public GameObject GetActiveCell(int index)
        {
            for(int j = 0; j < cellPool.Length; ++j) {
                if(cellPool[j].cell.activeSelf) {
                    if(cellPool[j].index == index) return cellPool[j].cell;
                }
            }
            return null;
        }

        public bool IsCulled(GameObject cell)
        {
            var cellRectTransform = cell.GetComponent<RectTransform>();
            var rect = cellRectTransform.rect;
            rect.position = ScrollRect.viewport.InverseTransformPoint(cellRectTransform.TransformPoint(rect.position));
            rect.size = cellRectTransform.InverseTransformVector(cellRectTransform.TransformVector(rect.size));
            return !ScrollRect.viewport.rect.Overlaps(rect);
        }

        public Vector3 GetPositionFromScrollAmount(float scrollAmount) => orientaion switch {
            Orientaion.Horizontal => new Vector3(Mathf.Max(0f, scrollAmount - ScrollRect.viewport.rect.width), 0f, 0f),
            _ => new Vector3(0f, Mathf.Max(0f, scrollAmount - ScrollRect.viewport.rect.height), 0f),
        };

        public abstract float GetScrollAmountForBottomOfLastItem();

        public abstract int DataSourceTotalCount { get; }

        public bool NeedsUpdateContent { get; private set; } = true;

        /// <summary>
        /// To use for forcing to reset cells up.
        /// </summary>
        public void Refresh()
        {
            foreach(var i in cellPool) i.cell.SetActive(false);
            NeedsUpdateContent = true;
        }

        /// <summary>
        /// Set NeedsUpdateContent is true
        /// </summary>
        public void NeedsRelayout()
        {
            NeedsUpdateContent = true;
        }

        public ScrollRect ScrollRect { get; protected set; }

        public Canvas Canvas { get; protected set; }
        bool OriginalCanvasOverridePixelPerfect;
        bool OriginalCanvasPixelPerfect;
        Vector2 previousScrollRectPosition;

        protected virtual void Awake()
        {
            Canvas = GetComponentInParent<Canvas>();
            OriginalCanvasOverridePixelPerfect = Canvas.overridePixelPerfect;
            OriginalCanvasPixelPerfect = Canvas.pixelPerfect;
            ScrollRect = GetComponentInParent<ScrollRect>();
        }

        protected virtual void Start()
        {
            Debug.Assert(ScrollRect);
            var layoutGroup = GetComponentInParent<LayoutGroup>();
            if(layoutGroup != null && layoutGroup.enabled) {
                LogWarning($"TableContentBase : {layoutGroup.GetType().Name} component will corrupt table view or cause of glitch. Please disable it before save a prefab/scene or before play.");
            }
            contentRectTransform = GetComponent<RectTransform>();
            cellPool = new Cell[transform.childCount];
            for(int i = 0; i < transform.childCount; i++) {
                var go = transform.GetChild(i).gameObject;
                go.SetActive(false);
                cellPool[i].cell = go;
            }
            previousScrollRectPosition = ScrollRect.normalizedPosition;
            ScrollRect.onValueChanged.AddListener(position => {
                if(isUpdatingContent) NeedsUpdateContent = true;
                else {
                    isUpdatingContent = true;
                    UpdateContent();
                    isUpdatingContent = false;
                }

                if((ScrollRect.content.rect.size * previousScrollRectPosition - ScrollRect.content.rect.size * position).magnitude > 0.01f) {
                    Canvas.overridePixelPerfect = true;
                    Canvas.pixelPerfect = false;
                }
                else {
                    ScrollRect.StopMovement();
                    Canvas.overridePixelPerfect = OriginalCanvasOverridePixelPerfect;
                    Canvas.pixelPerfect = OriginalCanvasPixelPerfect;
                }
                previousScrollRectPosition = position;
            });
            FrameCount = 0;
        }

        // workaround for 2019.1 or higher
        protected int FrameCount { get; private set; } = 0;

        bool isUpdatingContent;

        protected virtual void Update()
        {
            PreUpdate();
            if(NeedsUpdateContent || FrameCount < 2) {
                // needs set NeedsUpdateContent to false before UpdateContent.
                // If not, cannot specify needs UpdateContent on next frame in UpdateContent
                NeedsUpdateContent = false;
                isUpdatingContent = true;
                UpdateContent();
                isUpdatingContent = false;
            }
            if(FrameCount < int.MaxValue) ++FrameCount;
        }

        protected abstract void UpdateContent();
    }
}
