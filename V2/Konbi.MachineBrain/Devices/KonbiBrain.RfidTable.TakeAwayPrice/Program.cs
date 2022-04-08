using System;
using System.Collections.Generic;
using System.Windows.Forms;
using KonbiBrain.RfidTable.TakeAwayPrice;
using Serilog;

namespace WindowsApplication2
{
    static class Program
    {
        /// <summary>
        /// ??????????
        /// </summary>
        [STAThread]
        static void Main()
        {
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("Logs\\log.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();



            Log.Information("Init");
            Application.Run(new FormWritePriceVer2());
        }
    }
}