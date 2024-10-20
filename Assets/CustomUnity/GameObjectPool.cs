using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CustomUnity
{
    /// <summary>
    /// Game Object Pool
    /// </summary>
    [Serializable]
    public sealed class GameObjectPool
    {
        public GameObject prefab;
        public int quantity;
        
        struct Entry : IEquatable<Entry>
        {
            public GameObject go;
            public float time;

            public override readonly bool Equals(object obj) => obj is Entry entry && Equals(entry);

            public readonly bool Equals(Entry other) => EqualityComparer<GameObject>.Default.Equals(go, other.go) && time == other.time;

            public override readonly int GetHashCode() => HashCode.Combine(go, time);
        }

        Entry[] entries;

        public string Name => prefab ? prefab.name : string.Empty;
        public int ActiveCount => entries?.Count(i => i.go.activeInHierarchy) ?? 0;

        Transform _owner;

        public void SetUp(Transform owner = null)
        {
            if(prefab) {
                _owner = owner;
                entries = new Entry[quantity];
                for(int i = 0; i < quantity; i++) {
                    entries[i] = new Entry { go = UnityEngine.Object.Instantiate(prefab, owner), time = 0 };
                    entries[i].go.SetActive(false);
                }
            }
        }

        public void CollectInactives()
        {
            if(entries != null && _owner) {
                foreach(var i in entries) {
                    if(!i.go.activeSelf && i.go.transform.parent != _owner) {
                        i.go.transform.parent = _owner;
                    }
                }
            }
        }

        public void Deactivate(GameObject go)
        {
            if(entries.Any(x => x.go == go)) {
                go.SetActive(false);
                go.transform.parent = _owner;
            }
        }

        public void DeactivateAll()
        {
            foreach(var i in entries) {
                i.go.SetActive(false);
                i.go.transform.parent = _owner;
            }
        }

        public IEnumerable<GameObject> AllGameObjects => entries.Select(x => x.go);

        public IEnumerable<GameObject> ActiveGameObjects => entries.Where(x => x.go.activeInHierarchy).Select(x => x.go);

        GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent, bool parents)
        {
            float oldestTime = float.MaxValue;
            int retIndex = 0;
            for(int i = 0; i < entries.Length; i++) {
                if(!entries[i].go) {
                    Log.Error($"GameObjectPool : '{Name}' ({retIndex}) : Detected pooled GameObject is missing. If you want to spawn it under a shorter lifetime GameObject, needs to use GameObjectPool|GameObjectPoolSet.Deactivate() before destroy parent GameObject to rescue pooled GameObject.");
                    continue;
                }
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
            ret.go.transform.parent = parent;
            ret.go.transform.SetPositionAndRotation(position, rotation);
            ret.go.SetActive(true);
            entries[retIndex] = ret;
            return ret.go;
        }

        GameObject TrySpawn(Vector3 position, Quaternion rotation, Transform parent, bool parents)
        {
            return ActiveCount < quantity ? Spawn(position, rotation, parent, parents) : null;
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation) => Spawn(position, rotation, null, false);

        public GameObject TrySpawn(Vector3 position, Quaternion rotation) => TrySpawn(position, rotation, null, false);

        public GameObject Spawn(Transform parent, Vector3 position, Quaternion rotation) => Spawn(position, rotation, parent, true);

        public GameObject TrySpawn(Transform parent, Vector3 position, Quaternion rotation) => TrySpawn(position, rotation, parent, true);
    }
}
