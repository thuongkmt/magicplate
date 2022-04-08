using Abp.Dependency;
using Konbini.Messages.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.RFIDTable
{
    public interface IBarcodeScannerMessageHandler: ISingletonDependency
    {
        Task<CommandState> GetServiceInfosAsync();
        event EventHandler<CommandEventArgs> DeviceFeedBack;
    }
}
