using System.Collections.Generic;
using UnityEngine;

namespace CustomUnity
{
    /// <summary>
    /// Set of GameObject Pools
    /// </summary>
    [System.Serializable]
    public class GameObjectPoolSet
    {
        public GameObjectPool[] pools;
        public readonly Dictionary<string, GameObjectPool> index = new();

        public void SetUp(Transform parent, System.Action<GameObject> setUpProcPerObject = null)
        {
            index.Clear();
            foreach(var i in pools) {
                i.SetUp(parent);
                if(setUpProcPerObject != null) {
                    foreach(var j in i.AllGameObjects) setUpProcPerObject(j);
                }
                index.Add(i.Name, i);
            }
        }

        public void SetUp(Transform parent, System.Action<GameObjectPool, GameObject> setUpProcPerObject)
        {
            index.Clear();
            foreach(var i in pools) {
                i.SetUp(parent);
                if(setUpProcPerObject != null) {
                    foreach(var j in i.AllGameObjects) setUpProcPerObject(i, j);
                }
                index.Add(i.Name, i);
            }
        }

        public void SetUp(System.Action<GameObject> setUpProcPerObject = null)
        {
            SetUp(null, setUpProcPerObject);
        }

        public void SetUp(System.Action<GameObjectPool, GameObject> setUpProcPerObject)
        {
            SetUp(null, setUpProcPerObject);
        }

        public void CollectInactives()
        {
            foreach(var i in pools) i.CollectInactives();
        }

        public void Deactivate(GameObject go)
        {
            foreach(var i in pools) i.Deactivate(go);
        }

        public void DeactivateAll()
        {
            foreach(var i in pools) i.DeactivateAll();
        }

        public GameObject Spawn(string name, Vector3 position, Quaternion rotation) => Spawn(name, null, position, rotation);

        public GameObject TrySpawn(string name, Vector3 position, Quaternion rotation) => TrySpawn(name, null, position, rotation);

        public GameObject Spawn(string name, Transform parent, Vector3 position, Quaternion rotation)
        {
            if(index.TryGetValue(name, out GameObjectPool pool)) return pool.Spawn(parent, position, rotation);
            return null;
        }

        public GameObject TrySpawn(string name, Transform parent, Vector3 position, Quaternion rotation)
        {
            if(index.TryGetValue(name, out GameObjectPool pool)) return pool.TrySpawn(parent, position, rotation);
            return null;
        }

        public GameObject Spawn(Transform root, SpawnObjectParameter parameter)
        {
            if(parameter && index.TryGetValue(parameter.prefabName, out GameObjectPool pool)) {
                var origin = parameter.GetOriginNode(root);
                return parameter.parentToOrigin
                    ? pool.Spawn(origin, parameter.GetPosition(origin), parameter.GetRotation(origin)) 
                    : pool.Spawn(parameter.GetPosition(origin), parameter.GetRotation(origin));
            }
            return null;
        }

        public GameObject TrySpawn(Transform root, SpawnObjectParameter parameter)
        {
            if(parameter && index.TryGetValue(parameter.prefabName, out GameObjectPool pool)) {
                var origin = parameter.GetOriginNode(root);
                return parameter.parentToOrigin
                    ? pool.TrySpawn(origin, parameter.GetPosition(origin), parameter.GetRotation(origin))
                    : pool.TrySpawn(parameter.GetPosition(origin), parameter.GetRotation(origin));
            }
            return null;
        }
    }
}
