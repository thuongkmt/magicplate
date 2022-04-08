using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.StatusConnectServer
{
    public interface IStatusConnectServerService : IApplicationService
    {
        Task<bool> GetStatusConnectServer();
    }
}
