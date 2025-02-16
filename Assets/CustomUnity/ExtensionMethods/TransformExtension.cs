using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CustomUnity
{
    public static class TransformExtension
    {
        static System.Text.StringBuilder _work = null;

        public static string GetPath(this Transform transform, Transform root = null)
        {
            _work ??= new System.Text.StringBuilder(256);
            _work.Length = 0;
            for(var i = transform; i != null && i != root; i = i.parent) {
                if(i != transform) _work.Insert(0, "/");
                _work.Insert(0, i.GetName());
            }
            return _work.ToString();
        }

        public static Transform FindRecursive(this Transform transform, string name)
        {
            if(transform.GetName().Equals(name)) return transform;
            for(var i = 0; i < transform.childCount; i++) {
                var ret = transform.GetChild(i).FindRecursive(name);
                if(ret) return ret;
            }
            return null;
        }

        public static IEnumerable<Transform> FindMultipleRecursive(this Transform transform, string name)
        {
            if(transform.GetName().Equals(name)) yield return transform;
            for(var i = 0; i < transform.childCount; i++) {
                foreach(var j in transform.GetChild(i).FindMultipleRecursive(name)) yield return j;
            }
        }

        public static IEnumerable<Transform> EnumChildrenRecursive(this Transform transform)
        {
            for(var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                yield return child;
                foreach(var j in child.EnumChildrenRecursive()) yield return j;
            }
        }

        public static IEnumerable<Transform> EnumChildren(this Transform transform)
        {
            for(var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i);
                yield return child;
            }
        }

        public static GameObject[] GetChildGameObjects(this Transform transform)
        {
            if(transform.childCount > 0) {
                var ret = new GameObject[transform.childCount];
                for(var i = 0; i < transform.childCount; i++) {
                    ret[i] = transform.GetChild(i).gameObject;
                }
                return ret;
            }
            return null;
        }

        public static Vector3 GetDragAmount(this Transform transform, PointerEventData eventData)
        {
            var z = eventData.pressEventCamera.WorldToScreenPoint(transform.position).z;
            var prev = eventData.pressEventCamera.ScreenToWorldPoint((eventData.position - eventData.delta).ToVector3(z));
            var now = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position.ToVector3(z));
            return now - prev;
        }
        
        /// <summary>
        /// マウスのドラッグイベントにしたがって移動する
        /// </summary>
        /// <param name="transform">Transform.</param>
        /// <param name="eventData">Event data.</param>
        public static void ApplyDrag(this Transform transform, PointerEventData eventData)
        {
            transform.position += transform.GetDragAmount(eventData);
        }
    }
}