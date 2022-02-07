using UnityEditor;

namespace CustomUnity
{
    public static class EditorPrefs<T>
    {
        public static string Key(string key) => typeof(T).FullName + "." + key;

        public static int GetInt(string key, int @default = 0) => EditorPrefs.GetInt(Key(key), @default);

        public static string GetString(string key, string @default = null) => EditorPrefs.GetString(Key(key), @default);

        public static float GetFloat(string key, float @default = 0f) => EditorPrefs.GetFloat(Key(key), @default);

        public static bool GetBool(string key, bool @default = false) => EditorPrefs.GetBool(Key(key), @default);

        public static int Get(string key, int @default) => GetInt(key, @default);

        public static string Get(string key, string @default) => GetString(key, @default);

        public static float Get(string key, float @default) => GetFloat(key, @default);

        public static bool Get(string key, bool @default) => GetBool(key, @default);

        public static void SetInt(string key, int value) => EditorPrefs.SetInt(Key(key), value);

        public static void SetString(string key, string value) => EditorPrefs.SetString(Key(key), value);

        public static void SetFloat(string key, float value) => EditorPrefs.SetFloat(Key(key), value);

        public static void SetBool(string key, bool value) => EditorPrefs.SetBool(Key(key), value);

        public static void Set(string key, int value) => SetInt(key, value);

        public static void Set(string key, string value) => SetString(key, value);

        public static void Set(string key, float value) => SetFloat(key, value);

        public static void Set(string key, bool value) => SetBool(key, value);

        public static bool HasKey(string key) => EditorPrefs.HasKey(Key(key));
        
        public static void DeleteKey(string key) => EditorPrefs.DeleteKey(Key(key));

        //public static void DeleteAll() => EditorPrefs.DeleteKey(Key("*"));
    }
}
