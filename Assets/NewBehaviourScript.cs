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
            Log.Info($"SpawnObject {parameter.ToStringFields()}");
            gameObjectPools?.Spawn(transform, parameter);
            //DebugBreak();
        }

        public void TrySpawnObject(SpawnObjectParameter parameter)
        {
            gameObjectPools?.TrySpawn(transform, parameter);
        }
        
        public UnityEvent event1;
        public void FireEvent1()
        {
            event1.Invoke();
        }

        public UnityEvent<int> event2;
        public void FireEvent2(int i)
        {
            LogInfo($"FireEvent2 {i}");
            if(event2 != null) event2.Invoke(i);
        }
        
        public UnityEvent animationExitEvent;
        public void OnAnimationExit(Animator _0, int _1)
        {
            LogInfo("OnAnimationExit".Aqua());
            animationExitEvent?.Invoke();
        }
        
        void Awake()
        {
            gameObjectPools?.SetUp();
        }

        // Use this for initialization
        void Start()
        {
            gameObjectPools?.DeactivateAll();
            LogInfo("Start");
            LogInfo($"{"blahblah".Red().Bold()}\n{$"{"foobar".Grey()} {"hogehoge".Color(0x443322FF)}".Small()}");
            LogInfo(RichText.Sb.Bold(sb => sb.Red("blahblah")).Ln().Small(sb => sb.Grey("foobar").Space().Color(0x443322FF, "hogehoge")));
        }

        // Update is called once per frame
        void Update()
        {
            gameObjectPools?.CollectInactives();
        }
    }
}
