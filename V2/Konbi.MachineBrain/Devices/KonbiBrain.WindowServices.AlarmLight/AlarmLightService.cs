using Konbi.Common.Interfaces;
using KonbiBrain.Common.Services;
using KonbiBrain.WindowServices.AlarmLight.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace KonbiBrain.WindowServices.AlarmLight
{
    public partial class AlarmLightService : ServiceBase
    {

        public SerialPortHandler SerialPortHandler { get; set; }
        public IKonbiBrainLogService logger { get; set; }
        // public SignalRContext SignalContext { get; set; }

        public AlarmLightService(IKonbiBrainLogService logger)
        {
            InitializeComponent();
            this.logger = new LogService();
        }

        protected override void OnStart(string[] args)
        {
            logger.LogRfIdTableInfo("Alarm Light Service Starting ...");
            //System.Diagnostics.Debugger.Launch();
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                try
                {
                    logger.LogRfIdTableInfo($"{args[0]} is passed through. Storing the new port to config file and start Listening on the port ...");
                    var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var settings = configFile.AppSettings.Settings;

                    if (settings["alarmlight.comport"] == null)
                    {
                        settings.Add("alarmlight.comport", args[0]);
                    }

                    else
                    {
                        settings["alarmlight.comport"].Value = args[0];
                    }

                    configFile.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                }
                catch (Exception ex)
                {
                    logger.LogRfIdTableInfo("Exeption Message: " + ex.Message);
                    logger.LogRfIdTableInfo("Exeption StackTrace: " + ex.StackTrace);

                }
            }
            //var serialConnectionId = SerialPortHandler.Open(ConfigurationManager.AppSettings["table.comport"]);
            //if (serialConnectionId > 0)
            //{
            //    logger.LogRfIdTableInfo($"Connected to the device via {ConfigurationManager.AppSettings["table.comport"]}");
            //}
            //else
            //{
            //    logger.LogRfIdTableInfo($"Couldn't connect to the device via {ConfigurationManager.AppSettings["table.comport"]}");
            //}
        }

        protected override void OnStop()
        {
            logger.LogRfIdTableInfo($"Service is shutting down. Trying to close serial connection also");
            //SerialPortHandler.Close(true);
            //SerialPortHandler = null;
            Process thisProc = Process.GetCurrentProcess();
            if (thisProc != null)
                thisProc.Kill();
        }
        public void Start(string comport)
        {
            OnStart(new string[] { comport });
        }
    }
}
