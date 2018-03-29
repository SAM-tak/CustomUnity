using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace CustomUnity
{
    internal class LaunchAssetBundleServer : ScriptableSingleton<LaunchAssetBundleServer>
    {
        const string kLocalAssetbundleServerMenu = "Assets/AssetBundles/Local AssetBundle Server";

        [SerializeField]
        int m_ServerPID = 0;

        [MenuItem(kLocalAssetbundleServerMenu)]
        public static void ToggleLocalAssetBundleServer()
        {
            bool isRunning = IsRunning();
            if(!isRunning) {
                Run();
            }
            else {
                KillRunningAssetBundleServer();
            }
        }

        [MenuItem(kLocalAssetbundleServerMenu, true)]
        public static bool ToggleLocalAssetBundleServerValidate()
        {
            bool isRunnning = IsRunning();
            Menu.SetChecked(kLocalAssetbundleServerMenu, isRunnning);
            return true;
        }

        static bool IsRunning()
        {
            if(instance.m_ServerPID == 0) return false;

            try {
                var process = Process.GetProcessById(instance.m_ServerPID);
                if(process == null) return false;

                return !process.HasExited;
            }
            catch {
                return false;
            }
        }

        static void KillRunningAssetBundleServer()
        {
            // Kill the last time we ran
            try {
                if(instance.m_ServerPID == 0) return;

                var lastProcess = Process.GetProcessById(instance.m_ServerPID);
                lastProcess.Kill();
                instance.m_ServerPID = 0;
            }
            catch {
            }
        }

        static void Run()
        {
            var pathToAssetServer = Path.GetFullPath("Assets/CustomUnity/Editor/AssetBundleServer.exe");
            var assetBundlesDirectory = Path.Combine(Environment.CurrentDirectory, "AssetBundles");

            KillRunningAssetBundleServer();

            AssetBundleBuildScript.CreateAssetBundleDirectory();
            AssetBundleBuildScript.WriteServerURL();

            var args = assetBundlesDirectory;
            args = string.Format("\"{0}\" {1}", args, Process.GetCurrentProcess().Id);
            var startInfo = GetProfileStartInfoForMono(GetMonoInstallation(), GetMonoProfileVersion(), pathToAssetServer, args, true);
            startInfo.WorkingDirectory = assetBundlesDirectory;
            startInfo.UseShellExecute = false;
            var launchProcess = Process.Start(startInfo);
            if(launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0) {
                //Unable to start process
                UnityEngine.Debug.LogError("Unable Start AssetBundleServer process");
            }
            else {
                //We seem to have launched, let's save the PID
                instance.m_ServerPID = launchProcess.Id;
            }
        }

        static bool UpperVersion(string testee, string version)
        {
            var a = testee.Split('.').Select(int.Parse).ToArray();
            var b = version.Split('.').Select(int.Parse).ToArray();
            for(int i = 0; i < a.Length && i < b.Length; ++i) {
                if(a[i] > b[i]) return true;
            }
            return a.Length > b.Length;
        }

        static string GetMonoProfileVersion()
        {
            var path = Path.Combine(GetMonoInstallation(), "lib", "mono");

            var foldersWithApi = Directory.GetDirectories(path).Where(f => f.Contains("-api")).ToArray();
            var profileVersion = "1.0";

            for(int i = 0; i < foldersWithApi.Length; i++) {
                foldersWithApi[i] = Path.GetFileName(foldersWithApi[i]).Split('-').First();
                
                if(UpperVersion(foldersWithApi[i], profileVersion)) {
                    profileVersion = foldersWithApi[i];
                }
            }

            return profileVersion;
        }

        public static string GetFrameWorksFolder()
        {
            var editorAppPath = EditorApplication.applicationPath;
            if(Application.platform == RuntimePlatform.OSXEditor) {
                return Path.Combine(editorAppPath, "Contents");
            }
            else {
                return Path.Combine(Path.GetDirectoryName(editorAppPath), "Data");
            }
        }

        public static string GetProfileDirectory(BuildTarget target, string profile)
        {
            var monoprefix = GetMonoInstallation();
            return Path.Combine(monoprefix, "lib", "mono", profile);
        }

        public static string GetMonoInstallation()
        {
            return Path.Combine(GetFrameWorksFolder(), "MonoBleedingEdge");
        }

        private static readonly Regex UnsafeCharsWindows = new Regex(@"[^A-Za-z0-9_\-\.\:\,\/\@\\]");
        private static readonly Regex UnescapeableChars = new Regex(@"[\x00-\x08\x10-\x1a\x1c-\x1f\x7f\xff]");
        private static readonly Regex Quotes = new Regex(@"""");

        public ProcessStartInfo processStartInfo = null;

        public static string PrepareFileName(string input)
        {
            if(Application.platform == RuntimePlatform.OSXEditor) {
                return EscapeCharsQuote(input);
            }
            return EscapeCharsWindows(input);
        }

        public static string EscapeCharsQuote(string input)
        {
            if(input.IndexOf('\'') == -1) {
                return "'" + input + "'";
            }
            if(input.IndexOf('"') == -1) {
                return "\"" + input + "\"";
            }
            return null;
        }

        public static string EscapeCharsWindows(string input)
        {
            if(input.Length == 0) {
                return "\"\"";
            }
            if(UnescapeableChars.IsMatch(input)) {
                UnityEngine.Debug.LogWarning("Cannot escape control characters in string");
                return "\"\"";
            }
            if(UnsafeCharsWindows.IsMatch(input)) {
                return "\"" + Quotes.Replace(input, "\"\"") + "\"";
            }
            return input;
        }

        public static ProcessStartInfo GetProfileStartInfoForMono(string monodistribution, string profile, string executable, string arguments, bool setMonoEnvironmentVariables)
        {
            var monoexe = Path.Combine(monodistribution, "bin", "mono");
            var profileAbspath = Path.Combine(monodistribution, "lib", "mono", profile);
            if(Application.platform == RuntimePlatform.WindowsEditor) {
                monoexe = PrepareFileName(monoexe + ".exe");
            }

            var startInfo = new ProcessStartInfo {
                Arguments = PrepareFileName(executable) + " " + arguments,
                CreateNoWindow = true,
                FileName = monoexe,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Application.dataPath + "/..",
                UseShellExecute = false
            };

            if(setMonoEnvironmentVariables) {
                startInfo.EnvironmentVariables["MONO_PATH"] = profileAbspath;
                startInfo.EnvironmentVariables["MONO_CFG_DIR"] = Path.Combine(monodistribution, "etc");
            }
            return startInfo;
        }
    }
}
