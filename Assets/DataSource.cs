using UnityEngine;
using UnityEngine.UI;
using CustomUnity;

namespace YourProjectNamespace
{
    [RequireComponent(typeof(ContentsFiller))]
    public class DataSource : MonoBehaviour, ContentsFiller.IDataSource
    {
        [System.Serializable]
        public struct CellData
        {
            public Color color;
            public string buttonTitle;
            public int height;
        }

        public CellData[] dataSource;

        public int TotalCount {
            get {
                return dataSource.Length;
            }
        }

        public float CellSize(int index, ContentsFiller contentsFiller)
        {
            return dataSource[index].height;
        }

        public void SetUpCell(int index, ContentsFiller contentsFiller, GameObject cell)
        {
            cell.transform.Find("Image").GetComponent<Image>().color = dataSource[index].color;
            cell.transform.Find("Button/Text").GetComponent<Text>().text = dataSource[index].buttonTitle;
            var rectTransform = cell.GetComponent<RectTransform>();
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = dataSource[index].height;
            rectTransform.sizeDelta = sizeDelta;
        }

        void Awake()
        {
            GetComponent<ContentsFiller>().dataSource = this;
        }
    }
}
