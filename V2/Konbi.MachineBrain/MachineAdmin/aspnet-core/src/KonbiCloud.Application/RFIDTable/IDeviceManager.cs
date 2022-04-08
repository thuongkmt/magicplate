using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface IDeviceManager
    {
        Task<bool> PingAsync(string args="");
    }
}
