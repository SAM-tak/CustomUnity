using System.Diagnostics;
using CustomUnity;

namespace YourProjectNamespace
{
    public abstract class StateMachineBehaviour : CustomUnity.StateMachineBehaviour
    {
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
