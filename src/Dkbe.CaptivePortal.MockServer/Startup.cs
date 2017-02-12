using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Dkbe.CaptivePortal.MockServer.Models;
using Dkbe.CaptivePortal.MockServer.Services;

namespace Dkbe.CaptivePortal.MockServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStateProvider(s => s.ZoneNames = new string[] { "lerngarage" });
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddMvc().AddXmlSerializerFormatters();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.Use(next =>
            {
                return ctx =>
                {
                    ctx.Response.Headers.Remove("Server");
                    ctx.Response.Headers.Add("Server", "SonicWALL");
                    //ctx.Response.Headers.Add("User-Agent", "SonicOS HTTP Client 1.0");
                    return next(ctx);
                };
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
