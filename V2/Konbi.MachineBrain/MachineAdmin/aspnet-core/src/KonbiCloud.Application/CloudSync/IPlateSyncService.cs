using KonbiCloud.Dto;
using KonbiCloud.Plate.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IPlateSyncService
    {
        Task<EntitySyncInputDto<PlateSyncDto>> Sync(Guid machineId);
        Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input);
    }
}
