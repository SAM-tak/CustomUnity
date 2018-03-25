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
                Log.Info(i.Name);
            }
            Log.Info(index.Keys);
        }

        public void InactivateAll()
        {
            foreach(var i in pools) i.InactivateAll();
        }

        public GameObject Spawn(string name, Vector3 position, Quaternion rotation)
        {
            if(index.ContainsKey(name)) return index[name].Spawn(position, rotation);
            return null;
        }

        public GameObject TrySpawn(string name, Vector3 position, Quaternion rotation)
        {
            if(index.ContainsKey(name)) return index[name].TrySpawn(position, rotation);
            return null;
        }

        public GameObject Spawn(Transform root, SpawnObjectParameter parameter)
        {
            //Log.Info("parameter.name = {0}", parameter.name);
            GameObjectPool pool;
            if(parameter && index.TryGetValue(parameter.prefabName, out pool)) {
                //Log.Info("parameter.prefabName = {0}", parameter.prefabName);
                var pivot = parameter.GetPivotNode(root);
                var go = pool.Spawn(parameter.GetPosition(pivot), parameter.GetRotation(pivot));
                if(go) go.transform.parent = parameter.parenting ? pivot : null;
                return go;
            }
            return null;
        }

        public GameObject TrySpawn(Transform root, SpawnObjectParameter parameter)
        {
            GameObjectPool pool;
            if(parameter && index.TryGetValue(parameter.prefabName, out pool)) {
                var pivot = parameter.GetPivotNode(root);
                var go = pool.TrySpawn(parameter.GetPosition(pivot), parameter.GetRotation(pivot));
                if(go) go.transform.parent = parameter.parenting ? pivot : null;
                return go;
            }
            return null;
        }
    }
}