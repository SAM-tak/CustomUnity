using UnityEngine;
using UnityEngine.Events;

namespace CustomUnity
{
    /// <summary>
    /// Fire unityevent after specified flags on.
    /// </summary>
    [AddComponentMenu("CustomUnity/Flag/SumSomeFlagsEvent")]
    public class SumSomeFlagsEvent : MonoBehaviour
    {
        [System.Flags]
        public enum Flags
        {
            Flag1 = 1,
            Flag2 = 1 << 1,
            Flag3 = 1 << 2,
            Flag4 = 1 << 3,
            Flag5 = 1 << 4,
            Flag6 = 1 << 5,
            Flag7 = 1 << 6,
            Flag8 = 1 << 7,
            Flag9 = 1 << 8,
        }

#if !UNITY_2020_1_OR_NEWER
        [EnumFlags]
#endif
        public Flags goalFlags;
        public UnityEvent @event;

        public Flags CurrentFlags { get; private set; }
        
        [EnumAction(typeof(Flags))]
        public void SetFlag(int flag)
        {
            SetFlag((Flags)flag);
        }

        public void SetFlag(Flags flag)
        {
            CurrentFlags |= flag;
            if((CurrentFlags & goalFlags) == goalFlags) {
                @event?.Invoke();
                ClearFlags();
            }
        }

        public void ClearFlags()
        {
            CurrentFlags = 0;
        }
    }
}
