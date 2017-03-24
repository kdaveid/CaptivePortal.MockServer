// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Dkbe.CaptivePortal.MockServer.Models;
using Dkbe.CaptivePortal.MockServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddOptions();
            services.AddStateProvider();
            services.Configure<StaticZoneSettings>(Configuration.GetSection(nameof(StaticZoneSettings)));
            services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));
            services.Configure<CaptivePortalSettings>(Configuration.GetSection(nameof(CaptivePortalSettings)));
            services.AddMvc().AddXmlSerializerFormatters();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            app.Use(next =>
            {   // Simple middleware to fake SNWL headers
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
