using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
#if ENABLE_IOS_ON_DEMAND_RESOURCES
using UnityEngine.iOS;
#endif
using System.Collections;

namespace CustomUnity
{
    public abstract class AssetBundleLoadOperation : IEnumerator
    {
        public object Current => null;

        public bool MoveNext() => !IsDone();

        public void Reset() { }

        public virtual float Progress() => IsDone() ? 1.0f : 0.0f;

        abstract public bool Update();

        abstract public bool IsDone();
    }

    public abstract class AssetBundleDownloadOperation : AssetBundleLoadOperation
    {
        public string AssetBundleName { get; private set; }
        public bool IsImplicit { get; private set; }
        public LoadedAssetBundle AssetBundle { get; protected set; }
        public string Error { get; protected set; }

        protected abstract bool DownloadIsDone { get; }
        protected abstract void FinishDownload();

        bool done;

        public override bool Update()
        {
            if(!done && DownloadIsDone) {
                FinishDownload();
                done = true;
            }

            return !done;
        }

        public override bool IsDone() => done;

        public abstract string GetSourceURL();

        public AssetBundleDownloadOperation(string assetBundleName, bool isImplicit)
        {
            AssetBundleName = assetBundleName;
            IsImplicit = isImplicit;
        }
    }

#if ENABLE_IOS_ON_DEMAND_RESOURCES
    // Read asset bundle asynchronously from iOS / tvOS asset catalog that is downloaded
    // using on demand resources functionality.
    public class AssetBundleDownloadFromODROperation : AssetBundleDownloadOperation
    {
        OnDemandResourcesRequest request;

        public AssetBundleDownloadFromODROperation(string assetBundleName, bool isImplicit) : base(assetBundleName, isImplicit)
        {
            // Work around Xcode crash when opening Resources tab when a 
            // resource name contains slash character
            request = OnDemandResources.PreloadAsync(new string[] { assetBundleName.Replace('/', '>') });
        }

        protected override bool DownloadIsDone { get { return (request == null) || request.isDone; } }

        public override string GetSourceURL()
        {
            return "odr://" + AssetBundleName;
        }

        protected override void FinishDownload()
        {
            Error = request.error;
            if(Error != null) return;

            var path = "res://" + AssetBundleName;
            var bundle = UnityEngine.AssetBundle.LoadFromFile(path);
            if(bundle == null) {
                Error = string.Format("Failed to load {0}", path);
                request.Dispose();
            }
            else {
                AssetBundle = new LoadedAssetBundle(bundle, IsImplicit);
                // At the time of unload request is already set to null, so capture it to local variable.
                var localRequest = request;
                // Dispose of request only when bundle is unloaded to keep the ODR pin alive.
                AssetBundle.OnUnload += localRequest.Dispose;
            }

            request = null;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : request.progress;
        }
    }
#endif

#if ENABLE_IOS_APP_SLICING
    // Read asset bundle synchronously from an iOS / tvOS asset catalog
    public class AssetBundleOpenFromAssetCatalogOperation : AssetBundleDownloadOperation
    {
        public AssetBundleOpenFromAssetCatalogOperation(string assetBundleName, bool isImplicit) : base(assetBundleName, isImplicit)
        {
            var path = "res://" + assetBundleName;
            var bundle = UnityEngine.AssetBundle.LoadFromFile(path);
            if(bundle == null) Error = string.Format("Failed to load {0}", path);
            else AssetBundle = new LoadedAssetBundle(bundle, IsImplicit);
        }

        protected override bool DownloadIsDone { get { return true; } }

        protected override void FinishDownload() { }

        public override string GetSourceURL()
        {
            return "res://" + AssetBundleName;
        }
    }
#endif

    public class AssetBundleLoadFromFileOperation : AssetBundleDownloadOperation
    {
        AssetBundleCreateRequest request;
        readonly string path;

        public AssetBundleLoadFromFileOperation(string assetBundleName, bool isImplicit, string path) : base(assetBundleName, isImplicit)
        {
            this.path = path;
        }

        protected override bool DownloadIsDone => send ? (request?.isDone ?? true) : false;

        bool send;

        public override bool Update()
        {
            if(!send) {
                send = true;
                request = UnityEngine.AssetBundle.LoadFromFileAsync(path);
            }
            return base.Update();
        }

        protected override void FinishDownload()
        {
            var bundle = request.assetBundle;
            if(bundle == null) Error = string.Format("{0} is not a valid asset bundle.", AssetBundleName);
            else AssetBundle = new LoadedAssetBundle(bundle, IsImplicit);
            request = null;
        }

        public override string GetSourceURL() => path;

        public override float Progress() => IsDone() ? 1.0f : Mathf.Max(0, request?.progress ?? 0.0f);
    }

    public class AssetBundleDownloadFromWebOperation : AssetBundleDownloadOperation
    {
        UnityWebRequest webRequest;
        readonly string url;

        public AssetBundleDownloadFromWebOperation(string assetBundleName, bool isImplicit, UnityWebRequest webRequest) : base(assetBundleName, isImplicit)
        {
            if(webRequest == null) throw new System.ArgumentNullException("webRequest");
            this.webRequest = webRequest;
            url = webRequest.url;
        }

