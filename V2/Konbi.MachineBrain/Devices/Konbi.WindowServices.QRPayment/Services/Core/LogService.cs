
using Serilog;
using System;

namespace Konbi.WindowServices.QRPayment.Services.Core
{
    public class LogService
    {
        private ILogger InfoLogger;
        private ILogger FomoLogger;

        public void Init(string path)
        {

            InfoLogger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.Console()
               .WriteTo.File(path + "\\logs\\log-.txt", rollingInterval: RollingInterval.Day)
               .CreateLogger();
            FomoLogger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              //.WriteTo.Console()
              .WriteTo.File(path + "\\logs\\fomo-.txt", rollingInterval: RollingInterval.Day)
              .CreateLogger();
        }

        public void LogInfo(string message)
        {
            InfoLogger?.Information(message);
        }
    
        public void LogError(string message)
        {
            InfoLogger?.Error(message);
        }
       
        public void LogError(Exception ex)
        {
            InfoLogger?.Error(ex.ToString());
        }

        public void LogFomo(string s)
        {
            FomoLogger?.Information(s);
        }
    }
}
