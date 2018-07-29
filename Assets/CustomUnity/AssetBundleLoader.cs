using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public AssetBundle AssetBundle { get; internal set; }
        public int ReferencedCount { get; internal set; }

        internal event Action OnUnload;

        internal void Unload()
        {
            AssetBundle?.Unload(false);
            AssetBundle = null;
            OnUnload?.Invoke();
            OnUnload = null;
        }

        public LoadedAssetBundle(AssetBundle assetBundle)
        {
            AssetBundle = assetBundle;
            ReferencedCount = 1;
        }
    }

    /// <summary>
    /// Class takes care of loading assetBundle and its dependencies
    /// automatically, loading variants automatically.
    /// </summary>
    public class AssetBundleLoader : MonoBehaviour
    {
        public enum LogMode { All, JustErrors };
        public enum LogType { Info, Warning, Error };

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public const string DevelopmentAssetBundleServer = "http://127.0.0.1:7888";
#endif
#if UNITY_EDITOR
        static bool? simulatesAssetBundleInEditor;
        const string kSimulatesAssetBundles = "SimulatesAssetBundles";
        const string menuStringSimulationMode = "Assets/AssetBundles/Simulation Mode";

        [MenuItem(menuStringSimulationMode)]
        public static void ToggleSimulationMode()
        {
            SimulatesAssetBundleInEditor = !SimulatesAssetBundleInEditor;
        }

        [MenuItem(menuStringSimulationMode, true)]
        public static bool ToggleSimulationModeValidate()
        {
            Menu.SetChecked(menuStringSimulationMode, SimulatesAssetBundleInEditor);
            return true;
        }

        public const string kLocalAssetBundleServerURL = "LocalAssetBundleServerURL";
        /// <summary>
        /// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
        /// </summary>
        public static bool SimulatesAssetBundleInEditor {
            get {
                if(!simulatesAssetBundleInEditor.HasValue) simulatesAssetBundleInEditor = EditorPrefs.GetBool(kSimulatesAssetBundles, true);
                return simulatesAssetBundleInEditor.Value;
            }
            set {
                if(!simulatesAssetBundleInEditor.HasValue || simulatesAssetBundleInEditor.Value != value) {
                    simulatesAssetBundleInEditor = value;
                    EditorPrefs.SetBool(kSimulatesAssetBundles, value);
                }
            }
        }

        [MenuItem("Assets/AssetBundles/Clear All Cache")]
        static public void ClearAllCache()
        {
            Caching.ClearCache();
        }
#endif
        
        static readonly Dictionary<string, LoadedAssetBundle> loadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();
        static readonly Dictionary<string, string[]> dependencies = new Dictionary<string, string[]>();
        static readonly Dictionary<string, int> downloadingBundles = new Dictionary<string, int>(); // downloading name & refcount (start from 0)
        static readonly Dictionary<string, string> downloadingErrors = new Dictionary<string, string>();
        static readonly List<AssetBundleLoadOperation> inProgressOperations = new List<AssetBundleLoadOperation>();

        public static ReadOnlyDictionary<string, LoadedAssetBundle> LoadedAssetBundles { get; } = new ReadOnlyDictionary<string, LoadedAssetBundle>(loadedAssetBundles);
        public static ReadOnlyDictionary<string, string[]> Dependencies { get; } = new ReadOnlyDictionary<string, string[]>(dependencies);
        public static ReadOnlyDictionary<string, string> DownloadingErrors { get; } = new ReadOnlyDictionary<string, string>(downloadingErrors);
        public static ReadOnlyCollection<AssetBundleLoadOperation> InProgressOperations { get; } = new ReadOnlyCollection<AssetBundleLoadOperation>(inProgressOperations);

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
        public static string[] ActiveVariants { get; set; }

        /// <summary>
        /// AssetBundleManifest object which can be used to load the dependecies
        /// and check suitable assetBundle variants.
        /// </summary>
        public static AssetBundleManifest Manifest { get; internal set; }

        [SerializeField] bool foldoutManifest;
        [SerializeField] bool foldoutLoadedAssetBundles;
        [SerializeField] bool foldoutDownLoadings;
        
        static string GetStreamingAssetsPath()
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
        static void SetDevelopmentAssetBundleServer()
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to setup a download URL
            if(SimulatesAssetBundleInEditor) return;

            var localAssetBundleServerURL = EditorPrefs.GetString(kLocalAssetBundleServerURL);
            if(!string.IsNullOrEmpty(localAssetBundleServerURL)) {
                SetSourceAssetBundleURL(localAssetBundleServerURL);
                return;
            }
#endif
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if(string.IsNullOrEmpty(DevelopmentAssetBundleServer)) {
                Log.Error("[AssetBundelLoader] Development Server URL could not be found.");
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
            loadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if(!Dependencies.TryGetValue(assetBundleName, out dependencies)) return bundle;

            // Make sure all dependencies are loaded
            foreach(var dependency in dependencies) {
                if(DownloadingErrors.TryGetValue(dependency, out error)) return null;

                // Wait all the dependent assetBundles being loaded.
                LoadedAssetBundle dependentBundle;
                loadedAssetBundles.TryGetValue(dependency, out dependentBundle);
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
            return loadedAssetBundles.ContainsKey(assetBundleName);
        }

        /// <summary>
        /// Initializes asset bundle namager and starts download of manifest asset bundle.
        /// Returns the manifest asset bundle downolad operation object.
        /// </summary>
        static public AssetBundleLoadManifestOperation Initialize()
        {
            return Initialize(GetPlatformName());
        }

        static AssetBundleLoader instance;

        /// <summary>
        /// Initializes asset bundle namager and starts download of manifest asset bundle.
        /// Returns the manifest asset bundle downolad operation object.
        /// </summary>
        static public AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
        {
#if UNITY_EDITOR
            if(CurrentLogMode == LogMode.All) Log.Info($"[AssetBundelLoader] Simulation Mode: {(SimulatesAssetBundleInEditor ? "Enabled" : "Disabled")}");
#endif
            SetDevelopmentAssetBundleServer();

#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't need the manifest assetBundle.
            if(SimulatesAssetBundleInEditor) return null;
#endif

            if(!instance) {
                var go = new GameObject("AssetBundleManager", typeof(AssetBundleLoader));
                DontDestroyOnLoad(go);
                instance = go.GetComponent<AssetBundleLoader>();
            }

            LoadAssetBundle(manifestAssetBundleName, true);
            var operation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest");
            inProgressOperations.Add(operation);
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
            if(CurrentLogMode == LogMode.All) Log.Info($"[AssetBundelLoader] Loading Asset Bundle {(isLoadingAssetBundleManifest ? "Manifest" : "")} : {assetBundleName}");

#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
            if(SimulatesAssetBundleInEditor) return;
#endif

            if(!isLoadingAssetBundleManifest) {
                if(Manifest == null) {
                    Log.Error("[AssetBundelLoader] Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                    return;
                }
            }

            // Check if the assetBundle has already been processed.
            bool isAlreadyProcessed = LoadAssetBundleInternal(assetBundleName, false, isLoadingAssetBundleManifest);

            // Load dependencies.
            if(!isAlreadyProcessed && !isLoadingAssetBundleManifest) LoadDependencies(assetBundleName);
        }

        // Returns base downloading URL for the given asset bundle.
        // This URL may be overridden on per-bundle basis via overrideBaseDownloadingURL event.
        static protected string GetAssetBundleBaseDownloadingURL(string bundleName)
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
            var baseName = assetBundleName;
            // Get base bundle name
            if(assetBundleName.Contains('.')) baseName = assetBundleName.Split('.')[0];

            if(UsesExternalBundleVariantResolutionMechanism(baseName)) return baseName;

            if(assetBundleName.Contains('.')) return assetBundleName;

            string[] bundlesWithVariant;
#if UNITY_EDITOR
            if(SimulatesAssetBundleInEditor) {
                bundlesWithVariant = AssetDatabase.GetAllAssetBundleNames().Where(x => x.Contains('.')).ToArray();
            }
            else
#endif
            {
                bundlesWithVariant = Manifest.GetAllAssetBundlesWithVariant();
            }

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for(int i = 0; i < bundlesWithVariant.Length; i++) {
                var curSplit = bundlesWithVariant[i].Split('.');
                var curBaseName = curSplit[0];
                var curVariant = curSplit[1];

                if(curBaseName != baseName) continue;

                int found = ActiveVariants == null ? -1 : Array.IndexOf(ActiveVariants, curVariant);

                // If there is no active variant found. We still want to use the first
                if(found == -1) found = int.MaxValue - 1;

                if(found < bestFit) {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if(bestFit == int.MaxValue - 1) {
                if(CurrentLogMode == LogMode.All) Log.Warning($"[AssetBundelLoader] Ambigious asset bundle variant chosen because there was no matching active variant: {bundlesWithVariant[bestFitIndex]}");
            }

            if(bestFitIndex != -1) return bundlesWithVariant[bestFitIndex];
            else return assetBundleName;
        }
        
        // Sets up download operation for the given asset bundle if it's not downloaded already.
        static protected bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAsDependency, bool isLoadingAssetBundleManifest)
        {
            // Already loaded.
            LoadedAssetBundle bundle = null;
            loadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if(bundle != null) {
                if(isLoadingAsDependency) bundle.ReferencedCount++;
                return true;
            }

            if(downloadingBundles.ContainsKey(assetBundleName)) {
                if(isLoadingAsDependency) downloadingBundles[assetBundleName]++;
                return true;
            }

            var bundleBaseDownloadingURL = GetAssetBundleBaseDownloadingURL(assetBundleName);

            if(bundleBaseDownloadingURL.ToLower().StartsWith("odr://")) {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                if(CurrentLogMode == LogMode.All) Log.Info($"[AssetBundleLoader] Requesting bundle {assetBundleName} through ODR");
                InProgressOperations.Add(new AssetBundleDownloadFromODROperation(assetBundleName));
#else
                new ApplicationException($"Can't load bundle {assetBundleName} through ODR: this Unity version or build target doesn't support it.");
#endif
            }
            else if(bundleBaseDownloadingURL.ToLower().StartsWith("res://")) {
#if ENABLE_IOS_APP_SLICING
                if(CurrentLogMode == LogMode.All) Log.Info($"[AssetBundleLoader] Requesting bundle {assetBundleName} through asset catalog");
                InProgressOperations.Add(new AssetBundleOpenFromAssetCatalogOperation(assetBundleName));
#else
                new ApplicationException($"Can't load bundle {assetBundleName} through asset catalog: this Unity version or build target doesn't support it.");
#endif
            }
            else {
                WWW download = null;

                if(!bundleBaseDownloadingURL.EndsWith("/")) {
                    bundleBaseDownloadingURL += "/";
                }

                var url = bundleBaseDownloadingURL + assetBundleName;

                // For manifest assetbundle, always download it as we don't have hash for it.
                if(isLoadingAssetBundleManifest) download = new WWW(url);
                else download = WWW.LoadFromCacheOrDownload(url, Manifest.GetAssetBundleHash(assetBundleName), 0);

                inProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, download));
            }
            downloadingBundles[assetBundleName] = 0;

            return false;
        }

        // Where we get all the dependencies and load them all.
        static protected void LoadDependencies(string assetBundleName)
        {
            if(Manifest == null) {
                Log.Error("[AssetBundleLoader] Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            var dependencyNames = Manifest.GetAllDependencies(assetBundleName);
            if(dependencyNames.Length == 0) return;

            for(int i = 0; i < dependencyNames.Length; i++) {
                dependencyNames[i] = RemapVariantName(dependencyNames[i]);
            }

            // Record and load all dependencies.
            dependencies.Add(assetBundleName, dependencyNames);
            for(int i = 0; i < dependencyNames.Length; i++) {
                LoadAssetBundleInternal(dependencyNames[i], true, false);
            }
        }

        /// <summary>
        /// Unloads assetbundle and its dependencies.
        /// </summary>
        static public void UnloadAssetBundle(string assetBundleName)
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
            if(SimulatesAssetBundleInEditor) return;
#endif
            assetBundleName = RemapVariantName(assetBundleName);

            UnloadAssetBundleInternal(assetBundleName);
        }

        static protected void UnloadDependencies(string assetBundleName)
        {
            string[] dependencyNames = null;
            if(!dependencies.TryGetValue(assetBundleName, out dependencyNames)) return;

            // Loop dependencies.
            foreach(var i in dependencyNames) {
                UnloadAssetBundleInternal(i);
            }

            dependencies.Remove(assetBundleName);
        }

        static protected void UnloadAssetBundleInternal(string assetBundleName)
        {
            string error;
            var bundle = GetLoadedAssetBundle(assetBundleName, out error);
            if(bundle == null) return;

            if(--bundle.ReferencedCount == 0) {
                bundle.Unload();
                loadedAssetBundles.Remove(assetBundleName);
                UnloadDependencies(assetBundleName);
                if(CurrentLogMode == LogMode.All) Log.Info($"[AssetBundelLoader] {assetBundleName} has been unloaded successfully");
            }
        }

        void Update()
        {
            // Update all in progress operations
            for(int i = 0; i < InProgressOperations.Count;) {
                var operation = InProgressOperations[i];
                if(operation.Update()) i++;
                else {
                    inProgressOperations.RemoveAt(i);
                    ProcessFinishedOperation(operation);
                }
            }
        }

        void ProcessFinishedOperation(AssetBundleLoadOperation operation)
        {
            var download = operation as AssetBundleDownloadOperation;
            if(download == null) return;

            if(string.IsNullOrEmpty(download.Error)) {
                if(downloadingBundles.ContainsKey(download.AssetBundleName)) {
                    download.AssetBundle.ReferencedCount += downloadingBundles[download.AssetBundleName];
                }
                loadedAssetBundles.Add(download.AssetBundleName, download.AssetBundle);
            }
            else {
                var msg = string.Format("Failed downloading bundle {0} from {1}: {2}", download.AssetBundleName, download.GetSourceURL(), download.Error);
                downloadingErrors.Add(download.AssetBundleName, msg);
            }

            downloadingBundles.Remove(download.AssetBundleName);
        }

        /// <summary>
        /// Starts a load operation for asset bundle.
        /// </summary>
        static public AssetBundleLoadOperation LoadBundleAsync(string assetBundleName)
        {
            if(CurrentLogMode == LogMode.All) Log.Info($"[AssetBundelLoader] Loading {assetBundleName} bundle");

            AssetBundleLoadOperation operation = null;
#if UNITY_EDITOR
            if(SimulatesAssetBundleInEditor) {
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
                if(assetPaths.Length == 0) {
                    Log.Error($"[AssetBundelLoader] There is no asset bundle named {assetBundleName}");
                }
                return null;
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName, true);
                operation = new AssetBundleLoadOperationFull(assetBundleName);

                inProgressOperations.Add(operation);
            }

            return operation;
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadAssetOperation<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            if(CurrentLogMode == LogMode.All) Log.Info($"[AssetBundelLoader] Loading {assetName} from {assetBundleName} bundle");

#if UNITY_EDITOR
            if(SimulatesAssetBundleInEditor) {
                assetBundleName = RemapVariantName(assetBundleName);
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
                if(assetPaths.Length == 0) {
                    Log.Error($"[AssetBundelLoader] There is no asset with name \"{assetName}\" in {assetBundleName}");
                    return null;
                }
                foreach(var i in assetPaths) {
                    var target = AssetDatabase.LoadAssetAtPath(i, typeof(T)) as T;
                    if(target) return new AssetBundleLoadAssetOperationSimulation<T>(target);
                }
                Log.Error($"[AssetBundelLoader] There is no asset with name \"{assetName}\" with type {typeof(T)} in {assetBundleName}");
                return null;
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName, false);
                var operation = new AssetBundleLoadAssetOperationFull<T>(assetBundleName, assetName);

                inProgressOperations.Add(operation);

                return operation;
            }
        }

        /// <summary>
        /// Starts a load operation for a level from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
        {
            if(CurrentLogMode == LogMode.All) Log.Info($"[AssetBundelLoader] Loading {levelName} from {assetBundleName} bundle");

#if UNITY_EDITOR
            if(SimulatesAssetBundleInEditor) {
                assetBundleName = RemapVariantName(assetBundleName);
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, levelName);
                if(assetPaths.Length == 0) {
                    Log.Error($"[AssetBundelLoader] There is no asset with name \"{levelName}\" in {assetBundleName}");
                    return null;
                }
                foreach(var i in assetPaths) {
                    if(AssetDatabase.GetMainAssetTypeAtPath(i) == typeof(Scene)) return new AssetBundleLoadLevelSimulationOperation(i, isAdditive);
                }
                Log.Error($"[AssetBundelLoader] There is no asset with name \"{levelName}\" with type {typeof(Scene)} in {assetBundleName}");
                return null;
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName, false);
                var operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);

                inProgressOperations.Add(operation);

                return operation;
            }
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
            case BuildTarget.iOS:
                return "iOS";
#if UNITY_TVOS
            case BuildTarget.tvOS:
                return "tvOS";
#endif
            case BuildTarget.WebGL:
                return "WebGL";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "StandaloneWindows";
            case BuildTarget.StandaloneOSX:
                return "StandaloneOSX";
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
                return "StandaloneWindows";
            case RuntimePlatform.OSXPlayer:
                return "StandaloneOSX";
                //return "StandaloneOSXIntel";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
            }
        }
    } // End of AssetBundleManager.
}