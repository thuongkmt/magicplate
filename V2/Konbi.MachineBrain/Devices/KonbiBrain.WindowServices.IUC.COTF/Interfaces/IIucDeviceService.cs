using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC.COTF.Interfaces
{
    public interface IIucDeviceService
    {
        bool ConnectPort(string port, Action<string> action = null);
        Dictionary<string, string> TerminalInfo();
        void LogIucApi(string message);
        bool EnablePayment(int cents);
    }
}
