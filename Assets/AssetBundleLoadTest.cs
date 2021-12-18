using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using CustomUnity;

namespace YourProjectNamespace
{
    public class AssetBundleLoadTest : MonoBehaviour
    {
        public string[] activeVariants;

        [Serializable]
        public struct Address
        {
            public string bundleName;
            public string assetName;
        };

        public Address[] assets;

        IEnumerator Start()
        {
            AssetBundleLoader.ActiveVariants = activeVariants;

            yield return AssetBundleLoader.Initialize();

            AssetBundleLoader.Paused = true;

            var ops = new List<AssetBundleLoader.AssetLoadOperation<GameObject>>();
            foreach(var i in assets) ops.Add(AssetBundleLoader.LoadAssetAsync<GameObject>(i.bundleName, i.assetName));

            var headreqs = new List<UnityWebRequestAsyncOperation>();

            int totalDownloadSize = 0;
            var retries = new Dictionary<string, int>();
            var retryUrls = new List<string>();

            const int maxRetry = 5;
            void RetryHeadRequest()
            {
                retryUrls.Clear();
                foreach(var j in headreqs.Where(c => c.webRequest.result != UnityWebRequest.Result.Success)) {
                    LogError($"HEAD request error : {j.webRequest.error}");
                    retries[j.webRequest.url]++;
                    if(retries[j.webRequest.url] < maxRetry) retryUrls.Add(j.webRequest.url);
                }
                foreach(var j in headreqs.Where(c => c.webRequest.result == UnityWebRequest.Result.Success)) {
                    if(retries.ContainsKey(j.webRequest.url)) retries.Remove(j.webRequest.url);
                    totalDownloadSize += int.Parse(j.webRequest.GetResponseHeaders()["Content-Length"]);
                }
                headreqs.Clear();
                foreach(var j in retryUrls) headreqs.Add(UnityWebRequest.Head(j).SendWebRequest());
            }

            foreach(var i in AssetBundleLoader.InProgressOperations) {
                if(i is AssetBundleDownloadFromWebOperation op && !op.IsCached) {
                    while(headreqs.Count > 3) {
                        foreach(var req in headreqs) yield return req;
                        RetryHeadRequest();
                    }
                    headreqs.Add(UnityWebRequest.Head(op.GetSourceURL()).SendWebRequest());
                }
            }
            while(headreqs.Count > 0) {
                foreach(var req in headreqs) yield return req;
                RetryHeadRequest();
            }

            if(retries.Count > 0) {
                LogError($"Failed evaluate download size with network error.");
                yield break;
            }

            LogInfo($"totalDownloadSize {totalDownloadSize}");

            AssetBundleLoader.Paused = false;

            foreach(var op in ops) yield return op;
            foreach(var op in ops) Instantiate(op.Asset);
        }
    }
}