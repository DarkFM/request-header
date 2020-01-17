using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RequestHeaderParser
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    using var fs = new FileStream(Directory.GetCurrentDirectory() + "/Views/index.html", FileMode.Open);
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await fs.CopyToAsync(context.Response.Body);
                });

                endpoints.MapGet("/api/whoami", async context =>
                {
                    dynamic responseObj = new System.Dynamic.ExpandoObject();
                    responseObj.ipaddress = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    var headers = context.Request.Headers;
                    if (headers.TryGetValue("Accept-Language", out var langVal))
                    {
                        responseObj.language = langVal.ToString();
                        logger.LogInformation(langVal);
                    }
                    if (headers.TryGetValue("User-Agent", out var uaVal))
                    {
                        responseObj.software = uaVal.ToString();
                        logger.LogInformation(uaVal);
                    }
                    string objStr = JsonSerializer.Serialize(responseObj);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(objStr);
                });
            });
        }
    }
}
