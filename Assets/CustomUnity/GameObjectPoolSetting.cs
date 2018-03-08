using UnityEngine;

namespace CustomUnity
{
    [CreateAssetMenu(menuName = "GameObject Pool Setting")]
    public class GameObjectPoolSetting : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            public GameObject prefab;
            public int quantity;
        }

        public Entry[] pools;

        public GameObjectPool[] GameObjectPools {
            get {
                var ret = new GameObjectPool[pools.Length];
                for(int i = 0; i < pools.Length; ++i) {
                    ret[i] = new GameObjectPool {
                        prefab = pools[i].prefab,
                        quantity = pools[i].quantity
                    };
                }
                return ret;
            }
        }
    }
}