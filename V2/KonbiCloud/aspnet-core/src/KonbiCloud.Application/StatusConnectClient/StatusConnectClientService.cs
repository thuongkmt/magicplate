using Abp.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.StatusConnectClient
{
    public class StatusConnectClientService : KonbiCloudAppServiceBase, IStatusConnectClientService
    {
        public StatusConnectClientService()
        {

        }

        [AbpAllowAnonymous]
        public async Task<bool> GetStatusConnectClient()
        {
            return true;
        }
    }
}
