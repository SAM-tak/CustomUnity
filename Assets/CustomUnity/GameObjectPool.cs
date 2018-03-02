using UnityEngine;
using System.Linq;

namespace CustomUnity
{
    [System.Serializable]
	public sealed class GameObjectPool
	{
        public GameObject prefab;
        public int quantity;

        struct Entry {
			public Entry(GameObject go, float time)
			{
				this.go = go;
				this.time = time;
			}
			public GameObject go;
			public float time;
		}
		Entry[] objs;
        
		public int Count { get { return objs.Count(i => i.go.activeSelf); } }
        
        public void SetUp()
        {
            if(prefab) {
                objs = new Entry[quantity];
                for(int i = 0; i < quantity; i++) {
                    objs[i] = new Entry(Object.Instantiate(prefab), 0);
                }
            }
        }

        public void Reset()
		{
			foreach(var i in objs) i.go.SetActive(false);
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
				if(!objs[i].go.activeSelf) {
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
			ret.go.transform.position = position;
			ret.go.transform.rotation = rotation;
			ret.go.SetActive(false);
			ret.go.SetActive(true);
			objs[retIndex] = ret;
			return ret.go;
		}
	}
}