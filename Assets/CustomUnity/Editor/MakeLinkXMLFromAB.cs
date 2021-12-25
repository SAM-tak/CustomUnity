using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CustomUnity
{
    public static class MakeLinkXMLFromAB
    {
        static string latestSelectedPath;
        const string kLatestSelectedPath = "MakeLinkXMLFromAB.LatestSelectedPath";

        /// <summary>
        /// Latest selected folder. will save to EditorPrefs.
        /// </summary>
        public static string LatestSelectedPath {
            get {
                if(latestSelectedPath == null) {
                    latestSelectedPath = EditorPrefs.GetString(kLatestSelectedPath, Path.Combine("AssetBundles", AssetBundleLoader.GetPlatformName()));
                }
                return latestSelectedPath;
            }
            set {
                if(latestSelectedPath != value) {
                    latestSelectedPath = value;
                    EditorPrefs.SetString(kLatestSelectedPath, value);
                }
            }
        }

        [MenuItem("Assets/AssetBundles/Make link.xml...")]
        static void MakeLinkXML()
        {
            LatestSelectedPath = EditorUtility.OpenFolderPanel("Specify AssetBundles root path", LatestSelectedPath, "");
            MakeLinkXML(LatestSelectedPath, "Assets/link.xml", true);
        }

        /// <summary>
        /// Make link.xml file.
        /// </summary>
        /// <param name="rootPath">top folder of assetbundles</param>
        /// <param name="outputPath">folder for save link.xml</param>
        /// <param name="showProgressBar">shows progressbar in processing</param>
        public static void MakeLinkXML(string rootPath, string outputPath, bool showProgressBar = false)
        {
            var systemScriptMatch = new Regex(@"\- Class: (\d+)\n  Script: \{instanceID: (\d+)\}", RegexOptions.Singleline);
            var userScriptMatch = new Regex(@"\- Class: (\d+)\n  Script: \{fileID: (\d+), guid: ([^,]+), type: (\d+)\}", RegexOptions.Singleline);
            var classSet = new HashSet<Type>();
            //var missingHashes = new HashSet<string>();
            var manifests = Directory.GetFiles(rootPath, "*.manifest", SearchOption.AllDirectories);
            int cnt = 0;
            foreach(var i in manifests) {
                cnt++;
                if(showProgressBar) EditorUtility.DisplayProgressBar("Make link.xml", i, cnt / (float)manifests.Length);

                var text = File.ReadAllText(i);

                foreach(Match match in systemScriptMatch.Matches(text)) {
                    var classid = int.Parse(match.Groups[1].Value);
                    var type = GetType(classid);
                    if(type != null) classSet.Add(type);
                }

                foreach(Match match in userScriptMatch.Matches(text)) {
                    //var classid = int.Parse(match.Groups[1].Value);
                    var fileid = int.Parse(match.Groups[2].Value);
                    var hash = match.Groups[3].ToString();
                    var path = AssetDatabase.GUIDToAssetPath(hash);
                    if(Path.GetExtension(path) == ".dll") {
                        var type = GetType(path, fileid);
                        if(type != null) classSet.Add(type);
                    }
                    else {
                        if(!string.IsNullOrEmpty(path)) {
                            var type = AssetDatabase.LoadAssetAtPath<MonoScript>(path).GetClass();
                            if(type != null) classSet.Add(type);
                        }
                        //else missingHashes.Add(hash);
                    }
                }
            }

            var linkInfo = new Dictionary<string, List<string>>();
            foreach(var i in classSet) {
                var asmname = i.Assembly.FullName.Split(',')[0];
                if(linkInfo.ContainsKey(asmname)) linkInfo[asmname].Add(i.FullName);
                else linkInfo[asmname] = new List<string> { i.FullName };
            }

            Debug.Log($"<color=red>write to {outputPath}</color>");
            using(var s = File.CreateText(outputPath)) {
                s.WriteLine("<linker>");
                foreach(var i in linkInfo) {
                    s.WriteLine($"    <assembly fullname=\"{i.Key}\">");
                    foreach(var j in i.Value) {
                        s.WriteLine($"        <type fullname=\"{j}\" preserve=\"all\"/>");
                    }
                    s.WriteLine("    </assembly>");
                }
                s.WriteLine("</linker>");
            }

            //foreach(var i in missingHashes) Debug.LogWarningFormat("Missing Reference {0}", i);
            if(showProgressBar) EditorUtility.ClearProgressBar();
        }

        static Type GetType(string path, int id)
        {
            var assembly = Assembly.LoadFile(path);
            var modules = assembly.GetLoadedModules();
            foreach(var module in modules) {
                foreach(var t in module.GetTypes()) {
                    if(t.IsSubclassOf(typeof(Component)) && FileIDUtil.Compute(t) == id) return t;
                }
            }
            return null;
        }

        static Type GetType(int classId)
        {
            var assembly = Assembly.GetAssembly(typeof(MonoScript));
            var unityType = assembly.GetType("UnityEditor.UnityType");
            var findTypeByPersistentTypeID = unityType.GetMethod("FindTypeByPersistentTypeID");
            var classObject = findTypeByPersistentTypeID.Invoke(null, new object[] { classId });
            if(classObject == null) return null;
            var nameProperty = classObject.GetType().GetProperty("name");
            return Assembly.GetAssembly(typeof(UnityEngine.Object)).GetType("UnityEngine." + nameProperty.GetValue(classObject, null));
        }

        #region MD4
        // Taken from http://www.superstarcoders.com/blogs/posts/md4-hash-algorithm-in-c-sharp.aspx
        // Probably not the best implementation of MD4, but it works.
        internal class MD4 : HashAlgorithm
        {
            uint a, b, c, d;
            readonly uint[] x = new uint[16];
            int bytesProcessed;

            public MD4()
            {
                Initialize();
            }

            public override void Initialize()
            {
                a = 0x67452301;
                b = 0xefcdab89;
                c = 0x98badcfe;
                d = 0x10325476;

                bytesProcessed = 0;
            }

            protected override void HashCore(byte[] array, int offset, int length)
            {
                ProcessMessage(Bytes(array, offset, length));
            }

            protected override byte[] HashFinal()
            {
                try {
                    ProcessMessage(Padding());
                    return new[] { a, b, c, d }.SelectMany(word => Bytes(word)).ToArray();
                }
                finally {
                    Initialize();
                }
            }

            void ProcessMessage(IEnumerable<byte> bytes)
            {
                foreach(byte i in bytes) {
                    int j = bytesProcessed & 63;
                    int xi = j >> 2;
                    int s = (j & 3) << 3;

                    x[xi] = (x[xi] & ~((uint)255 << s)) | ((uint)i << s);

                    if(j == 63) {
                        Process16WordBlock();
                    }

                    bytesProcessed++;
                }
            }

            static IEnumerable<byte> Bytes(byte[] bytes, int offset, int length)
            {
                for(int i = offset; i < length; i++) {
                    yield return bytes[i];
                }
            }

            IEnumerable<byte> Bytes(uint word)
            {
                yield return (byte)(word & 255);
                yield return (byte)((word >> 8) & 255);
                yield return (byte)((word >> 16) & 255);
                yield return (byte)((word >> 24) & 255);
            }

            IEnumerable<byte> Repeat(byte value, int count)
            {
                for(int i = 0; i < count; i++) {
                    yield return value;
                }
            }

            IEnumerable<byte> Padding()
            {
                return Repeat(128, 1)
                    .Concat(Repeat(0, ((bytesProcessed + 8) & 0x7fffffc0) + 55 - bytesProcessed))
                    .Concat(Bytes((uint)bytesProcessed << 3))
                    .Concat(Repeat(0, 4));
            }

            void Process16WordBlock()
            {
                uint aa = a;
                uint bb = b;
                uint cc = c;
                uint dd = d;

                foreach(int k in new[] { 0, 4, 8, 12 }) {
                    aa = Round1Operation(aa, bb, cc, dd, x[k], 3);
                    dd = Round1Operation(dd, aa, bb, cc, x[k + 1], 7);
                    cc = Round1Operation(cc, dd, aa, bb, x[k + 2], 11);
                    bb = Round1Operation(bb, cc, dd, aa, x[k + 3], 19);
                }

                foreach(int k in new[] { 0, 1, 2, 3 }) {
                    aa = Round2Operation(aa, bb, cc, dd, x[k], 3);
                    dd = Round2Operation(dd, aa, bb, cc, x[k + 4], 5);
                    cc = Round2Operation(cc, dd, aa, bb, x[k + 8], 9);
                    bb = Round2Operation(bb, cc, dd, aa, x[k + 12], 13);
                }

                foreach(int k in new[] { 0, 2, 1, 3 }) {
                    aa = Round3Operation(aa, bb, cc, dd, x[k], 3);
                    dd = Round3Operation(dd, aa, bb, cc, x[k + 8], 9);
                    cc = Round3Operation(cc, dd, aa, bb, x[k + 4], 11);
                    bb = Round3Operation(bb, cc, dd, aa, x[k + 12], 15);
                }

                unchecked {
                    a += aa;
                    b += bb;
                    c += cc;
                    d += dd;
                }
            }

            static uint ROL(uint value, int numberOfBits)
            {
                return (value << numberOfBits) | (value >> (32 - numberOfBits));
            }

            static uint Round1Operation(uint a, uint b, uint c, uint d, uint xk, int s)
            {
                unchecked {
                    return ROL(a + ((b & c) | (~b & d)) + xk, s);
                }
            }

            static uint Round2Operation(uint a, uint b, uint c, uint d, uint xk, int s)
            {
                unchecked {
                    return ROL(a + ((b & c) | (b & d) | (c & d)) + xk + 0x5a827999, s);
                }
            }

            static uint Round3Operation(uint a, uint b, uint c, uint d, uint xk, int s)
            {
                unchecked {
                    return ROL(a + (b ^ c ^ d) + xk + 0x6ed9eba1, s);
                }
            }
        }

        internal static class FileIDUtil
        {
            public static int Compute(Type t)
            {
                string toBeHashed = "s\0\0\0" + t.Namespace + t.Name;

                using HashAlgorithm hash = new MD4();
                byte[] hashed = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(toBeHashed));

                int result = 0;

                for(int i = 3; i >= 0; --i) {
                    result <<= 8;
                    result |= hashed[i];
                }

                return result;
            }

            public static int Compute(string toBeHashed)
            {
                using HashAlgorithm hash = new MD4();
                byte[] hashed = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(toBeHashed));

                int result = 0;

                for(int i = 3; i >= 0; --i) {
                    result <<= 8;
                    result |= hashed[i];
                }

                return result;
            }
        }
        #endregion
    }
}
