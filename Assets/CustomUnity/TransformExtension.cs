using UnityEngine;

namespace UnityFighters
{
	public static class TransformExtension
	{
        static System.Text.StringBuilder work = null;

        public static string GetPath(this Transform transform, Transform root = null)
        {
            if(work == null) work = new System.Text.StringBuilder(256);
            work.Length = 0;
            for(var i = transform; i != null && i != root; i = i.parent) {
                if(i != transform) work.Insert(0, "/");
                work.Insert(0, i.GetName());
            }
            return work.ToString();
        }
        
        public static Transform FindRecursive(this Transform transform, string name)
		{
            if(transform.GetName().Equals(name)) {
                return transform;
            }
            if(transform.childCount > 0) {
                for(var i = 0; i < transform.childCount; i++) {
                    var ret = transform.GetChild(i).FindRecursive(name);
                    if(ret) return ret;
                }
            }
            return null;
        }
    }
}