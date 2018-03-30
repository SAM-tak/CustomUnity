using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*  The AssetBundle Manager provides a High-Level API for working with AssetBundles. 
    The AssetBundle Manager will take care of loading AssetBundles and their associated 
    Asset Dependencies.
        Initialize()
            Initializes the AssetBundle manifest object.
        LoadAssetAsync()
            Loads a given asset from a given AssetBundle and handles all the dependencies.
        LoadLevelAsync()
            Loads a given scene from a given AssetBundle and handles all the dependencies.
        LoadDependencies()
            Loads all the dependent AssetBundles for a given AssetBundle.
        BaseDownloadingURL
            Sets the base downloading url which is used for automatic downloading dependencies.
        SimulateAssetBundleInEditor
            Sets Simulation Mode in the Editor.
        Variants
            Sets the active variant.
        RemapVariantName()
            Resolves the correct AssetBundle according to the active variant.
*/

namespace CustomUnity
{
    /// <summary>
    /// Loaded assetBundle contains the references count which can be used to
    /// unload dependent assetBundles automatically.
    /// </summary>
    public class LoadedAssetBundle
    {
        public AssetBundle AssetBundle { get; }
        public int ReferencedCount { get; }

        internal AssetBundle m_AssetBundle;
        internal int m_ReferencedCount;

        internal event Action OnUnload;

