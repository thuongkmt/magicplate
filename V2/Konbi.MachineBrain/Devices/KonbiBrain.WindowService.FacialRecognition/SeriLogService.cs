using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowService.FacialRecognition
{
    public static class SeriLogService
    {
        private static ILogger InfoLogger = null;
        static string desktopPath = @"C:\Logs\Realsenel\";
        public static void CreateLoggers()
        {

            //DateTime.Now.Date.ToShortDateString().Replace('/', '_')

            InfoLogger = Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(desktopPath + "Info-.txt",rollingInterval: RollingInterval.Day)
            .CreateLogger();

        }

        public static void LogInfo(string s)
        {
            if(InfoLogger == null)
            {
                CreateLoggers();
            }
            //InfoLogger.Information(desktopPath);

            InfoLogger.Information(s);
        }
    }
}
