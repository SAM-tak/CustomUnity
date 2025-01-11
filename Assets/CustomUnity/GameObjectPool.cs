using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Game Object Pool
    /// </summary>
    [Serializable]
    public class GameObjectPool
    {
        public GameObject prefab;
        public int quantity;

        public string Name => prefab ? prefab.name : string.Empty;

        struct Entry : IEquatable<Entry>
        {
            public GameObject go;
            public float time;

            public override readonly bool Equals(object obj) => obj is Entry entry && Equals(entry);

            public readonly bool Equals(Entry other) => EqualityComparer<GameObject>.Default.Equals(go, other.go) && time == other.time;

            public override readonly int GetHashCode() => HashCode.Combine(go, time);
        }

        Entry[] _entries;

        public int ActiveCount => _entries?.Count(i => i.go.activeInHierarchy) ?? 0;

        Transform _owner;

        public void SetUp(Transform owner = null)
        {
            if(prefab) {
                _owner = owner;
                _entries = new Entry[quantity];
                for(int i = 0; i < quantity; i++) {
                    _entries[i] = new Entry { go = UnityEngine.Object.Instantiate(prefab, owner), time = 0 };
                    _entries[i].go.SetActive(false);
                }
            }
        }

        public async Awaitable SetUpAsync(Transform owner = null)
        {
            if(prefab) {
                _owner = owner;
                _entries = new Entry[quantity];
                var gos = await UnityEngine.Object.InstantiateAsync(prefab, quantity, owner);
                for(int i = 0; i < quantity; i++) {
                    _entries[i] = new Entry { go = gos[i], time = 0 };
                    _entries[i].go.SetActive(false);
                }
            }
        }

        public void CollectInactives()
        {
            if(_entries != null && _owner) {
                foreach(var i in _entries) {
                    if(!i.go.activeSelf && i.go.transform.parent != _owner) {
                        i.go.transform.parent = _owner;
                    }
                }
            }
        }

        public void Deactivate(GameObject go)
        {
            if(_entries.Any(x => x.go == go)) {
                go.SetActive(false);
                go.transform.parent = _owner;
            }
        }

        public void DeactivateAll()
        {
            foreach(var i in _entries) {
                if(i.go) {
                    i.go.SetActive(false);
                    i.go.transform.parent = _owner;
                }
            }
        }

        public IEnumerable<GameObject> AllGameObjects => _entries.Select(x => x.go);

        public IEnumerable<GameObject> ActiveGameObjects => _entries.Where(x => x.go.activeInHierarchy).Select(x => x.go);

        GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent)
        {
            float oldestTime = float.MaxValue;
            int retIndex = 0;
            for(int i = 0; i < _entries.Length; i++) {
                if(!_entries[i].go) {
                    Log.Error($"GameObjectPool : '{Name}' ({retIndex}) : Detected pooled GameObject is missing. If you want to spawn it under a shorter lifetime GameObject, needs to use GameObjectPool|GameObjectPoolSet.Deactivate() before destroy parent GameObject to rescue pooled GameObject.");
                    continue;
                }
                if(!_entries[i].go.activeInHierarchy) {
                    retIndex = i;
                    break;
                }
                if(_entries[i].time < oldestTime) {
                    oldestTime = _entries[i].time;
                    retIndex = i;
                }
            }
            var ret = _entries[retIndex];
            ret.time = Time.timeSinceLevelLoad;
            ret.go.SetActive(false);
            ret.go.transform.parent = parent;
            ret.go.transform.SetPositionAndRotation(position, rotation);
            ret.go.SetActive(true);
            _entries[retIndex] = ret;
            return ret.go;
        }

        GameObject TrySpawn(Vector3 position, Quaternion rotation, Transform parent)
        {
            return ActiveCount < quantity ? Spawn(position, rotation, parent) : null;
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation) => Spawn(position, rotation, null);

        public GameObject TrySpawn(Vector3 position, Quaternion rotation) => TrySpawn(position, rotation, null);

        public GameObject Spawn(Transform parent, Vector3 position, Quaternion rotation) => Spawn(position, rotation, parent);

        public GameObject TrySpawn(Transform parent, Vector3 position, Quaternion rotation) => TrySpawn(position, rotation, parent);
    }
}
