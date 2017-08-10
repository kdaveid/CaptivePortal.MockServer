// Copyright (c) David E. Keller. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Dkbe.CaptivePortal.MockServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Mock Server";

            var config = new ConfigurationBuilder()
               .AddCommandLine(args)
               .Build();

            var urls = (config[WebHostDefaults.ServerUrlsKey] ?? "http://0.0.0.0:5001")
                .Split(new[] { ',', ';' })
                .Select(url => url.Trim())
                .ToArray();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls(urls)
                .Build();

            host.Run();
        }
    }
}
