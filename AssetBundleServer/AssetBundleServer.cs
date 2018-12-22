//-------------------------------------------------------------------------
// How to build AssetBundleServer.
//
// in CustomUnity directory:
// [Windows]
// "C:\Program Files\Unity\Editor\Data\MonoBleedingEdge\bin\mcs.bat" AssetBundleServer\AssetBundleServer.cs -out:Assets\CustomUnity\Editor\AssetBundleServer.exe
// [MacOSX]
// /Application/Unity/Content/MonoBleedingEdge/bin/mcs AssetBundleServer/AssetBundleServer.cs -out:Assets/CustomUnity/Editor/AssetBundleServer.exe
//-------------------------------------------------------------------------
using System;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace AssetBundleServer
{
    class MainClass
    {
        public static void WatchDog(object processID)
        {
            Console.WriteLine($"Watching parent processID: {processID}!");
            Process masterProcess = Process.GetProcessById((int)processID);
            while(masterProcess == null || !masterProcess.HasExited) {
                Thread.Sleep(1000);
            }

            Console.WriteLine("Exiting because parent process has exited!");
            Environment.Exit(0);
        }

        public static void Main(string[] args)
        {
            int parentProcessID;
            string basePath = "";

            // For testing purposes...
            if(args.Length == 0) {
                Console.WriteLine("No commandline arguments, harcoded debug mode...");
                parentProcessID = 0;
            }
            else {
                basePath = args[0];
            }

            if(args.Length >= 2) {
                parentProcessID = int.Parse(args[1]);
            }
            else {
                parentProcessID = 0;
            }
            // Automatically quit bundle server when Unity exits
            if(parentProcessID != 0) {
                Thread thread = new Thread(WatchDog);
                thread.Start(parentProcessID);
            }

            bool detailedLogging = false;
            int port = 7888;

            Console.WriteLine("Starting up asset bundle server.");
            Console.WriteLine($"Port: {port}");
            Console.WriteLine($"Directory: {basePath}");

            var listener = new HttpListener();

            /*
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName ());
            foreach (IPAddress ip in host.AddressList)
            {
                //if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    Console.WriteLine(ip.AddressFamily.ToString() + " - " + ip.ToString());
                }
            }
            */
            
            listener.Prefixes.Add($"http://*:{port}/");
            listener.Start();

            while(true) {
                Console.WriteLine("Waiting for request...");

                HttpListenerContext context = listener.GetContext();

                WriteFile(context, basePath, detailedLogging);
            }
        }

        static void WriteFile(HttpListenerContext ctx, string basePath, bool detailedLogging)
        {
            var request = ctx.Request;
            string rawUrl = request.RawUrl;
            string path = basePath + rawUrl;

            if(detailedLogging) {
                Console.WriteLine($"Requesting file: '{path}'. Relative url: {request.RawUrl} Full url: '{request.Url} AssetBundleDirectory: '{basePath}''");
            }
            else {
                Console.Write($"Requesting file: '{request.RawUrl}' ... ");
            }

            var response = ctx.Response;
            try {
                if(request.HttpMethod != "GET" && request.HttpMethod != "HEAD") throw new ArgumentException("unsupported method");

                using(var fs = File.OpenRead(path)) {
                    var filename = Path.GetFileName(path);
                    //response is HttpListenerContext.Response...
                    response.ContentLength64 = fs.Length;
                    response.SendChunked = false;
                    response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
                    if(request.HttpMethod == "GET") {
                        response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                        var buffer = new byte[64 * 1024];
                        int read;
                        using(BinaryWriter bw = new BinaryWriter(response.OutputStream)) {
                            while((read = fs.Read(buffer, 0, buffer.Length)) > 0) {
                                bw.Write(buffer, 0, read);
                                bw.Flush(); //seems to have no effect
                            }

                            bw.Close();
                        }

                        Console.WriteLine("completed.");
                        //response.StatusCode = (int)HttpStatusCode.OK;
                        //response.StatusDescription = "OK";
                    }
                    response.OutputStream.Close();
                    response.Close();
                }
            }
            catch(System.Exception exc) {
                Console.WriteLine(" failed.");
                Console.WriteLine($"Requested file failed: '{path}'. Relative url: {request.RawUrl} Full url: '{request.Url} AssetBundleDirectory: '{basePath}''");
                Console.WriteLine($"Exception {exc.GetType()}: {exc.Message}'");
                response.Abort();
            }
        }
    }
}
