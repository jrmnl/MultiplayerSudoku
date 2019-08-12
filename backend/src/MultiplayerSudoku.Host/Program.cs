using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MultiplayerSudoku.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseHttpSys(options =>
                 {
                     options.AllowSynchronousIO = true;
                     options.Authentication.Schemes = AuthenticationSchemes.None;
                     options.Authentication.AllowAnonymous = true;
                     options.MaxConnections = null;
                     options.MaxRequestBodySize = 30000000;
                     options.UrlPrefixes.Add("http://localhost:8001");
                 });
    }
}