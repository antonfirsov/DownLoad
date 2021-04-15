using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DownLoad.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    bool http2 = args.Any(a => a.ToLower().Contains("http2"));
                    webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel(options =>
                        {
                            if (http2)
                            {
                                options.ListenLocalhost(5001);
                            }
                            else
                            {
                                options.ListenLocalhost(5000);
                            }
                        })
                        .ConfigureKestrel(options =>
                        {
                            if (http2)
                            {
                                options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http2);
                                Console.WriteLine("Configured for HTTP2!");
                            }
                        });
                });
    }
}
