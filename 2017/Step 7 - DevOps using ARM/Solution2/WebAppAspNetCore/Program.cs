using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebAppAspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureSecrets)
                .UseApplicationInsights()                
                .UseStartup<Startup>()
                .Build();

        static void ConfigureSecrets(WebHostBuilderContext webHostBuilderContext, IConfigurationBuilder configurationBuilder)
        {
            if (webHostBuilderContext.HostingEnvironment.IsDevelopment())
            {
                configurationBuilder.AddUserSecrets<Startup>();
            }
        }
    }
}
