using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace CustomUnity
{
    /// <summary>
    /// Create and manage sample point of CustomSampler
    /// </summary>
    public readonly struct ProfileSampler : IDisposable, IEquatable<ProfileSampler>
    {
#if ENABLE_PROFILER
        static readonly Dictionary<string, CustomSampler> samplerCache = new (256);
        static readonly List<CustomSampler> samplerStack = new (256);

        static CustomSampler PeekLast() => samplerStack.Count > 0 ? samplerStack[^1] : null;

        static int Index() => samplerStack.Count - 1;

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
#if ENABLE_PROFILER
            return new ProfileSampler(name);
#else
            return new ProfileSampler(string.Empty);
#endif
        }

        static public ProfileSampler Create(UnityEngine.Object obj, string memberName, int id = 0)
        {
#if ENABLE_PROFILER
            return new ProfileSampler(obj.GetMemberName(memberName, id));
#else
            return new ProfileSampler(string.Empty);
#endif
        }

#if ENABLE_PROFILER
        readonly CustomSampler customSampler;
        readonly int index;
#endif

        ProfileSampler(string name)
        {
            Begin(name);
#if ENABLE_PROFILER
            customSampler = PeekLast();
            index = Index();
#endif
        }

        public void Dispose()
        {
#if ENABLE_PROFILER
            if(customSampler != null) PopUntil(index);
#endif
        }

        [Conditional("ENABLE_PROFILER")]
        static public void Begin(string name)
        {
#if ENABLE_PROFILER
            if(!samplerCache.ContainsKey(name)) {
                samplerCache.Add(name, CustomSampler.Create(name));
            }
            var sampler = samplerCache[name];
            sampler.Begin();
            samplerStack.Add(sampler);
#endif
        }

        [Conditional("ENABLE_PROFILER")]
        static public void Begin(UnityEngine.Object obj, string memberName, int id)
        {
            Begin(obj.GetMemberName(memberName, id));
        }

        [Conditional("ENABLE_PROFILER")]
        static public void EndAndBegin(string name)
        {
            End();
            Begin(name);
        }

        [Conditional("ENABLE_PROFILER")]
        static public void EndAndBegin(UnityEngine.Object obj, string memberName, int id)
        {
            EndAndBegin(obj.GetMemberName(memberName, id));
        }

        [Conditional("ENABLE_PROFILER")]
        static public void End()
        {
#if ENABLE_PROFILER
            if(samplerStack.Count > 0) {
                PeekLast().End();
                samplerStack.RemoveAt(samplerStack.Count - 1);
            }
#endif
        }

#if ENABLE_PROFILER
        public override bool Equals(object obj) => obj is ProfileSampler sampler && Equals(sampler);

        public bool Equals(ProfileSampler other) => EqualityComparer<CustomSampler>.Default.Equals(customSampler, other.customSampler)
            && index == other.index;

        public override int GetHashCode() => HashCode.Combine(customSampler, index);
#else
        public bool Equals(ProfileSampler other) => false;
#endif
    }
}
