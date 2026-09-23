using System.Diagnostics;
using CustomUnity;

namespace YourProjectNamespace
{
    public abstract class MonoBehaviour : CustomUnity.MonoBehaviour
    {
        /// <summary>
        /// イベント関数用
        /// </summary>
        /// <param name="message"></param>
        protected void DebugLog(string message)
        {
            if(!UnityEngine.Debug.isDebugBuild) return;
            UnityEngine.Debug.Log(message, this);
        }

        /// <summary>
        /// イベント関数用
        /// </summary>
        protected void DebugBreak()
        {
            if(!UnityEngine.Debug.isDebugBuild) return;
            UnityEngine.Debug.Log("DebugBreak", this);
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
