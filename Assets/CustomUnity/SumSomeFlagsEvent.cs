using UnityEngine.Events;

namespace CustomUnity
{
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
        [EnumFlags]
        public Flags goalFlags;
        public UnityEvent onSetFlagsAll;

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
                @onSetFlagsAll?.Invoke();
                ClearFlags();
            }
        }

        public void ClearFlags()
        {
            CurrentFlags = 0;
        }
    }
}
