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
#if(UNITY_EDITOR || DEVELOPMENT_BUILD)
            UnityEngine.Debug.Log(message, this);
#endif
        }

        /// <summary>
        /// イベント関数用
        /// </summary>
        protected void DebugBreak()
        {
#if(UNITY_EDITOR || DEVELOPMENT_BUILD)
            UnityEngine.Debug.Log("DebugBreak", this);
            UnityEngine.Debug.Break();
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
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