        protected override bool DownloadIsDone => send ? (webRequest?.isDone ?? true) : false;

        bool send;

        public override bool Update()
        {
            if(!send) {
                send = true;
                webRequest.SendWebRequest();
            }
            return base.Update();
        }

        protected override void FinishDownload()
        {
            Error = webRequest.error;
            if(string.IsNullOrEmpty(Error)) {
                var bundle = ((DownloadHandlerAssetBundle)webRequest.downloadHandler).assetBundle;
                if(bundle == null) Error = string.Format("{0} is not a valid asset bundle.", AssetBundleName);
                else AssetBundle = new LoadedAssetBundle(bundle, IsImplicit);
            }
            webRequest.Dispose();
            webRequest = null;
        }

        public override string GetSourceURL() => url;

        public override float Progress() => IsDone() ? 1.0f : Mathf.Max(0, webRequest?.downloadProgress ?? 0.0f);
    }

#if UNITY_EDITOR
    public class AssetBundleLoadSceneSimulationOperation : AssetBundleLoadOperation
    {
        AsyncOperation operation = null;

        public AssetBundleLoadSceneSimulationOperation(string levelPath, LoadSceneMode loadSceneMode)
        {
            operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(levelPath, new LoadSceneParameters(loadSceneMode));
        }

        public override bool Update() => false;

        public override bool IsDone() => operation == null || operation.isDone;

        public override float Progress() => IsDone() ? 1.0f : operation?.progress ?? 0.0f;
    }
#endif

    public class AssetBundleLoadOperationFull : AssetBundleLoadOperation
    {
        public string AssetBundleName { get; protected set; }
        protected string downloadingError;
        protected bool done;

        public AssetBundleLoadOperationFull(string assetBundleName)
        {
            AssetBundleName = assetBundleName;
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if(AssetBundleLoader.GetLoadedAssetBundle(AssetBundleName, out downloadingError) != null || !string.IsNullOrEmpty(downloadingError)) {
                done = true;
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if(!string.IsNullOrEmpty(downloadingError)) {
                Debug.LogError(downloadingError);
                return true;
            }

            return done;
        }
    }

    public class AssetBundleLoadSceneOperation : AssetBundleLoadOperation
    {
        protected string assetBundleName;
        protected string levelName;
        protected LoadSceneMode loadSceneMode;
        protected string downloadingError;
        protected AsyncOperation request;

        public AssetBundleLoadSceneOperation(string assetBundleName, string levelName, LoadSceneMode loadSceneMode)
        {
            this.assetBundleName = assetBundleName;
            this.levelName = levelName;
            this.loadSceneMode = loadSceneMode;
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if(request != null) return false;

            var bundle = AssetBundleLoader.GetLoadedAssetBundle(assetBundleName, out downloadingError);
            if(bundle != null) {
                request = SceneManager.LoadSceneAsync(levelName, loadSceneMode);
                return false;
            }

            return string.IsNullOrEmpty(downloadingError);
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if(request == null && !string.IsNullOrEmpty(downloadingError)) {
                Debug.LogError(downloadingError);
                return true;
            }

            return request != null && request.isDone;
        }

        public override float Progress() => IsDone() ? 1.0f : (request?.progress ?? 0.0f);
    }

    public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public abstract Object Asset { get; }
    }

    public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
    {
        protected Object simulatedObject;

        public AssetBundleLoadAssetOperationSimulation(Object simulatedObject)
        {
            this.simulatedObject = simulatedObject;
        }

        public override Object Asset => simulatedObject;

        public override bool Update() => false;

        public override bool IsDone() => true;
    }

    public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
    {
        public string AssetBundleName { get; protected set; }
        public string AssetName { get; protected set; }
        public System.Type Type { get; protected set; }
        protected string downloadingError;
        protected AssetBundleRequest request = null;

        public AssetBundleLoadAssetOperationFull(string assetBundleName, string assetName, System.Type type)
        {
            AssetBundleName = assetBundleName;
            AssetName = assetName;
            Type = type;
        }

        public override Object Asset => request?.isDone ?? false ? request?.asset : null;

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if(request != null) return false;

            var bundle = AssetBundleLoader.GetLoadedAssetBundle(AssetBundleName, out downloadingError);
            if(bundle != null) {
                request = bundle.AssetBundle.LoadAssetAsync(AssetName, Type);
                return false;
            }
            return string.IsNullOrEmpty(downloadingError);
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if(request == null && !string.IsNullOrEmpty(downloadingError)) {
                Debug.LogError(downloadingError);
                return true;
            }

            return request != null && request.isDone;
        }

        public override float Progress() => IsDone() ? 1.0f : (request?.progress ?? 0.0f);
    }

    public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
    {
        public AssetBundleLoadManifestOperation(string bundleName, string assetName) : base(bundleName, assetName, typeof(AssetBundleManifest))
        {
        }

        public override bool Update()
        {
            base.Update();

            if(request != null && request.isDone) {
                AssetBundleLoader.Manifest = Asset as AssetBundleManifest;
                return false;
            }
            return true;
        }
    }
}
