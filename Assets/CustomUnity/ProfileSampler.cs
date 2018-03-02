#define PROFILING
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityFighters
{
    public struct ProfileSampler : System.IDisposable
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
        static Dictionary<string, CustomSampler> samplerCache = new Dictionary<string, CustomSampler>(256);
        static List<CustomSampler> samplerStack = new List<CustomSampler>(256);

        static CustomSampler PeekLast()
        {
            if(samplerStack.Count > 0) return samplerStack[samplerStack.Count - 1];
            return null;
        }

        static int Index()
        {
            return samplerStack.Count - 1;
        }

        static void PopUntil(int until)
        {
            for(int i = samplerStack.Count - 1; i >= 0 && until <= i; --i) {
                var j = samplerStack[i];
                j.End();
                samplerStack.RemoveAt(i);
            }
        }
#endif
        static public ProfileSampler Create(string name)
        {
#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
            return new ProfileSampler(name);
#else
            return new ProfileSampler(string.Empty);
#endif
        }

        static public ProfileSampler Create(Object obj, string memberName, int id = 0)
        {
#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
            return new ProfileSampler(obj.GetMemberName(memberName, id));
#else
            return new ProfileSampler(string.Empty);
#endif
        }

#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
        CustomSampler customSampler;
        int index;
#endif

        ProfileSampler(string name)
        {
            Begin(name);
#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
            customSampler = PeekLast();
            index = Index();
#endif
        }

        public void Dispose()
        {
#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
            if(customSampler != null) PopUntil(index);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Begin(string name)
        {
#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
            if(!samplerCache.ContainsKey(name)) {
                samplerCache.Add(name, CustomSampler.Create(name));
            }
            var sampler = samplerCache[name];
            sampler.Begin();
            samplerStack.Add(sampler);
#endif
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Begin(Object obj, string memberName, int id)
        {
            Begin(obj.GetMemberName(memberName, id));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void EndAndBegin(string name)
        {
            End();
            Begin(name);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void EndAndBegin(Object obj, string memberName, int id)
        {
            EndAndBegin(obj.GetMemberName(memberName, id));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void End()
        {
#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
            if(samplerStack.Count > 0) {
                PeekLast().End();
                samplerStack.RemoveAt(samplerStack.Count - 1);
            }
#endif
        }
    }
}
