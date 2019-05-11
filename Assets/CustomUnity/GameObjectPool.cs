using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CustomUnity
{
    [Serializable]
    public sealed class GameObjectPool
    {
        public GameObject prefab;
        public int quantity;
        
        struct Entry : IEquatable<Entry>
        {
            public GameObject go;
            public float time;

            public override bool Equals(object obj) => obj is Entry entry && Equals(entry);

            public bool Equals(Entry other) => EqualityComparer<GameObject>.Default.Equals(go, other.go) && time == other.time;

            public override int GetHashCode()
            {
                var hashCode = -1359831577;
                hashCode = hashCode * -1521134295 + EqualityComparer<GameObject>.Default.GetHashCode(go);
                hashCode = hashCode * -1521134295 + time.GetHashCode();
                return hashCode;
            }
        }

        Entry[] objs;

        public string Name { get { return prefab ? prefab.name : null; } }
        public int ActiveCount { get { return objs.Count(i => i.go.activeInHierarchy); } }

        public void SetUp()
        {
            if(prefab) {
                objs = new Entry[quantity];
                for(int i = 0; i < quantity; i++) {
                    objs[i] = new Entry {
                        go = UnityEngine.Object.Instantiate(prefab),
                        time = 0
                    };
                    objs[i].go.SetActive(false);
                }
            }
        }
        
        public void InactivateAll()
        {
            foreach(var i in objs) {
                i.go.SetActive(false);
                i.go.transform.parent = null;
            }
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
                if(!objs[i].go.activeInHierarchy) {
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