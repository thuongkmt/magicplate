using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.RFIDTable
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            var appbootstrapper = new AppBootstrapper();
            // If debugging then just run as console application. otherwise compile in service style.
            if (Debugger.IsAttached)
            {
                var x = appbootstrapper.GetInstance(typeof(RFIDTableService), null) as RFIDTableService;
                x.Start(args.Length > 0 ? args[0] : "");

                Console.WriteLine("Press Enter to quit");
                Console.ReadLine();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                appbootstrapper.GetInstance(typeof(RFIDTableService),null) as RFIDTableService
                };
                ServiceBase.Run(ServicesToRun);
            }
               
           
           

        }
    }
}
