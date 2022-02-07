using UnityEngine;

namespace CustomUnity
{
    public static class PlayerPrefs
    {
        public static string Key(string key) => Application.identifier + "." + key;

        public static int GetInt(string key, int @default = 0) => UnityEngine.PlayerPrefs.GetInt(Key(key), @default);

        public static string GetString(string key, string @default = null) => UnityEngine.PlayerPrefs.GetString(Key(key), @default);

        public static float GetFloat(string key, float @default = 0f) => UnityEngine.PlayerPrefs.GetFloat(Key(key), @default);

        public static bool GetBool(string key, bool @default = false) => UnityEngine.PlayerPrefs.GetInt(Key(key), @default ? 1 : 0) != 0;

        public static int Get(string key, int @default) => GetInt(key, @default);

        public static string Get(string key, string @default) => GetString(key, @default);

        public static float Get(string key, float @default) => GetFloat(key, @default);

        public static bool Get(string key, bool @default) => GetBool(key, @default);

        public static void SetInt(string key, int value) => UnityEngine.PlayerPrefs.SetInt(Key(key), value);

        public static void SetString(string key, string value) => UnityEngine.PlayerPrefs.SetString(Key(key), value);

        public static void SetFloat(string key, float value) => UnityEngine.PlayerPrefs.SetFloat(Key(key), value);

        public static void SetBool(string key, bool value) => UnityEngine.PlayerPrefs.SetInt(Key(key), value ? 1 : 0);

        public static void Set(string key, int value) => SetInt(key, value);

        public static void Set(string key, string value) => SetString(key, value);

        public static void Set(string key, float value) => SetFloat(key, value);

        public static void Set(string key, bool value) => SetBool(key, value);

        public static bool HasKey(string key) => UnityEngine.PlayerPrefs.HasKey(Key(key));
        
        public static void DeleteKey(string key) => UnityEngine.PlayerPrefs.DeleteKey(Key(key));

        public static void DeleteAll() => UnityEngine.PlayerPrefs.DeleteAll();

        public static void Save() => UnityEngine.PlayerPrefs.Save();
    }
}
