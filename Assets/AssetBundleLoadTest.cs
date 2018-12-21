using System.Collections;
using UnityEngine;
using CustomUnity;

namespace YourProjectNamespace
{
    public class AssetBundleLoadTest : MonoBehaviour
    {
        public string[] activeVariatns;

        IEnumerator Start()
        {
            AssetBundleLoader.ActiveVariants = activeVariatns;

            yield return AssetBundleLoader.Initialize();

            var op1 = AssetBundleLoader.LoadAssetAsync<GameObject>("a", "Sphere");
            var op2 = AssetBundleLoader.LoadAssetAsync<GameObject>("b", "Sphere");
            yield return new WaitForEnumerators(op1, op2);

            Instantiate(op1.Asset);
            Instantiate(op2.Asset);
        }
    }
}
