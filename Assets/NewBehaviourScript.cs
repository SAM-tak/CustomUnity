using CustomUnity;
using UnityEngine;
using UnityEngine.Events;

namespace YourProjectNamespace
{
    public class NewBehaviourScript : MonoBehaviour
    {
        public GameObjectPoolSet gameObjectPools;

        public void SpawnObject(SpawnObjectParameter parameter)
        {
            Log.Info("SpawnObject " + parameter.ToStringFields());
            gameObjectPools?.Spawn(transform, parameter);
            //DebugBreak();
        }

        public void TrySpawnObject(SpawnObjectParameter parameter)
        {
            gameObjectPools?.TrySpawn(transform, parameter);
        }
        
        public UnityEvent @event1;
        void FireEvent1()
        {
            @event1.Invoke();
        }

        public UnityEvent<int> @event2;
        void FireEvent2(int i)
        {
            LogInfo("FireEvent2 {0}", i);
            if(@event2 != null) @event2.Invoke(i);
        }
        
        public UnityEvent @animationExitEvent;
        public void OnAnimationExit(Animator animator, int stateMachinePathHash)
        {
            LogInfo("OnAnimationExit".Aqua());
            @animationExitEvent?.Invoke();
        }

        void Awake()
        {
            gameObjectPools?.SetUp();
        }

        // Use this for initialization
        void Start()
        {
            gameObjectPools?.InactivateAll();
            LogInfo("Start");
            Log.Info(this, "ssss");
            Log.Info(this, "ssss {0}", 1);
            Log.Info("ssss {0}", 2);
            LogInfo("Start {0}", 1);
            LogInfo("Start {0} {1}", 1, 2);
        }

        // Update is called once per frame
        void Update()
        {
            //LogInfo("Update");
        }
    }
}
