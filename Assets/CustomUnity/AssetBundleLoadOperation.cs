using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_IOS_ON_DEMAND_RESOURCES
using UnityEngine.iOS;
#endif
using System.Collections;

namespace CustomUnity
{
    public abstract class AssetBundleLoadOperation : IEnumerator
    {
        public object Current { get { return null; } }

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
        }

        public virtual float Progress()
        {
            return IsDone() ? 1.0f : 0.0f;
        }

        abstract public bool Update();

        abstract public bool IsDone();
    }

    public abstract class AssetBundleDownloadOperation : AssetBundleLoadOperation
    {
        bool done;

        public string AssetBundleName { get; private set; }
        public LoadedAssetBundle AssetBundle { get; protected set; }
        public string Error { get; protected set; }

        protected abstract bool DownloadIsDone { get; }
        protected abstract void FinishDownload();

        public override bool Update()
        {
            if(!done && DownloadIsDone) {
                FinishDownload();
                done = true;
            }

            return !done;
        }

        public override bool IsDone()
        {
            return done;
        }

        public abstract string GetSourceURL();

        public AssetBundleDownloadOperation(string assetBundleName)
        {
            AssetBundleName = assetBundleName;
        }
    }

#if ENABLE_IOS_ON_DEMAND_RESOURCES
    // Read asset bundle asynchronously from iOS / tvOS asset catalog that is downloaded
    // using on demand resources functionality.
    public class AssetBundleDownloadFromODROperation : AssetBundleDownloadOperation
    {
        OnDemandResourcesRequest request;

        public AssetBundleDownloadFromODROperation(string assetBundleName) : base(assetBundleName)
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
                AssetBundle = new LoadedAssetBundle(bundle);
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
        public AssetBundleOpenFromAssetCatalogOperation(string assetBundleName) : base(assetBundleName)
        {
            var path = "res://" + assetBundleName;
            var bundle = UnityEngine.AssetBundle.LoadFromFile(path);
            if(bundle == null) Error = string.Format("Failed to load {0}", path);
            else AssetBundle = new LoadedAssetBundle(bundle);
        }

        protected override bool DownloadIsDone { get { return true; } }

        protected override void FinishDownload() { }

        public override string GetSourceURL()
        {
            return "res://" + AssetBundleName;
        }
    }
#endif

    public class AssetBundleDownloadFromWebOperation : AssetBundleDownloadOperation
    {
        WWW www;
        readonly string url;

        public AssetBundleDownloadFromWebOperation(string assetBundleName, WWW www) : base(assetBundleName)
        {
            if(www == null) throw new System.ArgumentNullException("www");
            url = www.url;
            this.www = www;
        }

        protected override bool DownloadIsDone { get { return (www == null) || www.isDone; } }

        protected override void FinishDownload()
        {
            Error = www.error;
            if(string.IsNullOrEmpty(Error)) {
                var bundle = www.assetBundle;
                if(bundle == null) Error = string.Format("{0} is not a valid asset bundle.", AssetBundleName);
                else AssetBundle = new LoadedAssetBundle(www.assetBundle);
            }
            www.Dispose();
            www = null;
        }

        public override string GetSourceURL()
        {
            return url;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : www != null ? www.progress : 0.0f;
        }
    }

#if UNITY_EDITOR
    public class AssetBundleLoadLevelSimulationOperation : AssetBundleLoadOperation
    {
        AsyncOperation operation = null;

        public AssetBundleLoadLevelSimulationOperation(string levelPath, bool isAdditive)
        {
            if(isAdditive) operation = UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode(levelPath);
            else operation = UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(levelPath);
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return operation == null || operation.isDone;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : operation != null ? operation.progress : 0.0f;
        }
    }
#endif

    public class AssetBundleLoadOperationFull : AssetBundleLoadOperation
    {
        protected string assetBundleName;
        protected string downloadingError;
        protected bool done;

        public AssetBundleLoadOperationFull(string assetBundleName)
        {
            this.assetBundleName = assetBundleName;
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            var bundle = AssetBundleLoader.GetLoadedAssetBundle(assetBundleName, out downloadingError);
            if(bundle != null || !string.IsNullOrEmpty(downloadingError)) {
                done = true;
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if(downloadingError != null) {
                Debug.LogError(downloadingError);
                return true;
            }

            return done;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : 0.0f;
        }
    }

    public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
    {
        protected string assetBundleName;
        protected string levelName;
        protected bool isAdditive;
        protected string downloadingError;
        protected AsyncOperation request;

        public AssetBundleLoadLevelOperation(string assetBundleName, string levelName, bool isAdditive)
        {
            this.assetBundleName = assetBundleName;
            this.levelName = levelName;
            this.isAdditive = isAdditive;
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if(request != null) return false;

            var bundle = AssetBundleLoader.GetLoadedAssetBundle(assetBundleName, out downloadingError);
            if(bundle != null) {
                request = SceneManager.LoadSceneAsync(levelName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                return false;
            }

            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if(request == null && downloadingError != null) {
                Debug.LogError(downloadingError);
                return true;
            }

            return request != null && request.isDone;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : request != null ? request.progress : 0.0f;
        }
    }

    public abstract class AssetBundleLoadAssetOperation<T> : AssetBundleLoadOperation where T : Object
    {
        public abstract T Asset { get; }
    }

    public class AssetBundleLoadAssetOperationSimulation<T> : AssetBundleLoadAssetOperation<T> where T : Object
    {
        protected T simulatedObject;

        public AssetBundleLoadAssetOperationSimulation(T simulatedObject)
        {
            this.simulatedObject = simulatedObject;
        }

        public override T Asset { get { return simulatedObject; } }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return true;
        }
    }

    public class AssetBundleLoadAssetOperationFull<T> : AssetBundleLoadAssetOperation<T> where T : Object
    {
        protected string assetBundleName;
        protected string assetName;
        protected string downloadingError;
        protected AssetBundleRequest request = null;

        public AssetBundleLoadAssetOperationFull(string assetBundleName, string assetName)
        {
            this.assetBundleName = assetBundleName;
            this.assetName = assetName;
        }

        public override T Asset {
            get {
                if(request != null && request.isDone) return request.asset as T;
                else return null;
            }
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if(request != null) return false;

            var bundle = AssetBundleLoader.GetLoadedAssetBundle(assetBundleName, out downloadingError);
            if(bundle != null) {
                request = bundle.AssetBundle.LoadAssetAsync(assetName, typeof(T));
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if(request == null && downloadingError != null) {
                Debug.LogError(downloadingError);
                return true;
            }

            return request != null && request.isDone;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : request.progress;
        }
    }

    public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull<AssetBundleManifest>
    {
        public AssetBundleLoadManifestOperation(string bundleName, string assetName) : base(bundleName, assetName)
        {
        }

        public override bool Update()
        {
            base.Update();

            if(request != null && request.isDone) {
                AssetBundleLoader.Manifest = Asset;
                return false;
            }
            return true;
        }
    }
}
