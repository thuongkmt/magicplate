using KonbiBrain.WindowServices.RFIDTable.Interfaces;
using Slack.Webhooks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.RFIDTable.Services
{
    public class LogService : ILogService
    {
        private EventLog eventLog;

        private const string slackUrl = "https://hooks.slack.com/services/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";
        
        private const string slackChannel = "CEQ10HK8R";
        private Slack.Webhooks.SlackClient SlackLogger { get; set; }
        public LogService()
        {
            // initializing Window event log
            string eventSourceName = "RFIDTableService";
            string logName = "RFIDTableService";

            



            eventLog = new System.Diagnostics.EventLog();

            if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
            }

            eventLog.Source = eventSourceName;
           // eventLog.Log = logName;


            // config slack to map with #rftable channel
            SlackLogger = new Slack.Webhooks.SlackClient(slackUrl);
        }
        public void LogException(Exception ex)
        {
            LogException(ex.Message + Environment.NewLine + ex.StackTrace.ToString());
        }
        public void LogException(string message)
        {
            LogWindowEvent(message, EventLogEntryType.Error);
        }

        public void LogWindowEvent(string message, EventLogEntryType eventType)
        {
            eventLog.WriteEntry(message, eventType);
        }

        public void SendToSlackAlert(string message)
        {
            try
            {
                var slackMessage = new SlackMessage
                {
                    Channel = slackChannel,
                    Text = string.Format("[{0}] : {1}", Environment.MachineName, message),
                    IconEmoji = Emoji.AlarmClock,
                    Username = "Rftable-Alert",
                    Mrkdwn = true
                };

                SlackLogger.Post(slackMessage);
            }
            catch (Exception e)
            {
            }
        }
    }
}
