using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC
{
    static class Program
    {
        private const bool IsUsingConsoleApp = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            if (!IsUsingConsoleApp)
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    AppBootstrapper.Current.GetInstance(typeof(IucService),null) as IucService
                };
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                var x = AppBootstrapper.Current.GetInstance(typeof(IucService), null) as IucService;
                x.Start();
            }
           

            

            //Console.WriteLine("Press Enter to quit");
            Console.ReadLine();
        }
    }
}
