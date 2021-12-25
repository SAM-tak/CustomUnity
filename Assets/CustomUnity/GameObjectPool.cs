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

        Entry[] entries;

        public string Name => prefab ? prefab.name : string.Empty;
        public int ActiveCount => entries?.Count(i => i.go.activeInHierarchy) ?? 0;

        public void SetUp(Transform parent = null)
        {
            if(prefab) {
                entries = new Entry[quantity];
                for(int i = 0; i < quantity; i++) {
                    entries[i] = new Entry {
                        go = UnityEngine.Object.Instantiate(prefab),
                        time = 0
                    };
                    entries[i].go.transform.parent = parent;
                    entries[i].go.SetActive(false);
                }
            }
        }
        
        public void InactivateAll()
        {
            foreach(var i in entries) {
                i.go.SetActive(false);
                i.go.transform.parent = null;
            }
        }

        public IEnumerable<GameObject> AllGameObjects => entries.Select(x => x.go);

        public IEnumerable<GameObject> ActiveGameObjects => entries.Where(x => x.go.activeInHierarchy).Select(x => x.go);

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            float oldestTime = float.MaxValue;
            int retIndex = 0;
            for(int i = 0; i < entries.Length; i++) {
                if(!entries[i].go.activeInHierarchy) {
                    retIndex = i;
                    break;
                }
                if(entries[i].time < oldestTime) {
                    oldestTime = entries[i].time;
                    retIndex = i;
                }
            }
            var ret = entries[retIndex];
            ret.time = Time.timeSinceLevelLoad;
            ret.go.SetActive(false);
            ret.go.transform.SetPositionAndRotation(position, rotation);
            ret.go.SetActive(true);
            entries[retIndex] = ret;
            return ret.go;
        }

        public GameObject TrySpawn(Vector3 position, Quaternion rotation)
        {
            if(ActiveCount < quantity) return Spawn(position, rotation);
            return null;
        }
    }
}