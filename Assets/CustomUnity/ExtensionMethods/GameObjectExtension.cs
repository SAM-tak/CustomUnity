using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomUnity
{
    public static class GameObjectExtension
    {
        public static void UniqueName(this GameObject go, string basename)
        {
            int index = 0;
            var ret = basename;
            foreach(var i in go.SiblingGameObjects().OrderBy(x => x.name)) {
                if(i.name == ret) ret = string.Format("{0} ({1})", basename, ++index);
            }
            go.name = ret;
        }

        public static IEnumerable<GameObject> SiblingGameObjects(this GameObject go)
        {
            return (go.transform.parent ? go.transform.parent.GetChildGameObjects() : go.scene.GetRootGameObjects()).Where(x => x != go);
        }
    }
}