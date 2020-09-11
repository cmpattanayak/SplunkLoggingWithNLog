using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Targets;
using NLog.Web;

namespace SplunkLoggingWithNLog
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddEnvironmentVariables()
                          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                    ConfigureSplunkLogging(config.Build());
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);      //Set the LogLevel
                })
                .UseNLog()  // NLog: Setup NLog for Dependency injection with ILogger
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void ConfigureSplunkLogging(IConfiguration Configuration)
        {
            //Get nlog.config file
            var nlogConfiguration = NLog.LogManager.Configuration;

            //To ignore Splunk certificate errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            var webServiceTarget = nlogConfiguration.FindTargetByName<WebServiceTarget>("Splunk");
            webServiceTarget.Url = new Uri(Configuration["SplunkUrl"]);
            MethodCallParameter headerParameter = new MethodCallParameter
            {
                Name = Configuration["SplunkHeaderName"],
                Layout = Configuration["SplunkHeaderLayout"]
            };

            webServiceTarget.Headers.Add(headerParameter);

            if (!String.IsNullOrWhiteSpace(AppEnvironment) && AppEnvironment.Equals("local"))
            {
                var logconsole = new ConsoleTarget("logconsole");
                nlogConfiguration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            }

            NLog.LogManager.Configuration = nlogConfiguration;

            //Set Global Diagnostics context
            GlobalDiagnosticsContext.Set("application", Configuration["Application"]);
            GlobalDiagnosticsContext.Set("environment", AppEnvironment);
        }

        private static string AppEnvironment
        {
            get
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
        }
    }
}
