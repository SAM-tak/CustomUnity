#define PROFILING
using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace CustomUnity
{
    public struct ProfileSampler : IDisposable, IEquatable<ProfileSampler>
    {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
        static readonly Dictionary<string, CustomSampler> samplerCache = new (256);
        static readonly List<CustomSampler> samplerStack = new (256);

        static CustomSampler PeekLast()
        {
            if(samplerStack.Count > 0) return samplerStack[^1];
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

        static public ProfileSampler Create(UnityEngine.Object obj, string memberName, int id = 0)
        {
#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
            return new ProfileSampler(obj.GetMemberName(memberName, id));
#else
            return new ProfileSampler(string.Empty);
#endif
        }

#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
        readonly CustomSampler customSampler;
        readonly int index;
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
        static public void Begin(UnityEngine.Object obj, string memberName, int id)
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
        static public void EndAndBegin(UnityEngine.Object obj, string memberName, int id)
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

#if(UNITY_EDITOR || DEVELOPMENT_BUILD) && PROFILING
        public override bool Equals(object obj)
        {
            return obj is ProfileSampler sampler && Equals(sampler);
        }

        public bool Equals(ProfileSampler other)
        {
            return EqualityComparer<CustomSampler>.Default.Equals(customSampler, other.customSampler) &&
                   index == other.index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(customSampler, index);
        }
#else
        public bool Equals(ProfileSampler other) => false;
#endif
    }
}
