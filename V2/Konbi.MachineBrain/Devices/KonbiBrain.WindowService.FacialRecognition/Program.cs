using rsid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition
{
    class Program
    {
        private static RealSenseID realsenseID;
        static void Main(string[] args)
        {

            var service = new FacialRecognitionService();
            if (!Environment.UserInteractive)
            {
                // running as service                
                ServiceBase.Run(service);
            }
            else
            {
                service.RunAsConsole(args);
            }
        }
        private static void Start(string[] args)
        {
        }
        private static void Stop()
        {
            // onstop code here
        }

    }
}
