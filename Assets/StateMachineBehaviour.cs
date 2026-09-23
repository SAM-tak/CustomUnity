using System.Diagnostics;
using CustomUnity;

namespace YourProjectNamespace
{
    public abstract class StateMachineBehaviour : CustomUnity.StateMachineBehaviour
    {
#if UNITY_6000_6_OR_NEWER
        [Conditional("UNITY_INCLUDE_INSTRUMENTATION")]
#else
        [Conditional("DEVELOPMENT_BUILD")]
#endif
        [Conditional("UNITY_EDITOR")]
        protected void DebugBreak()
        {
            UnityEngine.Debug.Break();
        }

        [Conditional("ENABLE_PROFILER")]
        protected void Profiling(string memberName, int id = 0)
        {
            if(id == 0) ProfileSampler.Begin(this, memberName, id);
            else ProfileSampler.EndAndBegin(this, memberName, id);
        }

        protected ProfileSampler NewProfiling(string memberName, int id = 0)
        {
            return ProfileSampler.Create(this, memberName, id);
        }
    }
}
