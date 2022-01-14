using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace CustomUnity
{
    internal class LaunchAssetBundleServer : ScriptableSingleton<LaunchAssetBundleServer>
    {
        const string AssetBundlesOutputPath = "AssetBundles";

        const string LocalAssetbundleServerMenuString = "Assets/AssetBundles/Local AssetBundle Server";

        [SerializeField]
        int serverPID = 0;

        [SerializeField]
        string assetBundlesDirectory = Path.Combine(Environment.CurrentDirectory, AssetBundlesOutputPath);

        [MenuItem(LocalAssetbundleServerMenuString)]
        static public void ToggleLocalAssetBundleServer()
        {
            if(IsRunning()) KillRunningAssetBundleServer();
            else Run();
        }

        [MenuItem(LocalAssetbundleServerMenuString, true)]
        static public bool ToggleLocalAssetBundleServerValidate()
        {
            var isRunning = IsRunning();
            if(!isRunning) EditorPrefs.SetString(AssetBundleLoader.kLocalAssetBundleServerURL, null);
            Menu.SetChecked(LocalAssetbundleServerMenuString, isRunning);
            return true;
        }
        
        static bool IsRunning()
        {
            if(instance.serverPID == 0) return false;
            try {
                var process = Process.GetProcessById(instance.serverPID);
                if(process != null && !process.HasExited) return true;
                instance.serverPID = 0;
                return false;
            }
            catch {
                instance.serverPID = 0;
                return false;
            }
        }

        static void KillRunningAssetBundleServer()
        {
            EditorApplication.quitting -= KillRunningAssetBundleServer;

            if(instance.serverPID == 0) return;
            EditorPrefs.SetString(AssetBundleLoader.kLocalAssetBundleServerURL, null);
            // Kill the last time we ran
            try {
                var lastProcess = Process.GetProcessById(instance.serverPID);
                lastProcess.Kill();
                instance.serverPID = 0;
            }
            catch(Exception ex) {
                instance.serverPID = 0;
                Log.Exception(ex);
            }
        }
        
        static void Run()
        {
            if(!Directory.Exists(instance.assetBundlesDirectory)) {
                instance.assetBundlesDirectory = Path.Combine(Environment.CurrentDirectory, AssetBundlesOutputPath);
            }
            var assetBundlesDirectory = EditorUtility.OpenFolderPanel(
                "Select AssetBundles Directory",
                Path.GetDirectoryName(instance.assetBundlesDirectory), 
                Path.GetFileName(instance.assetBundlesDirectory));
            if(string.IsNullOrEmpty(assetBundlesDirectory)) return;
            
            instance.assetBundlesDirectory = assetBundlesDirectory;

            KillRunningAssetBundleServer();
            
            var pathToAssetServer = Path.GetFullPath("Assets/CustomUnity/Editor/AssetBundleServer.exe");
            var args = string.Format("\"{0}\" {1}", assetBundlesDirectory, Process.GetCurrentProcess().Id);
            var startInfo = GetProfileStartInfoForMono(GetMonoInstallation(), GetMonoProfileVersion(), pathToAssetServer, assetBundlesDirectory, args);
            var launchProcess = Process.Start(startInfo);
            if(launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0) {
                //Unable to start process
                UnityEngine.Debug.LogError("Unable Start AssetBundleServer process");
            }
            else {
                //We seem to have launched, let's save the PID
                instance.serverPID = launchProcess.Id;

                var localIP = "localhost";
                try {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach(var ip in host.AddressList) {
                        if(ip.AddressFamily == AddressFamily.InterNetwork) {
                            localIP = ip.ToString();
                            break;
                        }
                    }
                }
                catch {
                    localIP = "localhost";
                }
                EditorPrefs.SetString(AssetBundleLoader.kLocalAssetBundleServerURL, "http://" + localIP + ":7888/");
                Log.Info($"Started AssetBundleServer : http://{localIP}:7888/");

                if(startInfo.RedirectStandardOutput) {
                    launchProcess.OutputDataReceived += (sender, eventArgs) => {
                        if(eventArgs.Data != null) Log.Info($"<color=grey>[AssetBundleServer]</color>{eventArgs.Data}");
                    };
                    launchProcess.BeginOutputReadLine();
                }
                if(startInfo.RedirectStandardError) {
                    launchProcess.ErrorDataReceived += (sender, eventArgs) => {
                        if(eventArgs.Data != null) Log.Error($"<color=grey>[AssetBundleServer]</color>{eventArgs.Data}");
                    };
                    launchProcess.BeginErrorReadLine();
                }
                if(startInfo.RedirectStandardOutput || startInfo.RedirectStandardError) {
                    EditorApplication.quitting += KillRunningAssetBundleServer;
                }
            }
        }
        
        static string GetMonoProfileVersion()
        {
            var path = Path.Combine(GetMonoInstallation(), "lib", "mono");

            var profileVersion = "1.0";
            foreach(var i in Directory.GetDirectories(path).Where(f => f.Contains("-api")).Select(x => Path.GetFileName(x).Split('-').First())) {
                var a = i.Split('.').Select(int.Parse);
                var b = profileVersion.Split('.').Select(int.Parse);
                if(a.Zip(b, (x, y) => x > y).Any() || a.Count() > b.Count()) profileVersion = i;
            }

            return profileVersion;
        }
        
        static string GetMonoInstallation()
        {
            var editorAppPath = EditorApplication.applicationPath;
            if(Application.platform == RuntimePlatform.OSXEditor) {
                return Path.Combine(editorAppPath, "Contents", "MonoBleedingEdge");
            }
            else {
                return Path.Combine(Path.GetDirectoryName(editorAppPath), "Data", "MonoBleedingEdge");
            }
        }

        static readonly Regex UnsafeCharsWindows = new (@"[^A-Za-z0-9_\-\.\:\,\/\@\\]");
        static readonly Regex UnescapeableChars = new (@"[\x00-\x08\x10-\x1a\x1c-\x1f\x7f\xff]");
        static readonly Regex Quotes = new (@"""");
        
        static string PrepareFileName(string input)
        {
            if(Application.platform == RuntimePlatform.OSXEditor) {
                return EscapeCharsQuote(input);
            }
            return EscapeCharsWindows(input);
        }

        static string EscapeCharsQuote(string input)
        {
            if(input.IndexOf('\'') == -1) {
                return "'" + input + "'";
            }
            if(input.IndexOf('"') == -1) {
                return "\"" + input + "\"";
            }
            return null;
        }

        static string EscapeCharsWindows(string input)
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

        static ProcessStartInfo GetProfileStartInfoForMono(string monodistribution, string profile, string executable, string workingDirectory, string arguments)
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
                RedirectStandardError = Application.platform != RuntimePlatform.WindowsEditor,
                RedirectStandardOutput = Application.platform != RuntimePlatform.WindowsEditor,
                WorkingDirectory = workingDirectory,
                UseShellExecute = Application.platform == RuntimePlatform.WindowsEditor
            };

            if(!startInfo.UseShellExecute) {
                startInfo.EnvironmentVariables["MONO_PATH"] = profileAbspath;
                startInfo.EnvironmentVariables["MONO_CFG_DIR"] = Path.Combine(monodistribution, "etc");
            }
            return startInfo;
        }
    }
}
