﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.KonbiCredits
{
    static class Program
    {
        private const bool IsUsingConsoleApp = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new KonbiCreditsService()
            };
            ServiceBase.Run(ServicesToRun);

            Console.ReadLine();
        }
    }
}
