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
        WWW m_WWW;
        string m_Url;

        public AssetBundleDownloadFromWebOperation(string assetBundleName, WWW www) : base(assetBundleName)
        {
            if(www == null) throw new System.ArgumentNullException("www");
            m_Url = www.url;
            m_WWW = www;
        }

        protected override bool DownloadIsDone { get { return (m_WWW == null) || m_WWW.isDone; } }

        protected override void FinishDownload()
        {
            Error = m_WWW.error;
            if(string.IsNullOrEmpty(Error)) {
                var bundle = m_WWW.assetBundle;
                if(bundle == null) Error = string.Format("{0} is not a valid asset bundle.", AssetBundleName);
                else AssetBundle = new LoadedAssetBundle(m_WWW.assetBundle);
            }
            m_WWW.Dispose();
            m_WWW = null;
        }

        public override string GetSourceURL()
        {
            return m_Url;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : m_WWW != null ? m_WWW.progress : 0.0f;
        }
    }

#if UNITY_EDITOR
    public class AssetBundleLoadLevelSimulationOperation : AssetBundleLoadOperation
    {
        AsyncOperation m_Operation = null;

        public AssetBundleLoadLevelSimulationOperation(string assetBundleName, string levelName, bool isAdditive)
        {
            var levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, levelName);
            if(levelPaths.Length == 0) {
                ///@TODO: The error needs to differentiate that an asset bundle name doesn't exist
                //        from that there right scene does not exist in the asset bundle...
                Debug.LogError("There is no scene with name \"" + levelName + "\" in " + assetBundleName);
                return;
            }

            if(isAdditive) m_Operation = UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode(levelPaths[0]);
            else m_Operation = UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(levelPaths[0]);
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return m_Operation == null || m_Operation.isDone;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : m_Operation != null ? m_Operation.progress : 0.0f;
        }
    }
#endif

    public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
    {
        protected string m_AssetBundleName;
        protected string m_LevelName;
        protected bool m_IsAdditive;
        protected string m_DownloadingError;
        protected AsyncOperation m_Request;

        public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, bool isAdditive)
        {
            m_AssetBundleName = assetbundleName;
            m_LevelName = levelName;
            m_IsAdditive = isAdditive;
        }

        public override bool Update()
        {
            if(m_Request != null) return false;

            var bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if(bundle != null) {
                m_Request = SceneManager.LoadSceneAsync(m_LevelName, m_IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
                return false;
            }

            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if(m_Request == null && m_DownloadingError != null) {
                Debug.LogError(m_DownloadingError);
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : m_Request != null ? m_Request.progress : 0.0f;
        }
    }

    public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public abstract T GetAsset<T>() where T : Object;
    }

    public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
    {
        Object m_SimulatedObject;

        public AssetBundleLoadAssetOperationSimulation(Object simulatedObject)
        {
            m_SimulatedObject = simulatedObject;
        }

        public override T GetAsset<T>()
        {
            return m_SimulatedObject as T;
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return true;
        }
    }

    public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
    {
        protected string m_AssetBundleName;
        protected string m_AssetName;
        protected string m_DownloadingError;
        protected System.Type m_Type;
        protected AssetBundleRequest m_Request = null;

        public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, System.Type type)
        {
            m_AssetBundleName = bundleName;
            m_AssetName = assetName;
            m_Type = type;
        }

        public override T GetAsset<T>()
        {
            if(m_Request != null && m_Request.isDone) return m_Request.asset as T;
            else return null;
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if(m_Request != null) return false;

            var bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if(bundle != null) {
                ///@TODO: When asset bundle download fails this throws an exception...
                m_Request = bundle.m_AssetBundle.LoadAssetAsync(m_AssetName, m_Type);
                return false;
            }
            return true;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if(m_Request == null && m_DownloadingError != null) {
                Debug.LogError(m_DownloadingError);
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }

        public override float Progress()
        {
            return IsDone() ? 1.0f : m_Request.progress;
        }
    }

    public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
    {
        public AssetBundleLoadManifestOperation(string bundleName, string assetName, System.Type type) : base(bundleName, assetName, type)
        {
        }

        public override bool Update()
        {
            base.Update();

            if(m_Request != null && m_Request.isDone) {
                AssetBundleManager.Manifest = GetAsset<AssetBundleManifest>();
                return false;
            }
            return true;
        }
    }
}
