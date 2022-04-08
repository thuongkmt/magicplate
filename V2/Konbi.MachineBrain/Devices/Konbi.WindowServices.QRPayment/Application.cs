using Konbi.WindowServices.QRPayment.Services.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Konbi.WindowServices.QRPayment.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Konbi.WindowServices.QRPayment.Util;

namespace Konbi.WindowServices.QRPayment
{
    public class Application : IApplication, IDisposable
    {

        #region Services
        public static IConfigurationRoot configuration;
        private FomoService FomoService;
        private LogService LogService;
        #endregion

        public Application(LogService logService, FomoService fomoService)
        {
            LogService = logService;
            FomoService = fomoService;
        }
        public void Run(string[] args)
        {

            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var pathToContentRoot = Directory.GetCurrentDirectory();
            var webHostArgs = args.Where(arg => arg != "--console").ToArray();
            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }

            EnvironmentVariables.CurrentPath = pathToContentRoot;

            // Load config file
            var builder = new ConfigurationBuilder()
              .SetBasePath(pathToContentRoot)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            LogService.LogInfo("Path: " + pathToContentRoot);
            configuration = builder.Build();

            LogService.Init(pathToContentRoot);
            LogService.LogInfo("Starting");


            FomoService.Init(configuration);


            Task.Run(() =>
            {
                CreateWebHostBuilder(pathToContentRoot, webHostArgs, isService);
            });

            while (true)
            {
                if (!isService)
                {
                    var text = Console.ReadLine();
                    if (text == "1")
                    {
                        FomoService.GenerateQRAsync(Enums.ConditionCodes.SingtelDashQr, 1, new Random().Next().ToString(), "HAHAHA").Wait();
                    }

                    if (text == "2")
                    {
                        FomoService.CancelRequestAsync().Wait();
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                }
               
            }
        }

        public void CreateWebHostBuilder(string pathToContentRoot, string[] webHostArgs, bool isService)
        {
            var host = WebHost.CreateDefaultBuilder(webHostArgs)
                .UseContentRoot(pathToContentRoot)
                .UseStartup<Startup>()
                .UseUrls($"http://0.0.0.0:{configuration["App:ApiPort"]}/")
                .Build();

            if (isService)
            {
                host.RunAsService();
            }
            else
            {
                host.Run();
            }
        }
        public void Dispose()
        {
        }
    }

}
