using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
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
        LoadSceneAsync()
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
        public bool IsImplicit { get; internal set; }

        internal event Action OnUnload;

        internal void Unload()
        {
            if(AssetBundle) AssetBundle.Unload(false);
            AssetBundle = null;
            OnUnload?.Invoke();
            OnUnload = null;
        }

        public LoadedAssetBundle(AssetBundle assetBundle, bool isImplicit)
        {
            AssetBundle = assetBundle;
            IsImplicit = isImplicit;
            ReferencedCount = 1;
        }
    }

    /// <summary>
    /// Class takes care of loading assetBundle and its dependencies
    /// automatically, loading variants automatically.
    /// </summary>
    public class AssetBundleLoader : MonoBehaviour
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        public const string DevelopmentAssetBundleServer = "http://127.0.0.1:7888";
#endif
#if UNITY_EDITOR
        static bool? simulatesAssetBundleInEditor;
        const string kSimulatesAssetBundles = nameof(AssetBundleLoader) + ".SimulatesAssetBundles";
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

        public const string kLocalAssetBundleServerURL = nameof(AssetBundleLoader) + ".LocalAssetBundleServerURL";
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

        static bool dirty = false;
#endif
        
        static readonly Dictionary<string, LoadedAssetBundle> loadedAssetBundles = new();
        static readonly Dictionary<string, string[]> dependencies = new();
        static readonly Dictionary<string, int> downloadingBundles = new (); // downloading name & refcount (start from 0)
        static readonly Dictionary<string, string> downloadingErrors = new ();
        static readonly List<AssetBundleLoadOperation> inProgressOperations = new ();

        public static ReadOnlyDictionary<string, LoadedAssetBundle> LoadedAssetBundles { get; } = new (loadedAssetBundles);
        public static ReadOnlyDictionary<string, string[]> Dependencies { get; } = new (dependencies);
        public static ReadOnlyDictionary<string, string> DownloadingErrors { get; } = new (downloadingErrors);
        public static ReadOnlyCollection<AssetBundleLoadOperation> InProgressOperations { get; } = new (inProgressOperations);

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

        /// <summary>
        /// Paused operation.
        /// </summary>
        /// to use if you want to push loading operation only to know total downloading size.
        public static bool Paused { get; set; }

        const int MaxParallelDownloadCount = 4;

        [SerializeField] bool foldoutManifest;
        [SerializeField] bool foldoutLoadedAssetBundles;
        [SerializeField] bool foldoutDownloadings;
        [SerializeField] bool foldoutAssetLoadings;

        public enum LogFlags
        {
            Level1 = 1,
            Level2 = 1 << 1,
            Level3 = 1 << 2,
            All = Level1 | Level2 | Level3,
            ErrorsAndWarnning = Level2 | Level3,
            JustErrors = Level3
        };

        public static LogFlags LogMode { get; set; } = LogFlags.All;
        static bool LogsInfo => (LogMode & LogFlags.Level1) > 0;
        static bool LogsWarn => (LogMode & LogFlags.Level2) > 0;
        static bool LogsErrr => (LogMode & LogFlags.Level3) > 0;
        
        /// <summary>
        /// Sets base downloading URL to a directory relative to the streaming assets directory.
        /// Asset bundles are loaded from a local directory.
        /// </summary>
        public static void SetSourceAssetBundleDirectory(string relativePath)
        {
            BaseDownloadingURL = Path.Combine(Application.streamingAssetsPath, relativePath);
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
            if(!absolutePath.EndsWith("/")) absolutePath += "/";
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
                if(LogsErrr) Log.Error("[AssetBundelLoader] Development Server URL could not be found.");
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

            loadedAssetBundles.TryGetValue(assetBundleName, out LoadedAssetBundle bundle);
            if(bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            if(!Dependencies.TryGetValue(assetBundleName, out string[] dependencies)) return bundle;

            // Make sure all dependencies are loaded
            foreach(var dependency in dependencies) {
                if(DownloadingErrors.TryGetValue(dependency, out error)) return null;

                // Wait all the dependent assetBundles being loaded.
                loadedAssetBundles.TryGetValue(dependency, out LoadedAssetBundle dependentBundle);
                if(dependentBundle == null) return null;
            }

            return bundle;
        }

        /// <summary>
        /// Returns true if certain asset bundle has been loaded without checking
        /// whether the dependencies have been loaded.
        /// </summary>
        static public bool IsAssetBundleLoaded(string assetBundleName)
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
            if(LogsInfo) Log.Info($"[AssetBundelLoader] Simulation Mode: {(SimulatesAssetBundleInEditor ? "Enabled" : "Disabled")}");
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
            if(LogsInfo) Log.Info($"[AssetBundelLoader] Loading Asset Bundle {(isLoadingAssetBundleManifest ? "Manifest" : "")} : {assetBundleName}");

#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
            if(SimulatesAssetBundleInEditor) return;
#endif

            if(!isLoadingAssetBundleManifest) {
                if(Manifest == null) {
                    if(LogsErrr) Log.Error("[AssetBundelLoader] Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
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
                if(Manifest == null) {
                    //Log.Error("[AssetBundelLoader] Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                    return assetBundleName;
                }
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
                if(LogsWarn) Log.Warning($"[AssetBundelLoader] Ambigious asset bundle variant chosen because there was no matching active variant: {bundlesWithVariant[bestFitIndex]}");
            }

            if(bestFitIndex != -1) return bundlesWithVariant[bestFitIndex];
            else return assetBundleName;
        }
        
        // Sets up download operation for the given asset bundle if it's not downloaded already.
        static protected bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAsDependency, bool isLoadingAssetBundleManifest)
        {
            // Already loaded.
            loadedAssetBundles.TryGetValue(assetBundleName, out LoadedAssetBundle bundle);
            if(bundle != null) {
                if(isLoadingAsDependency) bundle.ReferencedCount++;
                else if(bundle.IsImplicit) {
                    bundle.IsImplicit = false;
                    bundle.ReferencedCount++;
                }
                return true;
            }

            if(downloadingBundles.ContainsKey(assetBundleName)) {
                if(isLoadingAsDependency) downloadingBundles[assetBundleName]++;
                return true;
            }

            if(downloadingErrors.ContainsKey(assetBundleName)) downloadingErrors.Remove(assetBundleName);
            var bundleBaseDownloadingURL = GetAssetBundleBaseDownloadingURL(assetBundleName);

            if(bundleBaseDownloadingURL.ToLower().StartsWith("odr://")) {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                if(LogMode == LogFlags.All) Log.Info($"[AssetBundleLoader] Requesting bundle {assetBundleName} through ODR");
                inProgressOperations.Add(new AssetBundleDownloadFromODROperation(assetBundleName, isLoadingAsDependency));
#else
                new ApplicationException($"Can't load bundle {assetBundleName} through ODR: this Unity version or build target doesn't support it.");
#endif
            }
            else if(bundleBaseDownloadingURL.ToLower().StartsWith("res://")) {
#if ENABLE_IOS_APP_SLICING
                if(LogMode == LogFlags.All) Log.Info($"[AssetBundleLoader] Requesting bundle {assetBundleName} through asset catalog");
                inProgressOperations.Add(new AssetBundleOpenFromAssetCatalogOperation(assetBundleName, isLoadingAsDependency));
#else
                new ApplicationException($"Can't load bundle {assetBundleName} through asset catalog: this Unity version or build target doesn't support it.");
#endif
            }
#if UNITY_EDITOR || !UNITY_WEBGL
            else if(bundleBaseDownloadingURL.ToLower().StartsWith(Application.streamingAssetsPath.ToLower() + "/")) {
                if(!bundleBaseDownloadingURL.EndsWith("/")) {
                    bundleBaseDownloadingURL += "/";
                }

                var path = bundleBaseDownloadingURL + assetBundleName;
                inProgressOperations.Add(new AssetBundleLoadFromFileOperation(assetBundleName, isLoadingAsDependency, path));
            }
#endif
            else {
                if(!bundleBaseDownloadingURL.EndsWith("/")) {
                    bundleBaseDownloadingURL += "/";
                }

                var url = bundleBaseDownloadingURL + assetBundleName;

                // For manifest assetbundle, always download it as we don't have hash for it.
                var download = isLoadingAssetBundleManifest ?
                    UnityWebRequestAssetBundle.GetAssetBundle(url) :
                    UnityWebRequestAssetBundle.GetAssetBundle(url, Manifest.GetAssetBundleHash(assetBundleName), 0);

                inProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, isLoadingAsDependency, download));
            }
            downloadingBundles[assetBundleName] = 0;

            return false;
        }

        // Where we get all the dependencies and load them all.
        static protected void LoadDependencies(string assetBundleName)
        {
            if(Manifest == null) {
                if(LogsErrr) Log.Error("[AssetBundleLoader] Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
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
            dirty = true;
#endif
            assetBundleName = RemapVariantName(assetBundleName);

            UnloadAssetBundleInternal(assetBundleName, false);
        }

        static protected void UnloadDependencies(string assetBundleName)
        {
            if(!dependencies.TryGetValue(assetBundleName, out string[] dependencyNames)) return;

            // Loop dependencies.
            foreach(var i in dependencyNames) {
                UnloadAssetBundleInternal(i, true);
            }

            dependencies.Remove(assetBundleName);
        }

        static protected void UnloadAssetBundleInternal(string assetBundleName, bool isImplicit)
        {
            var bundle = GetLoadedAssetBundle(assetBundleName, out _);
            if(bundle == null) return;

            if(isImplicit) --bundle.ReferencedCount;
            else if(!bundle.IsImplicit) {
                bundle.IsImplicit = true;
                --bundle.ReferencedCount;
            }

            if(bundle.ReferencedCount == 0) {
                bundle.Unload();
                loadedAssetBundles.Remove(assetBundleName);
                UnloadDependencies(assetBundleName);
                if(LogsInfo) Log.Info($"[AssetBundelLoader] {assetBundleName} has been unloaded successfully");
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            if(InProgressOperations.Count > 0 || dirty) {
                dirty = false;
                EditorUtility.SetDirty(this);
            }
#endif
            if(!Paused) {
                // Update all in progress operations
                for(int i = 0; i < InProgressOperations.Count && i < MaxParallelDownloadCount;) {
                    var operation = InProgressOperations[i];
                    if(operation.Update()) i++;
                    else {
                        inProgressOperations.RemoveAt(i);
                        ProcessFinishedOperation(operation);
                    }
                }
            }
        }

        void ProcessFinishedOperation(AssetBundleLoadOperation operation)
        {
            if(operation is not AssetBundleDownloadOperation download) return;

            if(string.IsNullOrEmpty(download.Error)) {
                if(downloadingBundles.ContainsKey(download.AssetBundleName)) {
                    download.AssetBundle.ReferencedCount += downloadingBundles[download.AssetBundleName];
                }
                loadedAssetBundles.Add(download.AssetBundleName, download.AssetBundle);
            }
            else {
                downloadingErrors.Add(download.AssetBundleName, $"Failed downloading bundle {download.AssetBundleName} from {download.GetSourceURL()}: {download.Error}");
            }

            downloadingBundles.Remove(download.AssetBundleName);
        }

        /// <summary>
        /// Starts a load operation for asset bundle.
        /// </summary>
        static public AssetBundleLoadOperation LoadAsync(string assetBundleName)
        {
            if(LogsInfo) Log.Info($"[AssetBundelLoader] Loading {assetBundleName} bundle");

            AssetBundleLoadOperation operation = null;
#if UNITY_EDITOR
            if(SimulatesAssetBundleInEditor) {
                assetBundleName = RemapVariantName(assetBundleName);
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
                if(assetPaths.Length == 0) {
                    if(LogsErrr) Log.Error($"[AssetBundelLoader] There is no asset bundle named {assetBundleName}");
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
        static public AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, Type type)
        {
            if(LogsInfo) Log.Info($"[AssetBundelLoader] Loading {assetName} from {assetBundleName} bundle");

#if UNITY_EDITOR
            if(SimulatesAssetBundleInEditor) {
                assetBundleName = RemapVariantName(assetBundleName);
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
                if(assetPaths.Length == 0) {
                    if(LogsErrr) Log.Error($"[AssetBundelLoader] There is no asset with name \"{assetName}\" in {assetBundleName}");
                    return null;
                }
                foreach(var i in assetPaths) {
                    var target = AssetDatabase.LoadAssetAtPath(i, type);
                    if(target) return new AssetBundleLoadAssetOperationSimulation(target);
                }
                if(LogsErrr) Log.Error($"[AssetBundelLoader] There is no asset with name \"{assetName}\" with type {type} in {assetBundleName}");
                return null;
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName, false);
                var operation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);

                inProgressOperations.Add(operation);

                return operation;
            }
        }
        
        public struct AssetLoadOperation<T> : IEnumerator, IEquatable<AssetLoadOperation<T>> where T : UnityEngine.Object
        {
            public AssetLoadOperation(AssetBundleLoadAssetOperation operation)
            {
                Operation = operation;
            }
            
            public AssetBundleLoadAssetOperation Operation { get; private set; }

            public readonly T Asset => Operation?.Result as T;

            public readonly object Current => Operation?.Current;

            public readonly bool MoveNext() => Operation?.MoveNext() ?? false;

            public readonly void Reset() => Operation?.Reset();
            
            public readonly bool IsDone() => Operation?.IsDone() ?? true;

            public override readonly bool Equals(object obj) => obj is AssetLoadOperation<T> operation && Equals(operation);

            public readonly bool Equals(AssetLoadOperation<T> other) => EqualityComparer<AssetBundleLoadAssetOperation>.Default.Equals(Operation, other.Operation);

            public override readonly int GetHashCode() => HashCode.Combine(Operation);
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.(Type specified)
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static public AssetLoadOperation<T> LoadAssetAsync<T>(string assetBundleName, string assetName) where T : UnityEngine.Object
        {
            return new AssetLoadOperation<T>(LoadAssetAsync(assetBundleName, assetName, typeof(T)));
        }

        /// <summary>
        /// Starts a load operation for a level from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadOperation LoadSceneAsync(string assetBundleName, string sceneName, LoadSceneMode loadSceneMode)
        {
            if(LogsInfo) Log.Info($"[AssetBundelLoader] Loading {sceneName} from {assetBundleName} bundle");

#if UNITY_EDITOR
            if(SimulatesAssetBundleInEditor) {
                assetBundleName = RemapVariantName(assetBundleName);
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, sceneName);
                if(assetPaths.Length == 0) {
                    if(LogsErrr) Log.Error($"[AssetBundelLoader] There is no scene with name \"{sceneName}\" in {assetBundleName}");
                    return null;
                }
                foreach(var i in assetPaths) {
                    if(AssetDatabase.GetMainAssetTypeAtPath(i) == typeof(Scene)) return new AssetBundleLoadSceneSimulationOperation(i, loadSceneMode);
                }
                if(LogsErrr) Log.Error($"[AssetBundelLoader] There is no scene with name \"{sceneName}\" with type {typeof(Scene)} in {assetBundleName}");
                return null;
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName, false);
                var operation = new AssetBundleLoadSceneOperation(assetBundleName, sceneName, loadSceneMode);

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
        static string GetPlatformForAssetBundles(BuildTarget target)
        {
            return target switch {
                BuildTarget.Android => "Android",
                BuildTarget.iOS => "iOS",
#if UNITY_TVOS
                BuildTarget.tvOS => "tvOS",
#endif
                BuildTarget.WebGL => "WebGL",
                BuildTarget.StandaloneWindows => "StandaloneWindows",
                BuildTarget.StandaloneWindows64 => "StandaloneWindows",
                BuildTarget.StandaloneOSX => "StandaloneOSX",
                BuildTarget.StandaloneLinux64 => "StandaloneLinux",
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                _ => null
            };
        }
#else
        static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            return platform switch {
                RuntimePlatform.Android => "Android",
                RuntimePlatform.IPhonePlayer => "iOS",
#if UNITY_TVOS
                RuntimePlatform.tvOS => "tvOS",
#endif
                RuntimePlatform.WebGLPlayer => "WebGL",
                RuntimePlatform.WindowsPlayer => "StandaloneWindows",
                RuntimePlatform.OSXPlayer => "StandaloneOSX",
                RuntimePlatform.LinuxPlayer => "StandaloneLinux",
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                _ => null,
            };
        }
#endif
    } // End of AssetBundleManager.
}