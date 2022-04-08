using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.RFIDTable.Interfaces
{
    public interface ILogService
    {
        void LogException(Exception ex);
        void LogException(string message);
        void LogWindowEvent(string message, EventLogEntryType eventType);
        void SendToSlackAlert(string message);
    }
}
