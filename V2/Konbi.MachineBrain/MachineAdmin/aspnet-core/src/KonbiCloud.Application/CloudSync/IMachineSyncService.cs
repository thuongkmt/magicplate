using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KonbiCloud.Machine;

namespace KonbiCloud.CloudSync
{
    public interface IMachineSyncService
    {
        Task<MachineSync> GetMachineFromServer(string Id);
        Task<bool> UpdateMachineToServer(Machine.Machine machine);
    }
}
