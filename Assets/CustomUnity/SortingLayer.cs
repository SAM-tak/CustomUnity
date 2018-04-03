/// Set Sorting Layer
/// Copyright (c) 2014 Tatsuhiko Yamamura
/// Released under the MIT license
// / http://opensource.org/licenses/mit-license.php

using UnityEngine;

namespace CustomUnity
{
    [ExecuteInEditMode, RequireComponent(typeof(Renderer)), AddComponentMenu("CustomUnity/SortingLayer")]
    public class SortingLayer : MonoBehaviour
    {
        [SerializeField, SortingLayer]
        string layerName = "Default";
        [SerializeField]
        int orderInLayer = 0;

        void Awake()
        {
            LayerName = layerName;
            OrderInLayer = orderInLayer;
        }

        void OnValidate()
        {
            LayerName = layerName;
            OrderInLayer = orderInLayer;
        }
        
        public string LayerName {
            get {
                return layerName;
            }
            set {
                layerName = value;
                foreach(var renderer in GetComponents<Renderer>()) {
                    renderer.sortingLayerName = layerName;
                }
            }
        }

        public int OrderInLayer {
            get {
                return orderInLayer;
            }
            set {
                orderInLayer = value;
                foreach(var renderer in GetComponents<Renderer>()) {
                    renderer.sortingOrder = orderInLayer;
                }
            }
        }
    }

    public class SortingLayerAttribute : PropertyAttribute
    {
    }
}