        internal void Unload()
        {
            m_AssetBundle?.Unload(false);
            m_AssetBundle = null;
            OnUnload?.Invoke();
            OnUnload = null;
        }

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 1;
        }
    }

    /// <summary>
    /// Class takes care of loading assetBundle and its dependencies
    /// automatically, loading variants automatically.
    /// </summary>
    public class AssetBundleManager : MonoBehaviour
    {
        public const string AssetBundlesOutputPath = "AssetBundles";

        public enum LogMode { All, JustErrors };
        public enum LogType { Info, Warning, Error };

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public const string DevelopmentAssetBundleServer = "http://127.0.0.1:7888";
#endif
#if UNITY_EDITOR
        static bool? m_SimulateAssetBundleInEditor;
        const string kSimulateAssetBundles = "SimulateAssetBundles";
        const string menuStringSimulationMode = "Assets/AssetBundles/Simulation Mode";

        [MenuItem(menuStringSimulationMode)]
        public static void ToggleSimulationMode()
        {
            SimulateAssetBundleInEditor = !SimulateAssetBundleInEditor;
        }

        [MenuItem(menuStringSimulationMode, true)]
        public static bool ToggleSimulationModeValidate()
        {
            Menu.SetChecked(menuStringSimulationMode, SimulateAssetBundleInEditor);
            return true;
        }

        public const string kLocalAssetBundleServerURL = "LocalAssetBundleServerURL";
        /// <summary>
        /// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
        /// </summary>
        public static bool SimulateAssetBundleInEditor {
            get {
                if(!m_SimulateAssetBundleInEditor.HasValue) m_SimulateAssetBundleInEditor = EditorPrefs.GetBool(kSimulateAssetBundles, true);
                return m_SimulateAssetBundleInEditor.Value;
            }
            set {
                if(!m_SimulateAssetBundleInEditor.HasValue || m_SimulateAssetBundleInEditor.Value != value) {
                    m_SimulateAssetBundleInEditor = value;
                    EditorPrefs.SetBool(kSimulateAssetBundles, value);
                }
            }
        }

        [MenuItem("Assets/AssetBundles/Clear All Cache")]
        static public void ClearAllCache()
        {
            Caching.ClearCache();
        }
#endif

        public static Dictionary<string, LoadedAssetBundle> LoadedAssetBundles { get; private set; } = new Dictionary<string, LoadedAssetBundle>();
        public static Dictionary<string, string> DownloadingErrors { get; private set; } = new Dictionary<string, string>();
        public static List<string> DownloadingBundles { get; private set; }  = new List<string>();
        public static List<AssetBundleLoadOperation> InProgressOperations { get; private set; } = new List<AssetBundleLoadOperation>();
        public static Dictionary<string, string[]> Dependencies { get; private set; } = new Dictionary<string, string[]>();

        public static LogMode CurrentLogMode { get; set; } = LogMode.All;

        /// <summary>
        /// The base downloading url which is used to generate the full
        /// downloading url with the assetBundle names.
        /// </summary>
        public static string BaseDownloadingURL { get; set; } = string.Empty;

        public delegate string OverrideBaseDownloadingURLDelegate(string bundleName);

        /// <summary>
        /// Implements per-bundle base downloading URL override.
        /// The subscribers must return null values for unknown bundle names;
        /// </summary>
        public static event OverrideBaseDownloadingURLDelegate OverrideBaseDownloadingURL;

        /// <summary>
        /// Variants which is used to define the active variants.
        /// </summary>
        public static string[] ActiveVariants { get; set; } = { };

        /// <summary>
        /// AssetBundleManifest object which can be used to load the dependecies
        /// and check suitable assetBundle variants.
        /// </summary>
        public static AssetBundleManifest Manifest { get; internal set; }

        [SerializeField]
        bool foldoutManifest;
        [SerializeField]
        bool foldoutLoadedAssetBundles;
        [SerializeField]
        bool foldoutDownLoadings;

        private static void Log(LogType logType, string text)
        {
            if(logType == LogType.Error) {
                Debug.LogError("[AssetBundleManager] " + text);
            }
            else if(CurrentLogMode == LogMode.All && logType == LogType.Warning) {
                Debug.LogWarning("[AssetBundleManager] " + text);
            }
            else if(CurrentLogMode == LogMode.All) {
                Debug.Log("[AssetBundleManager] " + text);
            }
        }

        private static string GetStreamingAssetsPath()
        {
            if(Application.isEditor) {
                return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
            }
            else if(Application.isMobilePlatform || Application.isConsolePlatform) {
                return Application.streamingAssetsPath;
            }
            else {// For standalone player.
                return "file://" + Application.streamingAssetsPath;
            }
        }

        /// <summary>
        /// Sets base downloading URL to a directory relative to the streaming assets directory.
        /// Asset bundles are loaded from a local directory.
        /// </summary>
        public static void SetSourceAssetBundleDirectory(string relativePath)
        {
            BaseDownloadingURL = GetStreamingAssetsPath() + relativePath;
        }

        /// <summary>
        /// Sets base downloading URL to a web URL. The directory pointed to by this URL
        /// on the web-server should have the same structure as the AssetBundles directory
        /// in the demo project root.
        /// </summary>
        /// <example>For example, AssetBundles/iOS/xyz-scene must map to
        /// absolutePath/iOS/xyz-scene.
        /// <example>
        public static void SetSourceAssetBundleURL(string absolutePath)
        {
            if(!absolutePath.EndsWith("/")) {
                absolutePath += "/";
            }

            BaseDownloadingURL = absolutePath + GetPlatformName() + "/";
        }

        /// <summary>
        /// Sets base downloading URL to a local development server URL.
        /// </summary>
        public static void SetDevelopmentAssetBundleServer()
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to setup a download URL
            if(SimulateAssetBundleInEditor) return;

            var localAssetBundleServerURL = EditorPrefs.GetString(kLocalAssetBundleServerURL);
            if(!string.IsNullOrEmpty(localAssetBundleServerURL)) {
                SetSourceAssetBundleURL(localAssetBundleServerURL);
                return;
            }
#endif
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if(string.IsNullOrEmpty(DevelopmentAssetBundleServer)) {
                Log(LogType.Error, "Development Server URL could not be found.");
            }
            else {
                SetSourceAssetBundleURL(DevelopmentAssetBundleServer);
            }
#endif
        }

        /// <summary>
        /// Retrieves an asset bundle that has previously been requested via LoadAssetBundle.
        /// Returns null if the asset bundle or one of its dependencies have not been downloaded yet.
        /// </summary>
        static public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
        {
            if(DownloadingErrors.TryGetValue(assetBundleName, out error)) return null;

            LoadedAssetBundle bundle = null;
            LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if(!Dependencies.TryGetValue(assetBundleName, out dependencies)) return bundle;

            // Make sure all dependencies are loaded
            foreach(var dependency in dependencies) {
                if(DownloadingErrors.TryGetValue(dependency, out error)) return null;

                // Wait all the dependent assetBundles being loaded.
                LoadedAssetBundle dependentBundle;
                LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if(dependentBundle == null) return null;
            }

            return bundle;
        }

        /// <summary>
        /// Returns true if certain asset bundle has been downloaded without checking
        /// whether the dependencies have been loaded.
        /// </summary>
        static public bool IsAssetBundleDownloaded(string assetBundleName)
        {
            return LoadedAssetBundles.ContainsKey(assetBundleName);
        }

        /// <summary>
        /// Initializes asset bundle namager and starts download of manifest asset bundle.
        /// Returns the manifest asset bundle downolad operation object.
        /// </summary>
        static public AssetBundleLoadManifestOperation Initialize()
        {
            return Initialize(GetPlatformName());
        }

        /// <summary>
        /// Initializes asset bundle namager and starts download of manifest asset bundle.
        /// Returns the manifest asset bundle downolad operation object.
        /// </summary>
        static public AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
        {
#if UNITY_EDITOR
            Log(LogType.Info, "Simulation Mode: " + (SimulateAssetBundleInEditor ? "Enabled" : "Disabled"));
#endif

            var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
            DontDestroyOnLoad(go);

#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't need the manifest assetBundle.
            if(SimulateAssetBundleInEditor) return null;
#endif

            LoadAssetBundle(manifestAssetBundleName, true);
            var operation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
            InProgressOperations.Add(operation);
            return operation;
        }

        // Temporarily work around a il2cpp bug
        static protected void LoadAssetBundle(string assetBundleName)
        {
            LoadAssetBundle(assetBundleName, false);
        }

        // Starts the download of the asset bundle identified by the given name, and asset bundles
        // that this asset bundle depends on.
        static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest)
        {
            Log(LogType.Info, "Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);

#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
            if(SimulateAssetBundleInEditor) return;
#endif

            if(!isLoadingAssetBundleManifest) {
                if(Manifest == null) {
                    Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                    return;
                }
            }

            // Check if the assetBundle has already been processed.
            bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);

            // Load dependencies.
            if(!isAlreadyProcessed && !isLoadingAssetBundleManifest) LoadDependencies(assetBundleName);
        }

        // Returns base downloading URL for the given asset bundle.
        // This URL may be overridden on per-bundle basis via overrideBaseDownloadingURL event.
        protected static string GetAssetBundleBaseDownloadingURL(string bundleName)
        {
            if(OverrideBaseDownloadingURL != null) {
                foreach(OverrideBaseDownloadingURLDelegate method in OverrideBaseDownloadingURL.GetInvocationList()) {
                    string res = method(bundleName);
                    if(res != null) return res;
                }
            }
            return BaseDownloadingURL;
        }

        // Checks who is responsible for determination of the correct asset bundle variant
        // that should be loaded on this platform. 
        //
        // On most platforms, this is done by the AssetBundleManager itself. However, on
        // certain platforms (iOS at the moment) it's possible that an external asset bundle
        // variant resolution mechanism is used. In these cases, we use base asset bundle 
        // name (without the variant tag) as the bundle identifier. The platform-specific 
        // code is responsible for correctly loading the bundle.
        static protected bool UsesExternalBundleVariantResolutionMechanism(string baseAssetBundleName)
        {
#if ENABLE_IOS_APP_SLICING
            var url = GetAssetBundleBaseDownloadingURL(baseAssetBundleName);
            if (url.ToLower().StartsWith("res://") ||
                url.ToLower().StartsWith("odr://"))
                return true;
#endif
            return false;
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        static protected string RemapVariantName(string assetBundleName)
        {
            var bundlesWithVariant = Manifest.GetAllAssetBundlesWithVariant();

            // Get base bundle name
            var baseName = assetBundleName.Split('.')[0];

            if(UsesExternalBundleVariantResolutionMechanism(baseName)) return baseName;

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for(int i = 0; i < bundlesWithVariant.Length; i++) {
                var curSplit = bundlesWithVariant[i].Split('.');
                var curBaseName = curSplit[0];
                var curVariant = curSplit[1];

                if(curBaseName != baseName) continue;

                int found = Array.IndexOf(ActiveVariants, curVariant);

                // If there is no active variant found. We still want to use the first
                if(found == -1) found = int.MaxValue - 1;

                if(found < bestFit) {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if(bestFit == int.MaxValue - 1) {
                Log(LogType.Warning, "Ambigious asset bundle variant chosen because there was no matching active variant: " + bundlesWithVariant[bestFitIndex]);
            }

            if(bestFitIndex != -1) {
                return bundlesWithVariant[bestFitIndex];
            }
            else {
                return assetBundleName;
            }
        }

        // Sets up download operation for the given asset bundle if it's not downloaded already.
        static protected bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
        {
            // Already loaded.
            LoadedAssetBundle bundle = null;
            LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle != null) {
                bundle.m_ReferencedCount++;
                return true;
            }

            // @TODO: Do we need to consider the referenced count of WWWs?
            // In the demo, we never have duplicate WWWs as we wait LoadAssetAsync()/LoadLevelAsync() to be finished before calling another LoadAssetAsync()/LoadLevelAsync().
            // But in the real case, users can call LoadAssetAsync()/LoadLevelAsync() several times then wait them to be finished which might have duplicate WWWs.
            if(DownloadingBundles.Contains(assetBundleName)) return true;

            var bundleBaseDownloadingURL = GetAssetBundleBaseDownloadingURL(assetBundleName);

            if(bundleBaseDownloadingURL.ToLower().StartsWith("odr://")) {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                Log(LogType.Info, "Requesting bundle " + assetBundleName + " through ODR");
                InProgressOperations.Add(new AssetBundleDownloadFromODROperation(assetBundleName));
#else
                new ApplicationException("Can't load bundle " + assetBundleName + " through ODR: this Unity version or build target doesn't support it.");
#endif
            }
            else if(bundleBaseDownloadingURL.ToLower().StartsWith("res://")) {
#if ENABLE_IOS_APP_SLICING
                Log(LogType.Info, "Requesting bundle " + assetBundleName + " through asset catalog");
                InProgressOperations.Add(new AssetBundleOpenFromAssetCatalogOperation(assetBundleName));
#else
                new ApplicationException("Can't load bundle " + assetBundleName + " through asset catalog: this Unity version or build target doesn't support it.");
#endif
            }
            else {
                WWW download = null;

                if(!bundleBaseDownloadingURL.EndsWith("/")) {
                    bundleBaseDownloadingURL += "/";
                }

                var url = bundleBaseDownloadingURL + assetBundleName;

                // For manifest assetbundle, always download it as we don't have hash for it.
                if(isLoadingAssetBundleManifest) {
                    download = new WWW(url);
                }
                else {
                    download = WWW.LoadFromCacheOrDownload(url, Manifest.GetAssetBundleHash(assetBundleName), 0);
                }

                InProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, download));
            }
            DownloadingBundles.Add(assetBundleName);

            return false;
        }

        // Where we get all the dependencies and load them all.
        static protected void LoadDependencies(string assetBundleName)
        {
            if(Manifest == null) {
                Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            var dependencies = Manifest.GetAllDependencies(assetBundleName);
            if(dependencies.Length == 0) return;

            for(int i = 0; i < dependencies.Length; i++) {
                dependencies[i] = RemapVariantName(dependencies[i]);
            }

            // Record and load all dependencies.
            Dependencies.Add(assetBundleName, dependencies);
            for(int i = 0; i < dependencies.Length; i++) {
                LoadAssetBundleInternal(dependencies[i], false);
            }
        }

        /// <summary>
        /// Unloads assetbundle and its dependencies.
        /// </summary>
        static public void UnloadAssetBundle(string assetBundleName)
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
            if(SimulateAssetBundleInEditor) return;
#endif
            assetBundleName = RemapVariantName(assetBundleName);

            UnloadAssetBundleInternal(assetBundleName);
        }

        static protected void UnloadDependencies(string assetBundleName)
        {
            string[] dependencies = null;
            if(!Dependencies.TryGetValue(assetBundleName, out dependencies)) return;

            // Loop dependencies.
            foreach(var dependency in dependencies) {
                UnloadAssetBundleInternal(dependency);
            }

            Dependencies.Remove(assetBundleName);
        }

        static protected void UnloadAssetBundleInternal(string assetBundleName)
        {
            string error;
            var bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if(bundle == null) return;

            if(--bundle.m_ReferencedCount == 0) {
                bundle.Unload();
                LoadedAssetBundles.Remove(assetBundleName);
                UnloadDependencies(assetBundleName);
                Log(LogType.Info, assetBundleName + " has been unloaded successfully");
            }
        }

        void Update()
        {
            // Update all in progress operations
            for(int i = 0; i < InProgressOperations.Count;) {
                var operation = InProgressOperations[i];
                if(operation.Update()) i++;
                else {
                    InProgressOperations.RemoveAt(i);
                    ProcessFinishedOperation(operation);
                }
            }
        }

        void ProcessFinishedOperation(AssetBundleLoadOperation operation)
        {
            var download = operation as AssetBundleDownloadOperation;
            if(download == null) return;

            if(string.IsNullOrEmpty(download.Error)) {
                LoadedAssetBundles.Add(download.AssetBundleName, download.AssetBundle);
            }
            else {
                var msg = string.Format("Failed downloading bundle {0} from {1}: {2}", download.AssetBundleName, download.GetSourceURL(), download.Error);
                DownloadingErrors.Add(download.AssetBundleName, msg);
            }

            DownloadingBundles.Remove(download.AssetBundleName);
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type)
        {
            Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");

            AssetBundleLoadAssetOperation operation = null;
#if UNITY_EDITOR
            if(SimulateAssetBundleInEditor) {
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
                if(assetPaths.Length == 0) {
                    Log(LogType.Error, "There is no asset with name \"" + assetName + "\" in " + assetBundleName);
                    return null;
                }

                // @TODO: Now we only get the main object from the first asset. Should consider type also.
                var target = AssetDatabase.LoadMainAssetAtPath(assetPaths[0]);
                operation = new AssetBundleLoadAssetOperationSimulation(target);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);

                InProgressOperations.Add(operation);
            }

            return operation;
        }

        /// <summary>
        /// Starts a load operation for a level from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
        {
            Log(LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");

            AssetBundleLoadOperation operation = null;
#if UNITY_EDITOR
            if(SimulateAssetBundleInEditor) {
                operation = new AssetBundleLoadLevelSimulationOperation(assetBundleName, levelName, isAdditive);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);

                InProgressOperations.Add(operation);
            }

            return operation;
        }

        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
            return GetPlatformForAssetBundles(Application.platform);
#endif
        }

#if UNITY_EDITOR
        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch(target) {
            case BuildTarget.Android:
                return "Android";
#if UNITY_TVOS
            case BuildTarget.tvOS:
                return "tvOS";
#endif
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebGL:
                return "WebGL";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSX:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
            }
        }
#endif

        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch(platform) {
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
#if UNITY_TVOS
            case RuntimePlatform.tvOS:
                return "tvOS";
#endif
            case RuntimePlatform.WebGLPlayer:
                return "WebGL";
            case RuntimePlatform.WindowsPlayer:
                return "Windows";
            case RuntimePlatform.OSXPlayer:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
            }
        }
    } // End of AssetBundleManager.
}