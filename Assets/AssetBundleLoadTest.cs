using System.Collections;
using UnityEngine;
using CustomUnity;

namespace YourProjectNamespace
{
    public class AssetBundleLoadTest : MonoBehaviour
    {
        IEnumerator Start()
        {
            AssetBundleLoader.ActiveVariants = new string[] { "y", "x" };

            yield return AssetBundleLoader.Initialize();

            var obj1 = AssetBundleLoader.LoadAssetAsync<GameObject>("a", "Sphere");
            yield return obj1;
            var obj2 = AssetBundleLoader.LoadAssetAsync<GameObject>("b", "Sphere");
            yield return obj2;

            Instantiate(obj1.Asset);
            Instantiate(obj2.Asset);
        }
    }
}
