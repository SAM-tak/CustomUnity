using System.Collections;
using UnityEngine;
using CustomUnity;

namespace YourProjectNamespace
{
    public class AssetBundleLoadTest : MonoBehaviour
    {
        IEnumerator Start()
        {
            AssetBundleManager.ActiveVariants = new string[] { "y" };

            yield return AssetBundleManager.Initialize();
            var obj1 = AssetBundleManager.LoadAssetAsync<GameObject>("a", "Sphere");
            yield return obj1;
            var obj2 = AssetBundleManager.LoadAssetAsync<GameObject>("b", "Sphere");
            yield return obj2;

            Instantiate(obj1.Asset);
            Instantiate(obj2.Asset);
        }
    }
}
