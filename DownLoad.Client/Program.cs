using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DownLoad.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string hostName = args.Length > 0 ? args[0] : "10.194.114.94";

            if (args.Length <= 1 || !int.TryParse(args[1], out int lengthMb))
            {
                lengthMb = 5;
            }

            int sep = hostName.IndexOf(':');
            int port = -1;
            if (sep >= 0){
                string portStr = hostName.Substring(sep + 1, hostName.Length - sep - 1);
                hostName = hostName.Substring(0, sep);
                port = int.Parse(portStr);
            }

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
            //    await TestHandler("WinHttpHandler HTTP 1.1", new WinHttpHandler(), hostName, false, lengthMb);
                
            //    // H2 fails with ERROR_WINHTTP_INVALID_SERVER_RESPONSE
            //    //await TestHandler("WinHttpHandler HTTP 2.0", new WinHttpHandler(), hostName, true, lengthMb);
            //}

            //await TestHandler("SocketsHttpHandler HTTP 1.1", new SocketsHttpHandler(), hostName, false, lengthMb);

            await TestHandler("SocketsHttpHandler HTTP 2.0", new SocketsHttpHandler(), hostName, true, lengthMb, port);   
        }

        static async Task TestHandler(string info, HttpMessageHandler handler, string hostName, bool http2, int lengthMb, int port = -1)
        {
            using var client = new HttpClient(handler, true);
            var message = GenerateRequestMessage(hostName, http2, lengthMb, ref port);
            Console.WriteLine($"{info} / {lengthMb} MB from {hostName}:{port}");
            Stopwatch sw = Stopwatch.StartNew();
            var response = await client.SendAsync(message);
            long elapsedMs = sw.ElapsedMilliseconds;

            Console.WriteLine($"{info}: {response.StatusCode} in {elapsedMs} ms");
        }
            
        static HttpRequestMessage GenerateRequestMessage(string hostName, bool http2, int lengthMb, ref int port)
        {
            if (port < 0)
            {
                port = http2 ? 5001 : 5000;                
            }

            string url = $"http://{hostName}:{port}?lengthMb={lengthMb}";
            var msg = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Version = new Version(1, 1)
            };

            if (http2)
            {
                msg.Version = new Version(2, 0);
                msg.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
            }

            return msg;
        }
    }
}