using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
    using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DownLoad.Server
{
    public class Startup
    {
        private static readonly Random _random = new Random(123456);

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", WriteLargeData);
            });
        }


        private static Lazy<byte[]> DataLazy = new Lazy<byte[]>(() =>
        {
            byte[] data = new byte[1024 * 64];
            const char First = '!';
            const char Last = '~';
            int c = First;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)c;
                c++;
                if (c > Last) c = First;
            }
            return data;
        });

        private static async Task WriteLargeData(HttpContext ctx)
        {
            byte[] data = DataLazy.Value;
            ctx.Response.ContentType = "text/plain; charset=us-ascii";
            
            string? lengthStr = ctx.Request.Query["length"].FirstOrDefault();
            if (!int.TryParse(lengthStr, out int length))
            {
                lengthStr = ctx.Request.Query["lengthMb"].FirstOrDefault();
                if (!double.TryParse(lengthStr, out double lengthMb)) lengthMb = 0.1;

                length = (int)(1024 * 1024 * lengthMb);
            }

            ctx.Response.ContentLength = length;
            ctx.Response.Headers.Add("Content-Disposition", $"attachment; filename = \"_DL_{length}_{Guid.NewGuid()}.txt\"");
            ctx.Response.Headers.Add("Protocol-FYI", ctx.Request.Protocol);

            for (int remaining = length; remaining > 0;)
            {
                int toWrite = Math.Min(remaining, data.Length);
                await ctx.Response.Body.WriteAsync(data.AsMemory(0, toWrite));
                remaining -= toWrite;
            }
                      
            await ctx.Response.Body.FlushAsync();
        }
    }
}
