using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.WatchDog
{
    public class ConfigurationInfo
    {
        public SlackChannel SlackChannel { get; set; }
        public List<RFIDService> RfidServiceName { get; set; }
        public List<RFIDWindowsApp> RfidWindowsApp { get; set; }
        public List<EventLogTracker> EventLogTrackers { get; set; }
    }

    public class SlackChannel
    {
        public string MachineName { get; set; }
        public string Token { get; set; }
        public string Channel { get; set; }
        public string PostUrl { get; set; }
    }

    public class RFIDService
    {
        public string ServiceName { get; set; }
    }

    public class RFIDWindowsApp
    {
        public string FullPath { get; set; }
    }

    public class EventLogTracker
    {
        public string LogName { get; set; }
        public List<long> TrackEventIDs { get; set; }
    }
}
