using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
//using UnityEngine.ResourceManagement.ResourceLocations;
using CustomUnity;

namespace YourProjectNamespace
{
    public class AddressablesLoadTest : MonoBehaviour
    {
        public string[] prefabSources = {
            "Assets/Bundles/a/Sphere.prefab",
            "Assets/Bundles/b/Sphere.prefab"
        };
        //public AssetReferenceGameObject[] prefabSources;

        IEnumerator Start()
        {
            LogInfo("start loading.");
            var ops = new List<AsyncOperationHandle<GameObject>>();
            foreach(var i in prefabSources) {
                ops.Add(Addressables.LoadAssetAsync<GameObject>(i));
            }
            yield return new WaitForEnumerators(ops.Select(i => (IEnumerator)i));
            LogInfo("done!");
            foreach(var i in ops) {
                if(i.Status == AsyncOperationStatus.Succeeded) {
                    LogInfo($"loaded {i.Result.name}");
                    Instantiate(i.Result);
                }
            }
        }
    }
}