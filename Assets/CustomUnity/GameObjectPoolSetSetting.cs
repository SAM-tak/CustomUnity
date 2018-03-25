using UnityEngine;

namespace CustomUnity
{
    [CreateAssetMenu(menuName = "Custom Unity/GameObject Pool Setting")]
    public class GameObjectPoolSetSetting : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public GameObject prefab;
            public int quantity;
        }
        
        public Entry[] pools;

        public GameObjectPool[] ToObjectPoolArray()
        {
            var ret = new GameObjectPool[pools.Length];
            for(int i = 0; i < pools.Length; ++i) {
                ret[i] = new GameObjectPool {
                    prefab = pools[i].prefab,
                    quantity = pools[i].quantity
                };
            }
            return ret;
        }

        public GameObjectPoolSet ToObjectPoolSet()
        {
            return new GameObjectPoolSet { pools = ToObjectPoolArray() };
        }
    }
}