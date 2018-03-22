using UnityEngine;
using System.Linq;

namespace CustomUnity
{
    [System.Serializable]
    public sealed class GameObjectPool
    {
        public GameObject prefab;
        public int quantity;
        
        struct Entry
        {
            public GameObject go;
            public float time;
        }

        Entry[] objs;

        public string Name { get { return prefab ? prefab.name : null; } }
        public int ActiveCount { get { return objs.Count(i => i.go.activeSelf); } }

        public void SetUp()
        {
            if(prefab) {
                objs = new Entry[quantity];
                for(int i = 0; i < quantity; i++) {
                    objs[i] = new Entry {
                        go = Object.Instantiate(prefab),
                        time = 0
                    };
                    objs[i].go.SetActive(false);
                }
            }
        }
        
        public void InactivateAll()
        {
            foreach(var i in objs) i.go.SetActive(false);
        }

        public void ForEachActiveObjects(System.Action<GameObject> action)
        {
            foreach(var i in objs) if(i.go.activeInHierarchy) action(i.go);
        }

        public void ForEachObjects(System.Action<GameObject> action)
        {
            foreach(var i in objs) action(i.go);
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            float oldestTime = float.MaxValue;
            int retIndex = 0;
            for(int i = 0; i < objs.Length; i++) {
                if(!objs[i].go.activeSelf) {
                    retIndex = i;
                    break;
                }
                if(objs[i].time < oldestTime) {
                    oldestTime = objs[i].time;
                    retIndex = i;
                }
            }
            var ret = objs[retIndex];
            ret.time = Time.timeSinceLevelLoad;
            ret.go.SetActive(false);
            ret.go.transform.position = position;
            ret.go.transform.rotation = rotation;
            ret.go.SetActive(true);
            objs[retIndex] = ret;
            return ret.go;
        }

        public GameObject TrySpawn(Vector3 position, Quaternion rotation)
        {
            if(ActiveCount < quantity) return Spawn(position, rotation);
            return null;
        }
    }
}