namespace CustomUnity
{
    public static class EditorPrefs
    {
        public static int GetInt(string key, int @default = 0) => UnityEditor.EditorPrefs.GetInt(key, @default);

        public static string GetString(string key, string @default = null) => UnityEditor.EditorPrefs.GetString(key, @default);

        public static float GetFloat(string key, float @default = 0f) => UnityEditor.EditorPrefs.GetFloat(key, @default);

        public static bool GetBool(string key, bool @default = false) => UnityEditor.EditorPrefs.GetBool(key, @default);

        public static int Get(string key, int @default) => GetInt(key, @default);

        public static string Get(string key, string @default) => GetString(key, @default);

        public static float Get(string key, float @default) => GetFloat(key, @default);

        public static bool Get(string key, bool @default) => GetBool(key, @default);

        public static void SetInt(string key, int value) => UnityEditor.EditorPrefs.SetInt(key, value);

        public static void SetString(string key, string value) => UnityEditor.EditorPrefs.SetString(key, value);

        public static void SetFloat(string key, float value) => UnityEditor.EditorPrefs.SetFloat(key, value);

        public static void SetBool(string key, bool value) => UnityEditor.EditorPrefs.SetBool(key, value);

        public static void Set(string key, int value) => SetInt(key, value);

        public static void Set(string key, string value) => SetString(key, value);

        public static void Set(string key, float value) => SetFloat(key, value);

        public static void Set(string key, bool value) => SetBool(key, value);

        public static bool HasKey(string key) => UnityEditor.EditorPrefs.HasKey(key);

        public static void DeleteKey(string key) => UnityEditor.EditorPrefs.DeleteKey(key);

        //public static void DeleteAll() => UnityEditor.EditorPrefs.DeleteKey(Key("*"));
    }
}
