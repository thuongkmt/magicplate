using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WS.MdbCashless
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
         
            //if (Environment.UserInteractive)
            //{
            //    var service1 = new KonbiMdbCashlessService();
            //    service1.Start().Wait();
            //    Console.ReadLine();
            //}
            //else
            //{
                // Put the body of your old Main method here.  
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new KonbiMdbCashlessService()
                };
                ServiceBase.Run(ServicesToRun);
            //}
        }
    }
}
