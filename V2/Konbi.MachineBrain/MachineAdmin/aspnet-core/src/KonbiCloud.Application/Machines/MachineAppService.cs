using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Machines
{
    public class MachineAppService : KonbiCloudAppServiceBase
    {
        private readonly IMachineSyncService _machineSyncService;
        private readonly IDetailLogService detailLogService;

        public MachineAppService(IMachineSyncService machineSyncService, IDetailLogService detailLog)
        {
            _machineSyncService = machineSyncService;
        }

        [AbpAuthorize(AppPermissions.Pages_SystemSetting)]
        public async Task<Machine.Machine> GetMachineFromServer(string Id)
        {
            try
            {
                var machine = await _machineSyncService.GetMachineFromServer(Id);
                if (machine == null)
                {
                    return null;
                }

                return new Machine.Machine()
                {
                    id = machine.id,
                    name = machine.name,
                    TenantId = machine.tenantId
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"Get machine from server: {ex.Message}", ex);
                throw new UserFriendlyException("Get machine from server failed");
            }
        }

        [AbpAuthorize(AppPermissions.Pages_SystemSetting)]
        public async Task<bool> UpdateMachineToServer(Machine.Machine input)
        {
            var machine = await _machineSyncService.UpdateMachineToServer(input);
            return machine;
        }
    }
}
