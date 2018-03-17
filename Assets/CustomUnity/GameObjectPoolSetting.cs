using UnityEngine;

namespace CustomUnity
{
    [CreateAssetMenu(menuName = "Custom Unity/GameObject Pool Setting")]
    public class GameObjectPoolSetting : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string name;
            public GameObject prefab;
            public int quantity;
        }
        
        public Entry[] pools;

        public GameObjectPool[] ToObjectPoolArray()
        {
            var ret = new GameObjectPool[pools.Length];
            for(int i = 0; i < pools.Length; ++i) {
                ret[i] = new GameObjectPool {
                    name = string.IsNullOrEmpty(name) ? pools[i].prefab.name : pools[i].name,
                    prefab = pools[i].prefab,
                    quantity = pools[i].quantity
                };
            }
            return ret;
        }
    }
}