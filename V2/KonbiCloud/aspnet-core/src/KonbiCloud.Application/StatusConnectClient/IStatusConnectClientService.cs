using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.StatusConnectClient
{
    public interface IStatusConnectClientService : IApplicationService
    {
        Task<bool> GetStatusConnectClient();
    }
}
