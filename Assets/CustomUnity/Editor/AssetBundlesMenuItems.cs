using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace CustomUnity
{
    public class AssetBundlesMenuItems
    {
        const string menuStringSimulationMode = "Assets/AssetBundles/Simulation Mode";

        [MenuItem(menuStringSimulationMode)]
        public static void ToggleSimulationMode()
        {
            AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
        }

        [MenuItem(menuStringSimulationMode, true)]
        public static bool ToggleSimulationModeValidate()
        {
            Menu.SetChecked(menuStringSimulationMode, AssetBundleManager.SimulateAssetBundleInEditor);
            return true;
        }

        [MenuItem("Assets/AssetBundles/Build AssetBundles")]
        static public void BuildAssetBundles()
        {
            AssetBundleBuildScript.BuildAssetBundles();
        }

        [MenuItem("Assets/AssetBundles/Build Player (for use with engine code stripping)")]
        static public void BuildPlayer()
        {
            AssetBundleBuildScript.BuildPlayer();
        }

        [MenuItem("Assets/AssetBundles/Build AssetBundles from Selection")]
        private static void BuildBundlesFromSelection()
        {
            // Get all selected *assets*
            var assets = Selection.objects.Where(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o))).ToArray();

            var assetBundleBuilds = new List<AssetBundleBuild>();
            var processedBundles = new HashSet<string>();

            // Get asset bundle names from selection
            foreach(var o in assets) {
                var assetPath = AssetDatabase.GetAssetPath(o);
                var importer = AssetImporter.GetAtPath(assetPath);

                if(importer == null) continue;

                // Get asset bundle name & variant
                var assetBundleName = importer.assetBundleName;
                var assetBundleVariant = importer.assetBundleVariant;
                var assetBundleFullName = string.IsNullOrEmpty(assetBundleVariant) ? assetBundleName : assetBundleName + "." + assetBundleVariant;

                // Only process assetBundleFullName once. No need to add it again.
                if(processedBundles.Contains(assetBundleFullName)) continue;

                processedBundles.Add(assetBundleFullName);
                
                assetBundleBuilds.Add(new AssetBundleBuild {
                    assetBundleName = assetBundleName,
                    assetBundleVariant = assetBundleVariant,
                    assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleFullName)
                });
            }

            AssetBundleBuildScript.BuildAssetBundles(assetBundleBuilds.ToArray());
        }
    }
}