using UnityEngine;
using System.Collections.Generic;

namespace CustomUnity
{
    [System.Serializable]
    public sealed class GameObjectPoolSet
    {
        public GameObjectPool[] pools;
        public readonly Dictionary<string, GameObjectPool> index = new Dictionary<string, GameObjectPool>();

        public void SetUp(System.Action<GameObject> setUpProcPerObject = null)
        {
            index.Clear();
            foreach(var i in pools) {
                i.SetUp();
                if(setUpProcPerObject != null) i.ForEachObjects(setUpProcPerObject);
                index.Add(i.Name, i);
            }
        }

        public void SetUp(System.Action<GameObjectPool, GameObject> setUpProcPerObject)
        {
            index.Clear();
            foreach(var i in pools) {
                i.SetUp();
                i.ForEachObjects(x => setUpProcPerObject(i, x));
                index.Add(i.Name, i);
            }
        }

        public void InactivateAll()
        {
            foreach(var i in pools) i.InactivateAll();
        }

        public GameObject Spawn(string name, Vector3 position, Quaternion rotation)
        {
            if(index.TryGetValue(name, out GameObjectPool pool)) return pool.Spawn(position, rotation);
            return null;
        }

        public GameObject TrySpawn(string name, Vector3 position, Quaternion rotation)
        {
            if(index.TryGetValue(name, out GameObjectPool pool)) return pool.TrySpawn(position, rotation);
            return null;
        }

        public GameObject Spawn(Transform root, SpawnObjectParameter parameter)
        {
            if(parameter && index.TryGetValue(parameter.prefabName, out GameObjectPool pool)) {
                var origin = parameter.GetOriginNode(root);
                var go = pool.Spawn(parameter.GetPosition(origin), parameter.GetRotation(origin));
                if(go) go.transform.parent = parameter.parentToOrigin ? origin : null;
                return go;
            }
            return null;
        }

        public GameObject TrySpawn(Transform root, SpawnObjectParameter parameter)
        {
            if(parameter && index.TryGetValue(parameter.prefabName, out GameObjectPool pool)) {
                var origin = parameter.GetOriginNode(root);
                var go = pool.TrySpawn(parameter.GetPosition(origin), parameter.GetRotation(origin));
                if(go) go.transform.parent = parameter.parentToOrigin ? origin : null;
                return go;
            }
            return null;
        }
    }
}