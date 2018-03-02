using System.Diagnostics;
using CustomUnity;

namespace YourProjectNamespace
{
    public abstract class StateMachineBehaviour : CustomUnity.StateMachineBehaviour
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void Profiling(string memberName)
        {
            ProfileSampler.Begin(this, memberName, 0);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        protected void Profiling(string memberName, int id)
        {
            ProfileSampler.EndAndBegin(this, memberName, id);
        }

        protected ProfileSampler NewProfiling(string memberName, int id = 0)
        {
            return ProfileSampler.Create(this, memberName, id);
        }
    }
}
