using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;

namespace Konbi.WatchDog
{
    public class WatchDogService
    {
        private Form mainForm;
        public ILogger logger;
        private ConfigurationInfo configurationInfo;
        private Timer appMonitoring;

        public WatchDogService(Form _mainForm)
        {
            this.mainForm = _mainForm;
        }

        /// <summary>
        /// Send message to Slack.
        /// </summary>
        /// <param name="message"></param>
        private void SendSlackMessage(SlackMessage message)
        {            
            var slackClient = new SlackClient(logger);
            message.Channel = configurationInfo.SlackChannel.Channel;
            slackClient.SendSlackMessage(message, logger);
        }

        public void OnStart()
        {
            logger = new LoggerConfiguration()
                    .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log-watch-dog-{Date}.log", shared: true)
                    .CreateLogger();
            logger.Information("[Machine {0}]  Start konbi-watch-dog");

            try
            {
                configurationInfo = GetConfigurationInfo();
                appMonitoring = new Timer(30 * 1000); //monitor every 1 minutes
                appMonitoring.Elapsed += AppMonitoring_Elapsed;
                appMonitoring.Start();
            }
            catch (Exception e)
            {
                logger.Fatal(e, "");
            }
        }

        private ConfigurationInfo GetConfigurationInfo()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var currentPath = Path.GetDirectoryName(path);
            using (StreamReader r = new StreamReader($"{currentPath}\\monitoring.json"))
            {
                string json = r.ReadToEnd();
                var configIinfo = JsonConvert.DeserializeObject<ConfigurationInfo>(json);

                return configIinfo;
            }
        }

        private void AppMonitoring_Elapsed(object sender, ElapsedEventArgs e)
        {
            MonitorWindowsServices();
            MonitorWindowsApplication();
        }

        private void MonitorWindowsServices()
        {
            var services = ServiceController.GetServices();

            foreach (var service in configurationInfo.RfidServiceName)
            {
                var myService = services.Where(s => s.ServiceName == service.ServiceName).FirstOrDefault();
                if (myService != null)
                {
                    if (myService.Status != ServiceControllerStatus.Running)
                    {
                        logger.Information(string.Format("[Machine {0}] Service: {1} is not running. WatchDog will start", configurationInfo.SlackChannel.MachineName, service.ServiceName));
                        StartService(service);

                        // Send message to Slack.
                        SlackMessage message = new SlackMessage();
                        message.Text = string.Format("[Machine {0}] Service: {1} is not running. WatchDog will start", configurationInfo.SlackChannel.MachineName, service.ServiceName);
                        SendSlackMessage(message);          
                    }
                }
                else
                {
                    logger.Information(string.Format("[Machine {0}]  Service {1} not found", configurationInfo.SlackChannel.MachineName, service.ServiceName));
                }
            }
        }

        private void MonitorWindowsApplication()
        {
            foreach (var app in configurationInfo.RfidWindowsApp)
            {
                if (!File.Exists(app.FullPath))
                {
                    logger.Information(string.Format("[Machine {0}]  Application {1} not found", configurationInfo.SlackChannel.MachineName, app.FullPath));
                    continue;
                }

                string FilePath = Path.GetDirectoryName(app.FullPath);
                string FileName = Path.GetFileNameWithoutExtension(app.FullPath);
   
                Process[] pList = Process.GetProcessesByName(FileName);
                if(pList.Length == 0)
                {
                    logger.Information(string.Format("[Machine {0}]  Windows App: {1} is not running. WatchDog will start", configurationInfo.SlackChannel.MachineName, FileName));
                    Process yourProcess = new Process();
                    yourProcess.StartInfo.FileName = app.FullPath;
                    yourProcess.Start();

                    // Send message to Slack.
                    SlackMessage message = new SlackMessage();
                    message.Text = string.Format("[Machine {0}]  Windows App: {1} is not running. WatchDog will start", configurationInfo.SlackChannel.MachineName, FileName);
                    SendSlackMessage(message);
                }
            }
        }

        private void StartService(RFIDService app)
        {
            try
            {
                //1. start cover app
                mainForm.Invoke(new Action(() =>
                {
                    mainForm.WindowState = FormWindowState.Maximized;
                }));
                //2. start application

                var process = new Process();
                var startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.UseShellExecute = false;
                startInfo.Verb = "runas";
                startInfo.Arguments = "/C net start " + app.ServiceName;
                process.StartInfo = startInfo;
                process.Start();

                Thread.Sleep(2000);
                //3. wait until ok
                mainForm.Invoke(new Action(() =>
                {
                    mainForm.WindowState = FormWindowState.Minimized;
                }));

            }
            catch (Exception e)
            {
                logger.Fatal(e, "");
            }

        }

        public void OnStop()
        {
            appMonitoring?.Stop();
            //systemRestartTimer?.Stop();
            //eventLogMonitoringTimer.Stop();
        }

    }
}
