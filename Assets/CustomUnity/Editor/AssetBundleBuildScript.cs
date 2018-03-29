using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CustomUnity
{
    public class AssetBundleBuildScript
    {
        public static string overloadedDevelopmentServerURL = "";

        static public string CreateAssetBundleDirectory()
        {
            // Choose the output path according to the build target.
            var outputPath = Path.Combine(AssetBundleManager.AssetBundlesOutputPath, AssetBundleManager.GetPlatformName());
            if(!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            return outputPath;
        }

        public static void BuildAssetBundles()
        {
            BuildAssetBundles(null);
        }

        public static void BuildAssetBundles(AssetBundleBuild[] builds)
        {
            // Choose the output path according to the build target.
            var outputPath = CreateAssetBundleDirectory();

            var options = BuildAssetBundleOptions.None;

            var shouldCheckODR = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
#if UNITY_TVOS
            shouldCheckODR |= EditorUserBuildSettings.activeBuildTarget == BuildTarget.tvOS;
#endif
            if(shouldCheckODR) {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                if(PlayerSettings.iOS.useOnDemandResources) options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
#if ENABLE_IOS_APP_SLICING
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
            }

            if(builds == null || builds.Length == 0) {
                //@TODO: use append hash... (Make sure pipeline works correctly with it.)
                BuildPipeline.BuildAssetBundles(outputPath, options, EditorUserBuildSettings.activeBuildTarget);
            }
            else {
                BuildPipeline.BuildAssetBundles(outputPath, builds, options, EditorUserBuildSettings.activeBuildTarget);
            }
        }

        public static void WriteServerURL()
        {
            var downloadURL = overloadedDevelopmentServerURL;
            if(string.IsNullOrEmpty(downloadURL)) {
                var localIP = "";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach(var ip in host.AddressList) {
                    if(ip.AddressFamily == AddressFamily.InterNetwork) {
                        localIP = ip.ToString();
                        break;
                    }
                }
                downloadURL = "http://" + localIP + ":7888/";
            }

            var assetBundleManagerResourcesDirectory = "Assets/CustomUnity/Resources";
            var assetBundleUrlPath = Path.Combine(assetBundleManagerResourcesDirectory, "AssetBundleServerURL.bytes");
            Directory.CreateDirectory(assetBundleManagerResourcesDirectory);
            File.WriteAllText(assetBundleUrlPath, downloadURL);
            AssetDatabase.Refresh();
        }

        public static void BuildPlayer()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if(outputPath.Length == 0) return;

            var levels = GetLevelsFromBuildSettings();
            if(levels.Length == 0) {
                Debug.Log("Nothing to build.");
                return;
            }

            var targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if(targetName == null) return;

            // Build and copy AssetBundles.
            BuildAssetBundles();
            WriteServerURL();

            BuildOptions option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
        }

        public static void BuildStandalonePlayer()
        {
            var outputPath = EditorUtility.SaveFolderPanel("Choose Location of the Built Game", "", "");
            if(outputPath.Length == 0) return;

            var levels = GetLevelsFromBuildSettings();
            if(levels.Length == 0) {
                Debug.Log("Nothing to build.");
                return;
            }

            var targetName = GetBuildTargetName(EditorUserBuildSettings.activeBuildTarget);
            if(targetName == null) return;

            // Build and copy AssetBundles.
            BuildAssetBundles();
            CopyAssetBundlesTo(Path.Combine(Application.streamingAssetsPath, AssetBundleManager.AssetBundlesOutputPath));
            AssetDatabase.Refresh();

            var option = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None;
            BuildPipeline.BuildPlayer(levels, outputPath + targetName, EditorUserBuildSettings.activeBuildTarget, option);
        }

        public static string GetBuildTargetName(BuildTarget target)
        {
            switch(target) {
            case BuildTarget.Android:
                return "/test.apk";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "/test.exe";
            case BuildTarget.StandaloneOSX:
                return "/test.app";
            case BuildTarget.WebGL:
            case BuildTarget.iOS:
                return "";
            // Add more build targets for your own.
            default:
                Debug.Log("Target not implemented.");
                return null;
            }
        }

        static void CopyAssetBundlesTo(string outputPath)
        {
            // Clear streaming assets folder.
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
            Directory.CreateDirectory(outputPath);

            var outputFolder = AssetBundleManager.GetPlatformName();

            // Setup the source folder for assetbundles.
            var source = Path.Combine(Path.Combine(System.Environment.CurrentDirectory, AssetBundleManager.AssetBundlesOutputPath), outputFolder);
            if(!Directory.Exists(source)) Debug.Log("No assetBundle output folder, try to build the assetBundles first.");

            // Setup the destination folder for assetbundles.
            var destination = Path.Combine(outputPath, outputFolder);
            if(Directory.Exists(destination)) FileUtil.DeleteFileOrDirectory(destination);

            FileUtil.CopyFileOrDirectory(source, destination);
        }

        static string[] GetLevelsFromBuildSettings()
        {
            var levels = new List<string>();
            for(int i = 0; i < EditorBuildSettings.scenes.Length; ++i) {
                if(EditorBuildSettings.scenes[i].enabled) levels.Add(EditorBuildSettings.scenes[i].path);
            }
            return levels.ToArray();
        }

        static string GetAssetBundleManifestFilePath()
        {
            var relativeAssetBundlesOutputPathForPlatform = Path.Combine(AssetBundleManager.AssetBundlesOutputPath, AssetBundleManager.GetPlatformName());
            return Path.Combine(relativeAssetBundlesOutputPathForPlatform, AssetBundleManager.GetPlatformName()) + ".manifest";
        }
    }
}