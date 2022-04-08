using KonbiBrain.WindowServices.CotfPad.Hardware;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.CotfPad
{
    static class Program
    {
        static ConsoleEventDelegate handler;
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                try
                {
                    if (TagMappingRunning)
                    {
                        var path = ConfigurationManager.AppSettings["TagMapping"].ToString();
                        Process.Start(path);
                    }
                    if (TARunning)
                    {
                        var path = ConfigurationManager.AppSettings["TakeAwayPath"].ToString();
                        Process.Start(path);
                    }
                }
                catch (System.Exception ex)
                {

                }
            }
            return false;
        }

        public static bool TagMappingRunning { get; set; }
        public static bool TARunning { get; set; }

        static void Main()
        {
            try
            {
                var tagMappings = Process.GetProcessesByName("Konbini.RfidTable.ProductTagMapping");
                foreach (Process worker in tagMappings)
                {
                    TagMappingRunning = true;
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                }

                var tas = Process.GetProcessesByName("KonbiBrain.RfidTable.TakeAwayPrice");
                foreach (Process worker in tas)
                {
                    TARunning = true;
                    worker.Kill();
                    worker.WaitForExit();
                    worker.Dispose();
                }
            }
            catch (System.Exception ex)
            {

            }

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            // Set desktop directory
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Logs\";
            // Init logger function
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(desktopPath + "log-ctofpad-.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                .CreateLogger();

            Log.Information("Init");


            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new CotfPadService()
            //};
            //ServiceBase.Run(ServicesToRun);

            var cotfPad = new CotfPadService();
            Task.Factory.StartNew(() =>
            {
                cotfPad.Start();
            });


            Console.WriteLine("Press Enter to quit...");
            Console.ReadLine();
            Console.WriteLine("Application exiting...");

            cotfPad?.Stop();
        }
    }
}
