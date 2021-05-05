using System;
using System.Diagnostics;
using System.Net.Http;
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

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
            //    await TestHandler("WinHttpHandler HTTP 1.1", new WinHttpHandler(), hostName, false, lengthMb);
                
            //    // H2 fails with ERROR_WINHTTP_INVALID_SERVER_RESPONSE
            //    //await TestHandler("WinHttpHandler HTTP 2.0", new WinHttpHandler(), hostName, true, lengthMb);
            //}

            //await TestHandler("SocketsHttpHandler HTTP 1.1", new SocketsHttpHandler(), hostName, false, lengthMb);
            await TestHandler("SocketsHttpHandler HTTP 2.0", new SocketsHttpHandler(), hostName, true, lengthMb);   
        }

        static async Task TestHandler(string info, HttpMessageHandler handler, string hostName, bool http2, int lengthMb)
        {
            using var client = new HttpClient(handler, true);
            var message = GenerateRequestMessage(hostName, http2, lengthMb);
            Console.WriteLine($"{info} / {lengthMb} MB from {hostName}");
            Stopwatch sw = Stopwatch.StartNew();
            var response = await client.SendAsync(message);
            long elapsedMs = sw.ElapsedMilliseconds;

            Console.WriteLine($"{info}: {response.StatusCode} in {elapsedMs} ms");
        }
            
        static HttpRequestMessage GenerateRequestMessage(string hostName, bool http2, int lengthMb = 25)
        {
            string url = $"http://{hostName}:{(http2 ? "5001" : "5000")}?lengthMb={lengthMb}";
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