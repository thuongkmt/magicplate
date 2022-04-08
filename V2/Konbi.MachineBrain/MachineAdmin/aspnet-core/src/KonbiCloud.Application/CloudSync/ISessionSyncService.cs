using KonbiCloud.Dto;
using KonbiCloud.Machines.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface ISessionSyncService
    {
        Task<EntitySyncInputDto<SessionSyncDto>> Sync(Guid machineId);
    }
}
