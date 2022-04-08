using IucBrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC.Interfaces
{
    public interface IIucDeviceService
    {
        IucPaymentMode Mode { get; set; }
        bool ConnectPort(string port, Action<string> action = null);
        Dictionary<string, string> TerminalInfo();       
        void LogIucApi(string message);
        bool EnablePayment(int cents);
    }
}
