using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomUnity
{
	public static class ObjectExtension
	{
        static Dictionary<Object, string> cache;
        static Dictionary<Object, Dictionary<string, Dictionary<int, string>>> memberCache;

        public static string GetName(this Object obj)
        {
            if(!Application.isPlaying) return obj.name;
            if(cache == null) {
                cache = new Dictionary<Object, string>(256);
                SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            }
            if(!cache.ContainsKey(obj)) cache[obj] = obj.name;
            return cache[obj];
        }

        public static string GetMemberName(this Object obj, string memberName, int id = 0)
        {
            if(!Application.isPlaying) return obj.name + "." + memberName;
            var objname = obj.GetName();
            if(memberCache == null) {
                memberCache = new Dictionary<Object, Dictionary<string, Dictionary<int, string>>>(256);
            }
            if(!memberCache.ContainsKey(obj)) {
                memberCache[obj] = new Dictionary<string, Dictionary<int, string>>();
                var l = new Dictionary<int, string>();
                l.Add(id, id == 0 ? objname + "." + memberName : objname + "." + memberName + " " + id);
                memberCache[obj].Add(memberName, l);
            }
            else if(!memberCache[obj].ContainsKey(memberName)) {
                var l = new Dictionary<int, string>();
                l.Add(id, id == 0 ? objname + "." + memberName : objname + "." + memberName + " " + id);
                memberCache[obj].Add(memberName, l);
            }
            else if(!memberCache[obj][memberName].ContainsKey(id)) {
                memberCache[obj][memberName][id] = id == 0 ? objname + "." + memberName : objname + "." + memberName + " " + id;
            }
            return memberCache[obj][memberName][id];
        }

        static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            cache.Clear();
            if(memberCache != null) memberCache.Clear();
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Profiling(this Object obj, string memberName)
        {
            ProfileSampler.Begin(obj, memberName, 0);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        static public void Profiling(this Object obj, string memberName, int id)
        {
            ProfileSampler.EndAndBegin(obj, memberName, id);
        }
        
        static public ProfileSampler NewProfiling(this Object obj, string memberName, int id = 0)
        {
            return ProfileSampler.Create(obj, memberName, id);
        }
    }
}